# Spec 0040 — Criar Modelo `Viagem` + `ViagensController` + Registrar Rotas

## Metadados

| Campo | Valor |
|-------|-------|
| **Código** | 0040 |
| **Fase** | 1 — Backend: Novos Controllers |
| **Tipo** | Novo + Refatoração |
| **Prioridade** | 🔴 Alta |
| **Status** | Concluído |
| **Dependências** | Spec 0030 (diretórios renomeados `eventos/` → `viagens/`) — concluída |
| **Dependentes** | Spec 0080 (ViagemService + CriarViagem.razor) |
| **Estimativa** | 1,5 horas |
| **Documento de referência** | [`roadmap.md`](../roadmap.md) — Seção 4, Fase 1 |

---

## 1. Objetivo

Transformar o controller de Eventos (domínio TicketPrime) no controller de Viagens (domínio TripPrime). Isso envolve:

1. **Substituir** o modelo `Evento` pelo modelo `Viagem` com os campos do novo domínio.
2. **Refatorar** a classe `EventosController` → `ViagensController` com endpoints renomeados.
3. **Adicionar** um novo endpoint de pesquisa (`/api/viagens/pesquisar`).
4. **Registrar** as novas rotas em `Program.cs` e **remover** as antigas de eventos.

---

## 2. Motivação

### 2.1. Pivotagem de domínio

O conceito de "Evento" (show/festival) foi substituído por "Viagem" (transporte entre origem e destino). O modelo de dados precisa refletir:

- **Origem e Destino** como campos de identificação principal (antes: apenas `Nome` e `Local`).
- **Datas de partida, chegada e volta** (antes: apenas `Data`).
- **Veículo associado** (antes: capacidade era um campo direto `QuantidadeIngressos`).
- **Preço base da viagem** (antes: `ValorIngresso`).

### 2.2. API RESTful com endpoints claros

O controller anterior (`EventosController`) oferecia 3 endpoints. O novo controller adiciona um endpoint de **pesquisa por origem/destino/data**, essencial para a funcionalidade F04 do documento de visão.

### 2.3. Independência do frontend

Esta spec altera APENAS o backend (API). O frontend continuará usando os modelos/serviços antigos (`Evento`, `EventoService`) até a Spec 0080. Durante a transição, os endpoints `/api/eventos/*` serão removidos e o frontend pode quebrar — isso é esperado e será resolvido na Spec 0080.

---

## 3. Estado Atual (ANTES)

### 3.1. Arquivo: `src/viagens/EventosController.cs`

```csharp
public static class EventosController{
    private static List<Evento> Eventos = new();
    private static int idAtual = 1;
    public static void ListarEventos(this WebApplication app){
        app.MapGet("/api/eventos/listar", () => 
        {
            return Results.Ok(Eventos);
        });
    }
    public static void ListarEventoPorId(this WebApplication app){
        app.MapGet("/api/eventos/listar/{id}", (int id) => 
        {
            var evento = Eventos.FirstOrDefault(e => e.Id == id);
            if(evento == null)
                return Results.NotFound("Evento não encontrado.");
            return Results.Ok(evento);
        });
    }
    public static void CadastrarEventos(this WebApplication app){
        app.MapPost("/api/eventos/cadastrar", (Evento novoEvento) => 
        {

            if(Eventos.Any(e => e.Nome == novoEvento.Nome)){
                return Results.BadRequest("O nome de evento informado já está cadastrado");
            }

            if(novoEvento.Data < DateTime.Now){
                return Results.BadRequest("A data do evento não pode ser antiga, coloque uma data futura!");
            }

            novoEvento.Id = idAtual;
            idAtual++;

            Eventos.Add(novoEvento);
            return Results.Ok(novoEvento);
        });
    }
}

public class Evento{
    public int Id {get;set;}
    public string Nome {get;set;} = "";
    public string Descricao {get;set;} = "";
    public string Local {get;set;} = "";
    public DateTime Data {get;set;}
    public int QuantidadeIngressos {get;set;}
    public float ValorIngresso {get;set;}
    public string? FotoUrl { get; set; }
}
```

