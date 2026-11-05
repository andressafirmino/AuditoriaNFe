// Executado pelo entrypoint do MongoDB só na primeira inicialização (volume vazio), com o usuário root.
// Convenções esperadas pela Infraestrutura (T028):
// - GUIDs gravados como binData subtipo 4 (GuidRepresentation.Standard do driver);
// - datas em UTC como BSON date;
// - chaveIsolamento = SHA-256 em hexadecimal minúsculo (data-model §0).

const nomeBanco = "auditoria_agente";
const usuarioApp = process.env.MONGO_APP_USERNAME;
const senhaApp = process.env.MONGO_APP_PASSWORD;

if (!usuarioApp || !senhaApp) {
  throw new Error("Defina MONGO_APP_USERNAME e MONGO_APP_PASSWORD no arquivo .env");
}

const banco = db.getSiblingDB(nomeBanco);

// Usuário do Api.Agente: leitura e escrita só neste banco.
banco.createUser({
  user: usuarioApp,
  pwd: senhaApp,
  roles: [{ role: "readWrite", db: nomeBanco }]
});

const chaveIsolamento = { bsonType: "string", pattern: "^[0-9a-f]{64}$" };
const identificador = { bsonType: "string", pattern: "^[A-Za-z0-9._:@-]{1,128}$" };
const guid = { bsonType: "binData" };

// Origem de cada item do contexto do usuário (FR-055b).
const origem = {
  bsonType: "object",
  additionalProperties: false,
  required: ["tipo", "registradoEm"],
  properties: {
    tipo: { enum: ["Execucao", "DeclaracaoUsuario"] },
    execucaoId: { bsonType: ["binData", "null"] },
    sessaoId: { bsonType: ["binData", "null"] },
    registradoEm: { bsonType: "date" }
  }
};

// Contexto do usuário (data-model §13, FR-055a): só os campos abaixo. CPF, nomes, endereços,
// valores de documentos e texto livre não têm onde ser gravados. Não existe para "anonimo" (FR-052c).
banco.createCollection("contextos_usuario", {
  validator: {
    $jsonSchema: {
      bsonType: "object",
      additionalProperties: false,
      required: [
        "_id", "usuarioId", "chamadorId", "cnpjsAuditados", "achadosRecorrentes",
        "execucoesContabilizadas", "criadoEm", "atualizadoEm", "ultimaAtividadeEm", "expiraEm"
      ],
      properties: {
        _id: chaveIsolamento,
        usuarioId: { ...identificador, not: { enum: ["anonimo"] } },
        chamadorId: identificador,
        cnpjsAuditados: {
          bsonType: "array",
          items: {
            bsonType: "object",
            additionalProperties: false,
            required: ["cnpj", "papel", "quantidadeDocumentos", "ultimaAuditoriaEm", "origem"],
            properties: {
              cnpj: { bsonType: "string", pattern: "^[0-9A-Z]{12}[0-9]{2}$" },
              papel: { enum: ["Emitente", "Destinatario"] },
              quantidadeDocumentos: { bsonType: ["int", "long"], minimum: 1 },
              ultimaAuditoriaEm: { bsonType: "date" },
              origem: origem
            }
          }
        },
        achadosRecorrentes: {
          bsonType: "array",
          items: {
            bsonType: "object",
            additionalProperties: false,
            required: ["chave", "categoria", "frequencia", "ultimaOcorrenciaEm", "origem"],
            properties: {
              chave: { bsonType: "string", pattern: "^[A-Za-z0-9._-]{1,64}$" },
              categoria: { enum: ["ErroEstrutural", "InconsistenciaNegocio"] },
              frequencia: { bsonType: ["int", "long"], minimum: 1 },
              ultimaOcorrenciaEm: { bsonType: "date" },
              origem: origem
            }
          }
        },
        preferencias: {
          bsonType: ["object", "null"],
          additionalProperties: false,
          required: ["nivelDetalhe", "tributosInteresse", "origem"],
          properties: {
            nivelDetalhe: { enum: ["Resumido", "Detalhado"] },
            tributosInteresse: {
              bsonType: "array",
              uniqueItems: true,
              items: { enum: ["ICMS", "ICMS-ST", "FCP", "IPI", "PIS", "COFINS", "ISSQN", "IBS", "CBS", "IS"] }
            },
            origem: origem
          }
        },
        execucoesContabilizadas: { bsonType: "array", items: guid },
        criadoEm: { bsonType: "date" },
        atualizadoEm: { bsonType: "date" },
        ultimaAtividadeEm: { bsonType: "date" },
        expiraEm: { bsonType: "date" }
      }
    }
  },
  validationLevel: "strict",
  validationAction: "error"
});

// Remoção automática no instante de expiraEm (FR-060: 90 dias sem atividade, configurável).
banco.contextos_usuario.createIndex(
  { expiraEm: 1 },
  { name: "ttl_contextos_usuario_expira_em", expireAfterSeconds: 0 });

banco.createCollection("sessoes");
banco.createCollection("execucoes");
banco.createCollection("documentos");
banco.createCollection("sessoes_agente");

// Todo acesso confere a chave de isolamento (FR-039).
banco.sessoes.createIndex(
  { chaveIsolamento: 1, ultimaAtividadeEm: -1 },
  { name: "ix_sessoes_chave_atividade" });

banco.execucoes.createIndex(
  { chaveIsolamento: 1, sessaoId: 1, iniciadaEm: -1 },
  { name: "ix_execucoes_chave_sessao_inicio" });

banco.documentos.createIndex(
  { chaveIsolamento: 1, sessaoId: 1 },
  { name: "ix_documentos_chave_sessao" });

banco.sessoes_agente.createIndex(
  { chaveIsolamento: 1 },
  { name: "ix_sessoes_agente_chave" });
