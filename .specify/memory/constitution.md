

# AgenteAuditoriaNFe Constitution

Documento normativo do projeto AuditoriaNFe. Os termos **DEVE**, **NÃO DEVE** e **PODE** indicam,
respectivamente, obrigação, proibição e opção. Toda regra marcada como DEVE ou NÃO DEVE é
verificável em revisão de pull request (PR) e prevalece sobre preferências individuais, convenções
de equipe e decisões de planos técnicos.

## 1. Propósito e escopo



### 1.1 Propósito

O AgenteAuditoriaNFe é um agente de IA que recebe arquivos XML de Nota Fiscal Eletrônica (NF-e),
valida sua estrutura e seu conteúdo e produz uma resposta estruturada, fundamentada e rastreável
sobre o resultado dessa validação.

A solução combina, nesta ordem de precedência:

1. Validação determinística de arquivos, estruturas e registros.
2. Consulta a manuais e documentos oficiais por RAG (Retrieval-Augmented Generation).
3. Uso controlado de ferramentas externas.
4. Análise complementar por LLM.
5. Resposta estruturada, validada e rastreável.
6. Exposição a outros agentes e orquestradores por A2A (Agent-to-Agent).



### 1.2 Escopo desta constitution

Esta constitution rege especificações, ADRs, planos técnicos, tarefas, implementações, testes e
revisões de todos os componentes da solução AgenteAuditoriaNFe.

Ela define princípios e restrições duradouros. Ela NÃO DEVE conter, e não substitui:

- Layouts, schemas, regras fiscais ou critérios de conformidade — estes DEVEM ser extraídos de
documentos oficiais e registrados nas especificações.
- Versões de SDKs, nomes de modelos, preços, tamanhos de chunk, topK, limiares de similaridade ou
outros parâmetros ajustáveis — estes pertencem aos planos técnicos e à configuração.



### 1.3 Fora de escopo até decisão explícita

Qualquer item listado na seção 9 (Decisões pendentes) está fora do escopo aprovado até ser
resolvido na etapa indicada. Suposições NÃO DEVEM ser tratadas como requisitos aprovados.

## 2. Princípios obrigatórios

Cada princípio possui identificador estável (P01–P18) para citação em especificações, planos,
tarefas e comentários de PR.

### P01 — Especificação antes da implementação

**Regra**

- Toda funcionalidade DEVE percorrer o fluxo da seção 6 antes de ter código de produção
incorporado à branch principal.
- Todo PR de implementação DEVE referenciar a especificação, o plano técnico e as tarefas que ele
atende.
- Código de produção sem requisito rastreável NÃO DEVE ser incorporado. Provas de conceito PODEM
existir em branches separadas, mas NÃO DEVEM ser incorporadas sem passar pelo fluxo.

**Justificativa**: o domínio fiscal exige que cada comportamento seja derivado de requisito
explícito; implementação sem especificação gera regras implícitas e não auditáveis.

**Verificação**: o PR contém links para `spec.md`, `plan.md` e `tasks.md` da funcionalidade; cada
alteração de comportamento corresponde a ao menos uma tarefa e a um requisito.

### P02 — Fontes oficiais como única base normativa

**Regra**

- Layouts, schemas (XSD), regras de validação, códigos de rejeição, regras fiscais e critérios de
conformidade DEVEM ser obtidos de documentos oficiais identificados por origem, identificador e
versão.
- Cada regra implementada DEVE referenciar, na especificação, o documento oficial, a versão e a
localização (seção, item, regra ou página), quando disponível.
- Agentes de IA, desenvolvedores e o próprio LLM NÃO DEVEM inventar, inferir ou completar regras
fiscais, layouts ou schemas sem fonte oficial registrada.
- Quando a fonte oficial for ambígua ou ausente, a regra DEVE ser registrada como decisão
pendente, e não implementada por suposição.

**Justificativa**: uma regra fiscal sem fonte não pode ser auditada nem atualizada quando o
documento oficial mudar.

**Verificação**: cada validador e cada regra de negócio possui referência à fonte oficial na
especificação; o revisor rejeita regras sem referência.

### P03 — Validação determinística precede e prevalece sobre o LLM

**Regra**

- Regras estruturais e regras de negócio determinísticas DEVEM ser implementadas como validadores
ou ferramentas verificáveis, com testes automatizados.
- O LLM NÃO DEVE substituir, repetir como fonte primária, reclassificar, suprimir ou alterar o
resultado de uma validação determinística.
- Os resultados dos validadores determinísticos DEVEM ser incorporados à resposta final pelo
sistema, a partir da saída dos próprios validadores, e não transcritos pelo LLM.
- A validação por XSD DEVE ser aplicada somente a documentos XML para os quais exista schema
oficial correspondente ao tipo e à versão identificados. Outros formatos DEVEM usar validadores
compatíveis com seus layouts.
- Quando não houver schema ou validador para o tipo e a versão identificados, a validação DEVE ser
registrada como "não realizada", e não como aprovada.

**Justificativa**: validações determinísticas são reprodutíveis e testáveis; respostas de LLM não
são.

**Verificação**: testes demonstram que o resultado dos validadores chega intacto ao envelope de
resposta; não há regra determinística implementada apenas em prompt.

### P04 — Classificação explícita dos achados

**Regra**

Todo achado, limitação ou falha DEVE pertencer a exatamente uma das categorias abaixo:


