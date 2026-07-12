# Spec 0120 — Refatorar `Home.razor`

## Metadados

| Campo | Valor |
|-------|-------|
| **Código** | 0120 |
| **Fase** | 3 — Refatoração Final e Limpeza |
| **Tipo** | Refatorar |
| **Prioridade** | 🔴 Alta |
| **Status** | Pendente |
| **Dependências** | Spec 0080 (ViagemService + modelo Viagem) |
| **Dependentes** | Fase 4 (Testes) |
| **Estimativa** | 30 minutos |
| **Documento de referência** | [`roadmap.md`](../roadmap.md) — Seção 6, Fase 3 |

---

## 1. Objetivo

Refatorar a página inicial (`Home.razor`) do domínio TicketPrime (eventos) para TripPrime (viagens). É uma refatoração tipo **rename + adapt in place** com ~90% de reaproveitamento de estrutura. Envolve:

1. Trocar `EventoService` → `ViagemService`.
2. Trocar modelo `Evento` → `Viagem`.
3. Adaptar textos e labels para o domínio de viagens.
4. Atualizar links de navegação.
5. Atualizar `Home.razor.css` (renomear classes CSS `event-*` → `viagem-*`).

---

## 2. Motivação

### 2.1. Home é a porta de entrada

A Home (`/`) é a primeira página que o usuário vê. Ela exibe o hero banner e os cards das entidades principais. Precisa refletir o domínio TripPrime: viagens com origem → destino, data de partida e link para o mapa de assentos.

### 2.2. Resolve 2 erros de build

`Home.razor` é um dos 3 arquivos restantes que quebram o build (referencia `EventoService` e `Evento`). Ao refatorá-lo, **2 dos 7 erros de compilação são eliminados**.

### 2.3. Mudanças mínimas, máximo reaproveitamento

~90% da estrutura visual é mantida. Apenas nomes de variáveis, textos e links são alterados.

---

## 3. Estado Atual (ANTES)

### 3.1. Home.razor (90 linhas)

```csharp
@page "/"
@using billet_2.Models
@inject billet_2.Services.EventoService EventoService

// Hero: "B i L l e T *", "eventos", "momentos inesquecíveis"
// Section title: "Nossos eventos ↷"
// Cards: evento.Nome, evento.Descricao, evento.Local, evento.Data, evento.FotoUrl
// Link: /vendas/@evento.Id → "Ver ingressos →"
// Code: List<Evento>? eventos; EventoService.ListarEventosAsync()
```

### 3.2. Home.razor.css (149 linhas)

Classes com prefixo `event-*`: `.events-section`, `.event-card`, `.event-img-cap`, `.event-card-body`, `.event-link`.

---

## 4. Estado Desejado (DEPOIS)

### 4.1. Home.razor (REFATORADO)

```razor
@page "/"
@using billet_2.Models
@inject billet_2.Services.ViagemService ViagemService

<div class="page-container">
    @* --- SEÇÃO 1: HERO BANNER (VÍDEO) --- *@
    <section class="hero-section">
        <div class="video-wrapper">
            <video autoplay loop muted playsinline class="video-element">
                <source src="videos/video1.mp4" type="video/mp4" />
                Seu navegador não suporta vídeos.
            </video>
            <div class="video-overlay"></div>
        </div>

        <div class="hero-content">
            <div class="container text-center text-white">
                <h1 class="hero-title">B i L l e T *</h1>
                <p class="hero-text">
                    O Billet não é apenas uma plataforma de vendas; é o ponto de encontro entre quem viaja e quem vive
                    momentos inesquecíveis.
                </p>
                <hr class="hero-hr" />
                <a href="/cadastro" class="btn-hero">Faça seu cadastro e seja família Billet *</a>
            </div>
        </div>
    </section>

    @* --- SEÇÃO 2: VIAGENS DINÂMICAS --- *@
    <section class="viagens-section">
        <div class="container py-5">
            <h2 class="section-title">Nossas viagens ↷</h2>
            
            <div class="row g-4">
                @if (viagens == null)
                {
                    <p class="text-center w-100">Carregando viagens...</p>
                }
                else if (!viagens.Any())
                {
                    <p class="text-center w-100">Nenhuma viagem disponível no momento. Volte logo!</p>
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
                                        <span class="text-muted">FOTO DA VIAGEM</span>
                                    }
                                </div>
                                <div class="viagem-card-body">
                                    <h5>@viagem.Origem → @viagem.Destino</h5>
                                    <p>@viagem.Descricao</p>
                                    <small>📅 @viagem.DataPartida.ToString("dd/MM/yyyy HH:mm")</small>
                                    
                                    <a href="/viagem/@viagem.Id/assentos" class="viagem-link">Ver assentos →</a>
                                </div>
                            </div>
                        </div>
                    }
                }
            </div>
        </div>
    </section>

    @* --- SEÇÃO 3: RODAPÉ --- *@
    <footer class="site-footer">
        <p>&copy; 2026 Billet - Todos os direitos reservados.</p>
    </footer>
</div>

@code {
    private List<Viagem>? viagens;

    protected override async Task OnInitializedAsync()
    {
        viagens = await ViagemService.ListarViagensAsync();
    }
}
```

