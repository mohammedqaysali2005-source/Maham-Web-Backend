using System.Text.Json;
using System.Text.Json.Serialization;

namespace Maham.Web.Services;

public class ApiResponseWrapper<T>
{
    public bool Success { get; set; }
    public T? Data { get; set; }
    public string Message { get; set; } = string.Empty;

    [JsonPropertyName("errors")]
    public JsonElement? RawErrors { get; set; }

    public List<string> GetErrorList()
    {
        var list = new List<string>();

        if (RawErrors.HasValue)
        {
            var element = RawErrors.Value;
            if (element.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in element.EnumerateArray())
                {
                    var str = item.GetString();
                    if (!string.IsNullOrWhiteSpace(str)) list.Add(str);
                }
            }
            else if (element.ValueKind == JsonValueKind.Object)
            {
                foreach (var prop in element.EnumerateObject())
                {
                    if (prop.Value.ValueKind == JsonValueKind.Array)
                    {
                        foreach (var item in prop.Value.EnumerateArray())
                        {
                            var str = item.GetString();
                            if (!string.IsNullOrWhiteSpace(str)) list.Add(str);
                        }
                    }
                    else if (prop.Value.ValueKind == JsonValueKind.String)
                    {
                        var str = prop.Value.GetString();
                        if (!string.IsNullOrWhiteSpace(str)) list.Add(str);
                    }
                }
            }
            else if (element.ValueKind == JsonValueKind.String)
            {
                var str = element.GetString();
                if (!string.IsNullOrWhiteSpace(str)) list.Add(str);
            }
        }

        if (!list.Any() && !string.IsNullOrWhiteSpace(Message) && Message != "OK")
        {
            list.Add(Message);
        }

        return list.Distinct().ToList();
    }
}
