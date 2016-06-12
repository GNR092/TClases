using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using TerrariaApi;
using TerrariaApi.Server;
using TShockAPI;
using TClases;
using TClases.DB;
using System.IO;

namespace TClases
{
    [ApiVersion(1,23)]
    public class TClasesPlugin : TerrariaPlugin
    {
        public override string Name { get { return "TClases"; } }
        public override string Author { get { return "GNR092"; } }
        public override string Description { get { return ""; } }
        public override Version Version { get { return new Version("1.0.0"); } }
        public TClasesPlugin(Main game) : base(game) {}

        private StatsManager statsmanager = new StatsManager();
        private TClassDamage ClassDamage = new TClassDamage();
        private static Config Config;

        public override void Initialize()
        {
            statsmanager.DBConnect();
            string path = Path.Combine(TShock.SavePath, "TClassConfig.json");
            Config = Config.Read(path);
            if (!File.Exists(path))
            {
                Config.Write(path);
            }

            ServerApi.Hooks.ServerJoin.Register(this, statsmanager.Onjoin);
            ServerApi.Hooks.NpcStrike.Register(this, ClassDamage.iniciar);

            StatsManager.BlockNPCs = Config.BlockNPCs;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {

            }
            base.Dispose(disposing);
        }       
    }
}