| Categoria                                 | Origem permitida                                                    |
| ----------------------------------------- | ------------------------------------------------------------------- |
| Erro estrutural do documento              | Validador estrutural ou de schema                                   |
| Inconsistência de negócio                 | Validador ou ferramenta determinística de regra de negócio          |
| Indício identificado pela IA              | Análise do LLM, com evidência associada                             |
| Ausência de evidências                    | Recuperação insuficiente ou fonte inexistente para o ponto avaliado |
| Falha técnica ou dependência indisponível | Erro de execução, timeout ou serviço indisponível                   |


- Um achado produzido pelo LLM NÃO DEVE ser classificado como erro estrutural ou inconsistência de
negócio.
- Uma falha técnica NÃO DEVE ser apresentada como erro do documento.

**Justificativa**: o usuário precisa distinguir o que foi comprovado deterministicamente, o que é
interpretação da IA e o que não pôde ser avaliado.

**Verificação**: o contrato de resposta possui o campo de categoria; testes cobrem ao menos um
caso de cada categoria; a validação semântica da saída (P08) rejeita categorias incompatíveis com
a origem.

### P05 — Nenhuma conformidade sem validações obrigatórias concluídas

**Regra**

- O agente NÃO DEVE declarar o documento conforme quando qualquer validação obrigatória não tiver
sido concluída com sucesso, incluindo os casos de "não realizada", "falhou" ou "dependência
indisponível".
- Cada etapa do fluxo de validação (seção 3.4) DEVE registrar seu estado: executada, falhou, não
aplicável (com justificativa) ou não executada.
- A resposta DEVE listar as validações não realizadas e o motivo.

**Justificativa**: uma conclusão de conformidade parcial seria interpretada como total e poderia
gerar decisões fiscais incorretas.

**Verificação**: testes com dependências indisponíveis (RAG, ferramentas, LLM) provam que o status
final nunca indica conformidade nesses casos.

### P06 — Fundamentação rastreável

**Regra**

- Toda afirmação baseada em conhecimento recuperado por RAG DEVE apresentar referências
recuperáveis: identificador do documento, versão e identificador do chunk, além da localização
quando disponível.
- Referências citadas na resposta DEVEM corresponder a chunks efetivamente recuperados naquela
execução.
- Na ausência de evidências suficientes, o agente DEVE declarar a limitação, usando a categoria
"Ausência de evidências", e NÃO DEVE preencher a lacuna com conhecimento genérico do modelo
apresentado como norma.

**Justificativa**: auditoria exige que cada conclusão possa ser conferida na fonte.

**Verificação**: a validação semântica rejeita referências não presentes no conjunto recuperado;
testes cobrem o cenário sem chunks relevantes.

### P07 — Conteúdo externo é dado não confiável

**Regra**

- O conteúdo de arquivos recebidos, chunks recuperados, resultados de ferramentas, memória
conversacional e mensagens recebidas por A2A DEVE ser tratado como dado, nunca como instrução.
- Esse conteúdo DEVE ser delimitado e identificado como dado não confiável no contexto enviado ao
LLM.
- Nenhum conteúdo externo PODE alterar instruções de sistema, contrato de saída, lista de
ferramentas autorizadas, regras de autorização ou parâmetros de execução.

**Justificativa**: XML, manuais e respostas de terceiros podem conter texto que tenta manipular o
agente (prompt injection).

**Verificação**: testes com documentos e chunks contendo instruções maliciosas provam que o
comportamento e o contrato permanecem inalterados.

### P08 — Saída estruturada contratada e validada

**Regra**

- A resposta do LLM DEVE seguir um contrato JSON explícito e versionado (ver seção 4.4).
- DEVE ser usado o mecanismo de saída estruturada compatível com o modelo configurado.
- Antes de aceita, a resposta DEVE passar por validação estrutural (schema) e semântica
(consistência com resultados de validadores, referências recuperadas, categorias e regra de
conformidade do P05).
- Respostas inválidas DEVEM seguir uma política de retentativas limitada e configurável. Esgotadas
as tentativas, a execução DEVE terminar com status de falha, e o conteúdo inválido NÃO DEVE ser
retornado como resultado válido.
- O LLM NÃO DEVE fabricar evidências, resultados de ferramentas ou referências; a validação
semântica DEVE detectar e rejeitar esses casos quando forem verificáveis.

**Justificativa**: consumidores humanos e agentes dependem de um formato estável e de conteúdo
confiável.

**Verificação**: testes de contrato com respostas válidas, malformadas e semanticamente
inconsistentes; teste do limite de retentativas.

### P09 — Metadados operacionais pertencem ao sistema

**Regra**

- O JSON produzido pelo LLM e o envelope retornado pela API DEVEM ser contratos distintos.
- Identificador da execução, correlação, usuário, timestamps, durações, consumo de tokens,
custos, versões de contrato, modelo utilizado e status técnico DEVEM ser preenchidos pelo
sistema.
- O LLM NÃO DEVE ser solicitado a gerar esses valores, e valores equivalentes eventualmente
presentes na saída do modelo DEVEM ser ignorados ou rejeitados.

**Justificativa**: o modelo não conhece de forma confiável esses dados, e qualquer valor gerado
por ele seria inventado.

**Verificação**: o schema do contrato do LLM não contém esses campos; testes confirmam que o
envelope é montado pelo sistema.

### P10 — Ferramentas governadas pelo sistema

**Regra**

