using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using LiteDB;
using Prime.Common;
using Prime.Common.Exchange;
using Prime.Common.Wallet.Withdrawal.History;
using Prime.Utility;
using Prime.Common.Wallet.Deposit.History;

namespace Prime.Plugins.Services.Binance
{
    public partial class BinanceProvider : IWithdrawalHistoryProvider, IDepositlHistoryProvider
    {
        public async Task<List<DepositHistoryEntry>> GetDepositHistoryAsync(DepositHistoryContext context)
        {
            var api = ApiProvider.GetApi(context);

            var rRaw = await api.GetDepositHistoryAsync().ConfigureAwait(false);
            CheckResponseErrors(rRaw);

            var r = rRaw.GetContent();

            var history = new List<DepositHistoryEntry>();

            foreach (var rDeposit in r.depositList)
            {
                history.Add(new DepositHistoryEntry()
                {
                    Price = new Money(rDeposit.amount, context.Asset),
                    Fee = new Money(0m, context.Asset), //TODO: Calculate fees using spec
                    CreatedTimeUtc = (rDeposit.insertTime / 1000).ToUtcDateTime(),
                    Address = rDeposit.address,
                    DepositRemoteId = rDeposit.txId,
                    DepositStatus = rDeposit.status == 0 ? DepositStatus.Confirmed : DepositStatus.Completed
                });
            }

            return history;
        }

        public async Task<List<WithdrawalHistoryEntry>> GetWithdrawalHistoryAsync(WithdrawalHistoryContext context)
        {
            //TODO: Check for supported assets and throw new AssetPairNotSupportedException(context.Asset.ShortCode, this);

            var api = ApiProvider.GetApi(context);
            var remoteCode = context.Asset.ToRemoteCode(this);
            var rRaw = await api.GetWitdrawHistoryAsync(remoteCode).ConfigureAwait(false);
            CheckResponseErrors(rRaw);

            var r = rRaw.GetContent();

            var history = new List<WithdrawalHistoryEntry>();

            foreach (var rHistory in r.withdrawList)
            {
                history.Add(new WithdrawalHistoryEntry()
                {
                    Price = new Money(rHistory.amount, context.Asset),
                    Fee = new Money(0m, context.Asset), //TODO: Calculate fees using spec
                    CreatedTimeUtc = (rHistory.applyTime / 1000).ToUtcDateTime(),
                    Address = rHistory.address,
                    WithdrawalRemoteId = rHistory.txId,
                    WithdrawalStatus = ParseWithdrawalStatus(rHistory.status)
                });
            }

            return history;
        }

        private WithdrawalStatus ParseWithdrawalStatus(int statusRaw)
        {
            switch (statusRaw)
            {
                case 0: //email sent
                case 2: //awaiting approval
                    return WithdrawalStatus.Awaiting;
                case 1: //canceled
                case 3: //rejected
                case 5: //failure
                    return WithdrawalStatus.Canceled;
                case 4:
                    return WithdrawalStatus.Confirmed;
                case 6:
                    return WithdrawalStatus.Completed;
                default:
                    throw new ApiResponseException($"A withdrawal status of {statusRaw} from Binance could not be parsed.");
            }
        }
    }
}
