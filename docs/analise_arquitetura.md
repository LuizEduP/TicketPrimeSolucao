# Análise de Padrões Arquiteturais e Violações — TripPrime

> **Itens AV2:** 03 (Análise de Padrões Arquiteturais) e 04 (Análise de Violações Arquiteturais)

---

## Parte 1: Análise de Padrões Arquiteturais

Para cada um dos 3 cenários abaixo, identifique o padrão arquitetural mais provável e descreva pelo menos 1 trade-off.

### Cenário 1: Sistema de E-commerce com milhões de produtos

Uma grande rede varejista precisa de um sistema de e-commerce que suporte catálogo com milhões de produtos, picos de acesso na Black Friday e tolerância a falhas em componentes individuais (ex: sistema de recomendação não pode derrubar o carrinho de compras).

**Padrão Arquitetural Provável:** Microsserviços (Microservices)

**Trade-off:**
- **Positivo:** Cada serviço (catálogo, carrinho, checkout, recomendação) pode ser escalado independentemente conforme a demanda, permitindo alocar mais recursos ao checkout durante a Black Friday sem afetar outros componentes.
- **Negativo:** A complexidade operacional aumenta significativamente — é necessário gerenciar comunicação entre serviços, consistência eventual de dados, service discovery, circuit breakers e monitoramento distribuído, exigindo uma equipe de DevOps dedicada.

---

### Cenário 2: Aplicativo de Notas com sincronização offline

Um desenvolvedor solo está criando um aplicativo de notas que precisa funcionar offline no celular e sincronizar automaticamente quando houver conexão com a internet. O app será usado por algumas centenas de usuários.

**Padrão Arquitetural Provável:** Arquitetura em Camadas (Layered/N-Tier) com Sincronização Offline-First

**Trade-off:**
- **Positivo:** A simplicidade da arquitetura em camadas (UI → Lógica de Negócio → Armazenamento Local → Sincronização Remota) permite que um desenvolvedor solo implemente e mantenha o sistema inteiro, com clara separação de responsabilidades e baixo custo de desenvolvimento.
- **Negativo:** Conflitos de sincronização (ex: mesma nota editada em dois dispositivos antes da sincronização) precisam ser resolvidos manualmente com estratégias de merge que adicionam complexidade à camada de sincronização, e a arquitetura não escala bem se o número de usuários crescer para milhões.

---

### Cenário 3: Sistema de Processamento de Vídeos sob demanda

Uma plataforma de streaming como YouTube precisa processar vídeos enviados por usuários: transcodificar para múltiplas resoluções, gerar thumbnails, detectar conteúdo impróprio com IA e disponibilizar para streaming global via CDN.

**Padrão Arquitetural Provável:** Arquitetura Orientada a Eventos (Event-Driven) com Filas de Mensagens

**Trade-off:**
- **Positivo:** O desacoplamento via eventos permite que cada etapa do pipeline (upload → transcodificação → thumbnail → moderação → CDN) seja processada por workers independentes que podem escalar horizontalmente, e falhas em uma etapa (ex: moderação de IA offline) não bloqueiam as outras etapas.
- **Negativo:** A rastreabilidade e depuração tornam-se mais difíceis — quando um vídeo não é processado corretamente, é necessário rastrear o fluxo através de múltiplos serviços e filas para identificar onde ocorreu a falha, exigindo ferramentas de distributed tracing.

---

## Parte 2: Análise de Violações Arquiteturais

Analise o trecho de código C# abaixo e liste pelo menos 5 violações arquiteturais.

### Trecho de Código para Análise

```csharp
public class PedidoController : ControllerBase
{
    private readonly string _connectionString = "Server=prod-db;Database=vendas;User Id=admin;Password=admin123;";

    [HttpPost]
    public IActionResult CriarPedido(Pedido pedido)
    {
        // Valida o pedido
        if (pedido.Itens.Count == 0)
            return BadRequest("Pedido sem itens");

        // Calcula o total
        decimal total = 0;
        foreach (var item in pedido.Itens)
        {
            total += item.Preco * item.Quantidade;
        }
        pedido.Total = total;

        // Salva no banco
        using (var conn = new SqlConnection(_connectionString))
        {
            conn.Open();
            var sql = "INSERT INTO Pedidos (ClienteId, Total, Status) VALUES (" 
                      + pedido.ClienteId + ", " + pedido.Total + ", 'Novo')";
            var cmd = new SqlCommand(sql, conn);
            cmd.ExecuteNonQuery();

            // Envia e-mail de confirmação diretamente
            var smtp = new SmtpClient("smtp.empresa.com");
            smtp.Send("vendas@empresa.com", pedido.EmailCliente, 
                      "Pedido Confirmado", "Seu pedido #" + pedido.Id + " foi criado!");
        }

        return Ok(pedido);
    }
}
```

### Violações Identificadas

---

