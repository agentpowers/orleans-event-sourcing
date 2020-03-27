using System;

namespace Grains.Account.ReadModelWriter
{
    public class AccountModel
    {
        public long Version { get; set; }
        public long Id { get; set; }
        public decimal Balance { get; set; }
        public DateTime Modified { get; set; }
    }
}