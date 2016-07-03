using System;
using System.Linq;
using TShockAPI;
using TShockAPI.DB;
using Terraria;
using TerrariaApi.Server;
using TClases.DB;
using System.Collections.Generic;

namespace TClases
{
    public class TClassDamage
    {
        StatsManager statsManager = new StatsManager();
         /// <summary>
        /// Formato para este diccionario:
        /// Key: NPC
        /// Value: Una lista de los jugadores que han hecho daño con el NPC
        /// </summary>
        private Dictionary<Terraria.NPC,List<PlayerDamage>> DamageDictionary = new Dictionary<NPC,List<PlayerDamage>>();
        /// <summary>
        /// objeto de sincronización para el acceso al diccionario.  Usted debe obtener  
        /// una exclusión mutua a través de este objeto para obtener acceso al miembro de diccionario.
        /// </summary>
        protected readonly object __dictionaryMutex = new object();
        /// <summary>
        /// Objeto de sincronización para el daño NPC, obligando a NPC daños a serializarse
        /// </summary>
        protected static readonly object __NPCDamageMutex = new object();

        public void Game_update(EventArgs args)
        {
            foreach (NPC npc in Main.npc)
            {
                if (npc == null || npc.townNPC == true || npc.lifeMax == 0)
                {
                    continue;
                }
                if (npc.active == false)
                {
                    GiveRewardsForNPC(npc);
                }
            }
        }

        protected void GiveRewardsForNPC(NPC npc)
        {
            List<PlayerDamage> playerDamageList = null;
            TSPlayer player;
            int exp = 0;
            lock (__dictionaryMutex)
            {
                if (DamageDictionary.ContainsKey(npc))
                {
                    playerDamageList = DamageDictionary[npc];

                    if (DamageDictionary.Remove(npc) == false)
                    {
                        TShock.Log.ConsoleError("Tclass Exp: Quite de NPC después de fallado el premio.  Se trata de un error interno.");
                    }
                }
            }
            if (playerDamageList == null)
            {
                return;
            }
            foreach (PlayerDamage damage in playerDamageList)
            {
                if (damage.player == null || (player = TShockAPI.TShock.Players.FirstOrDefault(i => i != null && i.Index == damage.player.whoAmI)) == null)
                {
                    continue;
                }

                exp = Convert.ToInt32(Math.Round(Convert.ToDouble(TClasesPlugin.Config.DmamagePoint) * damage.damage)); 

                TClassCharacterInfo info = TClasesPlugin.statsmanager.GetUserByName(damage.player.name);
                Color expc = new Color(0, 255, 255);
                if (string.Compare(info.Clase, "Novato", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    int lvlup = info.Level * (896 + (896 * 43 / 100));

                    info.Exp += exp;
                    using (var reader = TShock.DB.QueryReader("UPDATE tclass set Exp=@0 WHERE UserName=@1;", info.Exp, info.UserName))
                    { NetMessage.SendData((int)PacketTypes.CreateCombatText, -1, -1, "+" + exp + " EXP", (int)expc.PackedValue, TShock.Players[damage.player.whoAmI].X, TShock.Players[damage.player.whoAmI].Y, 0, 0, 0, 0); }
                    if (info.Exp >= lvlup)
                    {
                        info.Level++;
                        info.Exp = 0;
                        using (var reader = TShock.DB.QueryReader("UPDATE tclass set Level = @0, Exp= @1 WHERE UserName=@2;", info.Level, info.Exp, info.UserName))
                        {
                            TShock.Players[damage.player.whoAmI].SendInfoMessage("Congratulations +LevelUp+ "+info.Clase+" LVL "+info.Level); 
                            
                        }
                    }
                    else
                    {
                        if (StatsManager.BlockNPCs.ContainsKey(e.Npc.netID))
                        {
                            if (StatsManager.BlockNPCs[e.Npc.netID])
                            {
                                return;
                            }
                        }
                    }
                    //Clases aca
                }

                
            }
        }

        #region iniciarNpcStrikeEventArgs
        public void iniciar(NpcStrikeEventArgs e)
        {
            if (e.Handled)
            {
                return;
            }
            if (e.Player == null || e.Npc.active == false || e.Npc.life <= 0)
            {
                return;
            }
            if (!(statsManager.loadDBInfo(e.Player.name)))
            {
                return;
            }
            AddNPCDamage(e.Npc, e.Player, e.Damage, e.Critical);
        }
        #endregion

        protected void AddNPCDamage(NPC NPC, Player Player, int Damage, bool crit = false)
        {
            List<PlayerDamage> damagelist = null;
            PlayerDamage playerdamage = null;
            double dmg;
            lock (__dictionaryMutex)
            {
                if (DamageDictionary.ContainsKey(NPC))
                {
                    damagelist = DamageDictionary[NPC];
                }
                else
                {
                    damagelist = new List<PlayerDamage>(1);
                    DamageDictionary.Add(NPC, damagelist);
                }
            }
            lock (__NPCDamageMutex)
            {
                if ((playerdamage = damagelist.FirstOrDefault(i => i.player == Player)) == null)
                {
                    playerdamage = new PlayerDamage() { player = Player };
                    damagelist.Add(playerdamage);
                }
                if ((dmg = (crit ? 2 : 1) * Main.CalculateDamage(Damage, NPC.ichor ? NPC.defense - 20 : NPC.defense)) > NPC.life)
                {
                    dmg = NPC.life;
                }
                playerdamage.damage += dmg;
                if (playerdamage.damage > NPC.lifeMax)
                {
                    playerdamage.damage -= playerdamage.damage % NPC.lifeMax;
                }
            }
        }

    }
}
public class PlayerDamage
{
    public Terraria.Player player;
    public double damage;
}