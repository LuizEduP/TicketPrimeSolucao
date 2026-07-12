# Spec 0010 — Renomear `db/sql` para `db/script.sql` e Corrigir Índice Inválido

## Metadados

| Campo | Valor |
|-------|-------|
| **Código** | 0010 |
| **Fase** | 0 — Preparação e Correções |
| **Tipo** | Correção (bug fix) |
| **Prioridade** | 🔴 Alta |
| **Status** | Concluído |
| **Dependências** | Nenhuma |
| **Dependentes** | Spec 0190 (Script SQL da pivotagem) |
| **Estimativa** | 15 minutos |
| **Documento de referência** | [`roadmap.md`](../roadmap.md) — Seção 3, Fase 0 |

---

## 1. Objetivo

Realizar DUAS correções críticas no diretório de banco de dados do projeto:

1. **Renomear** o arquivo `db/sql` para `db/script.sql` — o arquivo atual não possui extensão `.sql`.
2. **Corrigir** o índice inválido na linha 64 do arquivo `db/sql`: a coluna referenciada `DataEvento` NÃO existe na tabela `Eventos`. A coluna correta é `"Data"`.

---

## 2. Motivação

### 2.1. Item 4 da AV1 zerado — Arquivo sem extensão `.sql`

- O arquivo `db/sql` contém DDL completo com `CREATE TABLE` para `Usuarios`, `Eventos`, `Cupons` e `Reservas`.
- O critério de avaliação exige **explicitamente** que o arquivo tenha extensão `.sql`.
- A ausência da extensão resultou em **0 pontos** no item 4 da AV1 (nota: 8/10).
- **Fonte:** [`CORRECAO.md`](../../CORRECAO.md), linha 18: "A pasta `/db` contém apenas um arquivo chamado literalmente `sql` (sem extensão). (...) o critério exige explicitamente arquivo com extensão `.sql`."

### 2.2. Índice inválido — Coluna `DataEvento` não existe

- A linha 64 do arquivo `db/sql` contém:
  ```sql
  CREATE INDEX idx_eventos_data ON Eventos(DataEvento);
  ```
- A tabela `Eventos` (linhas 16-25 do mesmo arquivo) NÃO possui a coluna `DataEvento`.
- A coluna de data na tabela `Eventos` é `"Data"` (tipo `TIMESTAMP`, linha 21).
- Se este script for executado contra um banco PostgreSQL, o comando **falhará com erro**, pois a coluna referenciada não existe.

---

## 3. Estado Atual (ANTES)

### 3.1. Estrutura do diretório `db/`

```
db/
└── sql          ← Arquivo sem extensão .sql (ERRO)
```

### 3.2. Conteúdo atual do arquivo `db/sql` (linha 64)

```sql
CREATE INDEX idx_eventos_data ON Eventos(DataEvento);
```

### 3.3. Definição atual da tabela `Eventos` (linhas 16-25)

```sql
CREATE TABLE IF NOT EXISTS "Eventos" (
    "Id" SERIAL PRIMARY KEY,
    "Nome" VARCHAR(255) NOT NULL,
    "Descricao" TEXT,
    "Local" VARCHAR(255),
    "Data" TIMESTAMP NOT NULL,           ← Coluna de data é "Data"
    "QuantidadeIngressos" INT NOT NULL,
    "ValorIngresso" REAL NOT NULL,
    "FotoUrl" TEXT
);
```

**Problema confirmado:** A coluna `DataEvento` referenciada no índice da linha 64 **não existe**. O nome correto é `"Data"`.

---

## 4. Estado Desejado (DEPOIS)

### 4.1. Estrutura do diretório `db/`

```
db/
└── script.sql   ← Arquivo renomeado COM extensão .sql (CORRETO)
```

### 4.2. Conteúdo da linha 64 (corrigido)

```sql
CREATE INDEX idx_eventos_data ON "Eventos"("Data");
```

**Mudanças aplicadas:**
- Nome da tabela `Eventos` → `"Eventos"` (com aspas duplas, para consistência com a definição da tabela na linha 16 que usa `"Eventos"` com aspas duplas)
- Nome da coluna `DataEvento` → `"Data"` (nome real da coluna, com aspas duplas para consistência com a definição)

---

## 5. Arquivos Afetados

| Arquivo | Operação | Descrição |
|---------|:--------:|-----------|
| `db/sql` | **EXCLUIR** | Arquivo original sem extensão será removido |
| `db/script.sql` | **CRIAR** | Novo arquivo, idêntico ao original, com o nome correto e a linha 64 corrigida |

