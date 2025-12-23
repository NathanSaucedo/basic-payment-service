# README de la Prueba Técnica – Basic Payment Service

En esta prueba uso Kafka. Aquí explico, en primera persona, cómo levanto todo en local con Visual Studio, cómo creo la base de datos con el script adjunto y cómo pruebo los endpoints. Todo lo necesario está dentro de las carpetas del proyecto.

## Secuencia rápida (obligatoria y en orden)
1. Levanto Kafka con Docker Compose:
    ```powershell
    docker compose -f ContainerKafka/docker-compose.yml up -d
    ```
2. Creo el tópico `payments` (revisando el nombre exacto del contenedor en [ContainerKafka/ComandosKafkaenDocker.txt](ContainerKafka/ComandosKafkaenDocker.txt)). Ejemplo genérico:
    ```powershell
    docker exec -it kafka-broker kafka-topics --create --topic payments.basic.services --bootstrap-server localhost:9092 --partitions 1 --replication-factor 1
    docker exec -it kafka-broker kafka-topics --describe --topic payments.basic.services --bootstrap-server localhost:9092
    ```
3. Creo la base de datos ejecutando el script [DataBase/setup.sql](DataBase/setup.sql):
    - Con SSMS: abro el archivo y lo ejecuto sobre mi instancia.
    - Con `sqlcmd` (Windows):
       ```powershell
       sqlcmd -S localhost -U sa -P YourStrong!Passw0rd -i DataBase\setup.sql
       ```
4. Configuro la cadena de conexión en [BasicPaymentsService/BasicPaymentsService.WebApi/appsettings.json](BasicPaymentsService/BasicPaymentsService.WebApi/appsettings.json):
    ```json
    {
       "ConnectionStrings": {
          "DefaultConnection": "Server=localhost;Database=BasicPaymentsDb;User Id=sa;Password=YourStrong!Passw0rd;TrustServerCertificate=True"
       }
    }
    ```
5. Levanto ambos proyectos en Visual Studio como múltiples proyectos de inicio:
    - Abro la solución: [BasicPaymentsService/BasicPaymentsService.slnx](BasicPaymentsService/BasicPaymentsService.slnx).
    - Solution Properties → Startup Project → Multiple startup projects.
    - Marco `Start` para:
       - `BasicPaymentsService.WebApi` (API)
       - `BasicPaymentsService.Consumer` (Worker que consume de Kafka)
    - Guardo y presiono F5. La API expone Swagger en `http(s)://localhost:<puerto>/swagger`.

## Estructura del proyecto (carpetas clave)
- **Web API**: [BasicPaymentsService/BasicPaymentsService.WebApi](BasicPaymentsService/BasicPaymentsService.WebApi)
   - Controladores HTTP y Swagger; configuración en [appsettings.json](BasicPaymentsService/BasicPaymentsService.WebApi/appsettings.json).
- **Aplicación**: [BasicPaymentsService/BasicPaymentsService.Application](BasicPaymentsService/BasicPaymentsService.Application)
   - Casos de uso: [RegisterPaymentUseCase.cs](BasicPaymentsService/BasicPaymentsService.Application/UseCases/RegisterPaymentUseCase.cs), [GetPaymentsByCustomerUseCase.cs](BasicPaymentsService/BasicPaymentsService.Application/UseCases/GetPaymentsByCustomerUseCase.cs).
   - DTOs: [RegisterPaymentRequestDto.cs](BasicPaymentsService/BasicPaymentsService.Application/DTOs/RegisterPaymentRequestDto.cs), [PaymentResponseDto.cs](BasicPaymentsService/BasicPaymentsService.Application/DTOs/PaymentResponseDto.cs).
- **Dominio**: [BasicPaymentsService/BasicPaymentsService.Domain](BasicPaymentsService/BasicPaymentsService.Domain)
   - Entidad: [Payment.cs](BasicPaymentsService/BasicPaymentsService.Domain/Entities/Payment.cs); Repositorio: [IPaymentRepository.cs](BasicPaymentsService/BasicPaymentsService.Domain/Interfaces/IPaymentRepository.cs).
