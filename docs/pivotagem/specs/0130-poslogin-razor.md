# Spec 0130 — Refatorar `Poslogin.razor`

## Metadados

| Campo | Valor |
|-------|-------|
| **Código** | 0130 |
| **Fase** | 3 — Refatoração Final e Limpeza |
| **Tipo** | Refatorar |
| **Prioridade** | 🔴 Alta |
| **Status** | Concluído |
| **Dependências** | Spec 0080 (ViagemService), Spec 0090 (VeiculoService), Spec 0110 (PassagemService) |
| **Dependentes** | Fase 4 (Testes) |
| **Estimativa** | 45 minutos |
| **Documento de referência** | [`roadmap.md`](../roadmap.md) — Seção 6, Fase 3 |

---

## 1. Objetivo

Refatorar a página pós-login (`Poslogin.razor`) do domínio TicketPrime (eventos) para TripPrime (viagens). É uma refatoração tipo **rename + adapt in place** com ~85% de reaproveitamento de estrutura. Envolve:

1. Trocar `EventoService` → `ViagemService`.
2. Trocar modelo `Evento` → `Viagem`.
3. Adaptar textos e labels para o domínio de viagens.
4. Adicionar menu admin com 2 links: Criar Viagem e Criar Veículo.
5. Atualizar link para usuário comum de "Meus Ingressos" para "Minhas Passagens".
6. Atualizar `Poslogin.razor.css` (renomear classes CSS `event-*` → `viagem-*`).

---

## 2. Motivação

### 2.1. Poslogin é o dashboard do usuário logado

A Poslogin (`/poslogin`) é a página que o usuário vê após fazer login. Ela exibe um banner de destaque e os cards das entidades principais. Precisa refletir o domínio TripPrime: viagens com origem → destino, data de partida e link para o mapa de assentos.

### 2.2. Resolve 2 erros de build

`Poslogin.razor` é um dos 2 arquivos restantes que quebram o build (referencia `EventoService` e `Evento`). Ao refatorá-lo, **os 2 últimos erros de compilação são eliminados** (assumindo que Venda.razor seja removido/redirecionado pela Spec 0140).

### 2.3. Menu admin expandido

O menu admin atual só tem "Criar Evento ➕". Após a pivotagem, precisa ter "Criar Viagem ➕" e "Criar Veiculo ➕" para refletir o novo domínio com 2 entidades gerenciáveis.

---

## 3. Estado Atual (ANTES)

### 3.1. Poslogin.razor (150 linhas)

```csharp
@page "/poslogin"
@using billet_2.Models
@inject billet_2.Services.EventoService EventoService
@inject NavigationManager Navigation
@inject billet_2.Services.AuthService AuthService
@rendermode RenderMode.InteractiveServer

// Navbar: brand "BiLleT*"
// Admin: link "/criarevento" com texto "Criar Evento ➕"
// Usuário comum: link "/meusingressos" com texto "Meus Ingressos 🎫"
// Dropdown de perfil com nome, email e botão Sair
// Hero banner: eventoDestaque com FotoUrl, Nome, Data
// Section title: "Nossos eventos 🤓"
// Cards: ev.Nome, ev.Descricao, ev.Local, ev.Data, ev.FotoUrl
// Link: /vendas/@ev.Id → "Garantir Ingressos →"
// Code: List<Evento>? eventos; Evento? eventoDestaque; EventoService.ListarEventosAsync()
```

### 3.2. Poslogin.razor.css (299 linhas)

Classes com prefixo `event-*`: `.events-section`, `.event-card`, `.event-img-cap`, `.event-card-body`, `.event-link`.

---

## 4. Estado Desejado (DEPOIS)

### 4.1. Poslogin.razor (REFATORADO)

