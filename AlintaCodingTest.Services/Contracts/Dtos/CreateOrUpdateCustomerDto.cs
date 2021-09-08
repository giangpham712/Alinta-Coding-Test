using System;

namespace AlintaCodingTest.Services.Contracts.Dtos
{
    public class CreateOrUpdateCustomerDto
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime DOB { get; set; }
    }
}