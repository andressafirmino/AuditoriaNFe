# Tabela de vigência dos pacotes de schema da NF-e (leiaute 4.00)

**Feature**: `001-agente-auditoria-nfe` | **Spec**: [spec.md](spec.md) (FR-010 a FR-010f)
**Situação**: Aprovada em 2026-11-04 pelo solicitante, sujeita a ajustes futuros
**Levantada em**: 2026-11-04 | **Data de corte ("hoje")**: 2026-11-04

## Método

- Fontes: as notas técnicas inventariadas em `docs/notas-tecnicas/inventario.csv` (última versão de
  cada NT) e os títulos dos pacotes no Portal Nacional da NF-e, inventariados em
  `docs/schemas-nfe-v4/inventario.csv`.
- A data de início de cada pacote é a data de entrada em **produção** do schema para a NF-e modelo
  55, na versão mais recente da NT. Quando a NT diz "até dd/mm/aaaa", essa data é usada como início.
- Cada pacote vale do seu início até o dia anterior ao início do pacote seguinte; o último vale sem
  data de fim.
- De cada pacote (letra) é usada a última revisão publicada.
- Pacotes que nunca estiveram sozinhos em produção ficam fora da tabela (ver "Pacotes excluídos").
  Isso acontece quando a produção foi adiada até a entrada de um pacote posterior que já os contém,
  ou quando dois pacotes entram no mesmo dia.
- A inclusão cumulativa dos pacotes foi conferida nos XSDs, buscando um campo característico de cada
  NT em `leiauteNFe_v4.00.xsd`, `tiposBasico_v4.00.xsd` e `DFeTiposBasicos_v1.00.xsd`.
- A data de referência do documento é comparada pelo dia, no fuso informado no próprio campo
  (`dhRecbto` ou `dhEmi`).

## Tabela de vigência

