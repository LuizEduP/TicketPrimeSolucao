# TripPrime (anteriormente TicketPrimeSolucao)

> **Pivotado de:** TicketPrime — plataforma de venda de ingressos para eventos  
> **Pivotado para:** TripPrime — plataforma de comercialização de assentos em transportes para excursões

Resolução da avaliação referente à matéria de **Engenharia de Software (UNIFESO)**.

Sistema de venda de passagens para viagens e excursões, composto por uma API backend e um frontend web, desenvolvidos em C# com .NET 10.

Integrantes:
- Gabriel Castor 06009642
- Gabriel Lepsch Monteiro 02001770
- Gabriel Ribeiro 06010603
- Lucas Oliveira 06010486
- Luiz Eduardo P. Rosa 06010412
- Thiago Zandonade Fernandes 06010263

---

## Resumo da Pivotagem (TicketPrime → TripPrime)

O projeto passou por uma pivotagem completa de domínio de negócio, migrando de **venda de ingressos para eventos culturais** (shows, festivais) para **comercialização de passagens em transportes para excursões** (ônibus, vans, micro-ônibus).

### Motivação

- Mercado de excursões carece de plataformas modernas e de código aberto para gestão de assentos
- Aproveitamento de ~80% da lógica existente (CRUD, controle de capacidade, reservas, cupons)
- Introdução de desafios reais: seleção visual de assentos, mapa de poltronas, controle de ocupação por poltrona

### Mudanças Conceituais

| TicketPrime (Antigo) | TripPrime (Novo) |
|---|---|
| Evento (show, festival) | Viagem / Excursão (origem → destino) |
| Ingresso | Passagem (assento no veículo) |
| Lote de ingressos | Classe de assentos (janela, corredor) |
| Setor (VIP, Normal) | Tipo de poltrona |
| Capacidade do evento | Capacidade do veículo |
| Reserva de vaga | Reserva de assento (poltrona específica) |

### Documentação da Pivotagem

A documentação completa da pivotagem está em `docs/pivotagem/`:
- [`pivotagem.md`](docs/pivotagem/pivotagem.md) — Visão, motivação e conceitos do novo domínio
- [`arquitetura-pivotagem.md`](docs/pivotagem/arquitetura-pivotagem.md) — Arquitetura detalhada das mudanças
- [`roadmap.md`](docs/pivotagem/roadmap.md) — Roadmap de desenvolvimento com 23 specs
- [`specs/`](docs/pivotagem/specs/) — 15 documentos de especificação implementados

Os documentos originais do TicketPrime (`docs/visao.md`, `docs/arquitetura.md`, `docs/historiasdeusuario.md`) foram preservados como referência histórica e **não foram alterados**.

---

## Tecnologias

| Camada | Tecnologia |
|---|---|
| Backend | ASP.NET Core 10 — Minimal API |
| Frontend | Blazor Web App (Interactive Server + WebAssembly) |
| Linguagem | C# (.NET 10) |
| Estilo | Bootstrap 5 (via arquivos estáticos) |
| Banco de dados | Script SQL disponível (PostgreSQL) — **não integrado ao código** |
| ORM planejado | Dapper (ver [ADR-001](docs/adrs/001-escolha-do-micro-orm.md)) — EF Core **proibido** |

---

## Estrutura de Pastas

