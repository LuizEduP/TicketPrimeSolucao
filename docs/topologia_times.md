# Topologia de Times — TripPrime

> **Item AV2:** 20 — Mapeamento dos 4 tipos de time do Team Topologies

---

## Mapeamento Team Topologies para o Contexto do Projeto

O Team Topologies define 4 tipos fundamentais de time. Abaixo, mapeamos cada tipo para o contexto do TripPrime.

---

### Stream-aligned Team (Time Alinhado ao Fluxo de Valor)

**Definição:** Time responsável por entregar valor de ponta a ponta para um fluxo de negócio específico, com autonomia para construir, testar, deployar e operar seu próprio software.

**Aplicação no TripPrime:** O **Time de Vendas e Passagens** é o stream-aligned team principal. Este time é responsável por todo o fluxo de compra de passagens — desde a pesquisa de viagens (`GET /api/viagens/pesquisar`) até a finalização da compra (`POST /api/passagens/comprar`) e visualização de ingressos (`GET /api/passagens/usuario/{cpf}`).

**Composição:** 3 desenvolvedores full-stack (C# + Blazor) + 1 Product Owner

**Responsabilidades:**
- Implementar e manter os endpoints de Viagens, Passagens, Assentos e Veículos
- Desenvolver as páginas Blazor correspondentes (Home, Poslogin, Venda, Meusingressos)
- Garantir que o fluxo de compra funcione de ponta a ponta
- Monitorar o SLO da rota crítica (`POST /api/passagens/comprar`)

---

### Platform Team (Time de Plataforma)

**Definição:** Time que constrói e mantém uma plataforma interna (APIs, ferramentas, serviços) que aceleram o trabalho dos stream-aligned teams, reduzindo a carga cognitiva.

**Aplicação no TripPrime:** O **Time de Infraestrutura e Dados** atua como platform team. Este time construiu o `DbConnectionFactory` e mantém o script DDL do PostgreSQL (`db/script.sql`), abstraindo a complexidade de acesso a dados para que o stream-aligned team foque apenas em lógica de negócio.

**Composição:** 1 desenvolvedor backend sênior + 1 DevOps

**Responsabilidades:**
- Manter o `DbConnectionFactory` e garantir que o Dapper esteja configurado corretamente
- Gerenciar o esquema do banco de dados (migrações, índices, performance de queries)
- Configurar e manter o pipeline CI/CD (GitHub Actions)
- Fornecer documentação interna sobre como usar o Dapper com parâmetros `@`
- Manter o ADR-001 atualizado

---

### Enabling Team (Time de Habilitação)

**Definição:** Time temporário que ajuda stream-aligned teams a adquirir novas capacidades, superar obstáculos técnicos e adotar novas tecnologias, sem se tornar dono permanente do código.

**Aplicação no TripPrime:** O **Time de Qualidade e Engenharia de Software** atua como enabling team. Este time não implementa funcionalidades diretamente, mas capacita os stream-aligned teams em práticas de qualidade, testes automatizados e documentação.

**Composição:** 1 Tech Lead + 1 QA Engineer

**Responsabilidades:**
- Ensinar o time a escrever testes seguindo o padrão AAA (`// Arrange`, `// Act`, `// Assert`)
- Estabelecer a convenção de nomenclatura de testes (`Metodo_Cenario_ResultadoEsperado`)
- Capacitar o time na taxonomia de manutenção de Swanson (Corretiva, Adaptativa, Perfectiva, Preventiva)
- Produzir a documentação SDD (ADR, análise arquitetural, registro de dívida técnica) e transferir conhecimento para que o time mantenha esses documentos no futuro
- Realizar code reviews focados em qualidade e segurança

---

### Complicated-Subsystem Team (Time de Subsistema Complexo)

**Definição:** Time especializado que lida com subsistemas de alta complexidade técnica que exigem conhecimento profundo em domínios específicos (ex: criptografia, IA, processamento de vídeo), isolando essa complexidade dos demais times.

**Aplicação no TripPrime:** O **Time de Segurança** atua como complicated-subsystem team. Este time é responsável por todos os aspectos de segurança que exigem conhecimento especializado, isolando a complexidade de segurança dos stream-aligned teams.

**Composição:** 1 especialista em segurança da informação (consultor externo ou membro com formação em cybersegurança)

**Responsabilidades:**
- Realizar Threat Modeling nas rotas críticas (especialmente `POST /api/passagens/comprar`)
- Configurar e operar as ferramentas de SAST (SonarQube) e DAST (OWASP ZAP)
- Definir e aplicar os 3 Gates de Segurança (SAST no commit, Testes de segurança no PR, Revisão manual antes do deploy)
- Auditar o código em busca de credenciais hardcoded, SQL Injection e outros padrões de vulnerabilidade (OWASP Top 10)
- Responder a incidentes de segurança e conduzir post-mortems

---

## Diagrama de Interação entre Times

```
┌──────────────────────────────────────────────────────────────────┐
│                     TRIPPRIME — ORGANIZAÇÃO                       │
│                                                                   │
│  ┌─────────────────────┐      ┌─────────────────────────────┐    │
│  │ Stream-aligned Team  │      │     Platform Team            │    │
│  │ Vendas e Passagens   │─────▶│     Infraestrutura e Dados   │    │
│  │                      │      │                              │    │
│  │ • Viagens endpoints  │      │ • DbConnectionFactory        │    │
│  │ • Passagens flow     │      │ • PostgreSQL schema          │    │
│  │ • Assentos management│      │ • CI/CD pipeline             │    │
│  │ • Blazor pages       │      │ • ADR maintenance            │    │
│  └──────────┬───────────┘      └─────────────────────────────┘    │
│             │                                                      │
│             │ solicita capacitação                                 │
│             ▼                                                      │
│  ┌─────────────────────┐      ┌─────────────────────────────┐    │
│  │ Enabling Team        │      │ Complicated-Subsystem Team  │    │
│  │ Qualidade e ES       │      │ Segurança                   │    │
│  │                      │      │                              │    │
│  │ • Testes AAA pattern │      │ • Threat Modeling           │    │
│  │ • Nomenclatura testes│      │ • SAST/DAST tools           │    │
│  │ • Documentação SDD   │      │ • Security Gates 1-2-3      │    │
│  │ • Code review        │      │ • Incident response         │    │
│  └─────────────────────┘      └─────────────────────────────┘    │
│                                                                   │
│  Interações:                                                      │
│  ──▶ Consome (API/plataforma)                                     │
│  ─ ─▶ Capacita (ensino temporário)                                │
│  ─ ─▶ Audita (segurança)                                          │
└──────────────────────────────────────────────────────────────────┘
```

---

## Notas sobre a Aplicação no Contexto Acadêmico

No contexto da AV2 (projeto acadêmico com 6 integrantes), os papéis são adaptados:

- **Stream-aligned:** 3 integrantes (Gabriel Lepsch, Lucas Oliveira, Thiago Zandonade)
- **Platform:** 1 integrante (Gabriel Castor — responsável pelo banco e Dapper)
- **Enabling:** 1 integrante (Gabriel Ribeiro — Tech Lead, documentação SDD)
- **Complicated-Subsystem:** 1 integrante (Luiz Eduardo — segurança e revisão de código)

Todos os integrantes participam de code review e testes, independentemente do time atribuído.
