# Catálogo de regras de conteúdo — totalizadores da NF-e (versão 1)

**Feature**: `001-agente-auditoria-nfe` | **Spec**: [spec.md](spec.md) (FR-012 a FR-014, FR-023)
**Situação**: Aprovado em 2026-11-04 pelo solicitante (resolve DP-04 para esta feature)
**Levantado em**: 2026-11-04

## Fontes

| Documento | Versão | Uso |
|---|---|---|
| MOC 7.0 — Anexo I, Leiaute e Regras de Validação da NF-e e da NFC-e | Capa: 7.00, novembro de 2020; controle de versões: 7.03, outubro de 2020 (p. 5) | Texto-base das regras do grupo W (seção 4.2.1, pp. 120–123), dos grupos Y e YA (pp. 125–126) e tolerância (*3) (p. 140) |
| NT 2016.002 | v1.61 | Regras W de FCP e IPI devolvido (pp. 62–63); vigência das regras do grupo Y (pp. 13 e 64–65) |
| NT 2020.005 | v1.21 | W16-10 com PIS-ST e COFINS-ST (p. 20) |
| NT 2023.001 | v1.60 | W16-10 com ICMS monofásico; regras W06b.1 a W06e (pp. 39–41) |
| NT 2023.004 | v1.20 | Texto vigente da W16-10 (pp. 15–16) |
| NT 2025.002 | v1.53 | W07-10; totais de IBS, CBS e IS (pp. 68–72) |
| NT 2026.008 | v1.01 | W01a-10 e W01a-20 (p. 8) |

As versões e os hashes dos arquivos estão em `docs/notas-tecnicas/inventario.csv`.

## Convenções de aplicação

- O catálogo vale só para a NF-e modelo 55. Regras exclusivas da NFC-e ficam fora.
- "Σ itens" é a soma do campo indicado em todos os itens (`det`) do documento. Campo opcional não
  informado vale zero.
- Os valores são comparados em decimal exato, sem arredondamento, salvo tolerância indicada na regra.
- A regra só é aplicada a documentos cuja data de referência (FR-010a) esteja dentro da vigência da
  versão da regra (FR-013a). Antes do início, a regra não é aplicada; não gera achado.
- As regras de IBS, CBS e IS só são aplicadas quando os grupos desses tributos estiverem preenchidos
  no documento (NT 2025.002 v1.53, cronograma).
- A coluna "SEFAZ" informa como a regra é aplicada na autorização: "Obrig." (todas as UFs rejeitam)
  ou "Facult." (cada UF decide). Uma NF-e autorizada pode conter divergência em regra facultativa,
  e é aí que o agente agrega mais valor.
- A severidade é a do achado no parecer (FR-023): Alta, Média, Baixa ou Informativa.

## Critério de severidade

| Severidade | Critério |
|---|---|
| Alta | Divergência no valor total da nota ou no valor de um tributo (ICMS, ICMS-ST, FCP, IPI, ICMS desonerado, DIFAL, ICMS monofásico, ISS, IBS, CBS) |
| Média | Divergência em base de cálculo, em componente do valor da nota (produtos, frete, seguro, desconto, outras despesas, II) ou em PIS e COFINS; grupo de total informado ou ausente indevidamente; divergência nos totais de cobrança ou de pagamento |
| Baixa | Divergência em quantidade tributada (ICMS monofásico) ou em total de retenção/dedução declaratória do ISSQN |
| Informativa | Valor aproximado de tributos (Lei da Transparência), sem efeito fiscal |

## Regras da versão 1

### ICMSTot (grupo W02)