```
TicketPrimeSolucao-main/
│
├── TicketPrimeSolucao-pivotagem.sln  ← Solution completa (api + tests + billet_2)
├── billet_2.slnx                      ← Solution original do frontend (preservada)
│
├── src/                               ← Backend: ASP.NET Core Minimal API (PIVOTADO)
│   ├── Program.cs                     ← Ponto de entrada — 16 rotas registradas
│   ├── api.csproj
│   ├── api.http
│   ├── appsettings.json
│   ├── viagens/                       ← 🔄 Renomeado de eventos/
│   │   └── ViagensController.cs       ← 🔄 Refatorado (4 endpoints: listar, buscar, pesquisar, cadastrar)
│   ├── veiculos/                      ← ➕ NOVO
│   │   └── VeiculosController.cs      ← ➕ NOVO (3 endpoints + geração automática de assentos)
│   ├── assentos/                      ← ➕ NOVO
│   │   └── AssentosController.cs      ← ➕ NOVO (4 endpoints: mapa, reservar, liberar, bloquear)
│   ├── passagens/                     ← ➕ NOVO
│   │   └── PassagensController.cs     ← ➕ NOVO (4 endpoints: listar, por usuário, comprar, cancelar)
│   ├── usuarios/
│   │   └── UsuariosController.cs      ← Mantido (sem alterações)
│   └── cupons/
│       └── CuponsController.cs        ← Mantido
│
├── billet_2/                          ← Frontend: Blazor Web App (PIVOTADO)
│   ├── billet_2/                      ← Projeto servidor (Interactive Server)
│   │   ├── Program.cs                 ← Serviços pivotados registrados (6 serviços)
│   │   ├── Components/Pages/
│   │   │   ├── Home.razor             ← 🔄 Refatorado: hero + grid de viagens/destinos
│   │   │   ├── Poslogin.razor         ← 🔄 Refatorado: dashboard com viagens + admin menu
│   │   │   ├── Login.razor            ← Mantido
│   │   │   ├── Cadastro.razor         ← Mantido
│   │   │   ├── CriarViagem.razor      ← 🔄 Refatorado de Criarevento.razor
│   │   │   ├── CriarVeiculo.razor     ← ➕ NOVO
│   │   │   ├── MapaAssentos.razor     ← ➕ NOVO (mapa visual interativo de poltronas)
│   │   │   ├── MinhasPassagens.razor  ← 🔄 Refatorado de Meusingressos.razor
│   │   │   ├── VendaRedirect.razor    ← ➕ NOVO (redireciona /vendas/{id} → /viagem/{id}/assentos)
│   │   │   └── Error.razor
│   │   ├── Models/
│   │   │   ├── Viagem.cs             ← 🔄 Renomeado de Evento.cs
│   │   │   ├── Veiculo.cs            ← ➕ NOVO
│   │   │   ├── Assento.cs            ← ➕ NOVO
│   │   │   ├── Passagem.cs           ← ➕ NOVO
│   │   │   └── Usuario.cs            ← Mantido
│   │   ├── Services/
│   │   │   ├── ViagemService.cs      ← 🔄 Refatorado de EventoService.cs
│   │   │   ├── VeiculoService.cs     ← ➕ NOVO
│   │   │   ├── AssentoService.cs     ← ➕ NOVO
│   │   │   ├── PassagemService.cs    ← ➕ NOVO
│   │   │   ├── UsuarioService.cs     ← Mantido
│   │   │   └── AuthService.cs        ← Mantido
│   │   └── wwwroot/images/viagens/   ← 🔄 Renomeado de eventos/
│   └── billet_2.Client/              ← Projeto cliente WebAssembly
│
├── db/
│   └── script.sql                     ← 🔄 Renomeado de sql (corrigido: extensão .sql)
│
├── docs/
│   ├── visao.md                       ← Original (TicketPrime — preservado)
│   ├── arquitetura.md                 ← Original (TicketPrime — preservado)
│   ├── historiasdeusuario.md          ← Original (24 histórias + 24 cenários BDD — preservado)
│   ├── pivotagem/                     ← ➕ NOVO — Documentação da pivotagem
│   │   ├── pivotagem.md               ← Visão da pivotagem
│   │   ├── arquitetura-pivotagem.md   ← Arquitetura pivotada
│   │   ├── roadmap.md                 ← Roadmap com 23 specs
│   │   └── specs/                     ← 15 specs implementadas (0010 a 0150)
│   ├── adrs/
│   │   └── 001-escolha-do-micro-orm.md ← ➕ NOVO — ADR: Dapper obrigatório, EF Core proibido
│   ├── operacao.md                    ← ➕ NOVO — Matriz de riscos, SLO, métricas
│   ├── seguranca_ciclo.md             ← ➕ NOVO — Threat model, gates de segurança
│   ├── registro_divida_tecnica.md     ← ➕ NOVO — 8 dívidas técnicas registradas
│   ├── fluxo_manutencao.md            ← ➕ NOVO
│   ├── analise_arquitetura.md         ← ➕ NOVO
│   ├── plano_iteracao.md              ← ➕ NOVO
│   └── topologia_times.md             ← ➕ NOVO
│
├── tests/                             ← Testes xUnit (5 testes, adaptados ao novo domínio)
│   ├── MeuProjeto.Tests.csproj
│   ├── TesteDescontoValido.cs
│   ├── TesteEventoCapacidade.cs
│   ├── TestePrecoPositivo.cs
│   ├── TesteReservaValida.cs
│   └── TesteReservaVazia.cs
│
├── release_checklist_final.md         ← ➕ NOVO — Checklist de entrega TripPrime
├── CORRECAO.md                        ← Correção da AV1 (nota: 8/10)
└── CLAUDE.md                          ← Regras para desenvolvimento por IA
```