| # | Pacote (arquivo) | Notas técnicas | Início | Fim | Fonte da data de início |
|---|---|---|---|---|---|
| 1 | `PL_009_V4_v1_40.zip` | NT 2016.002 v1.40 | 2017-12-04 ⚠️ | 2018-01-01 | NT 2016.002 v1.61, p. 13: "Ambiente de Produção: 04/12/2017" (bloco sem versão; ver P1) |
| 2 | `PL_009_V4_2017_001.zip` | NT 2017.001 v1.00 (GTIN) | 2018-01-02 | 2018-06-03 | NT 2017.001 v1.50, p. 5: "Implantações desta NT previstas somente para a versão 4.00 da NF-e e NFC-e 04/12/2017 02/01/2018" |
| 3 | `PL_009_V4_NT_2016_002_v1_50.zip` | NT 2016.002 v1.50/v1.51 | 2018-06-04 | 2018-07-22 | NT 2016.002 v1.61, p. 13: "define novos prazos de implantação da versão 1.50 … Ambiente de Produção:04/06/2018" |
| 4 | `PL_009_V4_NT_2016_002_v1_60b.zip` | NT 2016.002 v1.60 | 2018-07-23 | 2019-05-06 | NT 2016.002 v1.61, p. 13: "Ambiente de Produção: 23/07/2018" |
| 5 | `PL_009_V4_00_NT_2018_005_v1.20a.zip` | NT 2018.005 v1.20 | 2019-05-07 | 2019-09-01 | NT 2018.005 v1.52, p. 4: "Alteração da Versão do schema para versão 1.20 Até 25/02/2019 Até 07/05/2019" |
| 6 | `PL_009_V4_00_NT_2019_001_v1.20a.zip` | NT 2019.001 v1.20 | 2019-09-02 | 2021-04-04 | NT 2019.001 v1.70, p. 5: "Criado novo Valor para o Campo N18 Até 26/08/2019 02/09/2019" |
| 7 | `PL_009c_V4_00_NT_2020_006_v1.20.zip` | NT 2020.006 v1.00 | 2021-04-05 | 2021-08-31 | NT 2020.006 v1.31, p. 5: "Ambiente de Produção: 05/04/2021"; Portal: "Válido de 05/04/2021 a 31/08/2021" |
| 8 | `PL_009_V4_00_NT_2020_006_v1.20.zip` | NT 2020.006 v1.20 | 2021-09-01 | 2021-10-03 | NT 2020.006 v1.31, p. 5: "Ambiente de Produção: 01/09/2021"; Portal: "Válido a partir de 01/09/2021" (ver P3) |
| 9 | `PL_009h_V4_00_NT_2020_005_v120.zip` | NT 2020.005 v1.20, NT 2020.006, NT 2021.002, lista de serviços | 2021-10-04 ⚠️ | 2022-09-11 | NT 2020.005 v1.21, p. 4: "Publicação pacote PL_009g_V4_00_NT_2020_005_v1.20 02/08/2021 04/10/2021"; o 009h foi publicado em 04/10/2021 (ver P2) |
| 10 | `PL_009i_NT2021_004_v100d.zip` | NT 2021.004 | 2022-09-12 | 2023-03-29 | NT 2021.004 v1.35, p. 6: "o prazo de entrada em produção de TODA a NT, incluindo as alterações desta versão, fica para 12/09/2022" |
| 11 | `PL_009k_NT2023_001_v120.zip` | NT 2023.001 (contém a NT 2022.003) | 2023-03-30 | 2024-05-05 | NT 2023.001 v1.60, p. 8: "O prazo previsto para a implementação das mudanças nos schemas […] até 03/03/2023 […] 30/03/2023" |
| 12 | `PL_009n_NT2023_004_v101_e_NT2019_001_v162.zip` | NT 2023.004 v1.11, NT 2019.001 v1.62 | 2024-05-06 | 2024-09-15 | NT 2023.004 v1.20, p. 3: "1.11 Atualização do Schema Até 25/03/2024 06/05/2024"; NT 2019.001 v1.70, p. 11: "o schema entra em produção em 06/05/2024." |
| 13 | `PL_009o_NT2024_001_v100.zip` | NT 2024.001 | 2024-09-16 | 2025-08-31 | NT 2024.001 v1.20, p. 3: "1.20 Publicação em produção com campos e regras que não afetam o MEI - 16/09/2024" |
| 14 | `PL_009q_NT2025_001_v1.00.zip` | NT 2025.001 (altera só a NFC-e) | 2025-09-01 | 2025-11-09 | NT 2025.001 v1.03, p. 2: "NFC-e: Leiaute do QR-Code versão 3 e outras mudanças Até 02/06/2025 Até 01/09/2025" |
| 15 | `PL_010b_NT2025_002_v1.30.zip` | NT 2025.002 v1.30, NT 2024.003, NT 2025.001 | 2025-11-10 | 2026-04-05 | NT 2025.002 v1.53, p. 5: "Entrada do schema, das Regras de Validação e dos Eventos […] Até 29/10/2025 10/11/2025"; NT 2024.003 v1.10, p. 4: "Datas de entrada em produção do Schema XML. […] 10/11/2025" |
| 16 | `PL_010c_NT2022_002v1.30.zip` | NT 2022.002 v1.30 | 2026-04-06 | 2026-06-30 | NT 2022.002 v1.30a, p. 3: "alteração de Leiaute para atender o AJ 40/25 e outras alterações Até 30/03/2026 Até 06/04/2026" |
| 17 | `PL_010d_v1.03.zip` | NT 2026.004 v1.01 (CNPJ alfanumérico) | 2026-07-01 | 2026-08-02 | NT 2026.004 v1.01, p. 3: "Alteração da data de implantação em homologação […] Até 15/06/2026 01/07/2026" |
| 18 | `PL_010e_v1.02.zip` | NT 2025.002 v1.36/v1.40, NT 2026.002, NT 2026.003 | 2026-08-03 | 2026-11-15 | NT 2025.002 v1.53, p. 5: "Criação do campo para o Código Indicador do Local da Operação […] Até 01/07/2026 03/08/2026"; NT 2026.002 v1.11, p. 3: "Atualização de domínio em campos da NF-e/NFC-e (schema) […] 01/07/2026 03/08/2026" |
| 19 | `PL_010g_v1.03.zip` | NT 2025.002 v1.50, NT 2026.007 (ambas pelo 010f), NT 2026.006, NT 2026.008 | 2026-11-16 (futuro) | aberto | NT 2026.006 v1.01, p. 3: "Alteração das datas de implantação em Homologação e Produção Até 26/10/2026 16/11/2026"; mesma data na NT 2026.007 v1.11, p. 3, na NT 2026.008 v1.01, p. 3, e na NT 2025.002 v1.53, p. 6 |

⚠️ Data inferida, pendente de confirmação (ver "Pendências").

