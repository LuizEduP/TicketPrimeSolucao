namespace billet_2.Models;

public class Passagem
{
    public int Id { get; set; }
    public int ViagemId { get; set; }
    public int AssentoId { get; set; }
    public string UsuarioCpf { get; set; } = "";
    public float PrecoPago { get; set; }
    public string? CupomUtilizado { get; set; }
    public string Status { get; set; } = "Ativa";
    public DateTime DataCompra { get; set; }
    public DateTime? DataExpiracaoReserva { get; set; }
}