| ID | Total (tag, id) | Regra: o total deve ser igual a | Condições e exceções | Tolerância | SEFAZ (rejeição) | Vigência | Fonte | Severidade |
|---|---|---|---|---|---|---|---|---|
| W03-10 | vBC, W03 | Σ itens vBC (N15) | — | — | Facult. (531) | desde 2017-12-04 | Anexo I, p. 120 | Média |
| W04-10 | vICMS, W04 | Σ itens vICMS (N17) | Não somar itens com CST 40, 41 e 50 | — | Facult. (532) | desde 2017-12-04 | Anexo I, p. 120 | Alta |
| W04a-10 | vICMSDeson, W04a | Σ itens vICMSDeson (N28a) | — | — | Facult. (795) | desde 2017-12-04 | NT 2016.002 v1.61, p. 62; Anexo I, p. 120 | Alta |
| W04b-10 | vFCP, W04h | Σ itens vFCP (N17c) | A regra cita o id "W04b", mas no leiaute o vFCP é W04h; o catálogo usa a tag | — | Obrig. (861) | desde 2017-12-04 | NT 2016.002 v1.61, p. 62; Anexo I, p. 120 | Alta |
| W04c-10 | vFCPUFDest, W04c | Σ itens vFCPUFDest (NA13) | — | — | Obrig. (798) | desde 2017-12-04 | Anexo I, p. 120 | Alta |
| W04e-10 | vICMSUFDest, W04e | Σ itens vICMSUFDest (NA15) | Nulo vale zero | — | Obrig. (799) | desde 2017-12-04 | Anexo I, p. 120 | Alta |
| W04g-10 | vICMSUFRemet, W04g | Σ itens vICMSUFRemet (NA17) | Nulo vale zero | — | Obrig. (800) | desde 2017-12-04 | Anexo I, p. 120 | Alta |
| W05-10 | vBCST, W05 | Σ itens vBCST (N21) | — | — | Facult. (533) | desde 2017-12-04 | Anexo I, p. 120 | Média |
| W06-10 | vST, W06 | Σ itens vICMSST (N23) | — | — | Facult. (534) | desde 2017-12-04 | Anexo I, p. 120 | Alta |
| W06a-10 | vFCPST, W06a | Σ itens vFCPST (N23d) | — | — | Obrig. (862) | desde 2017-12-04 | NT 2016.002 v1.61, p. 62 | Alta |
| W06b-10 | vFCPSTRet, W06b | Σ itens vFCPSTRet (N27d) | — | — | Obrig. (859) | desde 2017-12-04 | NT 2016.002 v1.61, p. 62 | Alta |
| W06b.1-10 | qBCMono, W06b.1 | Σ itens qBCMono (N37a) | — | R$ 0,01 (*4) | Facult. (700) | desde 2023-10-30 | NT 2023.001 v1.60, p. 39 | Baixa |
| W06c-10 | vICMSMono, W06c | Σ itens vICMSMono (N39) | — | — | Facult. (967) | desde 2023-10-30 | NT 2023.001 v1.60, p. 39 | Alta |
| W06c.1-10 | qBCMonoReten, W06c.1 | Σ itens qBCMonoReten (N39a) | — | R$ 0,01 (*4) | Facult. (723) | desde 2023-10-30 | NT 2023.001 v1.60, p. 39 | Baixa |
| W06d-10 | vICMSMonoReten, W06d | Σ itens vICMSMonoReten (N41) | — | — | Facult. (968) | desde 2023-10-30 | NT 2023.001 v1.60, p. 39 | Alta |
| W06d.1-10 | qBCMonoRet, W06d.1 | Σ itens qBCMonoRet (N43a) | — | R$ 0,01 (*4) | Facult. (744) | desde 2023-10-30 | NT 2023.001 v1.60, p. 40 | Baixa |
| W06e-10 | vICMSMonoRet, W06e | Σ itens vICMSMonoRet (N45) | — | — | Facult. (969) | desde 2023-10-30 | NT 2023.001 v1.60, p. 40 | Alta |
| W07-10 (v1) | vProd, W07 | Σ itens vProd (I11) dos itens sujeitos ao ICMS | Só itens com indTot (I17b) = 1 | — | Facult. (564) | 2017-12-04 a 2026-08-02 | Anexo I, p. 121 | Média |
| W07-10 (v2) | vProd, W07 | Σ itens vProd (I11) dos itens sujeitos ao ICMS ou ao IBS/CBS/IS | Só itens com indTot (I17b) = 1 | — | Obrig. (564) | desde 2026-08-03 | NT 2025.002 v1.53, p. 70 | Média |
| W08-10 | vFrete, W08 | Σ itens vFrete (I15) | — | — | Facult. (535) | desde 2017-12-04 | Anexo I, p. 121 | Média |
| W09-10 | vSeg, W09 | Σ itens vSeg (I16) | — | — | Facult. (536) | desde 2017-12-04 | Anexo I, p. 121 | Média |
| W10-10 | vDesc, W10 | Σ itens vDesc (I17) | — | — | Facult. (537) | desde 2017-12-04 | Anexo I, p. 121 | Média |
| W11-10 | vII, W11 | Σ itens vII (P04) | — | — | Facult. (601) | desde 2017-12-04 | Anexo I, p. 121 | Média |
| W12-10 | vIPI, W12 | Σ itens vIPI (O14) | — | — | Facult. (538) | desde 2017-12-04 | Anexo I, p. 121 | Alta |
| W12a-10 | vIPIDevol, W12a | Σ itens vIPIDevol (UA04) | — | — | Facult. (863) | desde 2017-12-04 | NT 2016.002 v1.61, p. 62 | Alta |
| W13-10 | vPIS, W13 | Σ itens vPIS (Q09) dos itens com grupo ICMS | — | — | Facult. (602) | desde 2017-12-04 | Anexo I, p. 121 | Média |
| W14-10 | vCOFINS, W14 | Σ itens vCOFINS (S11) dos itens com grupo ICMS | — | — | Facult. (603) | desde 2017-12-04 | Anexo I, p. 121 | Média |
| W15-10 | vOutro, W15 | Σ itens vOutro (I17a) | — | — | Facult. (604) | desde 2017-12-04 | Anexo I, p. 121 | Média |
| W16-10 | vNF, W16 | Ver "Versões da W16-10" | Ver "Versões da W16-10" | R$ 0,50 (*3) | Facult. (610) | desde 2017-12-04, em 4 versões | Anexo I, pp. 122 e 140; NTs 2020.005, 2023.001, 2023.004 | Alta |
| W16a-10 | vTotTrib, W16a | Σ itens vTotTrib (M02) | Campo opcional; ausente vale zero | — | Facult. (685) | desde 2017-12-04 | Anexo I, p. 123 | Informativa |
| W01a-10 | vProdLiqTot, W01a | Informado quando algum item tiver vProdLiq (I11c) | — | — | Obrig. (1282) | desde 2026-11-16 | NT 2026.008 v1.01, p. 8 | Média |
| W01a-20 | vProdLiqTot, W01a | Σ itens vProdLiq (I11c) | Só quando vProdLiqTot for informado | — | Obrig. (1283) | desde 2026-11-16 | NT 2026.008 v1.01, p. 8 | Média |

