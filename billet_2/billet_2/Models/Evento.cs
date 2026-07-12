namespace billet_2.Models;

public class Evento
{
    public int Id { get; set; }
    public string Nome { get; set; } = "";
    public string Descricao { get; set; } = "";
    public string Local { get; set; } = "";
    public DateTime Data { get; set; }
    public int QuantidadeIngressos { get; set; }
    public float ValorIngresso { get; set; }
    public string? FotoUrl { get; set; }
}