# Spec 0070 — Criar Modelo `Passagem` + `PassagensController` + Registrar Rotas

## Metadados

| Campo | Valor |
|-------|-------|
| **Código** | 0070 |
| **Fase** | 1 — Backend: Novos Controllers |
| **Tipo** | Novo |
| **Prioridade** | 🔴 Alta |
| **Status** | Pendente |
| **Dependências** | Spec 0060 (AssentosController — lista `Assentos` + transições de estado), Spec 0040 (ViagensController — lista `Viagens` pública) |
| **Dependentes** | Spec 0110 (PassagemService + MinhasPassagens.razor) |
| **Estimativa** | 1,5 horas |
| **Documento de referência** | [`roadmap.md`](../roadmap.md) — Seção 4, Fase 1 |

---

## 1. Objetivo

Criar o controller de **Passagens** para o domínio TripPrime. Isso envolve:

1. **Criar** o modelo `Passagem` com 9 campos conforme especificado na arquitetura pivotada.
2. **Criar** a classe estática `PassagensController` com 4 endpoints.
3. **Calcular o preço no backend** a partir do `PrecoBase` da viagem, aplicando desconto de cupom quando informado — o frontend NÃO envia preço.
4. **Integrar** com `VeiculosController.Assentos` para alterar status de assentos durante compra e cancelamento.
5. **Registrar** as novas rotas em `Program.cs`.

---

## 2. Motivação

### 2.1. Passagem como conceito central

A passagem é o equivalente ao "ingresso" no TicketPrime original. Representa a compra confirmada de um assento em uma viagem. É o registro financeiro e operacional do sistema.

### 2.2. Fluxo completo: Reserva → Compra → Cancelamento

O ciclo de vida de uma passagem envolve 3 controllers:

```
VeiculosController          AssentosController         PassagensController
(Cadastrar veículo)  ──▶   (Reservar assento)   ──▶   (Comprar passagem)
                            (Liberar assento)   ◀──   (Cancelar passagem)
```

Esta spec implementa os 2 últimos passos: **comprar** (converte reserva em venda) e **cancelar** (reverte venda e libera assento).

### 2.3. Integridade de estado do assento

O endpoint `comprar` exige que o assento esteja "Reservado" e o transiciona para "Vendido". O endpoint `cancelar` exige que a passagem esteja "Ativa" e reverte o assento para "Disponível". Estas regras garantem que um assento não seja vendido duas vezes.

### 2.4. Segurança: Preço calculado no backend

O `PrecoPago` **NÃO** é recebido do frontend. O backend calcula o valor:

1. Busca a viagem pelo `ViagemId` → obtém `PrecoBase`.
2. Se um cupom for informado, busca o cupom pelo código → obtém `PercentualDesconto`.
3. `PrecoPago = PrecoBase × (1 − PercentualDesconto / 100)`.

**Motivo:** Se o preço viesse do frontend, um atacante poderia alterar o valor no navegador e pagar R\$ 0,01 por qualquer passagem. Com o cálculo no backend, o preço é determinado exclusivamente pelo servidor.

---

## 3. Estado Atual (ANTES)

### 3.1. Máquina de estados do assento (pós-Spec 0060)

```
Disponível ──reservar()──▶ Reservado ──liberar()──▶ Disponível
Disponível ──bloquear(true)──▶ Indisponível ──bloquear(false)──▶ Disponível
```

O estado "Vendido" **não é alcançável** atualmente. Nenhum endpoint faz `Reservado → Vendido`.

### 3.2. Listas públicas disponíveis

| Lista | Local | Visibilidade |
|-------|-------|:-----------:|
| `ViagensController.Viagens` | `src/viagens/ViagensController.cs` | `public` (Spec 0060) |
| `VeiculosController.Assentos` | `src/veiculos/VeiculosController.cs` | `public` (Spec 0050) |

### 3.3. Lista que precisa ser exposta

| Lista | Local | Visibilidade atual | Visibilidade necessária |
|-------|-------|:------------------:|:-----------------------:|
| `CuponsController.Cupons` | `src/cupons/CuponsController.cs` | `private` | `public` |

Para validar cupons no cálculo de desconto, a lista `Cupons` precisa ser tornada pública. Alteração de 1 palavra: `private` → `public`.

