using Fiap.RevendaVeiculos.Usuarios.Application;
using Fiap.RevendaVeiculos.Usuarios.Domain;

namespace Fiap.RevendaVeiculos.Usuarios.UnitTests;

public sealed class UserServiceTests
{
    [Fact]
    public async Task Register_defaults_role_to_buyer()
    {
        var service = CreateService(out _, out _);
        var user = await service.RegisterAsync(new("Maria", "52998224725", "maria@example.com", "Senha@123"), CancellationToken.None);
        Assert.Equal("COMPRADOR", user.Role);
    }

    [Fact]
    public async Task Register_and_login_authenticates_user_with_adm_role()
    {
        var service = CreateService(out _, out var tokens);
        await service.RegisterAsync(new("Admin", "11144477735", "admin@example.com", "Senha@123", "ADM"), CancellationToken.None);
        var login = await service.LoginAsync(new("admin@example.com", "Senha@123"), CancellationToken.None);
        Assert.Equal("token-ADM", login.AccessToken);
        Assert.True(tokens.WasCalled);
    }

    private static UserService CreateService(out MemoryRepository repository, out FakeTokens tokens)
    {
        repository = new(); tokens = new();
        return new UserService(repository, new PlainHasher(), tokens, new Ids(), new Clock());
    }
    private sealed class MemoryRepository : IUserRepository { private readonly List<User> items=[]; public Task AddAsync(User user,CancellationToken ct){items.Add(user);return Task.CompletedTask;} public Task<User?> GetByEmailAsync(string email,CancellationToken ct)=>Task.FromResult(items.SingleOrDefault(x=>x.Email==email)); }
    private sealed class PlainHasher : IPasswordHasher { public string Hash(string password)=>password; public bool Verify(string password,string hash)=>password==hash; }
    private sealed class FakeTokens : ITokenGenerator { public bool WasCalled {get;private set;} public (string Token,DateTime ExpiresAt) Create(User user){WasCalled=true;return ($"token-{user.Role}",DateTime.UtcNow.AddHours(1));} }
    private sealed class Ids : IIdGenerator { public string NewId()=>Guid.NewGuid().ToString(); }
    private sealed class Clock : IClock { public DateTime UtcNow=>DateTime.UtcNow; }
}
