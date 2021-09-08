using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AlintaCodingTest.Domain.Entities;
using AlintaCodingTest.Domain.Repositories;
using AlintaCodingTest.Services.Contracts.Dtos;
using AlintaCodingTest.Services.Contracts.Interfaces;
using AlintaCodingTest.Services.Exceptions;
using FluentValidation;

namespace AlintaCodingTest.Services.Services
{
    public class CustomerService : ICustomerService
    {
        private readonly ICustomerRepository _customerRepository;

        private readonly IValidatorFactory _validatorFactory;

        public CustomerService(
            ICustomerRepository customerRepository,
            IValidatorFactory validatorFactory)
        {
            _customerRepository = customerRepository;
            _validatorFactory = validatorFactory;
        }

        public async Task<List<CustomerDto>> GetCustomers(string search = null)
        {
            var customers = await _customerRepository.GetCustomers(search);
            return customers.Select(x => new CustomerDto()
            {
                Id = x.Id,
                FirstName = x.FirstName,
                LastName = x.LastName,
                DOB = x.DOB
            }).ToList();
        }

        public async Task<CustomerDto> CreateCustomer(CreateOrUpdateCustomerDto createCustomerInput)
        {
            var validator = _validatorFactory.GetValidator<CreateOrUpdateCustomerDto>();
            await validator.ValidateAndThrowAsync(createCustomerInput);

            var created = await _customerRepository.AddCustomer(new Customer()
            {
                FirstName = createCustomerInput.FirstName,
                LastName = createCustomerInput.LastName,
                DOB = createCustomerInput.DOB,
            });

            return new CustomerDto()
            {
                Id = created.Id,
                FirstName = created.FirstName,
                LastName = created.LastName,
                DOB = created.DOB
            };
        }

        public async Task<CustomerDto> UpdateCustomer(int id, CreateOrUpdateCustomerDto updateCustomerInput)
        {
            var existingCustomer = await _customerRepository.GetCustomerById(id);
            if (existingCustomer == null)
            {
                throw new EntityNotFoundException(nameof(Customer), id);
            }

            var validator = _validatorFactory.GetValidator<CreateOrUpdateCustomerDto>();
            await validator.ValidateAndThrowAsync(updateCustomerInput);

            existingCustomer.FirstName = updateCustomerInput.FirstName;
            existingCustomer.LastName = updateCustomerInput.LastName;
            existingCustomer.DOB = updateCustomerInput.DOB;

            await _customerRepository.UpdateCustomer(existingCustomer);

            return new CustomerDto()
            {
                Id = existingCustomer.Id,
                FirstName = existingCustomer.FirstName,
                LastName = existingCustomer.LastName,
                DOB = existingCustomer.DOB
            };
        }

        public async Task DeleteCustomer(int id)
        {
            var existingCustomer = await _customerRepository.GetCustomerById(id);
            if (existingCustomer == null)
            {
                throw new EntityNotFoundException(nameof(Customer), id);
            }

            await _customerRepository.DeleteCustomer(existingCustomer);
        }
    }
}