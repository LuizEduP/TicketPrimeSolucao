# Spec 0080 — Refatorar `EventoService` → `ViagemService` + `Criarevento.razor` → `CriarViagem.razor`

## Metadados

| Campo | Valor |
|-------|-------|
| **Código** | 0080 |
| **Fase** | 2 — Frontend: Serviços e Componentes |
| **Tipo** | Refatoração (rename + adapt in place) |
| **Prioridade** | 🔴 Alta |
| **Status** | Pendente |
| **Dependências** | Spec 0040 (ViagensController — endpoints `/api/viagens/*`) |
| **Dependentes** | Spec 0120 (Home.razor), Spec 0130 (Poslogin.razor) |
| **Estimativa** | 1,5 horas |
| **Documento de referência** | [`roadmap.md`](../roadmap.md) — Seção 5, Fase 2 |

---

## 1. Objetivo

Transformar o serviço e a página de criação de eventos (TicketPrime) no serviço e página de criação de viagens (TripPrime). Isso envolve:

1. **Criar** `Models/Viagem.cs` (modelo Blazor compatível com a API).
2. **Substituir** `Services/EventoService.cs` → `Services/ViagemService.cs` (rename + adaptar métodos e URLs).
3. **Substituir** `Components/Pages/Criarevento.razor` + `.css` → `CriarViagem.razor` + `.css` (rename + adaptar campos do formulário).
4. **Atualizar** `Program.cs` (Blazor) para registrar `ViagemService` no lugar de `EventoService`.

---

## 2. Motivação

### 2.1. Pivotagem de domínio no frontend

O serviço `EventoService` e a página `Criarevento.razor` ainda referenciam modelos e endpoints do TicketPrime (Evento, `/api/eventos/*`). A Fase 1 já implementou os endpoints do TripPrime (`/api/viagens/*`). O frontend precisa ser adaptado para consumir esses novos endpoints.

### 2.2. Aproveitamento de código (~70%)

A estrutura do serviço (HTTP calls) e o layout visual da página (CSS) são amplamente reutilizáveis. As mudanças são:
- **Serviço:** renomear classe, método e URLs (~30% de alteração).
- **Página:** renomear campos do formulário, labels e títulos (~50% de alteração).
- **CSS:** 0% de alteração (apenas renomear arquivo).

### 2.3. Independência entre specs do frontend

Esta spec altera APENAS os 4 arquivos listados. Componentes como `Home.razor`, `Poslogin.razor` e `Meusingressos.razor` ainda referenciam `EventoService` e o modelo `Evento` — essas dependências serão resolvidas nas specs subsequentes (0120, 0130, 0110).

---

## 3. Estado Atual (ANTES)

### 3.1. Arquivo: `Models/Evento.cs`

```csharp
namespace billet_2.Models;

public class Evento
{
    public int Id { get; set; }
    public string Nome { get; set; } = "";
    public string Descricao { get; set; } = "";
    public string Local { get; set; } = "";
    public DateTime Data { get; set; }
    public int QuantidadeIngressos { get; set; }
    public float ValorIngresso { get; set; }
    public string? FotoUrl { get; set; }
}
```

Modelo do domínio TicketPrime. Precisa ser substituído por `Viagem`.

### 3.2. Arquivo: `Services/EventoService.cs`

- Classe `EventoService` com 3 métodos.
- URLs apontam para `/api/eventos/listar`, `/api/eventos/listar/{id}`, `/api/eventos/cadastrar`.
- Modelo usado: `Evento`.
- **Não possui** método de pesquisa (novo endpoint `/api/viagens/pesquisar`).

### 3.3. Arquivo: `Components/Pages/Criarevento.razor`

- Rota: `@page "/criarevento"`
- Injeta `EventoService`.
- Modelo do formulário: `Evento`.
- Campos: Nome (título), Data e Hora, Quantidade de Ingressos, Local, Descricao, ValorIngresso, FotoUrl.
- Título da página: "Novo Evento".
- Botão: "Publicar Evento".
- Mensagem de sucesso: "🎉 Evento publicado com sucesso!".