- **Infraestructura**: [BasicPaymentsService/BasicPaymentsService.Infrastructure](BasicPaymentsService/BasicPaymentsService.Infrastructure)
   - Implementaciones de persistencia (SQL Server) y mensajería si aplica.
- **Kafka en contenedor**: [ContainerKafka](ContainerKafka)
   - Compose y comandos para broker/Zookeeper y tópicos.
- **Base de Datos**: [DataBase/setup.sql](DataBase/setup.sql)
   - Script para crear tablas iniciales.

## Endpoints y validaciones (lo que demuestro en la prueba)
- `POST /api/payments`
   - Request:
      ```json
      {
        "customerId": "8d9f1c20-01b2-4f55-9a5c-1c1c3a2b7a10",
        "serviceProvider": "ENTEL S.A.",
        "amount": 10,
        "currency": "BS"
      }
      ```
   - Reglas del negocio que aplico: `amount` > 0 y <= 1500 (Bs), rechazo pagos en USD, estado inicial `pendiente`.
- `GET /api/payments?paymentId=<GUID>`
   - Devuelve lista con `paymentId`, `serviceProvider`, `amount`, `status`, `createdAt`.

## Pruebas rápidas (Swagger y curl)
- Swagger: abro `/swagger` y pruebo `POST` y `GET`.
- curl:
   ```powershell
   curl -X POST "http://localhost:8080/api/payments" -H "Content-Type: application/json" -d "{\"customerId\":\"cfe8b150-2f84-4a1a-bdf4-923b20e34973\",\"serviceProvider\":\"SERVICIOS ELÉCTRICOS S.A.\",\"amount\":120.50}"
   curl "http://localhost:8080/api/payments?customerId=cfe8b150-2f84-4a1a-bdf4-923b20e34973"
   ```

## Notas finales
- Kafka es obligatorio en esta guía: por eso primero lo levanto y creo el tópico `payments`.
- La base de datos la creo ejecutando el script incluido en la carpeta del proyecto: [DataBase/setup.sql](DataBase/setup.sql).
- Todo está dentro de las carpetas del proyecto; no hay dependencias externas más allá de Docker para Kafka y SQL Server.
# Prueba Técnica – Basic Payment Service (README) Nathan Saucedo

Este README está diseñado para un examen técnico de Backend Intermedio. Incluye una explicación detallada de la arquitectura del proyecto, cómo configurarlo y ejecutarlo (local y contenedores), documentación de los endpoints, validaciones requeridas, y guía para levantar Kafka y crear tópicos (opcional).

## Contexto
- Solución interna para registrar pagos de servicios básicos (agua, electricidad, telecomunicaciones).
- API básica para registrar y consultar pagos.

## Stack Tecnológico
- .NET 8
- Base de datos relacional (SQL Server)
- Docker (opcional)
- Kafka (opcional; el proyecto lo incluye)

## Requisitos Técnicos (del examen)
- Registrar pago: `POST /api/payments`
   - Body JSON:
      ```json
      {
         "customerId": "cfe8b150-2f84-4a1a-bdf4-923b20e34973",
         "serviceProvider": "SERVICIOS ELÉCTRICOS S.A.",
         "amount": 120.50
      }
      ```
   - Persistir en base de datos.
   - Estado inicial: `pendiente`.
   - Rechazar montos > 1500 Bs.
   - Rechazar montos en dólares (USD).
- Consultar pagos: `GET /api/payments?customerId=...`
   - Devuelve una lista:
      ```json
      [
         {
            "paymentId": "a248ad43-1f44-4b32-b0a0-e1c725b9bb7d",
            "serviceProvider": "SERVICIOS ELÉCTRICOS S.A.",
            "amount": 120.50,
            "status": "pendiente",
            "createdAt": "2025-07-17T08:30:00Z"
         }
      ]
      ```

