# Plano de Operação — TripPrime

> **Itens AV2:** 12 (Matriz de Riscos), 13 (Gatilhos de Risco), 14 (Métrica de Fluxo DORA), 15 (Métrica de Qualidade), 16 (SLO) e 17 (Error Budget Policy)

---

## Matriz de Riscos Operacionais

| Risco | Probabilidade | Impacto | Gatilho | Estratégia | Ação Planejada |
|:---|:---:|:---:|:---|:---|:---|
| Indisponibilidade do banco de dados PostgreSQL em produção | Médio | Alto | O health check do PostgreSQL retorna status `down` por mais de 60 segundos consecutivos — o endpoint `/api/health` começa a retornar 503 Service Unavailable. | Mitigar | Configurar replicação primário-standby com failover automático via Patroni. Manter snapshots diários para recuperação point-in-time. Testar o procedimento de failover trimestralmente. |
| Vazamento de dados de usuários (CPF, e-mail) por ausência de criptografia em trânsito | Baixo | Alto | Scanner de segurança (OWASP ZAP) detecta tráfego HTTP sem TLS na rota `/api/usuarios/*` — alerta é gerado no canal #security-alerts do Slack com severidade crítica. | Mitigar | Habilitar HTTPS estrito via `app.UseHttpsRedirection()` já implementado no `Program.cs`. Configurar HSTS com `max-age=31536000; includeSubDomains`. Adicionar certificado TLS válido no ambiente de produção. |
| Ataque de negação de serviço (DDoS) no endpoint de pesquisa de viagens | Médio | Médio | O número de requisições para `/api/viagens/pesquisar` excede 100 requisições por segundo por IP — alarme do Cloudflare/NGINX dispara no canal #ops-alerts com threshold de 3 minutos consecutivos acima do limite. | Mitigar | Implementar rate limiting por IP (100 req/min) usando middleware ASP.NET Core. Configurar CDN/WAF (Cloudflare) com regras de rate limiting. Adicionar cache de 60 segundos para pesquisas idênticas (origem + destino + data). |
| Perda total de dados por ausência de backups automatizados | Alto | Alto | O script de backup agendado falha por 2 execuções consecutivas — o cron job de backup gera logs de erro `BACKUP_FAILED` que são enviados para o canal #db-alerts, e nenhum backup bem-sucedido é registrado no bucket S3 por mais de 48 horas. | Evitar | Configurar backup automatizado diário do PostgreSQL via `pg_dump` com retenção de 30 dias no S3. Implementar validação de integridade do backup (restore teste semanal em ambiente de staging). Monitorar via script que verifica idade do último backup (alerta se > 26h). |
| Queda de performance no endpoint de JOIN de passagens com crescimento de dados | Alto | Baixo | O tempo de resposta p95 do endpoint `GET /api/passagens/detalhadas` ultrapassa 500ms — o APM (Application Performance Monitoring) envia alerta quando a latência p95 excede o SLO definido de 500ms por mais de 5 minutos consecutivos, indicando degradação progressiva da performance da consulta com JOIN. | Aceitar | A degradação de performance em JOIN com crescimento de dados é esperada. Aceita-se o risco no curto prazo. Monitorar tendência de latência no Grafana. Se p95 ultrapassar 2000ms, escalar para Prioridade 1 com plano de ação: adicionar índices compostos, particionar tabelas ou migrar para materialized views. |

---

## Métrica de Fluxo (DORA): Lead Time for Changes

**Nome da Métrica:** Lead Time for Changes (Tempo de Entrega de Mudanças)

**O que Mede:** O tempo decorrido entre o primeiro commit de uma feature/correção (commit inicial na branch) até o deploy bem-sucedido em produção. Mede a agilidade do pipeline de entrega e a capacidade da equipe de entregar valor rapidamente.

**Fórmula:** `Lead Time = DataHora_Deploy_Produção - DataHora_Primeiro_Commit`

**Fonte de Dados:**
- Data do primeiro commit: Git log (`git log --reverse --format=%ci` no primeiro commit da branch)
- Data do deploy: Logs do pipeline CI/CD (GitHub Actions / Azure DevOps — timestamp do job `deploy-production` concluído com sucesso)

**Frequência de Coleta:** A cada deploy concluído em produção ( evento-driven, não agendado)

**Limites de Saúde:**
- 🟢 Saudável: Lead Time < 24 horas (elite, segundo DORA Accelerate State of DevOps)
- 🟡 Atenção: Lead Time entre 24h e 1 semana
- 🔴 Crítico: Lead Time > 1 semana (low performer)

**Ação se Violado:** Se o Lead Time médio das últimas 10 entregas ultrapassar 1 semana, realizar uma retrospectiva de pipeline para identificar gargalos (filas de code review, ambientes de staging indisponíveis, testes lentos). Ações típicas: reduzir tamanho dos PRs, paralelizar estágios do pipeline, automatizar smoke tests.

---

## Métrica de Qualidade: Change Failure Rate

**Nome da Métrica:** Change Failure Rate (Taxa de Falha em Mudanças)

