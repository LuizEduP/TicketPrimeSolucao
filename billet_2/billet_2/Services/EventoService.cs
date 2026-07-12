using System.Net.Http.Json;
using billet_2.Models;

namespace billet_2.Services;

public class EventoService
{
    private readonly HttpClient _http;

    public EventoService(HttpClient http)
    {
        _http = http;
    }

    public async Task<List<Evento>?> ListarEventosAsync()
    {
        return await _http.GetFromJsonAsync<List<Evento>>("api/eventos/listar");
    }

    public async Task<Evento?> BuscarPorIdAsync(int id)
    {
        return await _http.GetFromJsonAsync<Evento>($"api/eventos/listar/{id}");
    }
    public async Task<string?> CriarEventoAsync(Evento novoEvento)
    {
        try
        {
            var response = await _http.PostAsJsonAsync("api/eventos/cadastrar", novoEvento);
            
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