### 3.4. Diretório `src/passagens/`

**Não existe.** Este diretório será criado nesta spec.

### 3.4. Arquivo `src/Program.cs` (trecho relevante)

```csharp
app.MapaAssentos();
app.ReservarAssento();
app.LiberarAssento();
app.BloquearAssento();
app.CadastrarCupons();
```

Nenhuma rota de passagens está registrada.

---

## 4. Estado Desejado (DEPOIS)

### 4.1. Arquivo: `src/passagens/PassagensController.cs` (NOVO)

```csharp
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
```

### 4.2. Arquivo: `src/Program.cs` (linhas adicionadas)

```csharp
app.MapaAssentos();
app.ReservarAssento();
app.LiberarAssento();
app.BloquearAssento();
app.ListarPassagens();           // ← NOVA LINHA
app.ListarPassagensPorUsuario(); // ← NOVA LINHA
app.ComprarPassagem();           // ← NOVA LINHA
app.CancelarPassagem();          // ← NOVA LINHA
app.CadastrarCupons();
```

### 4.3. Detalhamento do Modelo `Passagem`

| Campo | Tipo | Restrição | Descrição |
|-------|------|-----------|-----------|
| `Id` | `int` | Auto-incremento | Identificador único |
| `ViagemId` | `int` | > 0 | Referência à viagem |
| `AssentoId` | `int` | > 0 | Referência ao assento |
| `UsuarioCpf` | `string` | Obrigatório (não vazio) | CPF do comprador |
| `PrecoPago` | `float` | >= 0 | Valor pago pela passagem |
| `CupomUtilizado` | `string?` | Opcional | Código do cupom (se aplicado) |
| `Status` | `string` | "Ativa" \| "Cancelada" \| "Utilizada" | Estado atual da passagem |
| `DataCompra` | `DateTime` | Default: `DateTime.Now` | Data/hora da compra |
| `DataExpiracaoReserva` | `DateTime?` | Opcional | Prazo de expiração (Spec 0200) |

### 4.4. Detalhamento dos Endpoints

| Método | Rota | Body/Params | Descrição | Sucesso | Erros |
|--------|------|-------------|-----------|---------|-------|
| `GET` | `/api/passagens/listar` | — | Lista todas as passagens | 200 + array | — |
| `GET` | `/api/passagens/usuario/{cpf}` | `cpf` na URL | Passagens de um usuário | 200 + array (vazio se nenhuma) | 400 (CPF vazio) |
| `POST` | `/api/passagens/comprar` | `{ "viagemId": int, "assentoId": int, "usuarioCpf": "string", "cupomUtilizado": "string?" }` | Finaliza compra: calcula preço (PrecoBase − desconto cupom), cria passagem + muda assento → Vendido | 200 + passagem | 400 (validação/cupom inválido) / 404 (viagem/assento) |
| `POST` | `/api/passagens/cancelar/{id}` | `id` na URL | Cancela passagem + libera assento | 200 + passagem | 400 (id ≤ 0 / status ≠ Ativa) / 404 |

### 4.5. Máquina de Estados do Assento (ATUALIZADA)

```
                   ┌──────────────┐
                   │  Disponível   │◀──────────────────────────┐
                   └───┬──────┬───┘                           │
         reservar()    │      │    bloquear(true)              │
                       ▼      ▼                               │
              ┌────────────┐ ┌──────────────┐                 │
              │  Reservado  │ │ Indisponível  │                │
              └──────┬─────┘ └──────┬───────┘                 │
        liberar()    │              │ bloquear(false)           │
                     ▼              ▼                          │
              ┌────────────┐ ┌──────────────┐                 │
              │  Disponível │ │  Disponível   │                │
              └────────────┘ └──────────────┘                 │
                                                               │
              ┌────────────┐                                   │
              │  Reservado  │──── comprar() ──▶ ┌──────────┐  │
              └────────────┘                    │  Vendido  │  │
                                                └────┬─────┘  │
                                                     │        │
                        cancelar() (libera assento) ─┘        │
                                                     │        │
                                                     ▼        │
                                              ┌──────────┐   │
                                              │ Disponível│───┘
                                              └──────────┘
```

