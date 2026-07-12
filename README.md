# TicketPrimeSolucao

Resolução da avaliação referente à matéria de **Engenharia de Software (UNIFESO)**.

Sistema de venda de ingressos composto por uma API backend e um frontend web, desenvolvidos em C# com .NET 10.

Integrantes 
Gabriel Castor 06009642
Gabriel Lepsch Monteiro 02001770
Gabriel Ribeiro 06010603
Lucas Oliveira 06010486
Luiz Eduardo P. Rosa 06010412
Thiago Zandonade Fernandes 06010263

## Tecnologias

| Camada | Tecnologia |
|---|---|
| Backend | ASP.NET Core 10 — Minimal API |
| Frontend | Blazor Web App (WebAssembly) |
| Linguagem | C# (.NET 10) |
| Estilo | Bootstrap 5 (via arquivos estáticos) |
| Banco de dados | Script SQL disponível (PostgreSQL) — **não integrado ao código** |

---

## Estrutura de Pastas

```
TicketPrimeSolucao-main/
│
├── billet_2.slnx                  ← Solution file (referencia apenas o frontend)
│
├── src/                           ← Backend: ASP.NET Core Minimal API
│   ├── Program.cs                 ← Ponto de entrada da API
│   ├── api.csproj
│   ├── api.http                   ← Exemplos de chamadas HTTP para teste
│   ├── appsettings.json
│   ├── eventos/
│   │   └── EventosController.cs   ← Endpoints de eventos
│   ├── usuarios/
│   │   └── Usuarios.cs            ← Endpoints de usuários
│   └── cupons/
│       └── CuponsController.cs    ← Endpoints de cupons
│
├── billet_2/                      ← Frontend: Blazor Web App
│   ├── billet_2/                  ← Projeto servidor
│   │   ├── Program.cs             ← Ponto de entrada do frontend
│   │   ├── Components/Pages/
│   │   │   ├── Home.razor         ← Listagem de eventos
│   │   │   ├── Cadastro.razor     ← Formulário de cadastro de usuário
│   │   │   └── Venda.razor        ← Detalhes do evento / ingresso
│   │   ├── Models/
│   │   │   ├── Evento.cs
│   │   │   └── Usuario.cs
│   │   ├── Services/
│   │   │   ├── EventoService.cs
│   │   │   └── UsuarioService.cs
│   │   └── wwwroot/               ← Assets estáticos (CSS, vídeo, imagens, Bootstrap)
│   └── billet_2.Client/           ← Projeto cliente WebAssembly
│
├── db/
│   └── sql                        ← Script DDL para criação das tabelas (PostgreSQL)
│
├── docs/
│   └── historiasdeusuario.md      ← Histórias de usuário
│
└── tela login/
    └── html                       ← Protótipo estático da tela de login (HTML puro)
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

---

## Endpoints da API

Todos os endpoints são prefixados com `/api`. Os dados são armazenados **em memória** — não há persistência entre reinicializações.

### Usuários

| Método | Rota | Descrição |
|---|---|---|
| `GET` | `/api/usuarios/listar` | Lista todos os usuários |
| `POST` | `/api/usuarios/cadastrar` | Cadastra um novo usuário |

**Corpo esperado para cadastro de usuário:**
```json
{
  "nome": "João Silva",
  "email": "joao@email.com",
  "cpf": "00000000000",
  "senha": "minhasenha"
}
```

### Eventos

| Método | Rota | Descrição |
|---|---|---|
| `GET` | `/api/eventos/listar` | Lista todos os eventos |
| `GET` | `/api/eventos/listar/{id}` | Busca evento por ID |
| `POST` | `/api/eventos/cadastrar` | Cadastra um novo evento |

**Corpo esperado para cadastro de evento:**
```json
{
  "nome": "Show de Rock",
  "descricao": "Uma noite incrível",
  "local": "Rio de Janeiro",
  "data": "2026-12-01T20:00:00",
  "quantidadeIngressos": 200,
  "valorIngresso": 150.00,
  "fotoUrl": "images/eventos/show_rock.jpg"
}
```

### Cupons

| Método | Rota | Descrição |
|---|---|---|
| `GET` | `/api/cupons/listar` | Lista todos os cupons |
| `POST` | `/api/cupons/cadastrar` | Cadastra um novo cupom |

**Corpo esperado para cadastro de cupom:**
```json
{
  "codigo": "PROMO10",
  "percentualDesconto": 10
}
```

---

## Páginas do Frontend

| Rota | Descrição |
|---|---|
| `/` | Home — lista os eventos cadastrados na API |
| `/cadastro` | Formulário de cadastro de usuário |
| `/vendas/{id}` | Detalhes do evento pelo ID |

---

## Banco de Dados

O arquivo `db/sql` contém o script DDL completo para criação das tabelas em **PostgreSQL**:

- `Usuarios`
- `Eventos`
- `Cupons`
- `Reservas` (com chaves estrangeiras e índices)

> **TODO:** O banco de dados não está integrado ao código. A API utiliza listas em memória (`List<T>`) em substituição a um banco real. Para integrar, seria necessário adicionar um ORM (ex: Entity Framework Core) e configurar a string de conexão em `appsettings.json`.

---

## Documentação OpenAPI

Em ambiente de desenvolvimento, a API expõe a documentação OpenAPI automaticamente em:

```
http://localhost:5289/openapi
```

---

## Limitações Conhecidas

- **Sem persistência:** todos os dados são perdidos ao reiniciar a API.
- **Sem autenticação:** a tela de login (`tela login/html`) é um protótipo estático sem funcionalidade real. Não há sistema de sessão, JWT ou cookies implementado.
- **Venda incompleta:** a página `/vendas/{id}` exibe os dados do evento, mas não possui fluxo de compra funcional.
- **Solution incompleta:** o `billet_2.slnx` referencia apenas o projeto Blazor. O projeto `src/api.csproj` não está incluído na solution.

---

## TODO

- [ ] Integrar banco de dados PostgreSQL usando o script em `db/sql`
- [ ] Implementar autenticação (login/sessão)
- [ ] Finalizar o fluxo de compra de ingresso na página de Venda
- [ ] Adicionar o projeto `src/api.csproj` à solution `billet_2.slnx`
- [ ] Implementar aplicação de cupons no processo de compra
- [ ] Corrigir chamada duplicada de cadastro em `Cadastro.razor`

---

## Licença

Distribuído sob a licença MIT. Consulte o arquivo `LICENSE` para mais informações.
