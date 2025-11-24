# GitHub Actions Workflow Policies

This document outlines the policies for GitHub Actions workflows in this repository to optimize usage and avoid quota limits.

## Workflow Overview

| Workflow | File | Status | Purpose |
|----------|------|--------|---------|
| CI/CD Pipeline | `.github/workflows/ci-cd.yml` | **Active** | Build, test, and deploy the application |

## Workflow Triggers

Workflows run only when relevant files change to reduce unnecessary runs:

### CI/CD Pipeline Triggers
- **Branches:** `main`, `develop`, `copilot/**`
- **Path filters:** Only runs when changes are made to:
  - `backend/**` - Backend source code
  - `frontend/**` - Frontend source code
  - `Dockerfile` - Docker configuration
  - `docker-compose.yml` - Docker Compose configuration
  - `.github/workflows/ci-cd.yml` - Workflow configuration
  - `e2e-tests/**` - End-to-end tests

### What Does NOT Trigger Workflows
- Documentation-only changes (`*.md` files outside of paths above)
- IDE configuration files (`.vscode/**`, `.idea/**`)
- Git configuration (`.gitignore`, `.gitattributes`)

## Artifact Policies

### Retention Periods
| Artifact | Retention | Condition |
|----------|-----------|-----------|
| `application-build` | 3 days | Always uploaded |
| `e2e-test-results` | 3 days | Only on failure |
| `e2e-test-report` | 3 days | Only on failure |

### Artifact Size Guidelines
- Upload only production-ready outputs (published binaries, not build intermediates)
- Exclude `node_modules`, `bin/obj`, and other dependencies
- Test artifacts should only include failure evidence (screenshots, logs)

## Job Structure

### CI/CD Pipeline Jobs
1. **Build Application** - Builds frontend and backend, runs unit tests
2. **Build Docker Image** - Builds and validates Docker image (depends on Build)
3. **Integration Tests** - Tests services working together (depends on Docker Build)
4. **End-to-End Tests** - Runs Playwright E2E tests (depends on Docker Build)
5. **Publish Docker Image** - Publishes to GHCR (only on `main` branch merge)

### Job Dependencies
```
Build → Docker Build → Integration Tests → Publish (main only)
                    ↘ E2E Tests
```

## Quota Management

### Current Optimizations
1. **Path filtering** - Workflows skip if no relevant files changed
2. **Conditional artifacts** - Test artifacts only uploaded on failure
3. **Reduced retention** - 3-day retention instead of default 90 days
4. **Consolidated workflows** - Single workflow instead of multiple separate ones
5. **Branch restrictions** - Limited to specific branches

### Monitoring Usage
- Check Actions usage in repository Settings → Actions → General
- Monitor artifact storage in repository Settings → Actions → Management

## Making Changes

When modifying workflows:
1. Test changes in a feature branch first
2. Use `workflow_dispatch` for manual testing if needed
3. Keep path filters up to date when adding new directories
4. Consider quota impact before adding new artifact uploads
5. Prefer `if: failure()` for debugging artifacts over `if: always()`

## Disabled/Removed Workflows

| Workflow | Reason | Date |
|----------|--------|------|
| `e2e-tests.yml` (separate) | Consolidated into `ci-cd.yml` to reduce workflow runs | 2024 |
