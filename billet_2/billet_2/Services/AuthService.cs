using billet_2.Models;

namespace billet_2.Services;

public class AuthService
{
    public Usuario? UsuarioLogado { get; private set; }
    public bool EstaLogado = false;

    public void Logar(Usuario usuario)
    {
        UsuarioLogado = usuario;
        EstaLogado = true;
    }

    public void Deslogar()
    {
        UsuarioLogado = null;
        EstaLogado = false;
    }
}