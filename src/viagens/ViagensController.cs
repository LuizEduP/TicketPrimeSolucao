public static class ViagensController
{
    public static List<Viagem> Viagens = new();
    private static int idAtual = 1;

    // GET /api/viagens/listar
    public static void ListarViagens(this WebApplication app)
    {
        app.MapGet("/api/viagens/listar", () =>
        {
            return Results.Ok(Viagens);
        });
    }

    // GET /api/viagens/listar/{id}
    public static void ListarViagemPorId(this WebApplication app)
    {
        app.MapGet("/api/viagens/listar/{id}", (int id) =>
        {
            var viagem = Viagens.FirstOrDefault(v => v.Id == id);
            if (viagem == null)
                return Results.NotFound("Viagem não encontrada.");
            return Results.Ok(viagem);
        });
    }

    // GET /api/viagens/pesquisar?origem=&destino=&data=
    public static void PesquisarViagens(this WebApplication app)
    {
        app.MapGet("/api/viagens/pesquisar", (string? origem, string? destino, DateTime? data) =>
        {
            var resultado = Viagens.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(origem))
                resultado = resultado.Where(v =>
                    v.Origem.Contains(origem, StringComparison.OrdinalIgnoreCase));

            if (!string.IsNullOrWhiteSpace(destino))
                resultado = resultado.Where(v =>
                    v.Destino.Contains(destino, StringComparison.OrdinalIgnoreCase));

            if (data.HasValue)
                resultado = resultado.Where(v => v.DataPartida.Date == data.Value.Date);

            return Results.Ok(resultado.ToList());
        });
    }

    // POST /api/viagens/cadastrar
    public static void CadastrarViagens(this WebApplication app)
    {
        app.MapPost("/api/viagens/cadastrar", (Viagem novaViagem) =>
        {
            // Validação 1: Origem é obrigatória
            if (string.IsNullOrWhiteSpace(novaViagem.Origem))
                return Results.BadRequest("A origem da viagem é obrigatória.");

            // Validação 2: Destino é obrigatório
            if (string.IsNullOrWhiteSpace(novaViagem.Destino))
                return Results.BadRequest("O destino da viagem é obrigatório.");

            // Validação 3: Data de partida deve ser futura
            if (novaViagem.DataPartida < DateTime.Now)
                return Results.BadRequest("A data de partida não pode ser antiga. Informe uma data futura.");

            // Validação 4: Data de chegada deve ser após a partida
            if (novaViagem.DataChegada <= novaViagem.DataPartida)
                return Results.BadRequest("A data de chegada deve ser posterior à data de partida.");

            // Validação 5: Se DataVolta foi informada, deve ser após DataChegada
            if (novaViagem.DataVolta.HasValue && novaViagem.DataVolta.Value <= novaViagem.DataChegada)
                return Results.BadRequest("A data de volta deve ser posterior à data de chegada.");

            // Validação 6: Preço base não pode ser negativo (zero é permitido para distribuição gratuita)
            if (novaViagem.PrecoBase < 0)
                return Results.BadRequest("O preço base da viagem não pode ser negativo.");

            novaViagem.Id = idAtual;
            idAtual++;

            Viagens.Add(novaViagem);
            return Results.Ok(novaViagem);
        });
    }
}

public class Viagem
{
    public int Id { get; set; }
    public string Origem { get; set; } = "";
    public string Destino { get; set; } = "";
    public DateTime DataPartida { get; set; }
    public DateTime DataChegada { get; set; }
    public DateTime? DataVolta { get; set; }
    public string Descricao { get; set; } = "";
    public int VeiculoId { get; set; }
    public float PrecoBase { get; set; }
    public string? FotoUrl { get; set; }
}
