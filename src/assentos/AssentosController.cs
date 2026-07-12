public static class AssentosController
{
    // GET /api/assentos/viagem/{viagemId}
    public static void MapaAssentos(this WebApplication app)
    {
        app.MapGet("/api/assentos/viagem/{viagemId}", (int viagemId) =>
        {
            // Localiza a viagem
            var viagem = ViagensController.Viagens.FirstOrDefault(v => v.Id == viagemId);
            if (viagem == null)
                return Results.NotFound("Viagem não encontrada.");

            // Filtra os assentos do veículo associado
            var assentos = VeiculosController.Assentos
                .Where(a => a.VeiculoId == viagem.VeiculoId)
                .ToList();

            return Results.Ok(assentos);
        });
    }

    // POST /api/assentos/reservar
    public static void ReservarAssento(this WebApplication app)
    {
        app.MapPost("/api/assentos/reservar", (ReservaRequest request) =>
        {
            // Validação 1: assentoId > 0
            if (request.AssentoId <= 0)
                return Results.BadRequest("ID do assento inválido.");

            // Validação 2: CPF obrigatório
            if (string.IsNullOrWhiteSpace(request.UsuarioCpf))
                return Results.BadRequest("CPF do usuário é obrigatório para reservar um assento.");

            // Localiza o assento
            var assento = VeiculosController.Assentos.FirstOrDefault(a => a.Id == request.AssentoId);
            if (assento == null)
                return Results.NotFound("Assento não encontrado.");

            // Verifica se o assento está disponível
            if (assento.Status != "Disponível")
                return Results.BadRequest($"Assento {assento.Numero} não está disponível. Status atual: {assento.Status}.");

            // Reserva o assento
            assento.Status = "Reservado";

            return Results.Ok(assento);
        });
    }

    // POST /api/assentos/liberar
    public static void LiberarAssento(this WebApplication app)
    {
        app.MapPost("/api/assentos/liberar", (LiberarRequest request) =>
        {
            // Validação: assentoId > 0
            if (request.AssentoId <= 0)
                return Results.BadRequest("ID do assento inválido.");

            // Localiza o assento
            var assento = VeiculosController.Assentos.FirstOrDefault(a => a.Id == request.AssentoId);
            if (assento == null)
                return Results.NotFound("Assento não encontrado.");

            // Só pode liberar assento que está Reservado
            if (assento.Status != "Reservado")
                return Results.BadRequest($"Assento {assento.Numero} não está reservado. Status atual: {assento.Status}.");

            // Libera o assento
            assento.Status = "Disponível";

            return Results.Ok(assento);
        });
    }

    // POST /api/assentos/bloquear
    public static void BloquearAssento(this WebApplication app)
    {
        app.MapPost("/api/assentos/bloquear", (BloquearRequest request) =>
        {
            // Validação: assentoId > 0
            if (request.AssentoId <= 0)
                return Results.BadRequest("ID do assento inválido.");

            // Localiza o assento
            var assento = VeiculosController.Assentos.FirstOrDefault(a => a.Id == request.AssentoId);
            if (assento == null)
                return Results.NotFound("Assento não encontrado.");

            if (request.Bloquear)
            {
                // Bloquear: apenas se estiver Disponível
                if (assento.Status != "Disponível")
                    return Results.BadRequest($"Não é possível bloquear o assento {assento.Numero}. Status atual: {assento.Status}. Apenas assentos Disponíveis podem ser bloqueados.");

                assento.Status = "Indisponível";
            }
            else
            {
                // Desbloquear: apenas se estiver Indisponível
                if (assento.Status != "Indisponível")
                    return Results.BadRequest($"Não é possível desbloquear o assento {assento.Numero}. Status atual: {assento.Status}. Apenas assentos Indisponíveis podem ser desbloqueados.");

                assento.Status = "Disponível";
            }

            return Results.Ok(assento);
        });
    }
}

// --- Modelos de Request (APENAS para entrada de dados — NÃO redefinem Assento) ---

public class ReservaRequest
{
    public int AssentoId { get; set; }
    public string UsuarioCpf { get; set; } = "";
}

public class LiberarRequest
{
    public int AssentoId { get; set; }
}

public class BloquearRequest
{
    public int AssentoId { get; set; }
    public bool Bloquear { get; set; }
}
