using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using AlintaCodingTest.Domain.Entities;

namespace AlintaCodingTest.Domain.Repositories
{
    public interface ICustomerRepository
    {
        Task<IEnumerable<Customer>> GetCustomers(string search = null);
        Task<Customer> GetCustomerById(int id);
        Task<Customer> AddCustomer(Customer customer);
        Task<Customer> UpdateCustomer(Customer customer);
        Task DeleteCustomer(Customer customer);
    }
}