```razor
@page "/poslogin"
@using billet_2.Models
@inject billet_2.Services.ViagemService ViagemService
@inject NavigationManager Navigation
@inject billet_2.Services.AuthService AuthService
@rendermode RenderMode.InteractiveServer

@* --- 1. BARRA DE NAVEGAÇÃO --- *@
@if (!AuthService.EstaLogado)
{
    <p>Redirecionando...</p>
    Navigation.NavigateTo("/");
}else{
    <nav class="navbar-top">
        <a href="/poslogin" class="brand-button">BiLleT*</a>

        <div class="nav-actions-right">
            @if(@AuthService.UsuarioLogado!.Adm){
                <a href="/criarviagem" class="nav-action-button create-btn">
                    Criar Viagem ➕
                </a>
                <a href="/criarveiculo" class="nav-action-button create-btn">
                    Criar Veículo ⚙
                </a>
            }else{
                <a href="/minhaspassagens" class="nav-action-button">
                    Minhas Passagens 🎫
                </a>
            }

            <div class="profile-wrapper">
                <button class="nav-action-button create-btn" @onclick='ToggleDropdown' style="cursor:pointer;">
                    @AuthService.UsuarioLogado!.Nome 👤
                </button>

                @if (dropdownAberto)
                {
                    <div class="profile-dropdown">
                        <div class="dropdown-header">
                            <p class="dropdown-nome">@AuthService.UsuarioLogado!.Nome</p>
                            <p class="dropdown-email">@AuthService.UsuarioLogado!.Email</p>
                        </div>
                        <hr class="dropdown-divider" />
                        <button class="dropdown-item-sair" @onclick="Deslogar">
                            🚪 Sair
                        </button>
                    </div>
                }
            </div>
        </div>
    </nav>

    @* --- 2. SEÇÃO HERO (BANNER DINÂMICO) --- *@
    @if (viagemDestaque != null)
    {
        <div class="hero-container">
            @* Imagem de fundo desfocada - Puxando do Banco *@
            <img src="@(string.IsNullOrEmpty(viagemDestaque.FotoUrl) ? "default.jpg" : viagemDestaque.FotoUrl)"
                class="img-fundo-blur" alt="Fundo" />

            <div class="card-frente-container">
                <img src="@(string.IsNullOrEmpty(viagemDestaque.FotoUrl) ? "default.jpg" : viagemDestaque.FotoUrl)"
                    class="img-card-frente" alt="@viagemDestaque.Origem → @viagemDestaque.Destino" />
                
                <div class="card-frente-info">
                    <h3>@viagemDestaque.Origem → @viagemDestaque.Destino</h3>
                    <p>Destaque • @viagemDestaque.DataPartida.ToString("dd/MM/yyyy")</p>
                </div>
            </div>
        </div>
    }

    @* --- 3. SEÇÃO DE VIAGENS --- *@
    <section class="viagens-section">
        <div class="container py-5">
            <h2 class="section-title">Nossas viagens 🤓</h2>
            
            <div class="row g-4">
                @if (viagens == null)
                {
                    <div class="col-12 text-center">
                        <p class="text-muted">Sincronizando viagens com o banco...</p>
                    </div>
                }
                else if (!viagens.Any())
                {
                    <div class="col-12 text-center">
                        <p class="text-muted">Nenhuma viagem cadastrada no momento.</p>
                    </div>
                }
                else
                {
                    @foreach (var viagem in viagens)
                    {
                        <div class="col-12 col-md-4">
                            <div class="viagem-card shadow-sm">
                                <div class="viagem-img-cap">
                                    @if (!string.IsNullOrEmpty(viagem.FotoUrl))
                                    {
                                        <img src="@viagem.FotoUrl" alt="@viagem.Origem → @viagem.Destino" style="width:100%; height:100%; object-fit:cover;" />
                                    }
                                    else
                                    {
                                        <span>FOTO DA VIAGEM</span>
                                    }
                                </div>
                                <div class="viagem-card-body">
                                    <h5>@viagem.Origem → @viagem.Destino</h5>
                                    <p>@viagem.Descricao</p>
                                    <small>📅 @viagem.DataPartida.ToString("dd/MM/yyyy HH:mm")</small>
                                    
                                    @* Passando o ID real do banco para a página de assentos *@
                                    <a href="/viagem/@viagem.Id/assentos" class="viagem-link">Ver assentos →</a>
                                </div>
                            </div>
                        </div>
                    }
                }
            </div>
        </div>
    </section>

    <footer class="site-footer">
        <p>&copy; 2026 Billet - Todos os direitos reservados.</p>
    </footer>
}
@code {
    private List<Viagem>? viagens;
    private Viagem? viagemDestaque;
    private bool dropdownAberto = false;

    private void ToggleDropdown()
    {
        dropdownAberto = !dropdownAberto;
    }

    private void Deslogar()
    {
        AuthService.Deslogar();
        Navigation.NavigateTo("/");
    }
    protected override async Task OnInitializedAsync()
    {
        // 1. Busca os dados reais usando o Service
        viagens = await ViagemService.ListarViagensAsync();

        // 2. Define a primeira viagem da lista como o destaque do banner Hero
        if (viagens != null && viagens.Any())
        {
            viagemDestaque = viagens.FirstOrDefault();
        }
    }
}
```

