# Correção TicketPrime (TicketPrimeSolucao) — AV1

## Resumo

| Avaliação | Nota |
| --- | --- |
| **AV1** | **8 / 10** |

---

## AV1 — Detalhamento

| Item | Critério | Resultado | Justificativa |
| --- | --- | --- | --- |
| 1 | `docs/requisitos.md` com pelo menos 3 blocos contendo `Como`, `Quero` e `Para` | ✅ 1 | O arquivo existe como `docs/historiasdeusuario.md` (nome aceitável como variante de requisitos) e traz 24 histórias de usuário no formato `Como <papel>, quero <ação> para <objetivo>`. Exemplo (ID 02): "Como visitante, quero criar uma conta na plataforma para acessar os benefícios de usuário cadastrado e poder comprar ingressos." |
| 2 | `docs/requisitos.md` com pelo menos 1 cenário contendo `Dado`, `Quando` e `Então` | ✅ 1 | O mesmo arquivo possui a seção "BDD das Histórias de Usuário" com 24 cenários estruturados em `Dado: ... / Quando: ... / Então: ...`. Exemplo (13): "Dado que estou logado e já comprei um ingresso com meu CPF / Quando tentar comprar outro ingresso / Então o sistema deve bloquear a compra e exibir uma mensagem informativa." |
| 3 | `README.md` com blocos de código Markdown contendo comandos de terminal (ex: `dotnet run`, `dotnet build`) | ✅ 1 | O README traz dois blocos ```bash distintos com `cd src; dotnet run` (API) e `cd billet_2/billet_2; dotnet run` (frontend Blazor), além de um bloco de estrutura de pastas. |
| 4 | Pasta `/db` com arquivo `.sql` contendo `CREATE TABLE` | ❌ 0 | A pasta `/db` contém apenas um arquivo chamado literalmente `sql` (sem extensão). O conteúdo tem `CREATE TABLE` para `Usuarios`, `Eventos`, `Cupons` e `Reservas`, mas o critério exige explicitamente arquivo **com extensão `.sql`**. Como a regra é estrita (0 ou 1), o item zera. **Correção trivial:** renomear `db/sql` para `db/script.sql` (ou equivalente). |
| 5 | `/src` com arquivos `.cs` contendo `app.MapGet` ou `app.MapPost` | ✅ 1 | Os controladores em `src/usuarios/UsuariosController.cs`, `src/eventos/EventosController.cs` e `src/cupons/CuponsController.cs` registram as rotas com `app.MapGet("/api/usuarios/listar", ...)`, `app.MapPost("/api/usuarios/cadastrar", ...)`, `app.MapGet("/api/eventos/listar", ...)`, `app.MapGet("/api/eventos/listar/{id}", ...)`, `app.MapPost("/api/eventos/cadastrar", ...)`, `app.MapGet("/api/cupons/listar", ...)` e `app.MapPost("/api/cupons/cadastrar", ...)`. |
| 6 | `/src` com retornos explícitos de `Results.BadRequest` ou `Results.NotFound` | ✅ 1 | Fail-fast presente nos controladores. Exemplos: `Results.BadRequest("O cpf deve ter 11 caracteres")`, `Results.BadRequest("A senha deve ter pelo menos 6 caracteres")`, `Results.BadRequest("O cpf informado já está cadastrado")`, `Results.NotFound("Evento não encontrado.")`, `Results.BadRequest("A data do evento não pode ser antiga, coloque uma data futura!")`. |
| 7 | Uso do caractere `@` nas strings de query Dapper | ❌ 0 | O projeto **não usa Dapper** (nem qualquer outro acesso a banco de dados). Buscas por `Dapper`, `SqlConnection`, `NpgsqlConnection`, `Execute` e `Query` em `src/**/*.cs` não retornaram ocorrências. Os dados são armazenados em listas em memória (`List<Usuario>`, `List<Evento>`, `List<Cupons>`). Sem queries SQL no código, não há como encontrar parâmetros `@`. O próprio README reconhece essa limitação: "O banco de dados não está integrado ao código. A API utiliza listas em memória em substituição a um banco real." |
| 8 | Não usar `+` nem interpolação `$"{ }"` em comandos `SELECT/INSERT/UPDATE/DELETE` | ✅ 1 | Regra negativa: as buscas por `$"...SELECT/INSERT/UPDATE/DELETE"` e por concatenação com `+` em strings SQL não retornaram ocorrências. Como não existem comandos SQL no código, nenhum padrão proibido foi encontrado, e o item é pontuado por ausência. |
| 9 | `/tests` com `.cs` contendo `[Fact]` ou `[Theory]` | ✅ 1 | `tests/` contém 5 arquivos de teste: `TesteReservaVazia.cs` e `TesteReservaValida.cs` e `TestePrecoPositivo.cs` (com `[Fact]`), `TesteEventoCapacidade.cs` e `TesteDescontoValido.cs` (com `[Theory]` + `[InlineData]`). Todos importam `using Xunit;`. |
| 10 | `Assert.` dentro dos métodos de teste | ✅ 1 | Todos os 5 métodos de teste possuem `Assert.False(...)` ao final do padrão Arrange-Act-Assert. Nenhum teste sem Assert. |

**Total AV1: 8 / 10**

---

## Justificativa da nota final

O projeto **TicketPrimeSolucao** entrega uma base sólida de documentação e estrutura, mas perde pontos em dois itens críticos que foram abertamente reconhecidos pelos próprios alunos no README ("limitações conhecidas" e "TODO"):

### Pontos fortes (8 itens OK)

- **Documentação de requisitos bem estruturada** em `docs/historiasdeusuario.md`, com 24 histórias de usuário no formato canônico e 24 cenários BDD correspondentes.
- **README executável**, com blocos `bash` claros para subir API (`http://localhost:5289`) e frontend Blazor (`http://localhost:5096`), além de exemplos de payload para cada endpoint.
- **Minimal API organizada por módulos** (`usuarios`, `eventos`, `cupons`), com rotas `MapGet`/`MapPost` e validações fail-fast retornando `BadRequest` e `NotFound` para regras como CPF inválido, senha curta, CPF duplicado, data passada e evento inexistente.
- **Testes xUnit** presentes em 5 arquivos, cobrindo reserva vazia, reserva com valor final, preço do evento, capacidade do evento e desconto do cupom. Todos com `[Fact]`/`[Theory]` e `Assert.`.

