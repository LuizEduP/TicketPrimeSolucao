# Spec 0020 — Adicionar `src/api.csproj` à Solution `billet_2.slnx`

## Metadados

| Campo | Valor |
|-------|-------|
| **Código** | 0020 |
| **Fase** | 0 — Preparação e Correções |
| **Tipo** | Correção |
| **Prioridade** | 🔴 Alta |
| **Status** | Concluído |
| **Dependências** | Nenhuma |
| **Dependentes** | Fase 1 (Backend: controllers dependem da API estar na solution para build integrado) |
| **Estimativa** | 5 minutos |
| **Documento de referência** | [`roadmap.md`](../roadmap.md) — Seção 3, Fase 0 |

---

## 1. Objetivo

Adicionar o projeto `src/api.csproj` (API backend .NET) ao arquivo de solution `billet_2.slnx` para que a API e o frontend Blazor possam ser compilados e executados a partir da mesma solution.

---

## 2. Motivação

### 2.1. Solution `billet_2.slnx` está incompleta

- O arquivo `billet_2.slnx` atualmente contém apenas o projeto Blazor (`billet_2/billet_2/billet_2.csproj`).
- O projeto `src/api.csproj` (API backend) NÃO está incluído nesta solution.
- Sem a API na solution, builds integrados (ex: `dotnet build` na raiz da solution) não compilam a API.

### 2.2. Duplicidade com `TicketPrimeSolucao-pivotagem.sln`

- O arquivo `TicketPrimeSolucao-pivotagem.sln` (nível workspace) JÁ referencia `api.csproj`.
- **Verificação de duplicidade:** NÃO há conflito. São solutions diferentes com propósitos diferentes:
  - `TicketPrimeSolucao-pivotagem.sln` → solution "pai" que agrupa todos os projetos do workspace.
  - `billet_2.slnx` → solution do frontend Blazor que deve incluir a API como dependência para execução integrada (ex: `dotnet run` que inicia ambos).

---

## 3. Estado Atual (ANTES)

### 3.1. Conteúdo atual de `billet_2.slnx`

```xml
<Solution>
  <Project Path="billet_2/billet_2/billet_2.csproj" />
</Solution>
```

### 3.2. Projeto `src/api.csproj`

- **Caminho:** `TicketPrimeSolucao-main/src/api.csproj`
- **Tipo:** `Microsoft.NET.Sdk.Web` (.NET 10, Minimal API)
- **Status:** EXISTE e é válido

---

## 4. Estado Desejado (DEPOIS)

### 4.1. Conteúdo corrigido de `billet_2.slnx`

```xml
<Solution>
  <Project Path="billet_2/billet_2/billet_2.csproj" />
  <Project Path="src/api.csproj" />
</Solution>
```

**Apenas UMA linha adicionada:** `<Project Path="src/api.csproj" />`

### 4.2. Caminho relativo

- O arquivo `billet_2.slnx` está em `TicketPrimeSolucao-main/billet_2.slnx`.
- O caminho `src/api.csproj` é relativo ao diretório da solution → resolve para `TicketPrimeSolucao-main/src/api.csproj`.

---

## 5. Arquivos Afetados

| Arquivo | Operação | Descrição |
|---------|:--------:|-----------|
| `TicketPrimeSolucao-main/billet_2.slnx` | **EDITAR** | Adicionar 1 linha: `<Project Path="src/api.csproj" />` |

**NENHUM outro arquivo é afetado.**
**NENHUM arquivo é criado ou removido.**

---

## 6. Passos de Execução

### Passo 1: Ler o arquivo `billet_2.slnx`

- Abrir `TicketPrimeSolucao-main/billet_2.slnx` para leitura.

### Passo 2: Adicionar a referência ao `api.csproj`

- Localizar a linha `</Solution>` (fim do arquivo).
- Inserir `<Project Path="src/api.csproj" />` em uma nova linha ANTES de `</Solution>`.
- **NÃO alterar nenhuma outra linha.**
- Manter a indentação consistente (2 espaços).

### Passo 3: Verificar o resultado

- Confirmar que `billet_2.slnx` contém ambas as entradas `<Project Path="...">`.
- Confirmar que o caminho `src/api.csproj` resolve corretamente a partir do diretório `TicketPrimeSolucao-main/`.
- Executar `dotnet build` a partir do diretório `TicketPrimeSolucao-main/billet_2.slnx` para garantir que a solution compila sem erros (OPCIONAL — pode ser verificado apenas se o SDK .NET estiver disponível).

---

## 7. Critérios de Aceitação

| ID | Critério | Verificação |
|:--:|----------|-------------|
| CA01 | `billet_2.slnx` contém `<Project Path="billet_2/billet_2/billet_2.csproj" />` | Grep pela string no arquivo |
| CA02 | `billet_2.slnx` contém `<Project Path="src/api.csproj" />` | Grep pela string no arquivo |
| CA03 | `billet_2.slnx` contém EXATAMENTE 2 entradas `<Project Path="...">` | Contagem de ocorrências = 2 |
| CA04 | O arquivo `src/api.csproj` EXISTE e é acessível a partir do caminho relativo | `-f src/api.csproj` a partir de `TicketPrimeSolucao-main/` |
| CA05 | NENHUMA outra alteração foi feita no arquivo `billet_2.slnx` além da adição da linha | Diff mostra apenas +1 linha |
| CA06 | `TicketPrimeSolucao-pivotagem.sln` NÃO foi alterado | Hash/checksum do arquivo inalterado |

---

## 8. Riscos e Observações

### 8.1. Risco: Caminho relativo incorreto

- **Análise:** O `.slnx` está em `TicketPrimeSolucao-main/billet_2.slnx`. O caminho `src/api.csproj` resolve para `TicketPrimeSolucao-main/src/api.csproj`. **Confirmado como correto.**

### 8.2. Observação: Formato `.slnx`

- O formato `.slnx` é o novo formato de solution do .NET (simplificado, XML puro).
- NÃO requer GUIDs, seções de configuração de build, ou nesting — o .NET SDK gerencia automaticamente.

### 8.3. Observação: Sem duplicidade real

- Embora `api.csproj` já esteja em `TicketPrimeSolucao-pivotagem.sln`, isso NÃO é uma duplicação problemática. Um mesmo `.csproj` pode pertencer a múltiplas solutions.
- Não há risco de conflito ou inconsistência.

---

## 9. Referências

| Documento | Caminho | Relevância |
|-----------|---------|------------|
| Solution alvo | `billet_2.slnx` | Arquivo a ser editado |
| Projeto API | `src/api.csproj` | Projeto a ser adicionado |
| Solution pai | `TicketPrimeSolucao-pivotagem.sln` | Já contém api.csproj (verificação de duplicidade) |
| Roadmap da pivotagem | [`roadmap.md`](../roadmap.md) | Definição da Spec 0020 na Fase 0 |

---

> **Aprovado por:** Castor
> **Data de aprovação:** 29/05/2026
> **Data de implementação:** 29/05/2026
