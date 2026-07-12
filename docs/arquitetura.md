# Documento de Arquitetura — TicketPrime

> **Instrução para leitura por IA:** Este documento descreve toda a arquitetura do sistema TicketPrime de forma estruturada e sequencial. Para compreender o projeto autonomamente, leia na ordem: (1) Visão Geral → (2) Stack → (3) Estrutura → (4) Backend → (5) Frontend → (6) Dados → (7) Testes → (8) Limitações. Cada seção contém informações completas e auto-contidas. Links para arquivos do código-fonte estão incluídos para consulta direta.

---

## 1. Visão Geral da Arquitetura

O **TicketPrime** é um sistema web de venda de ingressos composto por **dois serviços independentes** que se comunicam via HTTP:

```
┌─────────────────────────────────────────────────────────────────────┐
│                        NAVEGADOR (Cliente)                           │
│                                                                       │
│  ┌───────────────────────────────────────────────────────────────┐   │
│  │              Blazor Web App (Frontend)                         │   │
│  │  http://localhost:5096                                         │   │
│  │                                                                   │
│  │  ┌─────────────────────┐  ┌──────────────────────────────┐   │   │
│  │  │ Interactive Server   │  │  WebAssembly (WASM) Client   │   │   │
│  │  │ (billet_2)           │  │  (billet_2.Client)           │   │   │
│  │  └─────────────────────┘  └──────────────────────────────┘   │   │
│  └───────────────────────┬───────────────────────────────────────┘   │
└──────────────────────────┼───────────────────────────────────────────┘
                           │  HTTP (CORS)
                           ▼
┌─────────────────────────────────────────────────────────────────────┐
│          ASP.NET Core Minimal API (Backend)                          │
│  http://localhost:5289                                               │
│                                                                       │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐              │
│  │ Usuários      │  │ Eventos      │  │ Cupons       │              │
│  │ Controller    │  │ Controller   │  │ Controller   │              │
│  └──────┬───────┘  └──────┬───────┘  └──────┬───────┘              │
│         │                 │                 │                         │
│         └────────┬────────┴────────┬────────┘                         │
│                  ▼                 ▼                                   │
│        ┌─────────────────────────────────────────────────────────┐   │
│        │           Listas em Memória (List<T>)                    │   │
│        │   (Sem persistência — dados voláteis)                   │   │
│        └─────────────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────────────────┘
```

### 1.1. Padrão Arquitetural

O sistema adota uma **arquitetura de duas camadas (2-Tier)**:

| Camada | Tecnologia | Responsabilidade |
|--------|-----------|------------------|
| **Apresentação** | Blazor Web App (.NET 10) | Interface do usuário, interatividade, renderização de componentes |
| **API** | ASP.NET Core Minimal API (.NET 10) | Regras de negócio, validação, armazenamento de dados |

**Observação crítica:** Não há camada de banco de dados integrada. A API utiliza listas em memória (`List<T>`) como armazenamento temporário. Todos os dados são perdidos ao reiniciar a API. O modelo de dados descrito na seção 6 é o esquema **planejado** para PostgreSQL, ainda não implementado.

---

## 2. Stack Tecnológica

| Componente | Tecnologia | Versão | Onde é usado |
|-----------|-----------|--------|-------------|
| Runtime | .NET | 10.0 | Todo o sistema |
| Backend Framework | ASP.NET Core (Minimal API) | 10.0 | `src/` |
| Frontend Framework | Blazor Web App | 10.0 | `billet_2/` |
| Render Mode (Server) | Interactive Server (SignalR) | — | Páginas principais do Blazor |
| Render Mode (Client) | Blazor WebAssembly | — | `billet_2.Client/` |
| Documentação API | Microsoft.AspNetCore.OpenApi | 10.0.5 | `src/Program.cs` |
| CSS | Bootstrap 5 (CDN + local) | 5.3.0 | `wwwroot/` |
| Testes | xUnit | 2.9.3 | `tests/` |
| Test Runner | Microsoft.NET.Test.Sdk | 17.14.1 | `tests/` |
| SGBD (planejado) | PostgreSQL | — | `db/sql` (script DDL) |
| Micro-ORM (planejado) | Dapper (com parâmetros `@`) | — | Ver [`ADR-001`](adr.md) — EF Core proibido |

---

## 3. Estrutura de Diretórios

