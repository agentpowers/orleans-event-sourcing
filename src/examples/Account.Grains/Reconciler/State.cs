using System;
using System.Collections.Generic;
using EventSourcing;
using EventSourcingGrains;

namespace Account.Grains.Reconciler
{
    public class AccountReconciler: IState
    {
        public long LastProcessedEventId { get; set; }

        public void Init(string key)
        {
        }
    }
}