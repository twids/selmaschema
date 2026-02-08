# Code Review: Containerization & Portainer Deployment

**Ready for Production**: Yes (with env secrets configured)
**Critical Issues**: 1

## Priority 1 (Must Fix) ⛔

- Secrets were hardcoded in `docker-compose.yml` and `appsettings.json` (POSTGRES_PASSWORD, connection string).
  - Fix implemented: Parameterized Compose with `${POSTGRES_PASSWORD}` and created `.env` placeholders.
  - Action required: Set a strong, unique `POSTGRES_PASSWORD` before deployment; do not commit real secrets.

## Recommended Changes

- Harden runtime image by running as non-root user.

```dockerfile
# in Dockerfile (final stage)
RUN useradd -m -u 10001 appuser && chown -R appuser:appuser /app
USER appuser
```

- Add `.dockerignore` to minimize build context and reduce attack surface.

```gitignore
.git/
frontend/node_modules/
backend/**/bin/
backend/**/obj/
backend/CoParenting.Tests/
.e2e-tests/
.env
```

- Ensure Production configuration in Compose.

```yaml
# in docker-compose.yml
environment:
  - ASPNETCORE_ENVIRONMENT=${ASPNETCORE_ENVIRONMENT:-Production}
```

- Healthcheck for API container (optional if your image lacks curl/wget):

```yaml
# Example (requires curl)
healthcheck:
  test: ["CMD", "curl", "-f", "http://localhost:8080/health"]
  interval: 30s
  timeout: 5s
  retries: 5
```

- TLS & Reverse Proxy: Place the app behind a TLS-terminating reverse proxy (Traefik/Caddy/Nginx) and enforce HTTPS.

## OWASP Top 10 Focus

- A05 Security Misconfiguration: Removed hardcoded credentials; centralized env management via `.env`.
- A08 Software & Data Integrity: Use immutable images; pin base images to specific versions and update regularly.
- A06 Vulnerable/Outdated Components: Scan images with Trivy/Snyk; monitor Dependabot alerts.
- Zero Trust: Avoid trusting internal networks; require proper auth tokens (Portainer PAT via `X-API-Key`).

## Deployment Notes

- Use `scripts/deploy-portainer.ps1` to create/update the stack via Portainer REST API.
- Required environment: `PORTAINER_API_KEY` set with your personal access token.
- Ensure `.env` is present with a strong `POSTGRES_PASSWORD` before deploying.

---

Audited on 2026-02-08 by GitHub Copilot (GPT-5).