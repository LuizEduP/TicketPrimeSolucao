# Spec 0060 — Criar `AssentosController` + Compartilhar Listas + Registrar Rotas

## Metadados

| Campo | Valor |
|-------|-------|
| **Código** | 0060 |
| **Fase** | 1 — Backend: Novos Controllers |
| **Tipo** | Novo |
| **Prioridade** | 🔴 Alta |
| **Status** | Pendente |
| **Dependências** | Spec 0050 (VeiculosController — lista `Assentos` pública + modelo `Assento`) |
| **Dependentes** | Spec 0100 (AssentoService + MapaAssentos.razor) |
| **Estimativa** | 1,5 horas |
| **Documento de referência** | [`roadmap.md`](../roadmap.md) — Seção 4, Fase 1 |

---

## 1. Objetivo

Criar o controller de **Assentos** para o domínio TripPrime. Isso envolve:

1. **Criar** a classe estática `AssentosController` com 4 endpoints.
2. **Reutilizar** o modelo `Assento` já definido na Spec 0050 (`VeiculosController.cs`).
3. **Operar sobre** a lista pública `VeiculosController.Assentos` (sem duplicar dados).
4. **Expor** a lista de viagens tornando `ViagensController.Viagens` pública (alteração de 1 palavra: `private` → `public`).
5. **Registrar** as novas rotas em `Program.cs`.

---

## 2. Motivação

### 2.1. Mapa de assentos por viagem

O endpoint `GET /api/assentos/viagem/{viagemId}` é o ponto de entrada para o frontend exibir o mapa interativo de assentos. Ele precisa:
- Encontrar a viagem pelo ID.
- Descobrir qual veículo está associado à viagem (`VeiculoId`).
- Retornar todos os assentos daquele veículo com seus status atuais.

### 2.2. Operações de estado nos assentos

Os endpoints de reserva, liberação e bloqueio manipulam o status de um assento. Estas operações são a base para:
- **Reserva temporária** (F08 do documento de visão)
- **Compra de passagem** (F09 — a ser implementada na Spec 0070)
- **Gerenciamento administrativo** (F16)

### 2.3. Compartilhamento de listas em memória

O projeto usa listas estáticas em memória. Para que o `AssentosController` consulte quais assentos pertencem a uma viagem, ele precisa:
- Da lista de **viagens** → atualmente privada em `ViagensController`.
- Da lista de **assentos** → já pública em `VeiculosController.Assentos`.

A alteração de `private` para `public` em `ViagensController.Viagens` é a intervenção mínima necessária. Nenhuma outra modificação é feita no arquivo.

---

## 3. Estado Atual (ANTES)

### 3.1. Arquivo: `src/viagens/ViagensController.cs` (linha 3)

```csharp
private static List<Viagem> Viagens = new();
```

A lista de viagens é **privada** — nenhum outro controller pode acessá-la.

### 3.2. Arquivo: `src/veiculos/VeiculosController.cs` (linhas 1-6)

```csharp
public static class VeiculosController
{
    private static List<Veiculo> Veiculos = new();
    private static int idAtual = 1;

    public static List<Assento> Assentos = new();
```

A lista de assentos é **pública** e contém as classes `Veiculo` e `Assento` (já definidas).

### 3.3. Arquivo: `src/Program.cs` (trecho relevante)

```csharp
app.PesquisarViagens();
app.CadastrarVeiculos();
app.ListarVeiculos();
app.ListarVeiculoPorId();
app.CadastrarCupons();
```

Nenhuma rota de assentos está registrada.

### 3.4. Diretório `src/assentos/`

**Não existe.** Este diretório será criado nesta spec.

---

## 4. Estado Desejado (DEPOIS)

### 4.1. Arquivo: `src/viagens/ViagensController.cs` (alteração única — linha 3)

```csharp
public static List<Viagem> Viagens = new();
```

**Mudança:** `private` → `public`. NENHUMA outra linha deste arquivo é alterada.

### 4.2. Arquivo: `src/assentos/AssentosController.cs` (NOVO)

```csharp
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
```

### 4.3. Arquivo: `src/Program.cs` (linhas adicionadas)

```csharp
app.PesquisarViagens();
app.CadastrarVeiculos();
app.ListarVeiculos();
app.ListarVeiculoPorId();
app.MapaAssentos();           // ← NOVA LINHA
app.ReservarAssento();        // ← NOVA LINHA
app.LiberarAssento();         // ← NOVA LINHA
app.BloquearAssento();        // ← NOVA LINHA
app.CadastrarCupons();
```

### 4.4. Detalhamento dos Endpoints

