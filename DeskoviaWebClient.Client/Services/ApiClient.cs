using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Deskovia.Shared.Contracts;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.WebAssembly.Http;

namespace DeskoviaWebClient.Client.Services;

public sealed class ApiException(int status, string code, string message, JsonElement? details) : Exception(message)
{
    public int Status { get; } = status;
    public string Code { get; } = code;
    public JsonElement? Details { get; } = details;
}
public sealed class ApiClient(HttpClient http, NavigationManager navigation)
{
    private string? token;
    public CurrentUser? User { get; private set; }
    public event Action? Changed;
    public bool Admin => User?.Role is "Administrator" or "Owner";
    public bool Owner => User?.Role == "Owner";
    private async Task<HttpResponseMessage> Send(string method, string path, object? body = null, CancellationToken ct = default, bool quiet = false)
    {
        using var request = new HttpRequestMessage(new HttpMethod(method), path);
        request.SetBrowserRequestCredentials(BrowserRequestCredentials.Include);
        if (body != null) request.Content = JsonContent.Create(body);
        if (method != "GET")
        {
            token ??= (await Get<AntiforgeryToken>("api/auth/antiforgery")).Token;
            request.Headers.Add("X-CSRF-TOKEN", token);
        }
        HttpResponseMessage response;
        try { response = await http.SendAsync(request, ct); }
        catch (HttpRequestException) { throw new ApiException(0, "network", "The API is unavailable. Check that the backend is running and the HTTPS certificate is trusted.", null); }
        if (response.IsSuccessStatusCode) return response;
        using (response)
        {
            var content = await response.Content.ReadAsStringAsync();
            var message = $"API error ({(int)response.StatusCode}).";
            var code = "api_error"; JsonElement? details = null;
            try
            {
                using var json = JsonDocument.Parse(content);
                var data = json.RootElement;
                if (data.TryGetProperty("title", out var title)) message = title.GetString() ?? message;
                if (data.TryGetProperty("code", out var c)) code = c.GetString() ?? code;
                if (data.TryGetProperty("details", out var d)) details = d.Clone();
                if (data.TryGetProperty("errors", out var errors) && errors.ValueKind == JsonValueKind.Object) message += " " + string.Join(" ", errors.EnumerateObject().SelectMany(x => x.Value.EnumerateArray().Select(v => x.Name + ": " + v.GetString())));
            }
            catch (JsonException) { }
            if (!quiet && response.StatusCode == HttpStatusCode.Unauthorized && path != "api/auth/login" && path != "api/auth/register")
            {
                User = null; token = null; Changed?.Invoke();
                var local = "/" + navigation.ToBaseRelativePath(navigation.Uri);
                if (!local.StartsWith("/login")) navigation.NavigateTo("/login?returnUrl=" + Uri.EscapeDataString(local));
            }
            if (code == "antiforgery_failed") token = null;
            throw new ApiException((int)response.StatusCode, code, message, details);
        }
    }
    public async Task<T> Get<T>(string path)
    {
        using var r = await Send("GET", path); return (await r.Content.ReadFromJsonAsync<T>())!;
    }
    public async Task<T> Post<T>(string path, object? body = null, CancellationToken ct = default)
    {
        using var r = await Send("POST", path, body, ct); return (await r.Content.ReadFromJsonAsync<T>())!;
    }
    public async Task<T> Put<T>(string path, object body)
    {
        using var r = await Send("PUT", path, body); return (await r.Content.ReadFromJsonAsync<T>())!;
    }
    public async Task Action(string path, object? body = null, string method = "POST")
    {
        using var r = await Send(method, path, body);
    }
    public async Task<bool> Ensure()
    {
        try { User = await Get<CurrentUser>("api/auth/me"); Changed?.Invoke(); return true; }
        catch (ApiException e) when (e.Status == 401) { return false; }
    }
    public async Task Authenticate(bool register, string name, string email, string password)
    {
        User = register ? await Post<CurrentUser>("api/auth/register", new RegisterRequest(name, email, password)) :
            await Post<CurrentUser>("api/auth/login", new LoginRequest(email, password));
        token = null; Changed?.Invoke();
    }
    public string MicrosoftLoginUrl => new Uri(http.BaseAddress!, "api/auth/microsoft").AbsoluteUri;
    public async Task<string> FormSecurityToken()
    {
        token = (await Get<AntiforgeryToken>("api/auth/antiforgery")).Token;
        return token;
    }
    public async Task RefreshSession()
    {
        try
        {
            using var response = await Send("GET", "api/auth/me", quiet: true);
            User = await response.Content.ReadFromJsonAsync<CurrentUser>();
        }
        catch (ApiException e) when (e.Status == 401) { User = null; }
        token = null;
        Changed?.Invoke();
    }
    public async Task Logout()
    {
        await Action("api/auth/logout"); User = null; token = null; Changed?.Invoke(); navigation.NavigateTo("/login");
    }
}
