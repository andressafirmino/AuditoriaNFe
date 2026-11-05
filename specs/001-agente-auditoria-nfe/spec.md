# Feature Specification: Agente AuditoriaNFe — análise de XMLs de NF-e via A2A

**Feature Branch**: `001-agente-auditoria-nfe`

**Created**: 2026-10-29

**Status**: Draft

**Input**: User description: "Crie a especificação funcional de uma aplicação chamada AuditoriaNFe. A
aplicação será um agente de IA especializado em análise de XMLs de NF-e, que será utilizado
posteriormente por um orquestrador chamado SOFIA através do protocolo A2A. O agente deverá: receber
perguntas em linguagem natural; receber múltiplos arquivos XML; validar limite de tamanho total dos
arquivos; validar layout a partir da versão; validar conteúdo (campos de somatórios, etc.); gerar um
parecer sobre a base analisada; permitir continuidade de uma conversa por sessão; impedir a criação
de informações financeiras inexistentes; gerar recomendações demonstrativas, claramente
identificadas como tais; expor suas funcionalidades por A2A; publicar um Agent Card para descoberta
pela SOFIA; disponibilizar um projeto cliente chamado Sofia.Simulator para validar chamadas A2A.
Defina requisitos funcionais, requisitos não funcionais, histórias de usuário, critérios de
aceitação, tratamento de erros e cenários de teste."

**Constitution**: v2.1.0 — princípios P01–P18 aplicáveis. Os termos DEVE, NÃO DEVE e PODE seguem a
convenção da constitution.

## Clarifications

### Session 2026-10-29

- Q: Qual o limite de tamanho total e de quantidade de arquivos por solicitação? → A: 5 MB no total
  e até 20 arquivos, configuráveis.
- Q: O que a sessão guarda para perguntas de continuidade? → A: Apenas dados extraídos e resultados
  das análises; o XML original é descartado após a análise.
- Q: A ingestão dos manuais oficiais faz parte desta feature? → A: Não; é a feature separada 002,
  da qual esta depende.
- Q: O sistema utilizará banco de dados, e quais? → A: Sim. MongoDB para memória conversacional e
  sessões; PostgreSQL para consumo e custos de IA; PostgreSQL com PgVector para a base de
  conhecimento (feature 002).
- Q: Onde o agente será hospedado e qual o provedor de IA? → A: ASP.NET Core; provedor de IA OpenAI.
- Q: Quais ferramentas de engenharia serão usadas? → A: Repositório no GitHub, documentação da API
  com Swagger, testes automatizados e Docker; Vercel onde for aplicável (ver Limitações técnicas).
- Q: Quem controla a liberação de recomendações sensíveis? → A: Aprovação humana.
- Q: Onde os checkpoints de aprovação são persistidos? → A: Em arquivos JSON locais, para
  demonstração.
- Q: A comunicação A2A será real ou simulada? → A: Real, utilizando o SDK oficial do protocolo A2A.
- Q: O que o Sofia.Simulator faz? → A: Descobre o Agent Card e envia uma solicitação A2A.
- Q: Como o projeto trata MCP? → A: O projeto será preparado para futuras integrações MCP.
- Q: Quais recomendações são sensíveis e exigem aprovação humana? → A: As vinculadas a achado de
  severidade Alta ou que mencionem valores financeiros ou tributos.
- Q: Como a aprovação acontece e o que a SOFIA recebe enquanto está pendente? → A: O parecer é
  entregue na hora com as recomendações sensíveis retidas como "pendente de aprovação"; um aprovador
  decide pelo canal direto, e a SOFIA consulta depois as recomendações liberadas.
- Q: Qual o escopo obrigatório do Sofia.Simulator? → A: Fluxo básico obrigatório (descobrir o Agent
  Card e enviar uma solicitação A2A com XMLs e pergunta); sessão e cenários de erro são opcionais;
  os demais cenários ficam nos testes automatizados.
- Q: Como tratar a consulta aos manuais enquanto a integração MCP não existir? → A: O ponto de
  integração MCP fica preparado e desligado por configuração; a etapa é registrada como "não
  aplicável — integração não habilitada", com limitação declarada e revisão humana, e o resultado
  segue as validações determinísticas. Com a integração ligada, uma falha segue o ERR-013.

### Session 2026-11-04

- Q: Quais validações a versão 1 executa? → A: Validação de estrutura (schema XSD) e conferência de
  totalizadores do catálogo de regras; outras regras de conteúdo só entram se aprovadas no catálogo.
- Q: Como o agente escolhe o schema de cada NF-e, se todos os pacotes usam a versão 4.00? → A: Pela
  vigência do pacote de schema na data de referência: data de recebimento pela SEFAZ (`dhRecbto`)
  quando houver protocolo de autorização; senão, data de emissão (`dhEmi`). Cada pacote vale do
  início de sua produção até o início do pacote seguinte; o último vale até a data atual.
- Q: Quais são as fontes oficiais desta feature? → A: Os pacotes de schema oficiais do leiaute 4.00
  publicados no Portal Nacional da NF-e (séries PL_009 e PL_010), o Anexo I do MOC 7.0 (versão 7.00)
  e as notas técnicas, inventariados com origem, versão e SHA-256.
- Q: Como o agente identifica uma NF-e suportada? → A: Pelo elemento raiz (`NFe` ou `nfeProc`), pelo
  namespace da NF-e, pela versão 4.00 e pelo modelo 55 em `ide/mod` e na chave de acesso; qualquer
  divergência torna o documento não suportado. A NFC-e é recusada na identificação, antes do schema.
- Q: Como funciona a autenticação? → A: A versão 1 não tem autenticação. O chamador informa nos
  metadados o identificador do usuário e a sua própria identificação, que são registrados e usados
  para isolamento, sem verificação. A autenticação do chamador por credencial de serviço
  (constitution 5.2) fica para implementação futura, como desvio temporário D-05, e o agente roda
  apenas em ambiente local de testes.
- Q: O agente mantém informações sobre o usuário entre conversas? → A: Sim. O agente mantém um
  contexto do usuário próprio da auditoria, separado do histórico, em MongoDB; o contexto geral do
  usuário pertence à SOFIA.
- Q: O contrato com a SOFIA está definido? → A: Não. O contrato A2A desta feature é uma proposta v1
  provisória até a resolução da DP-27; o núcleo do agente é testável pelo canal direto.
- Q: Quais informações o agente guarda no contexto de cada usuário? → A: CNPJs emitentes e
  destinatários já auditados pelo usuário, com a quantidade de documentos; resumo dos achados
  recorrentes (regra ou tipo de erro e frequência), derivados dos resultados dos validadores; e
  preferências declaradas explicitamente pelo usuário (nível de detalhe do parecer e tributos de
  interesse).
- Q: Como o agente reconhece quem pode aprovar recomendações sensíveis sem login? → A: Na versão 1
  não há controle de papel: o aprovador é o usuário informado na solicitação, registrado no
  checkpoint sem verificação. O controle de quem pode aprovar fica para implementação futura,
  junto com a autenticação (desvio D-05).
- Q: Por quanto tempo o contexto do usuário fica guardado? → A: 90 dias sem atividade do usuário,
  configurável; depois disso, o contexto é excluído automaticamente.
- Q: O que acontece se a solicitação vier sem identificador do usuário ou do chamador? → A: A
  solicitação é aceita com valores padrão de testes (usuário "anonimo", chamador "nao-informado"),
  e nenhum contexto do usuário é lido ou gravado.
- Q: O que fazer quando a data de referência cai no período de convivência entre dois pacotes? → A:
  Validar contra o pacote vigente na data; se houver erro estrutural e a data estiver na convivência
  do pacote anterior, validar também contra o anterior. Se o anterior não apresentar erros, o
  documento é considerado válido no layout, e o parecer informa o pacote aplicado e que a data
  estava em convivência.
- Q: Como tratar o limite de taxa e a indisponibilidade do armazenamento de sessões, que não tinham
  código de erro? → A: Com dois códigos novos: ERR-024 (limite de taxa ou de análises simultâneas,
  com o tempo para nova tentativa) e ERR-025 (armazenamento de sessões e execuções indisponível,
  sem análise parcial). Aprovado por Andressa Firmino, 2026-11-04, na análise de consistência das
  tarefas.

## User Scenarios & Testing *(mandatory)*

### Atores

- **SOFIA**: orquestrador que descobre o agente pelo Agent Card e o invoca por A2A em nome de um
  usuário final.
- **Usuário final**: pessoa que, por meio da SOFIA ou do canal direto do agente, envia XMLs e
  perguntas e recebe o parecer.
- **Integrador**: pessoa que usa o Sofia.Simulator para validar a integração A2A antes de conectar a
  SOFIA real.
- **Aprovador**: pessoa com papel de aprovação que, pelo canal direto do agente, aprova ou rejeita a
  liberação de recomendações sensíveis.

### User Story 1 - Analisar um lote de XMLs e receber um parecer (Priority: P1)

O usuário final envia um ou mais arquivos XML de NF-e, com ou sem uma pergunta, e recebe um parecer
estruturado sobre a base enviada. O parecer informa quais arquivos foram analisados, quais erros de
layout e inconsistências de conteúdo foram encontrados, com evidências, e o que não pôde ser
validado.

**Why this priority**: é o valor central do agente. Sem a validação e o parecer, nenhuma outra
funcionalidade tem utilidade.

