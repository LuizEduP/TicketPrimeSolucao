using Xunit;

public class ReservaValorTests
{
    [Fact]
    public void ValidarValorFinal_QuandoNegativo_NaoDeveSerValido()
    {
        // Arrange
        decimal valorFinal = -10;

        // Act
        bool valido = valorFinal >= 0;

        // Assert
        Assert.False(valido);
    }
}
