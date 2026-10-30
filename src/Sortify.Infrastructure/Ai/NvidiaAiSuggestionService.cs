using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;
using Sortify.Core.Models;
using Sortify.Core.Rules;
using Sortify.Core.Services;

namespace Sortify.Infrastructure.Ai;

public sealed class NvidiaAiSuggestionService : IAiSuggestionService
{
    public static readonly Uri Endpoint = new("https://integrate.api.nvidia.com/v1/chat/completions");
    private static readonly SemaphoreSlim Requests = new(1, 1);
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = false,
        RespectRequiredConstructorParameters = true,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
        MaxDepth = 32
    };
    private readonly HttpClient client;
    private readonly Func<string?> getKey;
    private readonly Uri endpoint;
    private readonly TimeSpan timeout;

    public NvidiaAiSuggestionService(HttpClient client, Func<string?> getKey,
        Uri? endpoint = null, TimeSpan? timeout = null)
    {
        this.client = client;
        this.getKey = getKey;
        this.endpoint = endpoint ?? Endpoint;
        this.timeout = timeout ?? TimeSpan.FromSeconds(25);
        if (this.endpoint != Endpoint && !(this.endpoint.IsLoopback && this.endpoint.Scheme == "http"))
            throw new ArgumentException("Nur NVIDIA oder ein lokaler Testserver ist zulässig.", nameof(endpoint));
        if (this.timeout <= TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(timeout));
    }

    public async Task<AiSuggestionResult> SuggestAsync(AiSuggestionRequest request,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (!AiSuggestionPolicy.ShouldRequest(request.Settings, request.LocalResult))
            return new(null, "KI ist für diese Datei nicht aktiviert.");
        var key = getKey();
        if (string.IsNullOrWhiteSpace(key))
            return new(null, "NVIDIA_API_KEY fehlt. Der lokale Vorschlag bleibt verfügbar.");

        using var deadline = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        deadline.CancelAfter(timeout);
        var entered = false;
        try
        {
            // Bound concurrent traffic and include queue time in the deadline. No automatic retries.
            await Requests.WaitAsync(deadline.Token).ConfigureAwait(false);
            entered = true;
            var payload = JsonSerializer.SerializeToUtf8Bytes(new
            {
                model = AiPromptBuilder.Model, messages = AiPromptBuilder.Build(request),
                temperature = 0.2, top_p = 0.95, max_tokens = 2048,
                chat_template_kwargs = new { enable_thinking = false }, stream = false
            }, JsonOptions);
            if (payload.Length > 128 * 1024)
                return new(null, "Der vollständige KI-Kontext ist zu groß. Es wurden keine Daten übertragen.");
            using var message = new HttpRequestMessage(HttpMethod.Post, endpoint);
            message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", key.Trim());
            message.Content = new ByteArrayContent(payload);
            message.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            using var response = await client.SendAsync(message, HttpCompletionOption.ResponseHeadersRead, deadline.Token)
                .ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
                return new(null, response.StatusCode switch
                {
                    HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden => "NVIDIA hat den Zugriff abgelehnt. Bitte API-Schlüssel prüfen.",
                    HttpStatusCode.TooManyRequests => "Das NVIDIA-Nutzungslimit wurde erreicht. Der lokale Vorschlag bleibt verfügbar.",
                    _ => "Der KI-Dienst ist derzeit nicht verfügbar. Der lokale Vorschlag bleibt verfügbar."
                });

            await using var stream = await response.Content.ReadAsStreamAsync(deadline.Token).ConfigureAwait(false);
            using var buffer = new MemoryStream();
            var chunk = new byte[4096];
            int read;
            while ((read = await stream.ReadAsync(chunk, deadline.Token).ConfigureAwait(false)) > 0)
            {
                if (buffer.Length + read > 64 * 1024)
                    return new(null, "Die KI-Antwort ist zu groß und wurde verworfen.");
                buffer.Write(chunk, 0, read);
            }
            using var document = JsonDocument.Parse(buffer.ToArray(), new JsonDocumentOptions { MaxDepth = 32 });
            var choice = document.RootElement.GetProperty("choices")[0];
            if (choice.GetProperty("finish_reason").GetString() != "stop")
                return new(null, "Die KI-Antwort ist unvollständig und wurde verworfen.");
            var content = choice.GetProperty("message").GetProperty("content").GetString();
            return ValidateReply(content ?? "", request);
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            return new(null, "KI-Zeitlimit erreicht. Der lokale Vorschlag bleibt verfügbar.");
        }
        catch (Exception error) when (error is JsonException or KeyNotFoundException
            or InvalidOperationException or IndexOutOfRangeException or ArgumentException)
        {
            return new(null, "Die KI-Antwort oder Konfiguration ist ungültig. Der lokale Vorschlag bleibt verfügbar.");
        }
        catch (Exception error) when (error is HttpRequestException or IOException)
        {
            return new(null, "Der KI-Dienst ist nicht erreichbar. Der lokale Vorschlag bleibt verfügbar.");
        }
        finally
        {
            if (entered)
                Requests.Release();
        }
    }

    public static AiSuggestionResult ValidateReply(string content, AiSuggestionRequest request)
    {
        Reply? reply;
        try { reply = JsonSerializer.Deserialize<Reply>(UnwrapJson(content), JsonOptions); }
        catch (JsonException) { return new(null, "Die KI hat keinen gültigen JSON-Vorschlag geliefert."); }
        if (reply is null || string.IsNullOrWhiteSpace(reply.Reason) || reply.Reason.Length > 500
            || reply.AppliedRuleIds is null || reply.Reason.Any(c => char.IsControl(c) && c != '\n'))
            return new(null, "Die KI-Antwort enthält ungültige Angaben.");
        if (reply.Status == "uncertain" && reply.TargetFolderId is null && reply.ProposedFileName is null
            && reply.AppliedRuleIds.Length == 0)
            return new(null, "KI: Keine ausreichend begründete Zuordnung. Bitte manuell prüfen.");
        if (reply.Status != "suggested" || reply.TargetFolderId is not { } targetId
            || !request.Settings.TargetFolders.Any(target => target.Id == targetId)
            || string.IsNullOrEmpty(reply.ProposedFileName)
            || reply.ProposedFileName.Length > 255
            || !PortableNameValidator.IsValid(reply.ProposedFileName)
            || !string.Equals(Path.GetExtension(reply.ProposedFileName), request.File.Extension, StringComparison.Ordinal)
            || string.IsNullOrWhiteSpace(Path.GetFileNameWithoutExtension(reply.ProposedFileName))
            || Path.GetFileNameWithoutExtension(reply.ProposedFileName).StartsWith(' ')
            || Path.GetFileNameWithoutExtension(reply.ProposedFileName).EndsWith(' '))
            return new(null, "Das KI-Ziel oder der Dateiname ist unzulässig. Der Vorschlag wurde verworfen.");
        var matchedRuleIds = request.LocalResult.Candidates.SelectMany(candidate => candidate.Rules).Select(rule => rule.Id).ToHashSet();
        if (reply.AppliedRuleIds.Any(id => !matchedRuleIds.Contains(id)
            || !request.Settings.Rules.Any(rule => rule.Id == id && rule.IsEnabled)))
            return new(null, "Die KI nennt keine tatsächlich passende aktive Regel. Der Vorschlag wurde verworfen.");
        return new(new(targetId, reply.ProposedFileName, reply.Reason.Trim(), reply.AppliedRuleIds), "KI-Vorschlag verfügbar.");
    }

    private sealed record Reply(string Status, Guid? TargetFolderId, string? ProposedFileName,
        string Reason, Guid[] AppliedRuleIds);

    private static string UnwrapJson(string content)
    {
        var trimmed = content.Trim();
        if (!trimmed.StartsWith("```", StringComparison.Ordinal)) return trimmed;
        var firstLineEnd = trimmed.IndexOf('\n');
        if (firstLineEnd < 0 || !trimmed.EndsWith("```", StringComparison.Ordinal)) return trimmed;
        var label = trimmed[3..firstLineEnd].Trim();
        if (label.Length != 0 && !label.Equals("json", StringComparison.OrdinalIgnoreCase)) return trimmed;
        return trimmed[(firstLineEnd + 1)..^3].Trim();
    }
}