**Independent Test**: enviar, pelo canal direto do agente, um lote com XMLs válidos, XMLs com erro
de layout e XMLs com divergência de somatório, e conferir se o parecer aponta exatamente os
problemas conhecidos de cada arquivo.

**Acceptance Scenarios**:

1. **Given** um lote de XMLs válidos de NF-e na versão de layout suportada, **When** o usuário
   solicita a análise, **Then** o parecer lista todos os arquivos como analisados, sem achados de
   erro estrutural ou inconsistência de negócio, e o resultado geral é "Sem inconsistências nas
   validações realizadas".
2. **Given** um lote em que um XML viola o schema da sua versão, **When** o usuário solicita a
   análise, **Then** o parecer registra, para esse arquivo, cada violação na categoria "Erro
   estrutural do documento", com a localização do campo, e os demais arquivos são analisados
   normalmente.
3. **Given** um XML em que um totalizador difere do somatório dos itens correspondentes, conforme
   regra do catálogo de regras de conteúdo, **When** o usuário solicita a análise, **Then** o
   parecer registra uma "Inconsistência de negócio" com o arquivo, o campo, o valor informado, o
   valor calculado e a referência da regra oficial.
4. **Given** um lote cujo tamanho total excede o limite configurado, **When** o usuário envia o
   lote, **Then** a solicitação é rejeitada por inteiro, antes de qualquer análise, com mensagem que
   informa o limite e o tamanho recebido.
5. **Given** um XML com versão de layout não suportada, **When** o usuário solicita a análise,
   **Then** o parecer registra a validação desse arquivo como "não realizada", com o motivo, e o
   resultado geral nunca é "Sem inconsistências nas validações realizadas".

---

### User Story 2 - SOFIA descobre o agente e o invoca por A2A (Priority: P1)

A SOFIA obtém o Agent Card do AuditoriaNFe, identifica as habilidades disponíveis, os formatos
aceitos e os requisitos de autenticação, e envia por A2A uma solicitação de análise com XMLs e uma
pergunta, recebendo o mesmo parecer que o canal direto produziria.

**Why this priority**: o agente existe para ser usado pela SOFIA. Sem A2A e Agent Card, o agente não
cumpre seu objetivo de integração.

**Independent Test**: com um cliente A2A (por exemplo, o Sofia.Simulator), obter o Agent Card,
enviar um lote conhecido e comparar o parecer recebido com o parecer do mesmo lote pelo canal
direto.

**Acceptance Scenarios**:

1. **Given** o agente em execução, **When** a SOFIA consulta o endereço de descoberta, **Then**
   recebe um Agent Card válido com nome, descrição, versão, habilidades, formatos de entrada e
   saída, limites e requisitos de autenticação.
2. **Given** um Agent Card obtido, **When** a SOFIA envia por A2A uma pergunta e um lote de XMLs,
   **Then** recebe o parecer estruturado e um resumo textual, com identificadores
   de execução e de sessão.
3. **Given** o mesmo lote enviado pelo canal direto e por A2A, **When** os dois pareceres são
   comparados, **Then** os achados determinísticos (erros estruturais e inconsistências de
   negócio) são idênticos.
4. *(Adiado — desvio D-05)* **Given** uma chamada A2A sem credencial ou com credencial inválida,
   **When** ela é recebida, **Then** é recusada sem executar nenhuma análise.

---

### User Story 3 - Fazer perguntas sobre a base sem receber valores inventados (Priority: P2)

O usuário final faz perguntas em linguagem natural sobre a base analisada, como quantidade de
notas, totais, emitentes ou inconsistências encontradas. O agente responde apenas com informações
presentes nos XMLs ou calculadas a partir deles, e declara quando a informação não existe na base.

**Why this priority**: é o principal diferencial de um agente conversacional sobre os XMLs, e o
maior risco: a criação de valores financeiros inexistentes.

**Independent Test**: com uma base conhecida, fazer um conjunto de perguntas cujas respostas são
conhecidas, incluindo perguntas sobre valores inexistentes, e verificar que todo valor monetário da
resposta é rastreável a um campo de XML ou a um cálculo declarado.

**Acceptance Scenarios**:

1. **Given** uma base analisada, **When** o usuário pergunta o valor total das notas, **Then** a
   resposta apresenta o valor, os arquivos e os campos usados e a forma de cálculo.
2. **Given** uma base sem determinada informação, por exemplo valores de notas não enviadas ou
   projeções futuras, **When** o usuário pergunta por ela, **Then** o agente declara que a
   informação não está disponível na base analisada e não apresenta estimativa.
3. **Given** uma pergunta fora do escopo de NF-e e da base analisada, **When** ela é enviada,
   **Then** o agente informa que a pergunta está fora do seu escopo.

---

### User Story 4 - Continuar a conversa na mesma sessão (Priority: P2)

O usuário final, depois de uma análise, faz novas perguntas ou envia novos arquivos na mesma
sessão, e o agente considera o histórico e os resultados anteriores dessa sessão.

**Why this priority**: a SOFIA conduz conversas em várias etapas; repetir o envio dos arquivos a
cada pergunta é custoso e propenso a erro.

**Independent Test**: analisar um lote, guardar o identificador de sessão, fazer uma pergunta de
continuidade sem reenviar arquivos e verificar que a resposta usa os resultados da análise
anterior.

**Acceptance Scenarios**:

1. **Given** uma primeira solicitação sem identificador de sessão, **When** ela é processada,
   **Then** a resposta contém um novo identificador de sessão.
2. **Given** uma sessão com uma análise concluída, **When** o usuário pergunta "quais notas tiveram
   inconsistências?" informando o identificador da sessão, **Then** a resposta usa os resultados
   da análise anterior sem exigir reenvio dos arquivos.
3. **Given** uma sessão existente, **When** o usuário envia novos XMLs nela, **Then** os novos
   arquivos são analisados e incorporados à base da sessão, e duplicidades com arquivos anteriores
   são identificadas.
4. **Given** uma sessão criada por um usuário, **When** outro usuário tenta usá-la, **Then** a
   solicitação é recusada como "sessão não encontrada", sem revelar dados da sessão.
5. **Given** uma sessão expirada, **When** o usuário a informa, **Then** recebe erro explícito de
   sessão expirada e pode iniciar uma nova sessão.

---

### User Story 5 - Receber recomendações demonstrativas identificadas (Priority: P3)

Junto ao parecer, o agente PODE sugerir ações relacionadas aos achados. Cada sugestão é
apresentada como "Recomendação demonstrativa", separada dos achados, com aviso de que não
constitui orientação fiscal, contábil ou jurídica.

**Why this priority**: agrega valor ao parecer, mas o agente é útil sem essa funcionalidade.

**Independent Test**: analisar um lote com inconsistências conhecidas e verificar que todas as
recomendações estão rotuladas, vinculadas a achados, sem valores financeiros não rastreáveis e sem
alterar o resultado do parecer.

**Acceptance Scenarios**:

1. **Given** um parecer com achados, **When** o agente gera recomendações, **Then** cada uma é
   rotulada "Recomendação demonstrativa", referencia o(s) achado(s) relacionado(s) e acompanha o
   aviso de caráter ilustrativo.
2. **Given** um parecer sem achados, **When** o agente gera a resposta, **Then** ele NÃO DEVE
   gerar recomendações que sugiram problemas inexistentes.
3. **Given** uma recomendação vinculada a achado de severidade Alta ou que menciona valores
   financeiros ou tributos, **When** o parecer é entregue, **Then** essa recomendação aparece apenas
   como "pendente de aprovação", sem o seu conteúdo, e as recomendações não sensíveis são entregues
   normalmente.

---

### User Story 7 - Aprovar a liberação de recomendações sensíveis (Priority: P3)

O aprovador consulta, pelo canal direto, as recomendações sensíveis pendentes, lê o conteúdo e os
achados relacionados e aprova ou rejeita cada uma. Depois disso, a SOFIA consulta a execução ou a
sessão e recebe as recomendações aprovadas; as rejeitadas nunca são entregues.

**Why this priority**: protege o usuário de recomendações com impacto fiscal sem revisão, mas só tem
efeito quando houver recomendações (User Story 5).

**Independent Test**: gerar um parecer com uma recomendação sensível, aprovar e rejeitar
recomendações pelo canal direto, e verificar o que a consulta posterior da SOFIA recebe e o que fica
registrado no checkpoint de aprovação.

**Acceptance Scenarios**:

1. **Given** recomendações sensíveis pendentes, **When** o aprovador lista as pendências, **Then**
   vê cada recomendação com seu conteúdo, os achados vinculados, a execução e a sessão de origem.
2. **Given** uma recomendação pendente, **When** o aprovador a aprova, **Then** a decisão é
   registrada com aprovador, data e hora, e a próxima consulta da SOFIA a recebe rotulada como
   "Recomendação demonstrativa".
3. **Given** uma recomendação pendente, **When** o aprovador a rejeita informando justificativa,
   **Then** a recomendação nunca é entregue à SOFIA, que recebe apenas a indicação de que foi
   rejeitada.
4. *(Adiado — desvio D-05)* **Given** um usuário sem papel de aprovador, **When** ele tenta aprovar
   ou rejeitar, **Then** a operação é recusada.
5. **Given** uma recomendação já decidida, **When** alguém tenta decidir novamente, **Then** a
   operação é recusada, e a decisão original é mantida.

---

