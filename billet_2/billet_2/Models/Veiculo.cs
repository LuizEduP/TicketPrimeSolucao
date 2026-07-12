namespace billet_2.Models;

public class Veiculo
{
    public int Id { get; set; }
    public string Modelo { get; set; } = "";
    public string Placa { get; set; } = "";
    public int Capacidade { get; set; }
    public string Tipo { get; set; } = "";
    public int Linhas { get; set; }
    public int Colunas { get; set; }
}