**NENHUM outro arquivo do projeto é afetado por esta spec.**

---

## 6. Passos de Execução

### Passo 1: Ler o arquivo `db/sql` completo

- Abrir o arquivo `db/sql` para leitura.
- Armazenar o conteúdo completo em memória.

### Passo 2: Corrigir a linha 64

- Localizar a linha que contém `CREATE INDEX idx_eventos_data ON Eventos(DataEvento);`
- Substituir por `CREATE INDEX idx_eventos_data ON "Eventos"("Data");`
- **NÃO alterar nenhuma outra linha do arquivo.**
- **NÃO alterar formatação, espaçamento ou quebras de linha das demais linhas.**

### Passo 3: Escrever o novo arquivo `db/script.sql`

- Criar o arquivo `db/script.sql` com o conteúdo corrigido do Passo 2.
- O encoding deve ser **UTF-8** (mesmo encoding do arquivo original).

### Passo 4: Remover o arquivo antigo `db/sql`

- Excluir o arquivo `db/sql`.

### Passo 5: Verificar o resultado

- Confirmar que `db/script.sql` existe e contém `CREATE INDEX idx_eventos_data ON "Eventos"("Data");` na linha 64.
- Confirmar que `db/sql` NÃO existe mais.
- Confirmar que o conteúdo do novo arquivo é idêntico ao original, exceto pela alteração da linha 64.

---

## 7. Critérios de Aceitação

| ID | Critério | Verificação |
|:--:|----------|-------------|
| CA01 | O arquivo `db/sql` NÃO existe mais no diretório `db/` | `! -f db/sql` |
| CA02 | O arquivo `db/script.sql` EXISTE no diretório `db/` | `-f db/script.sql` |
| CA03 | O arquivo `db/script.sql` possui extensão `.sql` | Nome termina com `.sql` |
| CA04 | A linha 64 de `db/script.sql` contém `CREATE INDEX idx_eventos_data ON "Eventos"("Data");` | Grep pela string exata |
| CA05 | A linha 64 de `db/script.sql` NÃO contém `DataEvento` | Grep negativo |
| CA06 | O conteúdo do arquivo (exceto linha 64) é IDÊNTICO ao original `db/sql` | Diff entre original (com linha 64 corrigida) e novo arquivo mostra apenas a alteração da linha 64 |
| CA07 | O script SQL continua sintaticamente válido para PostgreSQL | O índice referencia uma coluna que existe na tabela `"Eventos"` |

---

## 8. Riscos e Observações

### 8.1. Risco: Perda acidental de conteúdo

- **Mitigação:** Ler o conteúdo completo do arquivo original ANTES de excluí-lo. Em caso de falha na escrita, o conteúdo está preservado na memória.

### 8.2. Observação: Encoding

- O arquivo original `db/sql` usa encoding UTF-8. O novo arquivo DEVE manter o mesmo encoding.
- NÃO adicionar BOM (Byte Order Mark) se o original não possuir.

### 8.3. Observação: Consistência de aspas duplas

- A tabela `Eventos` é criada com aspas duplas: `"Eventos"` (linha 16).
- A tabela `Usuarios` é criada com aspas duplas: `"Usuarios"` (linha 6).
- A tabela `Reservas` é criada SEM aspas duplas: `Reservas` (linha 35).
- A tabela `Cupons` é criada SEM aspas duplas: `Cupons` (linha 27).
- **Decisão:** Para esta spec, corrigimos APENAS o nome da coluna no índice. A consistência de nomenclatura (aspas duplas vs. sem aspas) entre as tabelas será tratada na Spec 0190 (Script SQL da pivotagem), que reestruturará todo o DDL.
- A adição de aspas duplas em `"Eventos"` no índice é feita para consistência MÍNIMA com a definição da própria tabela na linha 16.

---

## 9. Referências

| Documento | Caminho | Relevância |
|-----------|---------|------------|
| Arquivo SQL original | `db/sql` | Arquivo a ser renomeado e corrigido |
| CORRECAO.md | [`CORRECAO.md`](../../CORRECAO.md) | Detalhamento da nota AV1 (item 4 zerado) |
| Roadmap da pivotagem | [`roadmap.md`](../roadmap.md) | Definição da Spec 0010 na Fase 0 |
| Visão da pivotagem | [`pivotagem.md`](../pivotagem.md) | Contexto geral da pivotagem |

---

> **Aprovado por:** Castor
> **Data de aprovação:** 29/05/2026
> **Data de implementação:** 29/05/2026
