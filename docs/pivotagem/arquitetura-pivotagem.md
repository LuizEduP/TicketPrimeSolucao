# Documento de Arquitetura da Pivotagem — TicketPrime → TripPrime

> **Propósito:** Este documento descreve exclusivamente as **alterações arquiteturais** decorrentes da pivotagem do TicketPrime (venda de ingressos para eventos) para o TripPrime (comercialização de assentos em transportes para excursões).  
> **Pré-requisito de leitura:** [`docs/arquitetura.md`](../arquitetura.md) — documento de arquitetura original, que contém a stack tecnológica completa, padrões de implementação e demais informações que **não foram alteradas** pela pivotagem.

---

## 1. Visão Geral da Arquitetura Pivotada

A arquitetura de **duas camadas (2-Tier)** é mantida: frontend Blazor Web App comunica-se via HTTP com a ASP.NET Core Minimal API. O que muda é o **domínio de negócio** e, consequentemente, as entidades, endpoints, modelos de dados e componentes de interface.

```
┌──────────────────────────────────────────────────────────────────────────┐
│                           NAVEGADOR (Cliente)                             │
│                                                                           │
│  ┌──────────────────────────────────────────────────────────────────┐   │
│  │                    Blazor Web App (Frontend)                       │   │
│  │                    http://localhost:5096                           │   │
│  │                                                                     │   │
│  │  ┌────────────────────────────┐  ┌────────────────────────────┐   │   │
│  │  │   Interactive Server        │  │  WebAssembly (WASM)        │   │   │
│  │  │   (billet_2)                │  │  (billet_2.Client)         │   │   │
│  │  │   • Home (viagens)          │  │  • Páginas de exemplo      │   │   │
│  │  │   • Mapa de assentos        │  │    (não funcionais)        │   │   │
│  │  │   • Checkout de passagens   │  │                             │   │   │
│  │  │   • Minhas Passagens        │  │                             │   │   │
│  │  └────────────────────────────┘  └────────────────────────────┘   │   │
│  └────────────────────────────┬───────────────────────────────────────┘   │
└───────────────────────────────┼───────────────────────────────────────────┘
                                │  HTTP (CORS)
                                ▼
┌──────────────────────────────────────────────────────────────────────────┐
│              ASP.NET Core Minimal API (Backend) — PIVOTADA                │
│              http://localhost:5289                                         │
│                                                                           │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐ │
│  │ Usuários      │  │  Viagens     │  │  Veículos    │  │  Cupons      │ │
│  │ Controller    │  │  Controller  │  │  Controller  │  │  Controller  │ │
│  │ (sem         │  │  (novo)      │  │  (novo)      │  │  (expandido) │ │
│  │  alterações)  │  └──────┬───────┘  └──────┬───────┘  └──────┬───────┘ │
│  └──────┬───────┘         │                 │                 │         │
│         │                 │                 │                 │         │
│         └────────┬────────┴────────┬────────┴────────┬────────┘         │
│                  ▼                 ▼                 ▼                   │
│        ┌──────────────────────────────────────────────────────────┐    │
│        │              Listas em Memória (List<T>)                   │    │
│        │      (Sem persistência — mesma limitação do original)      │    │
│        └──────────────────────────────────────────────────────────┘    │
│                                                                           │
│  ┌──────────────────────────────────────────────────────────────────┐   │
│  │  NOVOS Endpoints:                                                 │   │
│  │  • GET       /api/viagens/listar                                 │   │
│  │  • GET       /api/viagens/listar/{id}                            │   │
│  │  • GET       /api/viagens/pesquisar?origem=&destino=&data=       │   │
│  │  • POST      /api/viagens/cadastrar                              │   │
│  │  • GET       /api/veiculos/listar                                │   │
│  │  • GET       /api/veiculos/listar/{id}                           │   │
│  │  • POST      /api/veiculos/cadastrar                             │   │
│  │  • GET       /api/passagens/listar                               │   │
│  │  • GET       /api/passagens/usuario/{cpf}                        │   │
│  │  • POST      /api/passagens/comprar                              │   │
│  │  • POST      /api/passagens/cancelar/{id}                        │   │
│  │  • GET       /api/assentos/viagem/{viagemId}                     │   │
│  │  • POST      /api/assentos/reservar                              │   │
│  │  • POST      /api/assentos/liberar                               │   │
│  │  • POST      /api/assentos/bloquear                              │   │
│  └──────────────────────────────────────────────────────────────────┘   │
└──────────────────────────────────────────────────────────────────────────┘
```

