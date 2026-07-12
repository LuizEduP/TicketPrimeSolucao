# Spec 0110 — Criar `PassagemService` + Refatorar `Meusingressos.razor` → `MinhasPassagens.razor`

## Metadados

| Campo | Valor |
|-------|-------|
| **Código** | 0110 |
| **Fase** | 2 — Frontend: Serviços e Componentes |
| **Tipo** | Misto (Novo + Refatorar) |
| **Prioridade** | 🔴 Alta |
| **Status** | Pendente |
| **Dependências** | Spec 0070 (PassagensController — endpoints `/api/passagens/*`) |
| **Dependentes** | Spec 0130 (Poslogin.razor — menu "Minhas Passagens") |
| **Estimativa** | 1,5 horas |
| **Documento de referência** | [`roadmap.md`](../roadmap.md) — Seção 5, Fase 2 |

---

## 1. Objetivo

Criar o serviço de passagens e refatorar a página "Meus Ingressos" para o domínio TripPrime. Esta spec é **mista**: o serviço e modelo são totalmente novos, enquanto a página é uma refatoração com ~70% de reaproveitamento de estrutura visual. Envolve:

1. **Criar** `Models/Passagem.cs` — modelo Blazor com 9 campos (compatível com a API).
2. **Criar** `Services/PassagemService.cs` — novo serviço com 4 métodos (`ListarTodas`, `ListarPorUsuario`, `Comprar`, `Cancelar`).
3. **Refatorar** `Meusingressos.razor` → `MinhasPassagens.razor` — rename + adapt in place (~70% de reaproveitamento do layout/estilo).
4. **Refatorar** `Meusingressos.razor.css` → `MinhasPassagens.razor.css` — rename com ajustes de classes CSS.
5. **Remover** `Meusingressos.razor` e `Meusingressos.razor.css` — arquivos antigos.
6. **Registrar** `PassagemService` no `Program.cs` (Blazor).

---

## 2. Motivação

### 2.1. Lista real de passagens (não carrinho ilusório)

O `Meusingressos.razor` atual é um **carrinho de compras local** que recebe itens via parâmetro de URL (`?Comprar=...`) e os mantém em uma lista em memória. Não há persistência — ao recarregar a página, o carrinho some.

A versão pivotada consulta a API (`GET /api/passagens/usuario/{cpf}`) e exibe passagens **reais**, persistidas no backend. O usuário digita seu CPF e vê o histórico real de compras.

### 2.2. Cancelamento real

O botão "Remover" do carrinho vira "Cancelar", que chama `POST /api/passagens/cancelar/{id}` — o assento volta a ficar Disponível e a passagem muda para status "Cancelada".

### 2.3. Resolve 2 erros de build

`Meusingressos.razor` é um dos 4 arquivos que quebram o build (referencia `EventoService` e `Evento`). Ao substituí-lo, **2 dos 9 erros de compilação são eliminados**.

---

## 3. Estado Atual (ANTES)

### 3.1. Arquivos existentes (a refatorar/remover)

- `Components/Pages/Meusingressos.razor` — 135 linhas. Usa `EventoService`, modelo `Evento`, carrinho local com `ItemCarrinho`, badges de setor VIP/Normal, checkout ilusório.
- `Components/Pages/Meusingressos.razor.css` — 205 linhas. CSS com classes `.tickets-*`, `.badge-sector`, `.vip-gold`, `.normal-white`.

### 3.2. Arquivos inexistentes (a criar)

- `Models/Passagem.cs` — **não existe**.
- `Services/PassagemService.cs` — **não existe**.
- `Components/Pages/MinhasPassagens.razor` — **não existe**.
- `Components/Pages/MinhasPassagens.razor.css` — **não existe**.

### 3.3. Program.cs (Blazor)

```csharp
builder.Services.AddScoped<ViagemService>();
builder.Services.AddScoped<VeiculoService>();
builder.Services.AddScoped<AssentoService>();
builder.Services.AddScoped<UsuarioService>();
builder.Services.AddSingleton<AuthService>();
```

Nenhum registro de `PassagemService`.

### 3.4. API — Modelo Passagem (backend)

Definido em `src/passagens/PassagensController.cs`:

```csharp
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
```

### 3.5. API — Endpoints de passagens (já registrados)