Na data de corte, o pacote vigente é o **010e** (linha 18). O 010g passa a valer automaticamente para
documentos com data de referência a partir de 2026-11-16. Documentos com data de referência anterior
a 2017-12-04 recebem ERR-023.

## Convivência entre pacotes

Nenhuma nota técnica do período do leiaute 4.00 prevê convivência entre pacotes de schema; todas as
NTs lidas trazem apenas a data de entrada em produção. A única convivência mencionada é a do leiaute
3.10 com o 4.00 (NT 2016.002 v1.61, p. 13: "Desativação da versão anterior: 02/08/18"), que está fora
do escopo, porque documentos 3.10 não são suportados. As janelas do Portal para os pacotes das
linhas 7 e 8 são consecutivas, sem sobreposição.

Por isso, a coluna de fim de convivência (FR-010e) fica vazia em todas as linhas, e a validação
contra o pacote anterior (FR-010f) não é acionada com esta tabela. A regra continua na especificação
para o caso de uma NT futura prever convivência.

## Pacotes excluídos

Pacotes inventariados que nunca estiveram sozinhos em produção:

| Pacote | Motivo |
|---|---|
| `PL_009.zip`, `PL_009_V4_NT_v1_20.zip`, `PL_009_V4_V1.30.zip` | Revisões da NT 2016.002 publicadas antes da entrada do leiaute 4.00 em produção, que foi adiada até a v1.40 |
| `PL_009_V4_NT_2016_002_v1_60.zip` | Substituído pela revisão `v1_60b` |
| `PL_009_V4_00_NT_2018_005_v100.zip`, `PL_009_V4_00_NT_2018_005_v1.10.zip` | Substituídos pela revisão v1.20a antes da produção |
| `PL_009_V4_00_NT_2020_006_v1.00.zip` | Substituído pelo 009c, que o Portal declara válido desde 05/04/2021 |
| `PL_009_V4_00_NT_2020_005_v1.10.zip` | A produção da NT 2020.005 foi adiada para 04/10/2021, data do 009g/009h |
| `PL_009g_V4_00_NT_2020_005_v120.zip` | Mesma data de início do 009h, que o contém (ver P2) |
| `PL_009i_NT2021_004_v100a/b/c.zip` | Substituídos pela revisão v1.00d |
| `PL_009j_NT2022_003_v100a/b.zip` | O 009k, que contém o campo `refNFeSig` da NT 2022.003, entrou em produção em 30/03/2023, antes da data do 009j (03/04/2023) |
| `PL_009k_NT2023_001_v100/v110a/v110b.zip` | Substituídos pela revisão v1.20 |
| `PL_009m_NT2023_004_v101_e_NT2019_001_v160.zip` | A produção foi adiada para 06/05/2024, mesma data do 009n |
| `PL_009p1_NT2024_003_v1.02.zip`, `PL_009p_NT2024_003_v1.03.zip` | Linha paralela com o grupo agropecuário (NT 2024.003), cujo schema só entrou em produção em 10/11/2025, dentro do 010b. O 009q e o 010a não contêm esse grupo |
| `PL_010a_NT2025_002_v1.01.zip` | A produção do schema da NT 2025.002 foi adiada para 10/11/2025, data do 010b |
| `PL_010b_NT2025_002_v1.00/v1.20/v1.21.zip` | Substituídos pela revisão v1.30 |
| `PL_010d_v1.02.zip`, `PL_010e_v1.01.zip` | Substituídos pelas revisões v1.03 e v1.02 |
| `PL_010f_v1.04.zip` | Entra em produção no mesmo dia do 010g (16/11/2026), e o 010g contém todas as alterações do 010f (conferido por comparação dos XSDs) |

## Pendências para aprovação

- **P1 — Início do leiaute 4.00 (linha 1)**: a NT 2016.002 v1.61 traz "Ambiente de Produção:
  04/12/2017" num bloco sem versão (p. 13). A atribuição à v1.40 é inferida, porque a v1.41 (p. 7)
  alterou as datas da v1.40 sem citar os valores.
- **P2 — Início do 009h (linha 9)**: o pacote acrescenta um código à lista de serviços e não tem NT
  com data. Foi publicado em 04/10/2021, mesma data de produção do 009g; a data de início é inferida.
