using Xunit;

public class CupomTests
{
    [Theory]
    [InlineData(-5)]
    [InlineData(120)]
    public void ValidarDesconto_QuandoForaDoIntervalo_NaoDeveSerValido(decimal desconto)
    {
        // Arrange
        // Os valores de desconto são fornecidos via [InlineData]

        // Act
        bool valido = desconto >= 0 && desconto <= 100;

        // Assert
        Assert.False(valido);
    }
}
