using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Stripe;
using HikeCampPlatform.Api.Data;
using HikeCampPlatform.Api.Models;
using HikeCampPlatform.Api.DTOs;

namespace HikeCampPlatform.Api.Controllers;

[ApiController]
[Route("api/bookings")]
public class BookingController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly Services.BookingService _bookingService;

    public BookingController(AppDbContext db, Services.BookingService bookingService)
    {
        _db = db;
        _bookingService = bookingService;
    }

    // POST /api/bookings -- User only
    [HttpPost]
    [Authorize]
    public async Task<ActionResult<BookingResponse>> CreateBooking([FromBody] CreateBookingRequest request)
    {
        var role = User.FindFirst(ClaimTypes.Role)?.Value;
        if (role != "User")
        {
            return Forbid();
        }

        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userIdClaim == null || !int.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized();
        }

        var departure = await _db.TourDepartures
            .Include(d => d.Tour)
            .FirstOrDefaultAsync(d => d.Id == request.TourDepartureId);

        if (departure == null)
        {
            return NotFound(new { message = "Departure not found." });
        }

        if (!_bookingService.HasEnoughCapacity(departure.MaxParticipants, departure.CurrentParticipants, request.NumberOfParticipants))
        {
            var spotsRemaining = _bookingService.CalculateSpotsRemaining(departure.MaxParticipants, departure.CurrentParticipants);
            return BadRequest(new { message = $"Only {spotsRemaining} spots remaining for this departure." });
        }

        var totalPrice = _bookingService.CalculateTotalPrice(departure.Tour!.PricePerPerson, request.NumberOfParticipants);

        var paymentIntentService = new PaymentIntentService();
        var paymentIntent = await paymentIntentService.CreateAsync(new PaymentIntentCreateOptions
        {
            Amount = (long)(totalPrice * 100),
            Currency = departure.Tour.Currency.ToLower(),
            AutomaticPaymentMethods = new PaymentIntentAutomaticPaymentMethodsOptions
            {
                Enabled = true,
                AllowRedirects = "never"
            },
            Metadata = new Dictionary<string, string>
            {
                { "userId", userId.ToString() },
                { "tourDepartureId", departure.Id.ToString() }
            }
        });

        var booking = new Booking
        {
            UserId = userId,
            TourDepartureId = departure.Id,
            NumberOfParticipants = request.NumberOfParticipants,
            TotalPrice = totalPrice,
            Status = BookingStatus.PendingPayment,
            StripePaymentIntentId = paymentIntent.Id
        };

        _db.Bookings.Add(booking);
        await _db.SaveChangesAsync();

        return Ok(new BookingResponse
        {
            Id = booking.Id,
            TourDepartureId = booking.TourDepartureId,
            NumberOfParticipants = booking.NumberOfParticipants,
            TotalPrice = booking.TotalPrice,
            Status = booking.Status,
            ClientSecret = paymentIntent.ClientSecret
        });
    }
    // GET /api/bookings/operator -- operator only, all bookings across their tours
[HttpGet("operator")]
[Authorize]
public async Task<ActionResult<List<OperatorBookingResponse>>> GetOperatorBookings()
{
    var role = User.FindFirst(ClaimTypes.Role)?.Value;
    if (role != "Operator")
    {
        return Forbid();
    }

    var operatorIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    if (operatorIdClaim == null || !int.TryParse(operatorIdClaim, out var operatorId))
    {
        return Unauthorized();
    }

    var bookings = await _db.Bookings
        .Include(b => b.User)
        .Include(b => b.TourDeparture)
            .ThenInclude(td => td!.Tour)
        .Where(b => b.TourDeparture!.Tour!.OperatorId == operatorId)
        .ToListAsync();

    var hasCompletionIds = await _db.Completions
        .Where(c => c.BookingId.HasValue)
        .Select(c => c.BookingId!.Value)
        .ToListAsync();

    var response = bookings.Select(b => new OperatorBookingResponse
    {
        Id = b.Id,
        UserFullName = b.User?.FullName ?? "",
        TourTitle = b.TourDeparture?.Tour?.Title ?? "",
        DepartureDate = b.TourDeparture?.DepartureDate ?? default,
        NumberOfParticipants = b.NumberOfParticipants,
        TotalPrice = b.TotalPrice,
        Status = b.Status,
        IsCompleted = hasCompletionIds.Contains(b.Id)
    }).ToList();

    return Ok(response);
}

    // PATCH /api/bookings/{id}/confirm-completion -- operator only, must own the tour
    [HttpPatch("{id}/confirm-completion")]
    [Authorize]
    public async Task<ActionResult<CompletionResponse>> ConfirmCompletion(int id)
    {
        var role = User.FindFirst(ClaimTypes.Role)?.Value;
        if (role != "Operator")
        {
            return Forbid();
        }

        var operatorIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (operatorIdClaim == null || !int.TryParse(operatorIdClaim, out var operatorId))
        {
            return Unauthorized();
        }

        var booking = await _db.Bookings
            .Include(b => b.TourDeparture)
                .ThenInclude(td => td!.Tour)
            .FirstOrDefaultAsync(b => b.Id == id);

        if (booking == null)
        {
            return NotFound(new { message = "Booking not found." });
        }

        var tour = booking.TourDeparture?.Tour;
        if (tour == null || tour.OperatorId != operatorId)
        {
            return Forbid();
        }

        if (booking.Status != BookingStatus.Confirmed)
        {
            return BadRequest(new { message = "Only confirmed (paid) bookings can be marked as completed." });
        }

        var alreadyExists = await _db.Completions.AnyAsync(c => c.BookingId == booking.Id);
        if (alreadyExists)
        {
            return BadRequest(new { message = "This booking has already been marked as completed." });
        }

        var completion = new Completion
        {
            BookingId = booking.Id,
            UserId = booking.UserId,
            TourId = tour.Id,
            ConfirmedByOperatorId = operatorId,
            IsSelfReported = false,
            CompletedAt = DateTime.UtcNow
        };

        _db.Completions.Add(completion);
        await _db.SaveChangesAsync();

        return Ok(new CompletionResponse
        {
            Id = completion.Id,
            BookingId = completion.BookingId,
            UserId = completion.UserId,
            TourId = completion.TourId,
            TourTitle = tour.Title,
            IsSelfReported = completion.IsSelfReported,
            CompletedAt = completion.CompletedAt
        });
    }
}