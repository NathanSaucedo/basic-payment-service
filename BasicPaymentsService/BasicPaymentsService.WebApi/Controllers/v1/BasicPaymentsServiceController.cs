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

        [HttpGet("{paymentId:guid}")]
        public async Task<IActionResult> GetPaymentById([FromRoute] Guid paymentId, [FromServices] GetPaymentByIdUseCase useCase)
        {
            try
            {
                var result = await useCase.ExecuteAsync(paymentId);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { error = ex.Message });
            }
        }
    }
}
