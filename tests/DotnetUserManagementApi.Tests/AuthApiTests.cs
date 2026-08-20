using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using DotnetUserManagementApi.Application.Dtos;

namespace DotnetUserManagementApi.Tests;

public sealed class AuthApiTests
{
    [Fact]
    public async Task Register_ValidUser_ReturnsCreated()
    {
        await using var factory = new TestWebAppFactory();
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/auth/register", new RegisterUserRequest("Ana Souza", "ana@example.com", "senha12345"));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<RegisterResponse>();
        Assert.NotNull(result);
        Assert.False(string.IsNullOrWhiteSpace(result.Message));
    }

    [Fact]
    public async Task Register_DuplicateEmail_ReturnsCreatedUniform()
    {
        await using var factory = new TestWebAppFactory();
        using var client = factory.CreateClient();
        var request = new RegisterUserRequest("Ana Souza", "ana@example.com", "senha12345");

        var first = await client.PostAsJsonAsync("/api/auth/register", request);
        var second = await client.PostAsJsonAsync("/api/auth/register", request);

        // T-01-10 anti-enumeração: resposta uniforme (201) mesmo para e-mail já existente.
        Assert.Equal(HttpStatusCode.Created, first.StatusCode);
        Assert.Equal(HttpStatusCode.Created, second.StatusCode);
    }

    [Fact]
    public async Task Register_InvalidEmail_ReturnsBadRequest()
    {
        await using var factory = new TestWebAppFactory();
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/auth/register", new RegisterUserRequest("Ana Souza", "email-invalido", "senha12345"));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Register_WeakPassword_ReturnsBadRequest()
    {
        await using var factory = new TestWebAppFactory();
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/auth/register", new RegisterUserRequest("Ana Souza", "ana@example.com", "curta"));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Login_ValidCredentials_ReturnsToken()
    {
        await using var factory = new TestWebAppFactory();
        using var client = factory.CreateClient();
        await client.PostAsJsonAsync("/api/auth/register", new RegisterUserRequest("Ana Souza", "ana@example.com", "senha12345"));

        var response = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest("ana@example.com", "senha12345"));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var login = await response.Content.ReadFromJsonAsync<LoginResponse>();
        Assert.NotNull(login);
        Assert.False(string.IsNullOrWhiteSpace(login.Token));
        Assert.Equal("Bearer", login.TokenType);
        Assert.Equal("ana@example.com", login.User.Email);
    }

    [Fact]
    public async Task Login_WrongPassword_ReturnsUnauthorized()
    {
        await using var factory = new TestWebAppFactory();
        using var client = factory.CreateClient();
        await client.PostAsJsonAsync("/api/auth/register", new RegisterUserRequest("Ana Souza", "ana@example.com", "senha12345"));

        var response = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest("ana@example.com", "senha-errada"));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Login_RepeatedFailures_ReturnsTooManyRequests()
    {
        await using var factory = new TestWebAppFactory();
        using var client = factory.CreateClient();
        await client.PostAsJsonAsync("/api/auth/register", new RegisterUserRequest("Ana Souza", "ana@example.com", "senha12345"));

        for (var i = 0; i < 5; i++)
        {
            await client.PostAsJsonAsync("/api/auth/login", new LoginRequest("ana@example.com", "senha-errada"));
        }

        var response = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest("ana@example.com", "senha-errada"));

        Assert.Equal(HttpStatusCode.TooManyRequests, response.StatusCode);
    }

    [Fact]
    public async Task GetUsers_WithoutToken_ReturnsUnauthorized()
    {
        await using var factory = new TestWebAppFactory();
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/api/users");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetUsers_WithToken_ReturnsUserList()
    {
        await using var factory = new TestWebAppFactory();
        using var client = factory.CreateClient();
        await client.PostAsJsonAsync("/api/auth/register", new RegisterUserRequest("Ana Souza", "ana@example.com", "senha12345"));
        await client.PostAsJsonAsync("/api/auth/register", new RegisterUserRequest("Bruno Lima", "bruno@example.com", "senha12345"));

        var loginResponse = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest("ana@example.com", "senha12345"));
        var login = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();

        var request = new HttpRequestMessage(HttpMethod.Get, "/api/users");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", login!.Token);
        var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var users = await response.Content.ReadFromJsonAsync<List<UserDto>>();
        Assert.NotNull(users);
        Assert.Equal(2, users.Count);
        Assert.DoesNotContain("passwordHash", await response.Content.ReadAsStringAsync(), StringComparison.OrdinalIgnoreCase);
    }
}