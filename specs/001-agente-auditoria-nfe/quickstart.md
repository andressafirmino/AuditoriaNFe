# Quickstart: Agente de Auditoria de NF-e (AuditoriaNFe)

**Feature**: `001-agente-auditoria-nfe` | **Data**: 2026-10-29, revisado em 2026-11-04 |
**Plano**: [plan.md](./plan.md)

Guia para subir o ambiente local e validar de ponta a ponta os cenários principais da feature. Ele
descreve comandos e resultados esperados; a implementação é definida em `tasks.md`. Contratos e
modelo de dados: [contracts/api-agente.openapi.yaml](./contracts/api-agente.openapi.yaml),
[contracts/sofia-a2a.md](./contracts/sofia-a2a.md) e [data-model.md](./data-model.md).

> Este guia vale depois da implementação das tarefas. Os nomes de projetos, variáveis e rotas são os
> definidos no [plano](./plan.md).

> **Somente ambiente local (desvio D-05).** A versão 1 não tem autenticação. Execute apenas na sua
> máquina, com as portas em `127.0.0.1`, e apenas com XMLs sintéticos ou anonimizados (FR-052b).

## 1. Pré-requisitos

| Item | Versão | Observação |
| ---- | ------ | ---------- |
| .NET SDK | 10.0 (LTS) | `dotnet --list-sdks` DEVE listar um SDK 10.0.x; o `global.json` fixa essa versão (10.0.403, instalado em 2026-11-04 na máquina de desenvolvimento) |
| Docker Desktop (contêineres Linux) | atual | Necessário para o compose e para os testes de integração (Testcontainers) |
| Git | atual | |
| PowerShell | 7.x | Os exemplos usam `Invoke-RestMethod -Form`, que não existe no Windows PowerShell 5.1 |
| Chave da API OpenAI | — | Opcional: sem ela, use o provedor simulado (seção 3) |

Nenhum segredo é versionado. Chaves e senhas ficam no `.env` local (ignorado pelo Git) ou em
`dotnet user-secrets`. Não reutilize chaves de outros projetos.

## 2. Obter o código

```powershell
git clone https://github.com/<organizacao>/AgenteAuditoriaNFe.git
cd AgenteAuditoriaNFe
git switch 001-agente-auditoria-nfe
```

## 3. Configurar

Copie o modelo e preencha os valores locais:

```powershell
Copy-Item .env.example .env
```

| Variável | Exemplo | Uso |
| -------- | ------- | --- |
| `MONGO_INITDB_ROOT_USERNAME` / `MONGO_INITDB_ROOT_PASSWORD` | (local) | Obrigatórias no compose (`${VAR:?}`) |
| `MONGO_APP_USERNAME` / `MONGO_APP_PASSWORD` | (local) | Usuário do Api.Agente no banco `auditoria_agente`, criado pelo script de inicialização |
| `POSTGRES_USER` / `POSTGRES_PASSWORD` | (local) | Administrador do PostgreSQL do compose |
| `POSTGRES_APP_USER` / `POSTGRES_APP_PASSWORD` | (local) | Usuário do Api.Agente, com acesso só ao schema `consumo` |
| `Ia__Provedor` | `Simulado` ou `OpenAI` | `Simulado` só é aceito em `Development`; o parecer indica "modelo: simulado" |
| `OpenAI__ApiKey` | (sua chave) | Obrigatória só com `Ia__Provedor=OpenAI` |
| `OpenAI__Modelo` | (modelo aprovado pela avaliação, DP-05) | Sem valor padrão |
| `Mcp__Habilitado` | `false` | Integração MCP desabilitada nesta feature |
| `A2A__UrlPublica` | `http://localhost:5310` | URL base publicada no Agent Card |
| `A2A__IdAgente` | `agente-auditoria-nfe` | `idAgente` do card e do resultado; igual ao registry da SOFIA |
| `A2A__ChamadorPadrao` | `sofia` | `chamadorId` das solicitações A2A (D-05) |
| `Limites__TamanhoTotalBytes` / `Limites__QuantidadeArquivos` | `5242880` / `20` | FR-004, FR-005 |
| `ContextoUsuario__RetencaoInatividade` | `90.00:00:00` | FR-060 |

O compose define `ASPNETCORE_ENVIRONMENT=Development` e `Seguranca__AutenticacaoHabilitada=false`.
O Api.Agente recusa iniciar sem autenticação fora de `Development` (D-05).

