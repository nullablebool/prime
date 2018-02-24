using System.Collections.Generic;
using System.Threading.Tasks;

namespace Prime.Common
{
    public interface IDepositHistoryProvider : INetworkProviderPrivate
    {
        Task<List<DepositHistoryEntry>> GetDepositHistoryAsync(DepositHistoryContext context);
    }
}