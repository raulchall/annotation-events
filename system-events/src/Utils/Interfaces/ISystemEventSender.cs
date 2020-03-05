using System.Threading;
using System.Threading.Tasks;
using Nest;
using SystemEvents.Models;

namespace SystemEvents.Utils.Interfaces
{
    public interface ISystemEventSender
    {
        bool Validate(EventRequestModel model, out string reason);
        Task<string> Create(EventRequestModel model, CancellationToken cancellationToken);
        Task<string> Start(EventRequestModel model, CancellationToken cancellationToken);
        Task End(string systemEventId, CancellationToken cancellationToken);
        Task<SystemEventElasticsearchDocument> Get(string systemEventId, CancellationToken cancellationToken);
    }
}