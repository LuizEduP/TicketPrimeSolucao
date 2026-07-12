# Spec 0030 — Renomear Diretórios `eventos/` para `viagens/`

## Metadados

| Campo | Valor |
|-------|-------|
| **Código** | 0030 |
| **Fase** | 0 — Preparação e Correções |
| **Tipo** | Renomeação estrutural |
| **Prioridade** | 🔴 Alta |
| **Status** | Concluído |
| **Dependências** | Spec 0010 (db/script.sql) e Spec 0020 (API na solution) — ambas concluídas |
| **Dependentes** | Spec 0040 (Modelo + Controller Viagem) |
| **Estimativa** | 15 minutos |
| **Documento de referência** | [`roadmap.md`](../roadmap.md) — Seção 3, Fase 0 |

---

## 1. Objetivo

Renomear **APENAS** os diretórios físicos que contêm o nome `eventos` para `viagens`. NENHUM conteúdo de arquivo será alterado.

As alterações de conteúdo (código, referências, rotas) serão tratadas nas specs seguintes:
- Spec 0040 — Refatorar `EventosController.cs` para controller de Viagens
- Spec 0110 — Refatorar `Meusingressos.razor` (que usa `images/eventos/`)

Esta spec é **puramente de movimentação de arquivos no sistema de arquivos**.

---

## 2. Motivação

### 2.1. Alinhamento com o novo domínio TripPrime

O projeto TicketPrime foi pivotado para TripPrime (plataforma de passagens de transporte). O conceito de "evento" foi substituído por "viagem". Os diretórios devem refletir o novo domínio.

### 2.2. Preparação para a Fase 1

A Fase 1 (Backend) criará o modelo `Viagem` e o controller `ViagensController`. O diretório `src/eventos/` será renomeado para `src/viagens/` para que o novo código seja colocado no local semanticamente correto.

### 2.3. Independência entre estrutura e conteúdo

Esta spec separa a movimentação estrutural (Fase 0) da refatoração de conteúdo (Fases 1-3). Isso permite que:
- As specs subsequentes assumam que os diretórios já estão nomeados corretamente.
- O rastreamento de mudanças no Git seja mais limpo (renomeação vs. alteração de conteúdo em commits separados).

---

## 3. Estado Atual (ANTES)

### 3.1. Estrutura dos diretórios

```
TicketPrimeSolucao-main/
├── src/
│   └── eventos/                          ← Diretório a ser renomeado
│       └── EventosController.cs
│
└── billet_2/
    └── billet_2/
        └── wwwroot/
            └── images/
                └── eventos/              ← Diretório a ser renomeado
                    ├── bonner.webp
                    ├── show_rock.jpg
                    └── showrock.png
```

### 3.2. Conteúdo do diretório `src/eventos/`

| Arquivo | Descrição |
|---------|-----------|
| `EventosController.cs` | Controller Minimal API com modelo `Evento` e endpoints `listar`, `listar/{id}`, `cadastrar` |

### 3.3. Conteúdo do diretório `wwwroot/images/eventos/`

| Arquivo | Tamanho |
|---------|--------:|
| `bonner.webp` | ~12 KB |
| `show_rock.jpg` | ~2.5 MB |
| `showrock.png` | ~943 KB |

---

## 4. Estado Desejado (DEPOIS)

### 4.1. Estrutura dos diretórios

```
TicketPrimeSolucao-main/
├── src/
│   └── viagens/                          ← Diretório renomeado
│       └── EventosController.cs          ← CONTEÚDO INALTERADO
│
└── billet_2/
    └── billet_2/
        └── wwwroot/
            └── images/
                └── viagens/              ← Diretório renomeado
                    ├── bonner.webp       ← CONTEÚDO INALTERADO
                    ├── show_rock.jpg     ← CONTEÚDO INALTERADO
                    └── showrock.png      ← CONTEÚDO INALTERADO
```

### 4.2. O que NÃO muda

| Aspecto | Status |
|---------|:------:|
| Nomes dos arquivos dentro dos diretórios | INALTERADOS |
| Conteúdo de qualquer arquivo `.cs`, `.razor`, `.json` | INALTERADO |
| Caminhos em strings de código (`/api/eventos/`, `images/eventos/`) | INALTERADOS |
| Nomes de classes, métodos, variáveis | INALTERADOS |
| Arquivo `Program.cs` | INALTERADO |

---

## 5. Impacto em Arquivos Existentes (Conhecido e Documentado)

### 5.1. Referências que NÃO serão corrigidas nesta spec

Após a renomeação, os seguintes caminhos em arquivos de código ficarão temporariamente desalinhados com os diretórios físicos:

| Arquivo | Referência | Correção será feita na |
|---------|-----------|:----------------------:|
| `src/viagens/EventosController.cs` | Rotas `/api/eventos/listar`, `/api/eventos/cadastrar`, etc. | Spec 0040 |
| `src/Program.cs` | `app.CadastrarEventos()`, `app.ListarEventos()`, `app.ListarEventoPorId()` | Spec 0040 |
| `.../Components/Pages/Cadastro.razor` | Referências a "eventos" | Spec 0120 |
| `.../Components/Pages/Home.razor` | Referências a "eventos" | Spec 0120 |
| `.../Components/Pages/Meusingressos.razor` | `<img src="/images/eventos/...">` | Spec 0110 |
| `.../Components/Pages/Poslogin.razor` | Referências a "eventos" | Spec 0130 |