### User Story 6 - Validar a integração A2A com o Sofia.Simulator (Priority: P2)

O integrador executa o Sofia.Simulator, que se comporta como a SOFIA: descobre o agente pelo Agent
Card e envia uma solicitação A2A real com XMLs de exemplo e uma pergunta, exibindo o parecer
recebido. Opcionalmente, o simulador continua a sessão e reproduz cenários de erro.

**Why this priority**: permite validar o contrato A2A sem depender da SOFIA real, reduzindo o risco
da integração.

**Independent Test**: executar o Sofia.Simulator contra uma instância do agente e verificar que ele
obtém o Agent Card, envia a solicitação e exibe o parecer com identificadores de execução e sessão.

**Acceptance Scenarios**:

1. **Given** o endereço do agente, **When** o integrador executa o simulador,
   **Then** o simulador obtém e exibe o Agent Card e as habilidades disponíveis.
2. **Given** o Agent Card obtido e os XMLs de exemplo, **When** o simulador envia a solicitação A2A
   com uma pergunta, **Then** exibe o estado da tarefa, o parecer estruturado, o resumo textual e
   os identificadores de execução e de sessão.
3. **Given** o agente indisponível ou o Agent Card inacessível, **When** o simulador é executado,
   **Then** ele informa a falha de descoberta de forma clara e termina sem enviar a solicitação.

---

### Edge Cases

- Lote com um único arquivo exatamente no limite de tamanho total: DEVE ser aceito.
- Lote com tamanho total um byte acima do limite: DEVE ser rejeitado por inteiro.
- Arquivo com extensão `.xml` cujo conteúdo não é XML, ou arquivo XML sem extensão: a decisão DEVE
  considerar o conteúdo, e não a extensão.
- XML bem formado que não é NF-e (por exemplo, outro documento fiscal ou um evento): registrado
  como "não suportado", com validação "não realizada".
- NF-e com e sem protocolo de autorização anexado: ambos são aceitos, e o parecer informa a
  presença ou ausência do protocolo.
- Mesmo documento (mesma chave de acesso) enviado duas vezes no lote ou na sessão: analisado uma
  vez e registrado como duplicidade.
- Dois arquivos diferentes com a mesma chave de acesso e conteúdo divergente: registrado como
  achado, sem escolher silenciosamente um dos dois.
- XML que contém texto com instruções ao agente, por exemplo no campo de informações
  complementares: tratado como dado, sem alterar o comportamento.
- Todos os arquivos do lote inválidos: o parecer é gerado com os erros de cada arquivo, e o
  resultado geral é "Com inconsistências" ou "Inconclusiva", nunca "Sem inconsistências".
- Base de conhecimento (com integração habilitada) ou provedor de IA indisponível: os resultados
  determinísticos são entregues, e o resultado geral é "Inconclusiva".
- Integração MCP desabilitada por configuração: a consulta aos manuais é "não aplicável", o parecer
  declara a limitação e indica revisão humana, e o resultado geral segue as validações
  determinísticas.
- Pergunta de continuidade numa sessão sem nenhuma análise: o agente informa que não há base
  analisada na sessão.
- Pergunta de continuidade sobre um campo do XML que não está entre os dados extraídos guardados na
  sessão: o agente declara a limitação e orienta o reenvio do arquivo.
- Base de conhecimento sem trechos de manual para o tipo e a versão do documento: os pontos
  afetados são registrados como "Ausência de evidências", e a revisão humana é indicada.
- Pergunta que exige somar valores de notas com erro estrutural: o agente informa quais notas foram
  excluídas do cálculo ou incluídas com ressalva.

## Requirements *(mandatory)*

### Functional Requirements

**Recepção e limites**

- **FR-001**: O agente DEVE aceitar, em uma mesma solicitação, uma pergunta em linguagem natural e
  zero ou mais arquivos XML; ao menos um dos dois DEVE estar presente.
- **FR-002**: Quando houver arquivos sem pergunta, o agente DEVE executar a análise completa do lote
  e gerar o parecer.
- **FR-003**: O agente DEVE aceitar múltiplos arquivos XML por solicitação, até a quantidade máxima
  configurada.
- **FR-004**: O agente DEVE validar o tamanho total dos arquivos de cada solicitação antes de
  qualquer análise e rejeitar a solicitação inteira, sem processamento parcial, quando o total
  exceder o limite configurado, informando o limite e o total recebido. O valor inicial do limite
  é de 5 MB de tamanho total e 20 arquivos por solicitação.
- **FR-005**: Os limites de tamanho total e de quantidade de arquivos DEVEM ser configuráveis sem
  alteração do agente e DEVEM ser publicados no Agent Card.
- **FR-006**: O agente DEVE identificar se cada arquivo é XML bem formado pelo conteúdo, e não pela
  extensão. Um arquivo malformado DEVE gerar "Erro estrutural do documento" para esse arquivo, sem
  impedir a análise dos demais.

**Identificação do documento e da versão**

- **FR-007**: Para cada XML, o agente DEVE identificar o tipo de documento, o modelo, a versão do
  layout, a chave de acesso, a presença de protocolo de autorização, a data de emissão (`dhEmi`) e,
  quando houver protocolo, a data de recebimento pela SEFAZ (`dhRecbto`).
- **FR-007a**: Um documento SOMENTE PODE ser identificado como NF-e suportada quando o elemento raiz
  for `NFe` ou `nfeProc`, no namespace da NF-e, com versão 4.00 e modelo 55 tanto em `ide/mod`
  quanto na chave de acesso. Qualquer divergência entre esses sinais DEVE tornar o documento não
  suportado.
- **FR-008**: Arquivos de tipo, modelo ou versão não suportados, incluindo a NFC-e (modelo 65), que
  usa o mesmo schema da NF-e, DEVEM ser registrados com validação "não realizada" e o motivo, sem
  serem considerados válidos e sem passar pela validação de schema.
- **FR-009**: O agente DEVE detectar documentos com a mesma chave de acesso no lote e na sessão,
  analisar cada documento uma única vez e registrar a duplicidade. Arquivos com a mesma chave e
  conteúdo diferente DEVEM gerar achado específico.

**Validação de layout**

- **FR-010**: Cada XML suportado DEVE ser validado contra o pacote de schema oficial vigente na sua
  data de referência, usando o schema de `NFe` ou de `nfeProc` conforme o elemento raiz.
- **FR-010a**: A data de referência DEVE ser a data de recebimento pela SEFAZ (`dhRecbto`) quando o
  documento tiver protocolo de autorização, e a data de emissão (`dhEmi`) quando não tiver.
- **FR-010b**: O pacote vigente DEVE ser obtido da tabela de vigência aprovada, na qual cada pacote
  vale do início de sua produção até o início da produção do pacote seguinte, e o último vale até
  a data atual. As datas DEVEM vir das notas técnicas oficiais, com documento e página registrados.
- **FR-010c**: Quando nenhum pacote disponível cobrir a data de referência, a validação de layout
  DEVE ser registrada como "não realizada — schema não disponível para a data" (ERR-023).
- **FR-010d**: O parecer DEVE informar, para cada documento, a data de referência usada, sua origem
  (`dhRecbto` ou `dhEmi`) e o pacote de schema aplicado.
- **FR-010e**: A tabela de vigência DEVE registrar, quando a nota técnica oficial previr, a data de
  fim da convivência em que o pacote anterior continuou aceito após o início do pacote seguinte.
- **FR-010f**: Quando a validação contra o pacote vigente na data apresentar erro estrutural e a data
  de referência estiver na convivência do pacote anterior, o documento DEVE ser validado também
  contra o pacote anterior. Se o pacote anterior não apresentar erros, o documento DEVE ser
  considerado válido no layout, com o pacote anterior como pacote aplicado; caso contrário, DEVEM
  ser registrados os erros do pacote vigente na data. O parecer DEVE informar que a data estava em
  convivência e quais pacotes foram usados.
- **FR-011**: Cada violação de layout DEVE ser registrada na categoria "Erro estrutural do
  documento", com o arquivo, a localização do campo e a descrição da violação.

**Validação de conteúdo**

- **FR-012**: O agente DEVE validar o conteúdo de cada XML aprovado na validação de layout com as
  regras do catálogo de regras de conteúdo, que inclui no mínimo a conferência entre totalizadores
  e somatórios dos itens definida nos documentos oficiais.
- **FR-013**: Cada regra do catálogo DEVE ter identificador, descrição, documento oficial de origem,
  versão, localização na fonte, período de vigência e severidade. Regras sem fonte oficial NÃO DEVEM
  ser aplicadas (constitution P02).
- **FR-013a**: Cada documento DEVE ser validado com as regras vigentes na mesma data de referência
  usada na escolha do schema (FR-010a).
- **FR-014**: Cada violação de regra de conteúdo DEVE ser registrada na categoria "Inconsistência de
  negócio", com o arquivo, o item (quando aplicável), os campos envolvidos, o valor informado, o
  valor calculado ou esperado e a referência da regra.
- **FR-015**: XMLs reprovados na validação de layout NÃO DEVEM passar pela validação de conteúdo; o
  parecer DEVE registrar para eles a validação de conteúdo como "não realizada — layout inválido".
- **FR-016**: As validações de layout e de conteúdo DEVEM ser determinísticas: o mesmo arquivo DEVE
  produzir sempre os mesmos achados, independentemente da análise por IA.

**Parecer**

