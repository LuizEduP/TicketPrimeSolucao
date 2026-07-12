# Spec 0140 — Remover `Venda.razor` e Redirecionar Rota

## Metadados

| Campo | Valor |
|-------|-------|
| **Código** | 0140 |
| **Fase** | 3 — Refatoração Final e Limpeza |
| **Tipo** | Remover |
| **Prioridade** | 🔴 Alta |
| **Status** | Concluído |
| **Dependências** | Spec 0100 (AssentoService + MapaAssentos.razor) |
| **Dependentes** | Fase 4 (Testes) |
| **Estimativa** | 15 minutos |
| **Documento de referência** | [`roadmap.md`](../roadmap.md) — Seção 6, Fase 3 |

---

## 1. Objetivo

Remover o componente `Venda.razor` (e seu CSS associado) — o único componente do TicketPrime original que **não tem reaproveitamento** no domínio TripPrime. A lógica de seleção de setor (VIP/Normal) é conceitualmente incompatível com o novo domínio de mapa de assentos. Em seu lugar, criar uma página de redirecionamento da rota antiga `/vendas/{id}` para a nova `/viagem/{id}/assentos`.

---

## 2. Motivação

### 2.1. `Venda.razor` não tem equivalente no novo domínio

Diferentemente dos outros componentes que puderam ser refatorados (rename + adapt in place), `Venda.razor` implementa uma lógica de seleção de setores (VIP com preço 1.5×, Normal com preço base) que não se traduz para o mapa interativo de assentos do TripPrime. O equivalente funcional já foi criado na Spec 0100 (`MapaAssentos.razor`).

### 2.2. Resolve os 2 últimos erros de build

Após a remoção de `Venda.razor`, **zero erros de compilação** devem permanecer. Os 2 erros atuais (`EventoService` e `Evento` não encontrados em `Venda.razor`) são eliminados.

### 2.3. Preserva compatibilidade com links antigos

Usuários que tenham favoritos/bookmarks para URLs antigas (`/vendas/5`) devem ser redirecionados automaticamente para a nova página equivalente (`/viagem/5/assentos`).

---

## 3. Estado Atual (ANTES)

### 3.1. Venda.razor (131 linhas)

```csharp
@page "/vendas/{Id:int}"
@using billet_2.Models
@inject billet_2.Services.EventoService EventoService    // ← causa erro de build
@inject NavigationManager Navigation
@inject billet_2.Services.AuthService AuthService
@rendermode RenderMode.InteractiveServer
@inject HttpClient Http

// Navbar com brand "BiLleT*" linkando para /poslogin
// Hero banner: evento.FotoUrl, evento.Nome, evento.Local, evento.Data, evento.Descricao
// Seção de setores: VIP (preço ×1.5) e Normal (preço base)
// Ao clicar: navega para /meusingressos?comprar={dados}
// Code: Evento? evento; EventoService.BuscarPorIdAsync(Id)
```

### 3.2. Venda.razor.css (213 linhas)

CSS com estilos para navbar, hero banner, card dividido, seção de setores (VIP/Normal), e rodapé. Nenhuma classe é referenciada por outros componentes.

### 3.3. Erros de build atuais

```
Venda.razor(3,27): error CS0234: "EventoService" não existe em "billet_2.Services"
Venda.razor(118,13): error CS0246: "Evento" não pode ser encontrado
```

---

## 4. Estado Desejado (DEPOIS)

### 4.1. Arquivos removidos

- `Components/Pages/Venda.razor` — **DELETADO**
- `Components/Pages/Venda.razor.css` — **DELETADO**

### 4.2. Nova página de redirecionamento

Criar um arquivo mínimo `Components/Pages/VendaRedirect.razor` que:

1. Mantém a rota `@page "/vendas/{Id:int}"` — compatibilidade com links antigos.
2. Ao inicializar, redireciona para `/viagem/{Id}/assentos` usando `NavigationManager`.
3. Exibe uma mensagem breve de "Redirecionando..." enquanto o redirecionamento ocorre.

