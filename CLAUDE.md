# Regras para Desenvolvimento por IA Autônoma

> Este documento é a fonte de autoridade máxima para o comportamento da IA.
> **Sempre leia este documento primeiro** antes de qualquer ação.
>
> ⚠️ **Todas as instruções neste documento foram escritas de forma explícita, estruturada e sem ambiguidades para que uma IA autônoma possa interpretar e executar corretamente.**
> ⚠️ **Sempre que escrever qualquer resposta, código, spec, comentário ou documentação, escreva de forma clara, explícita e estruturada para que outra IA (ou você mesmo no futuro) entenda sem ambiguidade.**

---

## 1. Princípios Fundamentais (em ordem de prioridade)

### 1.1 KISS (Keep It Simple, Stupid) — PRIORIDADE MÁXIMA
- Priorize soluções simples e diretas.
- Evite complexidade desnecessária em código, arquitetura e design.
- Se uma solução simples resolve o problema, ela é a escolha correta, mesmo que não seja a mais elegante.

### 1.2 DRY (Don't Repeat Yourself) — PRIORIDADE SECUNDÁRIA
- Evite duplicação de código sempre que possível.
- Extraia lógica repetida em funções, classes ou serviços reutilizáveis.
- Mantenha um único ponto de verdade para cada conceito no sistema.

### 1.3 KISS > DRY (Regra de Desempate)
- **SEMPRE** que KISS e DRY entrarem em conflito, KISS vence.
- Se aplicar DRY resultar em uma solução muito complexa ou demorada, opte pela repetição controlada em vez de uma abstração prematura.
- A simplicidade de manutenção é mais importante que a eliminação absoluta de duplicação.

---

## 2. Fluxo de Trabalho Obrigatório

### 2.1 Passo 1: Analisar Documentos Existente

Antes de qualquer ação, analise **TODOS** os documentos abaixo que existirem:

| Documento | Caminho | O que contém |
|-----------|---------|--------------|
| `visao.md` | [`TicketPrimeSolucao-main/docs/visao.md`](TicketPrimeSolucao-main/docs/visao.md) | Visão geral do projeto |
| `arquitetura.md` | [`TicketPrimeSolucao-main/docs/arquitetura.md`](TicketPrimeSolucao-main/docs/arquitetura.md) | Decisões arquiteturais |
| `adr.md` | [`TicketPrimeSolucao-main/docs/adr.md`](TicketPrimeSolucao-main/docs/adr.md) | Registro de Decisões Arquiteturais (ADR) |
| `licoes.md` | [`TicketPrimeSolucao-main/docs/licoes.md`](TicketPrimeSolucao-main/docs/licoes.md) | Lições aprendidas |
| `roadmap.md` | [`TicketPrimeSolucao-main/docs/roadmap.md`](TicketPrimeSolucao-main/docs/roadmap.md) | Roadmap do projeto |
| `pivotagem.md` | [`TicketPrimeSolucao-main/docs/pivotagem/pivotagem.md`](TicketPrimeSolucao-main/docs/pivotagem/pivotagem.md) | Visão da pivotagem (TripPrime) |

**Regras:**
- Se o documento **existir**: leia e considere seu conteúdo nas decisões.
- Se o documento **não existir**: ignore e prossiga normalmente (não crie nem peça).

### 2.1.1 Regra de Pivotagem — Proteção dos Documentos Originais

**NUNCA altere os documentos originais do projeto TicketPrime**, especificamente:
- [`TicketPrimeSolucao-main/docs/visao.md`](TicketPrimeSolucao-main/docs/visao.md)
- [`TicketPrimeSolucao-main/docs/arquitetura.md`](TicketPrimeSolucao-main/docs/arquitetura.md)
- [`TicketPrimeSolucao-main/docs/historiasdeusuario.md`](TicketPrimeSolucao-main/docs/historiasdeusuario.md)

**Motivo:** O projeto passou por uma pivotagem (TicketPrime → TripPrime). Os documentos originais representam o estado anterior do projeto e devem ser preservados como referência histórica.

