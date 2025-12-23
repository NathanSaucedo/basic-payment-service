using BasicPaymentsService.Infrastructure.Persistence;
using BasicPaymentsService.Infrastructure.Messaging;
using BasicPaymentsService.Application.UseCases;

var builder = WebApplication.CreateBuilder(args);


// Add services to the container.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();

// Add Persistence (SQL Server)
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")!;
builder.Services.AddPersistence(connectionString);
builder.Services.AddKafkaMessaging(builder.Configuration);
builder.Services.AddHealthChecks();
builder.Services.AddScoped<RegisterPaymentUseCase>();
builder.Services.AddScoped<GetPaymentsByCustomerUseCase>();
builder.Services.AddScoped<GetPaymentByIdUseCase>();

var app = builder.Build();

// Configure the HTTP request pipeline.

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapControllers();

app.Run();
