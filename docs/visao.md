# Documento de Visão — TicketPrime

## 1. Introdução

### 1.1. Propósito

Este documento define a visão de alto nível do sistema **TicketPrime**, uma plataforma de venda de ingressos para eventos. Ele descreve o problema a ser resolvido, o público-alvo e as funcionalidades propostas. Este documento serve como guia para a equipe de desenvolvimento e stakeholders durante todo o ciclo de vida do sistema.

### 1.2. Escopo

O TicketPrime é um sistema web para venda de ingressos, abrangendo desde a visualização pública de eventos até a confirmação da compra. As principais áreas cobertas são:

- **Catálogo de eventos:** visualização pública, pesquisa e filtros.
- **Gerenciamento de usuários:** cadastro, login e recuperação de senha.
- **Compra de ingressos:** seleção, carrinho, aplicação de cupons e finalização.
- **Administração:** criação, edição e cancelamento de eventos; controle de capacidade; gerenciamento de cupons; visualização de vendas realizadas.

### 1.3. Definições, Acrônimos e Abreviações

| Termo | Definição |
|-------|-----------|
| **Cupom** | Código promocional que concede desconto percentual sobre o valor do ingresso |
| **Reserva** | Registro que associa um usuário a um evento, representando a compra de um ingresso |
| **Lote de ingressos** | Conjunto de ingressos de um evento com quantidade e preço específicos |
| **BDD** | *Behavior-Driven Development* — técnica de desenvolvimento orientada a comportamento |

---

## 2. Posicionamento

### 2.1. Oportunidade de Negócio

O mercado de eventos e entretenimento movimenta bilhões de reais anualmente no Brasil. Muitas plataformas de venda de ingressos cobram taxas elevadas ou oferecem experiências complexas para organizadores de eventos de pequeno e médio porte. O TicketPrime surge como uma alternativa simplificada, de código aberto e de fácil implantação, permitindo que qualquer organizador cadastre eventos, gerencie ingressos e ofereça descontos promocionais — tudo em uma única plataforma.

### 2.2. Problema

Organizadores de eventos de pequeno e médio porte enfrentam dificuldades para:

- Disponibilizar ingressos online de forma rápida e acessível.
- Controlar a capacidade de eventos e evitar superlotação.
- Oferecer descontos promocionais de forma controlada.
- Gerenciar reservas e visualizar a lista de participantes confirmados.

### 2.3. Solução

O TicketPrime resolve esses problemas oferecendo:

- Cadastro e gerenciamento de eventos com controle de capacidade.
- Venda de ingressos com limite de um ingresso por CPF por evento.
- Sistema de cupons de desconto com validação automática.
- Painel administrativo para visualização e confirmação de ingressos vendidos.
- Interface responsiva e intuitiva para usuários finais.

---

## 3. Descrição dos Stakeholders e Usuários

### 3.1. Stakeholders

| Stakeholder | Descrição |
|-------------|-----------|
| **Equipe de Desenvolvimento** | Responsável por planejar, implementar e testar o sistema |
| **Professor Avaliador** | Responsável por avaliar o projeto conforme critérios acadêmicos |
| **Organizadores de Eventos** | Potenciais usuários administradores que utilizarão a plataforma para vender ingressos |
| **Compradores de Ingressos** | Público geral que utilizará a plataforma para adquirir entradas para eventos |

### 3.2. Perfis de Usuário

| Perfil | Descrição | Necessidades Principais |
|--------|-----------|------------------------|
| **Visitante** | Usuário não cadastrado que acessa a plataforma | Visualizar eventos disponíveis; criar conta |
| **Usuário Cadastrado** | Usuário com conta ativa na plataforma | Comprar ingressos; visualizar ingressos adquiridos; aplicar cupons; reservar vagas |
| **Administrador** | Usuário com privilégios administrativos | Criar, editar e cancelar eventos; gerenciar lotes de ingressos; visualizar vendas |

---

## 4. Funcionalidades do Produto

### 4.1. Funcionalidades para Visitantes

| ID | Funcionalidade | Prioridade |
|----|---------------|------------|
| F01 | Visualizar lista de eventos disponíveis | Alta |
| F02 | Visualizar detalhes de um evento (data, local, preço, descrição) | Alta |
| F03 | Criar conta de usuário | Alta |
| F04 | Pesquisar eventos por nome | Média |
| F05 | Filtrar eventos por região, categoria ou data | Média |
| F06 | Compartilhar eventos em redes sociais | Baixa |

### 4.2. Funcionalidades para Usuários Cadastrados

| ID | Funcionalidade | Prioridade |
|----|---------------|------------|
| F07 | Realizar login na plataforma | Alta |
| F08 | Recuperar senha | Média |
| F09 | Adicionar ingressos ao carrinho de compras | Alta |
| F10 | Finalizar compra (checkout) | Alta |
| F11 | Aplicar cupom de desconto na compra | Média |
| F12 | Receber ingresso por e-mail | Baixa |
| F13 | Visualizar ingressos adquiridos ("Meus Ingressos") | Alta |
| F14 | Reservar vaga em eventos futuros | Média |

### 4.3. Funcionalidades para Administradores

| ID | Funcionalidade | Prioridade |
|----|---------------|------------|
| F15 | Criar novos eventos | Alta |
| F16 | Visualizar todos os eventos cadastrados | Alta |
| F17 | Cadastrar e liberar lotes de ingressos | Média |
| F18 | Cancelar eventos | Alta |
| F19 | Definir limite de ingressos por evento | Alta |
| F20 | Visualizar e confirmar ingressos vendidos | Alta |
| F21 | Limitar uso de cupons (um por ingresso) | Média |

---

## 5. Critérios de Sucesso

1. **Funcionalidades de alta prioridade operacionais:** As funcionalidades F01, F02, F03, F07, F09, F10, F13, F15, F16, F18, F19 e F20 devem estar implementadas e funcionando.
2. **Documentação entregue:** O repositório deve conter README com instruções de execução, documento de requisitos com histórias de usuário e cenários BDD, e este documento de visão.

---

## 6. Referências

- [`README.md`](../README.md) — Instruções de execução e visão geral do projeto
- [`docs/historiasdeusuario.md`](historiasdeusuario.md) — 24 histórias de usuário com cenários BDD
