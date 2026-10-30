#!/usr/bin/env bash
# Runs on every start. Realm import skips realms that already exist, so this applies
# the realm settings the project owns to running realms too. Changing these keys in
# the console is undone on the next start.
set -euo pipefail

KCADM="/opt/keycloak/bin/kcadm.sh"
CONFIG="/tmp/kcadm.config"
SERVER="http://keycloak:8080"
PASSWORD_POLICY="length(8) and upperCase(1) and lowerCase(1) and digits(1) and specialChars(1)"

for attempt in $(seq 1 60); do
  if "$KCADM" config credentials --config "$CONFIG" --server "$SERVER" --realm master \
      --user "$KC_BOOTSTRAP_ADMIN_USERNAME" --password "$KC_BOOTSTRAP_ADMIN_PASSWORD" >/dev/null 2>&1; then
    break
  fi
  if [ "$attempt" -eq 60 ]; then
    echo "Could not sign in to Keycloak at $SERVER." \
      "Check that it is running and KC_BOOTSTRAP_ADMIN_USERNAME / KC_BOOTSTRAP_ADMIN_PASSWORD in infrastructure/keycloak/.env."
    exit 1
  fi
  sleep 2
done

# Prints the internal id of a client, or nothing when it does not exist.
client_id_of() {
  "$KCADM" get clients -r "$1" --config "$CONFIG" -q "clientId=$2" --fields id --format csv --noquotes
}

for realm in carsales carsalesDev; do
  "$KCADM" update "realms/$realm" --config "$CONFIG" \
    -s registrationEmailAsUsername=false \
    -s loginWithEmailAllowed=true \
    -s duplicateEmailsAllowed=false \
    -s revokeRefreshToken=true \
    -s refreshTokenMaxReuse=0 \
    -s "passwordPolicy=$PASSWORD_POLICY"

  # Realm role for catalog administration and later moderation.
  if ! "$KCADM" get roles/admin -r "$realm" --config "$CONFIG" >/dev/null 2>&1; then
    "$KCADM" create roles -r "$realm" --config "$CONFIG" -s name=admin -s "description=CarSales administrator"
  fi

  # ms-listing only introspects tokens, so it has no login flows of its own.
  if [ "$realm" = carsales ]; then listing_secret="$MS_LISTING_CLIENT_SECRET"; else listing_secret="$MS_LISTING_CLIENT_SECRET_DEV"; fi
  if [ -z "$listing_secret" ]; then
    echo "MS_LISTING_CLIENT_SECRET / MS_LISTING_CLIENT_SECRET_DEV is empty in infrastructure/keycloak/.env."
    exit 1
  fi
  listing_flags=(-s enabled=true -s publicClient=false -s clientAuthenticatorType=client-secret
    -s standardFlowEnabled=false -s directAccessGrantsEnabled=false -s implicitFlowEnabled=false
    -s serviceAccountsEnabled=false -s "secret=$listing_secret")
  listing_id="$(client_id_of "$realm" ms-listing)"
  if [ -z "$listing_id" ]; then
    "$KCADM" create clients -r "$realm" --config "$CONFIG" -s clientId=ms-listing "${listing_flags[@]}"
  else
    "$KCADM" update "clients/$listing_id" -r "$realm" --config "$CONFIG" "${listing_flags[@]}"
  fi

  # Login tokens are issued to ms-auth; this adds ms-listing to their audience.
  auth_id="$(client_id_of "$realm" ms-auth)"
  if ! "$KCADM" get "clients/$auth_id/protocol-mappers/models" -r "$realm" --config "$CONFIG" --fields name --format csv --noquotes \
      | grep -qx ms-listing-audience; then
    "$KCADM" create "clients/$auth_id/protocol-mappers/models" -r "$realm" --config "$CONFIG" \
      -s name=ms-listing-audience -s protocol=openid-connect -s protocolMapper=oidc-audience-mapper \
      -s 'config."included.client.audience"=ms-listing' \
      -s 'config."access.token.claim"=true' \
      -s 'config."id.token.claim"=false' \
      -s 'config."introspection.token.claim"=true'
  fi

  echo "Realm '$realm' settings applied"
done