### ISSQNtot (grupo W17)

| ID | Total (tag, id) | Regra: o total deve ser igual a | Condições e exceções | SEFAZ (rejeição) | Vigência | Fonte | Severidade |
|---|---|---|---|---|---|---|---|
| W18-10 | vServ, W18 | Σ itens vProd (I11) dos itens sujeitos ao ISSQN | — | Facult. (605) | desde 2017-12-04 | Anexo I, p. 123 | Média |
| W19-10 | vBC, W19 | Σ itens vBC do ISSQN (U02) | — | Facult. (606) | desde 2017-12-04 | Anexo I, p. 123 | Média |
| W20-10 | vISS, W20 | Σ itens vISSQN (U04) | — | Facult. (607) | desde 2017-12-04 | Anexo I, p. 123 | Alta |
| W21-10 | vPIS, W21 | Σ itens vPIS (Q09) dos itens sujeitos ao ISSQN | — | Facult. (608) | desde 2017-12-04 | Anexo I, p. 123 | Média |
| W22-10 | vCOFINS, W22 | Σ itens vCOFINS (S11) dos itens sujeitos ao ISSQN | — | Facult. (609) | desde 2017-12-04 | Anexo I, p. 123 | Média |
| W22b-10 | vDeducao, W22b | Σ itens vDeducao (U07) | — | Obrig. (364) | desde 2017-12-04 | Anexo I, p. 123 | Baixa |
| W22c-10 | vOutro, W22c | Σ itens vOutro (U08) | — | Obrig. (365) | desde 2017-12-04 | Anexo I, p. 123 | Baixa |
| W22d-10 | vDescIncond, W22d | Σ itens vDescIncond (U09) | — | Obrig. (366) | desde 2017-12-04 | Anexo I, p. 123 | Baixa |
| W22e-10 | vDescCond, W22e | Σ itens vDescCond (U10) | — | Obrig. (367) | desde 2017-12-04 | Anexo I, p. 123 | Baixa |
| W22f-10 | vISSRet, W22f | Σ itens vISSRet (U11) | — | Obrig. (368) | desde 2017-12-04 | Anexo I, p. 123 | Baixa |

O grupo retTrib (W23 a W30) não tem regras de validação (Anexo I, p. 123).

### IS, IBS e CBS (grupo W03 da NT 2025.002)

