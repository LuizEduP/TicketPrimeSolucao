# Architecture Decision Records (ADR) — TicketPrime → TripPrime

> **Propósito:** Este documento registra as decisões arquiteturais significativas do projeto, sejam elas proibições, restrições, adoções de tecnologia ou definições de padrão. Cada ADR possui um identificador único e seu status atual (Aceito, Proposto, Depreciado, Substituído).
>
> **Formato:** Cada ADR segue a estrutura: Contexto → Decisão → Motivação → Consequências.

---

## ADR-001: Proibição do Entity Framework Core e Adoção do Dapper com Parâmetros

- **Status:** Aceito
- **Data:** 28/05/2026

### Contexto

O projeto necessita de uma camada de acesso a dados para PostgreSQL que substitua as listas em memória (`List<T>`) atualmente utilizadas pela API. Os documentos de arquitetura originais mencionavam Entity Framework Core e Dapper como opções equivalentes, sem definir qual adotar.

### Decisão

1. **Entity Framework Core é proibido.** Nenhuma implementação poderá utilizar EF Core como ORM.
2. **Dapper é a biblioteca obrigatória** para acesso a dados (NuGet: `Dapper` + `Npgsql`).
3. **Todas as queries devem utilizar passagem de parâmetros nomeados com `@`**. É proibido concatenar valores em strings SQL ou usar interpolação para montar valores de parâmetros.

#### Exemplo Correto

```csharp
// ✅ CORRETO: Dapper com parâmetros nomeados via @
var viagem = await connection.QueryFirstOrDefaultAsync<Viagem>(
    "SELECT * FROM \"Viagens\" WHERE \"Id\" = @Id",
    new { Id = id }
);
```

#### Exemplos Incorretos

```csharp
// ❌ PROIBIDO: Concatenação de string (SQL Injection)
var sql = "SELECT * FROM \"Viagens\" WHERE \"Id\" = " + id;

// ❌ PROIBIDO: Interpolação de valores
var sql = $"SELECT * FROM \"Viagens\" WHERE \"Id\" = {id}";
```

### Motivação

| Motivo | Descrição |
|--------|-----------|
| **Simplicidade** | Dapper é um micro-ORM que estende `IDbConnection` com métodos de extensão, sem necessidade de configuração de contexto, migrações ou mapeamento complexo |
| **Controle explícito** | O desenvolvedor escreve SQL puro e tem controle total sobre a query executada |
| **Performance** | Dapper é significativamente mais rápido que EF Core por ser um wrapper leve sobre ADO.NET |
| **Segurança** | O uso obrigatório de parâmetros nomeados (`@`) elimina riscos de SQL Injection |
| **Curva de aprendizado** | Dapper requer conhecimento de SQL, que é uma habilidade mais transferível que APIs específicas de ORMs |

### Consequências

**Positivas:**
- Queries SQL explícitas e legíveis
- Controle total sobre a execução no banco de dados
- Performance próxima do ADO.NET puro
- Facilidade de depuração (basta copiar a query e executar diretamente no PostgreSQL)
- Sem necessidade de aprender EF Core ou gerenciar migrações

**Negativas:**
- Perda de produtividade em cenários CRUD simples (onde EF Core geraria o SQL automaticamente)
- Necessidade de escrever SQL manualmente para todas as operações
- Sem validação em tempo de compilação para as queries SQL
- Mapeamento manual entre resultados de queries e modelos de domínio

**Neutras:**
- O script DDL (`db/script.sql`) continua sendo o ponto único de verdade para o esquema do banco de dados
- As tabelas devem ser criadas manualmente via script SQL antes de serem acessadas pelo Dapper

---

## ADR-002: [Disponível para nova decisão arquitetural]

---

## Referências

- [Dapper GitHub](https://github.com/DapperLib/Dapper)
- [Npgsql Documentation](https://www.npgsql.org/doc/)
- [`docs/arquitetura.md`](arquitetura.md) — Documento de arquitetura original
- [`docs/pivotagem/arquitetura-pivotagem.md`](pivotagem/arquitetura-pivotagem.md) — Documento de arquitetura da pivotagem
- [`docs/pivotagem/roadmap.md`](pivotagem/roadmap.md) — Roadmap de desenvolvimento