- A seleção de uma ferramenta pelo LLM NÃO DEVE conceder autorização para executá-la.
- Antes de cada invocação, o sistema DEVE verificar a autorização com base no usuário, na
organização (quando aplicável), no contexto da execução e na política da ferramenta.
- Cada ferramenta DEVE cumprir os requisitos da seção 4.6.
- Ferramentas com efeitos colaterais DEVEM ser explicitamente autorizadas na especificação e NÃO
DEVEM ser habilitadas por padrão.

**Justificativa**: Function Calling transfere ao modelo a decisão de chamar uma função; a decisão
de permitir a chamada deve permanecer com o sistema.

**Verificação**: existe um ponto único de controle de autorização antes da execução das
ferramentas; testes provam que chamadas não autorizadas são negadas e registradas.

### P11 — Serviços proprietários dos seus dados

**Regra**

- Cada serviço DEVE ser o único proprietário dos seus dados e do seu armazenamento.
- A Api.Agente NÃO DEVE acessar diretamente tabelas, vetores, índices ou arquivos internos da
Api.RAG, e vice-versa.
- A comunicação funcional da Api.Agente com a Api.RAG DEVE ocorrer exclusivamente por MCP.
- Os serviços NÃO DEVEM compartilhar bibliotecas de domínio ou de acesso a dados. O
compartilhamento de contratos PODE ocorrer somente se justificado em ADR.

**Justificativa**: o acoplamento por banco de dados impede a evolução independente e viola os
limites de responsabilidade.

**Verificação**: a Api.Agente não possui connection string, credencial ou referência de projeto
para o armazenamento da Api.RAG; o revisor rejeita acessos diretos.

### P12 — Camadas separadas e simplicidade justificada

**Regra**

- Cada API DEVE separar endpoints, aplicação, domínio e infraestrutura conforme a seção 3.3.
- Projetos, bibliotecas, padrões ou dependências adicionais DEVEM ser justificados no plano
técnico (seção "Complexity Tracking") ou em ADR, indicando a alternativa mais simples rejeitada.
- Abstrações sem uso concreto atual NÃO DEVEM ser criadas.

**Justificativa**: a separação de camadas protege o domínio; a justificativa obrigatória evita
complexidade especulativa.

**Verificação**: as dependências entre camadas respeitam a seção 3.3; cada novo projeto ou
dependência relevante possui justificativa registrada.

### P13 — Configuração externa e parâmetros avaliados

**Regra**

- Modelo de LLM, parâmetros de execução, modelo de embeddings, endpoints, limites, timeouts,
quantidade de retentativas, tamanho de chunk, topK e limiares de similaridade DEVEM ser
configuração externa ao código.
- Parâmetros de recuperação e de segmentação NÃO DEVEM ser fixados arbitrariamente; DEVEM ser
especificados no plano técnico e avaliados com exemplos representativos, com o resultado da
avaliação registrado.
- Alterar esses parâmetros em produção DEVE exigir nova avaliação registrada.

**Justificativa**: parâmetros de IA determinam a qualidade das respostas e precisam ser ajustados
com evidência, não por intuição.

**Verificação**: não há valores literais desses parâmetros no código de produção; existe
registro de avaliação vinculado ao plano.

### P14 — Contabilização fiel de consumo e custo

**Regra**

- Toda chamada a provedor de LLM ou de embeddings, incluindo chamadas adicionais e retentativas,
DEVE gerar um registro de consumo conforme a seção 4.5.
- Valores estimados DEVEM ser distinguidos de valores informados pelo provedor.
- Consumo desconhecido DEVE ser registrado como desconhecido (ausente), e NÃO DEVE ser registrado
como zero.
- Falhas ao registrar consumo NÃO DEVEM ser silenciosas; DEVEM gerar log e métrica.

**Justificativa**: custos de IA precisam ser auditáveis por execução, e um zero falso distorce
relatórios financeiros.

**Verificação**: testes cobrem chamadas bem-sucedidas, falhas, retentativas e ausência de
informação de uso do provedor.

### P15 — Memória isolada e governada

**Regra**

- A memória conversacional DEVE ser isolada por usuário e por conversa, e por organização quando
aplicável.
- Ela DEVE ter políticas de retenção, expiração e exclusão definidas na especificação.
- O histórico enviado ao modelo DEVE ter limite configurável.
- O conteúdo da memória NÃO DEVE ser usado como fonte normativa nem como evidência oficial; o
conhecimento oficial vem exclusivamente da Api.RAG.
- A tecnologia de armazenamento (MongoDB ou Redis) DEVE ser definida em ADR antes da
implementação (DP-01).

**Justificativa**: o vazamento entre usuários e a confusão entre histórico e norma são riscos
graves de privacidade e de correção.

**Verificação**: testes provam que um usuário não acessa a memória de outro; a chave de
isolamento inclui usuário e conversa; existe ADR aprovado.

### P16 — Observabilidade e rastreabilidade de execução

**Regra**

- Toda execução DEVE possuir identificador único e identificador de correlação, gerados pelo
sistema e propagados entre Api.Agente, Api.RAG (via MCP), ferramentas e chamadas A2A.
- Logs DEVEM ser estruturados e conter a correlação da execução.
- Cada etapa do fluxo de validação, cada consulta RAG e cada execução de ferramenta DEVE ser
registrada com início, duração, status e erro, quando houver.
- Logs NÃO DEVEM conter segredos, conteúdo integral de documentos ou dados pessoais não
mascarados (ver seção 5.1).

**Justificativa**: auditar uma resposta exige reconstruir o que foi executado, consultado e
decidido.

**Verificação**: dado um identificador de execução, é possível reconstruir as etapas, as
consultas e as ferramentas usadas a partir dos registros.

