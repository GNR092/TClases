using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using TShockAPI;
using TerrariaApi.Server;
using TClases.DB;

namespace TClases
{
    public class TClassDamage
    {
       StatsManager statsManager = new StatsManager();

        public void iniciar(NpcStrikeEventArgs e)
        {
            if (e.Handled)
            {
                return;
            }
            //if (e.Player == null)
            //{
            //    return;
            //}
            if (!(statsManager.loadDBInfo(e.Player.name)))
            {
                return;
            }
            if (StatsManager.BlockNPCs.ContainsKey(e.Npc.netID))
            {
                if (StatsManager.BlockNPCs[e.Npc.netID])
                {
                    return;
                }
            }

            Color c = new Color(255, 120, 0);

            #region novato

            #endregion
        }
    }
}
