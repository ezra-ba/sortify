using System.Text.Json;
using Sortify.Core.Models;
using Sortify.Core.Rules;
using Sortify.Infrastructure.Persistence;

namespace Sortify.Infrastructure.Tests;

public sealed class SimpleRulePersistenceTests
{
    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    public async Task LegacyAndSimpleDefinitionsSurviveVersionedSaveAndReload(int inputVersion)
    {
        var root = Path.Combine(Path.GetTempPath(), "SortifySimple-" + Guid.NewGuid());
        Directory.CreateDirectory(root);
        var path = Path.Combine(root, "settings.json");
        JsonSettingsStore Store() => new(path, new SettingsValidator(new RuleValidator()));
        try
        {
            var target = new TargetFolder(Guid.NewGuid(), root, "School");
            var legacy = new SortRule(Guid.NewGuid(), "Legacy", "^old", [".docx"], null, "", "{name}{ext}", 4, true)
                { TargetFolderId = target.Id };
            FileNameCondition[] conditions = [new(FileNameConditionKind.Contains, "Fragenkatalog"), new(FileNameConditionKind.Contains, "CSS")];
            var simple = legacy with { Id = Guid.NewGuid(), Name = "Simple", Priority = 0,
                FileNamePattern = SimpleRuleCompiler.Compile(conditions).Pattern!, NameConditions = conditions };
            var original = new AppSettings(root, null, false, inputVersion < 4 ? [legacy] : [legacy, simple])
                { TargetFolders = [target] };
            var json = JsonSerializer.Serialize(new { Version = inputVersion, Settings = original });
            await File.WriteAllTextAsync(path, json);
            var loaded = await Store().LoadAsync();
            Assert.Equal(SettingsLoadStatus.Loaded, loaded.Status);
            Assert.Equal(json, await File.ReadAllTextAsync(path));
            Assert.Null(loaded.Settings!.Rules[0].NameConditions);
            Assert.True((await Store().SaveAsync(loaded.Settings)).IsSuccess);
            using var document = JsonDocument.Parse(await File.ReadAllTextAsync(path));
            Assert.Equal(4, document.RootElement.GetProperty("Version").GetInt32());
            var reloaded = (await Store().LoadAsync()).Settings!;
            Assert.Equal(legacy with { AllowedExtensions = reloaded.Rules[0].AllowedExtensions }, reloaded.Rules[0]);
            if (inputVersion < 4) return;
            Assert.Equal(conditions, reloaded.Rules[1].NameConditions!.ToArray());
            var result = new RuleEngine(new RuleValidator(), new RuleTargetValidator()).Evaluate(
                new("/source", "Fragenkatalog_CSS", ".DOCX", "Fragenkatalog_CSS.DOCX", 2, DateTimeOffset.UnixEpoch),
                reloaded.Rules, reloaded.TargetFolders);
            Assert.Equal(simple.Id, Assert.Single(Assert.Single(result.Candidates).Rules).Id);
        }
        finally { Directory.Delete(root, true); }
    }
}
