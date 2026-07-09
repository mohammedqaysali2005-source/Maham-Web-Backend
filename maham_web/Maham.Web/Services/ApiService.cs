using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Maham.Web.Services;

public class ApiService
{
    private readonly HttpClient _http;
    private readonly IHttpContextAccessor _ctx;
    private const string Base = "http://localhost:5233";

    private static readonly JsonSerializerOptions Opts = new()
    {
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public ApiService(HttpClient http, IHttpContextAccessor ctx)
    {
        _http = http;
        _ctx = ctx;
    }

    private void Auth()
    {
        var token = _ctx.HttpContext?.Session.GetString("Token");
        _http.DefaultRequestHeaders.Authorization = null;
        if (!string.IsNullOrEmpty(token))
            _http.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);
    }

    public async Task<(bool ok, T? data, string? message)> GetAsync<T>(string url) where T : class
    {
        Auth();
        try
        {
            var res = await _http.GetAsync(Base + url);
            var json = await res.Content.ReadAsStringAsync();
            var wrapper = JsonSerializer.Deserialize<WebApiResponse<T>>(json, Opts);
            return (res.IsSuccessStatusCode, wrapper?.Data, wrapper?.Message);
        }
        catch { return (false, default, "خطأ في الاتصال"); }
    }

    public async Task<(bool ok, T? data, string? message)> PostAsync<T>(string url, object body) where T : class
    {
        Auth();
        try
        {
            var content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");
            var res = await _http.PostAsync(Base + url, content);
            var json = await res.Content.ReadAsStringAsync();
            var wrapper = JsonSerializer.Deserialize<WebApiResponse<T>>(json, Opts);
            return (res.IsSuccessStatusCode, wrapper?.Data, wrapper?.Message);
        }
        catch { return (false, default, "خطأ في الاتصال"); }
    }

    public async Task<(bool ok, T? data, string? message)> PostFormAsync<T>(string url, MultipartFormDataContent form) where T : class
    {
        Auth();
        try
        {
            var res = await _http.PostAsync(Base + url, form);
            var json = await res.Content.ReadAsStringAsync();
            var wrapper = JsonSerializer.Deserialize<WebApiResponse<T>>(json, Opts);
            return (res.IsSuccessStatusCode, wrapper?.Data, wrapper?.Message);
        }
        catch { return (false, default, "خطأ في الاتصال"); }
    }

    public async Task<(bool ok, T? data, string? message)> PutAsync<T>(string url, object body) where T : class
    {
        Auth();
        try
        {
            var content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");
            var res = await _http.PutAsync(Base + url, content);
            var json = await res.Content.ReadAsStringAsync();
            var wrapper = JsonSerializer.Deserialize<WebApiResponse<T>>(json, Opts);
            return (res.IsSuccessStatusCode, wrapper?.Data, wrapper?.Message);
        }
        catch { return (false, default, "خطأ في الاتصال"); }
    }

    public async Task<bool> DeleteAsync(string url)
    {
        Auth();
        try
        {
            var res = await _http.DeleteAsync(Base + url);
            return res.IsSuccessStatusCode;
        }
        catch { return false; }
    }
}
