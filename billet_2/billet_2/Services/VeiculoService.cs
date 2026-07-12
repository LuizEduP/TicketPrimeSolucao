using System.Net.Http.Json;
using billet_2.Models;

namespace billet_2.Services;

public class VeiculoService
{
    private readonly HttpClient _http;

    public VeiculoService(HttpClient http)
    {
        _http = http;
    }

    public async Task<List<Veiculo>?> ListarVeiculosAsync()
    {
        return await _http.GetFromJsonAsync<List<Veiculo>>("api/veiculos/listar");
    }

    public async Task<Veiculo?> BuscarPorIdAsync(int id)
    {
        return await _http.GetFromJsonAsync<Veiculo>($"api/veiculos/listar/{id}");
    }

    public async Task<string?> CriarVeiculoAsync(Veiculo novoVeiculo)
    {
        try
        {
            var response = await _http.PostAsJsonAsync("api/veiculos/cadastrar", novoVeiculo);

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
