using System.Collections.Generic;
using System.Threading.Tasks;

namespace Prime.Common
{
    public interface IDepositHistoryProvider : INetworkProviderPrivate
    {
        Task<DepositHistory> GetDepositHistoryAsync(DepositHistoryContext context);
    }
}