Nota: El repositorio ya incluye componentes para publicación/consumo via Kafka; su uso es opcional.

---

## Arquitectura y Archivos Clave

La solución está organizada en proyectos por capas orientado a eventos. A continuación, se detalla la responsabilidad de cada proyecto y sus archivos clave.

- **Aplicación**: [BasicPaymentsService/BasicPaymentsService.Application](BasicPaymentsService/BasicPaymentsService.Application)
   - [DTOs/RegisterPaymentRequestDto.cs](BasicPaymentsService/BasicPaymentsService.Application/DTOs/RegisterPaymentRequestDto.cs): Datos de entrada para registrar pagos (incluye `CustomerId`, `ServiceProvider`, `Amount`, y opcional `Currency`). Para el examen, se valida que `Currency` no sea USD (o se omita para asumir Bs).
   - [DTOs/PaymentResponseDto.cs](BasicPaymentsService/BasicPaymentsService.Application/DTOs/PaymentResponseDto.cs): Datos de salida al consultar pagos.
   - [UseCases/RegisterPaymentUseCase.cs](BasicPaymentsService/BasicPaymentsService.Application/UseCases/RegisterPaymentUseCase.cs): Caso de uso para registrar pagos. Debe establecer `status = pendiente`, validar monto `<= 1500 Bs` y rechazar dólares.
   - [UseCases/GetPaymentByIdUseCase.cs](BasicPaymentsService/BasicPaymentsService.Application/UseCases/GetPaymentByIdUseCase.cs): Obtiene pago por ID.
   - [UseCases/GetPaymentsByCustomerUseCase.cs](BasicPaymentsService/BasicPaymentsService.Application/UseCases/GetPaymentsByCustomerUseCase.cs): Lista pagos por `CustomerId`.
   - [Messaging/IPaymentEventPublisher.cs](BasicPaymentsService/BasicPaymentsService.Application/Messaging/IPaymentEventPublisher.cs): Interfaz para publicar eventos de pagos (si se usa Kafka).

- **Dominio**: [BasicPaymentsService/BasicPaymentsService.Domain](BasicPaymentsService/BasicPaymentsService.Domain)
   - [Entities/Payment.cs](BasicPaymentsService/BasicPaymentsService.Domain/Entities/Payment.cs): Entidad de pago (propiedades como `PaymentId`, `CustomerId`, `ServiceProvider`, `Amount`, `Status`, `CreatedAt`).
   - [Interfaces/IPaymentRepository.cs](BasicPaymentsService/BasicPaymentsService.Domain/Interfaces/IPaymentRepository.cs): Contrato para persistencia y consultas.
   - `ValueObjects/` y otros: encapsulan reglas del dominio (p.ej., estados válidos, monedas, etc.).

- **Infraestructura**: [BasicPaymentsService/BasicPaymentsService.Infrastructure](BasicPaymentsService/BasicPaymentsService.Infrastructure)
   - `Persistence/`: Implementación de `IPaymentRepository` contra SQL Server.
   - `Messaging/`: Implementaciones de publisher (Kafka) si se habilita.
   - Archivo de proyecto: [BasicPaymentsService.Infrastructure.csproj](BasicPaymentsService/BasicPaymentsService.Infrastructure/BasicPaymentsService.Infrastructure.csproj)

- **Web API**: [BasicPaymentsService/BasicPaymentsService.WebApi](BasicPaymentsService/BasicPaymentsService.WebApi)
   - [Program.cs](BasicPaymentsService/BasicPaymentsService.WebApi/Program.cs): Bootstrap de la API (.NET 8), registro de servicios, Swagger.
   - `Controllers/`: Controladores HTTP, incluyendo endpoints `POST /api/payments` y `GET /api/payments`.
   - [appsettings.json](BasicPaymentsService/BasicPaymentsService.WebApi/appsettings.json): Configuración (ConnectionStrings, Kafka opcional).
   - [Dockerfile](BasicPaymentsService/BasicPaymentsService.WebApi/Dockerfile): Contenedor de la API (opcional).
   - `Swagger/`: Recursos para documentación interactiva.

