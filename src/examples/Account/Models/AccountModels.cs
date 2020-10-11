namespace Account.Models
{
    public class AccountModelBase
    {
        public decimal Amount { get; set; }
    }
    public class AccountDepositModel : AccountModelBase { }

    public class AccountWithdrawModel : AccountModelBase { }

    public class AccountTransferModel : AccountModelBase
    {
        public int ToAccountId { get; set; }
    }
}