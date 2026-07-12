using Xunit;

public class EventoPrecoTests
{
    [Fact]
    public void ValidarPreco_QuandoNegativo_NaoDeveSerValido()
    {
        // Arrange
        decimal preco = -50;

        // Act
        bool valido = preco >= 0;

        // Assert
        Assert.False(valido);
    }
}
