using System;
using EventSourcing;

namespace Grains.Account
{
    public abstract class AccountEvent: Event
    {
        public int AccountId { get; set; }
    }
}