**Novas transições (esta spec):**
- `Reservado → Vendido`: via `comprar` (`POST /api/passagens/comprar`).
- `Vendido → Disponível`: via `cancelar` (`POST /api/passagens/cancelar/{id}`).

---

## 5. Arquivos Afetados

| Arquivo | Operação | Descrição |
|---------|:--------:|-----------|
| `src/cupons/CuponsController.cs` | **EDITAR** (1 palavra) | `private` → `public` na lista `Cupons` |
| `src/passagens/` | **CRIAR** diretório | Nova pasta para o controller |
| `src/passagens/PassagensController.cs` | **CRIAR** | Controller com modelo Passagem, CompraRequest e 4 endpoints |
| `src/Program.cs` | **EDITAR** | Adicionar 4 linhas de registro de rotas |

**NENHUM outro arquivo do projeto é afetado por esta spec.**

---

## 6. Passos de Execução

### Passo 1: Expor a lista de cupons

- Editar `src/cupons/CuponsController.cs`, linha 2.
- Alterar `private static List<Cupons> Cupons` → `public static List<Cupons> Cupons`.
- **Não alterar nenhuma outra linha deste arquivo.**

### Passo 2: Criar o diretório `src/passagens/`

- Criar a pasta `src/passagens/` no projeto.

### Passo 3: Criar o arquivo `PassagensController.cs`

- Criar `src/passagens/PassagensController.cs` com o conteúdo exato da Seção 4.1.
- O arquivo deve conter:
  - Classe estática `PassagensController` com lista privada `Passagens` e contador `idAtual`.
  - 4 métodos de extensão: `ListarPassagens`, `ListarPassagensPorUsuario`, `ComprarPassagem`, `CancelarPassagem`.
  - Classe `Passagem` com 9 campos.
  - Classe `CompraRequest` com 4 campos (sem `PrecoPago`).

### Passo 4: Atualizar `Program.cs`

- **Adicionar** 4 linhas após as rotas de assentos:
  ```csharp
  app.ListarPassagens();
  app.ListarPassagensPorUsuario();
  app.ComprarPassagem();
  app.CancelarPassagem();
  ```
- As novas linhas devem ser inseridas entre `app.BloquearAssento();` e `app.CadastrarCupons();`.

### Passo 5: Verificar build

- Executar `dotnet build` na solution.
- O build deve retornar **0 erros**.

---

## 7. Critérios de Aceitação

| ID | Critério | Verificação |
|:--:|----------|-------------|
| CA01 | O diretório `src/passagens/` EXISTE | `ls -d src/passagens/` |
| CA02 | O arquivo `src/passagens/PassagensController.cs` EXISTE | `-f src/passagens/PassagensController.cs` |
| CA03 | A classe `Passagem` possui exatamente 9 campos: `Id`, `ViagemId`, `AssentoId`, `UsuarioCpf`, `PrecoPago`, `CupomUtilizado`, `Status`, `DataCompra`, `DataExpiracaoReserva` | Inspeção do código |
| CA04 | `GET /api/passagens/listar` retorna a lista de passagens (200) | Chamada HTTP |
| CA05 | `GET /api/passagens/usuario/{cpf}` retorna passagens do CPF ou lista vazia | Chamada HTTP com CPF existente e inexistente |
| CA06 | `GET /api/passagens/usuario/` (CPF vazio) retorna 400 | Chamada HTTP |
| CA07 | `POST /api/passagens/comprar` com assento "Reservado": calcula `PrecoPago = Viagem.PrecoBase`, cria passagem com status "Ativa", muda assento para "Vendido", retorna 200 | Chamada HTTP |
| CA08 | `POST /api/passagens/comprar` com assento "Disponível" (não reservado) retorna 400 | Chamada HTTP |
| CA09 | `POST /api/passagens/comprar` com `ViagemId` inexistente retorna 404 | Chamada HTTP |
| CA10 | `POST /api/passagens/comprar` com cupom válido aplica desconto no `PrecoPago` | Chamada HTTP com cupom de 10% → PrecoPago = PrecoBase × 0.9 |
| CA11 | `POST /api/passagens/comprar` com cupom inexistente retorna 400 | Chamada HTTP |
| CA12 | `POST /api/passagens/comprar` com CPF vazio retorna 400 | Chamada HTTP |
| CA13 | `POST /api/passagens/cancelar/{id}` com passagem "Ativa": muda status para "Cancelada", muda assento para "Disponível", retorna 200 | Chamada HTTP |
| CA14 | `POST /api/passagens/cancelar/{id}` com passagem já "Cancelada" retorna 400 | Chamada HTTP |
| CA15 | `POST /api/passagens/cancelar/{id}` com id inexistente retorna 404 | Chamada HTTP |
| CA16 | `Program.cs` CONTÉM chamadas a `ListarPassagens`, `ListarPassagensPorUsuario`, `ComprarPassagem`, `CancelarPassagem` | Grep positivo (4 ocorrências) |
| CA17 | `CuponsController.Cupons` é `public` (não `private`) | Inspeção do código |
| CA18 | `dotnet build` na solution compila com 0 erros | Build output: "Compilação com êxito. 0 Erro(s)" |

