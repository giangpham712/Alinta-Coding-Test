using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AlintaCodingTest.Services.Contracts.Dtos;
using AlintaCodingTest.Services.Contracts.Interfaces;

namespace AlintaCodingTest.Api.Controllers
{
    [ApiController]
    [Route("customers")]
    public class CustomersController : ControllerBase
    {
        private readonly ICustomerService _customerService;

        public CustomersController(ICustomerService customerService)
        {
            _customerService = customerService;
        }

        [HttpGet]
        public async Task<IEnumerable<CustomerDto>> GetCustomers([FromQuery] string search = null)
        {
            return await _customerService.GetCustomers(search);
        }

        [HttpPost]
        public async Task<CustomerDto> CreateCustomer([FromBody] CreateOrUpdateCustomerDto input)
        {
            return await _customerService.CreateCustomer(input);
        }

        [HttpPut]
        [Route("{id}")]
        public async Task<CustomerDto> CreateCustomer([FromRoute] int id, [FromBody] CreateOrUpdateCustomerDto input)
        {
            return await _customerService.UpdateCustomer(id, input);
        }

        [HttpDelete]
        [Route("{id}")]
        public async Task DeleteCustomer([FromRoute] int id)
        {
            await _customerService.DeleteCustomer(id);
        }
    }
}