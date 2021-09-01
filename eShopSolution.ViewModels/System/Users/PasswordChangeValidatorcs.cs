using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;

namespace eShopSolution.ViewModels.System.Users
{
    public class PasswordChangeValidatorcs : AbstractValidator<PasswordChangeRequest>
    {
        public PasswordChangeValidatorcs()
        {
            RuleFor(x => x.UserName).NotEmpty().WithMessage("Username is required");
            RuleFor(x => x.CurrentPassword).NotEmpty().WithMessage("Password is required")
                .MinimumLength(6).WithMessage("Password is at least 6 characters");
            RuleFor(x => x).Custom((request, context) =>
            {
                if (request.ConfirmPassword != request.NewPassword)
                {
                    context.AddFailure("Cofirm password is not match");
                }
            });
        }
    }
}