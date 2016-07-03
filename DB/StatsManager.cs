using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using Mono.Data.Sqlite;
using MySql.Data.MySqlClient;
using TShockAPI;
using TShockAPI.DB;
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
                new SqlColumn("Exp", MySqlDbType.Int32),
                new SqlColumn("Str", MySqlDbType.Int32),
                new SqlColumn("Vit", MySqlDbType.Int32),
                new SqlColumn("_Int", MySqlDbType.Int32),
                new SqlColumn("Agi", MySqlDbType.Int32),
                new SqlColumn("Lck", MySqlDbType.Int32)));
        }

        public void Onjoin(JoinEventArgs e)
        {
            var player = TShock.Players[e.Who].Name;

            if (!loadDBInfo(player))
                createDBInfo(player);
        }

        public void createDBInfo(string UserName)
        {
            db.Query("INSERT INTO TClass (UserName, Clase, Level, Exp, Str, Vit, _Int, Agi, Lck) VALUES (@0,@1,@2,@3,@4,@5,@6,@7,@8);", UserName, "Novato", 1, 0, 0, 0, 0, 0, 0);
        }

        public bool loadDBInfo(string UserName)
        {
            try
            {
                using (var reader = TShock.DB.QueryReader("SELECT * FROM tclass WHERE UserName=@0;", UserName))
                {
                    if (reader.Read())
                    {
                        return true;
                    }
                    else
                        return false;
                }
            }
            catch
            {
                return false;
            }
        }
    
        public TClassCharacterInfo GetUserByName(string username)
        {
            try
            {
                return GetInfo(new TClassCharacterInfo { UserName = username });
            }
            catch (MySqlException)
            {
            }
            return null;
        }

        public TClassCharacterInfo GetInfo(TClassCharacterInfo usename)
        {
            object name = usename.UserName;
                using (var reader = TShock.DB.QueryReader("SELECT * FROM tclass WHERE UserName=@0;", name))
                {
                    if (reader.Read())
                    {
                        usename = LoadInfoCharacter(usename, reader);
                        return usename;
                    }
                }
            return null;
        }

        private TClassCharacterInfo LoadInfoCharacter(TClassCharacterInfo tclass, QueryResult r)
        {
            tclass.UserName = r.Get<string>("UserName");
            tclass.Clase = r.Get<string>("Clase");
            tclass.Level = r.Get<Int32>("Level");
            tclass.Exp = r.Get<Int32>("Exp");
            tclass.Str = r.Get<Int32>("Str");
            tclass.Vit = r.Get<Int32>("Vit");
            tclass._Int = r.Get<Int32>("_Int");
            tclass.Agi = r.Get<Int32>("Agi");
            tclass.Lck = r.Get<Int32>("Lck");
            return tclass;
        }
    }
}

public class TClassCharacterInfo
{
    public string UserName { get; set; }
    public string Clase { get; set; }
    public int Level { get; set; }
    public int Exp { get; set; }
    public int Str { get; set; }
    public int Vit { get; set; }
    public int _Int { get; set; }
    public int Agi { get; set; }
    public int Lck { get; set; }

    public TClassCharacterInfo()
    {
        UserName = "";
        Clase = "";
        Level = 0;
        Exp = 0;
        Str = 0;
        Vit = 0;
        _Int = 0;
        Agi = 0;
        Lck = 0;
    }

    public TClassCharacterInfo(string username, string clase, int level, int exp, int str, int vit, int _int, int agi, int lck)
    {
        UserName = username;
        Clase = clase;
        Level = level;
        Exp = exp;
        Str = str;
        Vit = vit;
        _Int = _int;
        Agi = agi;
        Lck = lck;
    }
}
