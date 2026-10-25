using BCrypt.Net;
using Fiap.RevendaVeiculos.Usuarios.Application;
using Fiap.RevendaVeiculos.Usuarios.Domain;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace Fiap.RevendaVeiculos.Usuarios.Infrastructure;

public sealed class MongoSettings { public string ConnectionString {get;set;}="mongodb://localhost:27017"; public string DatabaseName {get;set;}="revenda_usuarios"; }
public sealed class JwtSettings { public string Key {get;set;}=null!; public string Issuer {get;set;}="revenda-usuarios"; public string Audience {get;set;}="revenda-vendas"; public int ExpirationMinutes {get;set;}=60; }
public sealed class MongoIdGenerator:IIdGenerator { public string NewId()=>ObjectId.GenerateNewId().ToString(); }
public sealed class SystemClock:IClock { public DateTime UtcNow=>DateTime.UtcNow; }
public sealed class BCryptPasswordHasher:IPasswordHasher { public string Hash(string password)=>BCrypt.Net.BCrypt.HashPassword(password); public bool Verify(string password,string hash)=>BCrypt.Net.BCrypt.Verify(password,hash); }
public sealed class JwtTokenGenerator(IOptions<JwtSettings> settings,IClock clock):ITokenGenerator
{
    public (string Token,DateTime ExpiresAt) Create(User c) { var s=settings.Value; var expires=clock.UtcNow.AddMinutes(s.ExpirationMinutes); var claims=new[]{new Claim(JwtRegisteredClaimNames.Sub,c.Id),new Claim(ClaimTypes.NameIdentifier,c.Id),new Claim(JwtRegisteredClaimNames.Email,c.Email),new Claim(ClaimTypes.Role,c.Role)}; var token=new JwtSecurityToken(s.Issuer,s.Audience,claims,expires:expires,signingCredentials:new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(s.Key)),SecurityAlgorithms.HmacSha256)); return(new JwtSecurityTokenHandler().WriteToken(token),expires); }
}
public sealed class UserDocument
{
    [BsonId,BsonRepresentation(BsonType.ObjectId)] public string Id {get;set;}=null!; public string Name {get;set;}=null!; public string Cpf {get;set;}=null!; public string Email {get;set;}=null!; public string PasswordHash {get;set;}=null!; public string Role {get;set;}="COMPRADOR"; public DateTime CreatedAt {get;set;} public DateTime UpdatedAt {get;set;}
    public static UserDocument From(User x)=>new(){Id=x.Id,Name=x.Name,Cpf=x.Cpf,Email=x.Email,PasswordHash=x.PasswordHash,Role=x.Role,CreatedAt=x.CreatedAt,UpdatedAt=x.UpdatedAt};
    public User ToDomain()=>new(Id,Name,Cpf,Email,PasswordHash,Role,CreatedAt,UpdatedAt);
}
public sealed class MongoUserRepository(IMongoDatabase db):IUserRepository
{
    private readonly IMongoCollection<UserDocument> _users=db.GetCollection<UserDocument>("users");
    public Task AddAsync(User user,CancellationToken ct)=>_users.InsertOneAsync(UserDocument.From(user),cancellationToken:ct);
    public async Task<User?> GetByEmailAsync(string email,CancellationToken ct)=>(await _users.Find(x=>x.Email==email).FirstOrDefaultAsync(ct))?.ToDomain();
}
public sealed class MongoInitializer(IMongoDatabase db)
{
    public async Task InitializeAsync(CancellationToken ct) { var c=db.GetCollection<UserDocument>("users"); await c.Indexes.CreateOneAsync(new CreateIndexModel<UserDocument>(Builders<UserDocument>.IndexKeys.Ascending(x=>x.Email),new CreateIndexOptions{Unique=true}),cancellationToken:ct); await c.Indexes.CreateOneAsync(new CreateIndexModel<UserDocument>(Builders<UserDocument>.IndexKeys.Ascending(x=>x.Cpf),new CreateIndexOptions{Unique=true}),cancellationToken:ct); }
}
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddMongoInfrastructure(this IServiceCollection services,Action<MongoSettings> configure)
    { services.Configure(configure);services.AddSingleton<IMongoClient>(sp=>new MongoClient(sp.GetRequiredService<IOptions<MongoSettings>>().Value.ConnectionString));services.AddSingleton(sp=>sp.GetRequiredService<IMongoClient>().GetDatabase(sp.GetRequiredService<IOptions<MongoSettings>>().Value.DatabaseName));services.AddSingleton<IUserRepository,MongoUserRepository>();services.AddSingleton<IPasswordHasher,BCryptPasswordHasher>();services.AddSingleton<ITokenGenerator,JwtTokenGenerator>();services.AddSingleton<IIdGenerator,MongoIdGenerator>();services.AddSingleton<IClock,SystemClock>();services.AddSingleton<MongoInitializer>();return services; }
}
