using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using AlintaCodingTest.Api.Json;
using AlintaCodingTest.Services.Contracts.Dtos;
using Microsoft.AspNetCore.Mvc;
using Xunit;
using Xunit.Priority;

namespace AlintaCodingTest.Api.IntegrationTests.Controllers
{
    [Collection("Customers Controller tests")]
    [TestCaseOrderer(PriorityOrderer.Name, PriorityOrderer.Assembly)]
    public class CustomersControllerTests : IClassFixture<TestWebApplicationFactory<Startup>>
    {
        readonly HttpClient _client;

        public CustomersControllerTests(TestWebApplicationFactory<Startup> fixture)
        {
            _client = fixture.CreateClient();

        }

        [Fact, Priority(1)]
        public async Task GetCustomers_NoCustomers_ReturnEmptyList()
        {
            var response = await _client.GetAsync($"/customers");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var json = await response.Content.ReadAsStringAsync();
            var products = JsonSerializer.Deserialize<List<CustomerDto>>(json, JsonHelper.GetDefaultJsonSerializerOptions());

            Assert.NotNull(products);
            Assert.Empty(products);
        }

        [Fact, Priority(2)] 
        public async Task CreateCustomer_ValidInput_ReturnCreatedCustomer()
        {
            var createInput = new CreateOrUpdateCustomerDto()
            {
                FirstName = "John",
                LastName = "Doe",
                DOB = new DateTime(1990, 10, 01)
            };

            var createInputJson = JsonSerializer.Serialize(createInput,
                new JsonSerializerOptions() {PropertyNamingPolicy = JsonNamingPolicy.CamelCase});

            var response = await _client.PostAsync($"/customers", new StringContent(createInputJson, Encoding.UTF8, "application/json"));
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var json = await response.Content.ReadAsStringAsync();
            var createdCustomer = JsonSerializer.Deserialize<CustomerDto>(json, JsonHelper.GetDefaultJsonSerializerOptions());

            Assert.NotNull(createdCustomer);
            Assert.NotEqual(0, createdCustomer.Id);
            Assert.Equal(
                (createInput.FirstName, createInput.LastName, createInput.DOB),
                (createdCustomer.FirstName, createdCustomer.LastName, createdCustomer.DOB));
        }

        [Fact, Priority(3)]
        public async Task GetCustomers_HasOneCustomer_ReturnCustomerList()
        {
            var response = await _client.GetAsync($"/customers");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var json = await response.Content.ReadAsStringAsync();
            var products = JsonSerializer.Deserialize<List<CustomerDto>>(json, JsonHelper.GetDefaultJsonSerializerOptions());

            Assert.NotNull(products);
            Assert.Single(products);
            Assert.Collection(products,
                customerDto => Assert.Equal(
                    (1, "John", "Doe", new DateTime(1990, 10, 01)),
                    (customerDto.Id, customerDto.FirstName, customerDto.LastName, customerDto.DOB)));
        }

        [Fact, Priority(4)]
        public async Task GetCustomers_SearchName_HasNoMatch_ReturnEmptyList()
        {
            var response = await _client.GetAsync($"/customers?search=Mary");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var json = await response.Content.ReadAsStringAsync();
            var products = JsonSerializer.Deserialize<List<CustomerDto>>(json, JsonHelper.GetDefaultJsonSerializerOptions());

            Assert.NotNull(products);
            Assert.Empty(products);
        }

        [Fact, Priority(5)]
        public async Task GetCustomers_SearchName_HasOneMatch_ReturnList()
        {
            var response = await _client.GetAsync($"/customers?search=Joh");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var json = await response.Content.ReadAsStringAsync();
            var products = JsonSerializer.Deserialize<List<CustomerDto>>(json, JsonHelper.GetDefaultJsonSerializerOptions());

            Assert.NotNull(products);
            Assert.Single(products);
            Assert.Collection(products,
                customerDto => Assert.Equal(
                    (1, "John", "Doe", new DateTime(1990, 10, 01)),
                    (customerDto.Id, customerDto.FirstName, customerDto.LastName, customerDto.DOB)));
        }