| Método | Rota | Descrição |
|--------|------|-----------|
| `GET` | `/api/passagens/listar` | Lista todas as passagens |
| `GET` | `/api/passagens/usuario/{cpf}` | Lista passagens de um CPF |
| `POST` | `/api/passagens/comprar` | Finaliza compra de passagem |
| `POST` | `/api/passagens/cancelar/{id}` | Cancela passagem |

---

## 4. Estado Desejado (DEPOIS)

### 4.1. Arquivo: `Models/Passagem.cs` (NOVO)

```csharp
namespace billet_2.Models;

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
```

### 4.2. Arquivo: `Services/PassagemService.cs` (NOVO)

```csharp
using System.Net.Http.Json;
using billet_2.Models;

namespace billet_2.Services;

public class PassagemService
{
    private readonly HttpClient _http;

    public PassagemService(HttpClient http)
    {
        _http = http;
    }

    public async Task<List<Passagem>?> ListarTodasAsync()
    {
        return await _http.GetFromJsonAsync<List<Passagem>>("api/passagens/listar");
    }

    public async Task<List<Passagem>?> ListarPorUsuarioAsync(string cpf)
    {
        return await _http.GetFromJsonAsync<List<Passagem>>($"api/passagens/usuario/{cpf}");
    }

    public async Task<string?> ComprarPassagemAsync(int viagemId, int assentoId, string cpf, string? cupom = null)
    {
        try
        {
            var payload = new
            {
                ViagemId = viagemId,
                AssentoId = assentoId,
                UsuarioCpf = cpf,
                CupomUtilizado = cupom
            };
            var response = await _http.PostAsJsonAsync("api/passagens/comprar", payload);

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

    public async Task<string?> CancelarPassagemAsync(int id)
    {
        try
        {
            var response = await _http.PostAsync($"api/passagens/cancelar/{id}", null);

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

### 4.3. Arquivo: `Components/Pages/MinhasPassagens.razor` (NOVO, baseado no Meusingressos)

```razor
@page "/minhaspassagens"
@using billet_2.Models
@rendermode RenderMode.InteractiveServer
@inject NavigationManager Navigation
@inject billet_2.Services.PassagemService PassagemService
@inject billet_2.Services.ViagemService ViagemService

@* --- 1. NAVBAR --- *@
<nav class="navbar-top">
    <a href="/poslogin" class="brand-button">
        BiLleT*
    </a>
</nav>

@* --- 2. CONTEÚDO DAS PASSAGENS --- *@
<main class="tickets-wrapper">
    <div class="tickets-container">
        <h1 class="tickets-title">Minhas Passagens</h1>

        @* Campo de CPF *@
        <div class="cpf-search">
            <input type="text" @bind="cpfBusca" placeholder="Digite seu CPF (000.000.000-00)" class="form-control" maxlength="14" />
            <button @onclick="BuscarPassagens" class="btn-save" disabled="@string.IsNullOrWhiteSpace(cpfBusca)">
                Buscar
            </button>
        </div>

        @if (estaBuscando)
        {
            <div class="text-center py-5">
                <p style="color: #888;">Buscando passagens...</p>
            </div>
        }
        else if (exibirErro)
        {
            <div class="alert alert-danger text-center">
                @mensagemErro
            </div>
        }
        else if (!cpfBuscado)
        {
            <div class="text-center py-5">
                <h2 style="color: #666;">Informe seu CPF para consultar suas passagens.</h2>
            </div>
        }
        else if (!passagens.Any())
        {
            <div class="text-center py-5">
                <h2 style="color: #666;">Nenhuma passagem encontrada.</h2>
                <button @onclick='() => Navigation.NavigateTo("/")' class="btn-add-more mt-3">Ver Viagens Disponíveis</button>
            </div>
        }
        else
        {
            @foreach (var passagem in passagens)
            {
                <div class="ticket-item">
                    @* Ícone da passagem *@
                    <div class="ticket-img-box">
                        <div class="ticket-icon">
                            🎫
                        </div>
                    </div>

                    @* Detalhes da Passagem *@
                    <div class="ticket-info">
                        <h3>@detalhesViagens[passagem.ViagemId].Origem → @detalhesViagens[passagem.ViagemId].Destino</h3>
                        <p class="ticket-meta">
                            Assento @detalhesAssentos[passagem.AssentoId] 
                            • @detalhesViagens[passagem.ViagemId].DataPartida.ToString("dd/MM/yyyy HH:mm")
                        </p>
                        <div class="ticket-badge-row">
                            <span class="badge-status @GetClasseStatus(passagem.Status)">
                                @passagem.Status.ToUpper()
                            </span>
                            <span class="ticket-date">Comprado em @passagem.DataCompra.ToString("dd/MM/yyyy")</span>
                        </div>
                        @if (!string.IsNullOrEmpty(passagem.CupomUtilizado))
                        {
                            <p class="cupom-info">🎟️ Cupom: @passagem.CupomUtilizado</p>
                        }
                    </div>

                    @* Preço e Ações *@
                    <div class="ticket-action-price">
                        <span class="ticket-price">R$ @passagem.PrecoPago.ToString("N2")</span>
                        @if (passagem.Status == "Ativa")
                        {
                            <button class="btn-remove" @onclick="() => CancelarPassagem(passagem.Id)">
                                Cancelar
                            </button>
                        }
                    </div>
                </div>
            }
        }
    </div>
