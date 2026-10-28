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

    [Fact]
    public async Task Update_changes_profile_role_and_password()
    {
        var service = CreateService(out _, out _);
        var created = await service.RegisterAsync(new("Maria", "52998224725", "maria@example.com", "Senha@123"), CancellationToken.None);
        var updated = await service.UpdateAsync(created.Id, new("Maria Silva", "39053344705", "nova@example.com", "ADM", "NovaSenha@123"), CancellationToken.None);
        var login = await service.LoginAsync(new("nova@example.com", "NovaSenha@123"), CancellationToken.None);
        Assert.Equal("Maria Silva", updated.Name);
        Assert.Equal("ADM", updated.Role);
        Assert.Equal("token-ADM", login.AccessToken);
    }

    [Fact]
    public async Task Update_rejects_email_owned_by_another_user()
    {
        var service = CreateService(out _, out _);
        var first = await service.RegisterAsync(new("Maria", "52998224725", "maria@example.com", "Senha@123"), CancellationToken.None);
        await service.RegisterAsync(new("Joao", "39053344705", "joao@example.com", "Senha@123"), CancellationToken.None);
        await Assert.ThrowsAsync<ConflictException>(() => service.UpdateAsync(first.Id, new("Maria", "52998224725", "joao@example.com", "COMPRADOR", null), CancellationToken.None));
    }

    [Fact]
    public async Task Delete_removes_user_and_get_returns_not_found()
    {
        var service = CreateService(out _, out _);
        var created = await service.RegisterAsync(new("Maria", "52998224725", "maria@example.com", "Senha@123"), CancellationToken.None);
        await service.DeleteAsync(created.Id, CancellationToken.None);
        await Assert.ThrowsAsync<NotFoundException>(() => service.GetAsync(created.Id, CancellationToken.None));
        Assert.Empty(await service.ListAsync(CancellationToken.None));
    }

    private static UserService CreateService(out MemoryRepository repository, out FakeTokens tokens)
    {
        repository = new(); tokens = new();
        return new UserService(repository, new PlainHasher(), tokens, new Ids(), new Clock());
    }
    private sealed class MemoryRepository : IUserRepository
    {
        private readonly List<User> items=[];
        public Task AddAsync(User user,CancellationToken ct){items.Add(user);return Task.CompletedTask;}
        public Task<User?> GetByIdAsync(string id,CancellationToken ct)=>Task.FromResult(items.SingleOrDefault(x=>x.Id==id));
        public Task<User?> GetByEmailAsync(string email,CancellationToken ct)=>Task.FromResult(items.SingleOrDefault(x=>x.Email==email));
        public Task<User?> GetByCpfAsync(string cpf,CancellationToken ct)=>Task.FromResult(items.SingleOrDefault(x=>x.Cpf==cpf));
        public Task<IReadOnlyList<User>> ListAsync(CancellationToken ct)=>Task.FromResult<IReadOnlyList<User>>(items);
        public Task ReplaceAsync(User user,CancellationToken ct){var index=items.FindIndex(x=>x.Id==user.Id);items[index]=user;return Task.CompletedTask;}
        public Task DeleteAsync(string id,CancellationToken ct){items.RemoveAll(x=>x.Id==id);return Task.CompletedTask;}
    }
    private sealed class PlainHasher : IPasswordHasher { public string Hash(string password)=>password; public bool Verify(string password,string hash)=>password==hash; }
    private sealed class FakeTokens : ITokenGenerator { public bool WasCalled {get;private set;} public (string Token,DateTime ExpiresAt) Create(User user){WasCalled=true;return ($"token-{user.Role}",DateTime.UtcNow.AddHours(1));} }
    private sealed class Ids : IIdGenerator { public string NewId()=>Guid.NewGuid().ToString(); }
    private sealed class Clock : IClock { public DateTime UtcNow=>DateTime.UtcNow; }
}
