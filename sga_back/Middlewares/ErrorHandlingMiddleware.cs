using FluentValidation;
using sga_back.Exceptions;
using sga_back.Wrappers;
using System.Net;
using System.Text.Json;

namespace sga_back.Middlewares;

public class ErrorHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IHostEnvironment _env;

    public ErrorHandlingMiddleware(RequestDelegate next, IHostEnvironment env)
    {
        _next = next;
        _env = env;
    }

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next(context);

            if (context.Response.StatusCode is 401 or 403)
            {
                throw new UnauthorizedAccessException();
            }
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        Response<object> response = new()
        {
            Success = false,
            StatusCode = (int)HttpStatusCode.InternalServerError,
            Errors = []
        };

        // Definir el mensaje en función del ambiente
        bool showStackTrace = _env.IsDevelopment();

        switch (exception)
        {
            case ValidationException validationException:
                response.StatusCode = (int)HttpStatusCode.BadRequest;
                response.Errors.AddRange(validationException.Errors.Select(e => e.ErrorMessage));
                break;

            case RepositoryException repositoryException:
                response.StatusCode = (int)HttpStatusCode.InternalServerError;
                response.Errors.Add(repositoryException.Message);
                if (showStackTrace)
                {
                    response.Errors.Add(repositoryException.InnerException.StackTrace);
                }
                break;

            case ServiceException serviceException:
                response.StatusCode = (int)HttpStatusCode.InternalServerError;
                response.Errors.Add(serviceException.Message);
                if (!showStackTrace)
                {
                    response.Errors.Add(serviceException.InnerException.StackTrace);
                }
                break;

            case NoDataFoundException noDataFoundException:
                response.StatusCode = (int)HttpStatusCode.NoContent;
                break;

            case ReglasdeNegocioException reglasdeNegocioException:
                response.StatusCode = (int)HttpStatusCode.BadRequest;
                response.Errors.Add(reglasdeNegocioException.Message);
                break;

            case ParametroFaltanteCadenaConexionException parametrosConexionFaltanteException:
                response.StatusCode = (int)HttpStatusCode.BadRequest;
                response.Errors.Add(parametrosConexionFaltanteException.Message);
                break;

            default:
                response.StatusCode = (int)HttpStatusCode.InternalServerError;
                response.Errors.Add("Ocurrió un error inesperado en el servidor.");
                break;
        }

        string jsonResponse = JsonSerializer.Serialize(response);
        context.Response.StatusCode = response.StatusCode;
        return context.Response.WriteAsync(jsonResponse);
    }

}

