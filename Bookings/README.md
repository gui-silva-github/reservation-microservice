# BookingsService — Guia arquivo por arquivo (para iniciantes)

Este documento explica **cada arquivo** do microsserviço de reservas em C# (.NET 8), com atenção especial ao **Redis**.

## Visão geral em 1 minuto

```
Cliente HTTP
    │
    ▼
API  (Controllers, JWT, Middleware)     ← recebe a requisição
    │
    ▼
BLL  (Services, Validators, Redis, HTTP) ← regras de negócio
    │
    ▼
DAL  (MongoDB Repository)               ← grava / lê no banco
```

| Camada | Pasta | Responsabilidade |
|--------|-------|------------------|
| **API** | `API/` | Endpoints HTTP, autenticação JWT, Swagger |
| **BLL** | `BLL/` | Regras de negócio, validação, Redis, chamadas a Users/Spaces |
| **DAL** | `DAL/` | Persistência no MongoDB |

**Fluxo ao criar uma reserva:**

1. `POST /api/bookings` chega no controller.
2. `BookingService` valida dados, usuário e espaço.
3. Verifica se já existe reserva confirmada no mesmo horário.
4. Salva no **MongoDB**.
5. Publica um evento na fila **Redis** (`booking-events`).
6. O **NotificationsService** (outro microsserviço) lê essa fila e cria a notificação.

---

## Conceitos C# que você vai ver o tempo todo

| Conceito | O que é |
|----------|---------|
| `namespace` | “Pasta lógica” do código (`Reservation.BookingsService.BLL...`) |
| `interface` | Contrato: define *o que* uma classe deve fazer, sem implementar |
| `class` | Implementação concreta do contrato |
| `record` | Tipo imutável (ótimo para DTOs / dados de entrada e saída) |
| `async` / `await` | Código assíncrono: não trava a thread enquanto espera I/O |
| `Task` / `Task<T>` | Representa uma operação assíncrona |
| `CancellationToken` | Permite cancelar uma operação (ex.: cliente desconectou) |
| DI (Injeção de Dependência) | O framework cria e entrega as classes que você precisa |
| `ILogger<T>` | Log tipado (Information, Warning, Error) |

---

# Redis — o essencial para este projeto

## O que é Redis?

Redis é um banco **em memória**, muito rápido. Aqui ele **não** guarda reservas (isso é MongoDB).  
Aqui ele funciona como uma **fila de mensagens** entre microsserviços.

Analogia:

- MongoDB = caderno permanente das reservas  
- Redis = bandeja de papéis entre a cozinha (Bookings) e o garçom (Notifications)

## Lista como fila (List)

Usamos comandos de **lista**:

| Comando | O que faz | Quem usa |
|---------|-----------|----------|
| `LPUSH` (`ListLeftPushAsync`) | Coloca o item **no início** da lista | Bookings (produtor) |
| `RPOP` (`ListRightPopAsync`) | Tira o item **do fim** da lista | Notifications (consumidor) |

```
LPUSH  →  [evento3, evento2, evento1]  →  RPOP tira evento1 (FIFO)
```

Chave da fila: `"booking-events"` (definida em `RedisQueueConstants`).

## Por que não Pub/Sub?

- **Pub/Sub**: se ninguém estiver ouvindo no momento, a mensagem **some**.
- **Lista (fila)**: a mensagem fica guardada até alguém consumir.

Para notificações, fila é mais segura.

## Fire-and-forget

Depois de salvar a reserva, o serviço publica o evento em uma `Task.Run` **sem esperar** o resultado na resposta HTTP:

- A API responde rápido (`201 Created`).
- Se o Redis falhar, a reserva **já foi salva**; só a notificação pode falhar (o erro vai para o log).

Isso é simples para aprendizado. Em produção costuma-se usar **Outbox Pattern** (garantia forte).

## Como o evento se parece (JSON)

```json
{
  "eventType": "BookingCreated",
  "bookingId": "...",
  "userId": "...",
  "spaceId": "...",
  "startDate": "...",
  "endDate": "..."
}
```