**Regras obrigatórias:**
1. **Documentos do diretório `/pivotagem` são a fonte de verdade** para todo o desenvolvimento futuro do TripPrime.
2. **Consulte** [`TicketPrimeSolucao-main/docs/pivotagem/pivotagem.md`](TicketPrimeSolucao-main/docs/pivotagem/pivotagem.md) como o documento de visão vigente.
3. **Crie novos documentos** (specs, arquitetura pivotada, histórias de usuário pivotadas) dentro do diretório [`TicketPrimeSolucao-main/docs/pivotagem/`](TicketPrimeSolucao-main/docs/pivotagem/).
4. **NUNCA edite, mova ou remova** os documentos originais listados acima.

### 2.2 Passo 2: Verificar se Spec é Necessária

**TODA E QUALQUER alteração no projeto REQUER uma spec.**
Isso inclui:
- Novas funcionalidades
- Correções de bugs (bug fixes)
- Hotfixes críticos
- Refatoração que altera comportamento
- Qualquer modificação no código, documentação ou configuração

**Única exceção (NÃO precisa de spec):**
- Ajustes de formatação, estilo ou linting que NÃO alteram lógica nem comportamento

**Dúvida?** Se não tiver certeza se precisa de spec, **PARE e pergunte ao usuário**.

### 2.3 Passo 3: Criar Spec

Se uma spec é necessária e não existe:

1. Analise **todos** os documentos do Passo 2.1, incluindo obrigatoriamente o [`roadmap.md`](TicketPrimeSolucao-main/docs/roadmap.md).
2. Crie o documento de spec no diretório [`TicketPrimeSolucao-main/docs/`](TicketPrimeSolucao-main/docs/).
3. A numeração segue duas regras:

   **Regra A — Spec original (nova funcionalidade):**
   - Numerada de **10 em 10**: `0010`, `0020`, `0030`, etc.
   - Exemplo: `0010-cadastro-usuario.md`

   **Regra B — Spec de correção (bug fix / hotfix em spec existente):**
   - Numerada a **partir do número da spec original**, incrementando de **1 em 1**.
   - Exemplo: A spec `0010-cadastro-usuario.md` apresentou erro. A spec de correção será `0011-correcao-cadastro-usuario.md`. Se ainda não resolver, a próxima será `0012-correcao-cadastro-usuario.md`, e assim por diante.
   - O nome do arquivo DEVE conter o prefixo `correcao-` para identificação clara.

4. **Após criar a spec, PARE e apresente ao usuário para aprovação.**
5. **NÃO implemente nada até o usuário aprovar a spec.**

### 2.4 Passo 4: Implementação (apenas com spec aprovada)

- Implementar **apenas** funcionalidades que possuem spec aprovada pelo usuário.
- Seguir **estritamente** o que foi especificado no documento de spec.
- Manter aderência aos princípios KISS e DRY durante a implementação.
- Se durante a implementação descobrir algo que foge da spec, **PARE e avise o usuário**.

---

## 3. Comportamento Geral da IA

### 3.1 Ao Iniciar uma Nova Tarefa
1. **LEIA ESTE DOCUMENTO (CLAUDE.md)** — você está lendo agora.
2. Leia os documentos do Passo 2.1 que existirem.
3. Determine se a tarefa requer spec (Passo 2.2).
4. Se requer spec e não existe → crie a spec (Passo 2.3) e peça aprovação.
5. Se requer spec e já existe → implemente seguindo a spec (Passo 2.4).
6. Se é formatação/estilo/linting (única exceção) → implemente diretamente seguindo KISS e DRY.

### 3.2 Escreva Sempre de Forma Clara para uma IA Entender

**Toda comunicação escrita neste projeto (código, comentários, docs, specs, mensagens) deve seguir estas regras:**

