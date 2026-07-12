# Spec 0050 — Criar Modelo `Veiculo` + `VeiculosController` + Registrar Rotas

## Metadados

| Campo | Valor |
|-------|-------|
| **Código** | 0050 |
| **Fase** | 1 — Backend: Novos Controllers |
| **Tipo** | Novo |
| **Prioridade** | 🔴 Alta |
| **Status** | Pendente |
| **Dependências** | Spec 0040 (ViagensController) — **Opcional** (apenas referência de padrão; não bloqueia) |
| **Dependentes** | Spec 0060 (AssentosController — usa lista de assentos gerada aqui), Spec 0090 (VeiculoService + CriarVeiculo.razor) |
| **Estimativa** | 1,5 horas |
| **Documento de referência** | [`roadmap.md`](../roadmap.md) — Seção 4, Fase 1 |

---

## 1. Objetivo

Criar o controller de **Veículos** para o domínio TripPrime. Isso envolve:

1. **Criar** o modelo `Veiculo` com 7 campos conforme especificado na arquitetura pivotada.
2. **Criar** a classe estática `VeiculosController` com 3 endpoints (listar, listar por id, cadastrar).
3. **Gerar assentos automaticamente** ao cadastrar um veículo: `Linhas × Colunas` assentos são criados e armazenados em uma lista pública compartilhada.
4. **Registrar** as novas rotas em `Program.cs`.

---

## 2. Motivação

### 2.1. Domínio TripPrime

O conceito de "Veículo" é novo — não existe equivalente no TicketPrime original. Cada viagem (`Viagem`) referencia um veículo (`VeiculoId`), e cada veículo define a capacidade e o mapa de assentos da viagem.

### 2.2. Geração automática de assentos

O roadmap define que, ao cadastrar um veículo, os assentos devem ser gerados automaticamente com base em `Linhas` e `Colunas`. Isso evita que o administrador cadastre manualmente cada poltrona.

A lista de assentos gerada (`VeiculosController.Assentos`) será **pública e estática**, permitindo que o futuro `AssentosController` (Spec 0060) acesse e manipule os mesmos dados.

### 2.3. Capacidade calculada

O campo `Capacidade` é derivado: `Capacidade = Linhas × Colunas`. Ele é calculado automaticamente no cadastro, não informado pelo usuário.

---

## 3. Estado Atual (ANTES)

### 3.1. Diretório `src/veiculos/`

**Não existe.** Este diretório será criado nesta spec.

### 3.2. Arquivo `src/Program.cs` (linhas 29-36)

```csharp
app.CadastrarUsuarios();
app.ListarUsuarios();
app.CadastrarViagens();
app.ListarViagens();
app.ListarViagemPorId();
app.PesquisarViagens();
app.CadastrarCupons();
app.ListarCupons();
```

Nenhuma rota de veículos está registrada.

---

## 4. Estado Desejado (DEPOIS)

### 4.1. Arquivo: `src/veiculos/VeiculosController.cs` (NOVO)

```csharp
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
```

### 4.2. Arquivo: `src/Program.cs` (linhas adicionadas)

```csharp
app.CadastrarUsuarios();
app.ListarUsuarios();
app.CadastrarViagens();
app.ListarViagens();
app.ListarViagemPorId();
app.PesquisarViagens();
app.CadastrarVeiculos();       // ← NOVA LINHA
app.ListarVeiculos();          // ← NOVA LINHA
app.ListarVeiculoPorId();      // ← NOVA LINHA
app.CadastrarCupons();
app.ListarCupons();
```

### 4.3. Detalhamento do Modelo `Veiculo`

| Campo | Tipo | Restrição | Descrição |
|-------|------|-----------|-----------|
| `Id` | `int` | Auto-incremento | Identificador único |
| `Modelo` | `string` | Obrigatório (não vazio) | Nome/modelo do veículo (ex: "Mercedes-Benz O500") |
| `Placa` | `string` | Obrigatório + Único (case-insensitive) | Placa do veículo (ex: "ABC-1234") |
| `Capacidade` | `int` | Calculado = `Linhas × Colunas` | Número total de assentos |
| `Tipo` | `string` | Deve ser um de: "Convencional", "Executivo", "Leito", "Micro-ônibus", "Van" | Categoria do veículo |
| `Linhas` | `int` | > 0 | Número de fileiras de assentos |
| `Colunas` | `int` | > 0 | Número de colunas por fileira |

