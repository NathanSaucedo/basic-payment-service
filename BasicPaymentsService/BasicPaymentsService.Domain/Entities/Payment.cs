namespace BasicPaymentsService.Domain.Entities
{
    public class Payment
    {
        public Guid PaymentId { get; set; }
        public Guid CustomerId { get; set; }
        public string ServiceProvider { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string? Currency { get; set; } = "BS";
        public string Status { get; set; } = "PENDIENTE";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
