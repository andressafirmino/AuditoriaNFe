# ADR-0005 — Retenção de sessões, contexto do usuário, consumo e checkpoints

- **Situação**: Aprovado
- **Data**: 2026-11-04
- **Aprovação**: Aprovado por Andressa Firmino, 2026-11-04
- **Relacionado a**: DP-13 (parcial); LGPD; FR-035i, FR-037a, FR-060

## Contexto

A DP-13 pede regras de retenção e exclusão de memória, arquivos, logs e registros de consumo. A spec
da feature 001 já fixa parte dos prazos.

## Decisão

| Dado | Regra |
| ---- | ----- |
| XML recebido | Descartado ao fim da execução; nunca persistido (FR-037a) |
| Sessão | Indisponível após 24 h de inatividade (configurável) ou quando encerrada |
| Contexto do usuário | Excluído após 90 dias sem atividade (FR-060, configurável), por índice TTL em `expiraEm`; a leitura ignora contextos vencidos |
| Registros de consumo | Mantidos sem conteúdo de XML, pergunta ou resposta |
| Checkpoints JSON de aprovação | Removidos junto com a sessão; pendências de sessão expirada ficam registradas como indisponíveis (FR-035i) |
| Logs | Sem conteúdo sensível, por política de observabilidade (ADR-0009) |

## Pendência aceita

Os prazos de **remoção física de sessões expiradas** e de **retenção dos registros de consumo** não
foram definidos. Enquanto não houver valor aprovado (parâmetros `Retencao:*`), não há remoção física
automática além do TTL do contexto do usuário. Isso não bloqueia a v1 local (D-05), mas DEVE ser
resolvido por novo ADR antes de qualquer uso com dados reais.

## Consequências

- Exclusão a pedido do usuário (FR-059) remove o contexto na hora, independentemente do TTL.
- O monitor de TTL do MongoDB remove o documento em até cerca de um minuto após o vencimento.

## Alternativas consideradas

- Fixar já os prazos pendentes: sem requisito ou orientação que os fundamente; ficariam arbitrários.

## Referências

- `specs/001-agente-auditoria-nfe/research.md` — R-19
- `specs/001-agente-auditoria-nfe/spec.md` — FR-037a, FR-060