- **P3 — Pacote 009f**: o Portal informa que o pacote da linha 8 foi "Substituido pelo
  PL_009f_V4_00_NT_2020_005_v100.zip", mas esse pacote não é oferecido para download na página de
  schemas do Portal (conferido em 2026-11-04). Fica mantido o pacote da linha 8, que é o oficial
  disponível; se o 009f for obtido de fonte oficial, ele DEVE ser inventariado e avaliado.
- **P4 — Datas "até"**: nas linhas 5, 14 e 16 a NT dá um prazo máximo ("até"); a SEFAZ pode ter
  ativado o schema antes. A data do prazo é usada como início.
- **P5 — Nota Fiscal Fácil (NT 2021.002)**: os grupos da NFF entraram em produção em 25/05/2021
  (NT 2021.002 v1.12, p. 3), mas só aparecem no 009g. Documentos emitidos pelo aplicativo da NFF
  entre 25/05/2021 e 03/10/2021 podem acusar erro estrutural contra os pacotes das linhas 7 e 8.
  O impacto esperado é baixo, porque esses documentos são gerados pelo aplicativo da SEFAZ.

## Regras de totais encontradas nas NTs

Regras do Grupo W (totais) e correlatas encontradas nas NTs, com a vigência em produção. O catálogo
aprovado, com fórmulas e severidades, está em
[catalogo-regras-totalizadores.md](catalogo-regras-totalizadores.md).

| Regra | Alteração | Produção | Fonte |
|---|---|---|---|
| W03-20 | Limite da base de cálculo total por modelo (facultativa) | 2019-09-02 | NT 2019.001 v1.70, p. 22 e pp. 5–6 |
| W16-10 | Soma vPIS/vCOFINS ST quando indSomaPISST/indSomaCOFINSST = 1 | 2021-10-04 | NT 2020.005 v1.21, p. 20 |
| W16-10 | Soma vICMSMonoReten | 2023-03-30 | NT 2023.001 v1.60, p. 11: "Estas alterações entram no ar […] 30/03/2023 em produção." |
| W06c-10, W06d-10, W06e-10, W06b.1-10, W06c.1-10, W06d.1-10 | Totais do ICMS monofásico (tolerância de R$ 0,01 desde a v1.30) | 2023-10-30 | NT 2023.001 v1.60, pp. 39–40: "Regra implantada até 25/09/2023 em homologação e em 30/10/2023 em produção." |
| W16-10 | Exceção 4 (indDeduzDeson) e ajustes das exceções 1 e 4 | 2024-07-01 | NT 2023.004 v1.20, pp. 5–6 e 15–16 |
| W59f-10, W59g-10 | Totais de IBS e CBS estornados | 2026-02-02 | NT 2025.002 v1.53, p. 5 |
| W16-30 a W16-60 | Valor máximo da NF-e com DANFE Simplificado Tipo 2 e ajustes | 2026-08-03 (ajustes de W16-40 a W16-60 em 2026-10-05) | NT 2026.002 v1.11, p. 3 e pp. 18–19 |
| W07-10 | Alterada na NT 2025.002 v1.40 | 2026-08-03 | NT 2025.002 v1.53, p. 6 |
| 3BA02-50, 3BA02-54, 3BA02-58, 3BA02-64, 3BA02-68 | Devolução: vNF (W16), vICMS (W04), W04h, W04e, W04c | 2026-11-16 | NT 2025.002 v1.53, p. 6 |
| W01a-10, W01a-20 | Total do valor líquido (vProdLiqTot) | 2026-11-16 | NT 2026.008 v1.01, pp. 3 e 8 |
| W33-10, W60-05, W60-10, VB01-05, VB01-10, VB01-20 | Total do IS, vNFTot e vItem | Implementação futura (sem data) | NT 2025.002 v1.53, pp. 68–72; NT 2026.008 v1.01, pp. 3, 7 e 8 |
| W31-10 a W59d-10 | Totais de IBS/CBS | Sem data própria; seguem o schema do 010b (2025-11-10) | NT 2025.002 v1.53 |
| YA03-10 (soma de vPag × vNF) | Modelo 55 | Implementação futura | NT 2025.001 v1.03, p. 10 |

## Aprovação

| Papel | Nome | Data | Decisão |
|---|---|---|---|
| Solicitante | Solicitante da feature | 2026-11-04 | Aprovada, incluindo as datas inferidas (P1, P2) e os prazos "até" (P4); sujeita a ajustes futuros |
