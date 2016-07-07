using System;
using System.IO;
using System.Linq;
using System.Reflection;
using TShockAPI;
using Terraria;
using TerrariaApi.Server;
using TClases;
using TClases.DB;

namespace TClases
{
    [ApiVersion(1,23)]
    public class TClasesPlugin : TerrariaPlugin
    {
        public override string Name {
            get {
                string s = "TClases Update " + this.Version.Build;
                return s; } }
        public override string Author { get { return "GNR092"; } }
        public override string Description { get { return "Clases automatizadas para Tshock by GNR092"; } }
        public override Version Version { get { return Assembly.GetExecutingAssembly().GetName().Version; } }
        public TClasesPlugin(Main game) : base(game) {}

        public static StatsManager statsmanager = new StatsManager();
        private TClassDamage ClassDamage = new TClassDamage();
        public static TClassCharacterInfo tClassCharacterInfo = new TClassCharacterInfo();
        public static Config Config;

        public override void Initialize()
        {
            intro();
            statsmanager.DBConnect();
            string path = Path.Combine(TShock.SavePath, "TClassConfig.json");
            Config = Config.Read(path);
            if (!File.Exists(path))
            {
                Config.Write(path);
            }

            ServerApi.Hooks.ServerJoin.Register(this, statsmanager.Onjoin);
            ServerApi.Hooks.NpcStrike.Register(this, ClassDamage.iniciar);
            ServerApi.Hooks.GameUpdate.Register(this, ClassDamage.Game_update);
            StatsManager.BlockNPCs = Config.BlockNPCs;

            Commands.ChatCommands.Add(new Command(Permisos.LevelP, ClassComando.LevelClass, "level") { AllowServer = false });
            Commands.ChatCommands.Add(new Command(Permisos.GuerreroP, ClassComando.GuerreroClass, "guerrero") { AllowServer = false, HelpText = "Cambia a clase Guerrero" });
            Commands.ChatCommands.Add(new Command(Permisos.ArqueroP, ClassComando.ArqueroClass, "arquero") { AllowServer = false, HelpText = "Cambia a clase Arquero" });
            Commands.ChatCommands.Add(new Command(Permisos.MagoP, ClassComando.MagoClass, "mago") { AllowServer = false, HelpText = "Cambia a clase Mago" });
        }
        protected void intro()
        {
            
            Console.ForegroundColor = ConsoleColor.Red;
            
            Console.Write(" TClass Update ");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write(this.Version.Build);

            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write(" Copyright (C) GNR092 - ");

            Console.ForegroundColor = ConsoleColor.Blue;
            Console.Write("https://github.com/GNR092/TClases");

            Console.WriteLine("\r\n");
            Console.WriteLine();
            Console.ResetColor();
        }
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                ServerApi.Hooks.GameUpdate.Deregister(this, ClassDamage.Game_update);
            }
            base.Dispose(disposing);
        }       
    }
}
