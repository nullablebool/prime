using System.Collections.Generic;
using System.Threading.Tasks;
using Prime.Common.Wallet.Deposit.History;

namespace Prime.Common
{
    public interface IDepositlHistoryProvider
    {
        Task<List<DepositHistoryEntry>> GetDepositHistoryAsync(DepositHistoryContext context);
    }
}