**O que Mede:** A porcentagem de deploys em produção que resultam em falha (incidente, rollback, hotfix corretivo imediatamente após o deploy). Mede a estabilidade das entregas e a eficácia do processo de qualidade.

**Fórmula:** `Change Failure Rate = (Número de deploys que resultaram em falha / Número total de deploys no período) × 100`

**Fonte de Dados:**
- Deploys totais: Logs do pipeline CI/CD (todos os jobs `deploy-production` concluídos)
- Deploys com falha: Sistema de incidentes (PagerDuty, OpsGenie ou planilha manual) — qualquer deploy seguido de rollback ou hotfix em até 24h é contado como falha

**Frequência de Coleta:** Semanal (toda segunda-feira, analisando a semana anterior)

**Limites de Saúde:**
- 🟢 Saudável: Change Failure Rate < 15% (elite, segundo DORA)
- 🟡 Atenção: Change Failure Rate entre 15% e 30%
- 🔴 Crítico: Change Failure Rate > 30% (low performer)

**Ação se Violado:** Se a taxa de falha exceder 30% por 2 semanas consecutivas, instituir um "Change Freeze" temporário: proibir deploys de novas features por 1 semana, focando exclusivamente em:
1. Revisar os 5 últimos deploys que falharam e documentar causa raiz
2. Adicionar testes de regressão para os cenários que falharam
3. Implementar canary releases e feature toggles para reduzir o blast radius de deploys futuros
4. Revisar o processo de code review (exigir 2 approvals em vez de 1)

---

## SLO (Service Level Objective) — Rota Crítica

### Rota Crítica: `POST /api/passagens/comprar`

Esta é a rota mais crítica do sistema, responsável por finalizar a compra de passagens. Uma falha nesta rota impede que usuários adquiram ingressos, resultando em perda direta de receita.

**SLI (Indicador):** Proporção de requisições bem-sucedidas (HTTP 200 OK) em relação ao total de requisições ao endpoint `POST /api/passagens/comprar` em uma janela de medição.

**Fórmula de Coleta:**
```
SLI = (Requisições com status 200 / Total de requisições ao endpoint) × 100
```
Excluem-se do denominador requisições que retornam 400 Bad Request (erros de validação do cliente), pois estas representam comportamento esperado e não falha do serviço. Incluem-se no denominador apenas requisições que passaram pela validação de entrada e atingiram a lógica de negócio (status 200, 404, 500, 503).

**Fonte do Dado:** Logs de aplicação do ASP.NET Core (Application Insights / Serilog / stdout estruturado). Métrica exposta via `/metrics` no formato Prometheus com middleware `prometheus-net`.

**Janela de Medição:** 7 dias (168 horas)

**Alvo (SLO):** 99.5% das requisições devem retornar sucesso (HTTP 200)

---

## Error Budget Policy

Com base no SLO de 99.5%, o Error Budget mensal é de **0.5%** — ou seja, o serviço pode ficar indisponível ou retornar erro por até aproximadamente **3.6 horas por mês** (0.5% × 720 horas).

A política de Error Budget define ações graduadas conforme o orçamento de erro é consumido:

### Nivel 1: Consumo ≤ 50% do Error Budget (≤ 1.8h de downtime no mês)

**Situação:** Operação normal. O serviço está dentro dos parâmetros esperados.

**Ações:**
- Continuar deploys normais de features e correções
- Nenhuma restrição adicional
- Revisão mensal padrão do SLO na retrospectiva de sprint

---

### Nivel 2: Consumo > 50% e ≤ 80% do Error Budget (1.8h a 2.88h de downtime)

**Situação:** Alerta amarelo. O consumo está acima do esperado para o período.

**Ações:**
- Notificar Tech Lead e Product Owner sobre o consumo acelerado
- Suspender deploys de features de baixa prioridade (apenas correções críticas e features de alta prioridade são permitidas)
- Realizar post-mortem das últimas 3 falhas para identificar padrão
- Reforçar code review e exigir 2 approvals para qualquer deploy

---

### Nivel 3: Consumo > 80% do Error Budget (> 2.88h de downtime) — Feature Freeze

**Situação:** Alerta vermelho. O orçamento de erro está quase esgotado. Zero novas funcionalidades são permitidas até a próxima janela de medição.

**Ações:**
- **Feature Freeze total:** Nenhuma feature nova pode ser deployada. Congelamento de novas funcionalidades.
- Apenas hotfixes de segurança (vulnerabilidades críticas) e correções de bugs que causam incidentes em produção são permitidos
- Todo o time de desenvolvimento é redirecionado para:
  - Escrever testes de regressão automatizados
  - Corrigir débitos técnicos de estabilidade (DT-001 e DT-002)
  - Implementar melhorias de observabilidade (logging, métricas, tracing)
- Reunião diária de war room (30 min) liderada pelo Tech Lead para acompanhar o consumo do budget
- O freeze só é levantado na virada do mês (reset do Error Budget) OU quando uma análise de causa raiz demonstrar que as falhas foram resolvidas e o serviço está estável por 72h consecutivas
