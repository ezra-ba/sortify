using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Sortify.App.ViewModels;

public partial class DownloadStatisticsViewModel : ViewModelBase
{
    public DownloadStatisticsViewModel()
    {
        Periods = ["Tag", "Woche", "Monat", "Jahr", "Gesamt"];
        Kpis = [new("Downloads gesamt", "1.284", "⇩"), new("Diese Woche", "126", "◷"), new("Erfolgreich sortiert", "98,4 %", "✓"), new("Häufigste Quelle", "github.com", "↗", "412 Downloads")];
        Timeline = [
            new("01", 18, 34), new("05", 26, 47), new("09", 22, 40),
            new("13", 38, 63), new("17", 31, 54), new("21", 46, 78), new("25", 42, 71), new("30", 52, 84)];
        Sources = [
            new("github.com", 412, "#7657D9", "GH"),
            new("microsoft.com", 286, "#5B8DEF", "MS"),
            new("npmjs.com", 238, "#D97757", "NP"),
            new("andere", 348, "#9CA3AF", "…")];
        TopSources = [
            new("github.com", 412, "GH"), new("microsoft.com", 286, "MS"),
            new("npmjs.com", 238, "NP"), new("google.com", 192, "GO"),
            new("stackoverflow.com", 156, "SO")];
    }

    public ObservableCollection<string> Periods { get; }
    public ObservableCollection<DownloadKpi> Kpis { get; }
    public ObservableCollection<DownloadTimelinePoint> Timeline { get; }
    public ObservableCollection<DownloadSource> Sources { get; }
    public ObservableCollection<TopDownloadSource> TopSources { get; }

    [ObservableProperty]
    public partial string SelectedPeriod { get; set; } = "Monat";
}

public sealed record DownloadTimelinePoint(string Label, int Downloads, int ChartHeight);
public sealed record DownloadKpi(string Title, string Value, string Icon, string? Detail = null);
public sealed record DownloadSource(string Name, int Downloads, string Color, string Initials)
{
    public double Share => Downloads / 1284d * 100;
    public string ShareText => $"{Share:0.#} %";
}
public sealed record TopDownloadSource(string Name, int Downloads, string Initials)
{
    public double BarWidth => Downloads / 412d * 100;
}
