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
        #region Update
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
        #endregion

        #region Recompensas
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

                int maxheal = 0;
                using (var reader = TShock.DB.QueryReader("SELECT MaxHealth, MaxMana FROM tsCharacter where account =@0", player.User.ID))
                {
                    if (reader.Read())
                    {
                        maxheal = reader.Get<Int32>("MaxHealth");
                    }
                }

                #region Clase Novato
                if (info.Level == 10 && info.Clase == "Novato")
                {
                    string grupo = TShock.Players[damage.player.whoAmI].Group.ToString();
                    if (grupo != "Novato10" && grupo != Permisos.ClassGroup[0] && grupo != Permisos.ClassGroup[1] && grupo != Permisos.ClassGroup[2])
                    {
                        var user = new User();
                        user.Name = damage.player.name;
                        TShock.Users.SetUserGroup(user, Permisos.ClassGroup[4]);
                    }
                    TShock.Players[damage.player.whoAmI].SendMessage("Nivel 10 alcanzado porfavor escoja una de las siguientes clases:",Color.White);
                    TShock.Players[damage.player.whoAmI].SendMessage("/Guerrero",Color.White);
                    TShock.Players[damage.player.whoAmI].SendMessage("/Arquero", Color.White);
                    TShock.Players[damage.player.whoAmI].SendMessage("/Mago", Color.White);
                    return;
                }
                if (string.Compare(info.Clase, "Novato", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    int lvlup = info.Level * (896 + (896 * 43 / 100));

                    info.Exp += exp;

                    using (var reader = TShock.DB.QueryReader("UPDATE tclass set Exp=@0 WHERE UserName=@1;", info.Exp, info.UserName))
                    { NetMessage.SendData((int)PacketTypes.CreateCombatText, -1, -1, "+" + exp + " EXP", (int)expc.PackedValue, TShock.Players[damage.player.whoAmI].X, TShock.Players[damage.player.whoAmI].Y, 0, 0, 0, 0); }
                   
                    if (info.Exp >= lvlup)
                    {
                        if ((info.Level % 2) == 0)
                        {
                            info.Level++;
                            info.Str++;
                            info.Vit++;
                            maxheal += 3;
                            info._Int++;
                            info.Agi++;
                            info.Lck++;
                            info.Exp = 0;
                            using (var reader = TShock.DB.QueryReader("UPDATE tclass set Level = @0, Exp= @1, Str=@2, Vit=@3, _Int=@4, Agi=@5, Lck=@6  WHERE UserName=@7;", info.Level, info.Exp, info.Str, info.Vit, info._Int, info.Agi, info.Lck, info.UserName))
                            {
                                TShock.Players[damage.player.whoAmI].SendInfoMessage("Congratulations +LevelUp+ " + info.Clase + " LVL " + info.Level);
                                TSPlayer.Server.SendInfoMessage("Congratulations " + damage.player.name + " +LevelUp+ " + info.Clase + " LVL " + info.Level);
                            }
                            player.TPlayer.statLifeMax = maxheal;
                            player.SendData(PacketTypes.PlayerHp, player.Name, player.Index);
                            player.SendData(PacketTypes.PlayerUpdate, player.Name, player.Index);
                            using (var reader = TShock.DB.QueryReader("UPDATE tsCharacter set MaxHealth = @0 where account =@1", maxheal, player.User.ID)) { }
                        }
                        else
                        {
                            info.Level++;
                            info.Exp = 0;
                            using (var reader = TShock.DB.QueryReader("UPDATE tclass set Level = @0, Exp= @1, Str=@2, Vit=@3, _Int=@4, Agi=@5, Lck=@6  WHERE UserName=@7;", info.Level, info.Exp, info.Str, info.Vit, info._Int, info.Agi, info.Lck, info.UserName))
                            {
                                TShock.Players[damage.player.whoAmI].SendInfoMessage("Congratulations +LevelUp+ " + info.Clase + " LVL " + info.Level);
                                TSPlayer.Server.SendInfoMessage("Congratulations " + damage.player.name + " +LevelUp+ " + info.Clase + " LVL " + info.Level);
                            }
                        }
                    }
                #endregion

                }
                else
                {
                    #region Bloqueo de NPC(FIX)
                    if (StatsManager.BlockNPCs.ContainsKey(npc.netID))
                    {   //Fix <=======>
                        //if (StatsManager.BlockNPCs[npc.netID])
                        //{
                        //    return;
                        //}
                        //Fix <=======>
                        if (npc.SpawnedFromStatue)
                        {
                            return;
                        }
                    }
                    #endregion

                    #region ExpClases
                    if (info.Clase == "Guerrero")
                    {
                        int lvlup = info.Level * (896 + (896 * 43 / 100));

                        info.Exp += exp;
                        using (var reader = TShock.DB.QueryReader("UPDATE tclass set Exp=@0 WHERE UserName=@1;", info.Exp, info.UserName))
                        { NetMessage.SendData((int)PacketTypes.CreateCombatText, -1, -1, "+" + exp + " EXP", (int)expc.PackedValue, TShock.Players[damage.player.whoAmI].X, TShock.Players[damage.player.whoAmI].Y, 0, 0, 0, 0); }
                        if (info.Exp >= lvlup)
                        {
                            info.Level++;
                            info.Str += 2;
                            info.Vit += 2;
                            maxheal += 6;
                            info._Int++;
                            info.Agi++;
                            info.Lck++;
                            info.Exp = 0;
                            using (var reader = TShock.DB.QueryReader("UPDATE tclass set Level = @0, Exp= @1, Str=@2, Vit=@3, _Int=@4, Agi=@5, Lck=@6  WHERE UserName=@7;", info.Level, info.Exp, info.Str, info.Vit, info._Int, info.Agi, info.Lck, info.UserName))
                            {
                                TShock.Players[damage.player.whoAmI].SendInfoMessage("Congratulations +LevelUp+ " + info.Clase + " LVL " + info.Level);
                                TSPlayer.Server.SendInfoMessage("Congratulations " + damage.player.name + " +LevelUp+ " + info.Clase + " LVL " + info.Level);
                            }
                            player.TPlayer.statLifeMax = maxheal;
                            player.SendData(PacketTypes.PlayerHp, player.Name, player.Index);
                            player.SendData(PacketTypes.PlayerUpdate, player.Name, player.Index);
                            using (var reader = TShock.DB.QueryReader("UPDATE tsCharacter set MaxHealth=@0 where account =@1", maxheal, player.User.ID)) { }
                        }
                        return;
                    }
                    if (info.Clase == "Arquero")
                    {
                        int lvlup = info.Level * (896 + (896 * 43 / 100));

                        info.Exp += exp;
                        using (var reader = TShock.DB.QueryReader("UPDATE tclass set Exp=@0 WHERE UserName=@1;", info.Exp, info.UserName))
                        { NetMessage.SendData((int)PacketTypes.CreateCombatText, -1, -1, "+" + exp + " EXP", (int)expc.PackedValue, TShock.Players[damage.player.whoAmI].X, TShock.Players[damage.player.whoAmI].Y, 0, 0, 0, 0); }
                        if (info.Exp >= lvlup)
                        {
                            info.Level++;
                            info.Str +=2 ;
                            info.Vit++;
                            maxheal += 3;
                            info._Int++;
                            info.Agi += 2;
                            info.Lck += 2;
                            info.Exp = 0;
                            using (var reader = TShock.DB.QueryReader("UPDATE tclass set Level = @0, Exp= @1, Str=@2, Vit=@3, _Int=@4, Agi=@5, Lck=@6  WHERE UserName=@7;", info.Level, info.Exp, info.Str, info.Vit, info._Int, info.Agi, info.Lck, info.UserName))
                            {
                                TShock.Players[damage.player.whoAmI].SendInfoMessage("Congratulations +LevelUp+ " + info.Clase + " LVL " + info.Level);
                                TSPlayer.Server.SendInfoMessage("Congratulations " + damage.player.name + " +LevelUp+ " + info.Clase + " LVL " + info.Level);
                            }
                            player.TPlayer.statLifeMax = maxheal;
                            player.SendData(PacketTypes.PlayerHp, player.Name, player.Index);
                            player.SendData(PacketTypes.PlayerUpdate, player.Name, player.Index);
                            using (var reader = TShock.DB.QueryReader("UPDATE tsCharacter set MaxHealth=@0 where account =@1", maxheal, player.User.ID)) { }
                        }
                        return;
                    }
                    if (info.Clase == "Mago")
                    {
                        int lvlup = info.Level * (896 + (896 * 43 / 100));

                        info.Exp += exp;
                        using (var reader = TShock.DB.QueryReader("UPDATE tclass set Exp=@0 WHERE UserName=@1;", info.Exp, info.UserName))
                        { NetMessage.SendData((int)PacketTypes.CreateCombatText, -1, -1, "+" + exp + " EXP", (int)expc.PackedValue, TShock.Players[damage.player.whoAmI].X, TShock.Players[damage.player.whoAmI].Y, 0, 0, 0, 0); }
                        if (info.Exp >= lvlup)
                        {
                            info.Level++;
                            info.Str += 2;
                            info.Vit++;
                            maxheal += 3;
                            info._Int += 2;
                            info.Agi++;
                            info.Lck++;
                            info.Exp = 0;
                            using (var reader = TShock.DB.QueryReader("UPDATE tclass set Level = @0, Exp= @1, Str=@2, Vit=@3, _Int=@4, Agi=@5, Lck=@6  WHERE UserName=@7;", info.Level, info.Exp, info.Str, info.Vit, info._Int, info.Agi, info.Lck, info.UserName))
                            {
                                TShock.Players[damage.player.whoAmI].SendInfoMessage("Congratulations +LevelUp+ " + info.Clase + " LVL " + info.Level);
                                TSPlayer.Server.SendInfoMessage("Congratulations " + damage.player.name + " +LevelUp+ " + info.Clase + " LVL " + info.Level);
                            }
                            player.TPlayer.statLifeMax = maxheal;
                            player.SendData(PacketTypes.PlayerHp, player.Name, player.Index);
                            player.SendData(PacketTypes.PlayerUpdate, player.Name, player.Index);
                            using (var reader = TShock.DB.QueryReader("UPDATE tsCharacter set MaxHealth=@0 where account =@1", maxheal, player.User.ID)) { }
                        }
                        return;
                    }
                    #endregion
                }
            }
        }
        #endregion

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

            Color melee = new Color(255, 120, 0);
            Color ranged = new Color(0, 255, 0);
            Color magic = new Color(0, 255, 255);
            TClassCharacterInfo info = TClasesPlugin.statsmanager.GetUserByName(e.Player.name);

            AddNPCDamage(e.Npc, e.Player, e.Damage, e.Critical);

            #region Cuerpo a cuerpo
            if (TShock.Players[e.Player.whoAmI].SelectedItem.melee)
            {
                if (e.Damage >= e.Npc.life)
                {
                    return;
                }
                int damage = (info.Str * 3);
                if (damage>0)
                {
                    NetMessage.SendData((int)PacketTypes.CreateCombatText, -1, -1, "+" + damage, (int)melee.PackedValue, e.Npc.position.X, e.Npc.position.Y, 0, 0, 0, 0);
                }
                e.Damage = e.Damage + damage;
                return;
            }
            #endregion

            #region Rango
            if (TShock.Players[e.Player.whoAmI].SelectedItem.ranged)
            {
                if (e.Damage >= e.Npc.life)
                {
                    return;
                }
                int damage = (info.Str * 3);
                if (damage > 0)
                {
                    NetMessage.SendData((int)PacketTypes.CreateCombatText, -1, -1, "+" + damage, (int)ranged.PackedValue, e.Npc.position.X, e.Npc.position.Y, 0, 0, 0, 0);
                }
                e.Damage = e.Damage + damage;
                return;
            }
            #endregion

            #region Magia
            if (TShock.Players[e.Player.whoAmI].SelectedItem.magic)
            {
                if (e.Damage >= e.Npc.life)
                {
                    return;
                }
                int damage = (info._Int * 3);
                if (damage > 0)
                {
                    NetMessage.SendData((int)PacketTypes.CreateCombatText, -1, -1, "+" + damage, (int)magic.PackedValue, e.Npc.position.X, e.Npc.position.Y, 0, 0, 0, 0);
                }
                e.Damage = e.Damage + damage;
                return;
            }
            #endregion
        }
        #endregion

        #region AddNPCDamage
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
        #endregion
    }
}
public class PlayerDamage
{
    public Player player;
    public double damage;
}