### 3.2. Arquivo: `src/Program.cs` (linhas 31-33)

```csharp
app.CadastrarEventos();
app.ListarEventos();
app.ListarEventoPorId();
```

---

## 4. Estado Desejado (DEPOIS)

### 4.1. Arquivo: `src/viagens/ViagensController.cs` (NOVO — substitui EventosController.cs)

```csharp
public static class ViagensController
{
    private static List<Viagem> Viagens = new();
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
```

### 4.2. Arquivo: `src/Program.cs` (linhas alteradas)

```csharp
app.CadastrarViagens();
app.ListarViagens();
app.ListarViagemPorId();
app.PesquisarViagens();
```

### 4.3. Mapeamento Campo a Campo (Evento → Viagem)

| Evento (ANTES) | Viagem (DEPOIS) | Observação |
|----------------|-----------------|------------|
| `Id` | `Id` | Mantido |
| `Nome` | `Origem` | Semântica alterada — agora é a cidade/estado de origem |
| — | `Destino` | **Novo** |
| `Data` | `DataPartida` | Renomeado |
| — | `DataChegada` | **Novo** |
| — | `DataVolta` | **Novo** (opcional — somente ida e volta) |
| `Descricao` | `Descricao` | Mantido |
| `Local` | — | **Removido** |
| `QuantidadeIngressos` | — | **Removido** (capacidade vem do Veículo) |
| — | `VeiculoId` | **Novo** (FK para Veiculo) |
| `ValorIngresso` | `PrecoBase` | Renomeado |
| `FotoUrl` | `FotoUrl` | Mantido |

---

## 5. Arquivos Afetados

| Arquivo | Operação | Descrição |
|---------|:--------:|-----------|
| `src/viagens/EventosController.cs` | **EXCLUIR** | Substituído pelo novo arquivo |
| `src/viagens/ViagensController.cs` | **CRIAR** | Novo controller com modelo Viagem e 4 endpoints |
| `src/Program.cs` | **EDITAR** | 4 linhas alteradas (remover chamadas antigas de Eventos, registrar novas de Viagens) |

**NENHUM outro arquivo do projeto é afetado por esta spec.**

---

## 6. Passos de Execução

### Passo 1: Excluir o arquivo antigo

- Excluir `src/viagens/EventosController.cs`.

### Passo 2: Criar o novo arquivo `ViagensController.cs`

- Criar `src/viagens/ViagensController.cs` com o conteúdo exato da Seção 4.1.
- O arquivo deve conter:
  - Classe estática `ViagensController` com lista em memória `Viagens` e contador `idAtual`.
  - Classe `Viagem` com 10 campos conforme tabela da Seção 4.3.
  - 4 métodos de extensão: `ListarViagens`, `ListarViagemPorId`, `PesquisarViagens`, `CadastrarViagens`.

### Passo 3: Atualizar `Program.cs`

- **Remover** as 3 linhas:
  ```csharp
  app.CadastrarEventos();
  app.ListarEventos();
  app.ListarEventoPorId();
  ```
- **Adicionar** as 4 linhas:
  ```csharp
  app.CadastrarViagens();
  app.ListarViagens();
  app.ListarViagemPorId();
  app.PesquisarViagens();
  ```
- As novas linhas devem ser inseridas no **mesmo local** onde estavam as antigas (entre `app.ListarUsuarios()` e `app.CadastrarCupons()`).

### Passo 4: Verificar build

- Executar `dotnet build` na solution.
- O build deve retornar **0 erros**.

---

## 7. Critérios de Aceitação