```
TicketPrimeSolucao-main/
│
├── billet_2.slnx                    ← Solution (apenas frontend incluso)
│
├── src/                             ← BACKEND: API
│   ├── Program.cs                   ← Entry point, CORS, registro de rotas
│   ├── api.csproj                   ← net10.0
│   ├── api.http                     ← Exemplos de chamadas HTTP
│   ├── appsettings.json
│   ├── Properties/launchSettings.json  ← Porta 5289
│   ├── eventos/EventosController.cs
│   ├── usuarios/UsuariosController.cs
│   └── cupons/CuponsController.cs
│
├── billet_2/                        ← FRONTEND: Blazor
│   ├── billet_2/                    ← Projeto servidor (Interactive Server)
│   │   ├── Program.cs               ← DI, HttpClient apontando p/ API :5289
│   │   ├── Components/
│   │   │   ├── App.razor            ← HTML raiz
│   │   │   ├── Routes.razor         ← Roteamento
│   │   │   ├── MainLayout.razor     ← Layout base
│   │   │   └── Pages/               ← 8 páginas (Home, Login, Cadastro, etc.)
│   │   ├── Models/                  ← Evento.cs, Usuario.cs
│   │   ├── Services/                ← AuthService, EventoService, UsuarioService
│   │   └── wwwroot/                 ← CSS, imagens, vídeos, Bootstrap, fontes
│   │
│   └── billet_2.Client/             ← Projeto WebAssembly
│       ├── Program.cs, Routes.razor
│       ├── Layout/, Pages/          ← Páginas de exemplo (Counter, Weather)
│       └── wwwroot/
│
├── db/sql                           ← Script DDL PostgreSQL (sem extensão .sql)
├── docs/
│   ├── visao.md                     ← Documento de visão do produto
│   ├── historiasdeusuario.md        ← 24 histórias de usuário + BDD
│   └── arquitetura.md               ← ← ESTE DOCUMENTO
├── tests/                           ← 5 testes xUnit
├── README.md
└── CORRECAO.md                      ← Feedback AV1 (nota 8/10)
```

---

## 4. Backend — API

### 4.1. Ponto de Entrada (`src/Program.cs`)

O [`Program.cs`](src/Program.cs:1) realiza 3 configurações principais:

1. **OpenAPI** — `AddOpenApi()` + `MapOpenApi()` para documentação automática (apenas em Development)
2. **CORS** — Política `BlazorPolicy` liberando `http://localhost:5096` (origem do frontend) com qualquer header/método
3. **Registro de rotas** — 7 métodos de extensão chamados sequencialmente no `WebApplication`:
   - [`app.CadastrarUsuarios()`](src/Program.cs:29) — POST `/api/usuarios/cadastrar`
   - [`app.ListarUsuarios()`](src/Program.cs:30) — GET `/api/usuarios/listar`
   - [`app.CadastrarEventos()`](src/Program.cs:31) — POST `/api/eventos/cadastrar`
   - [`app.ListarEventos()`](src/Program.cs:32) — GET `/api/eventos/listar`
   - [`app.ListarEventoPorId()`](src/Program.cs:33) — GET `/api/eventos/listar/{id}`
   - [`app.CadastrarCupons()`](src/Program.cs:34) — POST `/api/cupons/cadastrar`
   - [`app.ListarCupons()`](src/Program.cs:35) — GET `/api/cupons/listar`

### 4.2. Padrão de Implementação

Cada controlador é uma **classe estática** com métodos de extensão sobre `WebApplication`. Esse é o padrão recomendado para Minimal APIs no .NET.

```csharp
public static class NomeController
{
    private static List<Entidade> Dados = new();  // Armazenamento em memória
    private static int idAtual = 1;                // ID sequencial

    public static void Listar(this WebApplication app)
    {
        app.MapGet("/api/entidade/listar", () => Results.Ok(Dados));
    }

    public static void Cadastrar(this WebApplication app)
    {
        app.MapPost("/api/entidade/cadastrar", (Entidade nova) =>
        {
            if (/* condição de erro */)
                return Results.BadRequest("mensagem");
            nova.Id = idAtual++;
            Dados.Add(nova);
            return Results.Ok(nova);
        });
    }
}
```

### 4.3. Contratos da API

#### Usuários — [`UsuariosController.cs`](src/usuarios/UsuariosController.cs:1)

**Modelo:**

| Campo | Tipo | Restrições |
|-------|------|-----------|
| `Id` | `int` | Auto-incremento |
| `Nome` | `string` | Obrigatório |
| `Email` | `string` | Obrigatório |
| `Cpf` | `string` | 11 caracteres, único |
| `Senha` | `string` | Mínimo 6 caracteres |
| `Adm` | `bool` | Default: false |

**Endpoints:**

