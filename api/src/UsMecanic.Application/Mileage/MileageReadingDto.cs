using UsMecanic.Domain.Mileage;

namespace UsMecanic.Application.Mileage;

public sealed record MileageReadingDto(Guid Id, Guid VehicleId, DateOnly Date, int Kilometers, string? Reason, DateTimeOffset RecordedAt)
{
    public static MileageReadingDto From(MileageReading reading)
    {
        ArgumentNullException.ThrowIfNull(reading);

        return new(reading.Id, reading.VehicleId, reading.Date, reading.Kilometers, reading.Reason, reading.RecordedAt);
    }
}