### 4.2. Poslogin.razor.css (REFATORADO)

**Mesmo conteúdo**, com renomeação de classes:
- `.events-section` → `.viagens-section`
- `.event-card` → `.viagem-card`
- `.event-img-cap` → `.viagem-img-cap`
- `.event-card-body` → `.viagem-card-body`
- `.event-link` → `.viagem-link`

### 4.3. Mapeamento de Mudanças (Antes → Depois)

| Antes (TicketPrime) | Depois (TripPrime) |
|---------------------|---------------------|
| `@inject EventoService` | `@inject ViagemService` |
| `List<Evento>? eventos` | `List<Viagem>? viagens` |
| `Evento? eventoDestaque` | `Viagem? viagemDestaque` |
| `EventoService.ListarEventosAsync()` | `ViagemService.ListarViagensAsync()` |
| `evento.Nome` | `viagem.Origem → viagem.Destino` |
| `evento.Local` | *(removido — não existe em Viagem)* |
| `evento.Data` | `viagem.DataPartida` |
| `evento.Descricao` | `viagem.Descricao` |
| `evento.FotoUrl` | `viagem.FotoUrl` |
| `evento.Id` | `viagem.Id` |
| Título seção: "Nossos eventos 🤓" | "Nossas viagens 🤓" |
| Placeholder: "FOTO DO EVENTO" | "FOTO DA VIAGEM" |
| Loading: "Sincronizando eventos..." | "Sincronizando viagens..." |
| Empty: "Nenhum evento cadastrado" | "Nenhuma viagem cadastrada" |
| Link: `/vendas/{id}` | `/viagem/{id}/assentos` |
| Link text: "Garantir Ingressos →" | "Ver assentos →" |
| Admin link 1: `/criarevento` "Criar Evento ➕" | `/criarviagem` "Criar Viagem ➕" |
| Admin link 2: *(não existe)* | `/criarveiculo` "Criar Veículo ⚙" |
| Não-admin link: `/meusingressos` "Meus Ingressos 🎫" | `/minhaspassagens` "Minhas Passagens 🎫" |
| CSS: `.events-section` | `.viagens-section` |
| CSS: `.event-card` | `.viagem-card` |
| CSS: `.event-img-cap` | `.viagem-img-cap` |
| CSS: `.event-card-body` | `.viagem-card-body` |
| CSS: `.event-link` | `.viagem-link` |

---

## 5. Arquivos Afetados

| Arquivo | Operação | Descrição |
|---------|:--------:|-----------|
| `Components/Pages/Poslogin.razor` | **EDITAR** | Refatorar: Evento → Viagem, textos, links e menu admin |
| `Components/Pages/Poslogin.razor.css` | **EDITAR** | Renomear classes `event-*` → `viagem-*` |

**NENHUM outro arquivo do projeto é afetado por esta spec.**

---

## 6. Passos de Execução

### Passo 1: Refatorar `Poslogin.razor`

- Trocar `@inject EventoService` → `@inject ViagemService`.
- Trocar `List<Evento>? eventos` → `List<Viagem>? viagens`.
- Trocar `Evento? eventoDestaque` → `Viagem? viagemDestaque`.
- Trocar `ListarEventosAsync()` → `ListarViagensAsync()`.
- Substituir todas as referências a `evento.*` pelos campos correspondentes de `viagem.*`.
- Atualizar textos: section title, loading, empty state, placeholder de foto.
- Trocar link `/vendas/{id}` → `/viagem/{id}/assentos` e texto "Garantir Ingressos" → "Ver assentos".
- Menu admin: trocar link único `/criarevento` "Criar Evento ➕" por 2 links:
  - `/criarviagem` "Criar Viagem ➕"
  - `/criarveiculo` "Criar Veículo ⚙"
