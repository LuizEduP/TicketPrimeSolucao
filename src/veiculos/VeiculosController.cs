public static class VeiculosController
{
    private static List<Veiculo> Veiculos = new();
    private static int idAtual = 1;

    // Lista pública de assentos gerados — será consumida pela Spec 0060 (AssentosController)
    public static List<Assento> Assentos = new();

    // GET /api/veiculos/listar
    public static void ListarVeiculos(this WebApplication app)
    {
        app.MapGet("/api/veiculos/listar", () =>
        {
            return Results.Ok(Veiculos);
        });
    }

    // GET /api/veiculos/listar/{id}
    public static void ListarVeiculoPorId(this WebApplication app)
    {
        app.MapGet("/api/veiculos/listar/{id}", (int id) =>
        {
            var veiculo = Veiculos.FirstOrDefault(v => v.Id == id);
            if (veiculo == null)
                return Results.NotFound("Veículo não encontrado.");
            return Results.Ok(veiculo);
        });
    }

    // POST /api/veiculos/cadastrar
    public static void CadastrarVeiculos(this WebApplication app)
    {
        app.MapPost("/api/veiculos/cadastrar", (Veiculo novoVeiculo) =>
        {
            // Validação 1: Modelo é obrigatório
            if (string.IsNullOrWhiteSpace(novoVeiculo.Modelo))
                return Results.BadRequest("O modelo do veículo é obrigatório.");

            // Validação 2: Placa é obrigatória
            if (string.IsNullOrWhiteSpace(novoVeiculo.Placa))
                return Results.BadRequest("A placa do veículo é obrigatória.");

            // Validação 3: Placa deve ser única
            if (Veiculos.Any(v => v.Placa.Equals(novoVeiculo.Placa, StringComparison.OrdinalIgnoreCase)))
                return Results.BadRequest("Já existe um veículo cadastrado com esta placa.");

            // Validação 4: Linhas deve ser > 0
            if (novoVeiculo.Linhas <= 0)
                return Results.BadRequest("O número de linhas (fileiras) deve ser maior que zero.");

            // Validação 5: Colunas deve ser > 0
            if (novoVeiculo.Colunas <= 0)
                return Results.BadRequest("O número de colunas deve ser maior que zero.");

            // Validação 6: Tipo deve ser um valor válido
            var tiposValidos = new[] { "Convencional", "Executivo", "Leito", "Micro-ônibus", "Van" };
            if (!tiposValidos.Contains(novoVeiculo.Tipo, StringComparer.OrdinalIgnoreCase))
                return Results.BadRequest($"Tipo inválido. Tipos permitidos: {string.Join(", ", tiposValidos)}.");

            // Atribuir ID e calcular capacidade
            novoVeiculo.Id = idAtual;
            idAtual++;
            novoVeiculo.Capacidade = novoVeiculo.Linhas * novoVeiculo.Colunas;

            // Normalizar Tipo (primeira letra maiúscula, resto minúscula) para consistência
            novoVeiculo.Tipo = char.ToUpper(novoVeiculo.Tipo[0]) + novoVeiculo.Tipo[1..].ToLower();

            Veiculos.Add(novoVeiculo);

            // Gerar assentos automaticamente
            GerarAssentos(novoVeiculo);

            return Results.Ok(novoVeiculo);
        });
    }

    // Gera assentos para um veículo recém-cadastrado
    private static void GerarAssentos(Veiculo veiculo)
    {
        // Colunas são nomeadas com letras: A, B, C, D, E, ...
        var nomeColunas = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

        for (int linha = 1; linha <= veiculo.Linhas; linha++)
        {
            for (int col = 0; col < veiculo.Colunas; col++)
            {
                char letraColuna = nomeColunas[col];
                string numero = $"{linha}{letraColuna}";

                // Define o tipo do assento pela posição:
                // - Primeira e última coluna: "Janela"
                // - Colunas do meio (corredor): "Corredor"
                string tipo;
                if (col == 0 || col == veiculo.Colunas - 1)
                    tipo = "Janela";
                else
                    tipo = "Corredor";

                var assento = new Assento
                {
                    Id = Assentos.Count + 1,
                    VeiculoId = veiculo.Id,
                    Numero = numero,
                    Tipo = tipo,
                    Status = "Disponível"
                };

                Assentos.Add(assento);
            }
        }
    }
}

public class Veiculo
{
    public int Id { get; set; }
    public string Modelo { get; set; } = "";
    public string Placa { get; set; } = "";
    public int Capacidade { get; set; }
    public string Tipo { get; set; } = "";
    public int Linhas { get; set; }
    public int Colunas { get; set; }
}

public class Assento
{
    public int Id { get; set; }
    public int VeiculoId { get; set; }
    public string Numero { get; set; } = "";
    public string Tipo { get; set; } = "";
    public string Status { get; set; } = "Disponível";
}
