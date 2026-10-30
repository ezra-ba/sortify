using System.Text.Json;
using Sortify.Core.Models;
using Sortify.Core.Rules;
using Sortify.Infrastructure.Ai;

namespace Sortify.Infrastructure.Tests;

public sealed class AiSuggestionTests
{
    private static AiSuggestionRequest Request(string name = "SQL.pdf") => new(
        new FileDescriptor(@"C:\private\" + name, Path.GetFileNameWithoutExtension(name),
            Path.GetExtension(name), "private", 1234, DateTimeOffset.UnixEpoch),
        new AppSettings(@"C:\private", null, false, []) {
            AiMode = AiSuggestionMode.Fallback, AiMetadataConsent = true,
            TargetFolders = [new(Guid.Parse("11111111-1111-4111-8111-111111111111"), @"C:\secret", "Schule")]
        }, new(RuleEvaluationStatus.NoMatch, [], []));

    [Fact]
    public void PromptContainsExamplesAndMetadataButNoAbsolutePaths()
    {
        var messages = AiPromptBuilder.Build(Request());
        Assert.Equal(8, messages.Count);
        Assert.Equal(3, messages.Count(message => message.Role == "assistant"));
        var actual = messages[^1].Content;
        Assert.DoesNotContain("private", actual);
        Assert.DoesNotContain("secret", actual);
        using var context = JsonDocument.Parse(actual);
        Assert.Equal(1234, context.RootElement.GetProperty("file").GetProperty("sizeBytes").GetInt32());
        Assert.Single(context.RootElement.GetProperty("targets").EnumerateArray());
    }

    [Theory]
    [InlineData(AiSuggestionMode.Disabled, true, false)]
    [InlineData(AiSuggestionMode.Fallback, false, false)]
    [InlineData(AiSuggestionMode.Fallback, true, true)]
    [InlineData(AiSuggestionMode.Supplement, true, true)]
    public void ModeAndConsentGateRequests(AiSuggestionMode mode, bool consent, bool expected)
    {
        var request = Request();
        Assert.Equal(expected, AiSuggestionPolicy.ShouldRequest(
            request.Settings with { AiMode = mode, AiMetadataConsent = consent }, request.LocalResult));
    }

    [Theory]
    [InlineData("SQL.pdf", true)]
    [InlineData("SQL.exe", false)]
    [InlineData("../SQL.pdf", false)]
    [InlineData("SQL.PDF", false)]
    [InlineData("CON.pdf", false)]
    public void ReplyPreservesPortableNameAndExactExtension(string name, bool accepted)
    {
        var request = Request();
        var content = JsonSerializer.Serialize(new { status = "suggested",
            targetFolderId = request.Settings.TargetFolders[0].Id, proposedFileName = name,
            reason = "Semantischer Vorschlag", appliedRuleIds = Array.Empty<Guid>() });
        Assert.Equal(accepted, NvidiaAiSuggestionService.ValidateReply(content, request).Suggestion is not null);
    }

    [Theory]
    [InlineData("{}")]
    [InlineData("null")]
    [InlineData("not json")]
    public void InvalidRepliesDoNotProduceSuggestions(string content) =>
        Assert.Null(NvidiaAiSuggestionService.ValidateReply(content, Request()).Suggestion);

    [Fact]
    public void JsonCodeFenceIsAcceptedWithoutRelaxingSuggestionValidation()
    {
        var request = Request();
        var valid = JsonSerializer.Serialize(new { status = "suggested",
            targetFolderId = request.Settings.TargetFolders[0].Id,
            proposedFileName = "SQL.pdf", reason = "Semantischer Vorschlag",
            appliedRuleIds = Array.Empty<Guid>() });
        Assert.NotNull(NvidiaAiSuggestionService.ValidateReply("```json\n" + valid + "\n```", request).Suggestion);
        Assert.Null(NvidiaAiSuggestionService.ValidateReply("Erklärung\n" + valid, request).Suggestion);
    }

    [Fact]
    public async Task MissingKeyDoesNotRequireNetwork()
    {
        using var client = new HttpClient();
        var result = await new NvidiaAiSuggestionService(client, () => null).SuggestAsync(Request());
        Assert.Null(result.Suggestion);
        Assert.Contains("NVIDIA_API_KEY", result.Message);
    }

    [AiSimulationFact]
    public async Task RealHttpClientHandlesSuccessFailuresTimeoutAndCancellation()
    {
        using var client = new HttpClient(new HttpClientHandler { AllowAutoRedirect = false }) { Timeout = Timeout.InfiniteTimeSpan };
        var endpoint = new Uri(Environment.GetEnvironmentVariable("SORTIFY_TEST_AI_ENDPOINT")!);
        var service = new NvidiaAiSuggestionService(client, () => "sortify-test-only", endpoint);
        Assert.NotNull((await service.SuggestAsync(Request())).Suggestion);
        foreach (var name in new[] { "unauthorized.pdf", "limited.pdf", "server.pdf", "invalid.pdf", "large.pdf", "truncated.pdf" })
            Assert.Null((await service.SuggestAsync(Request(name))).Suggestion);
        var fast = new NvidiaAiSuggestionService(client, () => "sortify-test-only", endpoint, TimeSpan.FromMilliseconds(150));
        Assert.Contains("Zeitlimit", (await fast.SuggestAsync(Request("timeout.pdf"))).Message);
        using var cancellation = new CancellationTokenSource(TimeSpan.FromMilliseconds(150));
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => service.SuggestAsync(Request("cancel.pdf"), cancellation.Token));
    }
}

public sealed class AiSimulationFactAttribute : FactAttribute
{
    public AiSimulationFactAttribute()
    {
        if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("SORTIFY_TEST_AI_ENDPOINT")))
            Skip = "Start the local Postman simulation and set SORTIFY_TEST_AI_ENDPOINT.";
    }
}
