using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AlintaCodingTest.Domain.Entities;
using AlintaCodingTest.Domain.Repositories;
using AlintaCodingTest.Services.Contracts.Dtos;
using AlintaCodingTest.Services.Exceptions;
using AlintaCodingTest.Services.Services;
using AlintaCodingTest.Services.Validations;
using FluentValidation;
using FluentValidation.Internal;
using FluentValidation.Results;
using Moq;
using Xunit;

namespace AlintaCodingTest.Services.Tests.Services
{
    public class CustomerServiceTests
    {
        [Fact]
        public async Task GetCustomers_NoCustomers_ReturnEmptyList()
        {
            var mockCustomerRepository = new Mock<ICustomerRepository>();
            mockCustomerRepository.Setup(r => r.GetCustomers(It.IsAny<string>())).ReturnsAsync(new List<Customer>());
            var mockValidatorFactory = new Mock<IValidatorFactory>();

            var customerService = new CustomerService(mockCustomerRepository.Object, mockValidatorFactory.Object);
            var customers = await customerService.GetCustomers(string.Empty);

            mockCustomerRepository.Verify(r => r.GetCustomers(string.Empty), Times.Once);

            Assert.IsAssignableFrom<List<CustomerDto>>(customers);
            Assert.Empty(customers);
        }

        [Fact]
        public async Task GetCustomers_HasCustomers_ReturnCustomerList()
        {
            var customers = new List<Customer>()
            {
                new Customer() { Id = 1, FirstName = "Peter", LastName = "Parker", DOB = new DateTime(1990, 1, 1) },
                new Customer() { Id = 2, FirstName = "Mary", LastName = "Jane", DOB = new DateTime(1992, 11, 21) },
                new Customer() { Id = 3, FirstName = "Clark", LastName = "Kent", DOB = new DateTime(1988, 5, 15) },
            };

            var mockCustomerRepository = new Mock<ICustomerRepository>();
            mockCustomerRepository.Setup(r => r.GetCustomers(It.IsAny<string>())).ReturnsAsync(customers);
            var mockValidatorFactory = new Mock<IValidatorFactory>();

            var customerService = new CustomerService(mockCustomerRepository.Object, mockValidatorFactory.Object);
            var customerDtos = await customerService.GetCustomers(string.Empty);

            Assert.IsAssignableFrom<List<CustomerDto>>(customerDtos);
            Assert.Equal(3, customerDtos.Count);
            Assert.Collection(customerDtos, 
                customerDto => Assert.Equal(
                    (customers[0].Id, customers[0].FirstName, customers[0].LastName, customers[0].DOB),
                    (customerDtos[0].Id, customerDtos[0].FirstName, customerDtos[0].LastName, customerDtos[0].DOB)),
                customerDto => Assert.Equal(
                    (customers[1].Id, customers[1].FirstName, customers[1].LastName, customers[1].DOB),
                    (customerDtos[1].Id, customerDtos[1].FirstName, customerDtos[1].LastName, customerDtos[1].DOB)),
                customerDto => Assert.Equal(
                    (customers[2].Id, customers[2].FirstName, customers[2].LastName, customers[2].DOB),
                    (customerDtos[2].Id, customerDtos[2].FirstName, customerDtos[2].LastName, customerDtos[2].DOB)));
        }

        [Fact]
        public async Task CreateCustomer_ValidInput_ReturnCreatedCustomer()
        {
            var customer = new Customer()
                {Id = 1, FirstName = "Peter", LastName = "Parker", DOB = new DateTime(1990, 1, 1)};

            var mockCustomerRepository = new Mock<ICustomerRepository>();
            mockCustomerRepository.Setup(r => r.AddCustomer(It.IsAny<Customer>())).ReturnsAsync(customer);

            var mockValidator = new Mock<CreateOrUpdateCustomerValidator>();
            
            var mockValidatorFactory = new Mock<IValidatorFactory>();
            mockValidatorFactory.Setup(f => f.GetValidator<CreateOrUpdateCustomerDto>())
                .Returns(mockValidator.Object);

            var customerService = new CustomerService(mockCustomerRepository.Object, mockValidatorFactory.Object);
            var customerDto = await customerService.CreateCustomer(new CreateOrUpdateCustomerDto());

            Assert.IsAssignableFrom<CustomerDto>(customerDto);
            Assert.Equal((customer.Id, customer.FirstName, customer.LastName, customer.DOB),
                (customerDto.Id, customerDto.FirstName, customerDto.LastName, customerDto.DOB));
        }

        [Fact]
        public async Task CreateCustomer_InvalidInput_ThrowValidationException()
        {
            var customer = new Customer()
                { Id = 1, FirstName = "Peter", LastName = "Parker", DOB = new DateTime(1990, 1, 1) };

            var mockCustomerRepository = new Mock<ICustomerRepository>();
            mockCustomerRepository.Setup(r => r.AddCustomer(It.IsAny<Customer>())).ReturnsAsync(customer);
            
            var validationException = new ValidationException(new List<ValidationFailure>() { new ValidationFailure(nameof(Customer.FirstName), "First name is required.") });
            var mockValidator = new Mock<IValidator<CreateOrUpdateCustomerDto>>();
            mockValidator.Setup(v => v.ValidateAsync(It.Is<ValidationContext<CreateOrUpdateCustomerDto>>(context => context.ThrowOnFailures), default)).ThrowsAsync(validationException);

            var mockValidatorFactory = new Mock<IValidatorFactory>();
            mockValidatorFactory.Setup(f => f.GetValidator<CreateOrUpdateCustomerDto>())
                .Returns(mockValidator.Object);

            var customerService = new CustomerService(mockCustomerRepository.Object, mockValidatorFactory.Object);
            var exception = await Assert.ThrowsAsync<ValidationException>(() => customerService.CreateCustomer(new CreateOrUpdateCustomerDto()));

            Assert.IsAssignableFrom<ValidationException>(exception);
            Assert.Collection(exception.Errors,
                error => Assert.Equal((nameof(Customer.FirstName), "First name is required."), (error.PropertyName, error.ErrorMessage)));
        }

        [Fact]
        public async Task UpdateCustomer_InvalidCustomer_ThrowEntityNotFoundException()
        {
            var mockCustomerRepository = new Mock<ICustomerRepository>();
            mockCustomerRepository.Setup(r => r.GetCustomerById(It.IsAny<int>())).ReturnsAsync((Customer) null);

            var mockValidatorFactory = new Mock<IValidatorFactory>();
            var customerService = new CustomerService(mockCustomerRepository.Object, mockValidatorFactory.Object);
            var exception = await Assert.ThrowsAsync<EntityNotFoundException>(() => customerService.UpdateCustomer(1, new CreateOrUpdateCustomerDto()));

            Assert.IsAssignableFrom<EntityNotFoundException>(exception);
            Assert.Equal("Customer with ID 1 could not be found.", exception.Message);
        }
    }
}