        [Fact, Priority(5)]
        public async Task CreateCustomer_InvalidInputWithoutNames_Return422Error()
        {
            var createInput = new CreateOrUpdateCustomerDto()
            {
                FirstName = null,
                LastName = null,
                DOB = new DateTime(1990, 10, 01)
            };

            var createInputJson = JsonSerializer.Serialize(createInput,
                new JsonSerializerOptions() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

            var response = await _client.PostAsync($"/customers", new StringContent(createInputJson, Encoding.UTF8, "application/json"));

            Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);

            var json = await response.Content.ReadAsStringAsync();
            var errorResponse = JsonSerializer.Deserialize<ValidationProblemDetails>(json, JsonHelper.GetDefaultJsonSerializerOptions());

            Assert.IsAssignableFrom<ValidationProblemDetails>(errorResponse);
            Assert.Equal(2, errorResponse.Errors.SelectMany(x => x.Value).Count());
            Assert.Collection(errorResponse.Errors.SelectMany(x => x.Value),
                (error) => Assert.Equal("First name is required.", error),
                (error) => Assert.Equal("Last name is required.", error));
        }

        [Fact, Priority(6)]
        public async Task UpdateCustomer_InvalidInputWithoutFirstName_Return422Error()
        {
            var createInput = new CreateOrUpdateCustomerDto()
            {
                FirstName = null,
                LastName = "Parker",
                DOB = new DateTime(1990, 10, 01)
            };

            var createInputJson = JsonSerializer.Serialize(createInput,
                new JsonSerializerOptions() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

            var response = await _client.PutAsync($"/customers/1", new StringContent(createInputJson, Encoding.UTF8, "application/json"));

            Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);

            var json = await response.Content.ReadAsStringAsync();
            var errorResponse = JsonSerializer.Deserialize<ValidationProblemDetails>(json, JsonHelper.GetDefaultJsonSerializerOptions());

            Assert.IsAssignableFrom<ValidationProblemDetails>(errorResponse);
            Assert.Single(errorResponse.Errors.SelectMany(x => x.Value));
            Assert.Collection(errorResponse.Errors.SelectMany(x => x.Value),
                (error) => Assert.Equal("First name is required.", error));
        }

