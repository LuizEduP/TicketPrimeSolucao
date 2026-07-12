# Spec 0090 — Criar `VeiculoService` + `CriarVeiculo.razor`

## Metadados

| Campo | Valor |
|-------|-------|
| **Código** | 0090 |
| **Fase** | 2 — Frontend: Serviços e Componentes |
| **Tipo** | Novo |
| **Prioridade** | 🔴 Alta |
| **Status** | Pendente |
| **Dependências** | Spec 0050 (VeiculosController — endpoints `/api/veiculos/*`) |
| **Dependentes** | Spec 0130 (Poslogin.razor — menu admin com link "Criar Veículo") |
| **Estimativa** | 1 hora |
| **Documento de referência** | [`roadmap.md`](../roadmap.md) — Seção 5, Fase 2 |

---

## 1. Objetivo

Criar o serviço e a página de cadastro de veículos para o domínio TripPrime. Diferentemente da Spec 0080 (que foi uma refatoração), esta spec cria componentes **totalmente novos** — não existe equivalente anterior no TicketPrime. Isso envolve:

1. **Criar** `Models/Veiculo.cs` (modelo Blazor compatível com a API).
2. **Criar** `Services/VeiculoService.cs` (novo serviço com 3 métodos: listar, buscarPorId, criar).
3. **Criar** `Components/Pages/CriarVeiculo.razor` (formulário de cadastro de veículo).
4. **Criar** `Components/Pages/CriarVeiculo.razor.css` (estilos do formulário — reutiliza classes CSS do `CriarViagem.razor.css`).
5. **Registrar** `VeiculoService` no `Program.cs` (Blazor).

---

## 2. Motivação

### 2.1. Domínio TripPrime

O conceito de "Veículo" é novo no TripPrime. Cada viagem (`Viagem`) referencia um veículo (`VeiculoId`). O administrador precisa de uma interface para cadastrar veículos, definindo o layout de assentos (Linhas × Colunas).

### 2.2. Componente totalmente novo

Diferentemente de `CriarViagem.razor` (que substituiu `Criarevento.razor`), o `CriarVeiculo.razor` não tem equivalente anterior. Ele segue o mesmo estilo visual do `CriarViagem.razor` para manter consistência, mas todos os campos e a lógica são novos.

### 2.3. Capacidade calculada automaticamente

O campo `Capacidade` é derivado no backend (`Linhas × Colunas`). O formulário no frontend **não exibe** o campo `Capacidade` — apenas `Linhas` e `Colunas`. A capacidade é exibida na listagem de veículos (via `VeiculoService.ListarVeiculosAsync()`).

---

## 3. Estado Atual (ANTES)

### 3.1. Arquivos inexistentes

- `Models/Veiculo.cs` — **não existe**.
- `Services/VeiculoService.cs` — **não existe**.
- `Components/Pages/CriarVeiculo.razor` — **não existe**.
- `Components/Pages/CriarVeiculo.razor.css` — **não existe**.

### 3.2. Program.cs (Blazor)

```csharp
builder.Services.AddScoped<ViagemService>();
builder.Services.AddScoped<UsuarioService>();
builder.Services.AddSingleton<AuthService>();
```

Nenhum registro de `VeiculoService`.

---

## 4. Estado Desejado (DEPOIS)

### 4.1. Arquivo: `Models/Veiculo.cs` (NOVO)

```csharp
namespace billet_2.Models;

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
```

### 4.2. Arquivo: `Services/VeiculoService.cs` (NOVO)

