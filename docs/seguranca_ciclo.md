# Segurança no Ciclo de Desenvolvimento — TripPrime

> **Item AV2:** 19 — Threat Model e Gates de Segurança

---

## Threat Model — Rota de Maior Risco

### Rota Analisada: `POST /api/passagens/comprar`

Esta rota é a de maior risco porque:
1. Processa transações financeiras (compra de passagens)
2. Recebe dados sensíveis do usuário (CPF)
3. Aceita cupons de desconto que afetam o valor final da compra
4. Modifica estado de assentos (transição para "Vendido")

---

**Ativos Protegidos:**

| Ativo | Tipo | Sensibilidade |
|:---|:---|:---|
| CPF do usuário (`UsuarioCpf`) | PII (Personally Identifiable Information) | Alta — dado pessoal protegido pela LGPD |
| Preço da passagem (`PrecoPago`) | Dado financeiro | Média — integridade crítica (não pode ser manipulado pelo cliente) |
| Estado dos assentos (`Status`) | Dado operacional | Alta — inconsistência pode permitir overbooking (vender mesmo assento 2x) |
| Cupons de desconto (`CupomUtilizado`) | Dado promocional | Média — uso indevido causa prejuízo financeiro |
| Conexão com banco de dados | Infraestrutura | Crítica — acesso não autorizado compromete todos os ativos acima |

---

**Vetor de Ataque Provável:** Manipulação do campo `PrecoPago` no corpo da requisição

Um atacante pode tentar enviar um valor arbitrário no campo `PrecoPago` (ex: `0` ou `-100`) no JSON do request body:

```json
{
  "viagemId": 1,
  "assentoId": 5,
  "usuarioCpf": "12345678901",
  "cupomUtilizado": null,
  "precoPago": 0
}
```

Se o backend confiasse no valor enviado pelo cliente, o atacante compraria passagens de graça ou até com valor negativo (crédito).

---

**Falha Arquitetural Potencial:** Confiança implícita nos dados enviados pelo frontend

Em sistemas onde o frontend (Blazor) calcula o preço e o envia para o backend, um atacante pode usar ferramentas como Burp Suite ou Postman para interceptar a requisição HTTP e modificar o payload, ignorando completamente a lógica de frontend. Esta é a falha arquitetural clássica de "trust the client".

---

**Controle de Engenharia (Mitigação):** Cálculo do preço exclusivamente no backend

A implementação atual do TripPrime **já aplica esta mitigação** no `PassagensController.ComprarPassagem()`:

```csharp
// O preço NÃO é recebido do frontend — é calculado no backend
float precoBase = viagem.PrecoBase;
float percentualDesconto = 0;

if (!string.IsNullOrWhiteSpace(request.CupomUtilizado))
{
    var cupom = CuponsController.Cupons.FirstOrDefault(c =>
        c.Codigo.Equals(request.CupomUtilizado, StringComparison.OrdinalIgnoreCase));
    if (cupom == null)
        return Results.BadRequest($"Cupom '{request.CupomUtilizado}' não encontrado.");
    percentualDesconto = cupom.PercentualDesconto;
}

float precoPago = precoBase * (1 - (percentualDesconto / 100f));
if (precoPago < 0) precoPago = 0;
```

O modelo `CompraRequest` **não inclui** o campo `PrecoPago` — ele é calculado integralmente no servidor com base no `PrecoBase` da viagem e no percentual do cupom validado.

---

## Gates de Segurança

A equipe adotará os seguintes gates de segurança no pipeline de desenvolvimento:

### Gate 1: Análise Estática de Segurança (SAST) no Commit

**Quando:** A cada push em qualquer branch (acionado por webhook do GitHub)

**Ferramenta:** SonarQube Community Edition + regras de segurança OWASP

**Verificações:**
- Nenhuma string de conexão ou credencial hardcoded (Password, Pwd, User Id, ConnectionString= seguidos de valor literal)
- Nenhuma concatenação (`+`) ou interpolação (`$""`) em strings que contenham `SELECT`, `INSERT`, `UPDATE` ou `DELETE`
- Nenhum uso de `new SqlConnection` ou `new NpgsqlConnection` fora do `DbConnectionFactory`
- Nenhum endpoint que receba `PrecoPago` ou qualquer valor financeiro do request body

**Ação se falhar:** O push é bloqueado. O desenvolvedor recebe o relatório de violações e deve corrigir antes de tentar novamente.

---

### Gate 2: Testes de Segurança no Pull Request

**Quando:** Ao abrir um Pull Request para `main` ou `develop`

**Ferramenta:** OWASP ZAP (Zed Attack Proxy) em modo baseline scan + xUnit testes de segurança

**Verificações:**
- Scanner OWASP ZAP executa contra o ambiente de staging por 5 minutos
- Alertas de severidade High ou Critical bloqueiam o merge
- Testes automatizados de segurança incluem:
  - Tentativa de SQL Injection em todos os endpoints com parâmetros de query string
  - Tentativa de XSS nos campos de cadastro (`Nome`, `Email`)
  - Verificação de headers de segurança (Content-Security-Policy, X-Content-Type-Options, X-Frame-Options)

**Ação se falhar:** O merge é bloqueado até que todos os alertas High/Critical sejam resolvidos ou marcados como falso-positivo com justificativa documentada.

---

### Gate 3: Revisão Manual de Segurança antes do Deploy em Produção

**Quando:** Antes de cada deploy em produção (no estágio final do pipeline CI/CD)

**Ferramenta:** Checklist manual preenchido pelo Tech Lead ou DevOps

**Verificações:**
1. **Revisão de dependências:** `dotnet list package --vulnerable` não reporta vulnerabilidades conhecidas (CVE) com severidade High ou Critical
2. **Revisão de segredos:** Nenhum segredo (token, senha, chave de API) foi commitado acidentalmente — verificado com `git-secrets` ou `truffleHog`
3. **Revisão de CORS:** A política CORS em produção está restrita às origens corretas (não `AllowAnyOrigin`)
4. **Revisão de HTTPS:** `app.UseHttpsRedirection()` está ativo e HSTS está configurado
5. **Revisão de rate limiting:** Endpoint de login (`POST /api/usuarios/cadastrar`) possui rate limiting para prevenir enumeração de usuários
6. **Revisão de logs:** Nenhum dado sensível (CPF, senha, e-mail completo) está sendo logado em produção — verificar `appsettings.Production.json`

**Ação se falhar:** O deploy é bloqueado. O checklist deve ser completamente aprovado antes de prosseguir. O deploy só é liberado após todos os 6 itens serem marcados como `✅ Aprovado`.
