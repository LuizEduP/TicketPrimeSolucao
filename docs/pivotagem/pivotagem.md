# Documento de Visão da Pivotagem — TicketPrime → TripPrime

## 1. Resumo Executivo

Este documento formaliza a **pivotagem** do projeto **TicketPrime** — originalmente uma plataforma de venda de ingressos para shows e eventos — para o **TripPrime**, uma plataforma de comercialização de assentos em transportes para excursões.

A decisão de pivotar foi tomada pelo grupo após análise de mercado, viabilidade técnica e alinhamento com os objetivos de aprendizado da disciplina. A mudança preserva a maior parte da lógica e do código existente, redirecionando o domínio de negócio de **eventos culturais** para **viagens e excursões**.

---

## 2. Motivação da Pivotagem

### 2.1. Contexto Original

O TicketPrime foi concebido como um sistema web para venda de ingressos de eventos (shows, festivais, palestras). Sua estrutura contemplava:

- Cadastro e gerenciamento de eventos
- Compra de ingressos com limite por CPF
- Aplicação de cupons de desconto
- Reserva de vagas
- Painel administrativo

### 2.2. Razões para a Pivotagem

| Motivo | Descrição |
|--------|-----------|
| **Mercado nichado e sazonal** | O mercado de eventos presenciais é altamente sazonal e foi fortemente impactado por crises sanitárias e econômicas. |
| **Diferencial competitivo** | O mercado de excursões e transporte rodoviário de passageiros carece de plataformas modernas e de código aberto para gestão de assentos. |
| **Aproveitamento do código existente** | Cerca de 80% da lógica do TicketPrime (CRUD de entidades, controle de capacidade, reservas, cupons) é diretamente reutilizável no domínio de transportes. |
| **Complexidade realista** | A pivotagem introduz desafios relevantes (seleção de assento em mapa visual, controle de ocupação por poltrona, rotas com múltiplos trechos) que enriquecem o escopo acadêmico do projeto. |
| **Alinhamento pedagógico** | A disciplina exige a aplicação de conceitos de engenharia de software (BDD, documentação, testes). A pivotagem mantém todos esses requisitos, adicionando camadas de complexidade valiosas. |

### 2.3. Oportunidade de Negócio

O transporte rodoviário para excursões escolares, universitárias, corporativas e de lazer movimenta bilhões de reais anualmente no Brasil. As plataformas existentes são, em sua maioria, sistemas legados ou soluções fechadas. O **TripPrime** surge como uma alternativa moderna, de código aberto e de fácil implantação, permitindo que agências de viagem, transportadoras e organizadores de excursões gerenciem assentos, reservas e vendas online.

---

## 3. Principais Mudanças Conceituais

| Conceito TicketPrime (Antigo) | Conceito TripPrime (Novo) |
|------------------------------|---------------------------|
| Evento (show, festival) | Viagem / Excursão (rota com origem, destino, data/horário) |
| Ingresso (entrada para evento) | Passagem (assento no transporte) |
| Lote de ingressos | Classe de assentos (convencional, executivo, leito) |
| Setor (VIP, Normal) | Tipo de poltrona (janela, corredor, leito) |
| Capacidade do evento | Capacidade do veículo (número total de poltronas) |
| Reserva de vaga | Reserva de assento (poltrona específica) |
| Carrinho de compras | Checkout de passagens |
| Data do evento | Data e horário de partida / chegada |

---

## 4. Conceitos do Novo Domínio

### 4.1. Viagem / Excursão

Uma **viagem** (ou excursão) representa um deslocamento programado entre uma **origem** e um **destino**, em uma **data e horário** específicos, utilizando um **veículo** com capacidade definida. A origem e o destino são informados diretamente no cadastro da viagem.

### 4.2. Assento / Poltrona

Cada veículo possui um **mapa de assentos** numerados. Cada assento pode estar em um dos seguintes estados:

- **Disponível** — livre para reserva ou compra
- **Reservado** — temporariamente reservado (prazo expirável)
- **Vendido** — compra confirmada
- **Indisponível** — assento bloqueado (ex.: assento do motorista, avaria)

Os assentos podem ter tipos diferentes (janela, corredor, leito) e preços que variam conforme o tipo.

### 4.3. Passagem

A **passagem** é o equivalente ao ingresso no domínio anterior. Representa a compra de um assento específico em uma viagem. Cada passagem possui um status que indica se está ativa, cancelada ou já foi utilizada.

### 4.5. Veículo

Representa o transporte utilizado na viagem. Cada veículo possui um modelo, capacidade total de assentos e um tipo (ônibus convencional, micro-ônibus, van, executivo, leito).

### 4.6. Cupom de Desconto

Mantido do domínio original, com expansão para incluir valor mínimo para aplicação, controle de validade (ativo/inativo) e data de expiração.

---

## 5. Funcionalidades do Novo Produto

### 5.1. Funcionalidades para Visitantes

