using System.Collections.Generic;
using System.Threading.Tasks;

namespace Prime.Common
{
    public interface IWithdrawalHistoryProvider : INetworkProviderPrivate
    {
        Task<List<WithdrawalHistoryEntry>> GetWithdrawalHistoryAsync(WithdrawalHistoryContext context);
    }
}