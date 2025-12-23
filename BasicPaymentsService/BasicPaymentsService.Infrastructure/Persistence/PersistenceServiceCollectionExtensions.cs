using BasicPaymentsService.Domain.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace BasicPaymentsService.Infrastructure.Persistence
{
    public static class PersistenceServiceCollectionExtensions
    {
        public static IServiceCollection AddPersistence(this IServiceCollection services, string connectionString)
        {
            services.AddScoped<IPaymentRepository>(provider => new PaymentRepository(connectionString));
            return services;
        }
    }
}
