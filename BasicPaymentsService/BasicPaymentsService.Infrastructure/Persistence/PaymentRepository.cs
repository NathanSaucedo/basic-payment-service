using BasicPaymentsService.Domain.Interfaces;
using BasicPaymentsService.Domain.Entities;
using Microsoft.Data.SqlClient;
using System.Data;


namespace BasicPaymentsService.Infrastructure.Persistence
{
    public class PaymentRepository : IPaymentRepository
    {
        private readonly string _connectionString;

        public PaymentRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task AddAsync(Payment payment)
        {
            using var conn = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand("dbo.usp_RegisterPayment", conn)
            {
                CommandType = CommandType.StoredProcedure
            };
            cmd.Parameters.Add(new SqlParameter("@PaymentId", SqlDbType.UniqueIdentifier) { Value = payment.PaymentId });
            cmd.Parameters.Add(new SqlParameter("@CustomerId", SqlDbType.UniqueIdentifier) { Value = payment.CustomerId });
            cmd.Parameters.Add(new SqlParameter("@ServiceProvider", SqlDbType.NVarChar, 100) { Value = payment.ServiceProvider });
            cmd.Parameters.Add(new SqlParameter("@Amount", SqlDbType.Decimal) { Precision = 18, Scale = 2, Value = payment.Amount });
            cmd.Parameters.Add(new SqlParameter("@Currency", SqlDbType.NVarChar, 10) { Value = payment.Currency });
            cmd.Parameters.Add(new SqlParameter("@Status", SqlDbType.NVarChar, 20) { Value = payment.Status });

            await conn.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task<IEnumerable<Payment>> GetPaymentsByCustomerIdAsync(Guid customerId)
        {
            var results = new List<Payment>();
            using var conn = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand("dbo.usp_GetPaymentsByCustomer", conn)
            {
                CommandType = CommandType.StoredProcedure
            };
            cmd.Parameters.Add(new SqlParameter("@CustomerId", SqlDbType.UniqueIdentifier) { Value = customerId });

            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                results.Add(MapPayment(reader));
            }
            return results;
        }

        public async Task<Payment?> GetByIdAsync(Guid paymentId)
        {
            using var conn = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand("dbo.usp_GetPaymentById", conn)
            {
                CommandType = CommandType.StoredProcedure
            };
            cmd.Parameters.Add(new SqlParameter("@PaymentId", SqlDbType.UniqueIdentifier) { Value = paymentId });

            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync(CommandBehavior.SingleResult);
            if (await reader.ReadAsync())
            {
                return MapPayment(reader);
            }
            return null;
        }

        public async Task UpdateAsync(Payment payment)
        {
            using var conn = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand("dbo.usp_UpdatePaymentStatus", conn)
            {
                CommandType = CommandType.StoredProcedure
            };
            cmd.Parameters.Add(new SqlParameter("@PaymentId", SqlDbType.UniqueIdentifier) { Value = payment.PaymentId });
            cmd.Parameters.Add(new SqlParameter("@Status", SqlDbType.NVarChar, 20) { Value = payment.Status });

            await conn.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
        }

        private static Payment MapPayment(SqlDataReader reader)
        {
            return new Payment
            {
                PaymentId = reader.GetGuid(reader.GetOrdinal("PaymentId")),
                CustomerId = reader.GetGuid(reader.GetOrdinal("CustomerId")),
                ServiceProvider = reader.GetString(reader.GetOrdinal("ServiceProvider")),
                Amount = reader.GetDecimal(reader.GetOrdinal("Amount")),
                Currency = reader.GetString(reader.GetOrdinal("Currency")),
                Status = reader.GetString(reader.GetOrdinal("Status")),
                CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt"))
            };
        }
    }
}