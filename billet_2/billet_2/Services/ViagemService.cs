using System.Net.Http.Json;
using billet_2.Models;

namespace billet_2.Services;

public class ViagemService
{
    private readonly HttpClient _http;

    public ViagemService(HttpClient http)
    {
        _http = http;
    }

    public async Task<List<Viagem>?> ListarViagensAsync()
    {
        return await _http.GetFromJsonAsync<List<Viagem>>("api/viagens/listar");
    }

    public async Task<Viagem?> BuscarPorIdAsync(int id)
    {
        return await _http.GetFromJsonAsync<Viagem>($"api/viagens/listar/{id}");
    }

    public async Task<List<Viagem>?> PesquisarViagensAsync(string? origem, string? destino, DateTime? data)
    {
        var query = $"api/viagens/pesquisar?origem={origem}&destino={destino}&data={data:yyyy-MM-dd}";
        return await _http.GetFromJsonAsync<List<Viagem>>(query);
    }

    public async Task<string?> CriarViagemAsync(Viagem novaViagem)
    {
        try
        {
            var response = await _http.PostAsJsonAsync("api/viagens/cadastrar", novaViagem);

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
