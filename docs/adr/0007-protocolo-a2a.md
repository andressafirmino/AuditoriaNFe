# ADR-0007 — Protocolo A2A, Agent Card e contrato provisório

- **Situação**: Aprovado
- **Data**: 2026-11-04
- **Aprovação**: Aprovado por Andressa Firmino, 2026-11-04
- **Relacionado a**: DP-17, DP-27 (pendente); desvio D-04; P18; FR-043 a FR-048

## Contexto

O agente é exposto à SOFIA por A2A com o SDK oficial (FR-043). A SOFIA ainda não chama agentes
externos, e o contrato definitivo depende de acordo com o responsável por ela (DP-27). O hosting
A2A do MAF na versão fixada não suporta respostas em segundo plano.

## Decisão

- **Protocolo**: A2A 1.0 pelo SDK `A2A` 1.0.0-preview3, via `Microsoft.Agents.AI.Hosting.A2A.AspNetCore`.
- **Servidor**: `AddA2AServer("auditoria-nfe", AgentRunMode.DisallowBackground)`, com
  `MapA2AHttpJson` (preferencial) e `MapA2AJsonRpc` em `/a2a/auditoria-nfe`; `AgentSessionStore`
  MongoDB.
- **Agent Card**: `/.well-known/agent-card.json`, com habilidades, limites, versão e situação do
  contrato ("provisorio"), sem `securitySchemes` e com `autenticacao` declarada (ADR-0003).
- **Contrato**: `contracts/sofia-a2a.md` é uma proposta v1 provisória e versionada. Mudanças
  acordadas com a SOFIA afetam só o adaptador A2A e o card; o caso de uso é o mesmo do canal REST.
- **Sem tarefas A2A na v1 (D-04)**: todas as respostas são mensagens imediatas; a SOFIA vê só o
  estado final.
- **Ordem**: o canal A2A e o `Sofia.Simulator` são implementados depois do núcleo REST.

## Consequências

- Os pacotes A2A estão em preview: versões fixadas e testes de contrato a cada atualização.
- As lições V-06, V-09 e V-10 do AgenteFinanceiro DEVEM ser cobertas por testes antes de qualquer
  uso de tarefas A2A.
- O D-04 é reavaliado a cada nova versão do hosting A2A.

## Alternativas consideradas

- Implementar o protocolo ou as tarefas A2A manualmente: contraria o FR-043.
- API obsoleta `MapA2A(agent, path, agentCard)`.

## Referências

- `specs/001-agente-auditoria-nfe/research.md` — R-11, R-12, R-32, R-33
- `specs/001-agente-auditoria-nfe/contracts/sofia-a2a.md`
