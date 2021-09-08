using System;
using System.Collections.Generic;
using System.Text;
using FluentValidation;

namespace AlintaCodingTest.Services.Validations
{
    public class ValidatorFactory : IValidatorFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public ValidatorFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public IValidator<T> GetValidator<T>()
        {
            return (IValidator<T>) GetValidator(typeof(T));
        }

        public IValidator GetValidator(Type type)
        {
            var genericType = typeof(IValidator<>).MakeGenericType(type);
            var validator = _serviceProvider.GetService(genericType) as IValidator;

            return validator;
        }
    }
}