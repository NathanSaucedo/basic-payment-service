using BasicPaymentsService.Application.DTOs;
using BasicPaymentsService.Application.Messaging;
using BasicPaymentsService.Application.UseCases;
using BasicPaymentsService.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace BasicPaymentsService.WebApi.Controllers.v1
{
    [ApiController]
    [Route("/api/payments")]
    public class BasicPaymentsServiceController : Controller
    {
        [HttpPost]
        public async Task<IActionResult> RegisterPayment([FromBody] RegisterPaymentRequestDto request, [FromServices] RegisterPaymentUseCase useCase, [FromServices] IPaymentEventPublisher publisher)
        {
            try
            {
                var result = await useCase.ExecuteAsync(request);
                await publisher.PublishPaymentRegisteredEventAsync(result);
                return Created($"/api/payments/{result.PaymentId}", result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetPayments([FromQuery] Guid customerId, [FromServices] GetPaymentsByCustomerUseCase useCase)
        {
            try
            {
                var list = await useCase.ExecuteAsync(customerId);
                return Ok(list);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}
