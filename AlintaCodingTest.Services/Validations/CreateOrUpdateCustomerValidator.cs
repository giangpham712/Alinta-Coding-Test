using System;
using System.Collections.Generic;
using System.Text;
using AlintaCodingTest.Services.Contracts.Dtos;
using FluentValidation;

namespace AlintaCodingTest.Services.Validations
{
    public class CreateOrUpdateCustomerValidator : AbstractValidator<CreateOrUpdateCustomerDto>
    {
        public CreateOrUpdateCustomerValidator()
        {
            RuleFor(user => user.FirstName)
                .NotEmpty()
                .WithMessage("First name is required.");

            RuleFor(user => user.LastName)
                .NotEmpty()
                .WithMessage("Last name is required.");

            RuleFor(user => user.DOB)
                .NotEmpty()
                .WithMessage("Date of birth is required.");
        }
    }
}