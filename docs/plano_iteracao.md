# Plano de Iteração — TripPrime

> **Itens AV2:** 10 (Plano de Iteração) e 11 (Quadro Visual e Limite de WIP)

---

## Plano de Iteração

**Objetivo da Iteração:** Consolidar a integração com banco de dados PostgreSQL via Dapper, implementar endpoints com JOIN e validações de negócio, e produzir a documentação completa do SDD (Software Design Document) para a AV2.

**Escopo (Backlog Selecionado):**

| ID | História de Usuário | Pontos | Responsável |
|:---|:---|:---:|:---|
| US-01 | Integrar PostgreSQL com Dapper e criar DbConnectionFactory | 5 | Dev Backend |
| US-02 | Implementar endpoint `GET /api/passagens/detalhadas` com INNER JOIN | 3 | Dev Backend |
| US-03 | Implementar endpoint `GET /api/passagens/detalhadas/usuario/{cpf}` com LEFT JOIN e `@Cpf` | 5 | Dev Backend |
| US-04 | Registrar endpoints de Eventos ausentes no Program.cs | 1 | Dev Backend |
| US-05 | Corrigir testes unitários: padrão AAA completo e nomenclatura underscore | 2 | Dev Backend |
| US-06 | Produzir documento de análise de padrões arquiteturais (`analise_arquitetura.md`) | 3 | Tech Lead |
| US-07 | Produzir ADR (`adrs/001-escolha-do-micro-orm.md`) | 2 | Tech Lead |
| US-08 | Produzir registro de dívida técnica (`registro_divida_tecnica.md`) | 3 | Tech Lead |
| US-09 | Produzir fluxo de manutenção (`fluxo_manutencao.md`) | 3 | Tech Lead |
| US-10 | Produzir plano de iteração (`plano_iteracao.md`) | 2 | Scrum Master |
| US-11 | Produzir métricas operacionais e SLO (`operacao.md`) | 5 | DevOps |
| US-12 | Produzir documento de segurança (`seguranca_ciclo.md`) | 3 | DevOps |
| US-13 | Produzir topologia de times (`topologia_times.md`) | 2 | Scrum Master |
| US-14 | Produzir release checklist (`release_checklist_final.md`) | 1 | Scrum Master |

**Entregáveis (Evidências):**

1. **Código:** `src/Database/DbConnectionFactory.cs`, `src/Program.cs` atualizado, `PassagensController.cs` com 2 novos endpoints JOIN
2. **Pacotes:** Dapper 2.1.66 e Npgsql 9.0.3 adicionados ao `api.csproj`
3. **Testes:** 5 testes com padrão AAA completo (`// Arrange`, `// Act`, `// Assert`) e nomenclatura `Metodo_Cenario_ResultadoEsperado`
4. **Documentação:** 9 arquivos Markdown no diretório `docs/` + `release_checklist_final.md` na raiz

**Risco Principal do Ciclo:** O banco de dados PostgreSQL pode não estar disponível no ambiente de desenvolvimento local de todos os integrantes. Como mitigação, os testes unitários não dependem de conexão real com banco, e os endpoints Dapper são estruturados para funcionar assim que a conexão for configurada.

**Definição de Pronto (DOD):**

1. Código compila sem erros (`dotnet build` passa em `src/`)
2. Todos os 5 testes unitários passam (`dotnet test` em `tests/`)
3. Nenhuma credencial hardcoded nos arquivos `.cs` em `src/`
4. Todos os endpoints estão registrados em `Program.cs`
5. Nenhuma concatenação ou interpolação de string em comandos SQL
6. Os 9 arquivos de documentação existem com o conteúdo especificado nos itens 03-20 da AV2

---

## Quadro Visual e Limite de WIP

O quadro Kanban da iteração possui 4 colunas. O limite de WIP é definido com base no tamanho da equipe (6 integrantes).

| Backlog | Em Desenvolvimento | Code Review | Concluído |
|:---|:---|:---|:---|
| US-07 | US-02 (Dev A) | US-01 (Dev A) | |
| US-09 | US-03 (Dev B) | US-05 (Dev B) | |
| US-10 | US-06 (Tech Lead) | | |
| US-11 | US-08 (Tech Lead) | | |
| US-12 | | | |
| US-13 | | | |
| US-14 | | | |

**WIP máximo: 4 tarefas** (número menor que os 6 integrantes do grupo, garantindo foco na conclusão antes de iniciar novas tarefas).

**Regras do Quadro:**

1. Nenhuma tarefa pode ser movida para "Code Review" sem que os testes passem localmente
2. Code Review requer aprovação de pelo menos 1 outro membro da equipe
3. Tarefas em "Em Desenvolvimento" não podem exceder o WIP máximo de 4
4. Se uma tarefa ficar bloqueada, ela retorna para "Backlog" com uma nota explicativa
