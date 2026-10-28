-- Runs on every start. Creates missing roles and databases, and keeps passwords in sync with .env.
-- Run with: psql -v ON_ERROR_STOP=1 -v kc_password=...

-- keycloak
SELECT format('CREATE ROLE %I LOGIN', 'keycloak')
WHERE NOT EXISTS (SELECT FROM pg_roles WHERE rolname = 'keycloak')\gexec

ALTER ROLE "keycloak" WITH LOGIN PASSWORD :'kc_password';

SELECT format('CREATE DATABASE %I OWNER %I', 'keycloak', 'keycloak')
WHERE NOT EXISTS (SELECT FROM pg_database WHERE datname = 'keycloak')\gexec
