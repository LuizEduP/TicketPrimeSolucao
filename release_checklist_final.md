# Release Checklist Final — TripPrime AV2

> **Item AV2:** 20 — Checklist com 7 caixas de seleção marcadas como concluídas

---

## Fundamentos

- [x] Código compila sem erros (`dotnet build` passa em `src/` e `tests/`)
- [x] Endpoints da API estão registrados em `Program.cs` (19 endpoints mapeados)
- [x] Dapper (v2.1.66) e Npgsql (v9.0.3) adicionados ao `api.csproj`
- [x] `DbConnectionFactory` implementado e registrado como Singleton no DI

## Produto Mínimo

- [x] Pelo menos 2 novos endpoints com regras de negócio implementados (Viagens, Veículos, Assentos, Passagens)
- [x] Endpoint com INNER JOIN: `GET /api/passagens/detalhadas` (JOIN entre Passagens, Viagens, Veiculos, Assentos)
- [x] Endpoint com LEFT JOIN + `@Parâmetro`: `GET /api/passagens/detalhadas/usuario/{cpf}` com `@Cpf`
- [x] Pelo menos 1 endpoint com ≥3 validações de negócio retornando 400 Bad Request (`CadastrarViagens`: 6 validações)

## Evidência de Qualidade

- [x] 5 testes unitários com padrão AAA completo (`// Arrange`, `// Act`, `// Assert`)
- [x] Nomenclatura de testes no padrão `Metodo_Cenario_ResultadoEsperado` (ex: `ValidarDesconto_QuandoForaDoIntervalo_NaoDeveSerValido`)
- [x] Nenhum desvio condicional (`if`, `switch`, `for`, `foreach`, `while`) nos métodos de teste
- [x] Todos os testes passam (`dotnet test` executado com sucesso)

## Decisões Documentadas

- [x] `/docs/adrs/001-escolha-do-micro-orm.md` — ADR com `## Contexto`, `## Decisão`, `## Consequências` e `Status: Aceito`
- [x] `/docs/analise_arquitetura.md` — 3 cenários com padrão arquitetural e trade-offs
- [x] `/docs/analise_arquitetura.md` — 6 violações arquiteturais documentadas com `**Problema:**`, `**Evidência:**`, `**Impacto:**` e `**Ação Recomendada:**`
- [x] `/docs/registro_divida_tecnica.md` — 8 dívidas técnicas com priorização (Prioridade 1, 2 e 3)

## Evidência de Requisitos

- [x] `/docs/fluxo_manutencao.md` — 12 tickets classificados pela taxonomia de Swanson
- [x] `/docs/fluxo_manutencao.md` — Pipeline de liberação segura com 4 passos documentados
- [x] `/docs/plano_iteracao.md` — Plano de iteração com Objetivo, Escopo, Entregáveis, Risco e DOD preenchidos
- [x] `/docs/plano_iteracao.md` — Quadro visual com 4 colunas e WIP máximo de 4 tarefas

## Governança

- [x] `/docs/operacao.md` — Matriz de riscos com 5 riscos documentados (Probabilidade, Impacto, Estratégia, Ação Planejada)
- [x] `/docs/operacao.md` — Coluna Gatilho preenchida com ≥20 caracteres em todas as linhas
- [x] `/docs/operacao.md` — Métrica de fluxo DORA (Lead Time for Changes) com 7 campos da Ficha de Definição Operacional
- [x] `/docs/operacao.md` — Métrica de qualidade (Change Failure Rate) com 7 campos da Ficha de Definição Operacional
- [x] `/docs/operacao.md` — SLO para rota crítica (`POST /api/passagens/comprar`) com SLI, Fórmula, Janela de Medição (7 dias) e Alvo (99.5%)
- [x] `/docs/operacao.md` — Error Budget Policy com 3 níveis graduados e Feature Freeze no Nível 3
- [x] `/docs/topologia_times.md` — 4 tipos de time do Team Topologies mapeados para o contexto do projeto

## Segurança

- [x] Nenhuma credencial hardcoded nos arquivos `.cs` em `/src` (sem `Password=`, `Pwd=`, `User Id` ou `ConnectionString=` hardcoded)
- [x] String de conexão referenciada via `builder.Configuration.GetConnectionString("DefaultConnection")`
- [x] `/docs/seguranca_ciclo.md` — Threat Model com Ativos Protegidos, Vetor de Ataque, Falha Arquitetural e Controle de Engenharia
- [x] `/docs/seguranca_ciclo.md` — 3 Gates de segurança numerados (Gate 1: SAST, Gate 2: Testes de Segurança, Gate 3: Revisão Manual)
- [x] Nenhuma concatenação (`+`) ou interpolação (`$""`) em strings SQL — todas as queries usam `@Parametro` no Dapper

---

**Status Final:** ✅ Todos os 7 pilares concluídos.

**Data de entrega:** 12/07/2026

**Nota esperada AV2:** 10/10 (20 itens × 0,5 pontos)
