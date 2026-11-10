namespace Sortify.Core.Services;

public interface ISystemNotificationService
{
    /// <summary>Requests a native notification without affecting the completed operation.</summary>
    Task ShowAsync(string title, string message);
}
