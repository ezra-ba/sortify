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
    [InlineData(AiSuggestionMode.Disabled, false, false)]
    [InlineData(AiSuggestionMode.Fallback, false, false)]
    [InlineData(AiSuggestionMode.Fallback, true, true)]
    [InlineData(AiSuggestionMode.Supplement, true, true)]
    public void ModeAndConsentGateRequests(AiSuggestionMode mode, bool consent, bool expected)
    {
        var request = Request();
        Assert.Equal(expected, AiSuggestionPolicy.ShouldRequest(
            request.Settings with { AiMode = mode, AiMetadataConsent = consent }, request.LocalResult));
    }

    [Fact]
    public void InvalidUnmatchedRuleDoesNotBlockAiFallback()
    {
        var request = Request() with { LocalResult = new(RuleEvaluationStatus.NoMatch, [],
            [new(Guid.NewGuid(), "InvalidRule", "FileNamePattern", "Regel ungültig")]) };
        Assert.True(AiSuggestionPolicy.ShouldRequest(request.Settings, request.LocalResult));
    }

    [Fact]
    public async Task PersonalInstructionsAreStoredAndIncludedInThePrompt()
    {
        var path = Path.Combine(Path.GetTempPath(), "sortify-ai-" + Guid.NewGuid().ToString("N"), "agents.md");
        var store = new AiInstructionsStore(path);
        try
        {
            Assert.Equal("", store.Read());
            await store.SaveAsync("PDFs zu vorhandenen Büchern zuordnen; Schulfächer bevorzugen.");
            using var context = JsonDocument.Parse(AiPromptBuilder.Build(Request(), store.Read())[^1].Content);
            Assert.Equal(store.Read(), context.RootElement.GetProperty("sortifyInstructions").GetString());
            await Assert.ThrowsAsync<ArgumentException>(() => store.SaveAsync(new string('x', AiInstructionsStore.MaxCharacters + 1)));
            Assert.Contains("Schulfächer bevorzugen", store.Read());
        }
        finally
        {
            if (File.Exists(path)) File.Delete(path);
            if (Directory.Exists(Path.GetDirectoryName(path))) Directory.Delete(Path.GetDirectoryName(path)!);
        }
    }

    [Fact]
    public void TargetTreeUsesRelativeNamesOnlyAndIsOptIn()
    {
        var root = Path.Combine(Path.GetTempPath(), "sortify-tree-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(Path.Combine(root, "school"));
        File.WriteAllText(Path.Combine(root, "school", "notes.pdf"), "Private contents");
        try
        {
            var request = Request() with { Settings = Request().Settings with
            {
                TargetFolders = [new TargetFolder(Guid.NewGuid(), root, "Schule")]
            } };
            using var without = JsonDocument.Parse(AiPromptBuilder.Build(request)[^1].Content);
            Assert.Equal(JsonValueKind.Null, without.RootElement.GetProperty("targetFileTrees").ValueKind);
            var trees = TargetTreeReader.Capture(request.Settings.TargetFolders);
            using var withTree = JsonDocument.Parse(AiPromptBuilder.Build(request, targetFileTrees: trees)[^1].Content);
            var text = withTree.RootElement.GetProperty("targetFileTrees").ToString();
            Assert.Contains("school/notes.pdf", text);
            Assert.DoesNotContain(root, text);
            Assert.DoesNotContain("Private contents", text);
        }
        finally
        {
            var expected = Path.GetFullPath(root);
            if (!expected.StartsWith(Path.GetFullPath(Path.GetTempPath()), StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("Testpfad liegt außerhalb des Temp-Ordners.");
            Directory.Delete(expected, recursive: true);
        }
    }

    [Fact]
    public void SelectedContextDirectoryIsRecursiveAndExcludesContentsAndAbsolutePath()
    {
        var root = Path.Combine(Path.GetTempPath(), "sortify-context-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(Path.Combine(root, "school", "4", "inf"));
        File.WriteAllText(Path.Combine(root, "school", "4", "inf", "lesson.pdf"), "Private contents");
        try
        {
            var tree = TargetTreeReader.CaptureContextDirectory(root);
            Assert.Contains("school/4/inf/lesson.pdf", tree.RelativeEntries);
            using var context = JsonDocument.Parse(AiPromptBuilder.Build(Request(),
                contextDirectoryTree: tree)[^1].Content);
            var transmitted = context.RootElement.GetProperty("contextDirectoryTree").ToString();
            Assert.DoesNotContain(root, transmitted);
            Assert.DoesNotContain("Private contents", transmitted);
            using var without = JsonDocument.Parse(AiPromptBuilder.Build(Request())[^1].Content);
            Assert.Equal(JsonValueKind.Null, without.RootElement.GetProperty("contextDirectoryTree").ValueKind);
        }
        finally
        {
            var expected = Path.GetFullPath(root);
            if (!expected.StartsWith(Path.GetFullPath(Path.GetTempPath()), StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("Testpfad liegt außerhalb des Temp-Ordners.");
            Directory.Delete(expected, recursive: true);
        }
    }

    [Fact]
    public void RelevantContextNamesTakePriorityOverEarlierUnrelatedEntries()
    {
        var root = Path.Combine(Path.GetTempPath(), "sortify-rank-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(Path.Combine(root, "research"));
        try
        {
            for (var index = 0; index < 1300; index++)
                File.WriteAllText(Path.Combine(root, "research", $"misc-{index:0000}.txt"), "");
            File.WriteAllText(Path.Combine(root, "research", "SQL-Fragen.pdf"), "");
            var tree = TargetTreeReader.CaptureContextDirectory(root, "SQL-Fragen.pdf");
            Assert.Contains("research/SQL-Fragen.pdf", tree.RelativeEntries);
            Assert.True(tree.Truncated);
        }
        finally { Directory.Delete(root, recursive: true); }
    }

    [Fact]
    public void UncertainDestinationIsOfferedOnlyForManualReview()
    {
        var request = Request();
        var content = JsonSerializer.Serialize(new { status = "uncertain",
            targetFolderId = request.Settings.TargetFolders[0].Id, proposedFileName = "SQL.pdf",
            reason = "Name deutet auf Schule, könnte aber auch Arbeit sein.", appliedRuleIds = Array.Empty<Guid>() });
        var result = NvidiaAiSuggestionService.ValidateReply(content, request);
        Assert.True(result.Suggestion?.NeedsReview);
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
        var service = new NvidiaAiSuggestionService(client, () => "sortify-test-only", endpoint,
            getInstructions: () => "Schulfächer bevorzugen.");
        Assert.NotNull((await service.SuggestAsync(Request())).Suggestion);
        var treeRoot = Path.Combine(Path.GetTempPath(), "sortify-tree-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(Path.Combine(treeRoot, "school"));
        File.WriteAllText(Path.Combine(treeRoot, "school", "notes.pdf"), "Private contents");
        try
        {
            var treeRequest = Request("tree.pdf");
            treeRequest = treeRequest with { Settings = treeRequest.Settings with
            {
                AiShareTargetTrees = true,
                TargetFolders = [treeRequest.Settings.TargetFolders[0] with { Path = treeRoot }]
            } };
            Assert.NotNull((await service.SuggestAsync(treeRequest)).Suggestion);
        }
        finally
        {
            var expected = Path.GetFullPath(treeRoot);
            if (!expected.StartsWith(Path.GetFullPath(Path.GetTempPath()), StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("Testpfad liegt außerhalb des Temp-Ordners.");
            Directory.Delete(expected, recursive: true);
        }
        var contextRoot = Path.Combine(Path.GetTempPath(), "sortify-context-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(Path.Combine(contextRoot, "school", "4"));
        File.WriteAllText(Path.Combine(contextRoot, "school", "4", "lesson.pdf"), "Private contents");
        try
        {
            var contextRequest = Request("context.pdf");
            contextRequest = contextRequest with { Settings = contextRequest.Settings with
            {
                AiShareContextDirectory = true, AiContextDirectoryPath = contextRoot
            } };
            Assert.NotNull((await service.SuggestAsync(contextRequest)).Suggestion);
        }
        finally
        {
            var expected = Path.GetFullPath(contextRoot);
            if (!expected.StartsWith(Path.GetFullPath(Path.GetTempPath()), StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("Testpfad liegt außerhalb des Temp-Ordners.");
            Directory.Delete(expected, recursive: true);
        }
        foreach (var name in new[] { "unauthorized.pdf", "limited.pdf", "server.pdf", "invalid.pdf", "large.pdf", "truncated.pdf" })
            Assert.Null((await service.SuggestAsync(Request(name))).Suggestion);
        var fast = new NvidiaAiSuggestionService(client, () => "sortify-test-only", endpoint,
            TimeSpan.FromMilliseconds(150), () => "Schulfächer bevorzugen.");
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