| Método | Rota | Body/Params | Descrição | Sucesso | Erros |
|--------|------|-------------|-----------|---------|-------|
| `GET` | `/api/assentos/viagem/{viagemId}` | `viagemId` na URL | Retorna todos os assentos do veículo da viagem | 200 + array de assentos | 404 (viagem não encontrada) |
| `POST` | `/api/assentos/reservar` | `{ "assentoId": int, "usuarioCpf": "string" }` | Reserva um assento para o CPF informado | 200 + assento | 400 (assentoId ≤ 0 / CPF vazio / status ≠ Disponível), 404 (assento não encontrado) |
| `POST` | `/api/assentos/liberar` | `{ "assentoId": int }` | Libera um assento reservado | 200 + assento | 400 (assentoId ≤ 0 / status ≠ Reservado), 404 (assento não encontrado) |
| `POST` | `/api/assentos/bloquear` | `{ "assentoId": int, "bloquear": bool }` | Bloqueia (true) ou desbloqueia (false) um assento | 200 + assento | 400 (assentoId ≤ 0 / transição inválida), 404 (assento não encontrado) |

### 4.5. Máquina de Estados do Assento

```
                   ┌──────────────┐
                   │  Disponível   │
                   └───┬──────┬───┘
         reservar()    │      │    bloquear(true)
                       ▼      ▼
              ┌────────────┐ ┌──────────────┐
              │  Reservado  │ │ Indisponível  │
              └──────┬─────┘ └──────┬───────┘
        liberar()    │              │ bloquear(false)
                     ▼              ▼
              ┌────────────┐ ┌──────────────┐
              │  Disponível │ │  Disponível   │
              └────────────┘ └──────────────┘
```

**Regras de transição:**
- `Disponível → Reservado`: via `reservar` (requer CPF).
- `Reservado → Disponível`: via `liberar`.
- `Disponível → Indisponível`: via `bloquear(true)`.
- `Indisponível → Disponível`: via `bloquear(false)`.
- `Vendido` não transiciona por estes endpoints — será gerenciado pela Spec 0070 (PassagensController).
- `Reservado → Vendido`: será feito pela Spec 0070 no endpoint `comprar`.

---

## 5. Arquivos Afetados

| Arquivo | Operação | Descrição |
|---------|:--------:|-----------|
| `src/viagens/ViagensController.cs` | **EDITAR** (1 palavra) | `private` → `public` na linha da lista `Viagens` |
| `src/assentos/` | **CRIAR** diretório | Nova pasta para o controller |
| `src/assentos/AssentosController.cs` | **CRIAR** | Controller com 4 endpoints + 3 classes de request |
| `src/Program.cs` | **EDITAR** | Adicionar 4 linhas de registro de rotas |

**NENHUM outro arquivo do projeto é afetado por esta spec.**

---

## 6. Passos de Execução

### Passo 1: Expor a lista de viagens

- Editar `src/viagens/ViagensController.cs`, linha 3.
- Alterar `private static List<Viagem> Viagens` → `public static List<Viagem> Viagens`.
- **Não alterar nenhuma outra linha deste arquivo.**

### Passo 2: Criar o diretório `src/assentos/`

- Criar a pasta `src/assentos/` no projeto.

### Passo 3: Criar o arquivo `AssentosController.cs`

- Criar `src/assentos/AssentosController.cs` com o conteúdo exato da Seção 4.2.
- O arquivo deve conter:
  - Classe estática `AssentosController` com 4 métodos de extensão.
  - 3 classes de request: `ReservaRequest`, `LiberarRequest`, `BloquearRequest`.
  - **NÃO redefinir** a classe `Assento` (já existe em `VeiculosController.cs`).

### Passo 4: Atualizar `Program.cs`

- **Adicionar** 4 linhas após as rotas de veículos:
  ```csharp
  app.MapaAssentos();
  app.ReservarAssento();
  app.LiberarAssento();
  app.BloquearAssento();
  ```
- As novas linhas devem ser inseridas entre `app.ListarVeiculoPorId();` e `app.CadastrarCupons();`.

### Passo 5: Verificar build

- Executar `dotnet build` na solution.
- O build deve retornar **0 erros**.

---

## 7. Critérios de Aceitação

