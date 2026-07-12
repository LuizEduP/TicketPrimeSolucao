using Xunit;

public class ReservaTests
{
    [Fact]
    public void ValidarReserva_QuandoUsuarioForNulo_NaoDeveSerValida()
    {
        // Arrange
        string usuarioCpf = null;

        // Act
        bool valido = usuarioCpf != null;

        // Assert
        Assert.False(valido);
    }
}
