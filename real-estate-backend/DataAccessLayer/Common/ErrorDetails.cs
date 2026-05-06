using System.Text.Json;

namespace DataAccessLayer.Common;

public record ErrorDetails(int statusCode, string errorMessage, IEnumerable<string>? errors = null)
{
    public override string ToString() => JsonSerializer.Serialize(this);
}
