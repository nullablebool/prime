using System.Collections.Generic;
using System.Threading.Tasks;

namespace Prime.Common
{
    public interface IWithdrawalHistoryProvider : INetworkProviderPrivate
    {
        Task<WithdrawalHistory> GetWithdrawalHistoryAsync(WithdrawalHistoryContext context);
    }
}