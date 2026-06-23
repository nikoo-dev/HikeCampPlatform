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

    public BookingController(AppDbContext db)
    {
        _db = db;
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

        var spotsRemaining = departure.MaxParticipants - departure.CurrentParticipants;
        if (request.NumberOfParticipants > spotsRemaining)
        {
            return BadRequest(new { message = $"Only {spotsRemaining} spots remaining for this departure." });
        }

        var totalPrice = departure.Tour!.PricePerPerson * request.NumberOfParticipants;

        // Create a Stripe PaymentIntent for this booking
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
}