```csharp
using System.Net.Http.Json;
using billet_2.Models;

namespace billet_2.Services;

public class VeiculoService
{
    private readonly HttpClient _http;

    public VeiculoService(HttpClient http)
    {
        _http = http;
    }

    public async Task<List<Veiculo>?> ListarVeiculosAsync()
    {
        return await _http.GetFromJsonAsync<List<Veiculo>>("api/veiculos/listar");
    }

    public async Task<Veiculo?> BuscarPorIdAsync(int id)
    {
        return await _http.GetFromJsonAsync<Veiculo>($"api/veiculos/listar/{id}");
    }

    public async Task<string?> CriarVeiculoAsync(Veiculo novoVeiculo)
    {
        try
        {
            var response = await _http.PostAsJsonAsync("api/veiculos/cadastrar", novoVeiculo);

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

### 4.3. Arquivo: `Components/Pages/CriarVeiculo.razor` (NOVO)

```razor
@page "/criarveiculo"
@using billet_2.Models
@using Microsoft.AspNetCore.Components.Forms
@rendermode RenderMode.InteractiveServer
@inject billet_2.Services.VeiculoService VeiculoService
@inject NavigationManager Navigation

@* --- 1. NAVBAR --- *@
<nav class="navbar-top">
    <a href="/poslogin" class="brand-button">BiLleT*</a>
</nav>

@* --- 2. CONTEÚDO PRINCIPAL --- *@
<main class="create-wrapper">
    <div class="create-container">
        <h1 class="create-title">Novo Veículo</h1>

        @if (exibirSucesso)
        {
            <div class="alert alert-success text-center">
                🎉 Veículo cadastrado com sucesso!
            </div>
        }

        @if (exibirErro)
        {
            <div class="alert alert-danger text-center">
                @mensagemErro
            </div>
        }

        <EditForm Model="novoVeiculo" OnValidSubmit="SalvarVeiculo" class="create-form" FormName="CadastroVeiculo">
            <DataAnnotationsValidator />

            @* Modelo e Placa *@
            <div class="form-row">
                <div class="form-group">
                    <label>MODELO</label>
                    <InputText @bind-Value="novoVeiculo.Modelo" placeholder="Ex: Mercedes-Benz O500" class="form-control" />
                </div>
                <div class="form-group">
                    <label>PLACA</label>
                    <InputText @bind-Value="novoVeiculo.Placa" placeholder="Ex: ABC-1234" class="form-control" />
                </div>
            </div>

            @* Tipo do veículo *@
            <div class="form-group">
                <label>TIPO DO VEÍCULO</label>
                <InputSelect @bind-Value="novoVeiculo.Tipo" class="form-control">
                    <option value="">Selecione um tipo...</option>
                    <option value="Convencional">Convencional</option>
                    <option value="Executivo">Executivo</option>
                    <option value="Leito">Leito</option>
                    <option value="Micro-ônibus">Micro-ônibus</option>
                    <option value="Van">Van</option>
                </InputSelect>
            </div>

            @* Layout de assentos: Linhas e Colunas *@
            <div class="form-row">
                <div class="form-group">
                    <label>NÚMERO DE FILEIRAS (LINHAS)</label>
                    <InputNumber @bind-Value="novoVeiculo.Linhas" class="form-control" placeholder="Ex: 10" />
                </div>
                <div class="form-group">
                    <label>NÚMERO DE COLUNAS POR FILEIRA</label>
                    <InputNumber @bind-Value="novoVeiculo.Colunas" class="form-control" placeholder="Ex: 4" />
                </div>
            </div>

            @* Capacidade calculada (exibição informativa) *@
            @if (novoVeiculo.Linhas > 0 && novoVeiculo.Colunas > 0)
            {
                <div class="alert alert-info text-center">
                    Capacidade total: <strong>@(novoVeiculo.Linhas * novoVeiculo.Colunas)</strong> assentos
                </div>
            }

            @* Botões de Ação *@
            <div class="form-actions">
                <button type="button" @onclick='() => Navigation.NavigateTo("/poslogin")' class="btn-cancel">Cancelar</button>
                <button type="submit" class="btn-save" disabled="@estaProcessando">
                    @(estaProcessando ? "Salvando..." : "Cadastrar Veículo")
                </button>
            </div>
        </EditForm>
    </div>
