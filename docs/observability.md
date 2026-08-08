# Observabilidade

A API propaga `X-Correlation-ID` em logs e resposta, e expõe `/health/live` e `/health/ready`. O profile opcional `observability` do Compose provisiona OpenTelemetry Collector, Prometheus, Grafana e Jaeger; use `docker compose --profile observability up`. A instrumentação OTLP da aplicação e métricas de negócio (ingestão, outbox, consumidor, alertas e offline) ainda precisam ser conectadas antes de dashboards operacionais completos.