```razor
@page "/vendas/{Id:int}"
@inject NavigationManager Navigation

<p>Redirecionando para a nova página de assentos...</p>

@code {
    [Parameter]
    public int Id { get; set; }

    protected override void OnInitialized()
    {
        Navigation.NavigateTo($"/viagem/{Id}/assentos", replace: true);
    }
}
```

**Nota:** `replace: true` substitui a entrada no histórico do navegador, evitando que o botão "voltar" retorne à página de redirecionamento.

### 4.3. Resultado esperado do build

```
Build succeeded.
    0 Warning(s)
    0 Error(s)
```

---

## 5. Arquivos Afetados

| Arquivo | Operação | Descrição |
|---------|:--------:|-----------|
| `Components/Pages/Venda.razor` | **DELETAR** | Componente obsoleto (setores VIP/Normal) |
| `Components/Pages/Venda.razor.css` | **DELETAR** | CSS associado (não referenciado por outros) |
| `Components/Pages/VendaRedirect.razor` | **CRIAR** | Nova página de redirecionamento |

**NENHUM outro arquivo do projeto é afetado por esta spec.**

---

## 6. Passos de Execução

### Passo 1: Deletar arquivos obsoletos

- Deletar `Components/Pages/Venda.razor`
- Deletar `Components/Pages/Venda.razor.css`

### Passo 2: Criar página de redirecionamento

- Criar `Components/Pages/VendaRedirect.razor` com o conteúdo da seção 4.2.
- A rota `@page "/vendas/{Id:int}"` é mantida para compatibilidade.

### Passo 3: Verificar build

- Executar `dotnet build` no projeto Blazor.
- **Resultado esperado: 0 erros, 0 warnings.**

---

## 7. Critérios de Aceitação

| ID | Critério | Verificação |
|:--:|----------|-------------|
| CA01 | `Venda.razor` foi deletado do disco | `ls Components/Pages/Venda.razor` retorna erro |
| CA02 | `Venda.razor.css` foi deletado do disco | `ls Components/Pages/Venda.razor.css` retorna erro |
| CA03 | `VendaRedirect.razor` existe com rota `/vendas/{Id:int}` | Inspeção |
| CA04 | `VendaRedirect.razor` redireciona para `/viagem/{Id}/assentos` com `replace: true` | Inspeção |
| CA05 | `dotnet build` retorna **0 erros e 0 warnings** | Build output |

---

## 8. Riscos e Observações

### 8.1. NavMenu.razor ainda referencia "vendas"

O `NavMenu.razor` do projeto Client (WASM) tem um link `<NavLink href="vendas">`. Este link **não será alterado nesta spec** — será tratado na Spec 0150 (Routes + Layout + NavMenu), que unifica todas as atualizações de navegação.

### 8.2. CSS não é referenciado por outros componentes

`Venda.razor.css` é um arquivo de CSS isolado (scoped) do Blazor. Nenhum outro componente importa ou herda suas classes. A remoção é segura.

### 8.3. Redirecionamento é instantâneo

O redirecionamento ocorre em `OnInitialized()` (síncrono, sem await), portanto é executado antes mesmo da renderização da UI. O texto "Redirecionando..." raramente será visível para o usuário, servindo apenas como fallback caso o JavaScript esteja desabilitado.

---

## 9. Referências

| Documento | Caminho | Relevância |
|-----------|---------|------------|
| Spec 0100 (MapaAssentos) | [`specs/0100-assento-service-mapa-assentos.md`](0100-assento-service-mapa-assentos.md) | Página de destino do redirecionamento |
| Arquitetura pivotada | [`arquitetura-pivotagem.md`](../arquitetura-pivotagem.md) | Seção 5.5: remoção de Venda.razor |
| Roadmap | [`roadmap.md`](../roadmap.md) | Definição da Spec 0140 na Fase 3 |

---

> **Spec 0140 concluída em 18/06/2026.**
