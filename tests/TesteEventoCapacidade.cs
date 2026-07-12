using Xunit;

public class EventoTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(-10)]
    public void ValidarCapacidade_QuandoZeroOuNegativa_NaoDeveSerValida(int capacidade)
    {
        // Arrange
        // Os valores de capacidade são fornecidos via [InlineData]

        // Act
        bool valido = capacidade > 0;

        // Assert
        Assert.False(valido);
    }
}