**Isto é esperado e aceitável.** A Fase 0 é apenas preparação estrutural. Os arquivos de código continuam compilando e funcionando normalmente, pois o C# não vincula namespaces a diretórios neste projeto (não há declaração `namespace` nos controllers).

---

## 6. Arquivos Afetados (Operações de Sistema de Arquivos)

| Operação | Origem | Destino |
|:--------:|--------|---------|
| **RENOMEAR** | `src/eventos/` | `src/viagens/` |
| **RENOMEAR** | `billet_2/billet_2/wwwroot/images/eventos/` | `billet_2/billet_2/wwwroot/images/viagens/` |

**NENHUM arquivo é criado, editado ou excluído — apenas movido via renomeação de diretório.**

---

## 7. Passos de Execução

### Passo 1: Verificar pré-condições

- Confirmar que `src/eventos/` existe e contém `EventosController.cs`.
- Confirmar que `billet_2/billet_2/wwwroot/images/eventos/` existe e contém as 3 imagens.
- Confirmar que `src/viagens/` NÃO existe (evitar sobrescrita acidental).
- Confirmar que `billet_2/billet_2/wwwroot/images/viagens/` NÃO existe.

### Passo 2: Renomear `src/eventos/` → `src/viagens/`

- Executar `mv src/eventos src/viagens` a partir da raiz `TicketPrimeSolucao-main/`.
- OU usar `git mv` para preservar o histórico no Git.

### Passo 3: Renomear `wwwroot/images/eventos/` → `wwwroot/images/viagens/`

- Executar `mv billet_2/billet_2/wwwroot/images/eventos billet_2/billet_2/wwwroot/images/viagens`.
- OU usar `git mv` para preservar o histórico no Git.

### Passo 4: Verificar o resultado

- Confirmar que `src/eventos/` NÃO existe mais.
- Confirmar que `src/viagens/` existe e contém `EventosController.cs`.
- Confirmar que `billet_2/billet_2/wwwroot/images/eventos/` NÃO existe mais.
- Confirmar que `billet_2/billet_2/wwwroot/images/viagens/` existe e contém as 3 imagens.
- Confirmar que `dotnet build` na solution compila sem novos erros (apenas warnings ou erros já existentes antes da renomeação).

---

## 8. Critérios de Aceitação

| ID | Critério | Verificação |
|:--:|----------|-------------|
| CA01 | O diretório `src/eventos/` NÃO existe mais | `! -d src/eventos` |
| CA02 | O diretório `src/viagens/` EXISTE | `-d src/viagens` |
| CA03 | `src/viagens/EventosController.cs` existe com o MESMO conteúdo do original | `diff src/viagens/EventosController.cs` (contra backup) retorna vazio |
| CA04 | O diretório `billet_2/billet_2/wwwroot/images/eventos/` NÃO existe mais | `! -d billet_2/billet_2/wwwroot/images/eventos` |
| CA05 | O diretório `billet_2/billet_2/wwwroot/images/viagens/` EXISTE | `-d billet_2/billet_2/wwwroot/images/viagens` |
| CA06 | As 3 imagens (`bonner.webp`, `show_rock.jpg`, `showrock.png`) estão em `images/viagens/` | Arquivos idênticos (hash) aos originais |
| CA07 | `dotnet build` na solution compila sem novos erros introduzidos por esta spec | Mesmo número de erros de antes da renomeação |

---

## 9. Riscos e Observações

### 9.1. Risco: Sistema operacional Windows (case-insensitive)

- **Mitigação:** Usar `git mv` em vez de `mv` para garantir que o Git rastreie a renomeação corretamente, independentemente do sistema de arquivos.
- No Git Bash do Windows, ambos `mv` e `git mv` funcionam para renomeação de diretórios.

### 9.2. Observação: Referências não atualizadas

- Conforme documentado na Seção 5, referências a `eventos` no código NÃO serão alteradas nesta spec.
- Isso NÃO causa erros de compilação porque:
  - O C# não impõe correspondência entre diretórios e namespaces sem declaração explícita.
  - As imagens referenciadas em `Meusingressos.razor` (`/images/eventos/...`) só quebrariam em runtime, mas a funcionalidade já está em transição para ser refatorada na Spec 0110.
- O build do projeto continuará funcionando.

### 9.3. Observação: Git

- Se estiver usando Git, utilizar `git mv` é RECOMENDADO para que o histórico de alterações dos arquivos seja preservado através da renomeação.
- A renomeação de diretórios via `git mv` é rastreada como rename no Git, não como delete+create.

---

## 10. Referências

| Documento | Caminho | Relevância |
|-----------|---------|------------|
| Roadmap da pivotagem | [`roadmap.md`](../roadmap.md) | Definição da Spec 0030 na Fase 0 |
| Visão da pivotagem | [`pivotagem.md`](../pivotagem.md) | Contexto geral da pivotagem (Seção 3: mudanças conceituais) |
| Arquitetura pivotada | [`arquitetura-pivotagem.md`](../arquitetura-pivotagem.md) | Estrutura de diretórios do TripPrime |
| Spec 0040 | `0040-modelo-controller-viagem.md` | Próxima spec, dependente desta |

---

> **Aprovado por:** Castor
> **Data de aprovação:** 31/05/2026
> **Data de implementação:** 31/05/2026