### Pontos perdidos (2 itens zerados)

- **Item 4 (0 pt) — Script SQL sem extensão.** O arquivo existe em `db/sql` com todo o DDL necessário (`Usuarios`, `Eventos`, `Cupons`, `Reservas` com FKs e índices), mas foi salvo sem a extensão `.sql`. O critério é binário e exige explicitamente a extensão. **Correção trivial:** renomear para `db/script.sql`.
- **Item 7 (0 pt) — Ausência de Dapper.** A API inteira armazena dados em listas em memória. Não há `SqlConnection`, `NpgsqlConnection`, Dapper nem qualquer query parametrizada. O README já declara essa limitação: "O banco de dados não está integrado ao código. A API utiliza listas em memória". Sem queries SQL, não há como cumprir o critério de encontrar parâmetros `@` no código. Para recuperar esse ponto, seria necessário integrar o `db/sql` ao código via Dapper com PostgreSQL.

### Observação sobre o item 8

O item 8 é uma regra negativa ("NÃO encontrar concatenação ou interpolação em SQL"). Como o projeto não possui SQL algum no código, nenhum padrão proibido foi encontrado e o item foi pontuado por ausência de violação. Vale notar que esse ponto fica em zona cinza: em uma leitura mais exigente (que pressuponha que o item só é aplicável quando há Dapper), o projeto poderia perder também esse critério, o que levaria a nota para 7/10.

**Nota final AV1: 8 / 10.**
