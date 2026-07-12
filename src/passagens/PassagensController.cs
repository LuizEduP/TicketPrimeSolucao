using Dapper;

public static class PassagensController
{
    private static List<Passagem> Passagens = new();
    private static int idAtual = 1;

    // GET /api/passagens/listar
    public static void ListarPassagens(this WebApplication app)
    {
        app.MapGet("/api/passagens/listar", () =>
        {
            return Results.Ok(Passagens);
        });
    }

    // GET /api/passagens/detalhadas — INNER JOIN entre Passagens, Viagens, Veiculos e Assentos
    public static void ListarPassagensDetalhadas(this WebApplication app)
    {
        app.MapGet("/api/passagens/detalhadas", (DbConnectionFactory db) =>
        {
            using var connection = db.CreateConnection();
            var sql = @"
                SELECT
                    p.""Id"" AS PassagemId,
                    p.""UsuarioCpf"",
                    p.""PrecoPago"",
                    p.""Status"" AS PassagemStatus,
                    p.""DataCompra"",
                    p.""CupomUtilizado"",
                    v.""Id"" AS ViagemId,
                    v.""Origem"",
                    v.""Destino"",
                    v.""DataPartida"",
                    v.""DataChegada"",
                    v.""PrecoBase"",
                    ve.""Id"" AS VeiculoId,
                    ve.""Modelo"",
                    ve.""Placa"",
                    a.""Id"" AS AssentoId,
                    a.""Numero"" AS AssentoNumero,
                    a.""Tipo"" AS AssentoTipo,
                    a.""Status"" AS AssentoStatus
                FROM ""Passagens"" p
                INNER JOIN ""Viagens"" v ON p.""ViagemId"" = v.""Id""
                INNER JOIN ""Veiculos"" ve ON v.""VeiculoId"" = ve.""Id""
                INNER JOIN ""Assentos"" a ON p.""AssentoId"" = a.""Id""
                ORDER BY p.""DataCompra"" DESC";

            var resultado = connection.Query<PassagemDetalhada>(sql);
            return Results.Ok(resultado);
        });
    }

    // GET /api/passagens/detalhadas/usuario/{cpf} — LEFT JOIN com parâmetro @Cpf
    public static void ListarPassagensDetalhadasPorUsuario(this WebApplication app)
    {
        app.MapGet("/api/passagens/detalhadas/usuario/{cpf}", (string cpf, DbConnectionFactory db) =>
        {
            if (string.IsNullOrWhiteSpace(cpf))
                return Results.BadRequest("CPF é obrigatório.");

            using var connection = db.CreateConnection();
            var sql = @"
                SELECT
                    p.""Id"" AS PassagemId,
                    p.""UsuarioCpf"",
                    p.""PrecoPago"",
                    p.""Status"" AS PassagemStatus,
                    p.""DataCompra"",
                    p.""CupomUtilizado"",
                    v.""Id"" AS ViagemId,
                    v.""Origem"",
                    v.""Destino"",
                    v.""DataPartida"",
                    v.""DataChegada"",
                    v.""PrecoBase"",
                    ve.""Id"" AS VeiculoId,
                    ve.""Modelo"",
                    ve.""Placa"",
                    a.""Id"" AS AssentoId,
                    a.""Numero"" AS AssentoNumero,
                    a.""Tipo"" AS AssentoTipo,
                    a.""Status"" AS AssentoStatus
                FROM ""Passagens"" p
                INNER JOIN ""Viagens"" v ON p.""ViagemId"" = v.""Id""
                INNER JOIN ""Veiculos"" ve ON v.""VeiculoId"" = ve.""Id""
                LEFT JOIN ""Assentos"" a ON p.""AssentoId"" = a.""Id""
                WHERE p.""UsuarioCpf"" = @Cpf
                ORDER BY p.""DataCompra"" DESC";

            var resultado = connection.Query<PassagemDetalhada>(sql, new { Cpf = cpf });
            return Results.Ok(resultado);
        });
    }

    // GET /api/passagens/usuario/{cpf}
    public static void ListarPassagensPorUsuario(this WebApplication app)
    {
        app.MapGet("/api/passagens/usuario/{cpf}", (string cpf) =>
        {
            if (string.IsNullOrWhiteSpace(cpf))
                return Results.BadRequest("CPF é obrigatório.");

            var passagens = Passagens
                .Where(p => p.UsuarioCpf == cpf)
                .ToList();

            if (passagens.Count == 0)
                return Results.Ok(new List<Passagem>()); // Lista vazia, não 404

            return Results.Ok(passagens);
        });
    }

