ui            = true
disable_mlock = true
api_addr      = "http://localhost:8200"

# Vault 2.x requires a token for generate-root by default. Allow it without one,
# so that a new root token can be created from the unseal keys if every admin login is lost.
enable_unauthenticated_access = ["generate-root"]

storage "file" {
  path = "/vault/file"
}

listener "tcp" {
  address     = "0.0.0.0:8200"
  tls_disable = true
}
