# Error Handling - obsluga błędów

Standard zwracania błędów
* Problem details

Przechwytywanie wyjątków:
* Middleware
* Exception filter
* Error endpoint

Do sprawdzenia:
- [ ] [UseExceptionHandle](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/error-handling?view=aspnetcore-7.0)
- [ ] TraceId => Activity.Current?.Id ?? HttpContext.TraceIdentifier;

Arykuły:
* [Using the ProblemDetails Class in ASP.NET Core](https://code-maze.com/using-the-problemdetails-class-in-asp-net-core-web-api/)
* [YT - Global Error Handling](https://www.youtube.com/watch?v=gMwAhKddHYQ)


## Problem Details

[RFC Specification](https://www.rfc-editor.org/rfc/rfc7807.html)
[ProblemDetails Class](https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.mvc.problemdetails?view=aspnetcore-8.0)

```
// RFC Specification - Example
HTTP/1.1 403 Forbidden
Content-Type: application/problem+json
Content-Language: en

{
    "type": "https://example.com/problems/out-of-credit",
    "title": "You do not have enough credits.",
    "detail": "Your current balance is 30, but that costs 50.",
    "instance": "/accounts/1234/msgs/abc",
}
```

[Opis pól według specyfikacji RFC](https://www.rfc-editor.org/rfc/rfc7807.html#section-3.1)

**"type"** (string) - link do dokumentacji opisującej problem np, http://example.com/faq.html#problem-abc, jeżeli brak dokumentacji: **"about:blank"** <br />
**"title"** (string) - krótka czytelna informacja na temat problemu <br />
**"status"** (number) - numer błędu HTTP (400, 401, 500..) <br />
**"detail"** (string) - czytelne wyjaśnienie problemu, wiadomość powina skupić się na pomocy użytkownikowi w rozwiązaniu problemu, nie umieszczać żadnych stacktrace itp.!!! <br />
**"instance"** (string) - który endpoint był wołany gdy wystąpił błąd

``` json
// inny przykład
{
    "type": "https://example.com/errors/product-doesnt-exist",
    "title": "The product you are looking for doesn't exist.",
    "status": 404,
    "detail": "The selected product is currently out of stock and will be probably restocked soon.",
    "instance": "/api/Product/33"
}
```

## Przechwytywanie wyjątków

> Porównanie metod

| |Plusy|Minusy|
|--|--|--|
|Middleware| | |
|Filter| | |
|Endpoint| | |

### Middleware

> Jest jeszcze wersja middleware która implementuje IMiddleware - ale ostatecznie wychodzi na jedno

``` c#
public class ExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger _logger;

    public ExceptionHandlerMiddleware(RequestDelegate next, ILogger logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception exception)
        {
            HandleException(context, exception);
        }
    }

    private static void HandleException(HttpContext context, Exception exception) 
    {
        _logger.Error(exception, exception.Message);

        var problem = new ProblemDetails() {
            Type = "https://datatracker.ietf.org/doc/html/rfc7231#section-6.6.1",
            Title = "An error occurred while processing your request.",
            Status = (int)HttpStatusCode.InternalServerException,
            Instance = context.Request.Path.Value
        };

        context.Response.ContentType = "application/problem+json";
        context.Response.StatusCode = HttpStatusCode.InternalServerException;

        await context.Response.WriteAsync(JsonSerializer.Serialize(problem));
    }
}
```

``` c#
// Program.cs
var app = builder.Build();
app.UseMiddleware<ExceptionHandlerMiddleware>();
```

### Exception filter

``` c#
public class ExceptionHandlerFilterAttribute : ExceptionFilterAttribute
{
    private readonly ILogger<ExceptionHandlerFilterAttribute> _logger;

    public ExceptionHandlerFilterAttribute(ILogger<ExceptionHandlerFilterAttribute> logger)
    {
        _logger = logger;
    }

    public override void OnException(ExceptionContext context)
    {
        var ex = context.Exception; // access to Exception details
        _logger.Error(ex, ex.Message);

        var problem = new ProblemDetails() {
            Type = "https://datatracker.ietf.org/doc/html/rfc7231#section-6.6.1",
            Title = "An error occurred while processing your request.",
            Status = (int)HttpStatusCode.InternalServerException,
            Instance = context.Request.Path.Value
        };

        problem.Extensions["traceId"] = Activity.Current?.Id ?? HttpContext.TraceIdentifier

        var result = new ObjectResult(problem) {
            StatusCode = (int)HttpStatusCode.InternalServerError
        }

        context.Result = result;
        context.ExceptionHandled = true;
    }
}
```

``` c#
// Startup.cs - ConfigureServices()
services.AddControllers(options =>
{
    options.Filters.Add<ExceptionHandlerFilterAttribute>();
});
```

### Error endpoint

[EXAMPLE](examples/error-handling/error-controller.cs)

``` c#
public class ErrorsController : ControllerBase
{
	[Route("/error")]
	public IActionResult Error()
	{
	    var ex = HttpContext.Features.Get<IExceptionHandlerFeature>()?.Error;
	    return Problem(title: ex?.Message, statusCode: (int)HttpStatusCode.InternalServerError);
	}
}
```

``` c#
// Program.cs
var app = builder.Build();
app.UseExceptionHandler("/error");
```
