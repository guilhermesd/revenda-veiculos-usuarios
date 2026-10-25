using Fiap.RevendaVeiculos.Usuarios.Domain;

namespace Fiap.RevendaVeiculos.Usuarios.Application;

public interface IUserRepository { Task AddAsync(User user,CancellationToken ct); Task<User?> GetByEmailAsync(string email,CancellationToken ct); }
public interface IPasswordHasher { string Hash(string password); bool Verify(string password,string hash); }
public interface ITokenGenerator { (string Token,DateTime ExpiresAt) Create(User user); }
public interface IIdGenerator { string NewId(); }
public interface IClock { DateTime UtcNow {get;} }
public sealed class ConflictException(string message):Exception(message);
public sealed class UnauthorizedException(string message):Exception(message);
public record RegisterUserInput(string Name,string Cpf,string Email,string Password,string Role="COMPRADOR");
public record LoginInput(string Email,string Password);
public record UserResult(string Id,string Name,string Cpf,string Email,string Role,DateTime CreatedAt);
public record LoginResult(string AccessToken,DateTime ExpiresAt);

public sealed class UserService(IUserRepository users,IPasswordHasher passwordHasher,ITokenGenerator tokens,IIdGenerator ids,IClock clock)
{
    public async Task<UserResult> RegisterAsync(RegisterUserInput input,CancellationToken ct)
    {
        if(string.IsNullOrWhiteSpace(input.Password)||input.Password.Length<8)throw new DomainException("password must have at least 8 characters.");
        if(await users.GetByEmailAsync(input.Email.Trim().ToLowerInvariant(),ct) is not null)throw new ConflictException("email is already registered.");
        var user=new User(ids.NewId(),input.Name,input.Cpf,input.Email,passwordHasher.Hash(input.Password),input.Role,clock.UtcNow,clock.UtcNow);
        await users.AddAsync(user,ct); return ToResult(user);
    }
    public async Task<LoginResult> LoginAsync(LoginInput input,CancellationToken ct)
    {
        var user=await users.GetByEmailAsync(input.Email.Trim().ToLowerInvariant(),ct);
        if(user is null||!passwordHasher.Verify(input.Password,user.PasswordHash))throw new UnauthorizedException("invalid credentials.");
        var token=tokens.Create(user); return new(token.Token,token.ExpiresAt);
    }
    private static UserResult ToResult(User c)=>new(c.Id,c.Name,c.Cpf,c.Email,c.Role,c.CreatedAt);
}
