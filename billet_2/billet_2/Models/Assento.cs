namespace billet_2.Models;

public class Assento
{
    public int Id { get; set; }
    public int VeiculoId { get; set; }
    public string Numero { get; set; } = "";
    public string Tipo { get; set; } = "";
    public string Status { get; set; } = "Disponível";
}