### P17 — Testabilidade e avaliação contínua

**Regra**

- Validadores determinísticos DEVEM ter testes automatizados com casos válidos e inválidos,
derivados de documentos oficiais ou de exemplos com origem registrada.
- Integrações com LLM, MCP, A2A e ferramentas DEVEM ter testes de contrato e testes com
dependências substituídas por dublês.
- O RAG e a análise por LLM DEVEM possuir um conjunto de avaliação versionado, com exemplos
representativos e critérios de qualidade definidos no plano técnico.
- Os testes NÃO DEVEM usar dados reais de contribuintes sem anonimização.
- Testes de integração com PostgreSQL/PgVector e com o armazenamento de memória DEVEM usar
instâncias reais e descartáveis, e NÃO DEVEM substituir esses bancos por simulações em memória.

**Justificativa**: componentes probabilísticos só são confiáveis quando medidos continuamente
contra casos conhecidos.

**Verificação**: o pipeline de CI executa os testes; alterações de prompt, modelo ou parâmetro de
RAG incluem o resultado do conjunto de avaliação.

### P18 — Interoperabilidade por contrato (A2A e MCP)

**Regra**

- A exposição do agente por A2A e as ferramentas expostas por MCP DEVEM possuir contratos
versionados e documentados.
- Uma solicitação recebida por A2A DEVE percorrer o mesmo fluxo de validação, autenticação,
autorização e validação de saída que uma solicitação recebida pela API REST. Nenhum canal PODE
contornar o fluxo.
- Alterações incompatíveis em contratos DEVEM gerar nova versão e plano de migração para os
consumidores.

**Justificativa**: outros agentes e orquestradores dependem de contratos estáveis, e um canal
alternativo não pode ser uma porta de entrada com menos controles.

**Verificação**: testes de contrato por canal; o revisor confirma que os canais compartilham o
mesmo caso de uso de aplicação.

## 3. Restrições arquiteturais



### 3.1 Plataforma

- A solução DEVE se chamar AgenteAuditoriaNFe.
- A solução DEVE utilizar .NET 10 e ASP.NET Core.
- A integração com LLM DEVE utilizar a OpenAI.
- A orquestração de ferramentas por Function Calling na Api.Agente DEVE utilizar Semantic Kernel.
- Vetores DEVEM ser armazenados e consultados em PostgreSQL com PgVector.
- O controle de consumo e custos DEVE ser persistido em PostgreSQL.
- A memória conversacional DEVE utilizar MongoDB ou Redis, conforme ADR (DP-01).
- Versões específicas de SDKs e bibliotecas DEVEM ser definidas nos planos técnicos, e não nesta
constitution.



### 3.2 Serviços

**Api.Agente** — responsabilidades exclusivas:

- Disponibilizar API REST para execução do agente.
- Receber arquivos e solicitações de análise.
- Orquestrar o fluxo de validação (seção 3.4).
- Integrar-se à OpenAI.
- Consultar a Api.RAG como cliente MCP.
- Executar ferramentas por Function Calling via Semantic Kernel, sob controle de autorização
(P10).
- Gerenciar a memória conversacional.
- Registrar consumo de tokens e custos.
- Validar a resposta estruturada do LLM.
- Expor o agente por servidor A2A.

**Api.RAG** — responsabilidades exclusivas:

- Disponibilizar API REST de administração da base de conhecimento.
- Disponibilizar endpoint POST de ingestão.
- Processar documentos, gerar chunks e embeddings.
- Armazenar e consultar vetores em PostgreSQL com PgVector.
- Expor ferramentas de recuperação de conhecimento por servidor MCP.

Regras complementares:

- As ferramentas MCP expostas pela Api.RAG para a Api.Agente DEVEM ser de leitura (recuperação).
Operações de ingestão, alteração ou exclusão NÃO DEVEM ser expostas por MCP sem especificação e
ADR aprovados.
- A ingestão e a administração da base de conhecimento DEVEM exigir autorização administrativa
distinta da autorização de uso do agente.
- Uma responsabilidade NÃO DEVE ser implementada no serviço que não a possui.



### 3.3 Camadas internas

Cada API DEVE organizar seu código nas camadas abaixo, respeitando o sentido de dependência
Endpoints → Aplicação → Domínio, com a Infraestrutura implementando abstrações definidas na
Aplicação ou no Domínio.


| Camada         | Responsabilidade                                                                                 | Restrições                                                                                              |
| -------------- | ------------------------------------------------------------------------------------------------ | ------------------------------------------------------------------------------------------------------- |
| Endpoints      | HTTP, MCP server, A2A server, autenticação de entrada, mapeamento de contratos                   | NÃO DEVE conter regra de negócio nem acesso a dados                                                     |
| Aplicação      | Casos de uso, orquestração do fluxo, autorização de ferramentas, políticas                       | NÃO DEVE depender de detalhes de infraestrutura concretos                                               |
| Domínio        | Entidades, resultados de validação, categorias de achado, regras determinísticas                 | NÃO DEVE depender de ASP.NET Core, Semantic Kernel, SDKs de OpenAI, MCP ou A2A, nem de drivers de banco |
| Infraestrutura | Persistência, clientes OpenAI, MCP e A2A, Semantic Kernel, armazenamento de memória e de vetores | NÃO DEVE ser referenciada pelo Domínio                                                                  |