| Método | Rota | Request Body | Response (Sucesso) | Response (Erro) |
|--------|------|-------------|-------------------|-----------------|
| `GET` | `/api/usuarios/listar` | — | `200 OK` + `List<Usuario>` | — |
| `POST` | `/api/usuarios/cadastrar` | `{nome, email, cpf, senha, adm}` | `200 OK` + `Usuario` | `400 BadRequest` (CPF inválido, senha curta, CPF duplicado) |

#### Eventos — [`EventosController.cs`](src/eventos/EventosController.cs:1)

**Modelo:**

| Campo | Tipo | Restrições |
|-------|------|-----------|
| `Id` | `int` | Auto-incremento |
| `Nome` | `string` | Único |
| `Descricao` | `string` | — |
| `Local` | `string` | — |
| `Data` | `DateTime` | Deve ser futura |
| `QuantidadeIngressos` | `int` | — |
| `ValorIngresso` | `float` | — |
| `FotoUrl` | `string?` | Opcional |

**Endpoints:**

| Método | Rota | Request Body | Response (Sucesso) | Response (Erro) |
|--------|------|-------------|-------------------|-----------------|
| `GET` | `/api/eventos/listar` | — | `200 OK` + `List<Evento>` | — |
| `GET` | `/api/eventos/listar/{id}` | — | `200 OK` + `Evento` | `404 Not Found` |
| `POST` | `/api/eventos/cadastrar` | `{nome, descricao, local, data, qtdIngressos, valor, fotoUrl}` | `200 OK` + `Evento` | `400 BadRequest` (nome duplicado, data passada) |

#### Cupons — [`CuponsController.cs`](src/cupons/CuponsController.cs:1)

> **Nota:** O modelo atual da API difere do modelo de banco planejado (seção 6). Na API, a chave primária é `Id` (int auto-incremento); no banco planejado, a PK será `Codigo` (VARCHAR). A API também não possui os campos `ValorMinimo`, `Ativo` e `DataCriacao` previstos no banco.

**Modelo:**

| Campo | Tipo | Restrições |
|-------|------|-----------|
| `Id` | `int` | Auto-incremento |
| `Codigo` | `string` | Único |
| `PercentualDesconto` | `int` | 0-100 |

**Endpoints:**

| Método | Rota | Request Body | Response (Sucesso) | Response (Erro) |
|--------|------|-------------|-------------------|-----------------|
| `GET` | `/api/cupons/listar` | — | `200 OK` + `List<Cupons>` | — |
| `POST` | `/api/cupons/cadastrar` | `{codigo, percentualDesconto}` | `200 OK` + `Cupons` | `400 BadRequest` (código duplicado) |

---

## 5. Frontend — Blazor Web App

### 5.1. Modos de Renderização

O frontend opera em **dois modos simultâneos**:

| Modo | Projeto | Funcionamento | Onde é usado |
|------|---------|--------------|-------------|
| **Interactive Server** | `billet_2/billet_2` | UI processada no servidor, atualizada via SignalR | Todas as páginas funcionais (Home, Login, Cadastro, Venda, etc.) |
| **WebAssembly** | `billet_2.Client` | Executa no navegador via WASM | Páginas de exemplo (Counter, Weather) — não funcionais no contexto do projeto |

### 5.2. Fluxo de Dados

```
Página Blazor ──HTTP──> API (localhost:5289)
       │                      │
       │                      ▼
       │              JSON Response
       │                      │
       ▼                      ▼
  Services (EventoService, UsuarioService)
       │
       ▼
  Componente Blazor (atualiza UI)
```

### 5.3. Serviços de Integração

#### [`AuthService`](billet_2/billet_2/Services/AuthService.cs) — Singleton

Serviço de autenticação baseado em estado na memória (sem JWT/sessão):

| Propriedade/Método | Descrição |
|-------------------|-----------|
| `UsuarioLogado` | `Usuario?` — usuário atualmente autenticado |
| `EstaLogado` | `bool` — flag de autenticação |
| `Logar(Usuario)` | Define o usuário logado |
| `Deslogar()` | Limpa o estado de login |

#### [`EventoService`](billet_2/billet_2/Services/EventoService.cs) — Scoped

| Método | Chamada HTTP | Retorno |
|--------|-------------|---------|
| `ListarEventosAsync()` | `GET /api/eventos/listar` | `List<Evento>?` |
| `BuscarPorIdAsync(int id)` | `GET /api/eventos/listar/{id}` | `Evento?` |
| `CriarEventoAsync(Evento)` | `POST /api/eventos/cadastrar` | `string?` (null = sucesso, string = mensagem de erro) |

#### [`UsuarioService`](billet_2/billet_2/Services/UsuarioService.cs) — Scoped