Todas são "Obrig." na SEFAZ, sem tolerância no texto, e só se aplicam quando os grupos de IBS, CBS ou
IS estiverem preenchidos. Fonte: NT 2025.002 v1.53, pp. 70–71. Vigência: desde 2025-11-10 (entrada
do schema e das regras da v1.30), salvo indicação.

| ID | Regra | Rejeição | Vigência | Severidade |
|---|---|---|---|---|
| W31-10 | Grupo ISTot informado sem nenhum item com IS | 1073 | desde 2025-11-10 | Média |
| W31-20 | Grupo ISTot ausente com algum item com IS | 1117 | desde 2025-11-10 | Média |
| W34-10 | Grupo IBSCBSTot informado sem nenhum item com IBSCBS (UB12) | 1118 | desde 2025-11-10 | Média |
| W34-20 | Grupo IBSCBSTot ausente com algum item com IBSCBS | 1119 | desde 2025-11-10 | Média |
| W35-10 | vBCIBSCBS = Σ itens gIBSCBS/vBC | 1076 | desde 2025-11-10 | Média |
| W38-10, W39-10, W41-10 | vDif, vDevTrib e vIBSUF do IBS estadual = Σ itens | 1077, 1078, 1080 | desde 2025-11-10 | Alta |
| W43-10, W44-10, W46-10 | vDif, vDevTrib e vIBSMun do IBS municipal = Σ itens | 1081, 1082, 1084 | desde 2025-11-10 | Alta |
| W47-10 | vIBS = Σ itens gIBSCBS/vIBS | 1085 | desde 2025-11-10 | Alta |
| W48-10 | vCredPres do IBS = Σ itens | 1086 | desde 2025-11-10 | Alta |
| W53-10, W54-10, W56-10 | vDif, vDevTrib e vCBS = Σ itens | 1088, 1089, 1091 | desde 2025-11-10 | Alta |
| W56a-10 | vCredPres da CBS = Σ itens | 1087 | desde 2025-11-10 | Alta |
| W58-10, W59-10 | vIBSMono e vCBSMono = Σ itens | 1092, 1093 | desde 2025-11-10 | Alta |
| W59a-10, W59b-10 | vIBSMonoReten e vCBSMonoReten = Σ itens | 1120, 1121 | desde 2025-11-10 | Alta |
| W59c-10, W59d-10 | vIBSMonoRet e vCBSMonoRet = Σ itens | 1122, 1123 | desde 2025-11-10 | Alta |
| W59f-10, W59g-10 | vIBSEstCred e vCBSEstCred = Σ itens gEstornoCred/vIBS e /vCBS | 1176, 1177 | desde 2026-02-02 | Alta |

### Cobrança e pagamento (grupos Y e YA)

Regras que fecham os totais da fatura, das duplicatas e dos pagamentos com o valor da nota. Fonte do
texto: Anexo I, pp. 125–126. Vigência conforme a NT 2016.002 v1.61, que criou as regras do grupo Y
na v1.50 (produção em 2018-06-04, p. 13) e deu data própria à Y10-10 (pp. 64–65).

| ID | Regra | Condições e exceções | SEFAZ (rejeição) | Vigência | Severidade |
|---|---|---|---|---|---|
| Y05-10 | Desconto da fatura (vDesc, Y05) não pode ser maior que o valor original (vOrig, Y04) | Campos opcionais não informados valem zero | Obrig. (901) | desde 2018-06-04 | Média |
| Y06-10 | Valor líquido da fatura (vLiq, Y06) = vOrig (Y04) − vDesc (Y05) | Só quando vLiq e vOrig forem informados | Obrig. (902) | desde 2018-06-04 | Média |
| Y10-10 | Σ das duplicatas vDup (Y10) = vLiq (Y06) | — | Obrig. (851) | desde 2018-07-02 | Média |
| YA03-20 | Se Σ dos pagamentos vPag (YA03) for maior que vNF (W16), o troco (vTroco, YA09) deve ser informado | — | Facult. (866) | desde 2017-12-04 | Média |
| YA09-10 | Troco informado: vTroco (YA09) = Σ vPag (YA03) − vNF (W16) | O texto não diz se vPag é somado; o catálogo usa a soma (ver C5) | Obrig. (869) | desde 2017-12-04 | Média |

## Versões da W16-10 (valor total da NF-e)

