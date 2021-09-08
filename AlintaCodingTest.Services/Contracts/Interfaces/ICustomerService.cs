using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using AlintaCodingTest.Services.Contracts.Dtos;

namespace AlintaCodingTest.Services.Contracts.Interfaces
{
    public interface ICustomerService
    {
        Task<List<CustomerDto>> GetCustomers(string search = null);
        Task<CustomerDto> CreateCustomer(CreateOrUpdateCustomerDto createCustomerInput);
        Task<CustomerDto> UpdateCustomer(int id, CreateOrUpdateCustomerDto updateCustomerInput);
        Task DeleteCustomer(int id);
    }
}