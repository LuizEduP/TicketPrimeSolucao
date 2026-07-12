# Spec 0100 — Criar `AssentoService` + `MapaAssentos.razor`

## Metadados

| Campo | Valor |
|-------|-------|
| **Código** | 0100 |
| **Fase** | 2 — Frontend: Serviços e Componentes |
| **Tipo** | Novo |
| **Prioridade** | 🔴 Alta |
| **Status** | Pendente |
| **Dependências** | Spec 0060 (AssentosController — endpoints `/api/assentos/*`) |
| **Dependentes** | Spec 0140 (Remover Venda.razor) |
| **Estimativa** | 2 horas |
| **Documento de referência** | [`roadmap.md`](../roadmap.md) — Seção 5, Fase 2 |

---

## 1. Objetivo

Criar o serviço e a página de mapa de assentos para o domínio TripPrime. Diferentemente da Spec 0080 (refatoração), esta spec cria componentes **totalmente novos** — não existe equivalente anterior no TicketPrime (`Venda.razor` é setor VIP/Normal, totalmente diferente de um mapa de poltronas). Isso envolve:

1. **Criar** `Models/Assento.cs` (modelo Blazor compatível com a API — 5 campos).
2. **Criar** `Services/AssentoService.cs` (novo serviço com 4 métodos: obterMapa, reservar, liberar, bloquear).
3. **Criar** `Components/Pages/MapaAssentos.razor` (mapa visual interativo de poltronas com clique para seleção).
4. **Criar** `Components/Pages/MapaAssentos.razor.css` (estilos do mapa — cores por status, grid de ônibus com corredor central).
5. **Registrar** `AssentoService` no `Program.cs` (Blazor).

---

## 2. Motivação

### 2.1. Mapa visual interativo

O componente `MapaAssentos.razor` substitui `Venda.razor` (setor VIP/Normal) com um mapa real de poltronas. O usuário vê a planta do veículo, identifica visualmente poltronas disponíveis (verde), reservadas (amarelo), vendidas (vermelho) e indisponíveis (cinza), e reserva com um clique.

### 2.2. Layout de ônibus

O layout reflete um veículo de transporte: assentos à esquerda e à direita com um corredor central. Colunas das extremidades são "Janela", colunas internas são "Corredor".

### 2.3. Componente crítico do fluxo de compra

O mapa de assentos é a interface principal do usuário para escolha de poltrona (F08 do documento de visão). É o ponto de entrada para a reserva que precede a compra de passagem (F09).

---

## 3. Estado Atual (ANTES)

### 3.1. Arquivos inexistentes

- `Models/Assento.cs` — **não existe**.
- `Services/AssentoService.cs` — **não existe**.
- `Components/Pages/MapaAssentos.razor` — **não existe**.
- `Components/Pages/MapaAssentos.razor.css` — **não existe**.

### 3.2. Modelos existentes (Blazor)

- `Models/Viagem.cs` — existe.
- `Models/Veiculo.cs` — existe.
- `Models/Usuario.cs` — existe.
- `Models/Assento.cs` — **não existe**.

### 3.3. Program.cs (Blazor)

```csharp
builder.Services.AddScoped<ViagemService>();
builder.Services.AddScoped<VeiculoService>();
builder.Services.AddScoped<UsuarioService>();
builder.Services.AddSingleton<AuthService>();
```

Nenhum registro de `AssentoService`.

### 3.4. API — Modelo Assento (backend)

Definido em `src/veiculos/VeiculosController.cs`:

```csharp
public class Assento
{
    public int Id { get; set; }
    public int VeiculoId { get; set; }
    public string Numero { get; set; } = "";    // Ex: "1A", "2B", "12C"
    public string Tipo { get; set; } = "";       // "Janela", "Corredor"
    public string Status { get; set; } = "Disponível";  // "Disponível", "Reservado", "Vendido", "Indisponível"
}
```

### 3.5. API — Endpoints de assentos (já registrados)

