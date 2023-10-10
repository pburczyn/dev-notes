using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using System.Net;

public class ErrorController : ControllerBase
{
    [Route("/error")]
    public IActionResult Error()
    {
        var ex = HttpContext.Features.Get<IExceptionHandlerFeature>()?.Error;
        var path = Request.HttpContext.Features.Get<IExceptionHandlerPathFeature>()?.Path;

        if (ex is BaseException bex)
        {
            var httpStatusCode = bex switch
            {
                BusinessException => HttpStatusCode.BadRequest,
                //TechnicalException => HttpStatusCode.ServiceUnavailable,
                _ => HttpStatusCode.InternalServerError
            };

            var problem = Problem(
                statusCode: (int)httpStatusCode,
                title: bex.Message,
                detail: bex.Explanation,
                instance: path,
                type: "http://mojastrona.pl/pomoc"
                );

            if(bex is TechnicalException tex && !string.IsNullOrWhiteSpace(tex.ErrorMessage))
            (problem.Value as ProblemDetails)?.Extensions.Add("error", tex.ErrorMessage);

            return problem;
        }

        var result = Problem(title: ex?.Message, statusCode: (int)HttpStatusCode.InternalServerError);

        (result.Value as ProblemDetails)?.Extensions.Add("stack-trace", ex.StackTrace);

        return result;
    }
}

