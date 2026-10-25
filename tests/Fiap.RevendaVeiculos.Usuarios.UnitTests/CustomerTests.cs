using Fiap.RevendaVeiculos.Usuarios.Domain;

namespace Fiap.RevendaVeiculos.Usuarios.UnitTests;

public class UserTests
{
    [Fact]
    public void User_normalizes_email_and_cpf()
    {
        var user=new User("id","Maria","529.982.247-25","MARIA@EXAMPLE.COM","hash","COMPRADOR",DateTime.UtcNow,DateTime.UtcNow);
        Assert.Equal("maria@example.com",user.Email);
        Assert.Equal("52998224725",user.Cpf);
    }

    [Fact]
    public void User_rejects_invalid_cpf()
    {
        Assert.Throws<DomainException>(()=>new User("id","Maria","11111111111","maria@example.com","hash","COMPRADOR",DateTime.UtcNow,DateTime.UtcNow));
    }
}
