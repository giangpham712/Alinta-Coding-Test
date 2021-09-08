using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AlintaCodingTest.Domain.Entities;
using AlintaCodingTest.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace AlintaCodingTest.Persistence.Repositories
{
    public class CustomerRepository : ICustomerRepository
    {
        private readonly AppDbContext _context;

        public CustomerRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Customer>> GetCustomers(string search = null)
        {
            var queryable = _context.Set<Customer>()
                .AsNoTracking();

            if (!string.IsNullOrWhiteSpace(search))
            {
                queryable = queryable.Where(c =>
                    ($"{c.FirstName} {c.LastName}").Contains(search, StringComparison.CurrentCultureIgnoreCase));
            }

            return await queryable.ToListAsync();
        }

        public async Task<Customer> GetCustomerById(int id)
        {
            return await _context.Customers.FindAsync(id);
        }

        public async Task<Customer> AddCustomer(Customer customer)
        {
            await _context.Customers.AddAsync(customer);
            await _context.SaveChangesAsync();

            return customer;
        }

        public async Task<Customer> UpdateCustomer(Customer customer)
        {
            _context.Customers.Update(customer);
            await _context.SaveChangesAsync();

            return customer;
        }

        public async Task DeleteCustomer(Customer customer)
        {
            _context.Customers.Remove(customer);
            await _context.SaveChangesAsync();
        }
    }
}