### 3.4. Arquivo: `Program.cs` (Blazor)

```csharp
builder.Services.AddScoped<EventoService>();
```

---

## 4. Estado Desejado (DEPOIS)

### 4.1. Arquivo: `Models/Viagem.cs` (NOVO — substitui `Models/Evento.cs`)

```csharp
namespace billet_2.Models;

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

**Mapeamento campo a campo (Evento → Viagem):**

| Evento (ANTES) | Viagem (DEPOIS) | Observação |
|----------------|-----------------|------------|
| `Id` | `Id` | Mantido |
| `Nome` | `Origem` | Semântica alterada |
| *(não existia)* | `Destino` | **Novo** |
| `Data` | `DataPartida` | Renomeado |
| *(não existia)* | `DataChegada` | **Novo** |
| *(não existia)* | `DataVolta` | **Novo** (opcional) |
| `Descricao` | `Descricao` | Mantido |
| `Local` | *(removido)* | **Removido** |
| `QuantidadeIngressos` | *(removido)* | **Removido** |
| *(não existia)* | `VeiculoId` | **Novo** |
| `ValorIngresso` | `PrecoBase` | Renomeado |
| `FotoUrl` | `FotoUrl` | Mantido |

### 4.2. Arquivo: `Services/ViagemService.cs` (NOVO — substitui `Services/EventoService.cs`)

```csharp
using System.Net.Http.Json;
using billet_2.Models;

namespace billet_2.Services;

public class ViagemService
{
    private readonly HttpClient _http;

    public ViagemService(HttpClient http)
    {
        _http = http;
    }

    public async Task<List<Viagem>?> ListarViagensAsync()
    {
        return await _http.GetFromJsonAsync<List<Viagem>>("api/viagens/listar");
    }

    public async Task<Viagem?> BuscarPorIdAsync(int id)
    {
        return await _http.GetFromJsonAsync<Viagem>($"api/viagens/listar/{id}");
    }

    public async Task<List<Viagem>?> PesquisarViagensAsync(string? origem, string? destino, DateTime? data)
    {
        var query = $"api/viagens/pesquisar?origem={origem}&destino={destino}&data={data:yyyy-MM-dd}";
        return await _http.GetFromJsonAsync<List<Viagem>>(query);
    }

    public async Task<string?> CriarViagemAsync(Viagem novaViagem)
    {
        try
        {
            var response = await _http.PostAsJsonAsync("api/viagens/cadastrar", novaViagem);

            if (response.IsSuccessStatusCode)
            {
                return null; // Sucesso!
            }
            else
            {
                var erro = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Erro na API: {erro}");
                return erro; // Retorna a mensagem de erro da API
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exceção ao cadastrar: {ex.Message}");
            return "Erro de conexão com o servidor.";
        }
    }
}
```

### 4.3. Arquivo: `Components/Pages/CriarViagem.razor` (NOVO — substitui `Criarevento.razor`)

```razor
@page "/criarviagem"
@using billet_2.Models
@using Microsoft.AspNetCore.Components.Forms
@rendermode RenderMode.InteractiveServer
@inject billet_2.Services.ViagemService ViagemService
@inject NavigationManager Navigation

@* --- 1. NAVBAR --- *@
<nav class="navbar-top">
    <a href="/poslogin" class="brand-button">BiLleT*</a>
</nav>

@* --- 2. CONTEÚDO PRINCIPAL --- *@
<main class="create-wrapper">
    <div class="create-container">
        <h1 class="create-title">Nova Viagem</h1>

        @if (exibirSucesso)
        {
            <div class="alert alert-success text-center">
                🎉 Viagem criada com sucesso!
            </div>
        }

        @if (exibirErro)
        {
            <div class="alert alert-danger text-center">
                @mensagemErro
            </div>
        }

