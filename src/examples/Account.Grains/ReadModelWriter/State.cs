using System;

namespace Account.Grains.ReadModelWriter
{
    public class AccountModel
    {
        public long Version { get; set; }
        public long Id { get; set; }
        public decimal Balance { get; set; }
        public decimal PendingCredit { get; set; }
        public decimal PendingDebit { get; set; }
        public DateTime Modified { get; set; }
    }
}