namespace FiapX.Application.Interfaces.Services;

public interface INotificationService
{
    Task SendProcessingCompleteNotificationAsync(string userEmail, string videoName, string downloadUrl);
    Task SendProcessingFailedNotificationAsync(string userEmail, string videoName, string errorMessage);
}
