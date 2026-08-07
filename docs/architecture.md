# Arquitetura

## Decisões

O desenho alvo é um monólito modular: API, aplicação e persistência são implantados juntos; simulação e alertas assíncronos podem escalar como workers. O domínio puro protege invariantes sem depender da infraestrutura. PostgreSQL é o system of record; RabbitMQ transporta eventos, nunca telemetria permanente. SignalR é a única via de push ao navegador e não usa Redis backplane.

```mermaid
sequenceDiagram
  participant Client
  participant API
  participant DB as PostgreSQL
  participant Outbox
  participant MQ as RabbitMQ
  Client->>API: POST telemetry (MessageId)
  API->>DB: reading + current value
  API->>Outbox: event (same transaction)
  API-->>Client: accepted/duplicate
  Outbox->>MQ: TelemetryReadingCreated
```

## Estado entregue e próximos incrementos

O domínio final e contratos de evento estão implementados, a API usa PostgreSQL em execução normal e SQLite somente na suíte legada, e SignalR/health checks/correlation ID estão expostos. Ainda faltam integrar Identity/JWT, persistir outbox, publicar/consumir RabbitMQ, implementar os workers e substituir os endpoints legados pelos módulos completos. O Compose não declara workers enquanto não houver executáveis reais; isso evita uma implantação enganosa.
