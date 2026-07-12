| ID | História de Usuário | Critérios de Pronto (DoD) |
|----|---------------------|---------------------------|
| 01 | Como visitante, quero visualizar eventos sem precisar fazer login para decidir se vale a pena me cadastrar ou comprar um ingresso. | • Lista de eventos visível na página inicial.<br>• Informações básicas (nome, data, local) exibidas.<br>• Botão de compra redireciona para login ou cadastro. |
| 02 | Como visitante, quero criar uma conta na plataforma para acessar os benefícios de usuário cadastrado e poder comprar ingressos. | • Formulário de cadastro funcional.<br>• Validação de campos obrigatórios.<br>• Verificação de e-mail único.<br>• Mensagem de sucesso após cadastro. |
| 03 | Como usuário cadastrado, quero realizar login na plataforma para acessar minha conta e meus ingressos. | • Tela de login disponível.<br>• Validação de e-mail e senha.<br>• Redirecionamento para área logada após autenticação. |
| 04 | Como usuário cadastrado, quero recuperar minha senha caso eu a esqueça para conseguir acessar minha conta novamente. | • Opção "Esqueci minha senha".<br>• Envio de link de recuperação por e-mail.<br>• Possibilidade de redefinir senha com segurança. |
| 05 | Como usuário, quero pesquisar eventos para encontrar rapidamente eventos do meu interesse. | • Campo de busca disponível.<br>• Resultados exibidos conforme termo pesquisado.<br>• Atualização da lista de eventos com base na pesquisa. |
| 06 | Como usuário, quero filtrar eventos por região, categoria ou data para facilitar a busca por eventos relevantes. | • Filtros disponíveis na página de eventos.<br>• Aplicação dos filtros atualiza os resultados exibidos.<br>• Possibilidade de limpar filtros aplicados. |
| 07 | Como usuário, quero acessar uma tela de descrição do evento para visualizar detalhes como data, local, preço e informações importantes. | • Página de detalhes do evento disponível.<br>• Exibição de data, local, descrição e preço.<br>• Botão para adicionar ingresso ao carrinho. |
| 08 | Como usuário, quero compartilhar eventos para convidar amigos ou divulgar eventos de interesse. | • Botões de compartilhamento disponíveis.<br>• Geração de link direto para o evento.<br>• Compatibilidade com redes sociais ou aplicativos de mensagem. |
| 09 | Como usuário, quero adicionar ingressos ao carrinho de compras para realizar a compra posteriormente. | • Botão "Adicionar ao carrinho" disponível.<br>• Atualização do carrinho com itens selecionados.<br>• Exibição da quantidade de ingressos no carrinho. |
| 10 | Como usuário cadastrado, quero acessar a tela de pagamento para finalizar a compra dos ingressos selecionados. | • Página de checkout disponível.<br>• Exibição do resumo da compra.<br>• Opções de pagamento disponíveis.<br>• Confirmação da compra após pagamento. |
| 11 | Como usuário cadastrado, quero receber meus ingressos por e-mail, SMS ou WhatsApp para garantir acesso ao evento. | • Envio automático do ingresso após confirmação de pagamento.<br>• Disponibilidade de QR Code ou código de barras.<br>• Confirmação de envio ao usuário. |
| 12 | Como usuário cadastrado, quero visualizar meus ingressos adquiridos para garantir minha entrada no evento. | • Tela "Meus Ingressos" acessível no perfil.<br>• Listagem dos ingressos comprados.<br>• Exibição de QR Code ou código de validação. |
| 13 | Como usuário cadastrado, quero que o sistema limite a compra de ingressos a um por CPF para evitar duplicidade de compras. | • Verificação de CPF no banco de dados.<br>• Bloqueio de nova compra se já existir ingresso vinculado.<br>• Mensagem informativa ao usuário. |
| 14 | Como usuário, quero acessar uma aba de eventos disponíveis para facilitar a escolha de ingressos. | • Aba "Eventos" visível no menu principal.<br>• Lista de eventos carregada corretamente.<br>• Navegação para detalhes do evento. |
| 15 | Como usuário, quero visualizar os eventos em uma interface otimizada para facilitar a navegação e escolha dos ingressos. | • Interface responsiva (mobile e desktop).<br>• Cards com imagem, título e preço do evento.<br>• Navegação intuitiva entre eventos. |
| 16 | Como usuário cadastrado, quero inserir cupons de desconto na tela de pagamento para reduzir o valor da compra quando disponível. | • Campo para inserir cupom no checkout.<br>• Validação do cupom.<br>• Atualização automática do valor total. |
| 17 | Como usuário cadastrado, quero reservar vagas em eventos futuros para garantir minha participação. | • Botão de reserva disponível em eventos habilitados.<br>• Registro da reserva no sistema.<br>• Visualização da reserva no perfil do usuário. |
| 18 | Como administrador, quero criar novos eventos na plataforma para disponibilizar ingressos aos usuários. | • Formulário de criação de evento no painel admin.<br>• Campos para data, local, descrição e preço.<br>• Evento publicado no catálogo após salvar. |
| 19 | Como administrador, quero visualizar todos os eventos criados para gerenciar e acompanhar os eventos cadastrados. | • Tela administrativa com lista de eventos.<br>• Exibição de status do evento.<br>• Opção de editar ou visualizar evento. |
| 20 | Como administrador, quero cadastrar e liberar lotes de ingressos para controlar preços e disponibilidade ao longo das vendas. | • Cadastro de múltiplos lotes por evento.<br>• Definição de quantidade e preço por lote.<br>• Atualização automática da disponibilidade. |
| 21 | Como administrador, quero cancelar eventos quando necessário para evitar vendas ou participação em eventos cancelados. | • Opção de cancelar evento no painel admin.<br>• Bloqueio automático de vendas.<br>• Atualização do status do evento para "Cancelado". |
| 22 | Como administrador, quero definir um limite de ingressos por evento para evitar superlotação. | • Campo para definir quantidade máxima de ingressos.<br>• Sistema impede vendas após atingir limite.<br>• Exibição de "Esgotado". |
| 23 | Como administrador, quero limitar o uso de apenas um cupom por ingresso para evitar prejuízos financeiros. | • Validação que impede uso de múltiplos cupons.<br>• Exibição de aviso ao usuário.<br>• Aplicação de apenas um desconto por compra. |
| 24 | Como administrador, quero acessar uma tela para visualizar e confirmar ingressos vendidos para controle de participantes. | • Tela com lista de ingressos vendidos.<br>• Exibição de dados do comprador.<br>• Possibilidade de confirmar ou validar ingresso. |
----------------------------------------------------

