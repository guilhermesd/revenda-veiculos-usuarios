namespace Fiap.RevendaVeiculos.Usuarios.Domain;

public sealed class DomainException(string message) : Exception(message);

public sealed class User
{
    public User(string id, string name, string cpf, string email, string passwordHash, string role, DateTime createdAt, DateTime updatedAt)
    { Id=id; Name=Required(name,"name"); Cpf=NormalizeCpf(cpf); Email=NormalizeEmail(email); PasswordHash=Required(passwordHash,"passwordHash"); Role=role is "ADM" or "COMPRADOR" ? role : throw new DomainException("role must be ADM or COMPRADOR."); CreatedAt=createdAt; UpdatedAt=updatedAt; }
    public string Id {get;} public string Name {get;} public string Cpf {get;} public string Email {get;} public string PasswordHash {get;} public string Role {get;} public DateTime CreatedAt {get;} public DateTime UpdatedAt {get;}
    private static string Required(string value,string name)=>string.IsNullOrWhiteSpace(value)?throw new DomainException($"{name} is required."):value.Trim();
    private static string NormalizeEmail(string email) { var value=Required(email,"email").ToLowerInvariant(); if(!value.Contains('@'))throw new DomainException("email is invalid."); return value; }
    private static string NormalizeCpf(string cpf) { var digits=new string((cpf??string.Empty).Where(char.IsDigit).ToArray()); if(digits.Length!=11||digits.Distinct().Count()==1)throw new DomainException("cpf is invalid."); return digits; }
}