- **Consumer (Worker Service)**: [BasicPaymentsService/BasicPaymentsService.Consumer](BasicPaymentsService/BasicPaymentsService.Consumer)
   - [Worker.cs](BasicPaymentsService/BasicPaymentsService.Consumer/Worker.cs): Lógica para consumir eventos de pagos desde Kafka (opcional).
   - [appsettings.json](BasicPaymentsService/BasicPaymentsService.Consumer/appsettings.json): Configuración del consumer (BootstrapServers, Topic).
   - [Dockerfile](BasicPaymentsService/BasicPaymentsService.Consumer/Dockerfile): Contenedor del worker (opcional).

- **Producer**: [BasicPaymentsService/BasicPaymentsService.Producer](BasicPaymentsService/BasicPaymentsService.Producer)
   - Proyecto reservado para publicar eventos (si se requiere separarlo de la API).

- **Kafka en Contenedor**: [ContainerKafka](ContainerKafka)
   - [docker-compose.yml](ContainerKafka/docker-compose.yml): Levanta broker(s) Kafka y Zookeeper.
   - [ComandosKafkaenDocker.txt](ContainerKafka/ComandosKafkaenDocker.txt): Comandos útiles para crear/inspeccionar tópicos.

- **Base de Datos**: [DataBase/setup.sql](DataBase/setup.sql)
   - Script para crear esquema/tablas iniciales. Útil si no hay migraciones EF Core.

---

## Configuración