- **FR-017**: O agente DEVE gerar um parecer sobre a base analisada contendo: resumo quantitativo
  (arquivos recebidos, analisados, com achados e não analisados), resultado geral, achados por
  arquivo, categoria e severidade, evidências e referências, limitações, validações não realizadas
  e indicação de necessidade de revisão humana.
- **FR-018**: O resultado geral DEVE ser um destes: "Sem inconsistências nas validações
  realizadas", "Com inconsistências" ou "Inconclusiva".
- **FR-019**: O resultado "Sem inconsistências nas validações realizadas" SOMENTE PODE ser atribuído
  quando todas as validações obrigatórias de todos os arquivos tiverem sido concluídas sem achados
  de erro estrutural ou inconsistência de negócio. Qualquer validação "não realizada", falha técnica
  ou dependência indisponível DEVE resultar em "Com inconsistências" (se houver achados
  determinísticos) ou "Inconclusiva" (constitution P05).
- **FR-020**: Cada achado DEVE pertencer a exatamente uma das categorias da constitution (P04): erro
  estrutural do documento, inconsistência de negócio, indício identificado pela IA, ausência de
  evidências, ou falha técnica ou dependência indisponível.
- **FR-021**: Achados gerados pela IA DEVEM ser classificados como "Indício identificado pela IA", com
  evidência associada, e NÃO DEVEM alterar achados determinísticos.
- **FR-022**: Afirmações fundamentadas em manuais oficiais DEVEM citar o documento, a versão e o
  trecho consultado; na falta de fundamentação, o parecer DEVE registrar "Ausência de evidências".
- **FR-023**: A severidade DEVE usar a escala Alta, Média, Baixa e Informativa. A severidade de
  achados de conteúdo DEVE vir do catálogo de regras; erros estruturais DEVEM ter severidade Alta.
- **FR-024**: O parecer DEVE indicar necessidade de revisão humana quando o resultado for
  "Inconclusiva", quando houver indícios da IA de severidade Alta ou Média ou quando houver
  ausência de evidências para um ponto avaliado.
- **FR-025**: O parecer DEVE ser entregue em formato estruturado legível por máquina e com um resumo
  textual em português do Brasil.

**Perguntas e proteção contra informações financeiras inexistentes**

- **FR-026**: O agente DEVE responder perguntas em linguagem natural sobre a base analisada usando
  somente dados extraídos dos XMLs da sessão e os resultados das validações.
- **FR-027**: Todo valor monetário ou quantitativo financeiro apresentado DEVE ser rastreável a um
  campo de um XML identificado (arquivo e campo) ou a um cálculo declarado sobre esses campos. Valor
  não rastreável NÃO DEVE ser apresentado.
- **FR-028**: Antes de entregar uma resposta, o agente DEVE verificar que todo valor financeiro
  citado corresponde à base analisada. Se a verificação falhar, o valor DEVE ser removido e a
  limitação declarada, ou a resposta DEVE ser rejeitada e regenerada dentro do limite de tentativas.
- **FR-029**: Quando a informação pedida não existir na base, por exemplo notas não enviadas,
  projeções, valores estimados ou tributos não informados, o agente DEVE declarar que a informação
  não está disponível e NÃO DEVE estimá-la.
- **FR-030**: O agente NÃO DEVE inventar notas, chaves de acesso, participantes, itens, valores ou
  referências.
- **FR-031**: Em cálculos sobre a base, o agente DEVE informar quais documentos foram incluídos e
  quais foram excluídos ou incluídos com ressalva, como notas com erro estrutural ou duplicadas.
- **FR-032**: Perguntas fora do escopo de NF-e e da base analisada DEVEM ser respondidas com a
  indicação de que estão fora do escopo do agente.

**Recomendações demonstrativas**

- **FR-033**: O agente PODE gerar recomendações; cada uma DEVE ser rotulada "Recomendação
  demonstrativa", apresentada separadamente dos achados e acompanhada de aviso de que é ilustrativa
  e não constitui orientação fiscal, contábil ou jurídica.
- **FR-034**: Cada recomendação DEVE referenciar ao menos um achado do parecer e NÃO DEVE conter
  valores financeiros não rastreáveis (FR-027).
- **FR-035**: Recomendações NÃO DEVEM alterar o resultado geral, os achados ou a indicação de
  revisão humana.
- **FR-035a**: Uma recomendação DEVE ser classificada como sensível quando estiver vinculada a achado
  de severidade Alta ou quando mencionar valores financeiros ou tributos. A classificação DEVE ser
  feita pelo agente com base nesses critérios, e não pela IA.
- **FR-035b**: Recomendações sensíveis NÃO DEVEM ser liberadas ao solicitante sem aprovação humana.

**Aprovação humana de recomendações sensíveis**

- **FR-035c**: O parecer DEVE ser entregue sem esperar a aprovação. Cada recomendação sensível DEVE
  aparecer como "pendente de aprovação", com identificador, mas sem o seu conteúdo.
- **FR-035d**: O canal direto DEVE permitir listar as recomendações pendentes, com conteúdo, achados
  vinculados, execução e sessão de origem. Na versão 1, o aprovador é o usuário informado na
  solicitação, registrado sem verificação (desvio D-05).
- **FR-035e**: O aprovador DEVE poder aprovar ou rejeitar cada recomendação; a rejeição DEVE exigir
  justificativa. Cada recomendação PODE ser decidida uma única vez.
- **FR-035f**: Cada decisão DEVE ser registrada em um checkpoint de aprovação com: identificador da
  recomendação, execução, sessão, estado (pendente, aprovada ou rejeitada), aprovador, data e hora
  e justificativa, quando houver.
- **FR-035g**: A SOFIA DEVE poder consultar, por A2A, a execução ou a sessão e receber as
  recomendações aprovadas e o estado das demais. Recomendações rejeitadas NÃO DEVEM ter o conteúdo
  entregue.
- **FR-035h** *(adiado — desvio D-05)*: Usuários sem papel de aprovador NÃO DEVEM aprovar nem
  rejeitar recomendações.
- **FR-035i**: Recomendações pendentes DEVEM ficar indisponíveis quando a sessão de origem expirar ou
  for encerrada, e o checkpoint DEVE registrar essa situação.
- **FR-035j**: Os checkpoints de aprovação DEVEM ser persistidos em arquivos JSON locais, apenas para
  demonstração (ver Limitações técnicas).

**Sessão e continuidade**

- **FR-036**: A primeira solicitação sem identificador de sessão DEVE criar uma sessão e retornar
  seu identificador; solicitações seguintes com esse identificador DEVEM continuar a mesma sessão.
- **FR-037**: A sessão DEVE manter o histórico de perguntas e respostas e os resultados das análises
  realizadas nela, permitindo perguntas de continuidade sem reenvio dos arquivos.
- **FR-037a**: A sessão DEVE guardar somente os dados extraídos dos XMLs e os resultados das
  análises; o conteúdo original dos XMLs NÃO DEVE ser mantido após o término da análise. A sessão
  DEVE guardar, de cada documento, apenas a identificação do conteúdo necessária para detectar
  duplicidade e divergência (FR-009).
- **FR-037b**: Quando uma pergunta de continuidade exigir informação que não faz parte dos dados
  extraídos, o agente DEVE informar a limitação e orientar o reenvio do arquivo, sem estimar a
  informação.
- **FR-038**: Novos arquivos enviados em uma sessão existente DEVEM ser analisados e incorporados à
  base da sessão, aplicando a detecção de duplicidade (FR-009).
- **FR-039**: A sessão DEVE ser acessível somente pelo mesmo usuário, informado pelo mesmo chamador,
  que a criou; qualquer outro acesso DEVE receber "sessão não encontrada", sem revelar a existência
  da sessão.
- **FR-040**: A sessão DEVE expirar após período de inatividade configurável; o uso de sessão
  expirada DEVE retornar erro explícito.
- **FR-041**: O usuário DEVE poder encerrar a sessão, o que torna seus dados indisponíveis e os
  remove conforme a política de retenção.
- **FR-042**: Quando o histórico textual da sessão for reduzido por limite, os resultados das
  análises da sessão DEVEM continuar disponíveis.

**Contexto do usuário**

- **FR-055**: O agente DEVE manter, por usuário, um contexto próprio da auditoria, que persiste entre
  sessões, armazenado separadamente do histórico das sessões e do conhecimento oficial. O contexto
  geral do usuário pertence ao orquestrador chamador e NÃO DEVE ser replicado no agente.
- **FR-055a**: O contexto do usuário DEVE conter somente:
  1. os CNPJs emitentes e destinatários dos documentos já auditados pelo usuário, com a quantidade
     de documentos de cada um e a data da última auditoria;
  2. o resumo dos achados recorrentes, com regra ou tipo de erro, frequência e data da última
     ocorrência, derivado exclusivamente dos resultados dos validadores determinísticos;
  3. as preferências declaradas explicitamente pelo usuário: nível de detalhe do parecer (resumido
     ou detalhado) e tributos de interesse.
  Conteúdo dos XMLs, valores de documentos, nomes, endereços e textos livres da conversa NÃO DEVEM
  ser guardados no contexto.
- **FR-055b**: Cada informação do contexto DEVE registrar sua origem (execução que a gerou ou
  declaração do usuário, com sessão e data).