## BDD das Histórias de Usuário

- 01:

    Dado: Que sou visitante e não tenho conta criada

    Quando: Acessar a página home

    Então: Os eventos disponíveis são apresentados para o usuário

- 02:

    Dado: Que sou visitante e não tenho conta criada

    Quando: Clicar no botão para criar conta

    Então: Deve ser aberto a página ou o modal para cadastro da conta e a conta deve ser criada

- 03:

    Dado: Que tenho uma conta criada

    Quando: Clicar no botão de fazer login

    Então: Devo ser direcionado para á página ou modal de login e o login deve ser realizado corretamente

- 04:

    Dado: Que tenho uma conta cadastrada

    Quando: Clicar no botão de esqueci minha senha

    Então: Deve ser enviado um email para o email cadastrado na conta para a atualização da senha

- 05:

    Dado: Que estou na tela home

    Quando: Preencher o campo de pesquisa e clicar para pesquisar

    Então: Deve me retornar os eventos com o nome que pesquisei

- 06: 

    Dado: Que estou na tela home

    Quando: Selecionar um filtro 

    Então: Devem ser apresentados os eventos com relação ao filtro que escolhi (Região, data, categoria, etc)

- 07:

    Dado: Estou visualizando os eventos

    Quando: Clicar em um evento

    Então: Deve ser exibida a tela de observações sobre aquele evento com todas as suas informações (data, preço, local, etc)

- 08:

    Dado: Que estou na tela de observação de um evento

    Quando: Clicar no botão para compartilhar

    Então: Deve ser apresentada uma tela com as opçoes de redes sociais para compartilhamento ou um botão para copiar o link

- 09:

    Dado: Que estou na tela de observação de um evento

    Quando: Clicar no botão de comprar

    Então: O ingresso deste evento deve ser adicionado ao carrinho 

- 10:

    Dado: Estou no carrinho

    Quando: Clicar para finalizar a compra

    Então: Deve ser direcionado para a tela de pagamento com as opções de pagamento e permitindo pagar para finalizar a compra

- 11:

    Dado: Estou na tela de pagamento do ingresso

    Quando: Finalizar o pagamento e a compra for finalizada

    Então: Deve ser apresentada a opção de receber o ingresso por email e o ingresso deve ser enviado para o email cadastrado na conta

- 12:

    Dado: Que estou logado na minha conta

    Quando: Clicar no botão de meus ingressos

    Então: Devem ser exibidos todos os ingressos que adquiri

- 13:

    Dado: Que estou logado e já comprei um ingresso com meu CPF

    Quando: Tentar comprar outro ingresso

    Então: O sistema deve bloquear a compra e exibir uma mensagem informativa

- 14:

    Dado: Que estou na plataforma

    Quando: Acessar a aba de eventos

    Então: Deve ser exibida uma lista de eventos disponíveis com opção de visualizar detalhes

- 15:

    Dado: Que estou acessando a aba de eventos

    Quando: Visualizar os eventos disponíveis

    Então: Deve ser exibida uma interface responsiva com cards contendo imagem, título e preço

- 16:

    Dado: Que estou na tela de pagamento

    Quando: Inserir um cupom de desconto válido

    Então: O sistema deve validar o cupom e atualizar automaticamente o valor total da compra

- 17:

    Dado: Que estou logado e acessando um evento futuro

    Quando: Clicar no botão de reservar vaga

    Então: A reserva deve ser registrada e exibida no meu perfil

- 18:

    Dado: Que estou logado como administrador

    Quando: Preencher o formulário e criar um novo evento

    Então: O evento deve ser salvo e publicado no catálogo

- 19:

    Dado: Que estou no painel administrativo

    Quando: Acessar a lista de eventos

    Então: Devem ser exibidos todos os eventos com status e opção de editar ou visualizar

- 20:

    Dado: Que estou gerenciando um evento no painel admin

    Quando: Cadastrar lotes de ingressos

    Então: O sistema deve salvar os lotes com quantidade e preço e atualizar a disponibilidade

- 21:

    Dado: Que estou no painel administrativo

    Quando: Cancelar um evento

    Então: O sistema deve bloquear vendas e atualizar o status para "Cancelado"

- 22:

    Dado: Que estou criando ou editando um evento

    Quando: Definir um limite máximo de ingressos

    Então: O sistema deve impedir vendas após atingir o limite e exibir "Esgotado"

- 23:

    Dado: Que estou na tela de pagamento

    Quando: Tentar aplicar mais de um cupom

    Então: O sistema deve permitir apenas um cupom e exibir um aviso ao usuário

- 24:

    Dado: Que estou no painel administrativo

    Quando: Acessar a tela de ingressos vendidos

    Então: Devem ser exibidos os dados dos compradores e a opção de confirmar ou validar ingressos