- SDKs externos (OpenAI, Semantic Kernel, MCP, A2A e drivers de banco) DEVEM ficar confinados à
Infraestrutura ou a adaptadores das bordas, salvo exceção justificada em ADR.
- A forma física das camadas (projetos separados ou pastas e namespaces em um projeto) DEVE ser
definida no primeiro plano técnico (DP-19).
- O cumprimento das dependências entre camadas PODE ser verificado por testes de arquitetura.



### 3.4 Fluxo obrigatório de validação documental

Toda análise DEVE seguir, nesta ordem:

1. Receber o arquivo e identificar usuário, contexto e execução (identificadores gerados pelo
  sistema).
2. Validar tamanho, formato e condições de processamento.
3. Identificar o tipo de documento e a versão do layout.
4. Validar estrutura, registros e schema aplicável (P03).
5. Consultar por MCP os chunks do manual correspondente ao tipo e à versão identificados.
6. Executar ferramentas externas quando necessário e autorizado (P10).
7. Produzir a análise com o LLM.
8. Validar a saída estruturada (P08).
9. Retornar o resultado com evidências e rastreabilidade (P06, P09, P16).

Regras do fluxo:

- Uma etapa NÃO DEVE ser omitida silenciosamente; cada etapa DEVE registrar seu estado (P05).
- Falha nas etapas 1 ou 2 DEVE interromper o processamento com erro explícito.
- Falha ou indisponibilidade nas etapas 3 a 6 DEVE ser registrada na categoria correspondente
(P04) e impede a declaração de conformidade (P05). A especificação DEVE definir se o fluxo
continua de forma parcial ou se é interrompido.
- O resultado das etapas 3 e 4 DEVE ser fornecido ao LLM como dado de entrada, e não pode ser
alterado por ele (P03).

### 3.5 Repositório, empacotamento e implantação

**Repositório**

- O código-fonte, as especificações, os planos, as tarefas, os ADRs e esta constitution DEVEM ser
versionados em repositório Git hospedado no GitHub.
- A branch principal DEVE ser protegida: alterações somente via PR, com CI aprovado e revisão
aprovada (seção 5.3). Push direto e force push na branch principal NÃO DEVEM ser permitidos.
- A configuração de workflows, checks obrigatórios e regras de proteção DEVE ser definida no plano
técnico (DP-26).

**Contêineres**

- Cada serviço (Api.Agente e Api.RAG) DEVE ser empacotado como imagem de contêiner Docker
reprodutível, construída a partir do repositório.
- O ambiente local DEVE permitir subir os serviços e suas dependências (PostgreSQL com PgVector e
o armazenamento de memória) com um único comando documentado.
- Imagens NÃO DEVEM conter segredos, arquivos de configuração com credenciais ou dados de
contribuintes.
- Os contêineres dos serviços DEVEM executar com usuário não privilegiado (não root).
- Imagens base, Dockerfiles e arquivos de composição DEVEM ser definidos no plano técnico.

**Implantação**

- O ambiente de implantação DEVE suportar o runtime .NET 10, processos de longa duração para os
servidores MCP e A2A e as dependências da seção 3.1.
- A plataforma de implantação DEVE ser definida em ADR antes da primeira implantação (DP-25).



## 4. Contratos e governança dos dados



### 4.1 Documentos oficiais da base de conhecimento

Cada documento ingerido DEVE registrar, no mínimo:

- Origem (emissor ou fonte oficial e endereço de obtenção, quando houver).
- Identificador do documento.
- Versão e, quando disponíveis, data de publicação e período de vigência.
- Tipos de documento fiscal e versões de layout a que se aplica.
- Hash do conteúdo, para deduplicação e detecção de alteração.
- Data da ingestão e o responsável pela ingestão.

Um documento sem origem e versão identificadas NÃO DEVE ser ingerido como fonte normativa.

### 4.2 Chunks e embeddings

- Cada chunk DEVE permitir recuperar o documento de origem, sua versão e sua localização (seção,
item, página ou equivalente), quando disponível.
- Cada chunk DEVE registrar o modelo de embeddings, a versão desse modelo e a dimensão do vetor.
- Vetores gerados por modelos ou dimensões diferentes NÃO DEVEM ser comparados na mesma busca.
- O modelo de embeddings, a dimensão, a métrica de distância, o tipo de índice e a estratégia de
busca DEVEM ser compatíveis entre si e documentados em plano técnico ou ADR (DP-06).



### 4.3 Ingestão e recuperação

- A ingestão DEVE definir e documentar critérios de segmentação, deduplicação, reprocessamento e
atualização.
- A ingestão DEVE ser idempotente: reprocessar o mesmo documento e a mesma versão NÃO DEVE gerar
chunks duplicados.
- Uma nova versão de manual DEVE coexistir com as versões anteriores até sua desativação
explícita; versões antigas NÃO DEVEM ser sobrescritas silenciosamente.
- A troca do modelo de embeddings DEVE exigir o reprocessamento dos vetores afetados.
- A recuperação DEVE filtrar pelo tipo de documento e pela versão do manual aplicável à execução.
- O resultado da recuperação DEVE retornar, para cada chunk, o identificador, a referência da
fonte, a versão e a pontuação de similaridade.
- Tamanho de chunk, sobreposição, topK, limiares e eventual busca híbrida DEVEM seguir o P13.



### 4.4 Contrato de resposta do LLM e envelope da API

**Contrato do LLM** (produzido pelo modelo e validado pelo sistema) DEVE ser versionado e
contemplar, conforme a especificação:

- Resultado da análise.
- Achados com categoria (P04) e severidade.
- Evidências e referências (P06).
- Limitações e validações não realizadas.
- Indicação de necessidade de revisão humana.