- **FR-056**: O contexto do usuário DEVE ser isolado pelo identificador do usuário combinado com a
  identificação do chamador que o informou; um usuário NÃO DEVE acessar o contexto de outro.
- **FR-057**: O contexto do usuário DEVE ser gravado somente pelo agente, a partir dos resultados das
  validações determinísticas ou de informação fornecida explicitamente pelo usuário. A IA NÃO DEVE
  gravar, alterar ou excluir o contexto.
- **FR-058**: O contexto do usuário PODE ser usado para direcionar respostas e o parecer, mas NÃO
  DEVE ser usado como evidência, NÃO DEVE alterar achados determinísticos e DEVE ser tratado como
  dado não confiável quando enviado à IA (constitution P07).
- **FR-059**: O canal direto DEVE permitir consultar, corrigir e excluir o contexto de um usuário.
- **FR-060**: O contexto do usuário DEVE ser excluído automaticamente após 90 dias sem atividade do
  usuário (valor configurável). Cada execução ou declaração do usuário DEVE renovar o prazo.

**Exposição por A2A e Agent Card**

- **FR-043**: Todas as funcionalidades do agente (análise de lote, perguntas sobre a base e
  continuidade de sessão) DEVEM estar disponíveis por A2A, com as mesmas validações, autenticação e
  resultados do canal direto (constitution P18). A comunicação A2A DEVE ser real, implementada com o
  SDK oficial do protocolo, e não simulada.
- **FR-044**: O agente DEVE publicar um Agent Card no endereço de descoberta definido pelo protocolo
  A2A, contendo: nome, descrição, versão do agente, versão do contrato, habilidades, formatos de
  entrada e saída, limites de arquivos e requisitos de autenticação.
- **FR-045**: O Agent Card NÃO DEVE conter segredos nem dados de usuários.
- **FR-046**: Para análises que não terminam imediatamente, o agente DEVE informar o estado da
  tarefa (recebida, em processamento, concluída, falhou, rejeitada) e permitir que a SOFIA obtenha o
  resultado final.
- **FR-047**: O agente DEVE aceitar um identificador de correlação enviado pela SOFIA e devolvê-lo
  na resposta, junto com os identificadores de execução e de sessão gerados pelo agente.

**Preparação para integrações MCP**

- **FR-047a**: A consulta à base de conhecimento e futuras ferramentas externas DEVEM ser acessadas
  por um ponto de integração MCP, habilitado ou desabilitado por configuração, sem alteração do
  restante do fluxo de análise.
- **FR-047b**: Com a integração desabilitada, a etapa de consulta aos manuais DEVE ser registrada
  como "não aplicável — integração não habilitada"; o parecer DEVE declarar a limitação "consulta a
  manuais oficiais não habilitada" e indicar revisão humana, e o resultado geral DEVE seguir apenas
  as validações determinísticas (FR-019).
- **FR-047c**: Com a integração habilitada, a indisponibilidade ou falha da base de conhecimento DEVE
  seguir o ERR-013, com resultado geral "Inconclusiva".
- **FR-047d**: O estado da integração (habilitada ou não) DEVE constar do parecer de cada execução.

**Sofia.Simulator**

- **FR-048**: DEVE ser disponibilizado o projeto cliente Sofia.Simulator, que se comunica com o
  agente exclusivamente pelo contrato público A2A, usando o SDK oficial do protocolo A2A, sem
  acesso interno ao agente.
- **FR-049**: O Sofia.Simulator DEVE descobrir o agente obtendo o Agent Card no endereço de
  descoberta, exibir as habilidades e enviar uma solicitação A2A real com uma pergunta e um ou mais
  XMLs, exibindo o estado da tarefa, o parecer, o resumo textual e os identificadores de execução e
  sessão.
- **FR-050**: O Sofia.Simulator DEVE incluir XMLs de exemplo sintéticos ou anonimizados suficientes
  para o fluxo básico.
- **FR-051**: O Sofia.Simulator PODE continuar uma sessão, consultar recomendações liberadas e
  reproduzir cenários de erro; a verificação completa dos cenários de teste fica nos testes
  automatizados do agente.
- **FR-051a**: Se o Agent Card não puder ser obtido, o Sofia.Simulator DEVE informar a falha de
  descoberta e NÃO DEVE enviar a solicitação.

**Segurança e rastreabilidade**

- **FR-052** *(adiado — desvio D-05)*: Toda solicitação, pelo canal direto ou por A2A, DEVE exigir
  a credencial de serviço do sistema chamador; solicitações sem credencial válida ou não autorizadas
  DEVEM ser recusadas antes de qualquer análise (constitution 5.2).
- **FR-052a**: Na versão 1, a solicitação PODE informar nos metadados o identificador do usuário e a
  identificação do chamador. Quando informados, os dois DEVEM ser registrados e usados no isolamento
  de sessões e do contexto do usuário, sem verificação de identidade.
- **FR-052c**: Quando o identificador do usuário não for informado, a solicitação DEVE ser aceita com
  o usuário padrão "anonimo"; quando o chamador não for informado, com o chamador padrão
  "nao-informado". Solicitações com o usuário "anonimo" NÃO DEVEM ler nem gravar contexto do
  usuário, e o parecer DEVE indicar que o usuário não foi identificado.
- **FR-052b**: Enquanto o desvio D-05 vigorar, o agente SOMENTE PODE ser executado em ambiente local
  de testes, com as portas publicadas apenas na interface local da máquina, e o Agent Card DEVE
  declarar que não há autenticação. Os dados de teste DEVEM ser sintéticos ou anonimizados.
- **FR-053**: O conteúdo dos XMLs e das perguntas DEVE ser tratado como dado; instruções contidas
  nesse conteúdo NÃO DEVEM alterar regras, contrato de saída ou comportamento do agente
  (constitution P07).
- **FR-054**: Toda resposta DEVE conter identificador de execução, identificador de sessão, estado
  do processamento, versão do contrato e data e hora, preenchidos pelo agente, e não pela IA
  (constitution P09).

### Non-Functional Requirements

- **NFR-001 (Desempenho)**: Para um lote no limite máximo configurado, o parecer completo DEVE ser
  entregue em até 2 minutos em 95% das execuções, desconsiderando indisponibilidade de
  dependências externas.
- **NFR-002 (Rejeição rápida)**: Solicitações que excedem limites de tamanho ou de quantidade DEVEM
  ser rejeitadas em até 2 segundos após o recebimento completo.
- **NFR-003 (Perguntas de continuidade)**: Perguntas de continuidade sem novos arquivos DEVEM ser
  respondidas em até 30 segundos em 95% das execuções.
- **NFR-004 (Concorrência)**: O agente DEVE suportar ao menos 10 análises simultâneas mantendo as
  metas de NFR-001 a NFR-003.
- **NFR-005 (Determinismo)**: As validações de layout e de conteúdo DEVEM ser 100% reprodutíveis
  para os mesmos arquivos e a mesma versão do catálogo de regras.
- **NFR-006 (Resiliência)**: A indisponibilidade da base de conhecimento (com a integração MCP
  habilitada) ou do provedor de IA NÃO DEVE impedir a entrega dos resultados determinísticos; o
  resultado geral DEVE ser "Inconclusiva".
- **NFR-007 (Privacidade)**: O conteúdo integral dos XMLs e dados pessoais NÃO DEVEM aparecer em
  registros operacionais; o envio de dados ao provedor de IA DEVE seguir a política de minimização
  definida para o projeto (constitution DP-14).
- **NFR-008 (Rastreabilidade)**: A partir do identificador de execução, DEVE ser possível
  reconstruir as etapas executadas, as regras aplicadas, os trechos de manual consultados e o
  consumo de IA da execução.
- **NFR-009 (Custos)**: O consumo e o custo estimado de IA DEVEM ser registrados por execução,
  incluindo retentativas (constitution P14).
- **NFR-010 (Idioma)**: Pareceres, respostas e mensagens de erro DEVEM estar em português do Brasil.
- **NFR-011 (Configuração)**: Limites, tempo de expiração de sessão e quantidade de tentativas DEVEM
  ser configuráveis sem alteração do agente.
- **NFR-012 (Contratos)**: O contrato do parecer, o contrato A2A e o Agent Card DEVEM ser
  versionados; alterações incompatíveis DEVEM gerar nova versão.
- **NFR-013 (Interoperabilidade)**: O Agent Card e as mensagens DEVEM ser válidos segundo a versão
  do protocolo A2A adotada no projeto (constitution DP-17).

### Error Handling

Toda resposta de erro DEVE conter código estável, mensagem em português do Brasil, identificador de
execução e, quando aplicável, o arquivo afetado. Erros que interrompem a solicitação não produzem
parecer; erros por arquivo são registrados no parecer, e os demais arquivos seguem.

