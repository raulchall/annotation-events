using System;
using System.Threading;
using System.Threading.Tasks;
using Nest;
using SystemEvents.Models;

namespace SystemEvents.Utils.Interfaces
{
    public interface IMonitoredElasticsearchClient
    {
        Task<IIndexResponse> IndexAsync(SystemEventElasticsearchDocument document, CancellationToken cancellationToken);
        
        Task<IUpdateResponse<SystemEventElasticsearchDocument>> UpdateAsync(string documentId, string indexName, SystemEventElasticsearchPartialDocument partialDocument, CancellationToken cancellationToken, int retryOnConflict = 3);
    }
}