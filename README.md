# DevicePulse

API para cadastro e observabilidade de equipamentos, com histórico de leituras e
um resumo operacional para dashboards.

## Funcionalidades disponíveis

- criação, consulta, atualização e exclusão de equipamentos;
- cadastro, listagem e exclusão de alertas vinculados a equipamentos;
- acionamento de alertas pela comparação da leitura atual com o intervalo próprio do alerta;
- validação de nome, limites e valores numéricos;
- proteção contra nomes de equipamentos duplicados;
- registro do valor inicial de cada equipamento no histórico;
- registro de leituras com origem `Manual` ou `Autopilot`;
- consulta das 1 a 100 leituras mais recentes de um equipamento;
- resumo com quantidades de equipamentos e leituras por origem;
- painel web responsivo com indicadores e estado operacional dos equipamentos;
- cadastro, edição e exclusão de equipamentos diretamente pela interface;
- registro manual e consulta das 100 leituras mais recentes pela interface;
- piloto automático configurável, com geração periódica dentro dos limites de cada equipamento;
- controles de início e parada do piloto automático no painel;
- estados visuais de carregamento, conteúdo vazio, sucesso e erro;
- respostas de erro padronizadas para validações e conflitos;
- Swagger UI no ambiente de desenvolvimento;
- testes de integração dos fluxos HTTP com banco isolado;
- pipeline de CI para restore, build, testes e coleta de cobertura;
- execução local e publicação usando apenas o SDK do .NET, sem Docker;
- persistência local em um banco SQLite criado automaticamente.

## Endpoints da API

| Método | Rota | Descrição |
| --- | --- | --- |
| `POST` | `/api/equipments` | Cadastra um equipamento |
| `GET` | `/api/equipments` | Lista os equipamentos por nome |
| `GET` | `/api/equipments/{id}` | Consulta um equipamento |
| `PUT` | `/api/equipments/{id}` | Atualiza nome e limites |
| `DELETE` | `/api/equipments/{id}` | Exclui o equipamento e suas leituras |
| `POST` | `/api/equipments/{id}/readings` | Registra uma leitura |
| `GET` | `/api/equipments/{id}/readings?limit=50` | Consulta as leituras mais recentes |
| `POST` | `/api/alerts` | Cadastra um alerta com nome, equipamento e intervalo |
| `GET` | `/api/alerts` | Lista alertas e informa se cada um está acionado |
| `DELETE` | `/api/alerts/{id}` | Exclui um alerta |
| `GET` | `/api/dashboard/summary` | Obtém os totais do dashboard |
| `GET` | `/api/autopilot` | Consulta o estado e o intervalo do piloto automático |
| `POST` | `/api/autopilot/start` | Inicia a geração periódica de leituras |
| `POST` | `/api/autopilot/stop` | Interrompe a geração periódica de leituras |

## Tecnologias

- .NET 8 e ASP.NET Core Web API;
- Entity Framework Core 8;
- SQLite;
- Swagger/OpenAPI;
- HTML, CSS e JavaScript sem dependências externas;
- xUnit e SQLite em memória nos testes.

## Como executar

### Pré-requisitos

- [.NET SDK 8](https://dotnet.microsoft.com/download/dotnet/8.0)

### API

```bash
dotnet restore backend/DevicePulse.sln
dotnet run --project backend/DevicePulse.Api
```

A aplicação estará disponível em `http://localhost:5000`. Não é necessário
instalar ou iniciar um servidor de banco de dados: o SQLite cria o arquivo
`devicepulse.db` automaticamente no diretório a partir do qual o comando é
executado. As migrações também são aplicadas automaticamente ao iniciar.

Os endereços locais e o ambiente podem ser ajustados em
`backend/DevicePulse.Api/Properties/launchSettings.json`. Em desenvolvimento, a
interface web fica disponível na raiz (`/`) e a documentação interativa fica
disponível em `/swagger`.

O intervalo do piloto automático é definido, em segundos, pela configuração
`Autopilot:IntervalSeconds` (10 segundos por padrão). O piloto inicia parado e
pode ser controlado pelo painel ou pelos endpoints da API.

### Testes

```bash
dotnet test backend/DevicePulse.sln
```

Os testes incluem unidades dos serviços e cenários de integração que inicializam
a aplicação completa, aplicam as migrações em um banco temporário e exercitam os
endpoints HTTP. A automação em `.github/workflows/ci.yml` executa restore, build,
testes e coleta de cobertura em pushes e pull requests.

O projeto de testes centraliza a importação do xUnit em `GlobalUsings.cs`. Os
testes HTTP também compartilham as mesmas regras de serialização de enums usadas
pela API, evitando diferenças entre o cliente de teste e o contrato publicado.

## Publicação sem Docker

Para gerar os artefatos otimizados que podem ser executados sem o código-fonte,
use:

```bash
dotnet publish backend/DevicePulse.Api/DevicePulse.Api.csproj \
  --configuration Release \
  --output ./publish
```

Execute a aplicação publicada diretamente com o runtime do .NET 8:

```bash
cd publish
dotnet DevicePulse.Api.dll
```

Por padrão, o painel publicado escuta em `http://localhost:5000` e o banco fica
em `publish/devicepulse.db`. Mantenha esse arquivo em um diretório persistente e
inclua-o na rotina de backups. Para escolher outro endereço, porta ou local para
o banco, defina as variáveis de ambiente descritas abaixo antes de executar a
DLL.

### Configuração de produção

As opções do ASP.NET Core podem ser sobrescritas por variáveis de ambiente com
dois sublinhados como separador. As principais opções são:

| Variável | Padrão | Finalidade |
| --- | --- | --- |
| `ConnectionStrings__DevicePulse` | `Data Source=devicepulse.db;Default Timeout=30` | Caminho e opções do banco SQLite |
| `Autopilot__IntervalSeconds` | `10` | Intervalo, em segundos, entre ciclos automáticos |
| `ASPNETCORE_URLS` | `http://localhost:5000` | Endereço e porta HTTP da aplicação publicada |

Exemplo para disponibilizar a aplicação na porta `8080` e salvar os dados em
`/var/lib/devicepulse` no Linux:

```bash
mkdir -p /var/lib/devicepulse
ASPNETCORE_URLS=http://0.0.0.0:8080 \
ConnectionStrings__DevicePulse='Data Source=/var/lib/devicepulse/devicepulse.db;Default Timeout=30' \
dotnet ./publish/DevicePulse.Api.dll
```

As migrações são aplicadas automaticamente na inicialização. Em produção,
proteja o arquivo persistente com backups e não execute mais de uma instância
contra o mesmo arquivo SQLite. Para escalar horizontalmente, a persistência deve
ser migrada para um banco de dados compartilhado. Um gerenciador de serviços do
sistema operacional, como o `systemd` no Linux, pode manter o processo ativo e
reiniciá-lo quando necessário.

## Estrutura do repositório

```text
backend/
├── DevicePulse.Api/      # API, domínio, persistência e interface web em wwwroot
├── DevicePulse.Tests/    # testes unitários dos serviços e de integração HTTP
└── DevicePulse.sln       # solução .NET
```

Na raiz, `.github/workflows/ci.yml` mantém as verificações automatizadas. O
arquivo SQLite gerado localmente é ignorado pelo Git.

## Licença

Este projeto é distribuído sob os termos da [licença MIT](LICENSE).