        <EditForm Model="novaViagem" OnValidSubmit="SalvarViagem" class="create-form" FormName="CadastroViagem">
            <DataAnnotationsValidator />

            @* Origem e Destino *@
            <div class="form-row">
                <div class="form-group">
                    <label>ORIGEM</label>
                    <InputText @bind-Value="novaViagem.Origem" placeholder="Ex: São Paulo - SP" class="form-control" />
                </div>
                <div class="form-group">
                    <label>DESTINO</label>
                    <InputText @bind-Value="novaViagem.Destino" placeholder="Ex: Rio de Janeiro - RJ" class="form-control" />
                </div>
            </div>

            @* Datas: Partida, Chegada, Volta *@
            <div class="form-row">
                <div class="form-group">
                    <label>DATA DE PARTIDA</label>
                    <InputDate @bind-Value="novaViagem.DataPartida" Type="InputDateType.DateTimeLocal" class="form-control" />
                </div>
                <div class="form-group">
                    <label>DATA DE CHEGADA</label>
                    <InputDate @bind-Value="novaViagem.DataChegada" Type="InputDateType.DateTimeLocal" class="form-control" />
                </div>
            </div>

            <div class="form-group">
                <label>DATA DE VOLTA (opcional — apenas para ida e volta)</label>
                <InputDate @bind-Value="novaViagem.DataVolta" Type="InputDateType.DateTimeLocal" class="form-control" />
            </div>

            @* Veículo e Preço *@
            <div class="form-row">
                <div class="form-group">
                    <label>ID DO VEÍCULO</label>
                    <InputNumber @bind-Value="novaViagem.VeiculoId" class="form-control" placeholder="1" />
                </div>
                <div class="form-group">
                    <label>PREÇO BASE (R$)</label>
                    <InputNumber @bind-Value="novaViagem.PrecoBase" placeholder="0,00" class="form-control" />
                </div>
            </div>

            @* Descrição *@
            <div class="form-group">
                <label>DESCRIÇÃO DA VIAGEM</label>
                <InputTextArea @bind-Value="novaViagem.Descricao" rows="4" placeholder="Descreva os detalhes da viagem..." class="form-control" />
            </div>

            @* URL da imagem *@
            <div class="form-group">
                <label>URL DA IMAGEM (BANNER)</label>
                <InputText @bind-Value="novaViagem.FotoUrl" placeholder="Ex: banner-rio.webp" class="form-control" />
            </div>