- **Prerequisitos**
   - Instalar .NET 8 SDK.
   - Tener acceso a SQL Server (local o remoto).
   # Prueba Técnica – Basic Payment Service (README breve)

   Guía mínima para levantar el proyecto en local (Visual Studio) y probar los endpoints requeridos en el examen.

   ## Objetivo
   - API para registrar y consultar pagos de servicios básicos.
   - Reglas: estado inicial `pendiente`, rechazar montos > 1500 Bs, rechazar pagos en USD.

   ## Estructura esencial
   - **Web API**: [BasicPaymentsService/BasicPaymentsService.WebApi](BasicPaymentsService/BasicPaymentsService.WebApi)
      - Expone `POST /api/payments` y `GET /api/payments?customerId=...`.
      - Configuración en [appsettings.json](BasicPaymentsService/BasicPaymentsService.WebApi/appsettings.json).
   - **Aplicación**: [BasicPaymentsService/BasicPaymentsService.Application](BasicPaymentsService/BasicPaymentsService.Application)
      - Casos de uso: [RegisterPaymentUseCase.cs](BasicPaymentsService/BasicPaymentsService.Application/UseCases/RegisterPaymentUseCase.cs), [GetPaymentsByCustomerUseCase.cs](BasicPaymentsService/BasicPaymentsService.Application/UseCases/GetPaymentsByCustomerUseCase.cs).
      - DTOs: [RegisterPaymentRequestDto.cs](BasicPaymentsService/BasicPaymentsService.Application/DTOs/RegisterPaymentRequestDto.cs), [PaymentResponseDto.cs](BasicPaymentsService/BasicPaymentsService.Application/DTOs/PaymentResponseDto.cs).
   - **Dominio**: [BasicPaymentsService/BasicPaymentsService.Domain](BasicPaymentsService/BasicPaymentsService.Domain)
      - Entidad: [Payment.cs](BasicPaymentsService/BasicPaymentsService.Domain/Entities/Payment.cs).
      - Repositorio: [IPaymentRepository.cs](BasicPaymentsService/BasicPaymentsService.Domain/Interfaces/IPaymentRepository.cs).
   - **Infraestructura**: [BasicPaymentsService/BasicPaymentsService.Infrastructure](BasicPaymentsService/BasicPaymentsService.Infrastructure)
      - Implementaciones de persistencia (SQL Server).
   - **Base de Datos**: [DataBase/setup.sql](DataBase/setup.sql)
      - Script para crear tablas si no hay migraciones.
   - **Consumer**: [BasicPaymentsService/BasicPaymentsService.Consumer](BasicPaymentsService/BasicPaymentsService.Consumer)
      - Worker que consume eventos (Kafka).

   ## Requisitos
   - Visual Studio 2022 (o superior) con .NET 8.
   - SQL Server accesible (local o remoto).
   - Kafka y Docker: opcionales (no requeridos para el examen). Si usas Kafka en local, necesitarás Docker Desktop.

   ## Secuencia rápida (orden recomendado)
   1. Levantar Kafka (opcional): `docker compose -f ContainerKafka/docker-compose.yml up -d`.
      - Si necesitas crear el tópico `payments`, revisa los comandos en [ContainerKafka/ComandosKafkaenDocker.txt](ContainerKafka/ComandosKafkaenDocker.txt).
   2. Crear la base de datos: ejecutar el script [DataBase/setup.sql](DataBase/setup.sql) en SQL Server.
   3. Configurar la cadena de conexión en [BasicPaymentsService.WebApi/appsettings.json](BasicPaymentsService/BasicPaymentsService.WebApi/appsettings.json).
   4. Abrir la solución y ejecutar en Visual Studio los proyectos `BasicPaymentsService.WebApi` y (opcional) `BasicPaymentsService.Consumer` como múltiples proyectos de inicio.
      - Todo lo necesario está dentro de las carpetas del proyecto (ver rutas enlazadas arriba).

   ## Configuración
   - Editar `ConnectionStrings:DefaultConnection` en [BasicPaymentsService.WebApi/appsettings.json](BasicPaymentsService/BasicPaymentsService.WebApi/appsettings.json). Ejemplo:
      ```json
      {
         "ConnectionStrings": {
            "DefaultConnection": "Server=localhost;Database=BasicPaymentsDb;User Id=sa;Password=YourStrong!Passw0rd;TrustServerCertificate=True"
         }
      }
      ```
   - Crear el esquema ejecutando [DataBase/setup.sql](DataBase/setup.sql) en SQL Server (SSMS o `sqlcmd`).

   ## Ejecutar en Local con Visual Studio
   - Abrir la solución: [BasicPaymentsService/BasicPaymentsService.slnx](BasicPaymentsService/BasicPaymentsService.slnx).
   - Iniciar ambos proyectos en modo local con Visual Studio:
      - Clic derecho en la solución → Properties → Startup Project → Multiple startup projects.
      - Seleccionar `Start` para:
         - `BasicPaymentsService.WebApi`
         - `BasicPaymentsService.Consumer` (opcional; si no configuras Kafka, puede quedar en ejecución inactivo)
      - Guardar y presionar F5.
   - La Web API expone Swagger en `http(s)://localhost:<puerto>/swagger`.

   ## Endpoints y Validaciones
   - `POST /api/payments`
      - Request:
         ```json
         {
            "customerId": "8d9f1c20-01b2-4f55-9a5c-1c1c3a2b7a10",
            "serviceProvider": "ENTEL S.A.",
            "amount": 10,
            "currency": "BS"
          }
         ```
      - Reglas: `amount` > 0 y <= 1500 (Bs); rechazar USD; estado inicial `pendiente`.
   - `GET /api/payments?customerId=<GUID>`
      - Devuelve lista con `paymentId`, `serviceProvider`, `amount`, `status`, `createdAt`.

   ## Pruebas rápidas
   - Swagger: abrir `/swagger` y ejecutar `POST` y `GET`.
   - curl (ejemplos):
      ```powershell
      curl -X POST "http://localhost:8080/api/payments" -H "Content-Type: application/json" -d "{\"customerId\":\"cfe8b150-2f84-4a1a-bdf4-923b20e34973\",\"serviceProvider\":\"SERVICIOS ELÉCTRICOS S.A.\",\"amount\":120.50}"
      curl "http://localhost:8080/api/payments?customerId=cfe8b150-2f84-4a1a-bdf4-923b20e34973"
      ```

