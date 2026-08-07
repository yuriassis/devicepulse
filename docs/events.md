# Eventos

Todos os eventos possuem `EventId`, `OrganizationId`, `CorrelationId`, `OccurredAtUtc` e `Version`. Os contratos são `TelemetryReadingCreated`, `AlertTriggered`, `AlertUpdated`, `AlertAcknowledged`, `AlertResolved` e `DeviceStatusChanged`.

O fluxo alvo usa uma outbox transacional e consumidor com inbox idempotente, exponential backoff e dead-letter exchange. O código atual define os contratos, mas ainda não contém publisher ou consumer; o Compose disponibiliza RabbitMQ para o próximo incremento sem afirmar que existe processamento assíncrono.