---

## Pré-requisitos

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)

> Nenhuma outra dependência externa é necessária para rodar o projeto localmente.  
> As dependências NuGet são restauradas automaticamente pelo `dotnet run`.

---

## Como Executar

O projeto possui **dois serviços independentes** que devem ser iniciados em terminais separados.

### 1. Backend — API

```bash
cd src
dotnet run
```

A API estará disponível em: `http://localhost:5289`

### 2. Frontend — Blazor

```bash
cd billet_2/billet_2
dotnet run
```

O frontend estará disponível em: `http://localhost:5096`

> **Importante:** a API deve estar rodando antes de acessar o frontend, pois as páginas buscam dados via HTTP.

### 3. Testes

```bash
cd tests
dotnet test
```

---

## Endpoints da API

Todos os endpoints são prefixados com `/api`. Os dados são armazenados **em memória** — não há persistência entre reinicializações.

### Viagens (🔄 substitui `/api/eventos/*`)

| Método | Rota | Descrição |
|---|---|---|
| `GET` | `/api/viagens/listar` | Lista todas as viagens |
| `GET` | `/api/viagens/listar/{id}` | Busca viagem por ID |
| `GET` | `/api/viagens/pesquisar?origem=&destino=&data=` | Pesquisa com filtros |
| `POST` | `/api/viagens/cadastrar` | Cadastra nova viagem (6 validações) |

**Corpo esperado para cadastro de viagem:**
```json
{
  "origem": "Rio de Janeiro",
  "destino": "São Paulo",
  "dataPartida": "2026-12-01T08:00:00",
  "dataChegada": "2026-12-01T14:00:00",
  "dataVolta": null,
  "descricao": "Excursão corporativa com parada para almoço",
  "veiculoId": 1,
  "precoBase": 150.00,
  "fotoUrl": "images/viagens/sp.jpg"
}
```

### Veículos (➕ NOVO)

| Método | Rota | Descrição |
|---|---|---|
| `GET` | `/api/veiculos/listar` | Lista todos os veículos |
| `GET` | `/api/veiculos/listar/{id}` | Busca veículo por ID |
| `POST` | `/api/veiculos/cadastrar` | Cadastra novo veículo (6 validações + geração automática de assentos) |

**Corpo esperado para cadastro de veículo:**
```json
{
  "modelo": "Mercedes-Benz O-500",
  "placa": "ABC-1234",
  "tipo": "Executivo",
  "linhas": 10,
  "colunas": 4
}
```

### Assentos (➕ NOVO)

| Método | Rota | Descrição |
|---|---|---|
| `GET` | `/api/assentos/viagem/{viagemId}` | Mapa de assentos de uma viagem |
| `POST` | `/api/assentos/reservar` | Reserva temporária de assento |
| `POST` | `/api/assentos/liberar` | Libera reserva expirada |
| `POST` | `/api/assentos/bloquear` | Bloqueia/desbloqueia assento (admin) |

**Corpo para reservar assento:**
```json
{
  "assentoId": 1,
  "usuarioCpf": "00000000000"
}
```

### Passagens (➕ NOVO)

| Método | Rota | Descrição |
|---|---|---|
| `GET` | `/api/passagens/listar` | Lista todas as passagens |
| `GET` | `/api/passagens/usuario/{cpf}` | Passagens de um usuário |
| `POST` | `/api/passagens/comprar` | Finaliza compra de passagem |
| `POST` | `/api/passagens/cancelar/{id}` | Cancela passagem |

**Corpo para comprar passagem:**
```json
{
  "viagemId": 1,
  "assentoId": 5,
  "usuarioCpf": "00000000000",
  "cupomUtilizado": "PROMO10"
}
```

### Usuários (✅ Mantido sem alterações)

| Método | Rota | Descrição |
|---|---|---|
| `GET` | `/api/usuarios/listar` | Lista todos os usuários |
| `POST` | `/api/usuarios/cadastrar` | Cadastra um novo usuário |

