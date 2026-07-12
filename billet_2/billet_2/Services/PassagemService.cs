using System.Net.Http.Json;
using billet_2.Models;

namespace billet_2.Services;

public class PassagemService
{
    private readonly HttpClient _http;

    public PassagemService(HttpClient http)
    {
        _http = http;
    }

    public async Task<List<Passagem>?> ListarTodasAsync()
    {
        return await _http.GetFromJsonAsync<List<Passagem>>("api/passagens/listar");
    }

    public async Task<List<Passagem>?> ListarPorUsuarioAsync(string cpf)
    {
        return await _http.GetFromJsonAsync<List<Passagem>>($"api/passagens/usuario/{cpf}");
    }

    public async Task<string?> ComprarPassagemAsync(int viagemId, int assentoId, string cpf, string? cupom = null)
    {
        try
        {
            var payload = new
            {
                ViagemId = viagemId,
                AssentoId = assentoId,
                UsuarioCpf = cpf,
                CupomUtilizado = cupom
            };
            var response = await _http.PostAsJsonAsync("api/passagens/comprar", payload);

            if (response.IsSuccessStatusCode)
                return null;

            var erro = await response.Content.ReadAsStringAsync();
            return erro;
        }
        catch (Exception ex)
        {
            return $"Erro de conexão: {ex.Message}";
        }
    }

    public async Task<string?> CancelarPassagemAsync(int id)
    {
        try
        {
            var response = await _http.PostAsync($"api/passagens/cancelar/{id}", null);

            if (response.IsSuccessStatusCode)
                return null;

            var erro = await response.Content.ReadAsStringAsync();
            return erro;
        }
        catch (Exception ex)
        {
            return $"Erro de conexão: {ex.Message}";
        }
    }
}
