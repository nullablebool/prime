﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nito.AsyncEx;
using Prime.Common;

namespace Prime.Tests.Providers
{
    public abstract partial class ProviderDirectTestsBase
    {
        #region Wrappers

        public virtual void TestGetTradeOrderStatus() { }
        public void TestGetTradeOrderStatus(string remoteOrderId, AssetPair market = null)
        {
            var p = IsType<IOrderLimitProvider>();
            if (p.Success)
                GetTradeOrderStatus(p.Provider, remoteOrderId, market);
        }

        public virtual void TestPlaceOrderLimit() { }
        public void TestPlaceOrderLimit(AssetPair market, bool isBuy, Money quantity, Money rate)
        {
            var p = IsType<IOrderLimitProvider>();
            if (p.Success)
                PlaceOrderLimit(p.Provider, market, isBuy, quantity, rate);
        }

        public virtual void TestGetMarketFromOrder() { }

        public void TestGetMarketFromOrder(string remoteOrderId)
        {
            var p = IsType<IOrderLimitProvider>();
            if (p.Success)
                GetMarketFromOrder(p.Provider, remoteOrderId);
        }

        public virtual void TestGetBalances()
        {
            var p = IsType<IBalanceProvider>();
            if (p.Success)
                GetBalances(p.Provider);
        }

        #endregion

        #region Tests

        private void GetTradeOrderStatus(IOrderLimitProvider provider, string remoteOrderId, AssetPair market = null)
        {
            var context = new RemoteMarketIdContext(UserContext.Current, remoteOrderId, market);

            var r = AsyncContext.Run(() => provider.GetOrderStatusAsync(context));

            Assert.IsTrue(remoteOrderId.Equals(r.RemoteOrderId, StringComparison.Ordinal), "Remote trade order ids don't match");
            Trace.WriteLine($"Remote trade order id: {r.RemoteOrderId}");
            Trace.WriteLine($"Order side: {(r.IsBuy ? "buy": "sell")}");

            if (r.IsOpen) Trace.WriteLine("Order is open");
            if (r.IsCancelRequested) Trace.WriteLine("Order is requested to be canceled");
            if (r.IsCanceled) Trace.WriteLine("Order is canceled");
            if (r.IsClosed) Trace.WriteLine("Order is closed");
            if (r.IsFound) Trace.WriteLine("Order is found");

            if (r.Rate.HasValue) Trace.WriteLine($"The rate of order is {r.Rate.Value}");
            if (r.AmountInitial.HasValue) Trace.WriteLine($"Initial amount is {r.AmountInitial.Value}");
            if (r.AmountFilled.HasValue) Trace.WriteLine($"Filled amount is {r.AmountFilled.Value}");
            if (r.AmountRemaining.HasValue) Trace.WriteLine($"Remaining amount is {r.AmountRemaining.Value}");
        }

        private void PlaceOrderLimit(IOrderLimitProvider provider, AssetPair market, bool isBuy, Money quantity, Money rate)
        {
            var context = new PlaceOrderLimitContext(UserContext.Current, market, isBuy, quantity, rate);

            var r = AsyncContext.Run(() => provider.PlaceOrderLimitAsync(context));

            Assert.IsTrue(!String.IsNullOrWhiteSpace(r.RemoteOrderGroupId));
            Trace.WriteLine($"Remote trade order id: {r.RemoteOrderGroupId}");
        }

        private void GetMarketFromOrder(IOrderLimitProvider provider, string remoteOrderId)
        {
            var context = new RemoteIdContext(UserContext.Current, remoteOrderId);

            var r = AsyncContext.Run(() => provider.GetMarketFromOrderAsync(context));

            Trace.WriteLine($"Remote trade order id is {remoteOrderId}, market is {r.Market}");

            Assert.IsTrue(r.Market != null, "Returned market is null");
            Assert.IsTrue(!Equals(r.Market, AssetPair.Empty), "Returned market is AssetPair.Empty");
        }

        private void GetBalances(IBalanceProvider provider)
        {
            var ctx = new NetworkProviderPrivateContext(UserContext.Current);

            var balances = AsyncContext.Run(() => provider.GetBalancesAsync(ctx));

            Assert.IsTrue(balances != null);

            Trace.WriteLine("User balances: ");
            foreach (var b in balances.OrderByDescending(x => x.AvailableAndReserved.ToDecimalValue()))
            {
                Trace.WriteLine($"{b.Asset}: {b.Available} available, {b.Reserved} reserved, {b.AvailableAndReserved} total");
            }
        }

        #endregion
    }
}