### Cupons (✅ Mantido sem alterações)

| Método | Rota | Descrição |
|---|---|---|
| `GET` | `/api/cupons/listar` | Lista todos os cupons |
| `POST` | `/api/cupons/cadastrar` | Cadastra um novo cupom |

---

## Páginas do Frontend

| Rota | Descrição | Status |
|---|---|---|
| `/` | Home — hero banner + grid de viagens/destinos | 🔄 Refatorado |
| `/cadastro` | Formulário de cadastro de usuário | ✅ Mantido |
| `/login` | Tela de login | ✅ Mantido |
| `/poslogin` | Dashboard pós-login com viagens + admin menu | 🔄 Refatorado |
| `/criarviagem` | Formulário de cadastro de viagem (admin) | 🔄 Refatorado |
| `/criarveiculo` | Formulário de cadastro de veículo (admin) | ➕ Novo |
| `/viagem/{id}/assentos` | Mapa interativo de assentos para seleção | ➕ Novo |
| `/minhaspassagens` | Lista de passagens compradas pelo usuário | 🔄 Refatorado |
| `/vendas/{id}` | Redireciona para `/viagem/{id}/assentos` | 🔄 Redirecionamento |

---

## Fluxo de Compra (TripPrime)

```
Visitante
    │
    ├──> Home: visualiza viagens disponíveis (origem → destino)
    │
    ├──> [Login / Cadastro]
    │
    ├──> Poslogin: dashboard com viagens
    │       │
    │       ├──> Usuário comum: acessa Minhas Passagens
    │       └──> Admin: Criar Viagem / Criar Veículo
    │
    ├──> Seleciona viagem → Mapa de Assentos
    │       ├── Assentos disponíveis (verde)
    │       ├── Assentos reservados (amarelo)
    │       ├── Assentos vendidos (vermelho)
    │       └── Assentos indisponíveis (cinza)
    │
    ├──> Reserva assento → status muda para "Reservado"
    │
    └──> Finaliza compra → Passagem criada, assento marcado como "Vendido"
```

---

## Banco de Dados

O arquivo `db/script.sql` contém o script DDL para criação das tabelas em **PostgreSQL**:

- `Usuarios`
- `Eventos` (⚠️ ainda reflete o domínio antigo — pendente atualização na Fase 5)
- `Cupons`
- `Reservas` (⚠️ ainda reflete o domínio antigo — pendente de substituição por `Passagens`)

