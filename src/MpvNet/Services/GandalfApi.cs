using System.Net.Http;
using System.Text.Json;
using System.Text;
using MpvNet.Help;

namespace MpvNet.Services;

public class GandalfVideo
{
    public string Name { get; set; } = "";
    public string Url  { get; set; } = "";
    public int WatchedTime { get; set; }
}

public static class GandalfSession
{
    public static bool IsGandalf { get; set; }
    public static string LinkId { get; set; } = "";
    public static string Token  { get; set; } = "";
    public static int WatchedTime { get; set; }
    public static string Name { get; set; } = "";
}

public static class GandalfApi
{
    public static GandalfVideo? ResolveVideo(string linkId, string token)
    {
        try
        {
            using var client = new HttpClient();
            client.Timeout = TimeSpan.FromSeconds(10);
            var url = $"http://videoclu.imaleex.com/api/videoplayer/{linkId}?token={token}";
            var resp = client.GetAsync(url).GetAwaiter().GetResult();
            if (!resp.IsSuccessStatusCode)
            {
                Terminal.WriteError($"Failed to resolve Gandalf link (HTTP {(int)resp.StatusCode}).");
                return null;
            }
            var json = resp.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;
            var gv = new GandalfVideo
            {
                Name = root.TryGetProperty("Name", out var nameEl) ? nameEl.GetString() ?? "" : "",
                Url  = root.TryGetProperty("Url",  out var urlEl)  ? urlEl.GetString()  ?? "" : "",
                WatchedTime = root.TryGetProperty("WatchedTime", out var wtEl) && wtEl.ValueKind == JsonValueKind.Number ? wtEl.GetInt32() : 0
            };
            if (string.IsNullOrEmpty(gv.Url))
            {
                Terminal.WriteError("Gandalf API did not return a valid url.");
                return null;
            }
            return gv;
        }
        catch (Exception ex)
        {
            Terminal.WriteError($"Error resolving Gandalf link: {ex.Message}");
            return null;
        }
    }

    public static void ReportWatchTime(string linkId, string token, int seconds)
    {
        try
        {
            using var client = new HttpClient();
            client.Timeout = TimeSpan.FromSeconds(10);
            var url = "http://videoclu.imaleex.com/api/videoplayer/watchTime?token=" + token;
            var payload = JsonSerializer.Serialize(new
            {
                linkId = linkId,
                seconds = seconds
            });
            using var content = new StringContent(payload, Encoding.UTF8, "application/json");
            var resp = client.PostAsync(url, content).GetAwaiter().GetResult();
            if (!resp.IsSuccessStatusCode)
            {
                Terminal.WriteError($"Failed to report watch time (HTTP {(int)resp.StatusCode}).");
            }
        }
        catch (Exception ex)
        {
            Terminal.WriteError($"Error reporting watch time: {ex.Message}");
        }
    }
}
