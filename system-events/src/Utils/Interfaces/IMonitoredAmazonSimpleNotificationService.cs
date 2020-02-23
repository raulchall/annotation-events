using System.Threading;
using System.Threading.Tasks;
using Amazon.SimpleNotificationService.Model;

namespace SystemEvents.Utils.Interfaces
{
    public interface IMonitoredAmazonSimpleNotificationService
    {
        Task<PublishResponse> PublishAsync(string topicArn, string message, CancellationToken cancellationToken);
    }
}