**Problema:** Credenciais hardcoded no código fonte
**Evidência:** `private readonly string _connectionString = "Server=prod-db;Database=vendas;User Id=admin;Password=admin123;";` — a string de conexão contém User Id e Password em texto plano no código C#.
**Impacto:** Exposição de credenciais de banco de dados de produção no repositório de código. Qualquer pessoa com acesso ao código fonte obtém acesso ao banco de produção. Violação grave de segurança (OWASP A07:2021).
**Ação Recomendada:** Remover a string de conexão hardcoded e utilizar `builder.Configuration.GetConnectionString("DefaultConnection")` ou variáveis de ambiente (`Environment.GetEnvironmentVariable`). Armazenar credenciais em `appsettings.json` com gerenciamento de secrets (Azure Key Vault, AWS Secrets Manager ou .NET Secret Manager).

---

**Problema:** SQL Injection — concatenação de valores em comando SQL
**Evidência:** `var sql = "INSERT INTO Pedidos (ClienteId, Total, Status) VALUES (" + pedido.ClienteId + ", " + pedido.Total + ", 'Novo')";` — os valores são concatenados diretamente na string SQL usando o operador `+`.
**Impacto:** Um atacante pode injetar comandos SQL maliciosos através dos campos do pedido, potencialmente roubando, alterando ou destruindo dados do banco. Ex: `pedido.ClienteId = "1); DROP TABLE Pedidos; --"`.
**Ação Recomendada:** Utilizar parâmetros nomeados com Dapper (`new { ClienteId = pedido.ClienteId, Total = pedido.Total }`) ou `SqlParameter` com ADO.NET puro. Seguir ADR-001 que proíbe concatenação de strings em comandos SQL.

---

**Problema:** Violação do Princípio de Responsabilidade Única (SRP — SOLID)
**Evidência:** A classe `PedidoController` realiza simultaneamente: (1) validação de entrada, (2) cálculo de total do pedido, (3) persistência no banco de dados via SQL puro, (4) envio de e-mail via SMTP. Quatro responsabilidades completamente distintas em um único método.
**Impacto:** Dificuldade de manutenção e teste — qualquer alteração na lógica de e-mail, cálculo de preço ou persistência exige modificar o controller. Impossível testar isoladamente cada responsabilidade. Alto acoplamento entre camadas.
**Ação Recomendada:** Separar em serviços especializados: `PedidoService` (lógica de negócio + validação), `PedidoRepository` (persistência via Dapper), `EmailService` (envio de notificações). O controller deve apenas orquestrar as chamadas.

---

**Problema:** Ausência de tratamento de transações e resiliência
**Evidência:** O código salva o pedido no banco e envia e-mail de confirmação em sequência sem transação ou compensação. Se o envio de e-mail falhar, o pedido já foi persistido e o cliente não recebe confirmação. Se o banco falhar após o e-mail ser enviado, o cliente recebe confirmação de um pedido que não existe.
**Impacto:** Inconsistência de dados e experiência do usuário — estados inválidos onde um pedido existe sem confirmação ou uma confirmação existe sem pedido correspondente.
**Ação Recomendada:** Implementar o padrão Saga ou Outbox Pattern: primeiro persiste o pedido e uma mensagem de "e-mail pendente" na mesma transação; um worker separado processa a fila de e-mails pendentes com retry policy.

---

**Problema:** Lógica de negócio no Controller — falta de separação de camadas
**Evidência:** O cálculo `total += item.Preco * item.Quantidade` e a validação `pedido.Itens.Count == 0` estão implementados diretamente no método do controller, sem delegação para uma camada de serviço ou domínio.
**Impacto:** A lógica de negócio fica acoplada ao framework HTTP (ASP.NET Core), impossibilitando reutilização em outros contextos (ex: processamento batch, API GraphQL, mensageria). Dificulta testes unitários pois exige mock do `ControllerBase`.
**Ação Recomendada:** Extrair a lógica de validação e cálculo para uma classe de domínio `Pedido` com método `CalcularTotal()` e validação `Validar()`, seguindo o padrão Rich Domain Model. O controller apenas chama `pedido.Validar()` e `pedidoService.Criar(pedido)`.

---

**Problema:** Uso de `new SqlConnection` diretamente — ausência de injeção de dependência
**Evidência:** `using (var conn = new SqlConnection(_connectionString))` — a conexão com o banco é instanciada diretamente no método, com a string de conexão armazenada em campo privado.
**Impacto:** Dificuldade de teste unitário (não é possível mockar a conexão), violação do Dependency Inversion Principle (SOLID), e ausência de gerenciamento centralizado do ciclo de vida da conexão.
**Ação Recomendada:** Utilizar injeção de dependência com `DbConnectionFactory` (já implementada no projeto TripPrime) que é registrada como singleton no container DI. O controller deve receber `DbConnectionFactory` por injeção no construtor ou como parâmetro do Minimal API endpoint.

---
