using System.Threading;
using System.Threading.Tasks;
using SystemEvents.Models;

namespace SystemEvents.Utils.Interfaces
{
    public interface ICategorySubscriptionNotifier
    {
         Task OnEventCreated(string category, SystemEventElasticsearchDocument document, CancellationToken cancellationToken);
         Task OnEventStarted(string category, SystemEventElasticsearchDocument document, CancellationToken cancellationToken);
         Task OnEventFinished(string category, SystemEventElasticsearchDocument document, CancellationToken cancellationToken);
    }
}