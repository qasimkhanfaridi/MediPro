# MediPro — single image: API + React SPA (wwwroot). For client UAT on Fly.io / any Docker host.
# Build from this folder: docker build -t medipro .

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS api-build
WORKDIR /src
COPY MediPro.Api/MediPro.Api.csproj MediPro.Api/
RUN dotnet restore MediPro.Api/MediPro.Api.csproj
COPY MediPro.Api/ MediPro.Api/
RUN dotnet publish MediPro.Api/MediPro.Api.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM node:22-alpine AS web-build
WORKDIR /web
COPY MediPro.Web/package.json MediPro.Web/package-lock.json ./
RUN npm ci
COPY MediPro.Web/ ./
# Same-origin: browser calls /api on this host (no VITE_API_ORIGIN).
ENV VITE_API_ORIGIN=
RUN npm run build

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
ENV ASPNETCORE_URLS=http://0.0.0.0:8080
EXPOSE 8080
COPY --from=api-build /app/publish .
COPY --from=web-build /web/dist ./wwwroot
ENTRYPOINT ["dotnet", "MediPro.Api.dll"]