O modelo pivotado prevê as tabelas `Viagens`, `Veiculos`, `Assentos` e `Passagens` (ver [`arquitetura-pivotagem.md`](docs/pivotagem/arquitetura-pivotagem.md#6-mudanças-no-modelo-de-dados-postgresql)), mas o script ainda não foi atualizado.

> **TODO (Fase 5):** Atualizar `db/script.sql` com as novas tabelas do domínio TripPrime e integrar ao código via Dapper com PostgreSQL.

---

## Documentação OpenAPI

Em ambiente de desenvolvimento, a API expõe a documentação OpenAPI automaticamente em:

```
http://localhost:5289/openapi
```

---

## Artefatos de Engenharia de Software

| Artefato | Localização | Descrição |
|---|---|---|
| Histórias de Usuário | `docs/historiasdeusuario.md` | 24 histórias + 24 cenários BDD (domínio original) |
| Documento de Visão | `docs/visao.md` | Visão original do TicketPrime |
| Visão da Pivotagem | `docs/pivotagem/pivotagem.md` | Visão do TripPrime |
| Arquitetura Original | `docs/arquitetura.md` | Stack, padrões, execução |
| Arquitetura Pivotada | `docs/pivotagem/arquitetura-pivotagem.md` | Mudanças arquiteturais da pivotagem |
| ADR | `docs/adrs/001-escolha-do-micro-orm.md` | Decisão: Dapper obrigatório, EF Core proibido |
| Roadmap | `docs/pivotagem/roadmap.md` | 23 specs em 7 fases |
| Specs Implementadas | `docs/pivotagem/specs/` | 15 documentos (0010 a 0150) |
| Matriz de Riscos | `docs/operacao.md` | 6 riscos com gatilhos e estratégias |
| SLO / Error Budget | `docs/operacao.md` | SLO 99.5%, 3 níveis de Error Budget Policy |
| Segurança | `docs/seguranca_ciclo.md` | Threat model + 3 gates de segurança |
| Dívida Técnica | `docs/registro_divida_tecnica.md` | 8 dívidas com priorização |
| Checklist de Release | `release_checklist_final.md` | 7 checkpoints de entrega concluídos |
| Correção AV1 | `CORRECAO.md` | Nota: 8/10 |

---

## Resumo das Mudanças Implementadas na Pivotagem

### Fase 0 — Preparação e Correções ✅

| Spec | Descrição | Status |
|---|---|---|
| 0010 | Renomear `db/sql` → `db/script.sql` (extensão .sql ausente causou perda de 1 ponto na AV1) | ✅ |
| 0020 | Criar `TicketPrimeSolucao-pivotagem.sln` incluindo api + tests + billet_2 | ✅ |
| 0030 | Renomear `src/eventos/` → `src/viagens/` e `wwwroot/images/eventos/` → `wwwroot/images/viagens/` | ✅ |

### Fase 1 — Backend: 4 novos controllers ✅

| Spec | Descrição | Endpoints |
|---|---|---|
| 0040 | `ViagensController` (substitui EventosController) | 4 endpoints (listar, buscar por ID, pesquisar com filtros, cadastrar com 6 validações) |
| 0050 | `VeiculosController` (novo) | 3 endpoints + geração automática de assentos ao cadastrar veículo |
| 0060 | `AssentosController` (novo) | 4 endpoints (mapa por viagem, reservar, liberar, bloquear) |
| 0070 | `PassagensController` (novo) | 4 endpoints (listar, por usuário, comprar com cupom, cancelar com liberação de assento) |

### Fase 2 — Frontend: Serviços e Componentes ✅

| Spec | Descrição |
|---|---|
| 0080 | `EventoService` → `ViagemService` + `Criarevento.razor` → `CriarViagem.razor` |
| 0090 | `VeiculoService` + `CriarVeiculo.razor` (formulário com capacidade = Linhas × Colunas) |
| 0100 | `AssentoService` + `MapaAssentos.razor` (mapa visual interativo de poltronas) |
| 0110 | `PassagemService` + `Meusingressos.razor` → `MinhasPassagens.razor` |

### Fase 3 — Refatoração Final ✅

| Spec | Descrição |
|---|---|
| 0120 | `Home.razor` refatorado: hero banner de viagens + grid de destinos com cards |
| 0130 | `Poslogin.razor` refatorado: dashboard com viagens + menu admin (Criar Viagem / Criar Veículo) |
| 0140 | `Venda.razor` removido; `VendaRedirect.razor` redireciona `/vendas/{id}` → `/viagem/{id}/assentos` |
| 0150 | Rotas, layout e navegação atualizados para o novo domínio |

### Total: 15 specs implementadas (Fases 0-3)

---

## Pendências (Fases 4-6 do Roadmap)

| Fase | Spec | Descrição | Prioridade |
|---|---|---|---|
| 4 | 0160-0180 | Adaptar testes existentes + criar novos testes unitários e de integração | Média |
| 5 | 0190 | Atualizar `db/script.sql` com tabelas Viagens, Veiculos, Assentos, Passagens | Média |
| 6 | 0200 | Implementar expiração automática de reserva (timer no servidor) | Baixa |
| 6 | 0210 | Implementar autenticação JWT | Baixa |
| 6 | 0220 | Implementar checkout real no backend | Baixa |
| 6 | 0230 | Configurar CI/CD com GitHub Actions | Baixa |

---

## Limitações Conhecidas

- **Sem persistência:** todos os dados são perdidos ao reiniciar a API (listas em memória `List<T>`)
- **Autenticação frágil:** login baixa todos os usuários e valida no frontend — senhas trafegam expostas
- **Sem expiração real de reserva:** assentos reservados não são liberados automaticamente
- **Mapa de assentos estático:** layout do veículo é pré-definido (10×4), sem customização por veículo
- **Script SQL desatualizado:** `db/script.sql` ainda reflete o modelo antigo (Eventos, Reservas)
- **Sem integração de pagamento:** compra não processa pagamento real
- **URL da API hardcoded:** `http://localhost:5289` no `Program.cs` do frontend

---

## Licença

Distribuído sob a licença MIT. Consulte o arquivo `LICENSE` para mais informações.
