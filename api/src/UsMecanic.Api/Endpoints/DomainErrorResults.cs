using UsMecanic.Domain.Common;

namespace UsMecanic.Api.Endpoints;

/// <summary>Traduit les erreurs métier en réponses ProblemDetails.</summary>
internal static class DomainErrorResults
{
    public static IResult ToProblem(this DomainError error) => TypedResults.Problem(
        title: error.Message,
        statusCode: error.Kind switch
        {
            ErrorKind.NotFound => StatusCodes.Status404NotFound,
            ErrorKind.Conflict => StatusCodes.Status409Conflict,
            _ => StatusCodes.Status400BadRequest,
        },
        extensions: new Dictionary<string, object?> { ["code"] = error.Code });

    public static IResult ToOk<T>(this Result<T> result) =>
        result.IsSuccess ? TypedResults.Ok(result.Value) : result.Error!.ToProblem();
}
