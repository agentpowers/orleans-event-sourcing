using System;
using EventSourcing;

namespace Grains.Account
{
    #region Events
    public class Deposited: AccountEvent
    {
        public override string Type { get; set;} = nameof(Deposited);
        public decimal Amount { get; set; }
    }

    public class Withdrawn: AccountEvent
    {
        public override string Type { get; set; } = nameof(Withdrawn);
        public decimal Amount { get; set; }
    }

    public class BalanceRetrieved: AccountEvent
    {
        public override string Type { get; set; } = nameof(BalanceRetrieved);
    }

    #endregion

    public class Account : State
    {
        public decimal Amount { get; set; }
        public int AccountId { get; set; }

        public override void Init(string id)
        {
            AccountId = int.Parse(id);
        }
    }
}