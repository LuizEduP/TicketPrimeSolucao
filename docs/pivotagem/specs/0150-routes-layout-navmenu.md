# Spec 0150 — Atualizar `Routes.razor`, `MainLayout.razor` e `NavMenu.razor`

## Metadados

| Campo | Valor |
|-------|-------|
| **Código** | 0150 |
| **Fase** | 3 — Refatoração Final e Limpeza |
| **Tipo** | Refatorar |
| **Prioridade** | 🔴 Alta |
| **Status** | Concluído |
| **Dependências** | Spec 0120 (Home.razor), Spec 0130 (Poslogin.razor), Spec 0140 (Venda.razor removido) |
| **Dependentes** | Fase 4 (Testes) |
| **Estimativa** | 15 minutos |
| **Documento de referência** | [`roadmap.md`](../roadmap.md) — Seção 6, Fase 3 |

---

## 1. Objetivo

Realizar a limpeza final de navegação e layout, removendo referências ao domínio antigo nos arquivos de infraestrutura de roteamento. Esta spec fecha a Fase 3 garantindo que não há links quebrados ou referências obsoletas na camada de navegação.

---

## 2. Motivação

### 2.1. NavMenu.razor ainda referencia "Vendas"

O `NavMenu.razor` do projeto Client (WASM) contém um link `<NavLink href="vendas">` que aponta para uma rota agora inexistente (o `Venda.razor` foi removido na Spec 0140). O link precisa ser removido para evitar navegação para página inexistente.

### 2.2. Verificação de Routes.razor e MainLayout.razor

Diferentemente do que o roadmap sugeria inicialmente, estes arquivos **não precisam de alterações**:
- **Routes.razor** (server): Rotas são descobertas automaticamente via `@page` directives. Todas as páginas novas (Viagem, Veiculo, MapaAssentos, MinhasPassagens) já declaram suas rotas.
- **MainLayout.razor** (server): É um wrapper mínimo (`<main>@Body</main>`). Cada página gerencia seu próprio navbar e footer. Sem necessidade de alteração.
- **MainLayout.razor** (Client WASM): Layout padrão com sidebar. Sem referências ao domínio antigo.
- **Routes.razor** (Client WASM): Router padrão com `NotFoundPage`. Sem referências ao domínio antigo.

**Conclusão: apenas `NavMenu.razor` precisa de alteração.**

---

## 3. Estado Atual (ANTES)

### 3.1. NavMenu.razor — Client WASM (43 linhas)

```razor
<div class="top-row ps-3 navbar navbar-dark">
    <div class="container-fluid">
        <a class="navbar-brand" href="">billet_2</a>
    </div>
</div>

<input type="checkbox" title="Navigation menu" class="navbar-toggler" />

<div class="nav-scrollable" onclick="document.querySelector('.navbar-toggler').click()">
    <nav class="nav flex-column">
        <div class="nav-item px-3">
            <NavLink class="nav-link" href="" Match="NavLinkMatch.All">
                <span class="bi bi-house-door-fill-nav-menu" aria-hidden="true"></span> Home
            </NavLink>
        </div>

        <div class="nav-item px-3">
            <NavLink class="nav-link" href="counter">
                <span class="bi bi-plus-square-fill-nav-menu" aria-hidden="true"></span> Counter
            </NavLink>
        </div>

        <div class="nav-item px-3">
            <NavLink class="nav-link" href="weather">
                <span class="bi bi-list-nested-nav-menu" aria-hidden="true"></span> Weather
            </NavLink>
        </div>

        <div class="nav-item px-3">
            <NavLink class="nav-link" href="cadastro">
                <span class="bi bi-person-plus" aria-hidden="true"></span> Cadastro
            </NavLink>
        </div>

        <div class="nav-item px-3">                          ← REMOVER este bloco
            <NavLink class="nav-link" href="vendas">          ← rota morta (Venda.razor deletado)
                <span class="bi bi-cart" aria-hidden="true"></span> Vendas
            </NavLink>
        </div>
    </nav>
</div>
```

---

## 4. Estado Desejado (DEPOIS)

### 4.1. NavMenu.razor — Client WASM (REFATORADO)