| Código | Situação | Comportamento esperado | Abrangência |
| ------ | -------- | ---------------------- | ----------- |
| ERR-001 | Solicitação sem pergunta e sem arquivos | Rejeitar com orientação sobre o conteúdo mínimo | Solicitação |
| ERR-002 | Tamanho total acima do limite | Rejeitar o lote inteiro, informando limite e total recebido | Solicitação |
| ERR-003 | Quantidade de arquivos acima do limite | Rejeitar o lote inteiro, informando o limite | Solicitação |
| ERR-004 | *(Adiado — desvio D-05)* Não autenticado ou não autorizado | Recusar sem executar análise e sem detalhar o motivo de segurança | Solicitação |
| ERR-005 | Sessão inexistente ou de outro usuário | Responder "sessão não encontrada" | Solicitação |
| ERR-006 | Sessão expirada | Responder "sessão expirada" e orientar o início de nova sessão | Solicitação |
| ERR-007 | Mensagem A2A malformada ou fora do contrato | Rejeitar com erro de protocolo, sem executar análise | Solicitação |
| ERR-008 | Arquivo não é XML bem formado | Erro estrutural do documento; demais arquivos seguem | Arquivo |
| ERR-009 | XML não é NF-e, ou modelo não suportado | Validação "não realizada — documento não suportado" | Arquivo |
| ERR-010 | Versão de layout não suportada | Validação "não realizada — versão não suportada" | Arquivo |
| ERR-011 | Documento duplicado no lote ou na sessão | Achado Informativo; documento analisado uma vez | Arquivo |
| ERR-012 | Mesma chave com conteúdo divergente | Achado de severidade Alta; nenhum dos arquivos é descartado | Arquivo |
| ERR-013 | Base de conhecimento indisponível | Falha técnica ou dependência indisponível; resultados determinísticos entregues; resultado "Inconclusiva"; revisão humana | Execução |
| ERR-014 | Provedor de IA indisponível ou tempo esgotado | Igual ao ERR-013; sem análise por IA e sem recomendações | Execução |
| ERR-015 | Resposta da IA inválida após o limite de tentativas | Igual ao ERR-014; conteúdo inválido descartado | Execução |
| ERR-016 | Tempo máximo da execução esgotado | Estado "falhou", com as etapas concluídas e as não executadas registradas | Execução |
| ERR-017 | Informação financeira pedida não existe na base | Declarar indisponibilidade, sem estimativa (não é erro técnico) | Resposta |
| ERR-018 | Pergunta fora do escopo | Informar o escopo do agente (não é erro técnico) | Resposta |
| ERR-019 | *(Adiado — desvio D-05)* Aprovação ou rejeição por usuário sem papel de aprovador | Recusar a operação, sem alterar o checkpoint | Solicitação |
| ERR-020 | Recomendação já decidida ou inexistente | Recusar a operação, mantendo a decisão original | Solicitação |
| ERR-021 | Rejeição sem justificativa | Recusar a operação, informando que a justificativa é obrigatória | Solicitação |
| ERR-022 | Falha ao gravar ou ler o checkpoint de aprovação | Não liberar a recomendação; manter "pendente de aprovação" e registrar falha técnica | Solicitação |
| ERR-023 | Nenhum pacote de schema cobre a data de referência do documento | Validação de layout "não realizada — schema não disponível para a data"; conteúdo "não realizada"; resultado nunca "Sem inconsistências" | Arquivo |
| ERR-024 | Limite de taxa ou de análises simultâneas atingido | Rejeitar sem executar análise, informando em quanto tempo tentar de novo | Solicitação |
| ERR-025 | Armazenamento de sessões e execuções indisponível | Rejeitar sem análise parcial, como falha técnica ou dependência indisponível; nenhum resultado é apresentado como completo | Solicitação |

### Key Entities

- **Sessão**: contexto conversacional de um usuário; contém identificador, usuário e contexto de
  origem, histórico de perguntas e respostas, base de documentos analisados, data de criação e de
  última atividade e estado (ativa, expirada ou encerrada).
- **Execução**: processamento de uma solicitação; contém identificador, correlação recebida,
  sessão, canal (direto ou A2A), estado de cada etapa, estado final, tempos e consumo de IA.
- **Arquivo recebido**: arquivo enviado em uma execução; contém nome, tamanho, resultado da
  identificação e estado de cada validação.
- **Documento NF-e**: documento identificado a partir de um arquivo; contém chave de acesso,
  modelo, versão de layout, presença de protocolo e dados extraídos usados nas validações e nas
  respostas.
- **Regra de conteúdo**: item do catálogo de regras; contém identificador, descrição, fonte oficial,
  versão, localização na fonte, período de vigência e severidade.
- **Pacote de schema**: conjunto oficial de XSDs do leiaute 4.00; contém identificador do pacote,
  revisão, notas técnicas que implementa, origem, data de obtenção e hash de cada arquivo.
- **Vigência de pacote**: período em que um pacote de schema esteve em produção; contém pacote,
  data de início, data de fim (ou aberta), data de fim da convivência, quando houver, e documento e
  página oficiais de onde as datas vieram.
- **Contexto do usuário**: informações que o agente mantém sobre um usuário entre sessões; contém
  identificador do usuário, chamador que o informou, conteúdo permitido pela especificação, origem
  de cada informação, datas de criação e de atualização.
- **Achado**: resultado de uma validação ou análise; contém categoria, severidade, documento, campo
  ou item, valores informado e esperado, descrição, evidências e referências.
- **Evidência/Referência**: vínculo verificável de um achado ou de uma afirmação a um campo de XML,
  a uma regra do catálogo ou a um trecho de manual oficial com versão.
- **Parecer**: resultado consolidado sobre a base; contém resumo quantitativo, resultado geral,
  achados, limitações, validações não realizadas, revisão humana e recomendações demonstrativas.
- **Recomendação demonstrativa**: sugestão ilustrativa vinculada a achados, com rótulo e aviso
  obrigatórios; indica se é sensível e seu estado de liberação (liberada, pendente de aprovação,
  aprovada, rejeitada ou indisponível).
- **Checkpoint de aprovação**: registro da decisão humana sobre uma recomendação sensível; contém
  identificador da recomendação, execução, sessão, estado, aprovador, data e hora e justificativa.
- **Agent Card**: descrição pública do agente para descoberta, com habilidades, formatos, limites e
  requisitos de autenticação.
- **Solicitação do simulador**: chamada A2A enviada pelo Sofia.Simulator, com pergunta, XMLs de
  exemplo e o parecer recebido em resposta.

### Test Scenarios

Os cenários abaixo DEVEM ser automatizados nos testes do agente. Na coluna Simulador,
"Obrigatório" indica que o Sofia.Simulator também DEVE executar o cenário, e "Opcional" indica que
ele PODE executá-lo. Os XMLs de teste DEVEM ser sintéticos ou anonimizados.

