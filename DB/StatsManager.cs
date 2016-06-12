using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using Terraria;
using TShockAPI;
using TShockAPI.DB;
using System.Data;
using System.IO;
using Mono.Data.Sqlite;
using TShockAPI;
using TerrariaApi.Server;

namespace TClases.DB
{
    public class StatsManager
    {
        private IDbConnection db;
        public static Dictionary<int, bool> BlockNPCs = new Dictionary<int, bool>();

        public void DBConnect()
        {
            switch (TShock.Config.StorageType.ToLower())
            {
                case "mysql":
                    string[] dbHost = TShock.Config.MySqlHost.Split(':');
                    db = new MySqlConnection()
                    {
                        ConnectionString = string.Format("Server={0}; Port={1}; Database={2}; Uid={3}; Pwd={4};",
                            dbHost[0],
                            dbHost.Length == 1 ? "3306" : dbHost[1],
                            TShock.Config.MySqlDbName,
                            TShock.Config.MySqlUsername,
                            TShock.Config.MySqlPassword)

                    };
                    break;

                case "sqlite":
                    string sql = Path.Combine(TShock.SavePath, "Permabuffs.sqlite");
                    db = new SqliteConnection(string.Format("uri=file://{0},Version=3", sql));
                    break;
            }

            var sqlCreator = new SqlTableCreator(db, db.GetSqlType() == SqlType.Sqlite ? (IQueryBuilder)new SqliteQueryCreator() : new MysqlQueryCreator());

            sqlCreator.EnsureTableStructure(new SqlTable("TClass",
                new SqlColumn("UserName", MySqlDbType.VarChar) { Length = 32, Primary = true, Unique = true },
                new SqlColumn("Clase",MySqlDbType.VarChar){Length = 32},
                new SqlColumn("Level", MySqlDbType.Int32),
                new SqlColumn("Str", MySqlDbType.Int32),
                new SqlColumn("Vit", MySqlDbType.Int32),
                new SqlColumn("_Int", MySqlDbType.Int32),
                new SqlColumn("Agi", MySqlDbType.Int32),
                new SqlColumn("Lck", MySqlDbType.Int32)));
        }

        public void createDBInfo(string UserName)
        {
            db.Query("INSERT INTO TClass (UserName, Clase, Level, Str, Vit, _Int, Agi, Lck) VALUES (@0,@1,@2,@3,@4,@5,@6,@7);", UserName, "Novato", 1, 0, 0, 0, 0, 0);
        }

        public bool loadDBInfo(string UserName)
        {
            using (QueryResult result = db.QueryReader("SELECT * FROM TClass WHERE UserName=@0;", UserName))
            {
                if (result.Read())
                {
                    return true;
                }
                else
                    return false;
            }
        }

        public string GetTClass(TSPlayer UserName)
        {
            using (QueryResult result = db.QueryReader("SELECT * FROM TClass WHERE UserName=@0;", UserName))
            {
                if (result.Read())
                {
                    return result.Get<string>("Clase");
                }
                else
                {
                    return null;
                }
            }
        }

        public void Onjoin(JoinEventArgs e)
        {
            var player = TShock.Players[e.Who].Name;

            if (!loadDBInfo(player))
                createDBInfo(player);
        }
    }
}