---

## 8. Riscos e Observações

### 8.1. Observação: Validações de integridade

- `ViagemId` é validado (viagem deve existir para obter `PrecoBase`). Retorna 404 se não encontrada.
- `CupomUtilizado` é validado (cupom deve existir se informado). Retorna 400 se não encontrado.
- `UsuarioCpf` não é validado contra `UsuariosController.Usuarios` (lista permanece privada).
- `AssentoId` é validado quanto à existência e status, mas não quanto ao `VeiculoId` da viagem.
- Validações adicionais de integridade referencial serão implementadas na Spec 0190 (Banco de Dados).

### 8.2. Observação: Preço calculado no backend ✅

- O `PrecoPago` **NÃO** é recebido do frontend. O campo foi removido do `CompraRequest`.
- O backend calcula: `PrecoPago = Viagem.PrecoBase × (1 − Cupom.PercentualDesconto / 100)`.
- Se o cupom não for informado, `PrecoPago = Viagem.PrecoBase` (sem desconto).
- Se o desconto for maior que 100%, `PrecoPago` é fixado em 0 (nunca negativo).
- A Spec 0220 (Fase 6) ainda poderá expandir esta lógica com validações adicionais (validade do cupom, valor mínimo, etc.), mas a **base segura** já está implementada aqui.

### 8.3. Observação: Cancelamento libera assento

- Ao cancelar, o assento volta para "Disponível", permitindo nova reserva/compra.
- Se o assento não for encontrado (ex: lista reiniciada), o cancelamento da passagem **ainda ocorre** — o bloco `if (assento != null)` é defensivo.
- Não há regra de reembolso implementada (F12 do documento de visão — prioridade Média).

### 8.4. Observação: `DataExpiracaoReserva` não utilizado

- O campo `DataExpiracaoReserva` é definido como `null` na compra.
- Será populado quando a Spec 0200 (Expiração de reserva) for implementada na Fase 6.

### 8.5. Observação: Listas em memória

- Assim como os controllers existentes, os dados são armazenados em `List<T>` em memória.
- Reiniciar a API limpa todos os dados cadastrados.
- Esta limitação é conhecida e documentada no README original.

---

## 9. Referências

| Documento | Caminho | Relevância |
|-----------|---------|------------|
| Arquitetura pivotada | [`arquitetura-pivotagem.md`](../arquitetura-pivotagem.md) | Seção 4.2: modelo Passagem; Seção 4.3: endpoints |
| Roadmap da pivotagem | [`roadmap.md`](../roadmap.md) | Definição da Spec 0070 na Fase 1 |
| Visão da pivotagem | [`pivotagem.md`](../pivotagem.md) | Funcionalidades F09, F11, F12 |
| Spec 0060 (Assentos) | [`specs/0060-modelo-controller-assento.md`](0060-modelo-controller-assento.md) | Máquina de estados do assento |
| Spec 0040 (Viagens) | [`specs/0040-modelo-controller-viagem.md`](0040-modelo-controller-viagem.md) | Padrão de FK sem validação |
| Program.cs | `src/Program.cs` | Registro de rotas |

---

> **Aguardando aprovação do usuário.**
> **NÃO implementar até que o status seja alterado para "Aprovado".**