**Envelope da API** (produzido pelo sistema) DEVE contemplar:

- Identificador da execução e da correlação.
- Status do processamento.
- Versão do contrato do envelope e do contrato do LLM.
- Resultados dos validadores determinísticos (P03).
- O conteúdo validado do contrato do LLM.
- Metadados operacionais: modelo, tempos, consumo e custo, quando disponíveis (P09, P14).

Regras:

- A enumeração de status, a escala de severidades e os critérios de revisão humana DEVEM ser
definidos na especificação (DP-08, DP-09).
- A revisão humana DEVE ser indicada, no mínimo, quando uma conclusão depender exclusivamente de
indícios da IA ou quando houver ausência de evidências para um ponto avaliado.
- Alterações incompatíveis em qualquer contrato DEVEM gerar nova versão (P18).
- Os contratos REST DEVEM ser documentados em OpenAPI. Erros DEVEM seguir um formato padronizado
e documentado.



### 4.5 Registro de consumo e custos

Cada chamada ao provedor (incluindo retentativas e chamadas adicionais) DEVE registrar, quando
disponível:

- Identificador da execução e da correlação.
- Provedor e modelo.
- Tokens de entrada e de saída.
- Consumo de embeddings, quando aplicável.
- Data e hora, duração e status da chamada.
- Número da tentativa.
- Estimativa de custo, moeda e referência da versão da tabela de preços usada.
- Indicação de valor informado pelo provedor ou estimado pelo sistema.

A forma de registro do consumo de embeddings gerado pela Api.RAG DEVE respeitar o P11 e será
definida em ADR (DP-15).

### 4.6 Contrato de ferramentas

Cada ferramenta, incluindo funções do Semantic Kernel e ferramentas MCP, DEVE possuir:

- Nome único e uma responsabilidade única e descrita.
- Contrato de entrada e de saída documentado e versionado.
- Validação de argumentos antes da execução; argumentos inválidos DEVEM ser rejeitados, e não
corrigidos por suposição.
- Regras de autenticação e de autorização (P10).
- Timeout e tratamento de falhas mapeado para a categoria "Falha técnica ou dependência
indisponível".
- Política de retentativa e garantia de idempotência, quando aplicável.
- Testes automatizados e registro de execução (P16, P17).

Resultados de ferramentas DEVEM ser tratados como dados não confiáveis (P07).

### 4.7 Memória conversacional

- O histórico conversacional e o conhecimento oficial DEVEM ser armazenados e tratados
separadamente (P15).
- Dados sensíveis DEVEM ser protegidos conforme a classificação da seção 5.1.
- A exclusão de uma conversa ou de dados de um usuário DEVE ser suportada conforme a política de
retenção (DP-13).



## 5. Segurança e qualidade



### 5.1 Proteção de dados

- Documentos fiscais podem conter dados pessoais e dados fiscais sigilosos. A especificação DEVE
classificar os dados tratados e definir o mascaramento aplicável.
- O envio de conteúdo de documentos ao provedor de LLM DEVE seguir o princípio da minimização,
conforme política a ser definida (DP-14).
- Segredos (chaves de API, connection strings, certificados) NÃO DEVEM ser versionados no
repositório nem gravados em logs; DEVEM ser fornecidos por mecanismo seguro de configuração.
- Arquivos temporários DEVEM ser removidos ao término da execução, salvo retenção especificada.



### 5.2 Segurança de entrada

- Todos os endpoints REST, MCP e A2A DEVEM exigir autenticação e autorização, exceto os
explicitamente públicos (por exemplo, health checks sem dados), conforme ADR (DP-10).
- O processamento de XML NÃO DEVE permitir DTD nem resolução de entidades externas, e DEVE impor
limites de tamanho e de profundidade.
- O tipo do arquivo DEVE ser validado pelo conteúdo, e não apenas pela extensão ou pelo
content-type informado.
- Os limites de tamanho, de quantidade e de formatos aceitos DEVEM ser definidos na especificação
(DP-12) e aplicados antes de qualquer processamento custoso.
- Os endpoints DEVEM ter limitação de taxa ou de concorrência definida no plano técnico.
- A interface interativa de documentação da API (por exemplo, Swagger UI) NÃO DEVE ficar exposta
publicamente em produção sem autenticação. A escolha da ferramenta DEVE ser feita no plano técnico.



### 5.3 Qualidade de código

- O build DEVE compilar sem erros, e os avisos DEVEM seguir a política definida no plano técnico.
- Nullable reference types DEVEM estar habilitados.
- Todo PR DEVE passar por revisão de ao menos uma pessoa diferente do autor, com verificação
explícita da conformidade com esta constitution.
- O pipeline de CI DEVE executar build e testes; um PR com testes falhando NÃO DEVE ser
incorporado.
- Código gerado por agentes de IA DEVE cumprir as mesmas regras e passar pela mesma revisão que o
código escrito manualmente.



### 5.4 Resiliência

- Chamadas a OpenAI, Api.RAG, ferramentas externas e bancos de dados DEVEM ter timeout explícito.
- As retentativas DEVEM ser limitadas, configuráveis e restritas a falhas transitórias.
- A indisponibilidade de uma dependência DEVE resultar em resposta explícita de falha técnica ou
dependência indisponível, e não em resultado parcial apresentado como completo (P05).



## 6. Fluxo de desenvolvimento SDD



### 6.1 Etapas

Constitution → Especificação → Esclarecimentos → Plano técnico → Tarefas → Implementação →
Validação.


