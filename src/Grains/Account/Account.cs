using System;
using EventSourcing;

namespace Grains.Account
{
    #region Events
    public class Deposited: IAccountEvent
    {
        public string Type { get; set;} = nameof(Deposited);
        public decimal Amount { get; set; }
        public int AccountId { get; set; }
    }

    public class Withdrawn: IAccountEvent
    {
        public string Type { get; set; } = nameof(Withdrawn);
        public decimal Amount { get; set; }
        public int AccountId { get; set; }
    }

    public class BalanceRetrieved: IAccountEvent
    {
        public string Type { get; set; } = nameof(BalanceRetrieved);
        public int AccountId { get; set; }
    }

    #endregion

    public class Account : IState
    {
        public decimal Amount { get; set; }
        public int AccountId { get; set; }

        public void Init(string id)
        {
            AccountId = int.Parse(id);
        }
    }
}