### 4.4. Detalhamento dos Endpoints

| Método | Rota | Descrição |
|--------|------|-----------|
| `GET` | `/api/veiculos/listar` | Retorna a lista completa de veículos cadastrados |
| `GET` | `/api/veiculos/listar/{id}` | Retorna um veículo específico ou 404 se não encontrado |
| `POST` | `/api/veiculos/cadastrar` | Cadastra um novo veículo e gera seus assentos automaticamente |

### 4.5. Regras de Geração de Assentos

Ao cadastrar um veículo com `Linhas = L` e `Colunas = C`:

1. São gerados `L × C` assentos.
2. A numeração segue o padrão `"{linha}{letraColuna}"` (ex: "1A", "1B", "2A", "2B").
3. As colunas são nomeadas com letras: A=0, B=1, C=2, D=3, E=4.
4. O tipo do assento é definido pela posição:
   - **Janela**: primeira coluna (coluna 0) e última coluna (coluna C−1).
   - **Corredor**: todas as colunas do meio.
5. O status inicial de todo assento é **"Disponível"**.
6. Os assentos são armazenados na lista pública estática `VeiculosController.Assentos`.

**Exemplo:** Veículo com `Linhas=3, Colunas=4` gera 12 assentos:

```
Fileira 1: [1A Janela] [1B Corredor] [1C Corredor] [1D Janela]
Fileira 2: [2A Janela] [2B Corredor] [2C Corredor] [2D Janela]
Fileira 3: [3A Janela] [3B Corredor] [3C Corredor] [3D Janela]
```

---

## 5. Arquivos Afetados

| Arquivo | Operação | Descrição |
|---------|:--------:|-----------|
| `src/veiculos/` | **CRIAR** diretório | Nova pasta para o controller |
| `src/veiculos/VeiculosController.cs` | **CRIAR** | Controller com modelo Veiculo, modelo Assento e 3 endpoints |
| `src/Program.cs` | **EDITAR** | Adicionar 3 linhas de registro de rotas |

**NENHUM outro arquivo do projeto é afetado por esta spec.**

---

## 6. Passos de Execução

### Passo 1: Criar o diretório `src/veiculos/`

- Criar a pasta `src/veiculos/` no projeto.

### Passo 2: Criar o arquivo `VeiculosController.cs`

- Criar `src/veiculos/VeiculosController.cs` com o conteúdo exato da Seção 4.1.
- O arquivo deve conter:
  - Classe estática `VeiculosController` com:
    - Lista privada `Veiculos` e contador `idAtual`.
    - Lista pública estática `Assentos` (compartilhada com Spec 0060).
    - 3 métodos de extensão: `ListarVeiculos`, `ListarVeiculoPorId`, `CadastrarVeiculos`.
    - Método privado `GerarAssentos(Veiculo)` para geração automática.
  - Classe `Veiculo` com 7 campos.
  - Classe `Assento` com 5 campos.

### Passo 3: Atualizar `Program.cs`

- **Adicionar** 3 linhas após as rotas de viagens:
  ```csharp
  app.CadastrarVeiculos();
  app.ListarVeiculos();
  app.ListarVeiculoPorId();
  ```
- As novas linhas devem ser inseridas entre `app.PesquisarViagens();` e `app.CadastrarCupons();`.

### Passo 4: Verificar build

- Executar `dotnet build` na solution.
- O build deve retornar **0 erros**.

---

## 7. Critérios de Aceitação

