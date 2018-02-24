using Prime.Common;
using Prime.Utility;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Prime.Plugins.Services.Binance
{
    public partial class BinanceProvider : IWithdrawalHistoryProvider, IDepositHistoryProvider, IPrivateTradeHistoryProvider
    {
        public async Task<List<TradeHistoryEntry>> GetPrivateTradeHistoryAsync(TradeHistoryContext context)
        {
            var api = ApiProvider.GetApi(context);

            var rRaw = await api.GetTradeHistoryAsync(context.AssetPair.ToRemotePair(this)).ConfigureAwait(false);
            CheckResponseErrors(rRaw);

            var r = rRaw.GetContent();

            var history = new List<TradeHistoryEntry>();

            foreach (var rTrade in r)
            {
                history.Add(new TradeHistoryEntry(rTrade.id.ToString(),
                                                new Money(rTrade.price, context.AssetPair.Asset2),
                                                new Money(rTrade.qty, context.AssetPair.Asset1),
                                                new Money(rTrade.commission, Assets.I.Get(rTrade.commissionAsset, this)),
                                                rTrade.isBuyer,
                                                rTrade.isMaker,
                                                rTrade.time.ToUtcDateTime(),
                                                rTrade.isBestMatch));
            }

            return history;
        }

        public async Task<List<DepositHistoryEntry>> GetDepositHistoryAsync(DepositHistoryContext context)
        {
            var api = ApiProvider.GetApi(context);
            var remoteCode = context.Asset == null ? null : context.Asset.ToRemoteCode(this);
            var rRaw = await api.GetDepositHistoryAsync(remoteCode).ConfigureAwait(false);
            CheckResponseErrors(rRaw);

            var r = rRaw.GetContent();

            var history = new List<DepositHistoryEntry>();

            foreach (var rDeposit in r.depositList)
            {
                var localAsset = Assets.I.Get(rDeposit.asset, this);

                history.Add(new DepositHistoryEntry()
                {
                    Price = new Money(rDeposit.amount, localAsset),
                    Fee = new Money(0m, localAsset), //TODO: Calculate fees using spec
                    CreatedTimeUtc = rDeposit.insertTime.ToUtcDateTime(),
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
            var remoteCode = context.Asset == null ? null : context.Asset.ToRemoteCode(this);
            var rRaw = await api.GetWitdrawHistoryAsync(remoteCode).ConfigureAwait(false);
            CheckResponseErrors(rRaw);

            var r = rRaw.GetContent();

            var history = new List<WithdrawalHistoryEntry>();

            foreach (var rHistory in r.withdrawList)
            {
                var localAsset = Assets.I.Get(rHistory.asset, this);

                history.Add(new WithdrawalHistoryEntry()
                {
                    Price = new Money(rHistory.amount, localAsset),
                    Fee = new Money(0m, localAsset), //TODO: Calculate fees using spec
                    CreatedTimeUtc = rHistory.applyTime.ToUtcDateTime(),
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
