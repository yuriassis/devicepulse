# Modelo de domínio

`Organization` é a fronteira de tenant. `Site` pertence à organização e `Area` ao site. `Device` valida nome, intervalo do simulador e timeout, registra telemetria em UTC e calcula offline. `AlertRule` implementa os sete operadores e valida intervalos. `AlertOccurrence` separa reconhecimento de resolução. `SimulationProfile` contém seis modos e `SimulationSession` modela execução, pausa e parada.

```mermaid
erDiagram
 ORGANIZATION ||--o{ SITE : owns
 SITE ||--o{ AREA : contains
 ORGANIZATION ||--o{ DEVICE : owns
 AREA o|--o{ DEVICE : locates
 DEVICE ||--o{ TELEMETRY_READING : receives
 DEVICE ||--o{ ALERT_RULE : has
 ALERT_RULE ||--o{ ALERT_OCCURRENCE : opens
 DEVICE ||--o{ SIMULATION_PROFILE : has
 SIMULATION_PROFILE ||--o{ SIMULATION_SESSION : runs
```