    // POST /api/passagens/comprar
    public static void ComprarPassagem(this WebApplication app)
    {
        app.MapPost("/api/passagens/comprar", (CompraRequest request) =>
        {
            // Validação 1: ViagemId > 0
            if (request.ViagemId <= 0)
                return Results.BadRequest("ID da viagem inválido.");

            // Validação 2: AssentoId > 0
            if (request.AssentoId <= 0)
                return Results.BadRequest("ID do assento inválido.");

            // Validação 3: CPF obrigatório
            if (string.IsNullOrWhiteSpace(request.UsuarioCpf))
                return Results.BadRequest("CPF do usuário é obrigatório.");

            // Localiza a viagem (necessário para obter PrecoBase)
            var viagem = ViagensController.Viagens.FirstOrDefault(v => v.Id == request.ViagemId);
            if (viagem == null)
                return Results.NotFound("Viagem não encontrada.");

            // Localiza o assento
            var assento = VeiculosController.Assentos.FirstOrDefault(a => a.Id == request.AssentoId);
            if (assento == null)
                return Results.NotFound("Assento não encontrado.");

            // O assento DEVE estar Reservado para ser comprado
            if (assento.Status != "Reservado")
                return Results.BadRequest($"Assento {assento.Numero} não está reservado. Status atual: {assento.Status}. Apenas assentos Reservados podem ser comprados.");

            // Calcula o preço no backend (NÃO recebe do frontend — segurança)
            float precoBase = viagem.PrecoBase;
            float percentualDesconto = 0;
            string? cupomUtilizado = null;

            // Se um cupom foi informado, valida e aplica desconto
            if (!string.IsNullOrWhiteSpace(request.CupomUtilizado))
            {
                var cupom = CuponsController.Cupons.FirstOrDefault(c =>
                    c.Codigo.Equals(request.CupomUtilizado, StringComparison.OrdinalIgnoreCase));

                if (cupom == null)
                    return Results.BadRequest($"Cupom '{request.CupomUtilizado}' não encontrado.");

                percentualDesconto = cupom.PercentualDesconto;
                cupomUtilizado = cupom.Codigo; // Armazena o código original (case correto)
            }

            // Aplica desconto: PrecoPago = PrecoBase × (1 - desconto/100)
            float precoPago = precoBase * (1 - (percentualDesconto / 100f));

            // Preço não pode ser negativo após desconto
            if (precoPago < 0)
                precoPago = 0;

            // Transiciona assento para Vendido
            assento.Status = "Vendido";

            // Cria a passagem
            var passagem = new Passagem
            {
                Id = idAtual,
                ViagemId = request.ViagemId,
                AssentoId = request.AssentoId,
                UsuarioCpf = request.UsuarioCpf,
                PrecoPago = precoPago,
                CupomUtilizado = cupomUtilizado,
                Status = "Ativa",
                DataCompra = DateTime.Now,
                DataExpiracaoReserva = null // Reserva concluída — não expira mais
            };

            idAtual++;
            Passagens.Add(passagem);

            return Results.Ok(passagem);
        });
    }

    // POST /api/passagens/cancelar/{id}
    public static void CancelarPassagem(this WebApplication app)
    {
        app.MapPost("/api/passagens/cancelar/{id}", (int id) =>
        {
            // Validação: id > 0
            if (id <= 0)
                return Results.BadRequest("ID da passagem inválido.");

            // Localiza a passagem
            var passagem = Passagens.FirstOrDefault(p => p.Id == id);
            if (passagem == null)
                return Results.NotFound("Passagem não encontrada.");

            // Só pode cancelar passagem Ativa
            if (passagem.Status != "Ativa")
                return Results.BadRequest($"Passagem não pode ser cancelada. Status atual: {passagem.Status}. Apenas passagens Ativas podem ser canceladas.");

            // Transiciona passagem para Cancelada
            passagem.Status = "Cancelada";

            // Libera o assento associado
            var assento = VeiculosController.Assentos.FirstOrDefault(a => a.Id == passagem.AssentoId);
            if (assento != null && assento.Status == "Vendido")
            {
                assento.Status = "Disponível";
            }

            return Results.Ok(passagem);
        });
    }
}

// --- Modelo Passagem ---

public class Passagem
{
    public int Id { get; set; }
    public int ViagemId { get; set; }
    public int AssentoId { get; set; }
    public string UsuarioCpf { get; set; } = "";
    public float PrecoPago { get; set; }
    public string? CupomUtilizado { get; set; }
    public string Status { get; set; } = "Ativa";
    public DateTime DataCompra { get; set; }
    public DateTime? DataExpiracaoReserva { get; set; }
}

// --- Modelo de Request (APENAS para entrada do endpoint comprar) ---

public class CompraRequest
{
    public int ViagemId { get; set; }
    public int AssentoId { get; set; }
    public string UsuarioCpf { get; set; } = "";
    public string? CupomUtilizado { get; set; }
}

// --- Modelo de JOIN para endpoint detalhado ---

public class PassagemDetalhada
{
    public int PassagemId { get; set; }
    public string UsuarioCpf { get; set; } = "";
    public float PrecoPago { get; set; }
    public string PassagemStatus { get; set; } = "";
    public DateTime DataCompra { get; set; }
    public string? CupomUtilizado { get; set; }
    public int ViagemId { get; set; }
    public string Origem { get; set; } = "";
    public string Destino { get; set; } = "";
    public DateTime DataPartida { get; set; }
    public DateTime DataChegada { get; set; }
    public float PrecoBase { get; set; }
    public int VeiculoId { get; set; }
    public string Modelo { get; set; } = "";
    public string Placa { get; set; } = "";
    public int AssentoId { get; set; }
    public string AssentoNumero { get; set; } = "";
    public string AssentoTipo { get; set; } = "";
    public string AssentoStatus { get; set; } = "";
}
