namespace billet_2.Models;

public class Usuario
{
    public int Id { get; set; }
    public string Nome { get; set; } = "";
    public string Email { get; set; } = "";
    public string Cpf { get; set; } = "";
    public bool Adm {get;set;} = false;
    public string Senha { get; set; } = "";
}