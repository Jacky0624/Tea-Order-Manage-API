using FluentValidation;
using System.Text.RegularExpressions;
using TeaAPI.Models.Requests.Orders;

namespace TeaAPI.Validators.Orders
{
    public class EditOrderValidator : AbstractValidator<EditOrderRequest>
    {
        public EditOrderValidator() 
        {
            RuleFor(x => x.Phone)
            .NotEmpty().WithMessage("phone number is required")
            .Must(IsValidPhoneNumber).WithMessage("invalid format.");
    
            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("title is required")
                .MaximumLength(20).WithMessage("title must be at most 20 characters");

            RuleFor(x => x.Address)
                .NotEmpty().WithMessage("address is required")
                .MaximumLength(200).WithMessage("address must be at most 20 characters");

            RuleFor(x => x.Items)
                .NotEmpty().WithMessage("no item")
                .Must(items => items.All(i => i.Count > 0))
                .WithMessage("each Item need to over 1");
        }
        private bool IsValidPhoneNumber(string phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber))
            {
                return false;
            }
            string pattern = @"^(09\d{8}|\+886-?9\d{8}|0[2-8]\d{7,8}|0[3-9]\d{1,2}\d{6,8})$";
            return Regex.IsMatch(phoneNumber, pattern);
        }
    }
}
