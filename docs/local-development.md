# Recursos e execução local

Este guia mostra como executar o DevicePulse completo sem Docker, durante o desenvolvimento ou, opcionalmente, com Docker Compose.

## 1. Recursos necessários

### Opção recomendada: pacote local sem Docker

- runtime .NET 8 para executar o pacote pronto;
- cerca de 150 MB livres para aplicação e banco inicial;
- 2 GB de RAM livre e 2 CPUs;
- Git, .NET SDK 8, Node.js 20 e npm somente para gerar o pacote a partir do código.

O pacote usa SQLite, serve a interface React pela própria API e executa como um único processo. Não exige PostgreSQL, Node.js ou Docker depois de publicado.

### Opção Docker Compose

- Docker Engine 24 ou Docker Desktop recente, com o plugin Compose v2 (`docker compose`);
- Git para obter e atualizar o repositório;
- 4 GB de RAM livres e 2 CPUs disponíveis como mínimo funcional;
- 8 GB de RAM livres e 4 CPUs recomendados para executar também Grafana, Prometheus, Jaeger, RabbitMQ e OpenTelemetry sem contenção;
- aproximadamente 2 GB de disco livre para o ambiente mínimo, ou 6 GB ao incluir mensageria e observabilidade;
- `curl` (ou outro cliente HTTP) para preparar e controlar o simulador.

Não é necessário instalar .NET, Node.js ou PostgreSQL nessa opção: as imagens do Compose fornecem os runtimes e serviços. Em Docker Desktop, confirme em **Settings > Resources** que memória e CPUs suficientes foram atribuídas à VM do Docker.

### Opção de desenvolvimento: processos locais

- .NET SDK 8 (não apenas o runtime), verificado com `dotnet --info`;
- Node.js 20 e npm, verificados com `node --version` e `npm --version`;
- PostgreSQL 16 acessível pela máquina somente se esse provider for escolhido;
- Git e `curl`;
- opcionalmente Docker Compose para iniciar somente PostgreSQL e os serviços auxiliares.

O frontend e a API, isoladamente, normalmente cabem em 2 GB de RAM livre e 2 CPUs. SQLite funciona sem preparação. Ao escolher PostgreSQL, ele precisa de um banco chamado `devicepulse`, um usuário com permissão para criar/alterar o schema e conectividade na porta configurada. A API aplica as migrations do Entity Framework ao iniciar, portanto essa permissão é necessária.

## 2. Portas usadas

| Porta | Componente | Necessária para |
|---:|---|---|
| `3000` | frontend Nginx no Compose | acessar a aplicação empacotada |
| `5173` | Vite | acessar o frontend em desenvolvimento |
| `5000` | API | HTTP, endpoints do simulador, health checks e SignalR |
| `5432` | PostgreSQL | somente acesso direto/local; o Compose não a publica no host |
| `15672` | RabbitMQ Management | inspeção opcional das filas |
| `9090` | Prometheus | métricas |
| `3001` | Grafana | dashboards |
| `16686` | Jaeger | traces |

Libere essas portas ou altere os mapeamentos antes de iniciar. Dentro do Compose, os containers se comunicam pelos nomes dos serviços; por isso PostgreSQL e RabbitMQ não precisam publicar suas portas para a API.

## 3. Publicar e executar sem Docker

No Linux ou macOS, gere a interface, publique a API e inicie o pacote:

```bash
./scripts/publish-local.sh
./publish/devicepulse/run.sh
```

No PowerShell:

```powershell
./scripts/publish-local.ps1
./publish/devicepulse/run.ps1
```

A aplicação completa estará em <http://localhost:5000>. O pacote é framework-dependent: a máquina de destino precisa somente do runtime ASP.NET Core 8. O arquivo `data/devicepulse.db` é criado ao lado da aplicação. Pare o processo antes de copiar esse arquivo para backup; para restaurar, substitua-o com a aplicação parada.

Para atualizar, faça backup do banco, gere ou extraia a nova publicação e preserve o diretório `data`. Para desinstalar, pare o processo e remova a pasta publicada. Defina `ASPNETCORE_URLS` antes de iniciar para mudar a porta.

Também é possível iniciar diretamente do código com SQLite:

```bash
dotnet run --project backend/DevicePulse.Api
```

## 4. Subir o ambiente opcional com Docker Compose

Na raiz do repositório:

```bash
cp .env.example .env
```

Edite `.env` e substitua todos os valores de demonstração. Para uso local, gere valores aleatórios, por exemplo com `openssl rand -base64 32`. O arquivo `.env` não deve ser commitado. As variáveis efetivamente exigidas pelo Compose são:

- `POSTGRES_PASSWORD`: senha do usuário `devicepulse` no PostgreSQL;
- `RABBITMQ_PASSWORD`: senha do usuário `devicepulse` no RabbitMQ;
- `GRAFANA_PASSWORD`: senha do usuário administrador do Grafana.

`JWT_SIGNING_KEY` e `DEVICEPULSE_SEED_ADMIN_PASSWORD` estão reservadas para integrações descritas na arquitetura e não são consumidas pelo fluxo legado atual.

Valide e inicie:

```bash
docker compose config
docker compose up --build -d
docker compose ps
docker compose logs -f api frontend
```

Esse modo mínimo inicia PostgreSQL, API e frontend. Para incluir os serviços ainda opcionais:

```bash
docker compose --profile messaging --profile observability up --build -d
```

Espere `postgres` e `api` ficarem `healthy`. Se ativar `messaging`, espere também o `rabbitmq`. Em seguida, valide:

```bash
curl --fail http://localhost:5000/health/live
curl --fail http://localhost:5000/health/ready
curl --fail http://localhost:3000/health
```

A aplicação estará em <http://localhost:3000>. A API do Compose usa `Production`, portanto não publica Swagger. Para consultar a API, use [`devicepulse.http`](devicepulse.http), `curl` ou rode a API localmente em `Development` conforme a próxima seção.

Para parar sem apagar dados:

```bash
docker compose down
```

Para apagar também o banco local e recomeçar vazio (operação destrutiva):

```bash
docker compose down --volumes
```

## 5. Rodar API e frontend em modo de desenvolvimento

### 5.1 Escolher SQLite ou PostgreSQL

SQLite é o padrão e não exige preparação. Para usar PostgreSQL, crie o usuário e banco usando sua instalação local ou inicie somente o container após configurar `.env`:

```bash
docker compose up -d postgres
```

O Compose não publica `5432` no host. Para conectar uma API executada na máquina a esse banco, adicione temporariamente `ports: ["5432:5432"]` ao serviço `postgres`, ou use uma instalação PostgreSQL local. Não mantenha uma porta de banco desnecessariamente exposta.

Configure a connection string sem gravar a senha no repositório:

```bash
export ConnectionStrings__DevicePulse='Host=localhost;Port=5432;Database=devicepulse;Username=devicepulse;Password=<sua-senha>'
export Database__Provider=Postgres
export ASPNETCORE_ENVIRONMENT=Development
```

### 5.2 Iniciar a API

Em um terminal, na raiz:

```bash
dotnet restore backend/DevicePulse.sln
dotnet run --project backend/DevicePulse.Api --launch-profile http
```

A API aplicará as migrations e ouvirá em <http://localhost:5000>. Confira <http://localhost:5000/swagger>, `curl --fail http://localhost:5000/health/ready` e os logs do terminal. Se a inicialização falhar antes de abrir a porta, verifique primeiro a connection string, as credenciais, a disponibilidade do PostgreSQL e a permissão do usuário no schema.

### 5.3 Iniciar o frontend

Em outro terminal:

```bash
cd frontend
npm install
npm run dev
```

Abra <http://localhost:5173>. O Vite encaminha `/api` e o WebSocket `/hubs` para `http://localhost:5000`; a API precisa estar nessa porta para o frontend funcionar sem configuração adicional.

## 6. Preparar e rodar o simulador (Autopilot)

O simulador é um `BackgroundService` dentro da API, não um executável separado. Ele começa **parado**, usa todos os equipamentos cadastrados e, por padrão, gera uma leitura automática para cada equipamento a cada 10 segundos. O intervalo pode ser alterado antes de iniciar a API:

```bash
export Autopilot__IntervalSeconds=5
```

O valor mínimo efetivo é 1 segundo. No Compose, adicione `Autopilot__IntervalSeconds` ao bloco `environment` do serviço `api` se quiser sobrescrever o padrão.

### 6.1 Cadastrar ao menos um equipamento

Com a API pronta:

```bash
curl --fail-with-body \
  --request POST http://localhost:5000/api/equipments \
  --header 'Content-Type: application/json' \
  --header 'X-Correlation-ID: setup-simulator' \
  --data '{"name":"Sensor de demonstração","minimumValue":0,"maximumValue":100,"currentValue":25}'
```

O nome deve ser único, o mínimo deve ser menor que o máximo e o valor atual deve estar no intervalo. Sem equipamentos, o Autopilot pode ficar ativo, mas não produzirá leituras.

### 6.2 Iniciar e acompanhar

```bash
curl --fail-with-body --request POST http://localhost:5000/api/autopilot/start
curl --fail-with-body http://localhost:5000/api/autopilot
```

A resposta de status informa `isRunning`, `intervalSeconds` e `lastRunAtUtc`. Aguarde pelo menos um intervalo e valide os contadores:

```bash
curl --fail-with-body http://localhost:5000/api/dashboard/summary
curl --fail-with-body http://localhost:5000/api/equipments
```

O campo de leituras automáticas no dashboard deve crescer. No frontend, o painel atualiza a consulta periodicamente. Os clientes conectados ao hub `/hubs/device-updates` recebem as atualizações em tempo real.

### 6.3 Parar

```bash
curl --fail-with-body --request POST http://localhost:5000/api/autopilot/stop
curl --fail-with-body http://localhost:5000/api/autopilot
```

O estado do Autopilot fica apenas em memória: reiniciar a API o deixa parado novamente. Equipamentos e leituras permanecem no SQLite ou PostgreSQL configurado.

## 7. Diagnóstico rápido

- **API não inicia:** confira a seleção de provider e a connection string; no Compose, verifique `docker compose logs api postgres` e o health check do PostgreSQL.
- **Frontend mostra erro ao carregar:** teste `http://localhost:5000/health/ready`; no desenvolvimento, confirme que a API está na porta `5000`.
- **Autopilot ativo sem novas leituras:** confirme que existe ao menos um equipamento, aguarde o intervalo completo e veja `docker compose logs api`.
- **Porta em uso:** identifique o processo com `ss -ltnp` (Linux) ou altere o mapeamento/URL correspondente.
- **Dados antigos ou credenciais divergentes:** a senha inicial do PostgreSQL fica no volume. Se os dados puderem ser descartados, execute `docker compose down --volumes` e suba novamente.
- **Recursos insuficientes:** pare observabilidade opcional ou aumente CPUs/memória destinadas ao Docker; API, frontend e PostgreSQL são os componentes indispensáveis ao fluxo principal e ao simulador.