| Método | Rota | Descrição |
|--------|------|-----------|
| `GET` | `/api/assentos/viagem/{viagemId}` | Retorna todos os assentos do veículo da viagem |
| `POST` | `/api/assentos/reservar` | `{ "assentoId": int, "usuarioCpf": "string" }` — reserva um assento |
| `POST` | `/api/assentos/liberar` | `{ "assentoId": int }` — libera um assento reservado |
| `POST` | `/api/assentos/bloquear` | `{ "assentoId": int, "bloquear": bool }` — bloqueia/desbloqueia assento |

---

## 4. Estado Desejado (DEPOIS)

### 4.1. Arquivo: `Models/Assento.cs` (NOVO)

```csharp
namespace billet_2.Models;

public class Assento
{
    public int Id { get; set; }
    public int VeiculoId { get; set; }
    public string Numero { get; set; } = "";
    public string Tipo { get; set; } = "";
    public string Status { get; set; } = "Disponível";
}
```

### 4.2. Arquivo: `Services/AssentoService.cs` (NOVO)

```csharp
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
```

### 4.3. Arquivo: `Components/Pages/MapaAssentos.razor` (NOVO)

```razor
@page "/viagem/{ViagemId:int}/assentos"
@using billet_2.Models
@using Microsoft.AspNetCore.Components.Forms
@rendermode RenderMode.InteractiveServer
@inject billet_2.Services.AssentoService AssentoService
@inject NavigationManager Navigation

@* --- 1. NAVBAR --- *@
<nav class="navbar-top">
    <a href="/" class="brand-button">BiLleT*</a>
    <div class="nav-actions-right">
        <a href="/poslogin" class="nav-action-button">
            ⚙️ PAINEL
        </a>
    </div>
</nav>

@* --- 2. CONTEÚDO PRINCIPAL --- *@
<main class="mapa-wrapper">
    <div class="mapa-container">
        <h1 class="mapa-title">Mapa de Assentos</h1>

        @if (carregando)
        {
            <div class="loading-message">
                ⏳ Carregando mapa de assentos...
            </div>
        }
        else if (erroCarregamento)
        {
            <div class="alert alert-danger text-center">
                @mensagemErro
            </div>
            <div class="text-center" style="margin-top: 20px;">
                <button @onclick="CarregarMapa" class="btn-retry">Tentar novamente</button>
            </div>
        }
        else if (!assentos.Any())
        {
            <div class="alert alert-warning text-center">
                Nenhum assento encontrado para esta viagem.
            </div>
        }
        else
        {
            @* --- 2.1 LEGENDA --- *@
            <div class="legenda">
                <div class="legenda-item">
                    <span class="seat-icon disponivel"></span> Disponível
                </div>
                <div class="legenda-item">
                    <span class="seat-icon reservado"></span> Reservado
                </div>
                <div class="legenda-item">
                    <span class="seat-icon vendido"></span> Vendido
                </div>
                <div class="legenda-item">
                    <span class="seat-icon indisponivel"></span> Indisponível
                </div>
            </div>

            @* --- 2.2 CONFIRMAÇÃO DE RESERVA --- *@
            @if (exibirSucesso)
            {
                <div class="alert alert-success text-center">
                    ✅ Assento @assentoReservadoNumero reservado com sucesso!
                </div>
            }
            @if (exibirErroReserva)
            {
                <div class="alert alert-danger text-center">
                    @mensagemErroReserva
                </div>
            }

            @* --- 2.3 MODAL CPF --- *@
            @if (mostrarModalCpf)
            {
                <div class="modal-overlay" @onclick="FecharModalCpf">
                    <div class="modal-cpf" @onclick:stopPropagation="true">
                        <h3>Reservar Assento @assentoSelecionado?.Numero</h3>
                        <p>Tipo: @assentoSelecionado?.Tipo</p>
                        <div class="form-group">
                            <label>SEU CPF</label>
                            <input type="text" @bind="cpfDigitado" placeholder="000.000.000-00" class="form-control" maxlength="14" />
                        </div>
                        <div class="modal-actions">
                            <button @onclick="FecharModalCpf" class="btn-cancel">Cancelar</button>
                            <button @onclick="ConfirmarReserva" class="btn-save" disabled="@string.IsNullOrWhiteSpace(cpfDigitado)">
                                Reservar
                            </button>
                        </div>
                    </div>
                </div>
            }

            @* --- 2.4 MAPA DE ASSENTOS (GRID ÔNIBUS) --- *@
            <div class="onibus-container">
                @* Frente do ônibus *@
                <div class="onibus-frente">
                    <span>🚌 MOTORISTA</span>
                </div>

                <div class="onibus-grid">
                    @foreach (var row in fileiras)
                    {
                        <div class="fileira">
                            <span class="fileira-numero">@row.Key</span>
                            <div class="assentos-fileira">
                                @* Lado esquerdo *@
                                <div class="lado-esquerdo">
                                    @foreach (var assento in row.Value.Where(a => a.ColunaIndice < colunas / 2.0))
                                    {
                                        <button class="seat-btn @GetClasseStatus(assento)"
                                                disabled="@(!assento.PodeReservar)"
                                                @onclick='() => SelecionarAssento(assento)'
                                                title="@assento.Numero - @assento.Tipo - @assento.Status">
                                            @assento.Numero
                                        </button>
                                    }
                                </div>
                                @* Corredor central *@
                                <div class="corredor"></div>
                                @* Lado direito *@
                                <div class="lado-direito">
                                    @foreach (var assento in row.Value.Where(a => a.ColunaIndice >= colunas / 2.0))
                                    {
                                        <button class="seat-btn @GetClasseStatus(assento)"
                                                disabled="@(!assento.PodeReservar)"
                                                @onclick='() => SelecionarAssento(assento)'
                                                title="@assento.Numero - @assento.Tipo - @assento.Status">
                                            @assento.Numero
                                        </button>
                                    }
                                </div>
                            </div>
                        </div>
                    }
                </div>

                @* Traseira do ônibus *@
                <div class="onibus-traseira">
                    <span>TRASEIRA</span>
                </div>
            </div>
        }
    </div>
</main>

<footer class="site-footer">
    <p>&copy; 2026 Billet - Todos os direitos reservados.</p>
</footer>

@code {
    [Parameter]
    public int ViagemId { get; set; }

    private List<Assento> assentos = new();
    private Dictionary<int, List<AssentoExibicao>> fileiras = new();
    private int colunas;
    private bool carregando = true;
    private bool erroCarregamento = false;
    private string mensagemErro = "";

    private bool mostrarModalCpf = false;
    private AssentoExibicao? assentoSelecionado;
    private string cpfDigitado = "";
    private bool exibirSucesso = false;
    private bool exibirErroReserva = false;
    private string mensagemErroReserva = "";
    private string assentoReservadoNumero = "";

    protected override async Task OnInitializedAsync()
    {
        await CarregarMapa();
    }

    private async Task CarregarMapa()
    {
        carregando = true;
        erroCarregamento = false;
        exibirSucesso = false;
        exibirErroReserva = false;
        mostrarModalCpf = false;
        StateHasChanged();

        try
        {
            var resultado = await AssentoService.ObterMapaAssentosAsync(ViagemId);
            if (resultado == null)
            {
                erroCarregamento = true;
                mensagemErro = "Viagem não encontrada.";
                carregando = false;
                return;
            }

            assentos = resultado;
            OrganizarFileiras();
            carregando = false;
        }
        catch (Exception ex)
        {
            erroCarregamento = true;
            mensagemErro = $"Erro ao carregar o mapa: {ex.Message}";
            carregando = false;
        }
    }

    private void OrganizarFileiras()
    {
        if (!assentos.Any())
        {
            fileiras = new();
            colunas = 0;
            return;
        }

        // Determina o número de colunas (letras únicas)
        colunas = assentos.Select(a => a.Numero.Last()).Distinct().Count();

        // Agrupa por fileira (número)
        var agrupados = new Dictionary<int, List<AssentoExibicao>>();

        foreach (var assento in assentos.OrderBy(a => a.Numero))
        {
            // Extrai número da fileira e letra da coluna
            var numeroStr = new string(assento.Numero.Where(char.IsDigit).ToArray());
            var letra = assento.Numero.Last();

            if (!int.TryParse(numeroStr, out int fileira))
                continue;

            if (!agrupados.ContainsKey(fileira))
                agrupados[fileira] = new();

            agrupados[fileira].Add(new AssentoExibicao
            {
                Id = assento.Id,
                VeiculoId = assento.VeiculoId,
                Numero = assento.Numero,
                Tipo = assento.Tipo,
                Status = assento.Status,
                ColunaIndice = letra - 'A',
                PodeReservar = assento.Status == "Disponível"
            });
        }

        fileiras = agrupados.OrderBy(f => f.Key)
                           .ToDictionary(f => f.Key, f => f.Value);
    }

    private void SelecionarAssento(AssentoExibicao assento)
    {
        assentoSelecionado = assento;
        cpfDigitado = "";
        mostrarModalCpf = true;
    }

    private void FecharModalCpf()
    {
        mostrarModalCpf = false;
        assentoSelecionado = null;
        cpfDigitado = "";
    }

    private async Task ConfirmarReserva()
    {
        if (assentoSelecionado == null || string.IsNullOrWhiteSpace(cpfDigitado))
            return;

        mostrarModalCpf = false;
        var erro = await AssentoService.ReservarAssentoAsync(assentoSelecionado.Id, cpfDigitado.Trim());

        if (erro == null)
        {
            assentoReservadoNumero = assentoSelecionado.Numero;
            exibirSucesso = true;
            exibirErroReserva = false;
            await CarregarMapa();
        }
        else
        {
            exibirErroReserva = true;
            exibirSucesso = false;
            mensagemErroReserva = erro;
        }
    }

    private string GetClasseStatus(AssentoExibicao assento)
    {
        return assento.Status switch
        {
            "Disponível" => "seat-disponivel",
            "Reservado" => "seat-reservado",
            "Vendido" => "seat-vendido",
            "Indisponível" => "seat-indisponivel",
            _ => "seat-indisponivel"
        };
    }

    private class AssentoExibicao : Assento
    {
        public int ColunaIndice { get; set; }
        public bool PodeReservar { get; set; }
    }
}
```

