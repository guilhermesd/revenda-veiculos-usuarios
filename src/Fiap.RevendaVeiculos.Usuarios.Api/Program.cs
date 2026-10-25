using Fiap.RevendaVeiculos.Usuarios.Application;
using Fiap.RevendaVeiculos.Usuarios.Domain;
using Fiap.RevendaVeiculos.Usuarios.Infrastructure;
using Microsoft.AspNetCore.Diagnostics;

var builder=WebApplication.CreateBuilder(args);
builder.Services.AddEndpointsApiExplorer();builder.Services.AddSwaggerGen();
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("Jwt"));
builder.Services.AddMongoInfrastructure(o=>{o.ConnectionString=builder.Configuration.GetConnectionString("Mongo")!;o.DatabaseName=builder.Configuration["Mongo:DatabaseName"]??"revenda_usuarios";});
builder.Services.AddSingleton<UserService>();builder.Services.AddHealthChecks();
var app=builder.Build();await app.Services.GetRequiredService<MongoInitializer>().InitializeAsync(CancellationToken.None);
app.UseExceptionHandler(e=>e.Run(async c=>{var x=c.Features.Get<IExceptionHandlerFeature>()?.Error;var s=x is ConflictException?409:x is UnauthorizedException?401:x is DomainException?400:500;await Results.Problem(statusCode:s,title:s==401?"Unauthorized":s==409?"Conflict":s==400?"Validation error":"Unexpected error",detail:app.Environment.IsDevelopment()?x?.Message:null).ExecuteAsync(c);}));
app.UseSwagger();app.UseSwaggerUI();app.MapHealthChecks("/health/live");app.MapHealthChecks("/health/ready");
var users=app.MapGroup("/api/v1").WithTags("Users and Authentication");
users.MapPost("/users",async(RegisterRequest r,UserService s,CancellationToken ct)=>Results.Created("/api/v1/users",await s.RegisterAsync(new(r.Name,r.Cpf,r.Email,r.Password,r.Role),ct)));
users.MapPost("/auth/login",async(LoginRequest r,UserService s,CancellationToken ct)=>Results.Ok(await s.LoginAsync(new(r.Email,r.Password),ct)));
app.Run();
public record RegisterRequest(string Name,string Cpf,string Email,string Password,string Role="COMPRADOR");public record LoginRequest(string Email,string Password);public partial class Program{}
