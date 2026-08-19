using DotnetUserManagementApi.Infrastructure.Security;

namespace DotnetUserManagementApi.Tests;

public sealed class BcryptPasswordHasherTests
{
    private readonly BcryptPasswordHasher _sut = new();

    [Fact]
    public void Hash_ProducesNonPlaintextValue()
    {
        const string password = "senha-segura-123";

        var hash = _sut.Hash(password);

        Assert.False(string.IsNullOrWhiteSpace(hash));
        Assert.NotEqual(password, hash);
        Assert.StartsWith("$2", hash);
    }

    [Fact]
    public void Hash_IsSaltRandomized()
    {
        var first = _sut.Hash("senha-segura-123");
        var second = _sut.Hash("senha-segura-123");

        Assert.NotEqual(first, second);
    }

    [Fact]
    public void Verify_CorrectPassword_ReturnsTrue()
    {
        var hash = _sut.Hash("senha-segura-123");

        Assert.True(_sut.Verify("senha-segura-123", hash));
    }

    [Fact]
    public void Verify_WrongPassword_ReturnsFalse()
    {
        var hash = _sut.Hash("senha-segura-123");

        Assert.False(_sut.Verify("senha-errada", hash));
    }
}