### 4.4. Arquivo: `Components/Pages/MapaAssentos.razor.css` (NOVO)

Estilos customizados para o mapa de assentos com layout de ônibus. Cores por status e grid com corredor central.

### 4.5. Arquivo: `Program.cs` (Blazor) — alteração

```csharp
builder.Services.AddScoped<ViagemService>();
builder.Services.AddScoped<VeiculoService>();
builder.Services.AddScoped<AssentoService>();   // ← NOVA LINHA
builder.Services.AddScoped<UsuarioService>();
builder.Services.AddSingleton<AuthService>();
```

---

## 5. Arquivos Afetados

| Arquivo | Operação | Descrição |
|---------|:--------:|-----------|
| `Models/Assento.cs` | **CRIAR** | Novo modelo Blazor com 5 campos |
| `Services/AssentoService.cs` | **CRIAR** | Novo serviço com 4 métodos (obterMapa, reservar, liberar, bloquear) |
| `Components/Pages/MapaAssentos.razor` | **CRIAR** | Mapa visual interativo com rota `/viagem/{id}/assentos` |
| `Components/Pages/MapaAssentos.razor.css` | **CRIAR** | CSS para layout de ônibus + cores por status |
| `Program.cs` (Blazor) | **EDITAR** | Adicionar `builder.Services.AddScoped<AssentoService>()` |

