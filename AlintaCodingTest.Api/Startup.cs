using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AlintaCodingTest.Api.Error;
using AlintaCodingTest.Domain.Repositories;
using AlintaCodingTest.Persistence;
using AlintaCodingTest.Persistence.Repositories;
using AlintaCodingTest.Services.Contracts.Dtos;
using AlintaCodingTest.Services.Contracts.Interfaces;
using AlintaCodingTest.Services.Services;
using AlintaCodingTest.Services.Validations;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace AlintaCodingTest.Api
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<AppDbContext>(opt => { opt.UseInMemoryDatabase(databaseName: "in-memory"); });

            services.AddSingleton<IValidatorFactory, ValidatorFactory>();
            services.AddSingleton<IValidator<CreateOrUpdateCustomerDto>, CreateOrUpdateCustomerValidator>();
            services.AddScoped<ICustomerRepository, CustomerRepository>();

            services.AddScoped<ICustomerService, CustomerService>();

            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo {Title = "Alinta Customers Api", Version = "v1"});
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseMiddleware<ErrorHandlerMiddleware>();

            app.UseSwagger();
            app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Alinta Customers Api v1"));

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
        }
    }
}