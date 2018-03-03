using System.Collections.Generic;

namespace Prime.Common
{
    public class DepositHistory : List<DepositHistoryEntry>
    {
        public INetworkProvider Provider { get; private set; }

        public DepositHistory(INetworkProvider provider)
        {
            this.Provider = provider;
        }        
    }
}
