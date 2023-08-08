using EventSourcing;

namespace Account.Grains
{
    public class AccountAggregate : IAggregate<Account, IAccountEvent>
    {
        public Account State { get; set; }
        public void Apply(IAccountEvent @event)
        {
            switch (@event)
            {
                case Deposited deposited:
                    State.Amount += deposited.Amount;
                    break;
                case Withdrawn withdrawn:
                    State.Amount -= withdrawn.Amount;
                    break;
                case TransferCredited tranferCredited:
                    State.Amount -= tranferCredited.Amount;
                    break;
                case TransferDebited transferDebited:
                    State.Amount += transferDebited.Amount;
                    break;
                default:
                    break;
            }
        }
    }
}
