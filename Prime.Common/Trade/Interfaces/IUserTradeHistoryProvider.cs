using System.Collections.Generic;
using System.Threading.Tasks;

namespace Prime.Common
{
    public interface IPrivateTradeHistoryProvider : INetworkProviderPrivate
    {
        Task<TradeOrders> GetPrivateTradeHistoryAsync(TradeHistoryContext context);
    }
}
