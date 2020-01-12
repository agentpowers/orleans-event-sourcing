using System;

namespace Events
{
    # region Commands
    public interface IAccountCommands
    {
        long AccountId { get;set; }
    }

    public class Deposit : IAccountCommands
    {
        public long AccountId { get;set; }
        public decimal Amount { get; set; }
    }

    public class Withdraw : IAccountCommands
    {
        public long AccountId { get; set; }
        public decimal Amount { get; set; }
    }

    public class GetBalance: IAccountCommands 
    {
        public long AccountId { get; set; }
        public decimal Amount { get; set; }
    }

    #endregion

    #region Events
    public interface IAccountEvents
    {
        string Type { get; }
    }

    public class Deposited: IAccountEvents
    {
        public string Type { get; } = nameof(Deposited);
        public decimal Amount { get; set; }
    }

    public class Withdrawn: IAccountEvents
    {
        public string Type { get; } = nameof(Withdrawn);
        public decimal Amount { get; set; }
    }

    public class BalanceRetrieved: IAccountEvents
    {
        public string Type { get; } = nameof(BalanceRetrieved);
    }

    #endregion

    public class Account 
    {
        public decimal Amount { get; set; }
    }
}