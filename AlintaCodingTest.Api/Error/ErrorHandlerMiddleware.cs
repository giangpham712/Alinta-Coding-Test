using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using AlintaCodingTest.Services.Exceptions;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AlintaCodingTest.Api.Error
{
    public class ErrorHandlerMiddleware
    {
        private readonly RequestDelegate _next;

        public ErrorHandlerMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception error)
            {
                var response = context.Response;
                response.ContentType = "application/json";

                ProblemDetails responseModel;

                switch (error)
                {
                    case ValidationException e:
                        response.StatusCode = (int) HttpStatusCode.UnprocessableEntity;
                        var validationErrorsByProperty = e.Errors
                            .GroupBy(x => x.PropertyName)
                            .ToDictionary(x => x.Key, x => x.Select(y => y.ErrorMessage).ToArray());

                        responseModel = new ValidationProblemDetails(validationErrorsByProperty);
                        break;
                    case EntityNotFoundException e:
                        response.StatusCode = (int) HttpStatusCode.NotFound;
                        responseModel = new ProblemDetails();
                        break;
                    default:
                        response.StatusCode = (int) HttpStatusCode.InternalServerError;
                        responseModel = new ProblemDetails();
                        break;
                }

                responseModel.Title = error.Message;
                responseModel.Detail = error.Message;
                responseModel.Status = response.StatusCode;
                var result = JsonSerializer.Serialize(responseModel, responseModel.GetType());
                await response.WriteAsync(result);
            }
        }
    }
}