using BasicPaymentsService.Domain.Entities;

namespace BasicPaymentsService.Domain.Interfaces
{
    public interface IPaymentRepository
    {
        Task AddAsync(Payment payment);
        Task<IEnumerable<Payment>> GetPaymentsByCustomerIdAsync(Guid customerId);
        Task<Payment?> GetByIdAsync(Guid paymentId);
        Task UpdateAsync(Payment payment);
    }
}
