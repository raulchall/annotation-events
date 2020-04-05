using System.Collections.Generic;
using System.Threading.Tasks;
using SystemEvents.Api.Client.CSharp.Contracts;

namespace SlackAppBackend.Utils.Interfaces
{
    public interface IMonitoredSystemEventServiceClient
    {
         Task<EventResponse> EventPostAsync(EventRequestModel model);
         Task EventEndPostAsync(string eventId);
         Task<ICollection<Category>> CategoryAllGetAsync();
    }
}