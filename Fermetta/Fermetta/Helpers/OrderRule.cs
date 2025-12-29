using Fermetta.Models;

namespace Fermetta.Helpers
{
    public static class OrderRule
    {
        public static bool CanTransition(OrderStatus current, OrderStatus next)
        {
            if (current == OrderStatus.Shipped || current == OrderStatus.Cancelled)
                return false;

            return current switch
            {
                OrderStatus.New => next == OrderStatus.InProcess || next == OrderStatus.Cancelled,
                OrderStatus.InProcess => next == OrderStatus.Shipped || next == OrderStatus.Cancelled,
                _ => false
            };
        }
    }
}
