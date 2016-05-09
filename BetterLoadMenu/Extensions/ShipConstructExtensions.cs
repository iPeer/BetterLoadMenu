using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BetterLoadMenu.Extensions
{
    public static class ShipConstructExtensions
    {

        public static float GetTotalShipCost(this ShipConstruct c)
        {
            float _, __;
            return c.GetShipCosts(out _, out __);
        }

    }
}