| Método | Chamada HTTP | Retorno |
|--------|-------------|---------|
| `ListarUsuariosAsync()` | `GET /api/usuarios/listar` | `List<Usuario>?` |
| `CadastrarAsync(Usuario)` | `POST /api/usuarios/cadastrar` | `string?` (null = sucesso, string = mensagem de erro) |

### 5.4. Páginas e Rotas

| Rota | Componente | Descrição | Requer Login? | Requer Admin? |
|------|-----------|-----------|:---:|:---:|
| `/` | [`Home.razor`](billet_2/billet_2/Components/Pages/Home.razor) | Hero banner com vídeo + grid de eventos da API | ❌ | ❌ |
| `/cadastro` | [`Cadastro.razor`](billet_2/billet_2/Components/Pages/Cadastro.razor) | Formulário: nome, email, CPF, senha, checkbox admin | ❌ | ❌ |
| `/login` | [`Login.razor`](billet_2/billet_2/Components/Pages/Login.razor) | Autenticação por CPF + senha (validação local no frontend — inseguro para produção) | ❌ | ❌ |
| `/poslogin` | [`Poslogin.razor`](billet_2/billet_2/Components/Pages/Poslogin.razor) | Dashboard principal com eventos + menu do usuário | ✅ | ❌ |
| `/vendas/{id}` | [`Venda.razor`](billet_2/billet_2/Components/Pages/Venda.razor) | Detalhes do evento + seleção de setor (VIP = 1.5x, Normal = 1x) | ❌ | ❌ |
| `/criarevento` | [`Criarevento.razor`](billet_2/billet_2/Components/Pages/Criarevento.razor) | Formulário de criação de evento | ✅ | ✅ |
| `/meusingressos` | [`Meusingressos.razor`](billet_2/billet_2/Components/Pages/Meusingressos.razor) | Visualização de ingressos adquiridos pelo usuário logado | ✅ | ❌ |

### 5.5. Fluxo de Navegação

```
                    ┌──────────┐
                    │   Home   │
                    │   (/)    │
                    └────┬─────┘
                         │
              ┌──────────┼──────────┐
              ▼          ▼          ▼
         ┌────────┐ ┌────────┐ ┌──────────┐
         │Cadastro│ │ Login  │ │ Venda/   │
         │(/cadastro)│(/login)│ │ {id}     │
         └───┬────┘ └───┬────┘ └──────────┘
             │          │
             └────┬─────┘
                  ▼
           ┌────────────┐
           │  Poslogin  │  ← Dashboard principal
           │ (/poslogin)│
           └─────┬──────┘
                 │
        ┌────────┼────────┐
        ▼        ▼        ▼
   ┌────────┐ ┌────────┐ ┌──────────────┐
   │ Criar  │ │ Venda/ │ │ Meus         │
   │ Evento │ │ {id}   │ │ Ingressos    │
   │(admin) │ │        │ │               │
   └────────┘ └────────┘ └──────────────┘
```

---

## 6. Modelo de Dados — Esquema Futuro (PostgreSQL)

> **Atenção:** Este modelo é o esquema **planejado** para quando houver integração com banco de dados. Atualmente a API utiliza apenas listas em memória (`List<T>`), e os modelos da API (seção 4.3) podem diferir do que está descrito aqui.

O script [`db/sql`](db/sql) define o esquema PostgreSQL com 4 tabelas e seus relacionamentos:

### 6.1. Tabelas e Relacionamentos

| Tabela | PK | Campos Principais | Relacionamentos |
|--------|----|-------------------|-----------------|
| **Usuarios** | `Id` (SERIAL) | `Nome`, `Email` (UNIQUE), `Cpf` (UNIQUE), `Senha`, `Adm` | 1:N com Reservas via `Cpf` |
| **Eventos** | `Id` (SERIAL) | `Nome`, `Descricao`, `Local`, `Data` (TIMESTAMP), `QuantidadeIngressos`, `ValorIngresso`, `FotoUrl` | 1:N com Reservas via `Id` |
| **Cupons** | `Codigo` (VARCHAR) | `PorcentagemDesconto` (0-100), `ValorMinimo` (>=0), `Ativo`, `DataCriacao` | 1:N com Reservas via `Codigo` |
| **Reservas** | `Id` (SERIAL) | `UsuarioCpf` (FK), `EventoId` (FK), `CupomUtilizado` (FK nullable), `ValorFinalPago` (>=0), `DataReserva` | N:1 com Usuarios, Eventos, Cupons |

### 6.2. Regras de Integridade

