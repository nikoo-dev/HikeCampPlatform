using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Stripe;
using HikeCampPlatform.Api.Data;
using HikeCampPlatform.Api.Models;

namespace HikeCampPlatform.Api.Controllers;

[ApiController]
[Route("api/webhooks")]
public class StripeWebhookController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IConfiguration _config;

    public StripeWebhookController(AppDbContext db, IConfiguration config)
    {
        _db = db;
        _config = config;
    }

    [HttpPost("stripe")]
    public async Task<IActionResult> HandleStripeWebhook()
    {
        var json = await new StreamReader(Request.Body).ReadToEndAsync();
        var webhookSecret = _config["Stripe:WebhookSecret"];

        Event stripeEvent;
        try
        {
            stripeEvent = EventUtility.ConstructEvent(
                json,
                Request.Headers["Stripe-Signature"],
                webhookSecret
            );
        }
        catch (StripeException)
        {
            // Signature didn't match -- this request did NOT genuinely come from Stripe
            return BadRequest();
        }

        if (stripeEvent.Type == "payment_intent.succeeded")
        {
            var paymentIntent = stripeEvent.Data.Object as PaymentIntent;
            if (paymentIntent != null)
            {
                await HandlePaymentSucceeded(paymentIntent.Id);
            }
        }

        return Ok();
    }

    private async Task HandlePaymentSucceeded(string paymentIntentId)
    {
        var booking = await _db.Bookings
            .Include(b => b.TourDeparture)
            .FirstOrDefaultAsync(b => b.StripePaymentIntentId == paymentIntentId);

        if (booking == null || booking.Status == BookingStatus.Confirmed)
        {
            // Either we don't recognize this payment, or we've already processed it -- avoid double-counting
            return;
        }

        booking.Status = BookingStatus.Confirmed;
        booking.TourDeparture!.CurrentParticipants += booking.NumberOfParticipants;

        await _db.SaveChangesAsync();
    }
}