### 4.2. Home.razor.css (REFATORADO)

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
| `EventoService.ListarEventosAsync()` | `ViagemService.ListarViagensAsync()` |
| `evento.Nome` | `viagem.Origem → viagem.Destino` |
| `evento.Local` | *(removido — não existe em Viagem)* |
| `evento.Data` | `viagem.DataPartida` |
| `evento.Descricao` | `viagem.Descricao` |
| `evento.FotoUrl` | `viagem.FotoUrl` |
| `evento.Id` | `viagem.Id` |
| Hero text: "quem cria e quem vive momentos" | "quem viaja e quem vive momentos" |
| Título seção: "Nossos eventos ↷" | "Nossas viagens ↷" |
| Placeholder: "FOTO DO EVENTO" | "FOTO DA VIAGEM" |
| Link: `/vendas/{id}` | `/viagem/{id}/assentos` |
| Link text: "Ver ingressos →" | "Ver assentos →" |
| Loading: "Carregando eventos incríveis..." | "Carregando viagens..." |
| Empty: "Nenhum evento disponível" | "Nenhuma viagem disponível" |
| CSS: `.events-section` | `.viagens-section` |
| CSS: `.event-card` | `.viagem-card` |
| CSS: `.event-img-cap` | `.viagem-img-cap` |
| CSS: `.event-card-body` | `.viagem-card-body` |
| CSS: `.event-link` | `.viagem-link` |

---

## 5. Arquivos Afetados

| Arquivo | Operação | Descrição |
|---------|:--------:|-----------|
| `Components/Pages/Home.razor` | **EDITAR** | Refatorar: Evento → Viagem, textos e links |
| `Components/Pages/Home.razor.css` | **EDITAR** | Renomear classes `event-*` → `viagem-*` |

**NENHUM outro arquivo do projeto é afetado por esta spec.**

---

## 6. Passos de Execução

### Passo 1: Refatorar `Home.razor`

- Trocar `@inject EventoService` → `@inject ViagemService`.
- Trocar `List<Evento>? eventos` → `List<Viagem>? viagens`.
- Trocar `ListarEventosAsync()` → `ListarViagensAsync()`.
- Substituir todas as referências a `evento.*` pelos campos correspondentes de `viagem.*`.
- Atualizar textos: hero, section title, loading, empty state.
- Trocar link `/vendas/{id}` → `/viagem/{id}/assentos` e texto "Ver ingressos" → "Ver assentos".

### Passo 2: Refatorar `Home.razor.css`

- Renomear as 5 classes CSS com prefixo `event-` para `viagem-`.

### Passo 3: Verificar build

- Executar `dotnet build` no projeto Blazor.
- O build deve retornar **5 erros** (eram 7 — 2 de Home.razor eliminados). Os restantes são de Poslogin e Venda.

---

## 7. Critérios de Aceitação

| ID | Critério | Verificação |
|:--:|----------|-------------|
| CA01 | `Home.razor` injeta `ViagemService` (não `EventoService`) | Inspeção |
| CA02 | `Home.razor` usa `List<Viagem>?` (não `List<Evento>?`) | Inspeção |
| CA03 | `Home.razor` chama `ListarViagensAsync()` (não `ListarEventosAsync`) | Inspeção |
| CA04 | Cards exibem `Origem → Destino` no lugar de `Nome` | Inspeção |
| CA05 | Cards exibem `DataPartida` no lugar de `Data` + `Local` | Inspeção |
| CA06 | Título da seção é "Nossas viagens ↷" | Inspeção |
| CA07 | Link dos cards aponta para `/viagem/{id}/assentos` com texto "Ver assentos →" | Inspeção |
| CA08 | `Home.razor.css` renomeou classes `event-*` → `viagem-*` | Inspeção |
| CA09 | `dotnet build` retorna no máximo **5 erros** (2 a menos que antes) | Build output |

---

## 8. Riscos e Observações

### 8.1. Campo `Local` removido

O modelo `Viagem` não tem campo `Local`. O card agora exibe apenas a data de partida. A informação de localização é transmitida por `Origem → Destino` no título.

### 8.2. CSS mínimo

Apenas 5 classes CSS são renomeadas. Toda a estrutura visual do `Home.razor.css` (149 linhas) permanece idêntica.

### 8.3. Sem render mode explícito

Diferentemente dos componentes novos (que usam `@rendermode RenderMode.InteractiveServer`), a Home atual não declara render mode. Isso é mantido. A Home funciona no modo de renderização padrão do Blazor.

---

## 9. Referências

| Documento | Caminho | Relevância |
|-----------|---------|------------|
| Spec 0080 (ViagemService) | [`specs/0080-viagem-service-criar-viagem.md`](0080-viagem-service-criar-viagem.md) | ViagemService e modelo Viagem |
| Arquitetura pivotada | [`arquitetura-pivotagem.md`](../arquitetura-pivotagem.md) | Seção 5.2: Home.razor |
| Roadmap | [`roadmap.md`](../roadmap.md) | Definição da Spec 0120 na Fase 3 |
| Viagem.cs | `Models/Viagem.cs` | Modelo Viagem (10 campos) |

---

> **Aguardando aprovação do usuário.**
> **NÃO implementar até que o status seja alterado para "Aprovado".**
