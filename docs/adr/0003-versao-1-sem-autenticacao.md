# ADR-0003 — Versão 1 sem autenticação e modelo futuro de credencial

- **Situação**: Aprovado
- **Data**: 2026-11-04
- **Aprovação**: Aprovado por Andressa Firmino, 2026-11-04
- **Relacionado a**: desvio D-05; DP-10 (adiada); constitution 5.2, P18 e Definition of Done; FR-052,
  FR-052a a FR-052c, FR-035h, ERR-004, ERR-019, TS-031, TS-042

## Contexto

A versão 1 serve para testar a validação dos XMLs. A solicitante decidiu adiar a autenticação para
implementação futura. A constitution 5.2 exige credencial do chamador em endpoints não públicos;
por isso a ausência de autenticação é o desvio temporário D-05, aprovado no plano da feature 001.

## Decisão

- **Sem autenticação na v1**: o chamador informa `chamadorId` e `usuarioId` (metadados A2A ou
  cabeçalhos `X-Chamador-Id` e `X-Usuario-Id`). Formato validado: até 128 caracteres de
  `[A-Za-z0-9._:@-]`. Sem eles, "nao-informado" e "anonimo".
- **Mitigação obrigatória**:
  1. o `Api.Agente` recusa iniciar fora de `Development` enquanto
     `Seguranca:AutenticacaoHabilitada = false`;
  2. o compose publica as portas só em `127.0.0.1`;
  3. o Agent Card declara "nenhuma — somente ambiente local de testes" e não tem `securitySchemes`;
  4. dados de teste sintéticos ou anonimizados;
  5. o aprovador de recomendações é registrado como não verificado.
- **Modelo futuro (DP-10)**: credencial de serviço do chamador, rotacionável e fornecida por
  configuração segura, com o usuário informado pelo chamador autenticado; controle do papel de
  aprovador (FR-035h, ERR-019); mesma autenticação nos canais REST e A2A (P18).

## Consequências

- **Riscos aceitos**: personificação de usuário, leitura do contexto de outro usuário por quem
  souber o identificador e aprovação indevida de recomendações sensíveis.
- Implantação fora da máquina local e integração com a SOFIA real ficam proibidas até o fim do D-05.
- O fim do D-05 exige: novo ADR com o mecanismo concreto (DP-10), TS-031 e TS-042 ativos e
  isolamento combinando o usuário com o chamador autenticado.

## Alternativas consideradas

- JWT com `dotnet user-jwts` já na v1: rejeitado pela solicitante; seria uma autenticação
  provisória sem garantia em produção.
- Credencial de serviço já na v1: exigiria decidir a DP-10 antes de validar o núcleo.

## Referências

- `specs/001-agente-auditoria-nfe/research.md` — R-17
- `specs/001-agente-auditoria-nfe/plan.md` — Complexity Tracking, D-05
