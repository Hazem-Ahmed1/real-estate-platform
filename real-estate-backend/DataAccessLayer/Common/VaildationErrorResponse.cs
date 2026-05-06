namespace DataAccessLayer.Common;

public record VaildationErrorResponse(int statusCode, string ErrorMessage, IEnumerable<VaildationError> Errors);
