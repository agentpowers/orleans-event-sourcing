using System;
using System.Collections.Generic;
using EventSourcing;

namespace Grains.Account.Reconciler
{
    public class AccountReconciler: IState
    {
        public long lastProcessedEventId { get; set; }

        public void Init(string key)
        {
        }
    }
}