Serializado em **camelCase** pelo `RedisEventPublisher`.

## Configuração

Em `appsettings.json`:

```json
"ConnectionStrings": {
  "Redis": "localhost:6379"
}
```

`ConnectionMultiplexer` (StackExchange.Redis) é registrado como **Singleton**: uma conexão compartilhada por toda a aplicação.

---

# Estrutura de pastas

```
Bookings/
├── BookingsService.sln
├── README.md                          ← este arquivo
├── API/
│   ├── Program.cs
│   ├── BookingsService.API.csproj
│   ├── Dockerfile
│   ├── appsettings.json
│   ├── appsettings.Development.json
│   ├── Controllers/BookingsController.cs
│   ├── Middleware/ExceptionHandlingMiddleware.cs
│   └── Properties/launchSettings.json
├── BLL/
│   ├── DependencyInjection.cs
│   ├── BookingsService.BLL.csproj
│   ├── Configuration/JwtSettings.cs
│   ├── Constants/RedisQueueConstants.cs
│   ├── DTO/                           ← contratos de entrada/saída
│   ├── Exceptions/
│   ├── HttpClients/                   ← chama Users e Spaces
│   ├── Mappers/BookingMappingProfiles.cs
│   ├── Messaging/                     ← Redis
│   ├── Options/
│   ├── Services/
│   └── Validators/
└── DAL/
    ├── DependencyInjection.cs
    ├── BookingsService.DAL.csproj
    ├── Configuration/MongoDbSettings.cs
    ├── Constants/MongoDbConstants.cs
    ├── Context/MongoDbContext.cs
    ├── Entities/Booking.cs
    ├── Enums/BookingStatus.cs
    └── Repositories/
```

---

# API — camada HTTP

## `API/Program.cs`

**Ponto de entrada** da aplicação. Ordem importante:

1. `AddDataAccessLayer` → MongoDB + repositório  
2. `AddBusinessLogicLayer` → serviços, Redis, HttpClients, validators  
3. Configura **JWT** (issuer, audience, secret)  
4. Controllers + enums como string no JSON  
5. CORS para o Angular (`localhost:4200`)  
6. Pipeline: middleware de exceção → Swagger → HTTPS → CORS → Auth → Controllers  

Erros comuns que já corrigimos aqui:

- `AddBusinnessLogicLayer` → `AddBusinessLogicLayer`
- `ConfigureApiBehavior` → `ConfigureApiBehaviorOptions`
- `app.useAuthorization()` → `app.UseAuthorization()` (C# é case-sensitive)

## `API/Controllers/BookingsController.cs`

Expõe as rotas REST. O controller é **magro**: só recebe HTTP e chama o serviço.

| Método | Rota | Auth | Ação |
|--------|------|------|------|
| GET | `/api/bookings` | Anônimo | Lista todas |
| GET | `/api/bookings/{id}` | Anônimo | Busca por id |
| GET | `/api/bookings/search/userid/{userId}` | Anônimo | Por usuário |
| GET | `/api/bookings/search/spaceid/{spaceId}` | Anônimo | Por espaço |
| GET | `/api/bookings/search/date/{date}` | Anônimo | Por dia |
| POST | `/api/bookings` | JWT | Cria |
| PUT | `/api/bookings/{id}` | JWT | Atualiza |
| DELETE | `/api/bookings/{id}` | JWT | Cancela (soft) |

Atributos úteis:

- `[Authorize]` na classe → exige token por padrão  
- `[AllowAnonymous]` nos GETs → leitura pública  
- `[FromBody]` → lê JSON do corpo  
- `CreatedAtAction` → retorna `201` + header `Location`

## `API/Middleware/ExceptionHandlingMiddleware.cs`

Middleware = código que roda **em volta** de toda requisição.

Método obrigatório: `InvokeAsync` (não `InvokeTask`).

Mapeia exceções → status HTTP:

| Exceção | Status |
|---------|--------|
| `ValidationException` | 400 |
| `InvalidReferenceException` | 400 |
| `BookingConflictException` | 409 |
| `NotFoundException` | 404 |
| `ExternalServiceUnavailableException` | 503 |
| `BusinessException` | 400 |
| Outras | 500 |

Resposta padronizada: `ApiErrorResponse`.

## `API/appsettings.json`

Configuração em runtime:

- `ConnectionStrings:MongoConnection` → MongoDB  
- `ConnectionStrings:Redis` → Redis  
- `MongoDb:DatabaseName` → `ReservationBookings`  
- `UsersServiceUrl` / `SpacesServiceUrl` → URLs dos outros serviços  
- `Jwt` → chave, issuer e audience (devem bater com o UsersService)

## `API/appsettings.Development.json`

Sobrescreve configs só em ambiente Development.

## `API/Properties/launchSettings.json`

Portas locais: `http://localhost:5094` e `https://localhost:7094`.  
Abre o Swagger automaticamente.

## `API/BookingsService.API.csproj`

Projeto Web. Referencia o BLL. Pacotes: JWT Bearer + Swashbuckle (Swagger).

## `API/Dockerfile`

Imagem Docker em 2 estágios (build + runtime).  
Contexto de build: pasta `Bookings/` (pai de `API/`, `BLL/`, `DAL/`).

---

# BLL — regras de negócio

## `BLL/DependencyInjection.cs`

Método de extensão `AddBusinessLogicLayer` registra tudo da BLL no container DI:

| Registro | Por quê |
|----------|---------|
| `UsersServiceOptions` / `SpacesServiceOptions` | URLs dos serviços |
| `IConnectionMultiplexer` (Singleton) | Conexão Redis compartilhada |
| AutoMapper | Entity ↔ DTO |
| FluentValidation | Validadores |
| `AddHttpClient<...>` | HttpClients tipados + timeout 10s |
| `IEventPublisher` → `RedisEventPublisher` | Publicar eventos |
| `IBookingService` → `BookingService` (Scoped) | 1 instância por requisição HTTP |

**Atenção de naming:** a classe do serviço se chama `BookingService` (singular), **não** `BookingsService`, porque `BookingsService` já é parte do namespace (`Reservation.BookingsService`) e o C# confundiria tipo com namespace.

## `BLL/Services/IBookingService.cs`

Interface (contrato) do serviço. O controller depende da **interface**, não da classe concreta → facilita testes e troca de implementação.

## `BLL/Services/BookingService.cs`

Coração do negócio.

### Create

1. Valida request (FluentValidation)  
2. Normaliza datas para UTC  
3. Confirma que User existe (HTTP → UsersService)  
4. Confirma que Space existe e `IsActive` (HTTP → SpacesService)  
5. Verifica conflito de horário no MongoDB  
6. Gera `Guid`, status `Confirmed`, `CreatedAt`  
7. Salva  
8. Publica evento Redis (fire-and-forget)

### Update

- Não atualiza reserva já cancelada  
- Só checa conflito se o novo status for `Confirmed`  
- Preserva `CreatedAt`

### Cancel

- Soft delete: só muda status para `Cancelled`  
- Não apaga o documento do MongoDB

### Helpers

- `NormalizeToUtc` — evita bugs de fuso horário  
- `ValidateAsync` — lança `ValidationException` se inválido  
- `PublishBookingCreatedFireAndForget` — Redis em background

## `BLL/Messaging/IEventPublisher.cs`

Abstração: “quero publicar um evento”, sem saber se é Redis, RabbitMQ, etc.

## `BLL/Messaging/RedisEventPublisher.cs` ⭐ Redis

```csharp
// 1) Transforma o evento em JSON (camelCase)
string payload = JsonSerializer.Serialize(bookingCreatedEvent, JsonOptions);

// 2) Pega um "database" lógico do Redis (por padrão 0)
IDatabase database = _connectionMultiplexer.GetDatabase();

// 3) Empilha na lista "booking-events"
await database.ListLeftPushAsync(RedisQueueConstants.BookingEventsQueue, payload)
    .WaitAsync(cancellationToken);
```

Passo a passo mental:

1. Você já tem a reserva salva no Mongo.  
2. Monta um “aviso” (`BookingCreatedEvent`).  
3. Converte para texto JSON.  
4. Empurra esse texto para a lista Redis.  
5. Outro serviço (Notifications) vai tirar esse texto da lista e processar.

`WaitAsync(cancellationToken)` permite cancelar se a requisição for abortada **enquanto** a publicação ainda está ligada a um token (no fire-and-forget usamos o token padrão).

## `BLL/Constants/RedisQueueConstants.cs`

```csharp
BookingEventsQueue = "booking-events"
BookingCreatedEventType = "BookingCreated"
```

Constantes evitam “magic strings” espalhadas e typos entre produtor e consumidor.

## `BLL/DTO/` — Data Transfer Objects

Objetos que **entram e saem** da API (não misturar com a entidade Mongo).

| Arquivo | Uso |
|---------|-----|
| `CreateBookingRequest.cs` | Body do POST |
| `UpdateBookingRequest.cs` | Body do PUT (inclui `Status`) |
| `BookingResponse.cs` | Resposta ao cliente (inclui `SpaceId`) |
| `BookingCreatedEvent.cs` | Payload publicado no Redis |
| `UserReferenceResponse.cs` | JSON vindo do UsersService |
| `SpaceReferenceResponse.cs` | JSON vindo do SpacesService |
| `ApiErrorResponse.cs` | Formato padrão de erro |

`record` = imutável, comparação por valor, sintaxe curta.

## `BLL/Validators/`

FluentValidation: regras declarativas.

- `CreateBookingRequestValidator` — UserId/SpaceId/datas; End > Start  
- `UpdateBookingRequestValidator` — igual + status válido e **não** `Cancelled` (cancelamento é via DELETE)

## `BLL/Mappers/BookingMappingProfiles.cs`

AutoMapper: converte automaticamente:

- `Booking` → `BookingResponse` (enum vira string)  
- `CreateBookingRequest` → `Booking`  
- `UpdateBookingRequest` → `Booking`

`Ignore()` em `Id`/`CreatedAt` porque o serviço preenche esses campos.

## `BLL/HttpClients/`

Clientes HTTP tipados para outros microsserviços.

| Cliente | Endpoint | Se 404 | Se erro de rede / 5xx |
|---------|----------|--------|------------------------|
| `UsersHttpClient` | `GET api/Users/{id}` | retorna `null` | `ExternalServiceUnavailableException` (503) |
| `SpacesHttpClient` | `GET api/spaces/{id}` | retorna `null` | idem |

Timeout: 10 segundos (registrado no DI).

## `BLL/Options/`

Classes de configuração:

- `UsersServiceOptions.BaseUrl`  
- `SpacesServiceOptions.BaseUrl`

Preenchidas a partir de `UsersServiceUrl` / `SpacesServiceUrl` no `appsettings.json`.

## `BLL/Configuration/JwtSettings.cs`

Lê a seção `"Jwt"`: `SecretKey`, `Issuer`, `Audience`.

## `BLL/Exceptions/`

Hierarquia de erros de domínio (capturados pelo middleware):

| Classe | Significado |
|--------|-------------|
| `BusinessException` | Regra de negócio genérica |
| `NotFoundException` | Recurso não existe |
| `BookingConflictException` | Sobreposição de horário |
| `InvalidReferenceException` | User/Space inválido ou inativo |
| `ExternalServiceUnavailableException` | Users/Spaces fora do ar |

## `BLL/BookingsService.BLL.csproj`

Referencia o DAL. Pacotes: AutoMapper, FluentValidation, Http, **StackExchange.Redis**.

---

# DAL — acesso a dados (MongoDB)

## `DAL/DependencyInjection.cs`

- Registra `Guid` como string no BSON (mais legível no Mongo)  
- `IMongoClient` Singleton  
- `MongoDbContext` Singleton  
- `IBookingRepository` Scoped

## `DAL/Context/MongoDbContext.cs`

Atalho para a collection:

```csharp
Bookings = database.GetCollection<Booking>("bookings");
```

## `DAL/Entities/Booking.cs`

Documento Mongo:

| Campo | Tipo | Nota |
|-------|------|------|
| `Id` | Guid | `_id` como string |
| `UserId` / `SpaceId` | Guid | referências lógicas (não FK SQL) |
| `StartDate` / `EndDate` | DateTime | UTC |
| `Status` | enum | salvo como string |
| `CreatedAt` | DateTime | |

## `DAL/Enums/BookingStatus.cs`

`Pending` | `Confirmed` | `Cancelled`  

Na criação sempre usamos `Confirmed`. `Pending` existe para updates futuros.

## `DAL/Repositories/IBookingRepository.cs` e `BookingRepository.cs`

Operações:

- `GetAllAsync` / `GetByIdAsync` / `GetByUserIdAsync` / `GetBySpaceIdAsync` / `GetByDateAsync`  
- `HasConflictBookingAsync` — overlap clássico:  
  `StartDate < endDate && EndDate > startDate` **e** status `Confirmed`  
- `AddAsync` / `UpdateAsync` (replace) / `CancelAsync` (update de status)

## `DAL/Constants/MongoDbConstants.cs`

Nome da collection: `"bookings"`.

## `DAL/Configuration/MongoDbSettings.cs`

Seção `"MongoDb"` → `DatabaseName` (default `ReservationBookings`).

## `DAL/BookingsService.DAL.csproj`

Pacote principal: `MongoDB.Driver`.

---

# Como rodar localmente

### Pré-requisitos

- .NET 8 SDK  
- MongoDB em `localhost:27017`  
- Redis em `localhost:6379`  
- UsersService e SpacesService rodando (para criar reserva)

### Build e run

```bash
cd Bookings
dotnet build BookingsService.sln
dotnet run --project API
```

Swagger: `https://localhost:7094/swagger`

### Testar Redis (opcional)

Com Redis CLI:

```bash
redis-cli
LLEN booking-events
LRANGE booking-events 0 -1
```

Ao criar uma reserva, a lista deve receber um JSON novo.

---

# Erros que este projeto tinha (e o que aprendemos)

| Erro | Lição |
|------|--------|
| Typos em nomes de métodos (`AddBusinness…`, `useAuthorization`) | C# é case-sensitive; o compilador / IntelliSense ajudam |
| Classe chamada `BookingsService` dentro do namespace `…BookingsService…` | Evite nome de classe igual ao namespace |
| `InvokeTask` no middleware | O pipeline procura `Invoke` ou `InvokeAsync` |
| Mappers ausentes | AutoMapper precisa dos `Profile` registrados |
| `BookingResponse` sem `SpaceId` | DTO de resposta deve espelhar o que o cliente precisa |
| Referências de `.csproj` apontando pastas erradas | Caminhos relativos precisam bater com a pasta real (`BLL/`, `DAL/`) |
| Tipos trocados (`IReadOnlyList<BookingRepository>`, `Builders<Bookign>`) | Leia os erros do compilador com calma — quase sempre é typo |

---

# Glossário rápido

| Termo | Significado |
|-------|-------------|
| Microsserviço | Serviço pequeno e independente (API própria + banco próprio) |
| DTO | Objeto de transferência (API), diferente da entidade do banco |
| Repository | Encapsula acesso ao banco |
| Soft delete | “Apaga” mudando status, sem remover o registro |
| Overlap | Intervalos de data que se cruzam |
| JWT | Token de autenticação assinado |
| Singleton / Scoped | Tempo de vida no DI: app inteira vs. por request |
| LPUSH / RPOP | Empilhar / desempilhar em lista Redis (fila) |

---

# Próximos passos de estudo (Redis + C#)

1. Rodar Redis local e observar `LLEN` / `LRANGE` ao criar reserva.  
2. Ler o consumer no `NotificationsService` (ele faz o `RPOP`).  
3. Experimentar trocar fire-and-forget por `await` direto e ver a diferença de latência.  
4. Estudar **Redis Streams** e **Outbox** quando quiser garantia forte de entrega.  
5. Escrever testes unitários do `BookingService` com Moq (como em `Reservas/BookingsService`).