| ID | Critério | Verificação |
|:--:|----------|-------------|
| CA01 | `ViagensController.Viagens` é `public` (não `private`) | Inspeção do código — linha 3 |
| CA02 | O diretório `src/assentos/` EXISTE | `ls -d src/assentos/` |
| CA03 | O arquivo `src/assentos/AssentosController.cs` EXISTE | `-f src/assentos/AssentosController.cs` |
| CA04 | A classe `Assento` NÃO é redefinida em `AssentosController.cs` (usa a de `VeiculosController.cs`) | Grep: apenas 1 definição de `class Assento` no projeto |
| CA05 | `GET /api/assentos/viagem/{viagemId}` retorna os assentos do veículo da viagem (200) ou 404 se viagem não existe | Chamada HTTP |
| CA06 | `POST /api/assentos/reservar` com `assentoId` válido e status "Disponível" altera status para "Reservado" e retorna 200 | Chamada HTTP |
| CA07 | `POST /api/assentos/reservar` com assento já "Reservado" retorna 400 | Chamada HTTP |
| CA08 | `POST /api/assentos/liberar` com assento "Reservado" altera status para "Disponível" e retorna 200 | Chamada HTTP |
| CA09 | `POST /api/assentos/liberar` com assento "Disponível" retorna 400 | Chamada HTTP |
| CA10 | `POST /api/assentos/bloquear` com `bloquear: true` e assento "Disponível" altera status para "Indisponível" | Chamada HTTP |
| CA11 | `POST /api/assentos/bloquear` com `bloquear: false` e assento "Indisponível" altera status para "Disponível" | Chamada HTTP |
| CA12 | `POST /api/assentos/bloquear` com transição inválida retorna 400 | Chamada HTTP |
| CA13 | `Program.cs` CONTÉM chamadas a `MapaAssentos`, `ReservarAssento`, `LiberarAssento`, `BloquearAssento` | Grep positivo (4 ocorrências) |
| CA14 | `dotnet build` na solution compila com 0 erros | Build output: "Compilação com êxito. 0 Erro(s)" |

---

## 8. Riscos e Observações

### 8.1. Risco: `Assento` definido em 2 lugares

- A classe `Assento` foi definida em `VeiculosController.cs` (Spec 0050).
- Se for redefinida em `AssentosController.cs`, ocorrerá erro de compilação (`CS0101` — tipo duplicado).
- **Mitigação:** Esta spec explicitamente NÃO redefine `Assento`. O controller apenas referencia `VeiculosController.Assentos` e o tipo `Assento` já existente.

### 8.2. Observação: Reserva sem expiração automática

- O endpoint `reservar` define o status como "Reservado", mas NÃO implementa expiração automática.
- A expiração de reserva (timer no servidor) será implementada na Spec 0200 (Fase 6).
- Até lá, uma reserva permanece "Reservada" até que alguém chame `liberar` ou `comprar` (Spec 0070).

### 8.3. Observação: Transição "Vendido"

- O status "Vendido" NÃO é alterado por este controller.
- A transição `Reservado → Vendido` será feita pelo `PassagensController` (Spec 0070), endpoint `comprar`.
- Esta separação é intencional: assentos e passagens são domínios distintos.

### 8.4. Observação: `VeiculoId` sem validação

- O endpoint `GET /api/assentos/viagem/{viagemId}` usa `viagem.VeiculoId` para filtrar assentos.
- Se `VeiculoId` for inválido (veículo não cadastrado), a lista retornada será vazia — não é considerado erro.
- A validação de integridade referencial será reforçada na Spec 0190 (Banco de Dados).

### 8.5. Observação: Listas em memória

- Assim como os controllers existentes, os dados são armazenados em `List<T>` em memória.
- Reiniciar a API limpa todos os dados cadastrados.
- Esta limitação é conhecida e documentada no README original.

---

## 9. Referências

| Documento | Caminho | Relevância |
|-----------|---------|------------|
| Arquitetura pivotada | [`arquitetura-pivotagem.md`](../arquitetura-pivotagem.md) | Seção 4.2: modelo Assento; Seção 4.3: endpoints |
| Roadmap da pivotagem | [`roadmap.md`](../roadmap.md) | Definição da Spec 0060 na Fase 1 |
| Visão da pivotagem | [`pivotagem.md`](../pivotagem.md) | Funcionalidades F07, F08, F16 |
| Spec 0050 (Veiculos) | [`specs/0050-modelo-controller-veiculo.md`](0050-modelo-controller-veiculo.md) | Origem da lista `Assentos` e modelo `Assento` |
| Spec 0040 (Viagens) | [`specs/0040-modelo-controller-viagem.md`](0040-modelo-controller-viagem.md) | ViagensController (lista a ser exposta) |
| Program.cs | `src/Program.cs` | Registro de rotas |

---

> **Aguardando aprovação do usuário.**
> **NÃO implementar até que o status seja alterado para "Aprovado".**
