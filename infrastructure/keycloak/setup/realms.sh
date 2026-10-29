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

for realm in carsales carsalesDev; do
  "$KCADM" update "realms/$realm" --config "$CONFIG" \
    -s registrationEmailAsUsername=false \
    -s loginWithEmailAllowed=true \
    -s duplicateEmailsAllowed=false \
    -s revokeRefreshToken=true \
    -s refreshTokenMaxReuse=0 \
    -s "passwordPolicy=$PASSWORD_POLICY"
  echo "Realm '$realm' settings applied"
done
