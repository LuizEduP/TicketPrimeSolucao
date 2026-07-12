using System.Net.Http.Json;
using billet_2.Models;

namespace billet_2.Services;

public class AssentoService
{
    private readonly HttpClient _http;

    public AssentoService(HttpClient http)
    {
        _http = http;
    }

    public async Task<List<Assento>?> ObterMapaAssentosAsync(int viagemId)
    {
        return await _http.GetFromJsonAsync<List<Assento>>($"api/assentos/viagem/{viagemId}");
    }

    public async Task<string?> ReservarAssentoAsync(int assentoId, string cpf)
    {
        try
        {
            var payload = new { AssentoId = assentoId, UsuarioCpf = cpf };
            var response = await _http.PostAsJsonAsync("api/assentos/reservar", payload);

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

    public async Task<string?> LiberarAssentoAsync(int assentoId)
    {
        try
        {
            var payload = new { AssentoId = assentoId };
            var response = await _http.PostAsJsonAsync("api/assentos/liberar", payload);

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

    public async Task<string?> BloquearAssentoAsync(int assentoId, bool bloquear)
    {
        try
        {
            var payload = new { AssentoId = assentoId, Bloquear = bloquear };
            var response = await _http.PostAsJsonAsync("api/assentos/bloquear", payload);

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
