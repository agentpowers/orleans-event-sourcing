using System;
using System.Collections.Generic;
using EventSourcing;
using EventSourcingGrains;

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