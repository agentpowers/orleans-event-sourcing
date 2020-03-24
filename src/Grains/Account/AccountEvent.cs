using System;
using EventSourcing;

namespace Grains.Account
{
    public interface IAccountEvent: IEvent
    {
        int AccountId { get; set; }
    }
}