| ID | Funcionalidade | Prioridade |
|----|---------------|------------|
| F01 | Visualizar lista de viagens disponíveis (origem, destino, data, preço) | Alta |
| F02 | Visualizar detalhes de uma viagem (roteiro, horários, preço) | Alta |
| F03 | Criar conta de usuário | Alta |
| F04 | Pesquisar viagens por origem, destino ou data | Alta |
| F05 | Filtrar viagens por destino, faixa de preço ou horário | Média |

### 5.2. Funcionalidades para Usuários Cadastrados

| ID | Funcionalidade | Prioridade |
|----|---------------|------------|
| F06 | Realizar login na plataforma | Alta |
| F07 | Visualizar mapa interativo de assentos e selecionar poltrona | Alta |
| F08 | Reservar assento temporariamente (com prazo de expiração) | Alta |
| F09 | Finalizar compra da passagem (checkout) | Alta |
| F10 | Aplicar cupom de desconto na compra | Média |
| F11 | Visualizar passagens adquiridas ("Minhas Passagens") | Alta |
| F12 | Cancelar passagem (com regras de reembolso) | Média |
| F13 | Receber passagem por e-mail | Baixa |

### 5.3. Funcionalidades para Administradores

| ID | Funcionalidade | Prioridade |
|----|---------------|------------|
| F14 | Cadastrar veículos (modelo, capacidade, tipo) | Alta |
| F15 | Criar novas viagens (origem, destino, data, veículo, preço) | Alta |
| F16 | Gerenciar mapa de assentos (bloquear/liberar poltronas) | Alta |
| F17 | Visualizar todas as viagens cadastradas | Alta |
| F18 | Cancelar viagens | Alta |
| F19 | Visualizar e confirmar passagens vendidas | Alta |
| F20 | Gerenciar cupons de desconto | Média |
| F21 | Definir preços diferenciados por tipo de assento | Média |

---

## 6. Fluxo de Compra (Novo)

```
Visitante
    │
    ├──> Pesquisa viagens (origem, destino, data)
    │
    ├──> Visualiza detalhes da viagem
    │       ├── Roteiro (origem → paradas → destino)
    │       ├── Horários (partida e chegada)
    │       └── Preço base
    │
    ├──> [Login / Cadastro]
    │
    ├──> Visualiza mapa de assentos
    │       ├── Assentos disponíveis
    │       ├── Assentos reservados
    │       ├── Assentos vendidos
    │       └── Assentos indisponíveis
    │
    ├──> Seleciona assento(s)
    │       └── Aplica cupom de desconto (opcional)
    │
    ├──> Reserva temporária (prazo para pagamento)
    │
    └──> Finaliza compra
            └──> Confirmação
```

---

## 7. Aproveitamento do Código Existente

| Componente Original | Componente Pivotado | Aproveitamento |
|--------------------|--------------------|:--------------:|
| Evento | Viagem | ~70% |
| Usuário | Usuário (sem alterações) | 100% |
| Cupom | Cupom (expandido) | ~50% |
| Reserva | Passagem | ~60% |
| Páginas de listagem (Home, Login, Cadastro) | Adaptadas para viagens | ~80% |
| Página de venda (Venda.razor) | Mapa de assentos + seleção | ~40% |
| Página de ingressos (Meusingressos.razor) | Minhas Passagens | ~70% |
| Testes xUnit | Adaptados para o novo domínio | ~80% |

---

## 8. Novos Componentes a Desenvolver

| Componente | Descrição | Prioridade |
|-----------|-----------|:----------:|
| **Mapa de Assentos Interativo** | Interface visual que renderiza o layout do veículo com poltronas clicáveis | Alta |
| **Gerenciamento de Veículos** | Cadastro e manutenção de veículos com definição de layout de assentos | Alta |
| **Reserva Temporária** | Lógica de reserva com expiração automática | Alta |
| **Cancelamento de Passagem** | Regras de cancelamento e estorno | Média |

---

## 9. Critérios de Sucesso da Pivotagem

1. **Funcionalidades essenciais operacionais:** As funcionalidades F01, F02, F03, F06, F07, F08, F09, F11, F14, F15, F17 e F19 devem estar implementadas e funcionando.
2. **Mapa de assentos funcional:** O componente de seleção visual de assentos deve permitir visualizar ocupação e selecionar poltronas.
3. **Reserva temporária:** O sistema deve reservar o assento por um prazo determinado, liberando-o automaticamente se o pagamento não for concluído.
4. **Documentação atualizada:** README, documento de visão e demais artefatos devem refletir o novo domínio.
5. **Testes adaptados:** Testes existentes devem ser adaptados para o novo domínio, e novos testes devem cobrir seleção de assento e reserva temporária.

---

## 10. Referências

- [`docs/visao.md`](../visao.md) — Documento de visão original (TicketPrime)
- [`docs/arquitetura.md`](../arquitetura.md) — Documento de arquitetura original
- [`docs/historiasdeusuario.md`](../historiasdeusuario.md) — Histórias de usuário originais
- [`docs/pivotagem/pivotagem.md`](pivotagem.md) — **Este documento**