- Menu usuário comum: trocar link `/meusingressos` "Meus Ingressos 🎫" → `/minhaspassagens` "Minhas Passagens 🎫".

### Passo 2: Refatorar `Poslogin.razor.css`

- Renomear as 5 classes CSS com prefixo `event-` para `viagem-`.

### Passo 3: Verificar build

- Executar `dotnet build` no projeto Blazor.
- O build deve retornar **0 erros** relacionados a Poslogin. Os únicos erros restantes devem ser de `Venda.razor` (que será tratado na Spec 0140).

---

## 7. Critérios de Aceitação

| ID | Critério | Verificação |
|:--:|----------|-------------|
| CA01 | `Poslogin.razor` injeta `ViagemService` (não `EventoService`) | Inspeção |
| CA02 | `Poslogin.razor` usa `List<Viagem>?` (não `List<Evento>?`) | Inspeção |
| CA03 | `Poslogin.razor` chama `ListarViagensAsync()` (não `ListarEventosAsync`) | Inspeção |
| CA04 | Cards exibem `Origem → Destino` no lugar de `Nome` | Inspeção |
| CA05 | Cards exibem `DataPartida` no lugar de `Data` + `Local` | Inspeção |
| CA06 | Título da seção é "Nossas viagens 🤓" | Inspeção |
| CA07 | Link dos cards aponta para `/viagem/{id}/assentos` com texto "Ver assentos →" | Inspeção |
| CA08 | Menu admin tem 2 links: "Criar Viagem ➕" e "Criar Veículo ⚙" | Inspeção |
| CA09 | Menu usuário comum tem link "Minhas Passagens 🎫" apontando para `/minhaspassagens` | Inspeção |
| CA10 | `Poslogin.razor.css` renomeou classes `event-*` → `viagem-*` | Inspeção |
| CA11 | `dotnet build` não reporta erros em `Poslogin.razor` | Build output |

---

## 8. Riscos e Observações

### 8.1. Campo `Local` removido

O modelo `Viagem` não tem campo `Local`. O card agora exibe apenas a data de partida. A informação de localização é transmitida por `Origem → Destino` no título.

### 8.2. CSS mínimo

Apenas 5 classes CSS são renomeadas. Toda a estrutura visual do `Poslogin.razor.css` (299 linhas) permanece idêntica.

### 8.3. Menu admin com 2 botões

O menu admin agora exibe 2 botões lado a lado, ambos com o estilo `create-btn` (borda dourada). Isso mantém consistência visual com o design original de 1 botão, apenas adicionando o segundo.

### 8.4. Novo emoji no botão Criar Veículo

O botão "Criar Veículo" usa o emoji ⚙ para diferenciar-se visualmente de "Criar Viagem ➕". Ambos mantêm o estilo `create-btn` com borda dourada.

### 8.5. Dependência de rotas existentes

Esta spec assume que as rotas `/criarviagem`, `/criarveiculo`, `/minhaspassagens` e `/viagem/{id}/assentos` já existem (criadas em specs anteriores: 0080, 0090, 0100, 0110). Nenhuma nova rota é criada por esta spec.

---

## 9. Referências

| Documento | Caminho | Relevância |
|-----------|---------|------------|
| Spec 0080 (ViagemService) | [`specs/0080-viagem-service-criar-viagem.md`](0080-viagem-service-criar-viagem.md) | ViagemService e modelo Viagem |
| Spec 0090 (VeiculoService) | [`specs/0090-veiculo-service-criar-veiculo.md`](0090-veiculo-service-criar-veiculo.md) | VeiculoService e rota `/criarveiculo` |
| Spec 0110 (PassagemService) | [`specs/0110-passagem-service-minhas-passagens.md`](0110-passagem-service-minhas-passagens.md) | PassagemService e rota `/minhaspassagens` |
| Arquitetura pivotada | [`arquitetura-pivotagem.md`](../arquitetura-pivotagem.md) | Seção 5.3: Poslogin.razor |
| Roadmap | [`roadmap.md`](../roadmap.md) | Definição da Spec 0130 na Fase 3 |
| Viagem.cs | `Models/Viagem.cs` | Modelo Viagem (10 campos) |

---

> **Spec 0130 concluída em 18/06/2026.**
