using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace VTInformatica.Tests.Infrastructure;

/// <summary>Asserzioni sul formato unico degli errori: ProblemDetails con "code" e "traceId".</summary>
public static class ProblemAssert
{
    public static async Task<JsonElement> IsProblemAsync(HttpResponseMessage response, HttpStatusCode status, string code)
    {
        Assert.Equal(status, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);

        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal((int)status, json.GetProperty("status").GetInt32());
        Assert.Equal(code, json.GetProperty("code").GetString());
        Assert.False(string.IsNullOrEmpty(json.GetProperty("traceId").GetString()));
        return json;
    }

    /// <summary>Lista vuota: 200 con array vuoto (non 404).</summary>
    public static async Task IsEmptyListAsync(HttpResponseMessage response)
    {
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(JsonValueKind.Array, json.ValueKind);
        Assert.Equal(0, json.GetArrayLength());
    }
}
