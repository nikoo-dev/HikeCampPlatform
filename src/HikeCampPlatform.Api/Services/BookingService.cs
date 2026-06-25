namespace HikeCampPlatform.Api.Services;

public class BookingService
{
    public decimal CalculateTotalPrice(decimal pricePerPerson, int numberOfParticipants)
    {
        if (numberOfParticipants <= 0)
        {
            throw new ArgumentException("Number of participants must be at least 1.", nameof(numberOfParticipants));
        }

        return pricePerPerson * numberOfParticipants;
    }

    public bool HasEnoughCapacity(int maxParticipants, int currentParticipants, int requestedParticipants)
    {
        var spotsRemaining = maxParticipants - currentParticipants;
        return requestedParticipants <= spotsRemaining;
    }

    public int CalculateSpotsRemaining(int maxParticipants, int currentParticipants)
    {
        return maxParticipants - currentParticipants;
    }
}