---

## 2. Mudanças na Stack Tecnológica

A stack tecnológica **permanece idêntica** à documentada em [`arquitetura.md`](../arquitetura.md#2-stack-tecnológica). Não há alteração de runtime, frameworks, bibliotecas ou ferramentas.

| Componente | Status | Observação |
|-----------|--------|-----------|
| .NET 10 | ✅ Mantido | Mesmo runtime |
| ASP.NET Core Minimal API | ✅ Mantido | Backend |
| Blazor Web App (Interactive Server + WASM) | ✅ Mantido | Frontend |
| Bootstrap 5 | ✅ Mantido | Estilização |
| xUnit + .NET Test SDK | ✅ Mantido | Testes |
| PostgreSQL + Dapper | ⏳ Planejado | Ver [`ADR-001`](../adr.md) — EF Core proibido |

---

## 3. Mudanças na Estrutura de Diretórios

A estrutura de diretórios é **expandida** com novas pastas para refletir o novo domínio. Nenhuma pasta existente foi removida.

```
TicketPrimeSolucao-main/
│
├── src/                             ← BACKEND: API (PIVOTADO)
│   ├── Program.cs                   ← NOVAS rotas registradas
│   ├── eventos/                     ← RENOMEAR → viagens/ (ou manter como está)
│   │   └── EventosController.cs     ← REFATORADO → ViagensController.cs
│   ├── veiculos/                    ← NOVO
│   │   └── VeiculosController.cs    ← NOVO
│   ├── passagens/                   ← NOVO
│   │   └── PassagensController.cs   ← NOVO
│   ├── assentos/                    ← NOVO
│   │   └── AssentosController.cs    ← NOVO
│   ├── usuarios/                    ← Mantido (sem alterações)
│   └── cupons/                      ← Mantido (expandido)
│
├── billet_2/billet_2/               ← FRONTEND: Blazor (PIVOTADO)
│   ├── Models/
│   │   ├── Evento.cs                ← RENOMEAR → Viagem.cs
│   │   ├── Usuario.cs               ← Mantido
│   │   ├── Veiculo.cs               ← NOVO
│   │   ├── Passagem.cs              ← NOVO
│   │   └── Assento.cs               ← NOVO
│   ├── Services/
│   │   ├── EventoService.cs         ← RENOMEAR → ViagemService.cs
│   │   ├── UsuarioService.cs        ← Mantido
│   │   ├── VeiculoService.cs        ← NOVO
│   │   ├── PassagemService.cs       ← NOVO
│   │   └── AssentoService.cs        ← NOVO
│   ├── Components/Pages/
│   │   ├── Home.razor               ← REFATORADO (viagens no lugar de eventos)
│   │   ├── Poslogin.razor           ← REFATORADO
│   │   ├── Venda.razor              ← SUBSTITUÍDO por MapaAssentos.razor
│   │   ├── Meusingressos.razor      ← REFATORADO → MinhasPassagens.razor
│   │   ├── Criarevento.razor        ← REFATORADO → CriarViagem.razor
│   │   ├── CriarVeiculo.razor       ← NOVO
│   │   └── ... demais páginas mantidas
│   └── wwwroot/images/eventos/      ← RENOMEAR → wwwroot/images/viagens/
│
├── db/sql                           ← ATUALIZADO (novas tabelas)
│
└── tests/                           ← ATUALIZADO (novos testes)
```

---

## 4. Mudanças no Backend — API

### 4.1. Mapeamento de Entidades (Antigas → Novas)

| Entidade Original (TicketPrime) | Entidade Pivotada (TripPrime) | Tipo de Mudança |
|-------------------------------|------------------------------|:--------------:|
| `Evento` | `Viagem` | Renomeação + novos campos |
| `Usuario` | `Usuario` | **Sem alterações** |
| `Cupons` | `Cupons` | **Sem alterações** (já contemplava campos expandidos no script original) |
| *(não existia)* | `Veiculo` | **Nova entidade** |
| *(não existia)* | `Passagem` | **Nova entidade** (substitui Reserva) |
| *(não existia)* | `Assento` | **Nova entidade** |

### 4.2. Novos Modelos de Dados (API)

#### `Viagem` (substitui `Evento`)

| Campo | Tipo | Restrições | Diferença do Original |
|-------|------|-----------|:---------------------:|
| `Id` | `int` | Auto-incremento | ✅ Mantido |
| `Origem` | `string` | Obrigatório | 🔄 **Novo** (substitui `Nome` como título) |
| `Destino` | `string` | Obrigatório | 🔄 **Novo** |
| `DataPartida` | `DateTime` | Deve ser futura | 🔄 **Renomeado** (era `Data`) |
| `DataChegada` | `DateTime` | Deve ser após partida | 🔄 **Novo** |
| `DataVolta` | `DateTime?` | Opcional — preenchido apenas se a viagem for de ida e volta | ➕ **Novo** |
| `Descricao` | `string` | — | ✅ Mantido |
| `VeiculoId` | `int` | FK para Veiculo | 🔄 **Novo** |
| `PrecoBase` | `float` (API) / `NUMERIC(10,2)` (SQL) | — | 🔄 **Renomeado** (era `ValorIngresso`) |
| `FotoUrl` | `string?` | Opcional | ✅ Mantido |

#### `Veiculo` (nova entidade)

| Campo | Tipo | Restrições |
|-------|------|-----------|
| `Id` | `int` | Auto-incremento |
| `Modelo` | `string` | Obrigatório |
| `Placa` | `string` | Único |
| `Capacidade` | `int` | > 0 |
| `Tipo` | `string` | "Convencional", "Executivo", "Leito", "Micro-ônibus", "Van" |
| `Linhas` | `int` | Número de fileiras de assentos |
| `Colunas` | `int` | Número de colunas por fileira |

#### `Assento` (nova entidade)

| Campo | Tipo | Restrições |
|-------|------|-----------|
| `Id` | `int` | Auto-incremento |
| `VeiculoId` | `int` | FK para Veiculo |
| `Numero` | `string` | Ex: "1A", "2B", "12C" |
| `Tipo` | `string` | "Janela", "Corredor", "Leito" |
| `Status` | `string` | "Disponível", "Reservado", "Vendido", "Indisponível" |

#### `Passagem` (substitui o conceito de Reserva/Ingresso)

| Campo | Tipo | Restrições |
|-------|------|-----------|
| `Id` | `int` | Auto-incremento |
| `ViagemId` | `int` | FK para Viagem |
| `AssentoId` | `int` | FK para Assento |
| `UsuarioCpf` | `string` | FK para Usuario |
| `PrecoPago` | `float` (API) / `NUMERIC(10,2)` (SQL) | >= 0 |
| `CupomUtilizado` | `string?` | FK nullable para Cupons |
| `Status` | `string` | "Ativa", "Cancelada", "Utilizada" |
| `DataCompra` | `DateTime` | Default: now |
| `DataExpiracaoReserva` | `DateTime?` | Para reservas temporárias |

### 4.3. Novos Endpoints da API

#### Viagens (substitui `/api/eventos/*`)

| Método | Rota | Descrição |
|--------|------|-----------|
| `GET` | `/api/viagens/listar` | Lista todas as viagens |
| `GET` | `/api/viagens/listar/{id}` | Detalhes de uma viagem |
| `GET` | `/api/viagens/pesquisar?origem=&destino=&data=` | Pesquisa com filtros |
| `POST` | `/api/viagens/cadastrar` | Cadastra nova viagem |

#### Veículos (novo)

| Método | Rota | Descrição |
|--------|------|-----------|
| `GET` | `/api/veiculos/listar` | Lista todos os veículos |
| `GET` | `/api/veiculos/listar/{id}` | Detalhes de um veículo |
| `POST` | `/api/veiculos/cadastrar` | Cadastra novo veículo |

#### Assentos (novo)

| Método | Rota | Descrição |
|--------|------|-----------|
| `GET` | `/api/assentos/viagem/{viagemId}` | Mapa de assentos de uma viagem |
| `POST` | `/api/assentos/reservar` | Reserva temporária de assento |
| `POST` | `/api/assentos/liberar` | Libera reserva expirada |
| `POST` | `/api/assentos/bloquear` | Bloqueia/desbloqueia assento (admin) |

#### Passagens (novo)

| Método | Rota | Descrição |
|--------|------|-----------|
| `GET` | `/api/passagens/listar` | Lista todas as passagens |
| `GET` | `/api/passagens/usuario/{cpf}` | Passagens de um usuário |
| `POST` | `/api/passagens/comprar` | Finaliza compra de passagem |
| `POST` | `/api/passagens/cancelar/{id}` | Cancela passagem |

### 4.4. Endpoints Mantidos sem Alteração

| Método | Rota | Controller |
|--------|------|-----------|
| `GET` | `/api/usuarios/listar` | UsuariosController |
| `POST` | `/api/usuarios/cadastrar` | UsuariosController |
| `GET` | `/api/cupons/listar` | CuponsController |
| `POST` | `/api/cupons/cadastrar` | CuponsController |

---

## 5. Mudanças no Frontend — Blazor

### 5.1. Novos Componentes

| Componente | Rota | Descrição | Substitui |
|-----------|------|-----------|:---------:|
| [`MapaAssentos.razor`](billet_2/billet_2/Components/Pages/MapaAssentos.razor) | `/viagem/{id}/assentos` | Mapa visual interativo de poltronas com clique para seleção | `Venda.razor` |
| [`CriarViagem.razor`](billet_2/billet_2/Components/Pages/CriarViagem.razor) | `/criarviagem` | Formulário de cadastro de viagem (origem, destino, veículo, preço) | `Criarevento.razor` |
| [`CriarVeiculo.razor`](billet_2/billet_2/Components/Pages/CriarVeiculo.razor) | `/criarveiculo` | Cadastro de veículos com definição de layout de assentos | — |
| [`MinhasPassagens.razor`](billet_2/billet_2/Components/Pages/MinhasPassagens.razor) | `/minhaspassagens` | Lista de passagens compradas pelo usuário | `Meusingressos.razor` |

### 5.2. Componentes Refatorados

| Componente | Mudança |
|-----------|---------|
| [`Home.razor`](billet_2/billet_2/Components/Pages/Home.razor) | Hero banner adaptado para viagens; grid exibe viagens com origem → destino, data/hora |
| [`Poslogin.razor`](billet_2/billet_2/Components/Pages/Poslogin.razor) | Dashboard exibe viagens; menu admin com "Criar Viagem" e "Criar Veículo" |
| [`Login.razor`](billet_2/billet_2/Components/Pages/Login.razor) | Sem alterações |
| [`Cadastro.razor`](billet_2/billet_2/Components/Pages/Cadastro.razor) | Sem alterações |

### 5.3. Novos Serviços de Integração

#### `ViagemService` (substitui `EventoService`)

| Método | Chamada HTTP |
|--------|-------------|
| `ListarViagensAsync()` | `GET /api/viagens/listar` |
| `BuscarPorIdAsync(int id)` | `GET /api/viagens/listar/{id}` |
| `PesquisarViagensAsync(origem, destino, data)` | `GET /api/viagens/pesquisar` |
| `CriarViagemAsync(Viagem)` | `POST /api/viagens/cadastrar` |

#### `VeiculoService` (novo)

| Método | Chamada HTTP |
|--------|-------------|
| `ListarVeiculosAsync()` | `GET /api/veiculos/listar` |
| `BuscarPorIdAsync(int id)` | `GET /api/veiculos/listar/{id}` |
| `CriarVeiculoAsync(Veiculo)` | `POST /api/veiculos/cadastrar` |

#### `AssentoService` (novo)

| Método | Chamada HTTP |
|--------|-------------|
| `ObterMapaAssentosAsync(int viagemId)` | `GET /api/assentos/viagem/{viagemId}` |
| `ReservarAssentoAsync(int assentoId, string cpf)` | `POST /api/assentos/reservar` |
| `LiberarAssentoAsync(int assentoId)` | `POST /api/assentos/liberar` |
| `BloquearAssentoAsync(int assentoId, bool bloquear)` | `POST /api/assentos/bloquear` |

#### `PassagemService` (novo)

| Método | Chamada HTTP |
|--------|-------------|
| `ListarTodasAsync()` | `GET /api/passagens/listar` |
| `ListarPorUsuarioAsync(string cpf)` | `GET /api/passagens/usuario/{cpf}` |
| `ComprarPassagemAsync(Passagem)` | `POST /api/passagens/comprar` |
| `CancelarPassagemAsync(int id)` | `POST /api/passagens/cancelar/{id}` |

### 5.4. Mapa de Navegação Pivotado

```
                    ┌──────────┐
                    │   Home   │
                    │   (/)    │
                    └────┬─────┘
                         │
              ┌──────────┼──────────┐
              ▼          ▼          ▼
         ┌────────┐ ┌────────┐ ┌──────────────┐
         │Cadastro│ │ Login  │ │ Viagem/{id}  │
         │(/cadastro)│(/login)│ │ (detalhes)   │
         └───┬────┘ └───┬────┘ └──────┬───────┘
             │          │             │
             └────┬─────┘             │
                  ▼                   ▼
           ┌────────────┐    ┌────────────────┐
           │  Poslogin  │    │  Mapa Assentos │
           │ (/poslogin)│    │ /viagem/{id}/  │
           └─────┬──────┘    │   assentos     │
                 │           └───────┬────────┘
        ┌────────┼────────┐         │
        ▼        ▼        ▼         ▼
   ┌────────┐ ┌────────┐ ┌──────────────┐ ┌──────────────┐
   │ Criar  │ │ Criar  │ │ Mapa         │ │ Minhas       │
   │ Viagem │ │ Veículo│ │ Assentos     │ │ Passagens    │
   │(admin) │ │(admin) │ │              │ │              │
   └────────┘ └────────┘ └──────────────┘ └──────────────┘
```

---

## 6. Mudanças no Modelo de Dados (PostgreSQL)

O script [`db/sql`](../db/sql) original é **expandido** com 3 novas tabelas e alterações nas existentes.

### 6.1. Novas Tabelas

#### `Veiculos`

```sql
CREATE TABLE IF NOT EXISTS "Veiculos" (
    "Id" SERIAL PRIMARY KEY,
    "Modelo" VARCHAR(255) NOT NULL,
    "Placa" VARCHAR(20) UNIQUE NOT NULL,
    "Capacidade" INT NOT NULL CHECK ("Capacidade" > 0),
    "Tipo" VARCHAR(50) NOT NULL,
    "Linhas" INT NOT NULL,
    "Colunas" INT NOT NULL
);
```

#### `Assentos`

```sql
CREATE TABLE IF NOT EXISTS "Assentos" (
    "Id" SERIAL PRIMARY KEY,
    "VeiculoId" INT NOT NULL REFERENCES "Veiculos"("Id") ON DELETE CASCADE,
    "Numero" VARCHAR(10) NOT NULL,
    "Tipo" VARCHAR(20) NOT NULL,
    "Status" VARCHAR(20) NOT NULL DEFAULT 'Disponivel',
    UNIQUE("VeiculoId", "Numero")
);
```

#### `Passagens` (substitui `Reservas`)

```sql
CREATE TABLE IF NOT EXISTS "Passagens" (
    "Id" SERIAL PRIMARY KEY,
    "ViagemId" INT NOT NULL REFERENCES "Viagens"("Id") ON DELETE RESTRICT,
    "AssentoId" INT NOT NULL REFERENCES "Assentos"("Id") ON DELETE RESTRICT,
    "UsuarioCpf" VARCHAR(11) NOT NULL REFERENCES "Usuarios"("Cpf") ON DELETE RESTRICT,
    "PrecoPago" NUMERIC(10,2) NOT NULL CHECK ("PrecoPago" >= 0),
    "CupomUtilizado" VARCHAR(50) REFERENCES "Cupons"("Codigo") ON DELETE SET NULL,
    "Status" VARCHAR(20) NOT NULL DEFAULT 'Ativa',
    "DataCompra" TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    "DataExpiracaoReserva" TIMESTAMP
);
```

### 6.2. Tabelas Alteradas

#### `Eventos` → `Viagens`

| Campo Original | Campo Novo | Mudança |
|---------------|-----------|:-------:|
| `Nome` | `Origem` | 🔄 Renomeado + mudança semântica |
| *(não existia)* | `Destino` | ➕ Novo campo |
| `Data` | `DataPartida` | 🔄 Renomeado |
| *(não existia)* | `DataChegada` | ➕ Novo campo |
| *(não existia)* | `DataVolta` | ➕ Novo campo (opcional — ida e volta) |
| `Local` | *(removido)* | ❌ Removido |
| `QuantidadeIngressos` | *(removido)* | ❌ Removido (capacidade vem do Veículo) |
| `ValorIngresso` | `PrecoBase` | 🔄 Renomeado |
| *(não existia)* | `VeiculoId` (FK) | ➕ Novo campo |
| `Descricao` | `Descricao` | ✅ Mantido |
| `FotoUrl` | `FotoUrl` | ✅ Mantido |

### 6.3. Tabelas Mantidas sem Alteração

- `Usuarios` — sem alterações
- `Cupons` — sem alterações (já contemplava campos expandidos no script original)

---

## 7. Mudanças nos Testes

### 7.1. Testes Adaptados

| Teste Original | Teste Pivotado | Mudança |
|---------------|---------------|:-------:|
| `TesteDescontoValido.cs` | Mantido | ✅ Sem alterações |
| `TesteEventoCapacidade.cs` | `TesteVeiculoCapacidade.cs` | 🔄 Adaptado para validar capacidade do veículo > 0 |
| `TestePrecoPositivo.cs` | `TestePrecoPassagemPositivo.cs` | 🔄 Adaptado para preço de passagem |
| `TesteReservaValida.cs` | `TesteReservaAssentoValida.cs` | 🔄 Adaptado para reserva de assento |
| `TesteReservaVazia.cs` | `TestePassagemSemCpf.cs` | 🔄 Adaptado para passagem sem CPF |

### 7.2. Novos Testes

| Arquivo | Cenário |
|---------|---------|
| `TesteSelecaoAssento.cs` | Assento já vendido não pode ser selecionado |
| `TesteReservaExpirada.cs` | Reserva expirada libera o assento automaticamente |
| `TesteViagemSemVeiculo.cs` | Viagem não pode ser criada sem veículo associado |
| `TesteAssentoDuplicado.cs` | Dois assentos com mesmo número no mesmo veículo |

---

## 8. Limitações Conhecidas (Atualizadas)

As limitações do projeto original permanecem, com acréscimos específicos da pivotagem:

| # | Limitação | Descrição | Impacto |
|---|-----------|-----------|---------|
| 1 | **Sem persistência** | Dados em `List<T>` na memória | Perda total ao reiniciar a API |
| 2 | **Autenticação frágil** | Login baixa todos os usuários e valida no frontend | Exposição de dados; sem sessão segura |
| 3 | **Solution incompleta** | `billet_2.slnx` não inclui `src/api.csproj` | Dois projetos precisam ser abertos separadamente |
| 4 | **Sem checkout real** | Carrinho local sem finalização no backend | Compra não é concluída |
| 5 | **Sem expiração real de reserva** | Reserva temporária sem timer automático | Assento pode ficar reservado indefinidamente |
| 6 | **Mapa de assentos estático** | Layout do veículo pré-definido sem customização por veículo | Todos os veículos com mesmo formato de mapa |
| 7 | **Sem integração de pagamento** | Nenhum gateway de pagamento implementado | Compra não processa pagamento real |
| 8 | **Script SQL desatualizado** | `db/sql` ainda reflete modelo antigo (Eventos, Reservas) | Banco de dados não pode ser criado para o novo domínio |

### 8.1. Roadmap Pós-Pivotagem

1. Atualizar script `db/sql` com as novas tabelas (Veiculos, Assentos, Passagens, Viagens)
2. Renomear `db/sql` → `db/script.sql`
3. Implementar os novos controllers no backend (Viagens, Veiculos, Assentos, Passagens)
4. Criar os novos componentes Blazor (MapaAssentos, CriarViagem, CriarVeiculo, MinhasPassagens)
5. Adaptar serviços do frontend para o novo domínio
6. Implementar lógica de expiração de reserva (timer no servidor)
7. Integrar PostgreSQL com Dapper (ver [`ADR-001`](../adr.md))
8. Implementar autenticação JWT

---

## 9. Resumo das Mudanças

| Aspecto | TicketPrime (Original) | TripPrime (Pivotado) |
|---------|----------------------|---------------------|
| **Domínio** | Eventos culturais | Viagens e excursões |
| **Entidade principal** | `Evento` | `Viagem` |
| **Entidades** | 4 (Evento, Usuario, Cupom, Reserva) | 6 (Viagem, Usuario, Cupom, Veiculo, Assento, Passagem) |
| **Endpoints API** | 7 | ~18 |
| **Páginas Blazor** | 8 | ~10 |
| **Serviços Frontend** | 3 | 5 |
| **Tabelas BD** | 4 | 6 |
| **Testes** | 5 | 9 |
| **Stack tecnológica** | .NET 10 + Blazor + Bootstrap | ✅ **Mesma stack** |

---

## 10. Referências

| Documento | Descrição |
|-----------|-----------|
| [`docs/arquitetura.md`](../arquitetura.md) | Documento de arquitetura original (stack completa, padrões, execução) |
| [`docs/pivotagem/pivotagem.md`](pivotagem.md) | Documento de visão da pivotagem (motivação, conceitos, funcionalidades) |
| [`docs/visao.md`](../visao.md) | Documento de visão do produto |
| [`docs/historiasdeusuario.md`](../historiasdeusuario.md) | Histórias de usuário |
| [`db/sql`](../db/sql) | Script DDL PostgreSQL (a ser atualizado) |
