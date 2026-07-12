# ADR-001: Escolha do Micro-ORM Dapper com Parâmetros Nomeados

**Status:** Aceito

**Data:** 28/05/2026

---

## Contexto

O projeto TripPrime necessita de uma camada de acesso a dados para PostgreSQL que permita executar consultas SQL de forma segura e eficiente. O sistema original utilizava listas em memória (`List<T>`), o que impedia a persistência de dados entre reinicializações da API. A escolha do ORM/Micro-ORM impacta diretamente a segurança (prevenção de SQL Injection), a performance e a produtividade da equipe de desenvolvimento.

As opções consideradas foram:

1. **Entity Framework Core** — ORM completo com change tracking, migrações e LINQ-to-SQL
2. **Dapper** — Micro-ORM que estende `IDbConnection` com métodos de extensão leves
3. **ADO.NET puro** — Acesso direto ao banco sem abstração adicional

## Decisão

1. **Entity Framework Core é proibido.** Nenhuma implementação poderá utilizar EF Core como ORM.
2. **Dapper é a biblioteca obrigatória** para acesso a dados (NuGet: `Dapper` + `Npgsql`).
3. **Todas as queries devem utilizar passagem de parâmetros nomeados com `@`**. É proibido concatenar valores em strings SQL ou usar interpolação para montar valores de parâmetros.

### Exemplo Correto

```csharp
// ✅ CORRETO: Dapper com parâmetros nomeados via @
var passagens = await connection.QueryAsync<PassagemDetalhada>(
    @"SELECT p.""Id"", v.""Origem"", v.""Destino""
      FROM ""Passagens"" p
      INNER JOIN ""Viagens"" v ON p.""ViagemId"" = v.""Id""
      WHERE p.""UsuarioCpf"" = @Cpf",
    new { Cpf = cpf }
);
```

### Exemplos Incorretos

```csharp
// ❌ PROIBIDO: Concatenação de string (SQL Injection)
var sql = "SELECT * FROM \"Passagens\" WHERE \"UsuarioCpf\" = '" + cpf + "'";

// ❌ PROIBIDO: Interpolação de valores
var sql = $"SELECT * FROM \"Passagens\" WHERE \"UsuarioCpf\" = '{cpf}'";
```

## Consequências

**Prós:**
- Queries SQL explícitas e legíveis — o desenvolvedor tem controle total sobre o SQL executado
- Performance próxima do ADO.NET puro — Dapper é significativamente mais rápido que EF Core
- Segurança garantida via parâmetros nomeados (`@`) — elimina riscos de SQL Injection
- Facilidade de depuração — basta copiar a query e executar diretamente no PostgreSQL
- Curva de aprendizado reduzida — requer apenas conhecimento de SQL, sem APIs complexas de ORM

**Contras:**
- Perda de produtividade em cenários CRUD simples — onde EF Core geraria o SQL automaticamente
- Necessidade de escrever SQL manualmente para todas as operações
- Sem validação em tempo de compilação para as queries SQL — erros de sintaxe SQL só aparecem em runtime
- Mapeamento manual entre resultados de queries e modelos de domínio — sem automatic change tracking

**Neutras:**
- O script DDL (`db/script.sql`) continua sendo o ponto único de verdade para o esquema do banco de dados
- As tabelas devem ser criadas manualmente via script SQL antes de serem acessadas pelo Dapper
