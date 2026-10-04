using LibraryManagementSystem.Application.DTOs.PaymentDtos;
using LibraryManagementSystem.Application.Interfaces.IServices;
using Microsoft.AspNetCore.Mvc;

namespace LibraryManagementSystem.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PaymentController : ControllerBase
{
    private readonly IPaymentService _paymentService;

    public PaymentController(IPaymentService paymentService)
    {
        _paymentService = paymentService;
    }

    [HttpPost("checkout")]
    public async Task<IActionResult> CreateCheckoutSession([FromBody] CreateCheckoutSessionDto dto)
    {
        var result = await _paymentService.CreateCheckoutSessionAsync(dto);

        if (result.IsSuccess)
            return Ok(result);

        return BadRequest(result);
    }

    [HttpPost("cash")]
    public async Task<IActionResult> RecordCashPayment([FromBody] CreateCashPaymentDto dto)
    {
        var result = await _paymentService.RecordCashPaymentAsync(dto);

        if (result.IsSuccess)
            return Ok(result);

        return BadRequest(result);
    }

    [HttpGet("borrow/{borrowId:guid}")]
    public async Task<IActionResult> GetPaymentsByBorrowId([FromRoute] Guid borrowId)
    {
        try
        {
            var result = await _paymentService.GetPaymentsByBorrowIdAsync(borrowId);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpGet("success")]
    public async Task<IActionResult> PaymentSuccess([FromQuery(Name = "session_id")] string sessionId)
    {
        if (string.IsNullOrWhiteSpace(sessionId))
            return BadRequest(new { message = "session_id is required." });

        var result = await _paymentService.VerifyCheckoutSessionAsync(sessionId);

        if (result.IsSuccess)
            return Ok(result);

        return BadRequest(result);
    }

    [HttpGet("cancel")]
    public IActionResult PaymentCancel()
    {
        return Ok(new { message = "Payment was cancelled. No charge made. Payment stays Pending." });
    }
}
