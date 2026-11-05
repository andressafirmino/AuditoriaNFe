# ADR-0010 — Schemas XSD oficiais versionados e seleção pela data de referência

- **Situação**: Aprovado
- **Data**: 2026-11-04
- **Aprovação**: Aprovado por Andressa Firmino, 2026-11-04
- **Relacionado a**: DP-03 (resolvida para a feature 001); P02, P03; FR-010 a FR-010f, FR-013a,
  ERR-023, NFR-005

## Contexto

O leiaute 4.00 da NF-e teve 19 pacotes de schema oficiais. Validar um documento antigo contra o
pacote atual reprova campos criados depois; validar contra um pacote baixado em tempo de execução
torna o resultado não reprodutível.

## Decisão

- **Pacotes no repositório**: os 19 pacotes oficiais do Portal Nacional da NF-e em
  `src/AuditoriaNFe.Infrastructure/Schemas/NFe/<pacote>/`, com `manifesto.json` (ordem, notas
  técnicas, vigência, `fimConvivencia`, origem com URL e SHA-256 do zip, SHA-256 de cada XSD).
  `.gitattributes` marca os XSDs como `-text`.
- **Integridade**: o SHA-256 de cada XSD é conferido na inicialização; divergência impede a subida.
- **Resolução segura**: includes resolvidos só dentro da pasta do pacote; documentos lidos com
  `DtdProcessing.Prohibit` e `XmlResolver = null`.
- **Seleção**: data de referência = data de calendário de `dhRecbto` (com protocolo) ou `dhEmi`, no
  fuso do próprio XML; pacote com `vigenciaInicio ≤ data ≤ vigenciaFim`; o último pacote vale até a
  data atual. Fora da tabela → ERR-023.
- **Convivência**: regra genérica pelo `fimConvivencia` (vazio em todos os pacotes atuais), testada
  com tabela sintética.
- **Regras de conteúdo** usam a mesma data de referência (FR-013a).
- **Atualizações**: ajuste aprovado na tabela de vigência gera nova versão do manifesto no mesmo PR,
  com os testes de período.

## Consequências

- O repositório carrega cerca de 100 arquivos XSD.
- Cada novo pacote oficial exige PR com XSDs, manifesto, tabela de vigência e XML sintético do
  período.
- O `XmlSchemaSet` de cada pacote é compilado uma vez e mantido em cache; a validação concorrente
  sobre o mesmo conjunto DEVE ser confirmada por teste de carga.

## Alternativas consideradas

- Baixar schemas em tempo de execução: dependência externa e sem reprodutibilidade.
- NFePHP ou ACBr como fonte: não são fontes oficiais (P02).
- Um único pacote para todas as datas: reprova documentos antigos.

## Referências

- `specs/001-agente-auditoria-nfe/research.md` — R-14
- `specs/001-agente-auditoria-nfe/vigencia-pacotes-schema.md`
- `src/AuditoriaNFe.Infrastructure/Schemas/NFe/manifesto.json`