| ID | Critério | Verificação |
|:--:|----------|-------------|
| CA01 | O arquivo `src/viagens/EventosController.cs` NÃO existe mais | `! -f src/viagens/EventosController.cs` |
| CA02 | O arquivo `src/viagens/ViagensController.cs` EXISTE | `-f src/viagens/ViagensController.cs` |
| CA03 | A classe `Viagem` possui exatamente 10 campos: `Id`, `Origem`, `Destino`, `DataPartida`, `DataChegada`, `DataVolta`, `Descricao`, `VeiculoId`, `PrecoBase`, `FotoUrl` | Inspeção do código |
| CA04 | O endpoint `GET /api/viagens/listar` está registrado e retorna a lista de viagens | Chamada HTTP retorna 200 com array JSON |
| CA05 | O endpoint `GET /api/viagens/listar/{id}` retorna a viagem ou 404 | Chamada HTTP com ID existente → 200; ID inexistente → 404 |
| CA06 | O endpoint `GET /api/viagens/pesquisar?origem=X&destino=Y&data=Z` filtra corretamente | Chamada HTTP com parâmetros retorna resultados filtrados |
| CA07 | O endpoint `POST /api/viagens/cadastrar` valida: origem obrigatória, destino obrigatório, data futura, dataChegada > dataPartida, dataVolta > dataChegada (se informada), preco >= 0 (não pode ser negativo; zero é permitido) | Cada validação retorna 400 com mensagem |
| CA08 | `Program.cs` NÃO contém chamadas a `CadastrarEventos`, `ListarEventos` ou `ListarEventoPorId` | Grep negativo |
| CA09 | `Program.cs` CONTÉM chamadas a `CadastrarViagens`, `ListarViagens`, `ListarViagemPorId`, `PesquisarViagens` | Grep positivo (4 ocorrências) |
| CA10 | `dotnet build` na solution compila com 0 erros | Build output: "Compilação com êxito. 0 Erro(s)" |

---

## 8. Riscos e Observações

### 8.1. Risco: Frontend quebra temporariamente

- O frontend (`EventoService`, `Home.razor`, etc.) ainda referencia `/api/eventos/*` e o modelo `Evento`.
- Após esta spec, os endpoints `/api/eventos/*` deixarão de existir.
- **Impacto:** As páginas Blazor que dependem desses endpoints retornarão erro 404.
- **Mitigação:** Isto é esperado. A Spec 0080 (Frontend — ViagemService + CriarViagem) resolverá todas as dependências do frontend.
- **A Spec 0080 DEVE ser a próxima a ser executada após a Fase 1.**

### 8.2. Observação: `VeiculoId` sem validação de existência

- O campo `VeiculoId` referencia um veículo, mas o controller de Veículos (Spec 0050) ainda não existe.
- A validação de que o `VeiculoId` corresponde a um veículo real será adicionada quando ambos os controllers coexistirem (Spec 0070 ou Spec 0190).
- Por enquanto, `VeiculoId` é aceito como um inteiro qualquer.

### 8.3. Observação: Listas em memória

- Assim como o controller original, os dados são armazenados em `List<Viagem>` em memória.
- Reiniciar a API limpa todos os dados cadastrados.
- Esta limitação é conhecida e documentada no README original.

---

## 9. Referências

| Documento | Caminho | Relevância |
|-----------|---------|------------|
| Arquitetura pivotada | [`arquitetura-pivotagem.md`](../arquitetura-pivotagem.md) | Seção 4.2: modelo Viagem e endpoints |
| Roadmap da pivotagem | [`roadmap.md`](../roadmap.md) | Definição da Spec 0040 na Fase 1 |
| Visão da pivotagem | [`pivotagem.md`](../pivotagem.md) | Funcionalidades F01-F05, F15 |
| Controller original | `src/viagens/EventosController.cs` | Código a ser substituído |
| Program.cs | `src/Program.cs` | Registro de rotas |

---

> **Aprovado por:** Castor
> **Data de aprovação:** 31/05/2026
> **Data de implementação:** 31/05/2026
