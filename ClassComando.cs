using System;
using System.Linq;
using Microsoft.Xna.Framework;
using TClases;
using TShockAPI;
using TShockAPI.DB;

namespace TClases
{
    public class ClassComando
    {
        #region Level
        public static void LevelClass(CommandArgs args)
        {
            //fix
            TClassCharacterInfo info1 = TClasesPlugin.statsmanager.GetUserByName(args.Player.Name);
            if (args.Player.IsLoggedIn == false)
            {
                args.Player.SendMessage("Porfavor inicie sesion", Color.Silver);
                return;
            }
            if (!(TClasesPlugin.statsmanager.loadDBInfo(args.Player.Name)))
            {
                args.Player.SendMessage("No se encuentra en la base de datos porfavor reinicie el juego nuevamente.", Color.Silver);
                return;
            }
            TClassCharacterInfo info = TClasesPlugin.statsmanager.GetUserByName(args.Player.Name);
            int lvlup = info.Level * (896 + (896 * 43 / 100));
            int percent = (info.Exp * 100) / lvlup;
            args.Player.SendMessage("Nivel: " + info.Clase + " " + info.Level + " [" + info.Exp + "/" + lvlup + "] " + percent + "%",Color.Silver);
        }
        #endregion

        #region GuerreroClass
        public static void GuerreroClass(CommandArgs args)
        {
            TClassCharacterInfo info = TClasesPlugin.statsmanager.GetUserByName(args.Player.Name);
            var user = new User();
            user.Name = args.Player.Name;
            TShock.Users.SetUserGroup(user, Permisos.ClassGroup[0]);
            info.Level++;
            info.Clase = "Guerrero";
            using (var reader = TShock.DB.QueryReader("UPDATE tclass set Exp=@0,Clase=@1 WHERE UserName=@2;", info.Exp, info.Clase, info.UserName)) { };
            args.Player.SendSuccessMessage("Felicidades ustes ahora es: "+info.Clase);
        }
        #endregion

        #region ArqueroClass
        public static void ArqueroClass(CommandArgs args)
        {
            TClassCharacterInfo info = TClasesPlugin.statsmanager.GetUserByName(args.Player.Name);
            var user = new User();
            user.Name = args.Player.Name;
            TShock.Users.SetUserGroup(user, Permisos.ClassGroup[1]);
            info.Level++;
            info.Clase = "Arquero";
            using (var reader = TShock.DB.QueryReader("UPDATE tclass set Exp=@0,Clase=@1 WHERE UserName=@2;", info.Exp, info.Clase, info.UserName)) { };
            args.Player.SendSuccessMessage("Felicidades ustes ahora es: " + info.Clase);
        }
        #endregion

        #region MagoClass
        public static void MagoClass(CommandArgs args)
        {
            TClassCharacterInfo info = TClasesPlugin.statsmanager.GetUserByName(args.Player.Name);
            var user = new User();
            user.Name = args.Player.Name;
            TShock.Users.SetUserGroup(user, Permisos.ClassGroup[2]);
            info.Level++;
            info.Level++;
            info.Clase = "Mago";
            using (var reader = TShock.DB.QueryReader("UPDATE tclass set Exp=@0,Clase=@1 WHERE UserName=@2;", info.Exp, info.Clase, info.UserName)) { };
            args.Player.SendSuccessMessage("Felicidades ustes ahora es: " + info.Clase);
        }
        #endregion

        public static void TReload(CommandArgs args)
        {
            TClasesPlugin.Config = Config.Read(TClasesPlugin.path);
            args.Player.SendSuccessMessage("TClassConfig Cargado");
            return;
        }
    }
}
