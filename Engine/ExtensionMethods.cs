using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine
{
    public static class ExtensionMethods
    {
        #region Player

        public static Player FullHeal(this Player player)
        {
            player.CurrentHitPoints = player.MaximumHitPoints;
            return player;
        }

        #endregion
    }
}
