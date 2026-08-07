# DevicePulse

API para cadastro e observabilidade de equipamentos, com histórico de leituras e
um resumo operacional para dashboards.

## Situação atual

> **Última atualização do planejamento:** 7 de agosto de 2026
>
> **Etapa atual:** 4 — Interface web
>
> **Próxima entrega:** dashboard web consumindo a API existente

O backend do MVP está funcional em .NET 8 e persiste os dados em SQLite. Ele já
permite gerenciar equipamentos, registrar leituras manuais ou de piloto
automático, consultar o histórico e obter os totais usados pelo dashboard.

## Planejamento

A lista abaixo é a fonte de verdade do andamento do projeto. Uma etapa só deve
ser marcada como concluída depois que seus critérios de conclusão forem
atendidos e as verificações relacionadas tiverem sido executadas.

| Etapa | Estado | Entrega | Critério de conclusão |
| --- | --- | --- | --- |
| 1. Fundação do backend | ✅ Concluída | Solução .NET 8, API, SQLite, Entity Framework Core, migração inicial, Swagger e tratamento centralizado de erros | A aplicação compila, cria/atualiza o banco e expõe a documentação em desenvolvimento |
| 2. Gestão de equipamentos | ✅ Concluída | Cadastro, listagem, consulta, edição e exclusão de equipamentos, com validações de domínio | Operações CRUD cobertas por testes automatizados e nomes duplicados rejeitados |
| 3. Leituras e resumo operacional | ✅ Concluída | Registro e histórico de leituras, identificação da origem e resumo agregado para o dashboard | Serviços de leituras e resumo cobertos por testes automatizados |
| 4. Interface web | 🚧 Em andamento | Dashboard responsivo, listagem e formulários de equipamentos e visualização do histórico | Fluxos principais podem ser executados pela interface e possuem estados de carregamento, vazio e erro |
| 5. Piloto automático | ⏳ Planejada | Geração periódica de leituras automáticas e controles para ativar ou interromper a simulação | Leituras são geradas sem intervenção manual, persistidas com a origem correta e refletidas no dashboard |
| 6. Qualidade e entrega | ⏳ Planejada | Testes de integração, automação de CI, documentação de implantação e empacotamento da aplicação | Pipeline reproduzível executa build e testes, e o sistema pode ser implantado seguindo a documentação |

### Registro de conclusão das etapas

| Data | Etapa | Resultado |
| --- | --- | --- |
| 6 de agosto de 2026 | 1. Fundação do backend | Estrutura da solução, persistência SQLite, migração inicial e infraestrutura HTTP concluídas |
| 6 de agosto de 2026 | 2. Gestão de equipamentos | CRUD e validações de equipamentos concluídos |
| 7 de agosto de 2026 | 3. Leituras e resumo operacional | Histórico de leituras e endpoint de resumo concluídos |

### Como manter este planejamento atualizado

Ao concluir cada etapa, a mesma alteração deve:

1. trocar seu estado para `✅ Concluída` na tabela de planejamento;
2. mover o marcador **Etapa atual** para a próxima etapa ainda não concluída;
3. ajustar a **Próxima entrega** para o próximo resultado esperado;
4. atualizar a data no topo desta seção;
5. adicionar uma linha ao registro de conclusão, resumindo o resultado entregue;
6. confirmar os critérios de conclusão e registrar, no pull request, os comandos
   de build e testes executados;
7. revisar as seções de funcionalidades, endpoints e execução caso o uso do
   projeto tenha mudado.

Se o escopo de uma etapa mudar antes da conclusão, sua entrega e seus critérios
devem ser atualizados na tabela, sem marcá-la como concluída antecipadamente.

## Funcionalidades disponíveis

- criação, consulta, atualização e exclusão de equipamentos;
- validação de nome, limites e valores numéricos;
- proteção contra nomes de equipamentos duplicados;
- registro do valor inicial de cada equipamento no histórico;
- registro de leituras com origem `Manual` ou `Autopilot`;
- consulta das 1 a 100 leituras mais recentes de um equipamento;
- resumo com quantidades de equipamentos e leituras por origem;
- respostas de erro padronizadas para validações e conflitos;
- Swagger UI no ambiente de desenvolvimento.

## Endpoints do MVP

| Método | Rota | Descrição |
| --- | --- | --- |
| `POST` | `/api/equipments` | Cadastra um equipamento |
| `GET` | `/api/equipments` | Lista os equipamentos por nome |
| `GET` | `/api/equipments/{id}` | Consulta um equipamento |
| `PUT` | `/api/equipments/{id}` | Atualiza nome e limites |
| `DELETE` | `/api/equipments/{id}` | Exclui o equipamento e suas leituras |
| `POST` | `/api/equipments/{id}/readings` | Registra uma leitura |
| `GET` | `/api/equipments/{id}/readings?limit=50` | Consulta as leituras mais recentes |
| `GET` | `/api/dashboard/summary` | Obtém os totais do dashboard |

## Tecnologias

- .NET 8 e ASP.NET Core Web API;
- Entity Framework Core 8;
- SQLite;
- Swagger/OpenAPI;
- xUnit e SQLite em memória nos testes.

## Como executar

### Pré-requisitos

- [.NET SDK 8](https://dotnet.microsoft.com/download/dotnet/8.0)

### API

```bash
dotnet restore backend/DevicePulse.sln
dotnet run --project backend/DevicePulse.Api
```

A aplicação aplica automaticamente as migrações ao iniciar. Os endereços locais
e o ambiente podem ser ajustados em
`backend/DevicePulse.Api/Properties/launchSettings.json`. Em desenvolvimento, a
documentação interativa fica disponível em `/swagger`.

### Testes

```bash
dotnet test backend/DevicePulse.sln
```

## Estrutura do repositório

```text
backend/
├── DevicePulse.Api/      # API, domínio, serviços, persistência e migrações
├── DevicePulse.Tests/    # testes automatizados dos serviços
└── DevicePulse.sln       # solução .NET
```

## Licença

Este projeto é distribuído sob os termos da [licença MIT](LICENSE).