**NENHUM outro arquivo do projeto é afetado por esta spec.**

---

## 6. Passos de Execução

### Passo 1: Criar modelo Blazor `Assento.cs`

- Criar `Models/Assento.cs` com o conteúdo da Seção 4.1 (5 campos, compatível com a API).

### Passo 2: Criar serviço `AssentoService.cs`

- Criar `Services/AssentoService.cs` com o conteúdo da Seção 4.2 (4 métodos).

### Passo 3: Criar página `MapaAssentos.razor` + CSS

- Criar `Components/Pages/MapaAssentos.razor` com o conteúdo da Seção 4.3.
- Criar `Components/Pages/MapaAssentos.razor.css` com os estilos de grid de ônibus + cores por status.

### Passo 4: Atualizar `Program.cs` (Blazor)

- Adicionar `builder.Services.AddScoped<AssentoService>()` após o registro de `VeiculoService`.

### Passo 5: Verificar build

- Executar `dotnet build` no projeto Blazor.
- O build deve retornar **0 erros NOVOS** (os 9 erros existentes de `EventoService`/`Evento` em Home, Poslogin, Meusingressos, Venda permanecem — são das specs 0110-0140).

---

## 7. Critérios de Aceitação

| ID | Critério | Verificação |
|:--:|----------|-------------|
| CA01 | `Models/Assento.cs` EXISTE com 5 campos (`Id`, `VeiculoId`, `Numero`, `Tipo`, `Status`) | Inspeção |
| CA02 | `Services/AssentoService.cs` EXISTE com 4 métodos | Inspeção |
| CA03 | `MapaAssentos.razor` EXISTE com rota `@page "/viagem/{ViagemId:int}/assentos"` | Inspeção |
| CA04 | `MapaAssentos.razor.css` EXISTE | Inspeção |
| CA05 | Página exibe legenda com 4 status (Disponível, Reservado, Vendido, Indisponível) | Inspeção visual |
| CA06 | Assentos são exibidos em grid com corredor central (layout de ônibus) | Inspeção visual |
| CA07 | Assentos Disponíveis são clicáveis e abrem modal de CPF | Inspeção |
| CA08 | Assentos NÃO Disponíveis estão desabilitados (disabled) | Inspeção |
| CA09 | Ao confirmar reserva, chama `POST /api/assentos/reservar` e recarrega o mapa | Inspeção |
| CA10 | `AssentoService` injetado no `MapaAssentos.razor` | Inspeção |
| CA11 | `Program.cs` (Blazor) registra `AssentoService` | Inspeção |
| CA12 | `dotnet build` no projeto Blazor NÃO introduz novos erros | Build output |

