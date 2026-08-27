# syntax=docker/dockerfile:1.7

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS api-build
WORKDIR /source

COPY NuGet.Config ./
COPY src/InvestFlow.Api/InvestFlow.Api.csproj src/InvestFlow.Api/
RUN dotnet restore src/InvestFlow.Api/InvestFlow.Api.csproj --configfile NuGet.Config

COPY src/InvestFlow.Api/ src/InvestFlow.Api/
RUN dotnet publish src/InvestFlow.Api/InvestFlow.Api.csproj \
    --configuration Release \
    --no-restore \
    --output /app/publish \
    /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS api
WORKDIR /app

RUN apt-get update \
    && apt-get install --yes --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/* \
    && mkdir -p /var/lib/investflow/keys \
    && chown -R "$APP_UID:$APP_UID" /var/lib/investflow

COPY --from=api-build /app/publish ./

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
USER $APP_UID

HEALTHCHECK --interval=10s --timeout=3s --start-period=20s --retries=5 \
    CMD curl --fail --silent http://localhost:8080/api/health || exit 1

ENTRYPOINT ["dotnet", "InvestFlow.Api.dll"]

FROM node:24-alpine AS web-build
WORKDIR /source

RUN corepack enable && corepack prepare pnpm@11.19.0 --activate
COPY src/InvestFlow.Web/package.json src/InvestFlow.Web/pnpm-lock.yaml ./
RUN pnpm install --frozen-lockfile

COPY src/InvestFlow.Web/ ./
RUN pnpm build

FROM nginx:1.31.4-alpine AS web
COPY docker/nginx.conf /etc/nginx/conf.d/default.conf
COPY --from=web-build /source/dist /usr/share/nginx/html

EXPOSE 80

HEALTHCHECK --interval=10s --timeout=3s --start-period=5s --retries=3 \
    CMD wget --quiet --tries=1 --spider http://localhost/health || exit 1

CMD ["nginx", "-g", "daemon off;"]
