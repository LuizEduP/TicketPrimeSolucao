# Registro de Dívida Técnica — TripPrime

> **Itens AV2:** 06 (Tabela de Dívida Técnica) e 07 (Priorização)

---

## Tabela de Dívidas Técnicas

| ID da Dívida | Descrição Técnica | Freq. Alteração | Risco | Esforço | Decisão |
|:---|:---|:---:|:---:|:---:|:---|
| DT-001 | **Ausência de persistência de dados.** A API utiliza listas em memória (`List<T>`) como armazenamento principal. Dapper já foi adicionado ao projeto, mas endpoints como `CadastrarViagens`, `CadastrarVeiculos`, `ReservarAssento` e `ComprarPassagem` ainda utilizam listas estáticas. Todos os dados são perdidos ao reiniciar a API. | Alto | Alto | Médio | Prioridade 1 (Imediato) |
| DT-002 | **Autenticação frágil no frontend.** O `Login.razor` baixa todos os usuários da API (`GET /api/usuarios/listar`) e valida credenciais no lado do cliente. Senhas são comparadas em texto plano no navegador. Não há JWT, sessão ou qualquer mecanismo seguro de autenticação. | Baixo | Alto | Médio | Prioridade 1 (Imediato) |
| DT-003 | **Solução desatualizada não inclui projeto API.** O arquivo `billet_2.slnx` referencia apenas o projeto Blazor frontend. O projeto `src/api.csproj` não está incluído na solution, forçando os desenvolvedores a abrir dois projetos separadamente no IDE. | Baixo | Baixo | Baixo | Prioridade 2 (Próxima Sprint) |
| DT-004 | **Script SQL com arquivo duplicado sem extensão.** Existem dois arquivos de script DDL: `db/script.sql` (com extensão correta) e `db/sql` (sem extensão). O conteúdo é quase idêntico, mas o arquivo `db/sql` possui um índice em `Eventos(DataEvento)` que referencia uma coluna inexistente no esquema atual. | Baixo | Médio | Baixo | Prioridade 2 (Próxima Sprint) |
| DT-005 | **Endpoint de Eventos sem registro.** O `EventosController.cs` existe no código fonte com 3 endpoints (`CadastrarEventos`, `ListarEventos`, `ListarEventoPorId`), mas não está registrado no `Program.cs`. Isso foi corrigido, mas demonstra fragilidade no processo de registro manual de rotas — não há validação automatizada que garanta que todos os controllers estejam registrados. | Médio | Médio | Baixo | Prioridade 2 (Próxima Sprint) |
| DT-006 | **Cupons sem validação de percentual.** O `CuponsController.cs` não valida se `PercentualDesconto` está entre 0 e 100 no endpoint `CadastrarCupons`. A validação existe apenas no script SQL (`CHECK (PorcentagemDesconto BETWEEN 0 AND 100)`) mas não é aplicada na camada de aplicação. | Baixo | Baixo | Baixo | Prioridade 3 (Aceitar/Ignorar) |
| DT-007 | **Ausência de testes de integração.** A pasta `tests/` contém apenas 5 testes unitários simples que validam lógica booleana isolada (`Assert.False`). Não existem testes de integração que exercitem os endpoints da API com chamadas HTTP reais, nem testes que validem o fluxo completo de compra de passagem. | Médio | Alto | Alto | Prioridade 3 (Aceitar/Ignorar) |
| DT-008 | **CORS configurado com AllowAnyHeader/AllowAnyMethod.** A política CORS `BlazorPolicy` permite qualquer header e método de `http://localhost:5096`, mas em produção isso seria inadequado. Os métodos HTTP permitidos deveriam ser restritos a GET e POST, e os headers a Content-Type e Authorization. | Baixo | Médio | Baixo | Prioridade 3 (Aceitar/Ignorar) |

---

## Legenda das Colunas

- **Freq. Alteração:** Frequência com que o código afetado é modificado (Alto = várias vezes por sprint; Médio = algumas vezes por mês; Baixo = raramente alterado)
- **Risco:** Probabilidade de a dívida causar incidente em produção (Alto = provável; Médio = possível; Baixo = improvável)
- **Esforço:** Estimativa de trabalho para resolver a dívida (Alto = múltiplos dias; Médio = um dia; Baixo = algumas horas)
- **Decisão:**
  - **Prioridade 1 (Imediato):** Deve ser resolvida na sprint atual — risco inaceitável
  - **Prioridade 2 (Próxima Sprint):** Agendada para a próxima iteração
  - **Prioridade 3 (Aceitar/Ignorar):** Reconhecida mas postergada indefinidamente
