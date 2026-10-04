using LibraryManagementSystem.Application.DTOs.PaymentDtos;
using LibraryManagementSystem.Application.Interfaces;
using LibraryManagementSystem.Application.Interfaces.IServices;
using LibraryManagementSystem.Domain.Entities;

using LibraryManagementSystem.Infrastructure.Helper;
using Microsoft.Extensions.Options;

using Stripe.Checkout;
namespace LibraryManagementSystem.Infrastructure.Services;

using LibraryManagementSystem.Domain.Enums;


public class PaymentService : IPaymentService
{
    private readonly StripeOptions _stripeOptions;
    private readonly IUnitOfWork _unitOfWork;
    private readonly SessionService _sessionService;

    public PaymentService(IUnitOfWork unitOfWork, IOptions<StripeOptions> stripeOptions, SessionService sessionService)
    {
        _stripeOptions = stripeOptions.Value;
        _unitOfWork = unitOfWork;
        _sessionService = sessionService;
    }
    public async Task<PaymentResponseDto> CreateCheckoutSessionAsync(CreateCheckoutSessionDto dto)
    {
        await _unitOfWork.BeginTransactionAsync();

        try
        {
            // 1. Check that the borrow exists
            var borrow = await _unitOfWork._BorrowsRepo.GetByIdAsync(dto.BorrowId);
            if (borrow == null)
                throw new KeyNotFoundException("Borrow not found.");


            // 2. Check if the borrow is already paid
            var alreadyPaid = await _unitOfWork._PaymentsRepo.GetByBorrowIdAsync(dto.BorrowId);
            if (alreadyPaid.Any(p => p.Status == PaymentStatus.Paid))
                throw new InvalidOperationException("This borrow is already paid.");


            // 3. Create a Stripe checkout session
            var options = new SessionCreateOptions
            {
                Mode = "payment",
                SuccessUrl = _stripeOptions.SuccessUrl,
                CancelUrl = _stripeOptions.CancelUrl,

                LineItems = new List<SessionLineItemOptions>
                {
                    new SessionLineItemOptions
                    {
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            UnitAmountDecimal = dto.Amount * 100, // Amount in cents
                            Currency = "usd",
                            ProductData = new SessionLineItemPriceDataProductDataOptions
                            {
                                Name = $"Payment for Borrow ID: {dto.BorrowId}",
                            }
                        },
                        Quantity = 1,
                    }
                },
                Metadata = new Dictionary<string, string>
                {
                    ["BorrowId"] = borrow.BorrowId.ToString()
                }
            };

            var session = await _sessionService.CreateAsync(options);

            // 4. Create a payment record in the database with status Pending
            var payment = new Payment
            {
                PaymentId = Guid.NewGuid(),
                BorrowId = dto.BorrowId,
                Amount = borrow.TotalAmount,//// Use server-side amount
                PaymentMethod = PaymentMethod.Card,
                Status = PaymentStatus.Pending,
                PaymentDate = DateTime.UtcNow,
                StripeSessionId = session.Id,
                Notes = dto.Notes
            };
            await _unitOfWork._PaymentsRepo.AddAsync(payment);
            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitTransactionAsync();
            return new PaymentResponseDto
            {
                PaymentId = payment.PaymentId,
                BorrowId = payment.BorrowId,
                Amount = payment.Amount,
                PaymentMethod = payment.PaymentMethod,
                Status = payment.Status,
                PaymentDate = payment.PaymentDate,
                StripeSessionId = payment.StripeSessionId,
                SessionUrl = session.Url,
                IsSuccess = true,
                Message = "Checkout session created successfully."
            };
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync();
            return new PaymentResponseDto
            {
                IsSuccess = false,
                Message = "An error occurred while creating the checkout session."
            };
        }
    }
    public async Task<List<PaymentResponseDto>> GetPaymentsByBorrowIdAsync(Guid borrowId)
    {
        var borrow = await _unitOfWork._BorrowsRepo.GetByIdAsync(borrowId);
        if (borrow is null)
            throw new KeyNotFoundException("Borrow not found.");

        var payments = await _unitOfWork._PaymentsRepo.GetByBorrowIdAsync(borrowId);

        return payments
            .OrderByDescending(p => p.PaymentDate)
            .Select(p => new PaymentResponseDto
            {
                PaymentId = p.PaymentId,
                BorrowId = p.BorrowId,
                Amount = p.Amount,
                PaymentMethod = p.PaymentMethod,
                Status = p.Status,
                PaymentDate = p.PaymentDate,
                StripeSessionId = p.StripeSessionId,
                IsSuccess = true,
                Message = "Payment retrieved successfully."
            })
            .ToList();
    }
    public async Task<PaymentResponseDto> RecordCashPaymentAsync(CreateCashPaymentDto dto)
    {
        await _unitOfWork.BeginTransactionAsync();

        try
        {
            // 1. Check that the borrow exists
            var borrow = await _unitOfWork._BorrowsRepo.GetByIdAsync(dto.BorrowId);

            if (borrow == null)
                throw new KeyNotFoundException("Borrow not found.");

            // 2. Check if the borrow is already paid

            var alreadyPaid = await _unitOfWork._PaymentsRepo.GetByBorrowIdAsync(dto.BorrowId);
            if (alreadyPaid.Any(p => p.Status == PaymentStatus.Paid))
                throw new InvalidOperationException("This borrow is already paid.");

            // 3. Create the cash payment
            var payment = new Payment
            {
                PaymentId = Guid.NewGuid(),
                BorrowId = dto.BorrowId,
                Amount = dto.Amount,
                PaymentMethod = PaymentMethod.Cash,
                Status = PaymentStatus.Paid,
                PaymentDate = DateTime.UtcNow,
                Notes = dto.Notes

            };

            //4. add payment
            await _unitOfWork._PaymentsRepo.AddAsync(payment);

            // 5. Save everything
            await _unitOfWork.SaveChangesAsync();

            // 6. Commit transaction
            await _unitOfWork.CommitTransactionAsync();

            // 7. Return response
            return new PaymentResponseDto
            {
                PaymentId = payment.PaymentId,
                BorrowId = payment.BorrowId,
                Amount = payment.Amount,
                PaymentMethod = payment.PaymentMethod,
                Status = payment.Status,
                PaymentDate = payment.PaymentDate,
                IsSuccess = true,
                Message = "Paid Successfully"
            };
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync();
            return new PaymentResponseDto
            {
                IsSuccess = false,
                Message = "An error occurred while recording the cash payment."
            };
        }

    }
    public async Task<PaymentResponseDto> VerifyCheckoutSessionAsync(string sessionId)
    {
        var payment = await _unitOfWork._PaymentsRepo.GetByStripeSessionIdAsync(sessionId);
        if (payment is null)
        {
            return new PaymentResponseDto
            {
                IsSuccess = false,
                Message = "Payment not found for this session."
            };
        }

        if (payment.Status == PaymentStatus.Paid)
        {
            return new PaymentResponseDto
            {
                PaymentId = payment.PaymentId,
                BorrowId = payment.BorrowId,
                Amount = payment.Amount,
                PaymentMethod = payment.PaymentMethod,
                Status = payment.Status,
                PaymentDate = payment.PaymentDate,
                StripeSessionId = payment.StripeSessionId,
                SessionUrl = null,
                IsSuccess = true,
                Message = "Payment already confirmed as paid."
            };
        }

        var session = await _sessionService.GetAsync(sessionId);
        var isPaid = session.PaymentStatus == "paid";

        if (isPaid)
        {
            payment.Status = PaymentStatus.Paid;
            payment.StripePaymentIntentId = session.PaymentIntentId;
            _unitOfWork._PaymentsRepo.Update(payment);
            await _unitOfWork.SaveChangesAsync();
        }

        return new PaymentResponseDto
        {
            PaymentId = payment.PaymentId,
            BorrowId = payment.BorrowId,
            Amount = payment.Amount,
            PaymentMethod = payment.PaymentMethod,
            Status = payment.Status,
            PaymentDate = payment.PaymentDate,
            StripeSessionId = payment.StripeSessionId,
            SessionUrl = null,
            IsSuccess = isPaid,
            Message = isPaid ? "Payment confirmed as paid." : $"Stripe status: {session.PaymentStatus}. Not paid yet."
        };
    }
}
