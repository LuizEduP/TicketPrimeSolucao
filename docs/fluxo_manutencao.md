# Fluxo de Manutenção — TripPrime

> **Itens AV2:** 08 (Classificação de Manutenção — Taxonomia de Swanson) e 09 (Pipeline de Liberação Segura)

---

## Parte 1: Classificação de Manutenção (Taxonomia de Swanson)

Classifique cada um dos 12 tickets abaixo usando a taxonomia de Swanson: **Corretiva**, **Adaptativa**, **Perfectiva** ou **Preventiva**.

| # | Ticket | Classificação |
|:---|:---|:---|
| Ticket 1 | API retorna erro 500 ao tentar cadastrar usuário com CPF duplicado. | **Corretiva** |
| Ticket 2 | Migrar o banco de dados de PostgreSQL 15 para PostgreSQL 17. | **Adaptativa** |
| Ticket 3 | Adicionar filtro por data no endpoint de pesquisa de viagens (o endpoint atual pesquisa apenas por origem/destino). | **Perfectiva** |
| Ticket 4 | Frontend quebra ao abrir a tela de login no Safari 17 — erro de JavaScript não tratado. | **Corretiva** |
| Ticket 5 | Refatorar `PassagensController` para extrair a lógica de cálculo de desconto para um serviço separado, reduzindo duplicação com cupons. | **Perfectiva** |
| Ticket 6 | Atualizar o target framework do projeto de .NET 10 para .NET 11 quando lançado. | **Adaptativa** |
| Ticket 7 | Corrigir vulnerabilidade de SQL Injection encontrada no endpoint de pesquisa (conciliação de string detectada). | **Corretiva** |
| Ticket 8 | Adicionar índice no banco de dados na coluna `Viagens.DataPartida` após análise de slow queries mostrar que 80% das buscas filtram por data. | **Preventiva** |
| Ticket 9 | Substituir a biblioteca de envio de e-mail SMTP obsoleta (descontinuada pelo fornecedor) por uma nova API REST de e-mail transacional. | **Adaptativa** |
| Ticket 10 | Implementar rate limiting no endpoint de login para prevenir ataques de força bruta, antes que qualquer incidente seja reportado. | **Preventiva** |
| Ticket 11 | O comprovante de compra exibe o valor do ingresso sem o desconto do cupom aplicado — o valor mostrado está incorreto. | **Corretiva** |
| Ticket 12 | Adicionar paginação ao endpoint `GET /api/viagens/listar` que atualmente retorna todas as viagens de uma vez, causando timeout quando há mais de 1000 registros. | **Preventiva** |

---

## Parte 2: Pipeline de Liberação Segura

Descreva o pipeline de liberação em segurança para um ticket de correção (ex: Ticket 7 — SQL Injection no endpoint de pesquisa).

### Ticket: Corrigir vulnerabilidade de SQL Injection no endpoint de pesquisa de viagens

---

### 1. Análise de Impacto

Antes de qualquer alteração no código, a equipe deve realizar uma análise de impacto documentada:

- **Escopo da vulnerabilidade:** Identificar todos os endpoints que utilizam concatenação ou interpolação de strings em comandos SQL. No caso do TripPrime, verificar `ViagensController.PesquisarViagens()` e qualquer outro endpoint que construa queries dinâmicas.
- **Dados expostos:** Determinar quais tabelas e colunas podem ser acessadas via injeção. A tabela `Viagens` contém dados de viagens (origem, destino, datas) que são públicos, mas um ataque bem-sucedido poderia escalar para outras tabelas como `Usuarios` (CPF, email, senha).
- **Sistemas afetados:** API Backend (`src/`), Frontend Blazor (`billet_2/`), Banco PostgreSQL.
- **Usuários impactados:** Todos os usuários da plataforma — a correção exige deploy da API, resultando em breve interrupção (rolling update para zero downtime).
- **Regressão potencial:** A refatoração do endpoint de pesquisa para usar Dapper com parâmetros pode alterar sutilmente o comportamento da busca (ex: case sensitivity, ordenação). Todos os cenários de pesquisa existentes precisam ser retestados.

