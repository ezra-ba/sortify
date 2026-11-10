using Sortify.Core.Models;
using Sortify.Infrastructure.Persistence;

namespace Sortify.Infrastructure.Tests;

public sealed class RuleTransferServiceTests
{
    [Fact]
    public async Task ExportAndImportKeepExistingSettingsAndAvoidDuplicates()
    {
        var root = Path.Combine(Path.GetTempPath(), "sortify-rules-" + Guid.NewGuid());
        var targetPath = Directory.CreateDirectory(Path.Combine(root, "target")).FullName;
        try
        {
            var target = new TargetFolder(Guid.NewGuid(), targetPath, "Schule");
            var rule = new SortRule(Guid.NewGuid(), "PDFs", "^invoice", [".pdf"], null,
                targetPath, "sorted_{name}{ext}", 0, true) { TargetFolderId = target.Id };
            var exported = new AppSettings(null, null, false, [rule]) { TargetFolders = [target] };
            var file = Path.Combine(root, "rules.json");
            await RuleTransferService.ExportAsync(file, exported);

            var document = await RuleTransferService.ReadAsync(file);
            var existing = new AppSettings(root, null, false, []) { DarkMode = true };
            var (imported, count) = RuleTransferService.Merge(existing, document);
            Assert.Equal(1, count);
            Assert.True(imported.DarkMode);
            Assert.Equal(root, imported.WatchedFolderPath);
            Assert.Equal(targetPath, Assert.Single(imported.TargetFolders).Path);
            Assert.Equal(Assert.Single(imported.TargetFolders).Id, Assert.Single(imported.Rules).TargetFolderId);

            var (again, secondCount) = RuleTransferService.Merge(imported, document);
            Assert.Equal(0, secondCount);
            Assert.Single(again.Rules);
            Assert.Single(again.TargetFolders);
        }
        finally { Directory.Delete(root, true); }
    }

    [Fact]
    public async Task MissingTargetAndConflictingRuleIdAreRejected()
    {
        var root = Path.Combine(Path.GetTempPath(), "sortify-rules-" + Guid.NewGuid());
        var targetPath = Directory.CreateDirectory(Path.Combine(root, "target")).FullName;
        try
        {
            var target = new TargetFolder(Guid.NewGuid(), targetPath, "Schule");
            var rule = new SortRule(Guid.NewGuid(), "PDFs", "^invoice", [".pdf"], null,
                targetPath, null, 0, true) { TargetFolderId = target.Id };
            var document = new RuleTransferDocument(1, [target], [rule]);
            var existing = new AppSettings(null, null, false, []);
            Directory.Delete(targetPath);
            Assert.Throws<InvalidDataException>(() => RuleTransferService.Merge(existing, document));

            Directory.CreateDirectory(targetPath);
            var conflicting = existing with { Rules = [rule with { Name = "Andere Regel" }], TargetFolders = [target] };
            Assert.Throws<InvalidDataException>(() => RuleTransferService.Merge(conflicting, document));
        }
        finally { Directory.Delete(root, true); }
    }
}
