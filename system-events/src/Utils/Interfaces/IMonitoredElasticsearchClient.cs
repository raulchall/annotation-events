using System;
using System.Threading;
using System.Threading.Tasks;
using Nest;
using SystemEvents.Models;

namespace SystemEvents.Utils.Interfaces
{
    public interface IMonitoredElasticsearchClient
    {
        Task<SystemEventElasticsearchDocument> GetAsync(string documentId, CancellationToken cancellationToken);
        
        Task<IIndexResponse> IndexAsync(SystemEventElasticsearchDocument document, CancellationToken cancellationToken);
        
        Task<IUpdateResponse<SystemEventElasticsearchDocument>> UpdateAsync(
            string documentId, 
            SystemEventElasticsearchPartialDocument partialDocument, 
            CancellationToken cancellationToken, 
            int retryOnConflict = 3, 
            bool retryWithPreviousIndex = false);
    }
}