using System;
using System.ComponentModel;

namespace BasicPaymentsService.Application.DTOs
{
    public class RegisterPaymentRequestDto
    {
        [DefaultValue(typeof(Guid), "8D9F1C20-01B2-4F55-9A5C-1C1C3A2B7A10")]
        public Guid CustomerId { get; set; } = Guid.Parse("8D9F1C20-01B2-4F55-9A5C-1C1C3A2B7A10");

        [DefaultValue("ENTEL S.A.")]
        public string ServiceProvider { get; set; } = "ENTEL S.A.";

        [DefaultValue(typeof(decimal), "10")]
        public decimal Amount { get; set; } = 10m;

        [DefaultValue("BS")]
        public string? Currency { get; set; } = "BS";
    }
}
