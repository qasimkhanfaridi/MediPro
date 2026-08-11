# GitHub Actions — MediPro

**Repo:** https://github.com/qasimkhanfaridi/MediPro

## Pipelines

| Workflow | When | What |
|----------|------|------|
| **CI** (`.github/workflows/ci.yml`) | Push / PR to `main` | .NET restore → build → unit tests; npm ci → Vite build |
| **Deploy UAT** (`.github/workflows/deploy-uat.yml`) | Push to `main` (src changes) or manual run | `fly deploy --app medipro-uat` |

## Enable auto-deploy to Fly

1. Create a Fly deploy token: `fly tokens create deploy -x 999999h` (or from Fly dashboard).
2. GitHub → **MediPro** → **Settings** → **Secrets and variables** → **Actions**.
3. New repository secret:
   - Name: `FLY_API_TOKEN`
   - Value: the Fly token

Until that secret exists, **Deploy UAT** is skipped (CI still runs).

## Manual deploy from Actions

**Actions** → **Deploy UAT (Fly.io)** → **Run workflow**.

## Local check (same as CI)

```powershell
cd B:\MediPro\src
dotnet test MediPro.Api.Tests/MediPro.Api.Tests.csproj -c Release
cd MediPro.Web
npm ci
npm run build
```
