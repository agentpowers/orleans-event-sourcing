using System;

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

    public class Account 
    {
        public decimal Amount { get; set; }
    }
}