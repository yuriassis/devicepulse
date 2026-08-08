# Deployment

## Compose

Docker é opcional. Use `cp .env.example .env && docker compose up --build` para o ambiente mínimo com PostgreSQL, API e frontend. Ative RabbitMQ com `--profile messaging` e a pilha de telemetria com `--profile observability`. PostgreSQL usa volume nomeado e credenciais vêm apenas do ambiente.

## Pacote local

`scripts/publish-local.sh` e `scripts/publish-local.ps1` geram uma distribuição framework-dependent em `publish/devicepulse`. Ela reúne frontend e API, usa SQLite por padrão e precisa apenas do runtime ASP.NET Core 8. Os launchers mantêm o banco em `data/devicepulse.db`.

## Kubernetes e OpenShift

Crie `devicepulse-secrets` fora do Git e execute `kubectl apply -k deploy/kubernetes/base`. A Route OpenShift usa TLS edge. Security contexts proíbem privilege escalation e os deployments aceitam o UID atribuído pela plataforma. PostgreSQL e RabbitMQ são demonstrações; produção deve usar serviços gerenciados/operadores. Migrações devem rodar em Job único no processo de release.