| Etapa           | Artefato                            | Critério de saída                                                                  |
| --------------- | ----------------------------------- | ---------------------------------------------------------------------------------- |
| Especificação   | `spec.md`                           | Requisitos e critérios de aceite verificáveis; fontes oficiais referenciadas (P02) |
| Esclarecimentos | `spec.md` atualizado                | Nenhuma ambiguidade crítica aberta; pendências registradas com etapa de resolução  |
| Plano técnico   | `plan.md` e ADRs                    | "Constitution Check" aprovado; parâmetros ajustáveis e desvios justificados        |
| Tarefas         | `tasks.md`                          | Cada tarefa ligada a requisito(s) e a teste(s)                                     |
| Implementação   | Código e testes                     | Tarefas concluídas, com testes correspondentes                                     |
| Validação       | Resultados de testes e de avaliação | Definition of Done (seção 7) cumprida                                              |




### 6.2 Rastreabilidade

- Requisitos, critérios de aceite, tarefas e testes DEVEM ter identificadores estáveis.
- Cada tarefa DEVE referenciar ao menos um requisito; cada requisito DEVE ser coberto por ao menos
um teste ou avaliação.
- Cada regra fiscal ou de layout DEVE referenciar sua fonte oficial (P02).



### 6.3 ADRs

Decisões arquiteturais relevantes DEVEM ser registradas em ADR antes da implementação que
dependa delas. São relevantes, no mínimo: escolha de tecnologia de armazenamento, contratos entre
serviços, modelo de autenticação e autorização, estratégia de RAG, desvios desta constitution e
inclusão de projetos ou dependências estruturais. Cada ADR DEVE registrar contexto, decisão,
alternativas consideradas, consequências, status e data.

### 6.4 Gate do plano técnico

O "Constitution Check" do plano DEVE verificar, no mínimo, os princípios P01–P18, as seções 3 e 4
e as decisões pendentes que bloqueiam a funcionalidade. Violações DEVEM ser corrigidas ou
registradas como desvio (seção 8.3).

## 7. Definition of Done

Uma funcionalidade só está concluída quando todos os itens aplicáveis forem verdadeiros:

- [ ] Especificação, plano e tarefas existem e estão vinculados ao PR (P01).
- [ ] Toda regra fiscal ou de layout implementada referencia fonte oficial e versão (P02).
- [ ] Validações determinísticas estão implementadas fora do LLM e testadas (P03, P17).
- [ ] Os achados usam as cinco categorias, coerentes com a origem (P04).
- [ ] Cenários com validação incompleta não declaram conformidade (P05).
- [ ] Referências do RAG são recuperáveis e pertencem ao conjunto recuperado (P06).
- [ ] Existem testes contra injeção de instruções em arquivos, chunks e ferramentas (P07).
- [ ] O contrato do LLM é versionado e validado estrutural e semanticamente, com retentativas
limitadas (P08).
- [ ] Os metadados operacionais são preenchidos pelo sistema (P09).
- [ ] As ferramentas têm autorização verificada pelo sistema e cumprem a seção 4.6 (P10).
- [ ] Não há acesso direto aos dados de outro serviço (P11).
- [ ] As camadas respeitam a seção 3.3, e a complexidade adicional está justificada (P12).
- [ ] Parâmetros ajustáveis estão externalizados, e a avaliação está registrada (P13).
- [ ] O consumo e o custo de todas as chamadas, incluindo retentativas, são registrados (P14).
- [ ] A memória está isolada, com retenção definida (P15).
- [ ] A execução é rastreável por identificador de correlação, sem dados sensíveis em log (P16).
- [ ] O conjunto de avaliação foi executado quando houve alteração de prompt, modelo ou RAG (P17).
- [ ] Os testes de integração usam instâncias reais e descartáveis dos bancos (P17).
- [ ] Os serviços afetados geram imagem de contêiner sem segredos, executando como não root, e o
ambiente local sobe com o comando documentado (seção 3.5).
- [ ] A documentação interativa da API não está exposta publicamente em produção (seção 5.2).
- [ ] Os contratos REST, MCP e A2A estão documentados e versionados (P18, seção 4.4).
- [ ] Os requisitos de segurança da seção 5 foram atendidos.
- [ ] O CI está verde, e a revisão foi aprovada por pessoa diferente do autor.
- [ ] Os desvios foram registrados conforme a seção 8.3.



## 8. Governança e alterações



### 8.1 Supremacia

Esta constitution prevalece sobre especificações, planos, tarefas e convenções. Um artefato que a
contradiga DEVE ser corrigido ou ter o desvio registrado.

### 8.2 Revisão de conformidade

- Todo PR DEVE ser revisado quanto à conformidade com esta constitution, e o revisor DEVE citar o
princípio (Pxx) ou a seção violada ao solicitar mudanças.
- O "Constitution Check" do plano técnico DEVE ser reavaliado após o desenho detalhado.



### 8.3 Desvios

Todo desvio DEVE registrar:

- A regra desviada (princípio ou seção).
- A justificativa e a alternativa conforme rejeitada.
- O impacto e os riscos.
- O responsável.
- A aprovação (quem aprovou e quando).
- O prazo de revisão, quando o desvio for temporário.

Desvios DEVEM ser registrados em ADR ou na seção "Complexity Tracking" do plano técnico. Um desvio
sem aprovação registrada NÃO DEVE ser incorporado. Desvios temporários vencidos DEVEM ser
corrigidos ou reaprovados.

### 8.4 Alterações desta constitution

