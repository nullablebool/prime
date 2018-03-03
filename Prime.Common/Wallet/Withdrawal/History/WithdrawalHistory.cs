using System.Collections.Generic;

namespace Prime.Common
{
    public class WithdrawalHistory : List<WithdrawalHistoryEntry>
    {
        public INetworkProvider Provider { get; private set; }

        public WithdrawalHistory(INetworkProvider provider)
        {
            this.Provider = provider;
        }
    }
}