## 4. Subir o ambiente

**Tudo em contêineres** (um comando — constitution 3.5):

```powershell
docker compose up -d --build
docker compose ps
```

| Serviço | Endereço no host | Observação |
| ------- | ---------------- | ---------- |
| `api-agente` | `http://localhost:5310` | Swagger em `/swagger` |
| `mongodb` | `localhost:27020` | Coleções e índices criados por `infra/mongodb/init` |
| `postgres` | `localhost:5436` | Schema `consumo`; tabela criada pelas migrações na inicialização da API |

Todas as portas são publicadas só em `127.0.0.1`. Os checkpoints de aprovação ficam no volume
`./data/aprovacoes`.

**Alternativa para desenvolvimento** (bancos em contêiner, API pelo SDK):

```powershell
docker compose up -d mongodb postgres
dotnet run --project src/Api.Agente
```

## 5. Verificar a saúde e a descoberta

```powershell
Invoke-RestMethod http://localhost:5310/health/live
Invoke-RestMethod http://localhost:5310/health/ready
Invoke-RestMethod http://localhost:5310/.well-known/agent-card.json
```

Esperado:

- `live` e `ready` saudáveis; `ready` falha se o MongoDB, o PostgreSQL ou o diretório de checkpoints
  estiverem indisponíveis, ou se algum XSD não conferir com o SHA-256 do manifesto.
- O Agent Card no formato A2A v0.3 (`contracts/sofia-a2a.md` §2): `name = AgenteAuditoriaNFe`,
  `url = http://localhost:5310/a2a/auditoria-nfe`, `protocolVersion = 0.3.0`, `preferredTransport =
  JSONRPC`, a habilidade `analisar_documentos` e a extensão `urn:infis:sofia:contrato` com
  `versaoContrato = 1.0`, `idAgente = agente-auditoria-nfe`, `nomeArtifact = analise-sofia-v1`, os
  limites (5 MB, 20 documentos) e `autenticacao = nenhuma — somente ambiente local de testes`. Nenhum
  segredo nem `securitySchemes` no card (TS-029). Com o cabeçalho `A2A-Version: 1.0`, o mesmo card sai
  no formato A2A 1.0.

## 6. Validar o núcleo pelo canal direto

Os exemplos usam XMLs sintéticos de `tests/AuditoriaNFe.Tests/Fixtures/Xml/`, um por período de
vigência dos pacotes de schema.

```powershell
$cabecalhos = @{ 'X-Usuario-Id' = 'usuario-demo-01'; 'X-Chamador-Id' = 'teste-local' }
$form = @{
  pergunta = 'Quais notas têm totalizador divergente?'
  arquivos = Get-Item tests/AuditoriaNFe.Tests/Fixtures/Xml/PL_010e_v1.02/*.xml
}
$envelope = Invoke-RestMethod -Method Post http://localhost:5310/api/v1/execucoes `
  -Headers $cabecalhos -Form $form
$envelope.parecer.documentos | Format-Table arquivo, dataReferencia, origemDataReferencia, pacoteAplicado
$envelope.parecer.achados | Format-Table categoria, severidade, regraId, arquivo
```

Esperado:

- Estado `Concluida`, as nove etapas registradas, etapa 5 "não aplicável — integração não
  habilitada" e a limitação "consulta a manuais oficiais não habilitada" (TS-046).
- Para cada documento: data de referência, origem (`dhRecbto` quando há protocolo, senão `dhEmi`) e
  pacote aplicado (FR-010d).
- Divergências de totalizador como `InconsistenciaNegocio`, com valor informado, valor esperado e
  `regraId` do catálogo (TS-004).
- Uma NFC-e (modelo 65) do diretório de fixtures sai como "não suportado" (ERR-009), sem validação de
  schema (TS-052).

Repita sem os cabeçalhos: o envelope traz `usuarioIdentificado = false`, a limitação de usuário não
identificado e nenhum contexto do usuário é gravado (TS-059).

## 7. Validar o contexto do usuário

```powershell
Invoke-RestMethod -Method Put http://localhost:5310/api/v1/contexto-usuario/preferencias `
  -Headers $cabecalhos -ContentType 'application/json' `
  -Body '{ "nivelDetalhe": "Detalhado", "tributosInteresse": ["ICMS", "IPI"] }'
Invoke-RestMethod http://localhost:5310/api/v1/contexto-usuario -Headers $cabecalhos
Invoke-RestMethod -Method Delete http://localhost:5310/api/v1/contexto-usuario -Headers $cabecalhos
```