</main>

<footer class="site-footer">
    <p>&copy; 2026 Billet - Todos os direitos reservados.</p>
</footer>

@code {
    private string cpfBusca = "";
    private bool cpfBuscado = false;
    private bool estaBuscando = false;
    private bool exibirErro = false;
    private string mensagemErro = "";

    private List<Passagem> passagens = new();
    private Dictionary<int, Viagem> detalhesViagens = new();
    private Dictionary<int, string> detalhesAssentos = new();

    private async Task BuscarPassagens()
    {
        if (string.IsNullOrWhiteSpace(cpfBusca))
            return;

        estaBuscando = true;
        exibirErro = false;
        StateHasChanged();

        var resultado = await PassagemService.ListarPorUsuarioAsync(cpfBusca.Trim());

        if (resultado == null)
        {
            exibirErro = true;
            mensagemErro = "Erro ao consultar passagens.";
            estaBuscando = false;
            return;
        }

        passagens = resultado;
        cpfBuscado = true;
        estaBuscando = false;

        // Carrega detalhes das viagens e assentos
        await CarregarDetalhes();
    }

    private async Task CarregarDetalhes()
    {
        detalhesViagens.Clear();
        detalhesAssentos.Clear();

        foreach (var passagem in passagens)
        {
            if (!detalhesViagens.ContainsKey(passagem.ViagemId))
            {
                var viagem = await ViagemService.BuscarPorIdAsync(passagem.ViagemId);
                if (viagem != null)
                    detalhesViagens[passagem.ViagemId] = viagem;
            }

            if (!detalhesAssentos.ContainsKey(passagem.AssentoId))
            {
                detalhesAssentos[passagem.AssentoId] = $"#{passagem.AssentoId}";
            }
        }

        StateHasChanged();
    }

    private async Task CancelarPassagem(int id)
    {
        var erro = await PassagemService.CancelarPassagemAsync(id);

        if (erro == null)
        {
            // Recarrega a lista
            await BuscarPassagens();
        }
        else
        {
            exibirErro = true;
            mensagemErro = erro;
        }
    }

    private string GetClasseStatus(string status)
    {
        return status switch
        {
            "Ativa" => "status-ativa",
            "Cancelada" => "status-cancelada",
            "Utilizada" => "status-utilizada",
            _ => "status-cancelada"
        };
    }
}
```

### 4.4. Arquivo: `Components/Pages/MinhasPassagens.razor.css` (NOVO, baseado no Meusingressos.razor.css)

**Conteúdo baseado** no `Meusingressos.razor.css` com as seguintes alterações:
- Remove `.vip-gold` e `.normal-white` (badges de setor)
- Adiciona `.status-ativa`, `.status-cancelada`, `.status-utilizada` (badges de status)
- Adiciona `.cpf-search` (barra de busca de CPF)
- Adiciona `.ticket-icon` (ícone placeholder no lugar da imagem)
- Adiciona `.cupom-info` (exibição do cupom utilizado)
- Mantém todas as demais classes CSS intactas

### 4.5. Arquivos REMOVIDOS

- `Components/Pages/Meusingressos.razor` — **DELETAR**
- `Components/Pages/Meusingressos.razor.css` — **DELETAR**

### 4.6. Arquivo: `Program.cs` (Blazor) — alteração

```csharp
builder.Services.AddScoped<ViagemService>();
builder.Services.AddScoped<VeiculoService>();
builder.Services.AddScoped<AssentoService>();
builder.Services.AddScoped<PassagemService>();   // ← NOVA LINHA
builder.Services.AddScoped<UsuarioService>();
builder.Services.AddSingleton<AuthService>();
```

---

## 5. Arquivos Afetados

| Arquivo | Operação | Descrição |
|---------|:--------:|-----------|
| `Models/Passagem.cs` | **CRIAR** | Novo modelo Blazor com 9 campos |
| `Services/PassagemService.cs` | **CRIAR** | Novo serviço com 4 métodos |
| `Components/Pages/MinhasPassagens.razor` | **CRIAR** | Baseado no Meusingressos (~70% reaproveitado, adaptado para passagens) |
| `Components/Pages/MinhasPassagens.razor.css` | **CRIAR** | Baseado no Meusingressos.razor.css (~80% reaproveitado) |
| `Components/Pages/Meusingressos.razor` | **DELETAR** | Substituído pelo MinhasPassagens |
| `Components/Pages/Meusingressos.razor.css` | **DELETAR** | Substituído pelo MinhasPassagens.razor.css |
| `Program.cs` (Blazor) | **EDITAR** | Adicionar `builder.Services.AddScoped<PassagemService>()` |

**NENHUM outro arquivo do projeto é afetado por esta spec.**

---

## 6. Passos de Execução

### Passo 1: Criar modelo Blazor `Passagem.cs`

- Criar `Models/Passagem.cs` com 9 campos (idênticos ao modelo da API).

### Passo 2: Criar serviço `PassagemService.cs`

- Criar `Services/PassagemService.cs` com 4 métodos: `ListarTodasAsync`, `ListarPorUsuarioAsync`, `ComprarPassagemAsync`, `CancelarPassagemAsync`.

### Passo 3: Criar `MinhasPassagens.razor` + CSS

- Criar `Components/Pages/MinhasPassagens.razor` com base na estrutura do `Meusingressos.razor`, adaptando:
  - `@page "/minhaspassagens"` (era `/meusingressos`)
  - Trocar `EventoService` → `PassagemService` + `ViagemService`
  - Remover lógica de `ItemCarrinho` / `Comprar` parameter
  - Adicionar campo de busca de CPF
  - Listar passagens com informações: origem→destino, assento, data, status, preço
  - Badge de status (Ativa/Cancelada/Utilizada) no lugar do setor VIP/Normal
  - Botão "Cancelar" apenas para passagens Ativas
- Criar `Components/Pages/MinhasPassagens.razor.css` baseado no `Meusingressos.razor.css` com ajustes.

### Passo 4: Remover arquivos antigos

- Deletar `Components/Pages/Meusingressos.razor`
- Deletar `Components/Pages/Meusingressos.razor.css`

### Passo 5: Atualizar `Program.cs` (Blazor)

- Adicionar `builder.Services.AddScoped<PassagemService>()` após `AssentoService`.

### Passo 6: Verificar build

- Executar `dotnet build` no projeto Blazor.
- O build deve retornar **7 erros** (eram 9 — 2 de Meusingressos removidos). Os restantes são de Home, Poslogin e Venda (specs 0120-0140).

---

## 7. Critérios de Aceitação

| ID | Critério | Verificação |
|:--:|----------|-------------|
| CA01 | `Models/Passagem.cs` EXISTE com 9 campos | Inspeção |
| CA02 | `Services/PassagemService.cs` EXISTE com 4 métodos | Inspeção |
| CA03 | `MinhasPassagens.razor` EXISTE com rota `@page "/minhaspassagens"` | Inspeção |
| CA04 | `MinhasPassagens.razor.css` EXISTE | Inspeção |
| CA05 | `Meusingressos.razor` NÃO existe mais | Inspeção |
| CA06 | `Meusingressos.razor.css` NÃO existe mais | Inspeção |
| CA07 | Página exibe campo de busca de CPF | Inspeção visual |
| CA08 | Página lista passagens com: origem→destino, assento, data, status, preço | Inspeção visual |
| CA09 | Badge de status substitui badge de setor VIP/Normal | Inspeção visual |
| CA10 | Botão "Cancelar" aparece apenas para passagens Ativas | Inspeção visual |
| CA11 | `PassagemService` injetado em `MinhasPassagens.razor` | Inspeção |
| CA12 | `Program.cs` (Blazor) registra `PassagemService` | Inspeção |
| CA13 | `dotnet build` retorna no máximo **7 erros** (2 a menos que antes) | Build output |

---

## 8. Riscos e Observações

### 8.1. Comparação Antes/Depois

| Aspecto | Meusingressos (ANTES) | MinhasPassagens (DEPOIS) |
|---------|----------------------|-------------------------|
| Rota | `/meusingressos` | `/minhaspassagens` |
| Serviço injetado | `EventoService` | `PassagemService` + `ViagemService` |
| Modelo | `Evento` + `ItemCarrinho` | `Passagem` + `Viagem` (para detalhes) |
| Fonte de dados | Carrinho local (parâmetro URL `?Comprar=`) | API (`GET /api/passagens/usuario/{cpf}`) |
| Entrada do usuário | Parâmetro de URL automático | Campo de CPF |
| Badge | Setor VIP/Normal | Status Ativa/Cancelada/Utilizada |
| Ação principal | "Remover" do carrinho | "Cancelar" passagem (chama API) |
| Resumo de compra | Subtotal/Total (cálculo local) | *(removido — não se aplica)* |

### 8.2. Reaproveitamento de CSS

- ~80% do CSS do `Meusingressos.razor.css` é reaproveitado.
- Classes mantidas: `.navbar-top`, `.brand-button`, `.tickets-wrapper`, `.tickets-container`, `.tickets-title`, `.ticket-item`, `.ticket-img-box`, `.ticket-info`, `.ticket-meta`, `.ticket-badge-row`, `.ticket-date`, `.ticket-action-price`, `.ticket-price`, `.btn-remove`, `.btn-add-more`, `.site-footer`.
- Classes removidas: `.badge-sector`, `.vip-gold`, `.normal-white`, `.checkout-summary`, `.summary-line`, `.action-buttons`, `.btn-checkout`.
- Classes novas: `.cpf-search`, `.status-ativa`, `.status-cancelada`, `.status-utilizada`, `.ticket-icon`, `.cupom-info`.

### 8.3. Detalhes de viagens e assentos

A página carrega detalhes de cada viagem (origem, destino, data) via `ViagemService.BuscarPorIdAsync()`. Como os dados estão em memória, isso funciona. No futuro (Spec 0190 — BD), um JOIN eliminaria essas chamadas extras.

### 8.4. Número do assento

Atualmente, o `GET /api/passagens/usuario/{cpf}` não retorna o número do assento (apenas `AssentoId`). A página exibe `#ID` como placeholder. Para obter o número real, seria necessário um endpoint adicional ou um JOIN. Isso é uma limitação conhecida (#6 do documento de arquitetura).

### 8.5. Sem proteção de rota

Assim como os demais componentes, esta página não tem proteção. Qualquer CPF digitado mostra as passagens daquele CPF. A autenticação será implementada na Spec 0210 (JWT).

---

## 9. Referências

| Documento | Caminho | Relevância |
|-----------|---------|------------|
| Spec 0070 (PassagensController) | [`specs/0070-modelo-controller-passagem.md`](0070-modelo-controller-passagem.md) | API endpoints e modelo Passagem |
| Arquitetura pivotada | [`arquitetura-pivotagem.md`](../arquitetura-pivotagem.md) | Seção 5.3: PassagemService; Seção 5.1: MinhasPassagens |
| Roadmap | [`roadmap.md`](../roadmap.md) | Definição da Spec 0110 na Fase 2 |
| Meusingressos.razor | `Components/Pages/Meusingressos.razor` | Arquivo base a ser refatorado |
| Meusingressos.razor.css | `Components/Pages/Meusingressos.razor.css` | CSS base a ser refatorado |
| Program.cs (Blazor) | `billet_2/billet_2/Program.cs` | Registro de serviços |

---

> **Aguardando aprovação do usuário.**
> **NÃO implementar até que o status seja alterado para "Aprovado".**
