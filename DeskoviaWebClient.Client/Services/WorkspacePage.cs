using Deskovia.Shared.Contracts;
using Microsoft.AspNetCore.Components;

namespace DeskoviaWebClient.Client.Services;

public abstract class WorkspacePage : ComponentBase
{
    [Inject] protected ApiClient Api { get; set; } = null!;
    protected bool Busy { get; set; }
    protected string? Error { get; set; }
    protected string? Notice { get; set; }
    protected async Task Run(Func<Task> work)
    {
        if (Busy) return;
        Busy = true; Error = null; Notice = null;
        try { await work(); }
        catch (ApiException e)
        {
            if (e.Status == 409)
            {
                try { await OnConflict(); } catch (Exception) { }
            }
            Error = e.Message;
            if (e.Details is { ValueKind: System.Text.Json.JsonValueKind.Array } details)
            {
                Error += " " + string.Join("; ", details.EnumerateArray().Select(x =>
                    x.TryGetProperty("date", out var date) ? date.GetString() + ": " + (x.TryGetProperty("reason", out var reason) ? reason.GetString() : "") :
                    x.TryGetProperty("id", out var id) ? "Reservation " + id.GetString() : x.ToString()));
            }
        }
        catch (Exception) { Error = "Unable to process the data. Check the fields and try again."; }
        finally { Busy = false; }
    }
    protected virtual Task OnConflict() => Task.CompletedTask;
    protected static string Local(DateTimeOffset value, string zone) => TimeZoneInfo.ConvertTime(value, TimeZoneInfo.FindSystemTimeZoneById(zone)).ToString("dd MMM yyyy HH:mm", System.Globalization.CultureInfo.InvariantCulture);
    protected static string Number(decimal n) => n.ToString(System.Globalization.CultureInfo.InvariantCulture);
}