Esperado: depois da análise da seção 6, o contexto lista os CNPJs auditados com contagem, os achados
recorrentes e a preferência declarada, cada item com a origem; nada de valores, nomes, CPF ou texto
livre (TS-057). Com outro `X-Usuario-Id` ou `X-Chamador-Id`, a consulta não encontra o contexto
(TS-054). Após a exclusão, a consulta responde 404 (TS-056).

## 8. Validar com o Sofia.Simulator

O simulador fala o contrato SOFIA v1.0 sobre A2A v0.3 (`contracts/sofia-a2a.md`): envia uma
`SolicitacaoAnaliseSofia` com nome fictício, uma pergunta e os XMLs sintéticos de
`src/Sofia.Simulator/Exemplos` lidos como texto, e lê o `ResultadoAnaliseSofia` do Artifact
`analise-sofia-v1`. O endereço do agente é a única configuração (SC-005).

### 8.1 Fluxo básico — TS-035 (obrigatório)

```powershell
dotnet run --project src/Sofia.Simulator -- --endereco http://localhost:5310 --cenario basico
```

Esperado no console, em ordem:

1. Agent Card obtido, com a habilidade `analisar_documentos`, o `idAgente`, os limites e a ausência
   de autenticação.
2. Solicitação enviada por `message/send`, com `idExecucao` e `idSolicitacao` novos.
3. Task no estado `completed`; `status` (`concluido` ou `parcial`), a `analise` em pt-BR, os
   `achados` com severidade, documentos e evidências, e as `observacoes` (validação determinística,
   pacote de schema, limitações e revisão humana).
4. Conferência dos IDs: `idExecucao` e `idSolicitacao` do resultado iguais aos enviados.

### 8.2 Agent Card inacessível — TS-035a (obrigatório)

```powershell
dotnet run --project src/Sofia.Simulator -- --endereco http://localhost:9999 --cenario basico
```

Esperado: mensagem de falha de descoberta e nenhuma solicitação enviada (código de saída diferente
de zero).

### 8.3 Documento não suportado — TS-065 (opcional)

```powershell
dotnet run --project src/Sofia.Simulator -- --endereco http://localhost:5310 --cenario documento-nao-suportado
```

Esperado: uma NF-e válida e um texto que não é NF-e na mesma solicitação; `status: parcial`, achados
da NF-e e uma observação com ERR-009 para o outro documento.

### 8.4 Contrato inválido — TS-066 (opcional)

```powershell
dotnet run --project src/Sofia.Simulator -- --endereco http://localhost:5310 --cenario contrato-invalido
```

Esperado: solicitação com `versaoContrato: "2.0"`; Task `completed` com `status: erro`,
`erro.codigo = ERR-007`, os IDs devolvidos e nenhuma execução criada.

### 8.5 Chamada manual (sem o simulador)

```powershell
$solicitacao = @{
  versaoContrato = '1.0'
  idExecucao     = [guid]::NewGuid().ToString()
  idSolicitacao  = [guid]::NewGuid().ToString()
  usuario        = @{ nome = 'Usuária Fictícia' }
  pergunta       = 'Verifique divergências fiscais nos documentos.'
  documentos     = @(@{
    idDocumento = 'doc-001'
    nome        = 'nfe-sintetica.xml'
    texto       = Get-Content -Raw src/Sofia.Simulator/Exemplos/<arquivo-valido>.xml
  })
} | ConvertTo-Json -Depth 5 -Compress

$envelope = @{
  jsonrpc = '2.0'; id = 'rpc-001'; method = 'message/send'
  params  = @{ message = @{
    kind = 'message'; role = 'user'; messageId = [guid]::NewGuid().ToString()
    parts = @(@{ kind = 'text'; text = $solicitacao })
  } }
} | ConvertTo-Json -Depth 8 -Compress

$resposta = Invoke-RestMethod -Method Post http://localhost:5310/a2a/auditoria-nfe `
  -ContentType 'application/json; charset=utf-8' -Body ([Text.Encoding]::UTF8.GetBytes($envelope))
