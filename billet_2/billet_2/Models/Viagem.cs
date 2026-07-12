namespace billet_2.Models;

public class Viagem
{
    public int Id { get; set; }
    public string Origem { get; set; } = "";
    public string Destino { get; set; } = "";
    public DateTime DataPartida { get; set; }
    public DateTime DataChegada { get; set; }
    public DateTime? DataVolta { get; set; }
    public string Descricao { get; set; } = "";
    public int VeiculoId { get; set; }
    public float PrecoBase { get; set; }
    public string? FotoUrl { get; set; }
}
