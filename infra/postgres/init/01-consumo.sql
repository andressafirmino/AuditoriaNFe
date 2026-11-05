-- Executado pelo entrypoint do PostgreSQL só na primeira inicialização (volume vazio), no banco
-- POSTGRES_DB (auditoria_agente), com o usuário administrador. Cria apenas o usuário do Api.Agente e
-- o schema consumo (ADR-0006); a tabela consumo.chamadas_provedor vem da migração EF Core (T029),
-- que também DEVE manter o histórico de migrações no schema consumo.

\set ON_ERROR_STOP on
\getenv usuario_app POSTGRES_APP_USER
\getenv senha_app POSTGRES_APP_PASSWORD
\if :{?usuario_app}
\else
  \set usuario_app ''
\endif
\if :{?senha_app}
\else
  \set senha_app ''
\endif

SELECT coalesce(:'usuario_app', '') = '' OR coalesce(:'senha_app', '') = '' AS faltando_credenciais \gset
\if :faltando_credenciais
  DO $$ BEGIN RAISE EXCEPTION 'Defina POSTGRES_APP_USER e POSTGRES_APP_PASSWORD no arquivo .env'; END $$;
\endif

CREATE ROLE :"usuario_app" LOGIN PASSWORD :'senha_app'
    NOSUPERUSER NOCREATEDB NOCREATEROLE NOINHERIT NOREPLICATION;

-- Só o usuário do Api.Agente conecta a este banco; nada é criado no schema public.
REVOKE ALL ON DATABASE :"DBNAME" FROM PUBLIC;
GRANT CONNECT, TEMPORARY ON DATABASE :"DBNAME" TO :"usuario_app";
REVOKE ALL ON SCHEMA public FROM PUBLIC;

CREATE SCHEMA consumo AUTHORIZATION :"usuario_app";
ALTER ROLE :"usuario_app" IN DATABASE :"DBNAME" SET search_path = consumo;

COMMENT ON SCHEMA consumo IS 'Consumo e custos das chamadas ao provedor de IA do Api.Agente (data-model §14).';
