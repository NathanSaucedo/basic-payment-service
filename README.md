# README - Pruebas del sistema

Este documento describe cómo levantar y probar el sistema (Web API y Worker Consumer) y validar el flujo de pagos end-to-end.

## Prerrequisitos
- .NET 8 SDK instalado.
- SQL Server accesible y cadena de conexión configurada en `BasicPaymentsService.WebApi/appsettings.json` y (si aplica) `BasicPaymentsService.Infrastructure`.
- Kafka accesible y configurado (por ejemplo, `localhost:9092`) en `BasicPaymentsService.Consumer/appsettings.json` y `BasicPaymentsService.WebApi/appsettings.json`.
- Topic Kafka existente: `payments` (o el configurado en `Kafka:Topic`).

## Configuración
- Verificar `ConnectionStrings:DefaultConnection` en `BasicPaymentsService.WebApi/appsettings.json`.
- Verificar `Kafka:BootstrapServers` y `Kafka:Topic` en:
  - `BasicPaymentsService.WebApi/appsettings.json`
  - `BasicPaymentsService.Consumer/appsettings.json`
- Crear base de datos y aplicar migraciones si el proyecto las incluye, o permitir que el repositorio cree tablas al primer uso.

## Levantar los servicios
1. Web API
   - Abrir la solución.
   - Establecer proyecto `BasicPaymentsService.WebApi` como de inicio.
   - Ejecutar (F5). Se expone Swagger en `https://localhost:xxxx/swagger`.

2. Worker Consumer
   - En una segunda instancia/terminal, ejecutar el proyecto `BasicPaymentsService.Consumer` (Worker Service).
   - Asegurarse de que no esté bloqueado el `.exe` (si aparece error de archivo bloqueado, detener proceso y limpiar `bin/obj`).

## Pruebas con Swagger
1. Registrar un pago
   - Endpoint POST `api/payments` (o el que corresponda en el controlador `BasicPaymentsServiceController`).
   - Body `RegisterPaymentRequestDto` con campos:
     - `CustomerId`: GUID del cliente.
     - `ServiceProvider`: proveedor del servicio.
     - `Amount`: monto.
     - `Currency`: código de moneda (opcional).
   - Enviar la solicitud y verificar respuesta `201/200`.
   - Esto debe persistir el pago (SQL Server) y publicar un evento en Kafka.

2. Consultar pago por ID
   - Endpoint GET `api/payments/{id}`.
   - Usar el `PaymentId` devuelto al registrar.
   - Verificar que se obtiene el estado y detalles.

3. Consultar pagos por cliente
   - Endpoint GET `api/customers/{customerId}/payments` (si está implementado en `GetPaymentsByCustomerUseCase`).
   - Validar listado de pagos del cliente.

## Validación del Consumer (Kafka)
- Con el `Worker` en ejecución, tras registrar un pago, el evento en `Kafka:Topic` debe ser consumido por `BasicPaymentsService.Consumer`.
- Revisar logs de la consola del Worker para ver procesamiento del evento.

## Pruebas de error y resiliencia
- Probar con `Kafka` detenido: el publicador debe manejar errores (ver logs) y/o reintentar según configuración.
- Probar con SQL Server fuera de línea para verificar manejo de excepciones en repositorio.

## Troubleshooting
- Error de archivo bloqueado al compilar el `Consumer`: detener proceso en el Administrador de tareas o `taskkill /IM BasicPaymentsService.Consumer.exe /F`, limpiar `bin/obj`, reconstruir.
- Problemas de conexión a Kafka: verificar `BootstrapServers`, firewall y disponibilidad del broker.
- Swagger sin valores por defecto: confirmar `DefaultValueSchemaFilter` registrado y atributos `[DefaultValue]` en DTOs.

## Ejecución por línea de comandos
- Web API: `dotnet run --project BasicPaymentsService.WebApi`
- Consumer: `dotnet run --project BasicPaymentsService.Consumer`

## E2E esperado
- POST registro de pago -> inserta en base de datos -> publica evento en Kafka -> Consumer procesa evento -> logs confirman procesamiento.