$resposta.result.status.state
($resposta.result.artifacts | Where-Object name -eq 'analise-sofia-v1').parts[0].text | ConvertFrom-Json
```

Esperado: `completed` e um `ResultadoAnaliseSofia` que valida em
`contracts/sofia-resultado-analise-v1.schema.json`. O nome do usuário aparece na sessão e na
execução gravadas no MongoDB, mas não nos logs da API (TS-067).

## 9. Validar a aprovação de recomendações sensíveis (TS-039 a TS-041)

1. Execute pelo canal direto (seção 6) um lote que gere recomendação sensível. Esperado: o parecer
   chega na hora, com a recomendação "pendente de aprovação" e sem conteúdo. Pelo A2A, o
   `ResultadoAnaliseSofia` só traz a observação "pendente de aprovação", nunca o conteúdo.
2. Liste as pendências e decida, informando o aprovador nos cabeçalhos (sem verificação na v1):

   ```powershell
   $aprovador = @{ 'X-Usuario-Id' = 'aprovador-01'; 'X-Chamador-Id' = 'sofia-simulador' }
   Invoke-RestMethod http://localhost:5310/api/v1/recomendacoes/pendentes -Headers $aprovador
   Invoke-RestMethod -Method Post http://localhost:5310/api/v1/recomendacoes/<id>/aprovacao -Headers $aprovador
   Invoke-RestMethod -Method Post http://localhost:5310/api/v1/recomendacoes/<id>/rejeicao -Headers $aprovador `
     -ContentType 'application/json' -Body '{ "justificativa": "Motivo da rejeição" }'
   ```

3. Consulte a execução pelo canal direto (o contrato SOFIA v1.0 não tem consulta de recomendações):

   ```powershell
   Invoke-RestMethod http://localhost:5310/api/v1/execucoes/<execucaoId> -Headers $cabecalhos
   ```

Esperado:

- Aprovada: o texto da recomendação é entregue. Rejeitada: o estado é entregue, nunca o texto.
- O arquivo `./data/aprovacoes/registros/<id>.json` contém os campos do FR-035f, com
  `aprovadorVerificado = false`, e nenhum conteúdo de XML nem texto da recomendação.
- Segunda decisão sobre a mesma recomendação: ERR-020. Decisão sem `X-Usuario-Id`: ERR-007.

## 10. Executar os testes

```powershell
dotnet test --project tests/AuditoriaNFe.Tests --filter-not-trait "Categoria=Avaliacao"
```

- Unidade (seleção de pacote, regras do catálogo, cálculos), aplicação, ferramentas, aprovação,
  contrato (A2A, OpenAPI, contrato do LLM), arquitetura e integração (MongoDB e PostgreSQL reais via
  Testcontainers; requer Docker).
- Os testes usam `IChatClient` simulado; não há chamadas pagas nem chave da OpenAI.

Conjunto de avaliação (SC-003, SC-004, SC-008), sob demanda e com OpenAI real:

```powershell
dotnet test --project tests/AuditoriaNFe.Tests --filter-trait "Categoria=Avaliacao"
```

Esperado: 0 respostas com valor financeiro não rastreável; resultado registrado conforme o P13 e
o P17.

## 11. Problemas comuns

| Sintoma | Causa provável | Ação |
| ------- | -------------- | ---- |
| `global.json` não encontra o SDK | SDK .NET 10 não instalado | Instalar o SDK 10.0.x |
| `docker compose up` falha com "Defina ... no arquivo .env" | Variável obrigatória ausente | Preencher o `.env` a partir do `.env.example` |
| Porta em uso | Outro projeto usando 5310, 27020 ou 5436 | `docker compose ps` nos outros projetos; ajustar a porta no compose e no `.env` |
| API não inicia: "autenticação desabilitada fora de Development" | `ASPNETCORE_ENVIRONMENT` diferente de `Development` | Usar o compose ou o perfil de desenvolvimento (D-05) |
| API não inicia: hash de schema divergente | XSD alterado ou convertido no fim de linha | Restaurar o arquivo do Git (`.gitattributes` marca os XSDs como `-text`) |
| `ready` não saudável | MongoDB ou PostgreSQL sem subir | `docker compose ps` e `docker compose logs` |
| Resultado "Inconclusiva" com ERR-014 | Chave ou modelo OpenAI inválido, ou provedor indisponível | Conferir `OpenAI__ApiKey` e `OpenAI__Modelo` |
| Recomendações somem após reiniciar | Volume de checkpoints não montado | Conferir `./data/aprovacoes` no compose |

## 12. Encerrar

```powershell
docker compose down        # mantém volumes
docker compose down -v     # remove volumes (dados de demonstração)
```