| ID | Cenário | Resultado esperado | Requisitos | Simulador |
| -- | ------- | ------------------ | ---------- | --------- |
| TS-001 | Lote de XMLs válidos na versão suportada | Resultado "Sem inconsistências nas validações realizadas"; nenhum achado de erro ou inconsistência | FR-010, FR-012, FR-018, FR-019 | Opcional |
| TS-002 | XML com violação de schema | Erro estrutural com localização; demais arquivos analisados | FR-010, FR-011 | Opcional |
| TS-003 | XML malformado entre arquivos válidos | ERR-008 apenas para o arquivo malformado | FR-006 | Opcional |
| TS-004 | Totalizador divergente do somatório dos itens | Inconsistência de negócio com valor informado, valor calculado e regra | FR-012, FR-014 | Opcional |
| TS-005 | XML com erro de layout e de somatório | Apenas erro estrutural; conteúdo "não realizada — layout inválido" | FR-015 | Não |
| TS-006 | Lote exatamente no limite de tamanho | Aceito e analisado | FR-004 | Não |
| TS-007 | Lote acima do limite de tamanho | ERR-002, sem processamento parcial | FR-004, NFR-002 | Opcional |
| TS-008 | Quantidade de arquivos acima do limite | ERR-003 | FR-003 | Opcional |
| TS-009 | Versão de layout não suportada | ERR-010; resultado nunca "Sem inconsistências" | FR-008, FR-019 | Opcional |
| TS-010 | XML bem formado que não é NF-e | ERR-009 | FR-008 | Não |
| TS-011 | Mesmo documento enviado duas vezes | ERR-011; documento analisado uma vez | FR-009 | Não |
| TS-012 | Mesma chave com conteúdo divergente | ERR-012 | FR-009 | Não |
| TS-013 | Mesmo lote analisado duas vezes | Achados determinísticos idênticos | FR-016, NFR-005 | Não |
| TS-014 | Pergunta sobre o valor total da base | Valor com arquivos, campos e cálculo declarados | FR-026, FR-027, FR-031 | Opcional |
| TS-015 | Pergunta sobre valor inexistente na base | ERR-017, sem estimativa | FR-029 | Opcional |
| TS-016 | Conjunto adversarial de perguntas que induzem a inventar valores | Nenhum valor financeiro não rastreável nas respostas | FR-027, FR-028, FR-030 | Não |
| TS-017 | Pergunta fora do escopo | ERR-018 | FR-032 | Não |
| TS-018 | XML com instruções ao agente em campo de texto | Comportamento e contrato inalterados | FR-053 | Não |
| TS-019 | Recomendações em parecer com achados | Todas rotuladas, com aviso e vinculadas a achados | FR-033, FR-034 | Não |
| TS-020 | Parecer sem achados | Nenhuma recomendação sugerindo problemas inexistentes | FR-035 | Não |
| TS-021 | Primeira solicitação e pergunta de continuidade | Sessão criada; continuidade sem reenvio de arquivos | FR-036, FR-037 | Opcional |
| TS-022 | Novos arquivos em sessão existente | Base da sessão ampliada, com duplicidades detectadas | FR-038 | Não |
| TS-023 | Sessão usada por outro usuário | ERR-005 | FR-039 | Não |
| TS-024 | Sessão expirada | ERR-006 | FR-040 | Opcional |
| TS-025 | Encerramento de sessão | Sessão indisponível após encerramento | FR-041 | Não |
| TS-026 | Base de conhecimento indisponível | ERR-013; resultado "Inconclusiva" | FR-019, NFR-006 | Não |
| TS-027 | Provedor de IA indisponível | ERR-014; resultados determinísticos entregues | FR-019, NFR-006 | Não |
| TS-028 | Resposta da IA inválida repetidamente | ERR-015 após o limite de tentativas | FR-028 | Não |
| TS-029 | Descoberta pelo Agent Card | Agent Card válido com todos os campos de FR-044 | FR-044, FR-045, NFR-013 | Obrigatório |
| TS-030 | Mesmo lote por canal direto e por A2A | Achados determinísticos idênticos | FR-043 | Opcional |
| TS-031 | *(Adiado — desvio D-05)* Chamada sem credencial ou com credencial inválida | ERR-004 | FR-052 | Opcional |
| TS-032 | Mensagem A2A malformada | ERR-007 | FR-043 | Opcional |
| TS-033 | Análise longa por A2A | Estados da tarefa informados e resultado final obtido | FR-046 | Opcional |
| TS-034 | Correlação enviada pela SOFIA | Correlação devolvida com identificadores de execução e sessão | FR-047, FR-054 | Opcional |
| TS-035 | Fluxo básico do simulador: descoberta e solicitação A2A com XMLs e pergunta | Parecer, estado da tarefa e identificadores exibidos | FR-049, FR-050 | Obrigatório |
| TS-035a | Agent Card inacessível para o simulador | Falha de descoberta informada; nenhuma solicitação enviada | FR-051a | Obrigatório |
| TS-036 | Sessão após análise concluída | XML original não mantido; apenas dados extraídos e resultados disponíveis | FR-037a | Não |
| TS-037 | Pergunta de continuidade sobre campo não extraído | Limitação declarada e reenvio orientado, sem estimativa | FR-037b | Não |
| TS-038 | Base de conhecimento sem trechos para a versão do documento | "Ausência de evidências" e revisão humana; resultado geral segundo o FR-019 | FR-022, FR-024 | Não |
| TS-039 | Parecer com recomendação sensível | Parecer entregue na hora; recomendação sensível "pendente de aprovação", sem conteúdo | FR-035a, FR-035b, FR-035c | Opcional |
| TS-040 | Aprovação de recomendação sensível | Checkpoint gravado; a consulta seguinte por A2A recebe a recomendação | FR-035e, FR-035f, FR-035g | Não |
| TS-041 | Rejeição com justificativa | Checkpoint gravado; o conteúdo nunca é entregue | FR-035e, FR-035g | Não |
| TS-042 | *(Adiado — desvio D-05)* Decisão por usuário sem papel de aprovador | ERR-019 | FR-035h | Não |
| TS-043 | Segunda decisão sobre a mesma recomendação | ERR-020 | FR-035e | Não |
| TS-044 | Falha de gravação do checkpoint | ERR-022; recomendação continua pendente | FR-035f | Não |
| TS-045 | Sessão expirada com recomendação pendente | Recomendação indisponível, registrada no checkpoint | FR-035i | Não |
| TS-046 | Lote válido com integração MCP desabilitada | Etapa "não aplicável"; limitação e revisão humana no parecer; resultado segundo o FR-019 | FR-047a, FR-047b, FR-047d | Não |
| TS-047 | Integração MCP habilitada e base indisponível | ERR-013; resultado "Inconclusiva" | FR-047c | Não |
| TS-048 | NF-e com protocolo, emitida num período e recebida pela SEFAZ no período do pacote seguinte | Validada contra o pacote vigente em `dhRecbto`; parecer informa data, origem e pacote | FR-010, FR-010a, FR-010d | Não |
| TS-049 | NF-e sem protocolo | Validada contra o pacote vigente em `dhEmi` | FR-010a | Não |
| TS-050 | Um XML sintético por período de vigência, válido no seu pacote | Todos sem erro estrutural, cada um com o pacote correto | FR-010, FR-010b | Não |
| TS-051 | Data de referência anterior ao primeiro pacote disponível | ERR-023; resultado nunca "Sem inconsistências" | FR-010c | Não |
| TS-052 | NFC-e (modelo 65) bem formada e válida no schema | ERR-009, sem validação de schema | FR-007a, FR-008 | Não |
| TS-053 | Modelo divergente entre `ide/mod` e a chave de acesso | ERR-009 | FR-007a | Não |
| TS-054 | Contexto do usuário acessado com identificador de outro usuário ou outro chamador | Contexto não revelado | FR-056 | Não |
| TS-055 | Resposta da IA que tenta gravar ou alterar o contexto do usuário | Contexto inalterado | FR-057 | Não |
| TS-056 | Exclusão do contexto de um usuário pelo canal direto | Contexto indisponível nas execuções seguintes | FR-059 | Não |
| TS-057 | Auditoria concluída seguida de preferência declarada pelo usuário | Contexto contém CNPJs, contagens, achados recorrentes e a preferência, cada um com origem; não contém conteúdo do XML, valores nem texto livre | FR-055a, FR-055b | Não |
| TS-058 | Contexto sem atividade além do prazo configurado (prazo reduzido no teste) | Contexto excluído; nova execução cria contexto vazio | FR-060 | Não |
| TS-059 | Solicitação sem identificador do usuário e do chamador | Análise executada com "anonimo" e "nao-informado"; nenhum contexto lido ou gravado; parecer indica usuário não identificado | FR-052c | Não |
| TS-060 | NF-e no layout do pacote anterior, com data de referência dentro da convivência | Válida no layout; pacote aplicado é o anterior; parecer indica convivência | FR-010f | Não |
| TS-061 | NF-e com erro estrutural nos dois pacotes, data dentro da convivência | Erros do pacote vigente na data; parecer indica convivência e os dois pacotes usados | FR-010f | Não |
| TS-062 | NF-e no layout do pacote anterior, com data após o fim da convivência | Erros estruturais do pacote vigente; o pacote anterior não é tentado | FR-010e, FR-010f | Não |
| TS-063 | Solicitações do mesmo usuário acima do limite de taxa configurado | ERR-024, sem análise, com o tempo para nova tentativa | NFR-004 | Não |
| TS-064 | Armazenamento de sessões indisponível durante a solicitação | ERR-025 dentro do tempo limite configurado; nenhum parecer parcial | NFR-006 | Não |

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: No conjunto de teste, 100% dos erros de layout conhecidos são apontados com a
  localização correta, e nenhum arquivo com erro conhecido recebe resultado "Sem inconsistências".
- **SC-002**: No conjunto de teste, 100% das divergências de somatório conhecidas são apontadas com
  valor informado, valor calculado e referência da regra.
- **SC-003**: Em um conjunto de avaliação de no mínimo 50 perguntas, incluindo perguntas
  adversariais, 0 respostas apresentam valor financeiro não rastreável à base analisada.
- **SC-004**: 100% das recomendações geradas no conjunto de avaliação estão rotuladas como
  "Recomendação demonstrativa" e vinculadas a achados.
- **SC-004a**: 0 recomendações sensíveis têm o conteúdo entregue à SOFIA sem aprovação registrada
  em checkpoint.
- **SC-005**: O integrador conecta o Sofia.Simulator ao agente informando apenas o endereço, e 100% dos cenários marcados como obrigatórios para o simulador são aprovados.
- **SC-006**: 95% das análises de lotes no limite máximo entregam o parecer em até 2 minutos.
- **SC-007**: 100% das solicitações acima dos limites são rejeitadas sem processamento parcial.
- **SC-008**: 100% das perguntas de continuidade do conjunto de avaliação são respondidas sem
  reenvio de arquivos e com base nos resultados corretos da sessão.
- **SC-009**: 0 acessos bem-sucedidos a sessões de outro usuário nos testes de isolamento.
- **SC-010**: Em 100% dos testes com dependência indisponível, o resultado geral é "Inconclusiva" e
  os resultados determinísticos são entregues.
- **SC-011**: No conjunto de teste com um XML por período de vigência, 100% dos documentos são
  validados contra o pacote de schema correto para a sua data de referência.
- **SC-012**: 0 acessos bem-sucedidos ao contexto de outro usuário ou de outro chamador nos testes de
  isolamento.

## Assumptions

- **Escopo documental da versão 1**: NF-e modelo 55, na versão de layout 4.00, a única em vigor para
  emissão. São aceitos o XML da NF-e com ou sem o protocolo de autorização anexado. NFC-e
  (modelo 65), XMLs de eventos (cancelamento, carta de correção e outros) e demais documentos
  fiscais ficam fora do escopo e são tratados como "não suportados" (resolve DP-02 para esta
  feature).
- **Fontes oficiais (resolve DP-03 para os schemas desta feature)**: os pacotes de schema oficiais
  do leiaute 4.00 publicados no Portal Nacional da NF-e (séries PL_009 e PL_010, de 2017 ao pacote
  vigente), o Anexo I do MOC 7.0 (versão 7.00, novembro de 2020) e as notas técnicas que alteraram
  o leiaute ou as regras de validação. Cada arquivo é inventariado com origem, versão e SHA-256. Os
  pacotes da série PL_010 não trazem o schema de `nfeProc`; é usado o `procNFe_v4.00.xsd` oficial
  do pacote PL_009q, idêntico em todos os pacotes da série PL_009, com a origem registrada.