Rejeição 610, "Facult." no Anexo I (a NT 2016.002 a marcava como "Obrig."). Tolerância de R$ 0,50
para mais ou para menos em todas as versões: marcador (*3), definido no Anexo I, p. 140 ("Considerar
uma tolerância de R$ 0,50 para mais ou para menos (NT 2012/003)").

| Versão | Vigência | Fórmula: vNF deve ser igual a | Exceções | Fonte |
|---|---|---|---|---|
| 1 | 2017-12-04 a 2021-10-03 | vProd − vDesc − vICMSDeson + vST + vFCPST + vFrete + vSeg + vOutro + vII + vIPI + vIPIDevol + vServ | 1) Faturamento direto de veículo novo (tpOp = 2): sem vST, vFCPST e vIPIDevol. 2) Não se aplica a importação (CFOP iniciado por 3). 3) Não rejeita se vICMSDeson não foi subtraído | Anexo I, p. 122 |
| 2 | 2021-10-04 a 2023-03-29 | Versão 1 + vPIS do grupo PISST (R06), se indSomaPISST = 1, + vCOFINS do grupo COFINSST (T06), se indSomaCOFINSST = 1 | Iguais à versão 1, com PIS-ST e COFINS-ST também na exceção 1 | NT 2020.005 v1.21, p. 20 |
| 3 | 2023-03-30 a 2024-06-30 | Versão 2 + vICMSMonoReten (W06d) | Iguais à versão 2 | NT 2023.001 v1.60, pp. 11 e 41 |
| 4 | desde 2024-07-01 | Igual à versão 3 | 1) Veículo novo (tpOp = 2): sem vST, vFCPST e vIPIDevol, com vICMSMonoReten. 2) Não se aplica a importação (CFOP iniciado por 3). 4) Não rejeita se não foi subtraído o vICMSDeson (N28a) dos itens com indDeduzDeson (N28b) = 0 ou não informado. A exceção 3 deixa de existir | NT 2023.004 v1.20, pp. 5–6 e 15–16 |

## Regras fora da versão 1

| Regra | Motivo |
|---|---|
| W03-20, W04-20, W06-20, W16-20, W16-30 | Limites de valor definidos por cada UF; o XML não traz o limite aplicável |
| W16-40, W16-50, W16-60, W16-70 | Regras de identificação do destinatário e de pagamento com limite por UF ou exclusivas da NFC-e |
| W33-10, W60-05, W60-10, VB01-05, VB01-10, VB01-20 | "Implementação futura" nas NTs 2025.002 v1.53 e 2026.008 v1.01; não estão em vigor |
| 3BA02-50, 3BA02-54, 3BA02-58, 3BA02-64, 3BA02-68 | Comparam a devolução com as NF-e referenciadas, que exigem consulta à base da SEFAZ (fora do escopo da v1) |
| YA03-10 | "Implementação futura para modelo 55" (NT 2025.001 v1.03, p. 10); no Anexo I vale só para a NFC-e |
| YA09-20 | Limite de valor do troco, parâmetro de vigência imediata alterado pela NT 2023.004 v1.20; não é fechamento de total |

## Pendências

- **C1 — Severidades**: resolvida. O solicitante aprovou o critério de severidade em 2026-11-04.
- **C2 — Versão do Anexo I**: a capa diz 7.00 (novembro de 2020) e o controle de versões lista 7.03
  (outubro de 2020) como a mais recente. O conteúdo corresponde à 7.03.
- **C3 — Texto tachado**: a conversão dos PDFs não preserva texto riscado. Na NT 2023.004, a
  exceção 3 da W16-10 ainda aparece no texto, mas a própria NT diz que ela foi substituída pela
  exceção 4. O catálogo segue a NT. Recomenda-se conferir no PDF original.
- **C4 — Arredondamento**: o Anexo I não define tolerância para as regras "total igual à soma dos
  itens", exceto W16-10 e as de quantidade monofásica. O catálogo compara em valor exato.
- **C5 — YA09-10**: o Anexo I não diz se o vPag da fórmula do troco é o de cada pagamento ou a soma;
  o catálogo usa a soma, porque o troco é único no grupo de pagamento.

## Aprovação

| Papel | Nome | Data | Decisão |
|---|---|---|---|
| Solicitante | Solicitante da feature | 2026-11-04 | Aprovado o critério de severidade proposto e o escopo "totais iguais à soma dos itens + cobrança e pagamento"; regras de implementação futura fora da v1 |
