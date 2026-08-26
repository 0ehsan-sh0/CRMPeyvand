# PBKDF2-SHA256 password hashing replaces reversible Base64 encoding

Passwords were stored as UTF-8 → Base64 (`UserBLL.Encode`, commented "Encode For Password Is Security") and verified by ciphertext equality — obfuscation, not security. Passwords are now hashed with PBKDF2-SHA256 (`Rfc2898DeriveBytes` with `HashAlgorithmName.SHA256`, native to .NET Framework 4.7.2 — no new dependency in an app that vendors its DLLs deliberately), 100,000 iterations, 16-byte random salt per user, stored in the existing Password column as the versioned string `pbkdf2-sha256:<iterations>:<saltB64>:<hashB64>` so iteration count can rise later without a schema change. `Encode`/`Decode` are deleted; login loads by username and recomputes the hash to verify.

## Considered Options

- BCrypt (NuGet package): rejected — adds an external binary dependency to a project that vendors DLLs on purpose.
- Keep Base64: rejected — trivially reversible.
- ASP.NET Identity: rejected — massive footprint for a desktop app with ~one auth surface.

## Consequences

- Passwords can never be recovered or displayed, only reset.
- Editing a user with a blank password keeps the stored hash (blank means "unchanged"), fixing the legacy wipe-the-password bug.
- Pre-existing credential data is not migrated: the dev-stage database is wiped (no production deployments exist).
