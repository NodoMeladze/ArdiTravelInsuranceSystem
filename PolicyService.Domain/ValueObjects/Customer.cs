namespace PolicyService.Domain.ValueObjects
{
    public class Customer
    {
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }

        public Customer() { }

        public Customer(string name, string email, string? phoneNumber = null)
        {
            Name = name;
            Email = email;
            PhoneNumber = phoneNumber;
        }
    }
}
