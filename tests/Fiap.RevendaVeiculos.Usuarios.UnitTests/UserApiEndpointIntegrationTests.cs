using System.Net;
using System.Net.Http.Json;
using Xunit.Sdk;

namespace Fiap.RevendaVeiculos.Usuarios.UnitTests;

[Trait("Category", "Docker")]
public sealed class UserApiEndpointIntegrationTests
{
    private static readonly Uri BaseAddress = new(Environment.GetEnvironmentVariable("USERS_API_URL") ?? "http://localhost:8181");

    [Fact]
    public async Task All_users_api_endpoints_register_login_and_health_checks_work()
    {
        using var api = new HttpClient { BaseAddress = BaseAddress };
        await EnsureApiIsRunning(api);
        Assert.Equal(HttpStatusCode.OK, (await api.GetAsync("/health/live")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await api.GetAsync("/health/ready")).StatusCode);
        var suffix = Guid.NewGuid().ToString("N")[..10];
        var register = await api.PostAsJsonAsync("/api/v1/users", new { name = "Comprador", cpf = Random.Shared.NextInt64(10_000_000_000L, 99_999_999_999L).ToString(), email = $"user{suffix}@example.com", password = "Senha@123", role = "COMPRADOR" });
        Assert.Equal(HttpStatusCode.Created, register.StatusCode);
        var login = await api.PostAsJsonAsync("/api/v1/auth/login", new { email = $"user{suffix}@example.com", password = "Senha@123" });
        Assert.Equal(HttpStatusCode.OK, login.StatusCode);
        var token = await login.Content.ReadFromJsonAsync<LoginResponse>();
        Assert.False(string.IsNullOrWhiteSpace(token?.AccessToken));
    }

    private static async Task EnsureApiIsRunning(HttpClient api)
    {
        try { if ((await api.GetAsync("/health/live")).IsSuccessStatusCode) return; } catch (HttpRequestException) { }
        throw new XunitException("API de Usuários indisponível. Inicie: docker compose -f compose.yaml -f compose.local-ports.yaml up -d --build na raiz de Vendas.");
    }
    private sealed record LoginResponse(string AccessToken);
}
