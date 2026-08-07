# Deployment

## Compose

Use `cp .env.example .env && docker compose up --build`. PostgreSQL usa volume nomeado. Credenciais vêm apenas do ambiente.

## Kubernetes e OpenShift

Crie `devicepulse-secrets` fora do Git e execute `kubectl apply -k deploy/kubernetes/base`. A Route OpenShift usa TLS edge. Security contexts proíbem privilege escalation e os deployments aceitam o UID atribuído pela plataforma. PostgreSQL e RabbitMQ são demonstrações; produção deve usar serviços gerenciados/operadores. Migrações devem rodar em Job único no processo de release.
