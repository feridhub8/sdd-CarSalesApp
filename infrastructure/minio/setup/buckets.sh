#!/bin/sh
# Runs on every start and is safe to run again: creates any missing bucket, and syncs
# policies and keys to the files here and to infrastructure/minio/.env.
set -eu

for name in MINIO_ROOT_PASSWORD LISTING_STORAGE_SECRET_KEY LISTING_STORAGE_SECRET_KEY_DEV; do
  eval "value=\${$name:-}"
  [ -n "$value" ] || { echo "$name is empty in infrastructure/minio/.env"; exit 1; }
done

mc alias set carsales http://minio:9000 "$MINIO_ROOT_USER" "$MINIO_ROOT_PASSWORD" >/dev/null

# Creates a bucket whose files anyone can read, and the only key that can write to it.
setup_bucket() {
  bucket="$1"
  access_key="$2"
  secret_key="$3"

  mc mb --ignore-existing "carsales/$bucket"
  mc anonymous set-json "/setup/policies/$bucket-anonymous.json" "carsales/$bucket"

  mc admin user add carsales "$access_key" "$secret_key" >/dev/null
  mc admin policy create carsales "$access_key" "/setup/policies/$access_key.json" >/dev/null
  mc admin policy attach carsales "$access_key" --user "$access_key" >/dev/null
}

setup_bucket listing-images ms-listing "$LISTING_STORAGE_SECRET_KEY"
setup_bucket listing-images-dev ms-listing-dev "$LISTING_STORAGE_SECRET_KEY_DEV"

echo "MinIO setup finished"