            @* Botões de Ação *@
            <div class="form-actions">
                <button type="button" @onclick='() => Navigation.NavigateTo("/poslogin")' class="btn-cancel">Cancelar</button>
                <button type="submit" class="btn-save" disabled="@estaProcessando">
                    @(estaProcessando ? "Salvando..." : "Criar Viagem")
                </button>
            </div>
        </EditForm>
    </div>
</main>

<footer class="site-footer">
    <p>&copy; 2026 Billet - Todos os direitos reservados.</p>
</footer>

@code {
    private Viagem novaViagem = new();
    private bool estaProcessando = false;
    private bool exibirSucesso = false;
    private bool exibirErro = false;
    private string mensagemErro = "";

    private async Task SalvarViagem()
    {
        estaProcessando = true;

        var response = await ViagemService.CriarViagemAsync(novaViagem);

        if (string.IsNullOrEmpty(response))
        {
            exibirErro = false;
            exibirSucesso = true;
            StateHasChanged();
            await Task.Delay(2000);
            Navigation.NavigateTo("/poslogin");
        }
        else
        {
            exibirErro = true;
            mensagemErro = response;
            estaProcessando = false;
        }
    }
}
```

### 4.4. Arquivo: `Components/Pages/CriarViagem.razor.css` (RENOMEADO de `Criarevento.razor.css`)

**Conteúdo idêntico.** Nenhuma alteração no CSS — apenas renomear o arquivo.

### 4.5. Mapeamento de Campos do Formulário (ANTES → DEPOIS)

| Antes (Criarevento) | Depois (CriarViagem) | Campo no Modelo |
|----------------------|----------------------|-----------------|
| "TÍTULO DO EVENTO" → `Nome` | "ORIGEM" → `Origem` | string |
| *(não existia)* | "DESTINO" → `Destino` | string |
| "DATA E HORA" → `Data` | "DATA DE PARTIDA" → `DataPartida` | DateTime |
| *(não existia)* | "DATA DE CHEGADA" → `DataChegada` | DateTime |
| *(não existia)* | "DATA DE VOLTA (opcional)" → `DataVolta` | DateTime? |
| "LOCAL" → `Local` | *(removido)* | — |
| "QUANTIDADE DE INGRESSOS" → `QuantidadeIngressos` | "ID DO VEÍCULO" → `VeiculoId` | int |
| "VALOR DO INGRESSO (R$)" → `ValorIngresso` | "PREÇO BASE (R$)" → `PrecoBase` | float |
| "SOBRE O EVENTO" → `Descricao` | "DESCRIÇÃO DA VIAGEM" → `Descricao` | string |
| "URL DA IMAGEM" → `FotoUrl` | "URL DA IMAGEM (BANNER)" → `FotoUrl` | string? |
| "Novo Evento" (título) | "Nova Viagem" (título) | — |
| "Publicar Evento" (botão) | "Criar Viagem" (botão) | — |

### 4.6. Arquivo: `Program.cs` (Blazor) — alteração

```csharp
// Antes:
builder.Services.AddScoped<EventoService>();

// Depois:
builder.Services.AddScoped<ViagemService>();
```

---

## 5. Arquivos Afetados

| Arquivo | Operação | Descrição |
|---------|:--------:|-----------|
| `Models/Evento.cs` | **EXCLUIR** | Substituído por `Models/Viagem.cs` |
| `Models/Viagem.cs` | **CRIAR** | Novo modelo Blazor com 10 campos |
| `Services/EventoService.cs` | **EXCLUIR** | Substituído por `Services/ViagemService.cs` |
| `Services/ViagemService.cs` | **CRIAR** | Novo serviço com 4 métodos (listar, buscarPorId, pesquisar, criar) |
| `Components/Pages/Criarevento.razor` | **EXCLUIR** | Substituído por `CriarViagem.razor` |
| `Components/Pages/Criarevento.razor.css` | **EXCLUIR** | Substituído por `CriarViagem.razor.css` (idêntico) |
| `Components/Pages/CriarViagem.razor` | **CRIAR** | Novo formulário com campos do modelo Viagem |
| `Components/Pages/CriarViagem.razor.css` | **CRIAR** | CSS idêntico ao do arquivo antigo |
| `Program.cs` (Blazor) | **EDITAR** | `EventoService` → `ViagemService` (1 linha) |

---

## 6. Passos de Execução

### Passo 1: Criar modelo Blazor `Viagem.cs`

- Criar `Models/Viagem.cs` com o conteúdo da Seção 4.1.
- Excluir `Models/Evento.cs`.

### Passo 2: Criar serviço `ViagemService.cs`

- Criar `Services/ViagemService.cs` com o conteúdo da Seção 4.2 (4 métodos).
- Excluir `Services/EventoService.cs`.

### Passo 3: Criar página `CriarViagem.razor` + CSS

- Criar `Components/Pages/CriarViagem.razor` com o conteúdo da Seção 4.3.
- Criar `Components/Pages/CriarViagem.razor.css` com o conteúdo idêntico ao CSS antigo.
- Excluir `Components/Pages/Criarevento.razor` e `Criarevento.razor.css`.

### Passo 4: Atualizar `Program.cs` (Blazor)

- Alterar `builder.Services.AddScoped<EventoService>()` → `builder.Services.AddScoped<ViagemService>()`.

### Passo 5: Verificar build

- Executar `dotnet build` no projeto Blazor.
- O build deve retornar **0 erros** (avisos de páginas que referenciam `EventoService` são esperados — serão resolvidos nas specs 0110-0150).

---

## 7. Critérios de Aceitação

| ID | Critério | Verificação |
|:--:|----------|-------------|
| CA01 | `Models/Evento.cs` NÃO existe mais | `! -f Models/Evento.cs` |
| CA02 | `Models/Viagem.cs` EXISTE com 10 campos | Inspeção |
| CA03 | `Services/EventoService.cs` NÃO existe mais | `! -f Services/EventoService.cs` |
| CA04 | `Services/ViagemService.cs` EXISTE com 4 métodos | Inspeção |
| CA05 | `Criarevento.razor` e `.css` NÃO existem mais | `! -f Components/Pages/Criarevento.razor*` |
| CA06 | `CriarViagem.razor` EXISTE com rota `@page "/criarviagem"` | Inspeção |
| CA07 | `CriarViagem.razor.css` EXISTE (idêntico ao CSS antigo) | Inspeção |
| CA08 | `ViagemService` injetado no `CriarViagem.razor` (não `EventoService`) | Inspeção |
| CA09 | Formulário contém campos: Origem, Destino, DataPartida, DataChegada, DataVolta, VeiculoId, PrecoBase, Descricao, FotoUrl | Inspeção |
| CA10 | `Program.cs` (Blazor) registra `ViagemService` (não `EventoService`) | Inspeção |
| CA11 | `dotnet build` no projeto Blazor compila com 0 erros | Build output |

---

## 8. Riscos e Observações

### 8.1. Risco: Quebra de páginas que usam `EventoService`

- `Home.razor`, `Poslogin.razor`, `Meusingressos.razor` e `Venda.razor` ainda referenciam `EventoService` e o modelo `Evento`.
- Após esta spec, essas páginas **NÃO compilarão** (erro de tipo/resolução de serviço).
- **Mitigação:** Isto é esperado e planejado. As specs 0080-0150 resolverão cada dependência sequencialmente:
  - Spec 0110: `Meusingressos.razor` → `MinhasPassagens.razor`
  - Spec 0120: `Home.razor`
  - Spec 0130: `Poslogin.razor`
  - Spec 0140: Remove `Venda.razor`
- **O build do Blazor pode falhar temporariamente** até que todas as specs da Fase 2 e 3 sejam concluídas.

### 8.2. Observação: `VeiculoId` como campo numérico

- O formulário usa `InputNumber` para `VeiculoId`. Não há dropdown de veículos ainda.
- A Spec 0090 (CriarVeiculo.razor + VeiculoService) trará o cadastro de veículos.
- Um dropdown para selecionar veículo pode ser adicionado na Spec 0220 ou como melhoria futura.

### 8.3. Observação: CSS mantido intacto

- O CSS (`CriarViagem.razor.css`) é idêntico ao original — apenas renomeado.
- As classes CSS (`create-wrapper`, `create-form`, etc.) permanecem as mesmas.

---

## 9. Referências

| Documento | Caminho | Relevância |
|-----------|---------|------------|
| Spec 0040 (ViagensController) | [`specs/0040-modelo-controller-viagem.md`](0040-modelo-controller-viagem.md) | Modelo Viagem da API e endpoints |
| Roadmap | [`roadmap.md`](../roadmap.md) | Definição da Spec 0080 na Fase 2 |
| EventoService.cs | `Services/EventoService.cs` | Código a ser substituído |
| Criarevento.razor | `Components/Pages/Criarevento.razor` | Página a ser substituída |
| Program.cs (Blazor) | `billet_2/billet_2/Program.cs` | Registro de serviços |

---

> **Aguardando aprovação do usuário.**
> **NÃO implementar até que o status seja alterado para "Aprovado".**
