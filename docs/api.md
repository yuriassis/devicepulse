# API

O contrato legado permanece em `/api/equipments`, `/api/alerts`, `/api/dashboard` e `/api/autopilot`. Swagger descreve requests, respostas e erros. Novos consumidores devem enviar `X-Correlation-ID`; quando ausente a API gera e devolve um identificador. O hub está em `/hubs/device-updates`. A versão `/api/v1`, paginação uniforme, autenticação e endpoints administrativos permanecem como trabalho de integração declarado.

Veja exemplos executáveis em [`devicepulse.http`](devicepulse.http).
