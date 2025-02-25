using userauthjwt.DataAccess.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace ColpherTrade.Middlewares.Validations
{
    public class IsCountryCode : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (string.IsNullOrWhiteSpace(value?.ToString()))
                return new ValidationResult("The CountryCode field is required");

            //Get repository
            var repo = (IRepositoryWrapper)validationContext.GetRequiredService<IRepositoryWrapper>();

            var supportedCodes = repo.LookupRepository.GetCountries().Result.Select(x => x.DialingCode);

            bool IsCorrect = supportedCodes.Contains(value);

            return IsCorrect ? ValidationResult.Success : new ValidationResult($"The country dailing code '{value}' is not supported. Check the lookup for acceptable countries and their dialing codes");
        }

    }
}