```razor
<div class="top-row ps-3 navbar navbar-dark">
    <div class="container-fluid">
        <a class="navbar-brand" href="">billet_2</a>
    </div>
</div>

<input type="checkbox" title="Navigation menu" class="navbar-toggler" />

<div class="nav-scrollable" onclick="document.querySelector('.navbar-toggler').click()">
    <nav class="nav flex-column">
        <div class="nav-item px-3">
            <NavLink class="nav-link" href="" Match="NavLinkMatch.All">
                <span class="bi bi-house-door-fill-nav-menu" aria-hidden="true"></span> Home
            </NavLink>
        </div>

        <div class="nav-item px-3">
            <NavLink class="nav-link" href="counter">
                <span class="bi bi-plus-square-fill-nav-menu" aria-hidden="true"></span> Counter
            </NavLink>
        </div>

        <div class="nav-item px-3">
            <NavLink class="nav-link" href="weather">
                <span class="bi bi-list-nested-nav-menu" aria-hidden="true"></span> Weather
            </NavLink>
        </div>

        <div class="nav-item px-3">
            <NavLink class="nav-link" href="cadastro">
                <span class="bi bi-person-plus" aria-hidden="true"></span> Cadastro
            </NavLink>
        </div>
    </nav>
</div>
```

**Única mudança:** remoção do bloco `nav-item` que contém o link "Vendas" (5 linhas removidas).

### 4.2. Routes.razor e MainLayout.razor

**Nenhuma alteração.** Ambos permanecem idênticos.

---

## 5. Arquivos Afetados

| Arquivo | Operação | Descrição |
|---------|:--------:|-----------|
| `billet_2.Client/Layout/NavMenu.razor` | **EDITAR** | Remover link "Vendas" obsoleto |
| `billet_2/Components/Routes.razor` | **NENHUMA** | Já está correto (rotas via `@page`) |
| `billet_2/Components/MainLayout.razor` | **NENHUMA** | Já está correto (wrapper mínimo) |
| `billet_2.Client/Layout/MainLayout.razor` | **NENHUMA** | Sem referências ao domínio antigo |
| `billet_2.Client/Routes.razor` | **NENHUMA** | Sem referências ao domínio antigo |

---

## 6. Passos de Execução

### Passo 1: Editar `NavMenu.razor`

- Remover o bloco `<div class="nav-item px-3">` que contém o `<NavLink href="vendas">`.

### Passo 2: Verificar build

- Executar `dotnet build` na solution completa.
- **Resultado esperado: 0 erros, 0 warnings.**

---

## 7. Critérios de Aceitação

| ID | Critério | Verificação |
|:--:|----------|-------------|
| CA01 | `NavMenu.razor` não contém link para "vendas" | Inspeção |
| CA02 | `NavMenu.razor` mantém links Home, Counter, Weather, Cadastro intactos | Inspeção |
| CA03 | `dotnet build` retorna **0 erros e 0 warnings** | Build output |

---

## 8. Riscos e Observações

### 8.1. NavMenu do Client WASM é secundário

O projeto principal usa Blazor Interactive Server (`billet_2`). O Client WASM (`billet_2.Client`) contém apenas páginas de exemplo (Counter, Weather) e a página de Cadastro. A navegação principal do usuário ocorre via navbar customizada dentro de cada página (Home, Poslogin), não via este NavMenu. O impacto de manter ou remover este link é mínimo.

### 8.2. Nenhum CSS afetado

O arquivo `NavMenu.razor.css` contém apenas estilos genéricos (`.nav-item`, `.nav-link`, `.bi-*`). Nenhuma classe específica do domínio TicketPrime existe neste arquivo. Nenhuma alteração necessária.

### 8.3. Fase 3 concluída após esta spec

Após a execução desta spec, todas as tarefas da Fase 3 (Refatoração Final e Limpeza) estarão concluídas. O build estará limpo (0 erros) e o projeto estará pronto para a Fase 4 (Testes).

---

## 9. Referências

| Documento | Caminho | Relevância |
|-----------|---------|------------|
| Spec 0140 (Venda.razor) | [`specs/0140-remover-venda-redirecionar.md`](0140-remover-venda-redirecionar.md) | Remoção que tornou o link "vendas" obsoleto |
| Arquitetura pivotada | [`arquitetura-pivotagem.md`](../arquitetura-pivotagem.md) | Seção 5: mapeamento de componentes |
| Roadmap | [`roadmap.md`](../roadmap.md) | Definição da Spec 0150 na Fase 3 |

---

> **Spec 0150 concluída em 18/06/2026.**
