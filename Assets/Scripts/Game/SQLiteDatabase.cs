#nullable enable
using System.Collections.Generic;
using Tetr4lab.UnityEngine.SQLite;
using Unity.Logging;

namespace Game
{
    public class SQLiteDatabase
    {
        private SQLite<SQLiteTable<SQLiteRow>, SQLiteRow> sqlDB;

        public SQLiteDatabase()
        {
            Log.Debug("Database init");
            var initQuery = @"
                create table if not exists saves (save_id integer, text string, turnWaitTime real, bgm integer not null default 0, primary key(save_id));
                create table if not exists latest_tracker(save_id integer, turn integer, primary key(save_id));
                create table if not exists maps (save_id integer, map_id string, text string, primary key(save_id, map_id));
                create table if not exists global_statistics (text string);
                create table if not exists statistics (save_id integer, text string, primary key(save_id));
                create table if not exists global_settings (name string, value integer, primary key(name));
                create table if not exists settings (save_id integer, name string, value integer, primary key(save_id, name));";

            sqlDB = new SQLite<SQLiteTable<SQLiteRow>, SQLiteRow>("save.db", initQuery, path: "");
            Log.Debug("Database init done");
        }

        public void Save(int save_id, string saveData, float turnWaitTime, int bgm)
        {
            sqlDB.ExecuteNonQuery(
                "insert or replace into saves (save_id, text, turnWaitTime, bgm) values (:save_id, :text, :turnWaitTime, :bgm)",
                new SQLiteRow
                {
                    { "save_id", save_id },
                    { "text", saveData },
                    { "turnWaitTime", turnWaitTime },
                    { "bgm", bgm }
                });
        }
        public void SaveTurn(int save_id, int turn)
        {
            sqlDB.ExecuteNonQuery(
                "insert or replace into latest_tracker values(:save_id, :turn)",
                new SQLiteRow
                {
                    { "save_id", save_id },
                    { "turn", turn }
                });
        }
        public void SaveMap(int save_id, string map_id, string mapData)
        {
            sqlDB.ExecuteNonQuery(
                "insert or replace into maps values(:save_id, :map_id, :text)",
                new SQLiteRow
                {
                    { "save_id", save_id },
                    { "map_id", map_id },
                    { "text", mapData }
                });
        }
        public void SaveGlobalStatistics(string globalStatisticsData)
        {
            // ensure only single latest row exists
            sqlDB.ExecuteNonQuery("delete from global_statistics");
            sqlDB.ExecuteNonQuery(
                "insert into global_statistics values(:text)",
                new SQLiteRow
                {
                    { "text", globalStatisticsData }
                });
        }
        public void SaveStatistics(int save_id, string statisticsData)
        {
            sqlDB.ExecuteNonQuery(
                "insert or replace into statistics values(:save_id, :text)",
                new SQLiteRow
                {
                    { "save_id", save_id },
                    { "text", statisticsData }
                });
        }
        public void SaveGlobalSettings(Dictionary<string, int> globalSettings)
        {
            foreach (var setting in globalSettings)
            {
                sqlDB.ExecuteNonQuery(
                    "insert or replace into global_settings values(:name, :value)",
                    new SQLiteRow
                    {
                        { "name", setting.Key },
                        { "value", setting.Value }
                    });
            }
        }
        public void SaveSettings(int save_id, Dictionary<string, int> settings)
        {
            foreach (var setting in settings)
            {
                sqlDB.ExecuteNonQuery(
                    "insert or replace into settings values(:save_id, :name, :value)",
                    new SQLiteRow
                    {
                        { "save_id", save_id },
                        { "name", setting.Key },
                        { "value", setting.Value }
                    });
            }
        }
        public bool ExistGlobal()
        {
            var dataTable = sqlDB.ExecuteQuery("select * from global_statistics");
            return dataTable.Rows.Count > 0;
        }
        public bool ExistSave(int save_id)
        {
            var dataTable = sqlDB.ExecuteQuery(
                "select * from saves where save_id = :save_id",
                new SQLiteRow { { "save_id", save_id } });
            return dataTable.Rows.Count > 0;
        }
        public (string world, float turnWaitTime, int bgm) Load(int save_id)
        {
            var dataTable = sqlDB.ExecuteQuery(
                "select * from saves where save_id = :save_id",
                new SQLiteRow
                {
                    { "save_id", save_id }
                });
            string world = (string)dataTable.Rows[0]["text"];
            float turnWaitTime = (float)(double)dataTable.Rows[0]["turnWaitTime"];
            int bgm = (int)dataTable.Rows[0]["bgm"];
            return (world, turnWaitTime, bgm);
        }
        public int LoadLatestTurn(int save_id)
        {
            var dataTable = sqlDB.ExecuteQuery(
                "select * from latest_tracker where save_id = :save_id",
                new SQLiteRow { { "save_id", save_id } });
            return (int)dataTable.Rows[0]["turn"];
        }
        public string LoadMap(int save_id, string map_id)
        {
            var dataTable = sqlDB.ExecuteQuery(
                "select * from maps where save_id = :save_id and map_id = :map_id",
                new SQLiteRow
                {
                    { "save_id", save_id },
                    { "map_id", map_id }
                });
            return dataTable.Rows[0]["text"] as string;
        }
        public string? LoadGlobalStatistics()
        {
            var dataTable = sqlDB.ExecuteQuery("select * from global_statistics");
            if (dataTable.Rows.Count == 0)
                return null;
            return dataTable.Rows[0]["text"] as string;
        }
        public string? LoadStatistics(int save_id)
        {
            var dataTable = sqlDB.ExecuteQuery(
                "select * from statistics where save_id = :save_id",
                new SQLiteRow
                {
                    { "save_id", save_id }
                });
            return dataTable.Rows[0]["text"] as string;
        }
        public Dictionary<string, int> LoadGlobalSettings()
        {
            var dataTable = sqlDB.ExecuteQuery("select * from global_settings");
            var settings = new Dictionary<string, int>();
            foreach (var row in dataTable.Rows)
            {
                if (row["name"] is string name && row["value"] is int value)
                {
                    settings[name] = value;
                }
            }
            return settings;
        }
        public Dictionary<string, int> LoadSettings()
        {
            var dataTable = sqlDB.ExecuteQuery("select * from settings");
            var settings = new Dictionary<string, int>();
            foreach (var row in dataTable.Rows)
            {
                if (row["name"] is string name && row["value"] is int value)
                {
                    settings[name] = value;
                }
            }
            return settings;
        }
        public void ClearSave()
        {
            sqlDB.ExecuteNonQuery("delete from saves");
            sqlDB.ExecuteNonQuery("delete from maps");
        }
    }
}