- **Seja explícito, não implícito** — Não use subentendidos, ironia ou linguagem figurativa. Diga exatamente o que quer dizer.
- **Estruture com seções e listas** — Use títulos, subtítulos, bullets e numeração para organizar ideias.
- **Evite ambiguidades** — Palavras como "depois", "em breve", "eventualmente" são proibidas. Use prazos ou condições exatas.
- **Prefira frases curtas e diretas** — Uma ideia por frase. Máximo de 20 palavras por frase.
- **Use marcadores de ênfase explícitos** — `OBRIGATÓRIO`, `PROIBIDO`, `PARE`, `SEMPRE`, `NUNCA` para ações críticas.
- **Inclua exemplos concretos** — Quando descrever um padrão, mostre um exemplo de código ou texto.
- **Defina o contexto primeiro** — Antes de dar uma instrução, explique **por que** ela existe.
- **Preveja ambiguidades** — Se uma instrução pode ser interpretada de duas formas, já escreva a exceção ou o desempate junto.

**Exemplo do que NÃO fazer:**
> "Faça a validação dos campos depois de verificar o usuário."

**Exemplo do que fazer:**
> "1. Primeiro, verifique se o usuário existe no banco (consulta `UsuarioService.ObterPorId`).
> 2. Após a verificação do passo 1 retornar sucesso, então valide os campos do formulário.
> 3. Se o passo 1 falhar (usuário não existe), retorne erro 404 imediatamente — NÃO prossiga para o passo 2."

### 3.3 Quando Estiver em Dúvida
- **PARE** e pergunte ao usuário. Não assuma.
- Se duas interpretações são possíveis, peça esclarecimento.
- Se uma instrução parece contradizer este documento, peça esclarecimento.

### 3.4 Localização dos Diretórios do Projeto

| Diretório | Caminho | Conteúdo |
|-----------|---------|----------|
| Documentação | [`TicketPrimeSolucao-main/docs/`](TicketPrimeSolucao-main/docs/) | Documentos de visão, arquitetura, specs |
| Banco de Dados | [`TicketPrimeSolucao-main/db/`](TicketPrimeSolucao-main/db/) | Scripts SQL |
| API | [`TicketPrimeSolucao-main/src/`](TicketPrimeSolucao-main/src/) | Código fonte da API (.NET) |
| Testes | [`TicketPrimeSolucao-main/tests/`](TicketPrimeSolucao-main/tests/) | Testes unitários |
| Web App (Blazor) | [`TicketPrimeSolucao-main/billet_2/`](TicketPrimeSolucao-main/billet_2/) | Aplicação Blazor |

---

## 4. Resumo Visual do Fluxo

```
[Início da Tarefa]
       │
       ▼
[Ler CLAUDE.md] ◄──── OBRIGATÓRIO
       │
       ▼
[Analisar docs existentes: visao.md, arquitetura.md, adr.md, licoes.md, roadmap.md]
       │
       ▼
┌─── É formatação/estilo/linting? ───┐
│    (única exceção)                  │
│                                     │
├─ Sim ─► Implementar diretamente     │
│         (KISS + DRY)                │
│                                     │
├─ Não ─► Spec existe? ──────────────┤
│          │                          │
│     ├─ Sim ─► Seguir spec para      │
│     │         implementar           │
│     │                               │
│     └─ Não ─► Criar spec           │
│               │                     │
│               ▼                     │
│    ┌─── É funcionalidade nova? ───┐│
│    │    (Regra A)                  ││
│    ├─ Sim ─► Numerar de 10 em 10  ││
│    │         0010, 0020, 0030...   ││
│    │                               ││
│    └─ Não ─► É correção de bug     ││
│               em spec existente?   ││
│               (Regra B)            ││
│               ├─ Sim ─► Numerar    ││
│               │         a partir   ││
│               │         da spec    ││
│               │         original   ││
│               │         0011,0012  ││
│               │         (incremento││
│               │         de 1 em 1) ││
│               │                    ││
│               └─ Não ─► PARE e     ││
│                          pergunte  ││
│                          ao usuário││
│                                    ││
│               ▼                    ││
│    [Apresentar spec ao usuário]    ││
│               │                    ││
│    ┌─── Usuário aprovou? ───┐     ││
│    │                         │     ││
│    ├─ Sim ───────────────────┘     ││
│    └─ Não ─► PARE (não            ││
│               implementar)         ││
│                                    ││
└────────────────────────────────────┘
```
