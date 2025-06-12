namespace PolicyService.Domain.ValueObjects
{
    public class TripDetails
    {
        public string Destination { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int DurationInDays => (EndDate - StartDate).Days + 1;

        public TripDetails() { }

        public TripDetails(string destination, DateTime startDate, DateTime endDate)
        {
            if (startDate >= endDate)
                throw new ArgumentException("Start date must be before end date");

            if (startDate < DateTime.Today)
                throw new ArgumentException("Start date cannot be in the past");

            Destination = destination;
            StartDate = startDate;
            EndDate = endDate;
        }
    }
}
