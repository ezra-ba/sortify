using Sortify.Core.Models;

namespace Sortify.Infrastructure.Persistence;

internal sealed record SettingsDocument(
    int Version,
    AppSettings? Settings);