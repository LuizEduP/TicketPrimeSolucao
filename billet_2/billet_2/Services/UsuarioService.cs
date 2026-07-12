using System.Net.Http.Json;
using billet_2.Models;

namespace billet_2.Services;

public class UsuarioService
{
    private readonly HttpClient _http;

    public UsuarioService(HttpClient http)
    {
        _http = http;
    }

    // Retorna a lista de usuários (útil para o admin ver quem se cadastrou)
    public async Task<List<Usuario>?> ListarUsuariosAsync()
    {
        return await _http.GetFromJsonAsync<List<Usuario>>("api/usuarios/listar");
    }

    // Realiza o cadastro e trata erros vindos da API
    public async Task<string?> CadastrarAsync(Usuario usuario)
    {
        try
        {
            // Limpa o CPF para mandar apenas números para o banco
            if (!string.IsNullOrEmpty(usuario.Cpf))
            {
                usuario.Cpf = usuario.Cpf.Replace(".", "").Replace("-", "");
            }

            var response = await _http.PostAsJsonAsync("api/usuarios/cadastrar", usuario);

            if (response.IsSuccessStatusCode)
            {
                return null; // Sucesso!
            }
            else
            {
                var erro = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Erro na API: {erro}");
                return erro; // Retorna a mensagem de erro da API
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exceção ao cadastrar: {ex.Message}");
            return "Erro de conexão com o servidor.";
        }
    }
}