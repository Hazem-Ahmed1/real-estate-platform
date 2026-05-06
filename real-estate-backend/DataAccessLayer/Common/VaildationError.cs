namespace DataAccessLayer.Common;

public record VaildationError(string Field, IEnumerable<string> Errors);
