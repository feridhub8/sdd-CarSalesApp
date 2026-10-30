-- Runs on every start. Creates missing roles and databases, and keeps passwords in sync with .env.
-- Run with: psql -v ON_ERROR_STOP=1 -v kc_password=... -v listing_password=... -v listing_dev_password=...

-- keycloak
SELECT format('CREATE ROLE %I LOGIN', 'keycloak')
WHERE NOT EXISTS (SELECT FROM pg_roles WHERE rolname = 'keycloak')\gexec

ALTER ROLE "keycloak" WITH LOGIN PASSWORD :'kc_password';

SELECT format('CREATE DATABASE %I OWNER %I', 'keycloak', 'keycloak')
WHERE NOT EXISTS (SELECT FROM pg_database WHERE datname = 'keycloak')\gexec

-- ms-listing (Production)
SELECT format('CREATE ROLE %I LOGIN', 'listing')
WHERE NOT EXISTS (SELECT FROM pg_roles WHERE rolname = 'listing')\gexec

ALTER ROLE "listing" WITH LOGIN PASSWORD :'listing_password';

SELECT format('CREATE DATABASE %I OWNER %I', 'listing', 'listing')
WHERE NOT EXISTS (SELECT FROM pg_database WHERE datname = 'listing')\gexec

-- ms-listing (Development)
SELECT format('CREATE ROLE %I LOGIN', 'listingDev')
WHERE NOT EXISTS (SELECT FROM pg_roles WHERE rolname = 'listingDev')\gexec

ALTER ROLE "listingDev" WITH LOGIN PASSWORD :'listing_dev_password';

SELECT format('CREATE DATABASE %I OWNER %I', 'listingDev', 'listingDev')
WHERE NOT EXISTS (SELECT FROM pg_database WHERE datname = 'listingDev')\gexec

-- Every database is reachable only by its owner (and the superuser), so a leaked
-- development password cannot connect to production or Keycloak data.
REVOKE CONNECT ON DATABASE "keycloak" FROM PUBLIC;
REVOKE CONNECT ON DATABASE "listing" FROM PUBLIC;
REVOKE CONNECT ON DATABASE "listingDev" FROM PUBLIC;