| ID | Critério | Verificação |
|:--:|----------|-------------|
| CA01 | O diretório `src/veiculos/` EXISTE | `ls -d src/veiculos/` |
| CA02 | O arquivo `src/veiculos/VeiculosController.cs` EXISTE | `-f src/veiculos/VeiculosController.cs` |
| CA03 | A classe `Veiculo` possui exatamente 7 campos: `Id`, `Modelo`, `Placa`, `Capacidade`, `Tipo`, `Linhas`, `Colunas` | Inspeção do código |
| CA04 | O endpoint `GET /api/veiculos/listar` está registrado e retorna a lista de veículos | Chamada HTTP retorna 200 com array JSON |
| CA05 | O endpoint `GET /api/veiculos/listar/{id}` retorna o veículo ou 404 | Chamada HTTP: ID existente → 200; ID inexistente → 404 |
| CA06 | O endpoint `POST /api/veiculos/cadastrar` valida: modelo obrigatório, placa obrigatória, placa única, linhas > 0, colunas > 0, tipo válido | Cada validação retorna 400 com mensagem |
| CA07 | Ao cadastrar um veículo com `Linhas=3, Colunas=4`, a `Capacidade` é automaticamente definida como 12 | Inspeção da resposta do POST |
| CA08 | Ao cadastrar um veículo com `Linhas=3, Colunas=4`, exatamente 12 assentos são gerados em `VeiculosController.Assentos` | `Assentos.Count == 12` após o cadastro |
| CA09 | Os assentos gerados têm numeração correta: "1A", "1B", "1C", "1D", "2A", ..., "3D" | Inspeção da lista `Assentos` |
| CA10 | Os assentos das extremidades (coluna 0 e última) são do tipo "Janela"; os demais são "Corredor" | Inspeção da lista `Assentos` |
| CA11 | O status inicial de todo assento gerado é "Disponível" | Inspeção da lista `Assentos` |
| CA12 | `Program.cs` CONTÉM chamadas a `CadastrarVeiculos`, `ListarVeiculos`, `ListarVeiculoPorId` | Grep positivo (3 ocorrências) |
| CA13 | `dotnet build` na solution compila com 0 erros | Build output: "Compilação com êxito. 0 Erro(s)" |

---

## 8. Riscos e Observações

### 8.1. Risco: Duplicação do modelo `Assento`

- O modelo `Assento` é definido dentro de `VeiculosController.cs` nesta spec.
- A Spec 0060 (AssentosController) também precisará do modelo `Assento`.
- **Mitigação:** A Spec 0060 DEVE referenciar o modelo `Assento` já existente em `VeiculosController.cs`, NÃO redefini-lo. Caso a classe `Assento` já exista no namespace, a Spec 0060 apenas a reutiliza.

### 8.2. Observação: Placa como identificador único

- A unicidade da placa é validada apenas no cadastro. Se a lista for reiniciada (API reiniciada), a validação recomeça.
- O campo `Placa` não possui formatação obrigatória (ex: "ABC-1234" ou "ABC1D23"). Apenas não pode ser vazia nem repetida.

### 8.3. Observação: Colunas limitadas a 26 (A-Z)

- O sistema de numeração usa letras A-Z para colunas. O número máximo de colunas é 26.
- **NÃO** há validação explícita para `Colunas > 26` nesta spec. Se `Colunas > 26`, ocorrerá `IndexOutOfRangeException` em `GerarAssentos`.
- Esta limitação é aceitável no escopo atual (veículos reais raramente têm mais de 5 colunas). Pode ser abordada na Fase 6 (Melhorias).

### 8.4. Observação: Listas em memória

- Assim como os controllers existentes, os dados são armazenados em `List<T>` em memória.
- Reiniciar a API limpa todos os dados cadastrados.
- Esta limitação é conhecida e documentada no README original.

### 8.5. Observação: Independência da Spec 0040

- Esta spec NÃO depende da Spec 0040 para ser implementada. Os controllers são independentes.
- A dependência listada como "Opcional" existe apenas para referência de padrão de código (estilo, nomenclatura, estrutura).

---

## 9. Referências

| Documento | Caminho | Relevância |
|-----------|---------|------------|
| Arquitetura pivotada | [`arquitetura-pivotagem.md`](../arquitetura-pivotagem.md) | Seção 4.2: modelo Veiculo; Seção 4.3: endpoints |
| Roadmap da pivotagem | [`roadmap.md`](../roadmap.md) | Definição da Spec 0050 na Fase 1 |
| Visão da pivotagem | [`pivotagem.md`](../pivotagem.md) | Funcionalidade F14 (cadastrar veículos) |
| Spec 0040 (Viagens) | [`specs/0040-modelo-controller-viagem.md`](0040-modelo-controller-viagem.md) | Padrão de estrutura e estilo |
| Program.cs | `src/Program.cs` | Registro de rotas |

---

> **Aguardando aprovação do usuário.**
> **NÃO implementar até que o status seja alterado para "Aprovado".**