        [Fact, Priority(7)]
        public async Task UpdateCustomer_InvalidCustomer_Return404Error()
        {
            var updateInput = new CreateOrUpdateCustomerDto()
            {
                FirstName = "Peter",
                LastName = "Parker",
                DOB = new DateTime(1990, 10, 01)
            };

            var createInputJson = JsonSerializer.Serialize(updateInput,
                new JsonSerializerOptions() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

            var response = await _client.PutAsync($"/customers/2", new StringContent(createInputJson, Encoding.UTF8, "application/json"));

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

            var json = await response.Content.ReadAsStringAsync();
            var errorResponse = JsonSerializer.Deserialize<ProblemDetails>(json, JsonHelper.GetDefaultJsonSerializerOptions());

            Assert.IsAssignableFrom<ProblemDetails>(errorResponse);
            Assert.Equal($"Customer with ID 2 could not be found.", errorResponse.Title);
        }

        [Fact, Priority(8)]
        public async Task UpdateCustomer_ValidInput_ReturnUpdatedCustomer()
        {
            var updateInput = new CreateOrUpdateCustomerDto()
            {
                FirstName = "Peter",
                LastName = "Parker",
                DOB = new DateTime(1999, 9, 9)
            };

            var createInputJson = JsonSerializer.Serialize(updateInput,
                new JsonSerializerOptions() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

            var response = await _client.PutAsync($"/customers/1", new StringContent(createInputJson, Encoding.UTF8, "application/json"));

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var json = await response.Content.ReadAsStringAsync();
            var updatedCustomer = JsonSerializer.Deserialize<CustomerDto>(json, JsonHelper.GetDefaultJsonSerializerOptions());

            Assert.NotNull(updatedCustomer);
            Assert.Equal(1, updatedCustomer.Id);
            Assert.Equal(
                (updateInput.FirstName, updateInput.LastName, updateInput.DOB),
                (updatedCustomer.FirstName, updatedCustomer.LastName, updatedCustomer.DOB));
        }

        [Fact, Priority(9)]
        public async Task DeleteCustomer_InvalidCustomer_Return404Error()
        {
            var response = await _client.DeleteAsync($"/customers/3");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

            var json = await response.Content.ReadAsStringAsync();
            var errorResponse = JsonSerializer.Deserialize<ProblemDetails>(json, JsonHelper.GetDefaultJsonSerializerOptions());

            Assert.IsAssignableFrom<ProblemDetails>(errorResponse);
            Assert.Equal($"Customer with ID 3 could not be found.", errorResponse.Title);
        }

        [Fact, Priority(10)]
        public async Task DeleteCustomer_ValidCustomer_CustomerDeleted()
        {
            var deleteResponse = await _client.DeleteAsync($"/customers/1");

            Assert.Equal(HttpStatusCode.OK, deleteResponse.StatusCode);

            var deleteResponseJson = await deleteResponse.Content.ReadAsStringAsync();

            Assert.Empty(deleteResponseJson);

            var getCustomersResponse = await _client.GetAsync($"/customers");
            Assert.Equal(HttpStatusCode.OK, getCustomersResponse.StatusCode);

            var getCustomersJson = await getCustomersResponse.Content.ReadAsStringAsync();
            var products = JsonSerializer.Deserialize<List<CustomerDto>>(getCustomersJson, JsonHelper.GetDefaultJsonSerializerOptions());

            Assert.NotNull(products);
            Assert.Empty(products);
        }

        [Fact, Priority(11)]
        public async Task CreateMultipleCustomers_CreatedSuccessfully_GetCustomers_ReturnCreatedCustomers()
        {
            var createInputs = new List<CreateOrUpdateCustomerDto>()
            {
                new CreateOrUpdateCustomerDto() {
                    FirstName = "John",
                    LastName = "Doe",
                    DOB = new DateTime(1990, 10, 01)
                },
                new CreateOrUpdateCustomerDto() {
                    FirstName = "Mary",
                    LastName = "Jane",
                    DOB = new DateTime(1995, 05, 03)
                },
                new CreateOrUpdateCustomerDto() {
                    FirstName = "Clark",
                    LastName = "Kent",
                    DOB = new DateTime(1980, 1, 1)
                }
            };

            foreach (var createInput in createInputs)
            {
                var createInputJson = JsonSerializer.Serialize(createInput,
                    new JsonSerializerOptions() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

                var createResponse = await _client.PostAsync($"/customers", new StringContent(createInputJson, Encoding.UTF8, "application/json"));
                Assert.Equal(HttpStatusCode.OK, createResponse.StatusCode);
            }

            var getCustomersResponse = await _client.GetAsync($"/customers");
            Assert.Equal(HttpStatusCode.OK, getCustomersResponse.StatusCode);

            var json = await getCustomersResponse.Content.ReadAsStringAsync();
            var products = JsonSerializer.Deserialize<List<CustomerDto>>(json, JsonHelper.GetDefaultJsonSerializerOptions());

            Assert.NotNull(products);
            Assert.Equal(3, products.Count);
            Assert.Collection(products,
                customerDto => Assert.Equal(
                    (2, "John", "Doe", new DateTime(1990, 10, 01)),
                    (customerDto.Id, customerDto.FirstName, customerDto.LastName, customerDto.DOB)),
                customerDto => Assert.Equal(
                    (3, "Mary", "Jane", new DateTime(1995, 05, 03)),
                    (customerDto.Id, customerDto.FirstName, customerDto.LastName, customerDto.DOB)),
                customerDto => Assert.Equal(
                    (4, "Clark", "Kent", new DateTime(1980, 1, 1)),
                    (customerDto.Id, customerDto.FirstName, customerDto.LastName, customerDto.DOB)));
        }
    }
}