</main>

<footer class="site-footer">
    <p>&copy; 2026 Billet - Todos os direitos reservados.</p>
</footer>

@code {
    private Veiculo novoVeiculo = new();
    private bool estaProcessando = false;
    private bool exibirSucesso = false;
    private bool exibirErro = false;
    private string mensagemErro = "";

    private async Task SalvarVeiculo()
    {
        estaProcessando = true;

        var response = await VeiculoService.CriarVeiculoAsync(novoVeiculo);

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

### 4.4. Arquivo: `Components/Pages/CriarVeiculo.razor.css` (NOVO)

**Conteúdo idêntico** ao `CriarViagem.razor.css`. Reutiliza as mesmas classes CSS (`create-wrapper`, `create-container`, `create-title`, `create-form`, `form-row`, `form-group`, `form-actions`, `btn-cancel`, `btn-save`, `navbar-top`, `brand-button`, `site-footer`).

### 4.5. Mapeamento de Campos do Formulário

| Campo | Tipo no Modelo | Input Blazor | Observação |
|-------|---------------|-------------|------------|
| `Modelo` | `string` | `InputText` | Obrigatório (validado pela API) |
| `Placa` | `string` | `InputText` | Obrigatório + único (validado pela API) |
| `Tipo` | `string` | `InputSelect` | Dropdown com 5 opções |
| `Linhas` | `int` | `InputNumber` | > 0 (validado pela API) |
| `Colunas` | `int` | `InputNumber` | > 0 (validado pela API) |
| `Capacidade` | `int` | *(não exibido)* | Calculado automaticamente pela API |
| `Id` | `int` | *(não exibido)* | Atribuído pela API |

### 4.6. Arquivo: `Program.cs` (Blazor) — alteração

```csharp
builder.Services.AddScoped<ViagemService>();
builder.Services.AddScoped<VeiculoService>();   // ← NOVA LINHA
builder.Services.AddScoped<UsuarioService>();
builder.Services.AddSingleton<AuthService>();
```

---

## 5. Arquivos Afetados

| Arquivo | Operação | Descrição |
|---------|:--------:|-----------|
| `Models/Veiculo.cs` | **CRIAR** | Novo modelo Blazor com 7 campos |
| `Services/VeiculoService.cs` | **CRIAR** | Novo serviço com 3 métodos (listar, buscarPorId, criar) |
| `Components/Pages/CriarVeiculo.razor` | **CRIAR** | Novo formulário de cadastro de veículo |
| `Components/Pages/CriarVeiculo.razor.css` | **CRIAR** | CSS idêntico ao CriarViagem.razor.css |
| `Program.cs` (Blazor) | **EDITAR** | Adicionar `builder.Services.AddScoped<VeiculoService>()` |

**NENHUM outro arquivo do projeto é afetado por esta spec.**

---

## 6. Passos de Execução

### Passo 1: Criar modelo Blazor `Veiculo.cs`

- Criar `Models/Veiculo.cs` com o conteúdo da Seção 4.1.

### Passo 2: Criar serviço `VeiculoService.cs`

- Criar `Services/VeiculoService.cs` com o conteúdo da Seção 4.2 (3 métodos).

### Passo 3: Criar página `CriarVeiculo.razor` + CSS

- Criar `Components/Pages/CriarVeiculo.razor` com o conteúdo da Seção 4.3.
- **Copiar** o CSS de `CriarViagem.razor.css` para `CriarVeiculo.razor.css`.

### Passo 4: Atualizar `Program.cs` (Blazor)

- Adicionar `builder.Services.AddScoped<VeiculoService>()` após o registro de `ViagemService`.

### Passo 5: Verificar build

- Executar `dotnet build` no projeto Blazor.
- O build deve retornar **0 erros NOVOS** (os 9 erros existentes de `EventoService`/`Evento` em Home, Poslogin, Meusingressos, Venda permanecem — são das specs 0110-0140).

---

## 7. Critérios de Aceitação

| ID | Critério | Verificação |
|:--:|----------|-------------|
| CA01 | `Models/Veiculo.cs` EXISTE com 7 campos (`Id`, `Modelo`, `Placa`, `Capacidade`, `Tipo`, `Linhas`, `Colunas`) | Inspeção |
| CA02 | `Services/VeiculoService.cs` EXISTE com 3 métodos (`ListarVeiculosAsync`, `BuscarPorIdAsync`, `CriarVeiculoAsync`) | Inspeção |
| CA03 | `CriarVeiculo.razor` EXISTE com rota `@page "/criarveiculo"` | Inspeção |
| CA04 | `CriarVeiculo.razor.css` EXISTE | Inspeção |
| CA05 | Formulário contém campos: Modelo, Placa, Tipo (dropdown), Linhas, Colunas | Inspeção |
| CA06 | Formulário NÃO contém campo Capacidade (calculado pela API) | Inspeção |
| CA07 | Capacidade calculada é exibida como informação (`Linhas × Colunas`) quando > 0 | Inspeção |
| CA08 | `VeiculoService` injetado no `CriarVeiculo.razor` | Inspeção |
| CA09 | `Program.cs` (Blazor) registra `VeiculoService` | Inspeção |
| CA10 | `dotnet build` no projeto Blazor NÃO introduz novos erros (apenas os 9 existentes de Evento/EventoService) | Build output |

---

## 8. Riscos e Observações

### 8.1. Observação: CSS compartilhado

- O CSS de `CriarVeiculo.razor.css` é idêntico ao `CriarViagem.razor.css`.
- As classes CSS (`create-wrapper`, `create-form`, etc.) são compartilhadas entre as páginas de criação.
- Se o CSS for alterado futuramente, ambas as páginas devem ser atualizadas.

### 8.2. Observação: Tipo como dropdown

- Diferentemente do endpoint da API (que aceita string livre e valida no servidor), o formulário usa `<InputSelect>` com 5 opções fixas.
- Isso melhora a UX e reduz erros de digitação.
- Os valores do dropdown correspondem exatamente aos valores aceitos pela API.

### 8.3. Observação: Página acessível apenas para admin

- Assim como `CriarViagem.razor`, esta página não tem proteção de rota.
- A autenticação/autorização será implementada na Spec 0210 (JWT).
- Por enquanto, qualquer usuário que conheça a URL `/criarveiculo` pode acessá-la.

### 8.4. Observação: Independência do build quebrado

- Esta spec NÃO depende das specs 0110-0140 para compilar.
- Ela cria arquivos novos, sem alterar os existentes que quebram o build.
- O build do Blazor continuará com 9 erros (Home, Poslogin, Meusingressos, Venda) até que as specs correspondentes sejam implementadas.

---

## 9. Referências

| Documento | Caminho | Relevância |
|-----------|---------|------------|
| Spec 0050 (VeiculosController) | [`specs/0050-modelo-controller-veiculo.md`](0050-modelo-controller-veiculo.md) | Modelo Veiculo da API e endpoints |
| Arquitetura pivotada | [`arquitetura-pivotagem.md`](../arquitetura-pivotagem.md) | Seção 5.3: VeiculoService |
| Roadmap | [`roadmap.md`](../roadmap.md) | Definição da Spec 0090 na Fase 2 |
| CriarViagem.razor | `Components/Pages/CriarViagem.razor` | Referência de estilo e estrutura |
| CriarViagem.razor.css | `Components/Pages/CriarViagem.razor.css` | CSS a ser copiado |
| Program.cs (Blazor) | `billet_2/billet_2/Program.cs` | Registro de serviços |

---

> **Aguardando aprovação do usuário.**
> **NÃO implementar até que o status seja alterado para "Aprovado".**