---

## 8. Riscos e Observações

### 8.1. Layout responsivo

O grid de ônibus usa CSS Grid para posicionar assentos à esquerda e à direita do corredor. O layout é baseado no número de colunas do veículo (extraído das letras dos números de assento).

### 8.2. Ordenação das fileiras

As fileiras são ordenadas numericamente a partir do número extraído do campo `Numero` (ex: "1A" → fileira 1, "12C" → fileira 12). A ordenação usa `OrderBy` numérico.

### 8.3. Divisão esquerda/direita

A lógica divide as colunas no meio: colunas com índice < `colunas/2.0` vão para a esquerda, as demais para a direita. Para veículos com número ímpar de colunas (ex: 3), a coluna do meio vai para o lado direito.

### 8.4. Sem proteção de rota

Assim como os demais componentes, esta página não tem proteção de rota. A autenticação será implementada na Spec 0210 (JWT).

### 8.5. Classe interna `AssentoExibicao`

Para não poluir o modelo `Assento`, a página define uma classe interna `AssentoExibicao` que estende `Assento` com campos específicos de UI (`ColunaIndice`, `PodeReservar`).

---

## 9. Referências

| Documento | Caminho | Relevância |
|-----------|---------|------------|
| Spec 0060 (AssentosController) | [`specs/0060-modelo-controller-assento.md`](0060-modelo-controller-assento.md) | API endpoints e modelo Assento |
| Arquitetura pivotada | [`arquitetura-pivotagem.md`](../arquitetura-pivotagem.md) | Seção 5.3: AssentoService; Seção 5.1: MapaAssentos |
| Roadmap | [`roadmap.md`](../roadmap.md) | Definição da Spec 0100 na Fase 2 |
| CriarVeiculo.razor | `Components/Pages/CriarVeiculo.razor` | Referência de estilo e estrutura |
| Program.cs (Blazor) | `billet_2/billet_2/Program.cs` | Registro de serviços |

---

> **Aguardando aprovação do usuário.**
> **NÃO implementar até que o status seja alterado para "Aprovado".**
