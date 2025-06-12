namespace PolicyService.Domain.DTOs
{
    public class ValidationResult
    {
        public bool IsValid => Errors.Count == 0;
        public List<string> Errors { get; set; } = [];

        public static ValidationResult Success()
        {
            return new ValidationResult();
        }

        public static ValidationResult Failure(params string[] errors)
        {
            return new ValidationResult
            {
                Errors = [.. errors]
            };
        }

        public static ValidationResult Failure(IEnumerable<string> errors)
        {
            return new ValidationResult
            {
                Errors = [.. errors]
            };
        }

        public void AddError(string error)
        {
            Errors.Add(error);
        }
    }
}