- Toda alteração DEVE registrar versão, data, motivação e avaliação de impacto nas especificações,
planos e implementações existentes.
- O versionamento DEVE seguir SemVer:
  - MAJOR: remoção ou redefinição incompatível de princípio ou regra.
  - MINOR: novo princípio ou seção, ou ampliação material de regra.
  - PATCH: esclarecimento ou redação, sem mudança semântica.
- As especificações afetadas por uma alteração MAJOR ou MINOR DEVEM ser reavaliadas e, quando
necessário, atualizadas.
- Os aprovadores de alterações DEVEM ser definidos conforme DP-20.

### 8.5 Histórico de alterações

| Versão | Data | Motivação | Impacto nas especificações existentes |
| ------ | ---------- | --------- | ------------------------------------- |
| 1.0.0 | 2026-10-29 | Adoção inicial da constitution | Nenhum (não havia especificações) |
| 1.1.0 | 2026-10-29 | Regras de repositório GitHub, contêineres Docker, implantação, documentação interativa da API e testes de integração com bancos reais | Nenhum (não havia especificações) |

**Versão**: 1.1.0 | **Ratificada em**: 2026-10-29 | **Última alteração**: 2026-10-29

## 9. Decisões pendentes

Os itens abaixo NÃO estão aprovados. Nenhuma implementação PODE depender deles antes de sua
resolução na etapa indicada.


| ID    | Decisão pendente                                                                                            | Etapa de resolução                                          |
| ----- | ----------------------------------------------------------------------------------------------------------- | ----------------------------------------------------------- |
| DP-01 | Armazenamento da memória conversacional: MongoDB ou Redis                                                   | ADR antes do plano técnico da funcionalidade de memória     |
| DP-02 | Tipos de documento, modelos e versões de layout no escopo (inclusive eventos e formatos não XML)            | Especificação da primeira funcionalidade de validação       |
| DP-03 | Manuais, notas técnicas e pacotes de schema oficiais a ingerir, com suas versões                            | Especificação de ingestão (Api.RAG)                         |
| DP-04 | Regras de negócio determinísticas a implementar e respectivas fontes oficiais                               | Especificação e esclarecimentos de cada funcionalidade      |
| DP-05 | Modelo OpenAI e parâmetros de execução                                                                      | Plano técnico da Api.Agente                                 |
| DP-06 | Provedor e modelo de embeddings, dimensão, métrica de distância, índice e estratégia de busca               | ADR e plano técnico da Api.RAG                              |
| DP-07 | Segmentação, sobreposição, topK, limiares e busca híbrida, com conjunto de avaliação                        | Plano técnico da Api.RAG, com avaliação registrada          |
| DP-08 | Schema v1 do contrato do LLM e do envelope, com enumeração de status e escala de severidades                | Especificação da funcionalidade de análise                  |
| DP-09 | Critérios de revisão humana além do mínimo da seção 4.4                                                     | Especificação da funcionalidade de análise                  |
| DP-10 | Mecanismo de autenticação e autorização, modelo de organização (multi-tenant) e autenticação entre serviços | ADR antes da primeira funcionalidade exposta                |
| DP-11 | Ferramentas externas disponíveis, seus contratos e políticas de autorização                                 | Especificação de cada ferramenta                            |
| DP-12 | Limites de tamanho, quantidade e formatos de arquivo aceitos                                                | Especificação da funcionalidade de recepção                 |
| DP-13 | Retenção e exclusão de memória, arquivos, logs e registros de consumo (LGPD)                                | Especificação e ADR                                         |
| DP-14 | Política de envio de dados ao provedor de LLM (minimização, mascaramento, retenção pelo provedor)           | ADR antes da primeira chamada ao LLM com dados de documento |
| DP-15 | Registro e correlação do consumo de embeddings da Api.RAG sem violar o P11                                  | ADR antes do plano de ingestão                              |
| DP-16 | Topologia física do PostgreSQL por serviço (instância, banco, schema e credenciais)                         | ADR antes do primeiro plano com persistência                |
| DP-17 | Versão do protocolo A2A, Agent Card, skills expostas e autenticação                                         | Especificação da funcionalidade A2A                         |
| DP-18 | Transporte e versão do protocolo MCP entre Api.Agente e Api.RAG                                             | ADR e plano técnico da integração MCP                       |
| DP-19 | Forma física das camadas e estrutura da solução                                                             | Primeiro plano técnico, com ADR                             |
| DP-20 | Aprovadores da constitution, dos desvios e dos ADRs; local dos ADRs no repositório                          | Antes da primeira especificação                             |
| DP-21 | Processamento síncrono ou assíncrono (filas, lotes) para arquivos grandes                                   | Especificação e plano da funcionalidade de recepção         |
| DP-22 | Fonte, versionamento e atualização da tabela de preços                                                      | Plano técnico de consumo e custos                           |
| DP-23 | Ferramentas de observabilidade (logs, métricas e tracing)                                                   | Plano técnico inicial                                       |
| DP-24 | Política de continuidade do fluxo após falha parcial nas etapas 3 a 6                                       | Especificação da funcionalidade de análise                  |
| DP-25 | Plataforma de implantação das APIs e dependências, atendendo à seção 3.5 (a Vercel não oferece runtime oficial para .NET e só PODE ser considerada para um eventual frontend) | ADR antes da primeira implantação |
| DP-26 | Ferramenta de CI/CD, checks obrigatórios, regras de proteção de branch e registro de imagens de contêiner | Plano técnico inicial |


