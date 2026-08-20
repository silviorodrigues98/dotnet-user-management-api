using System.Security.Cryptography;
using System.Text;
using DotnetUserManagementApi.Api.Middlewares;
using DotnetUserManagementApi.Application;
using DotnetUserManagementApi.Infrastructure;
using DotnetUserManagementApi.Infrastructure.Persistence;
using DotnetUserManagementApi.Infrastructure.Security;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Npgsql;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

var jwtSection = builder.Configuration.GetSection(JwtOptions.SectionName);
var jwtOptions = jwtSection.Get<JwtOptions>() ?? new JwtOptions();

if (string.IsNullOrWhiteSpace(jwtOptions.Key))
{
    if (builder.Environment.IsDevelopment())
    {
        jwtOptions.Key = Convert.ToHexString(RandomNumberGenerator.GetBytes(64));
        builder.Configuration["Jwt:Key"] = jwtOptions.Key;
        Console.WriteLine("[SECURITY] Jwt:Key não configurado. Chave aleatória gerada para esta execução (desenvolvimento local).");
    }
    else
    {
        // D-08: fail-fast — JWT__KEY (Jwt:Key) é obrigatória fora do ambiente de desenvolvimento
        throw new InvalidOperationException("JWT__KEY (Jwt:Key) é obrigatório em produção. Defina a variável de ambiente JWT__KEY antes de iniciar a API.");
    }
}

builder.Services.Configure<JwtOptions>(jwtSection);
builder.Services.PostConfigure<JwtOptions>(options =>
{
    if (string.IsNullOrWhiteSpace(options.Key))
    {
        options.Key = jwtOptions.Key;
    }
});

builder.Services.AddControllers();

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtOptions.Issuer,
            ValidAudience = jwtOptions.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Key)),
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "dotnet-user-management-api", Version = "v1" });
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Informe o token JWT obtido no login.",
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" },
            },
            Array.Empty<string>()
        },
    });
});

var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options => options.SwaggerEndpoint("/swagger/v1/swagger.json", "dotnet-user-management-api v1"));
}

app.UseDefaultFiles();
app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapGet("/favicon.ico", () =>
    Results.File(Path.Combine(app.Environment.WebRootPath, "favicon.svg"), "image/svg+xml"));

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var databaseProvider = app.Configuration.GetConnectionString("Database");

    if (string.Equals(databaseProvider, "Postgres", StringComparison.OrdinalIgnoreCase))
    {
        // D-02/D-07: PostgreSQL (Docker) aplica migrações EF Core no startup com retry limitado
        const int maxAttempts = 10;
        for (var attempt = 1; attempt <= maxAttempts; attempt++)
        {
            try
            {
                dbContext.Database.Migrate();
                break;
            }
            catch (NpgsqlException ex)
            {
                Console.WriteLine($"[DB] PostgreSQL indisponível — tentativa {attempt}/{maxAttempts}. Aguardando 2s... ({ex.Message})");
                if (attempt == maxAttempts)
                {
                    throw;
                }

                Thread.Sleep(TimeSpan.FromSeconds(2));
            }
        }
    }
    else
    {
        // D-02: local (SQLite) usa EnsureCreated — zero migrações
        dbContext.Database.EnsureCreated();
    }
}

app.Run();

public partial class Program;