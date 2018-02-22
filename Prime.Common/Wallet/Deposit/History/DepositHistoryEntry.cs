using System;

namespace Prime.Common
{
    public class DepositHistoryEntry
    {
        public string DepositRemoteId { get; set; }
        public Money Price { get; set; }
        public string Address { get; set; }
        public Money Fee { get; set; }

        public DepositStatus DepositStatus { get; set; }

        public DateTime CreatedTimeUtc { get; set; }
    }
}