- **Tabela de vigência dos pacotes**: anexada em
  [vigencia-pacotes-schema.md](vigencia-pacotes-schema.md), com as datas de produção extraídas das
  notas técnicas (documento, página e citação). DEVE ser aprovada antes das tarefas de validação de
  layout. Pacotes que nunca estiveram sozinhos em produção ficam fora da tabela. Nenhuma NT do
  leiaute 4.00 prevê convivência entre pacotes; a regra do FR-010f só é acionada se uma NT futura
  a previr.
- **Catálogo de regras de conteúdo (resolve DP-04 para esta feature)**: anexado em
  [catalogo-regras-totalizadores.md](catalogo-regras-totalizadores.md) e aprovado pelo solicitante
  em 2026-11-04. Contém as regras de totais iguais à soma dos itens (ICMS, ISSQN, IBS, CBS e IS) e as
  de fechamento de cobrança e pagamento, cada uma com fonte oficial (Anexo I e notas técnicas),
  fórmula, tolerância, período de vigência e severidade. Regras em "implementação futura", limites
  definidos por UF e regras que dependem da base da SEFAZ ficam fora da versão 1. Nenhuma regra é
  presumida por esta especificação.
- **Ferramentas externas**: a versão 1 não consulta serviços externos, como a situação da NF-e na
  SEFAZ. A validação da assinatura digital e da cadeia de certificados também fica fora do escopo da
  versão 1.
- **Autenticação**: fora da versão 1 (desvio D-05). O modelo previsto para a implementação futura é
  o da constitution 5.2: somente o sistema chamador (SOFIA, Sofia.Simulator ou cliente do canal
  direto) se autentica, por credencial de serviço, e informa o identificador do usuário final, sem
  login do usuário. O mecanismo concreto da credencial e o controle de quem pode aprovar
  recomendações sensíveis serão definidos em ADR (DP-10).
- **Contrato com a SOFIA (DP-27)**: ainda não definido. O contrato A2A desta feature é uma proposta
  v1 provisória e versionada; mudanças acordadas com a SOFIA afetam o adaptador A2A e o Agent Card,
  e não o caso de uso de análise, que é o mesmo do canal direto.
- **Expiração de sessão**: valor inicial de 24 horas de inatividade, configurável; a retenção após a
  expiração segue a política a ser definida (DP-13).
- **Canal direto**: além de A2A, o agente oferece um canal direto de requisição com o mesmo
  comportamento, previsto na constitution.
- **SOFIA fora do escopo**: o orquestrador SOFIA é um sistema externo e não é implementado neste
  projeto. Ele apenas acessa o agente pelo contrato público A2A (Agent Card e mensagens). Este
  projeto entrega o agente, o contrato A2A e o Sofia.Simulator, que é um cliente de teste e não
  substitui nem reproduz a lógica de orquestração da SOFIA. Requisitos sobre o comportamento da
  SOFIA (por exemplo, obter o token com a identidade do usuário final ou reenviar o identificador
  da sessão) são obrigações de integração do lado dela, documentadas no contrato.
- **Idioma**: português do Brasil em todas as interações.
- **Base de conhecimento**: a ingestão e a administração dos manuais oficiais pertencem a uma
  feature separada (002). Esta feature prepara o ponto de integração MCP para consultar a base, e
  ele fica desabilitado até a feature 002 existir (FR-047a a FR-047d). Com a integração habilitada,
  se a base não tiver trechos para o tipo e a versão do documento, os pontos afetados são
  registrados como "Ausência de evidências", com revisão humana; a indisponibilidade técnica segue
  o ERR-013.
- **Alinhamento com a constitution**: a etapa 5 do fluxo (seção 3.4) desabilitada por configuração
  é tratada como "não aplicável (com justificativa)", estado previsto no P05, e não como
  indisponibilidade. O "Constitution Check" do plano técnico DEVE confirmar esse entendimento ou
  registrá-lo como desvio (seção 8.3 da constitution).
- **Limites (resolve DP-12 para esta feature)**: valores iniciais de 5 MB de tamanho total e 20
  arquivos por solicitação, configuráveis. Lotes nesse volume são processados dentro das metas de
  NFR-001; o processamento assíncrono de lotes maiores fica fora do escopo da versão 1.
- **Retenção na sessão**: o XML original é descartado após a análise; a sessão guarda apenas os dados
  extraídos e os resultados, reduzindo a exposição de dados pessoais e fiscais (LGPD).
- **Recomendações**: são exemplos ilustrativos para apoio à análise e não substituem a avaliação de
  profissional habilitado.
- **Metas de desempenho**: NFR-001 a NFR-004 são valores iniciais de referência para lotes de até
  5 MB e 20 arquivos, e DEVEM ser confirmados no plano técnico.

**Restrições técnicas (decididas em 2026-10-29)**

Estas decisões vêm do solicitante e da constitution (seção 3). Os detalhes de versão e configuração
pertencem ao plano técnico.

- **Hospedagem**: o agente é hospedado em ASP.NET Core sobre .NET 10.
- **Provedor de IA**: OpenAI.
- **Framework de agente**: Microsoft Agent Framework (constitution 3.1, v2.0.0).
- **Memória conversacional, sessões e contexto do usuário**: MongoDB (DP-01 resolvida na constitution
  v2.0.0; a decisão DEVE ser registrada em ADR no plano técnico).
- **Consumo e custos de IA**: PostgreSQL.
- **Base de conhecimento**: PostgreSQL com PgVector, pertencente à feature 002.
- **A2A**: comunicação real com o SDK oficial do protocolo A2A, tanto no agente (servidor) quanto no
  Sofia.Simulator (cliente). A versão do protocolo adotada é a suportada pela versão do SDK fixada
  no plano técnico (resolve parcialmente a DP-17).
- **MCP**: o agente atua como cliente MCP por um ponto de integração preparado e desabilitado por
  configuração nesta feature; a conexão real com a Api.RAG chega com a feature 002 (DP-18 continua
  pendente).
- **Checkpoints de aprovação**: persistidos em arquivos JSON locais, apenas para demonstração.
- **Repositório**: GitHub, conforme a seção 3.5 da constitution.
- **Documentação da API**: contrato OpenAPI com interface Swagger no canal direto, protegida em
  produção (constitution 5.2).
- **Testes**: testes automatizados dos cenários desta especificação, conforme o P17 da constitution.
- **Contêineres**: Docker para o agente e suas dependências (MongoDB e PostgreSQL), com subida do
  ambiente local por um único comando.

**Limitações técnicas**

- **Sem autenticação na versão 1 (desvio temporário D-05)**: a versão 1 não implementa a
  autenticação do chamador exigida pela constitution 5.2, nem o controle de papel de aprovador
  (FR-052, FR-035h, ERR-004, ERR-019, TS-031 e TS-042 adiados). Justificativa: a versão 1 serve
  para testar a validação dos XMLs. Riscos: qualquer processo com acesso à porta do agente pode
  enviar solicitações, se passar por outro usuário e aprovar recomendações sensíveis. Mitigação:
  execução apenas local (FR-052b) e dados sintéticos ou anonimizados. O desvio DEVE ser registrado
  na seção "Complexity Tracking" do plano técnico, com responsável e aprovação (constitution 8.3),
  e DEVE ser encerrado antes de qualquer implantação fora da máquina local ou da integração com a
  SOFIA real.

- **Vercel não é usada nesta feature**: desde 2026-10-27 a Vercel oferece, em beta, um runtime de
  contêiner que implanta um Dockerfile como Vercel Functions, sem disco local durável, com escala
  sob demanda e instâncias sem estado. Nessa modalidade, os checkpoints de aprovação em arquivos
  JSON locais (FR-035j) não seriam duráveis nem compartilhados entre instâncias, e o MongoDB e o
  PostgreSQL precisariam ser serviços externos. Por isso, nesta feature o agente roda localmente com
  Docker. A implantação continua pendente (DP-25) e só PODE usar a Vercel após ADR e após a troca dos
  checkpoints por armazenamento durável.
- **Checkpoints em arquivos JSON locais**: servem apenas para demonstração e NÃO DEVEM ser usados em
  produção. Funcionam com uma única instância do agente; não oferecem consistência entre instâncias,
  controle transacional ou criptografia. No contêiner, os arquivos se perdem se não houver volume
  persistente montado. Os arquivos NÃO DEVEM conter o conteúdo dos XMLs nem dados pessoais além dos
  identificadores necessários.
- **SDK oficial do A2A**: a versão do protocolo, os estados de tarefa disponíveis, o endereço de
  descoberta do Agent Card e o suporte a streaming e notificações dependem da versão do SDK. Se o
  SDK estiver em versão de pré-lançamento, mudanças incompatíveis são possíveis, e a versão DEVE ser
  fixada no plano técnico. Na versão fixada no plano (hosting A2A do Microsoft Agent Framework
  1.26.0-preview), respostas em segundo plano (tarefas A2A) não são suportadas: toda resposta é uma
  mensagem final, e os estados intermediários do FR-046 não são observáveis por A2A (desvio D-04 do
  plano, aprovado por Andressa Firmino em 2026-11-04).
- **OpenAI**: a análise por IA depende de serviço externo, com custo por uso, limites de taxa e
  disponibilidade fora do controle do projeto; os dados enviados seguem a política de minimização
  (DP-14).