- `Reservas.ValorFinalPago` >= 0 (CHECK)
- `Cupons.PorcentagemDesconto` entre 0 e 100 (CHECK)
- `Cupons.ValorMinimo` >= 0 (CHECK)
- FK `Reservas.UsuarioCpf` → `Usuarios(Cpf)` com `ON DELETE RESTRICT`
- FK `Reservas.EventoId` → `Eventos(Id)` com `ON DELETE RESTRICT`
- FK `Reservas.CupomUtilizado` → `Cupons(Codigo)` com `ON DELETE SET NULL`
- Índices em: `Reservas(UsuarioCpf)`, `Reservas(EventoId)`, `Eventos(Data)`

---

## 7. Testes

### 7.1. Testes Unitários (xUnit)

Localização: [`tests/`](tests/)

| Arquivo | Classe | Tipo | Cenário Testado |
|---------|--------|------|-----------------|
| [`TesteDescontoValido.cs`](tests/TesteDescontoValido.cs) | `CupomTests` | `[Theory]` com `[InlineData(-5)]` e `[InlineData(120)]` | Desconto deve estar entre 0% e 100% |
| [`TesteEventoCapacidade.cs`](tests/TesteEventoCapacidade.cs) | `EventoTests` | `[Theory]` com `[InlineData(0)]` e `[InlineData(-10)]` | Capacidade do evento deve ser > 0 |
| [`TestePrecoPositivo.cs`](tests/TestePrecoPositivo.cs) | `EventoPrecoTests` | `[Fact]` | Preço do ingresso não pode ser negativo |
| [`TesteReservaValida.cs`](tests/TesteReservaValida.cs) | `ReservaValorTests` | `[Fact]` | Valor final da reserva não pode ser negativo |
| [`TesteReservaVazia.cs`](tests/TesteReservaVazia.cs) | `ReservaTests` | `[Fact]` | Reserva deve ter CPF de usuário associado |

### 7.2. Padrão Utilizado

Todos os testes seguem **Arrange-Act-Assert** com `Assert.False()` validando condições inválidas.

---

## 8. Limitações Conhecidas

| # | Limitação | Descrição | Impacto |
|---|-----------|-----------|---------|
| 1 | **Sem persistência** | Dados em `List<T>` na memória | Perda total ao reiniciar a API |
| 2 | **Autenticação frágil** | Login baixa todos os usuários e valida no frontend | Exposição de dados; sem sessão segura |
| 3 | **Solution incompleta** | `billet_2.slnx` não inclui `src/api.csproj` | Dois projetos precisam ser abertos separadamente |
| 4 | **Sem checkout real** | Carrinho local sem finalização | Compra não é concluída |
| 5 | **Sem envio de e-mail** | Funcionalidade F12 não implementada | Usuário não recebe ingresso |
| 6 | **Sem recuperação de senha** | Funcionalidade F08 não implementada | Usuário não redefine senha |
| 7 | **Script SQL sem extensão** | Arquivo `db/sql` sem `.sql` | Perdeu 1 ponto na correção AV1 |

### 8.1. Roadmap Recomendado

1. Renomear `db/sql` → `db/script.sql`
2. Integrar PostgreSQL com Dapper (ver [`ADR-001`](adr.md))
3. Implementar autenticação JWT com endpoint `/api/auth/login`
4. Adicionar `src/api.csproj` à solution `billet_2.slnx`
5. Migrar carrinho para o backend com endpoint de checkout
6. Implementar envio de e-mail e recuperação de senha
7. Adicionar testes de integração para os endpoints
8. Configurar CI/CD com GitHub Actions

---

## 9. Como Executar

### Pré-requisito
- .NET 10 SDK

### Terminal 1 — Backend (API)
```bash
cd src && dotnet run
# http://localhost:5289  |  OpenAPI: http://localhost:5289/openapi
```

### Terminal 2 — Frontend (Blazor)
```bash
cd billet_2/billet_2 && dotnet run
# http://localhost:5096
```

### Testes
```bash
cd tests && dotnet test
```

> **Ordem obrigatória:** A API deve estar rodando antes do frontend, pois as páginas buscam dados via HTTP.

---

## 10. Referências

| Documento | Descrição |
|-----------|-----------|
| [`docs/visao.md`](visao.md) | Visão do produto, stakeholders, funcionalidades |
| [`docs/historiasdeusuario.md`](historiasdeusuario.md) | 24 histórias de usuário + 24 cenários BDD |
| [`README.md`](../README.md) | Instruções de execução e visão geral |
| [`CORRECAO.md`](../CORRECAO.md) | Feedback AV1 (nota 8/10) |
| [`db/sql`](../db/sql) | Script DDL PostgreSQL |
