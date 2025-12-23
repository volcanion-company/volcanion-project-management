using VolcanionPM.Domain.Common;

namespace VolcanionPM.Domain.ValueObjects;

public class DateRange : ValueObject
{
    public DateTime StartDate { get; private set; }
    public DateTime EndDate { get; private set; }

    private DateRange(DateTime startDate, DateTime endDate)
    {
        StartDate = startDate;
        EndDate = endDate;
    }

    public static DateRange Create(DateTime startDate, DateTime endDate)
    {
        if (startDate > endDate)
            throw new ArgumentException("Start date cannot be after end date");

        return new DateRange(startDate, endDate);
    }

    public int DurationInDays => (EndDate - StartDate).Days;

    public bool Contains(DateTime date)
    {
        return date >= StartDate && date <= EndDate;
    }

    public bool Overlaps(DateRange other)
    {
        return StartDate <= other.EndDate && EndDate >= other.StartDate;
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return StartDate;
        yield return EndDate;
    }

    public override string ToString() => $"{StartDate:yyyy-MM-dd} to {EndDate:yyyy-MM-dd}";
}