**Documento gerado:** `docs/security/impact-analysis-001.md`

---

### 2. Teste como Instrumento Cirúrgico

Testes são a ferramenta principal para garantir que a correção não introduz regressões:

- **Teste de regressão (antes da correção):** Escrever um teste de integração que reproduza a vulnerabilidade — enviar um payload malicioso (`' OR '1'='1`) e verificar se o endpoint retorna mais dados do que deveria (prova de que a vulnerabilidade existe).
- **Teste de correção (após a correção):** Após migrar para Dapper com `@Parametro`, o mesmo teste deve passar a retornar resultados vazios ou erro controlado (prova de que a vulnerabilidade foi eliminada).
- **Teste de funcionalidade:** Escrever 3 testes que cubram os cenários de pesquisa existentes:
  - Pesquisa por origem: `GET /api/viagens/pesquisar?origem=Rio`
  - Pesquisa por destino: `GET /api/viagens/pesquisar?destino=São Paulo`
  - Pesquisa por data: `GET /api/viagens/pesquisar?data=2026-07-12`
- **Teste de segurança negativa:** Enviar payloads de SQL injection conhecidos e verificar que todos retornam resultados vazios ou erros 400.

**Ferramenta:** xUnit + `Microsoft.AspNetCore.TestHost` para testes de integração.

---

### 3. Feature Toggle

Para mitigar o risco de regressão em produção, a correção será implantada atrás de um Feature Toggle:

- **Nome do toggle:** `feature.use-dapper-search`
- **Estratégia:** O endpoint `PesquisarViagens` terá dois caminhos de execução:
  - **Caminho A (legado):** Executa a lógica antiga com LINQ em memória (mantido como fallback)
  - **Caminho B (novo):** Executa a consulta via Dapper com parâmetros nomeados (`@Origem`, `@Destino`, `@Data`)
- **Ativação progressiva:**
  1. **10% dos usuários** (canary release) — monitorar por 2 horas
  2. **50% dos usuários** — monitorar por 4 horas
  3. **100% dos usuários** — rollout completo
- **Métrica de health:** Taxa de erro 5xx e latência p95 do endpoint
- **Rollback automático:** Se a taxa de erro exceder 1% no grupo canário, o toggle reverte automaticamente para o Caminho A

---

### 4. Estratégia de Release e Regressão

A release segue o fluxo GitFlow com ambientes segregados:

1. **Branch `hotfix/sql-injection-pesquisa`** criada a partir de `main`
2. **Code Review** obrigatório por pelo menos 2 membros da equipe — foco em segurança (ausência de concatenação/interpolação em SQL)
3. **Pipeline CI/CD:**
   - Build + Restore de pacotes NuGet
   - Execução da suíte completa de testes (unitários + integração)
   - Análise estática de segurança (SAST) — busca por padrões de concatenação SQL
   - Deploy no ambiente de **Staging**
4. **Validação em Staging:**
   - Execução de smoke tests automatizados (todos os endpoints respondem 200)
   - Teste manual de segurança: tentar SQL Injection nos endpoints
   - Validação de regressão visual no frontend Blazor
5. **Deploy em Produção:**
   - Feature Toggle ativado inicialmente para 10% (canary)
   - Monitoramento por 2 horas (Datadog/Prometheus)
   - Aumento progressivo: 50% → 100%
   - Após 24h sem incidentes, remover o código legado (Caminho A) e o Feature Toggle em um PR de limpeza
6. **Plano de rollback:** Caso a taxa de erro 5xx ultrapasse 1% ou latência p95 aumente 2x, reverter o Feature Toggle para 0% (Caminho A legado para todos os usuários). O rollback leva menos de 1 minuto (alteração de configuração, sem deploy).
