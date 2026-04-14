using PawNect.Domain.Enums;

namespace PawNect.Application.Interfaces;

public interface INotificationSender
{
    Task SendOtpAsync(OtpChannel channel, string destination, string code, OtpPurpose purpose, CancellationToken cancellationToken = default);
}
