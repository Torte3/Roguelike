#nullable enable
using System;
using System.Collections.Generic;
using Tetr4lab.UnityEngine.SQLite;
using Unity.Logging;

namespace Game
{
    internal class SQLiteDatabase
    {
        private SQLite<SQLiteTable<SQLiteRow>, SQLiteRow> sqlDB;

        public SQLiteDatabase()
        {
            Log.Debug("Database init");
            var initQuery = @"
                create table if not exists saves (save_id integer, text string, turnWaitTime real, primary key(save_id));
                create table if not exists latest_tracker(save_id integer, turn integer, primary key(save_id));
                create table if not exists maps (save_id integer, map_id string, text string, primary key(save_id, map_id));
                create table if not exists global_statistics (text string);
                create table if not exists statistics (save_id integer, text string, primary key(save_id));
                create table if not exists global_settings (name string, value integer, primary key(name));
                create table if not exists settings (save_id integer, name string, value integer, primary key(save_id, name));";

            sqlDB = new SQLite<SQLiteTable<SQLiteRow>, SQLiteRow>("save.db", initQuery, path: "");

            // 接続をセッション中つなぎっぱなしにする。これで各操作ごとのファイル open/close を避け、
            // さらに複数の書き込みを1トランザクションに束ねられる（Transaction を参照）。
            sqlDB.Open();
            // WAL + synchronous=NORMAL でコミット時の fsync コストを下げる（毎ターン保存のヒッチ対策）。
            // journal_mode は結果行を返すため ExecuteQuery で実行する。
            sqlDB.ExecuteQuery("PRAGMA journal_mode=WAL;");
            sqlDB.ExecuteNonQuery("PRAGMA synchronous=NORMAL;");

            Log.Debug("Database init done");
        }

        // 複数の書き込みを1トランザクション（=コミット1回・fsync1回）にまとめる。
        // 接続は開いたままなので、内側の Save 群は open/close せずこのトランザクションに参加する。
        public void Transaction(string operation, Action body)
        {
            Guard(operation, () =>
            {
                sqlDB.ExecuteNonQuery("BEGIN;");
                try
                {
                    body();
                    sqlDB.ExecuteNonQuery("COMMIT;");
                }
                catch
                {
                    sqlDB.ExecuteNonQuery("ROLLBACK;");
                    throw;
                }
            });
        }

        // DB 操作を実行し、失敗時はどの操作かを添えてログしてから再throwする。
        // 通常運用では失敗しない想定だが、失敗が無言で消えてセーブデータの不整合に気づけない事態を防ぐ。
        private void Guard(string operation, Action action)
        {
            try
            {
                action();
            }
            catch (Exception e)
            {
                Log.Error($"[Save]{operation} に失敗しました: {e}");
                throw;
            }
        }

        private T Guard<T>(string operation, Func<T> func)
        {
            try
            {
                return func();
            }
            catch (Exception e)
            {
                Log.Error($"[Save]{operation} に失敗しました: {e}");
                throw;
            }
        }

        public void Save(int save_id, string saveData, float turnWaitTime)
        {
            Guard(nameof(Save), () =>
                sqlDB.ExecuteNonQuery(
                    "insert or replace into saves (save_id, text, turnWaitTime) values (:save_id, :text, :turnWaitTime)",
                    new SQLiteRow
                    {
                        { "save_id", save_id },
                        { "text", saveData },
                        { "turnWaitTime", turnWaitTime }
                    }));
        }
        public void SaveTurn(int save_id, int turn)
        {
            Guard(nameof(SaveTurn), () =>
                sqlDB.ExecuteNonQuery(
                    "insert or replace into latest_tracker values(:save_id, :turn)",
                    new SQLiteRow
                    {
                        { "save_id", save_id },
                        { "turn", turn }
                    }));
        }
        public void SaveMap(int save_id, string map_id, string mapData)
        {
            Guard(nameof(SaveMap), () =>
                sqlDB.ExecuteNonQuery(
                    "insert or replace into maps values(:save_id, :map_id, :text)",
                    new SQLiteRow
                    {
                        { "save_id", save_id },
                        { "map_id", map_id },
                        { "text", mapData }
                    }));
        }
        public void SaveGlobalStatistics(string globalStatisticsData)
        {
            Guard(nameof(SaveGlobalStatistics), () =>
            {
                // 常に最新の1行だけが存在するようにする（古い行を消してから挿入する）
                sqlDB.ExecuteNonQuery("delete from global_statistics");
                sqlDB.ExecuteNonQuery(
                    "insert into global_statistics values(:text)",
                    new SQLiteRow
                    {
                        { "text", globalStatisticsData }
                    });
            });
        }
        public void SaveStatistics(int save_id, string statisticsData)
        {
            Guard(nameof(SaveStatistics), () =>
                sqlDB.ExecuteNonQuery(
                    "insert or replace into statistics values(:save_id, :text)",
                    new SQLiteRow
                    {
                        { "save_id", save_id },
                        { "text", statisticsData }
                    }));
        }
        public void SaveGlobalSettings(Dictionary<string, int> globalSettings)
        {
            Guard(nameof(SaveGlobalSettings), () =>
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
            });
        }
        public void SaveSettings(int save_id, Dictionary<string, int> settings)
        {
            Guard(nameof(SaveSettings), () =>
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
            });
        }
        public bool ExistGlobal()
        {
            return Guard(nameof(ExistGlobal), () =>
            {
                var dataTable = sqlDB.ExecuteQuery("select * from global_statistics");
                return dataTable.Rows.Count > 0;
            });
        }
        public bool ExistSave(int save_id)
        {
            return Guard(nameof(ExistSave), () =>
            {
                var dataTable = sqlDB.ExecuteQuery(
                    "select * from saves where save_id = :save_id",
                    new SQLiteRow { { "save_id", save_id } });
                return dataTable.Rows.Count > 0;
            });
        }
        public (string world, float turnWaitTime) Load(int save_id)
        {
            return Guard(nameof(Load), () =>
            {
                var dataTable = sqlDB.ExecuteQuery(
                    "select * from saves where save_id = :save_id",
                    new SQLiteRow
                    {
                        { "save_id", save_id }
                    });
                string world = (string)dataTable.Rows[0]["text"];
                float turnWaitTime = (float)(double)dataTable.Rows[0]["turnWaitTime"];
                return (world, turnWaitTime);
            });
        }
        public int LoadLatestTurn(int save_id)
        {
            return Guard(nameof(LoadLatestTurn), () =>
            {
                var dataTable = sqlDB.ExecuteQuery(
                    "select * from latest_tracker where save_id = :save_id",
                    new SQLiteRow { { "save_id", save_id } });
                return (int)dataTable.Rows[0]["turn"];
            });
        }
        public string LoadMap(int save_id, string map_id)
        {
            return Guard(nameof(LoadMap), () =>
            {
                var dataTable = sqlDB.ExecuteQuery(
                    "select * from maps where save_id = :save_id and map_id = :map_id",
                    new SQLiteRow
                    {
                        { "save_id", save_id },
                        { "map_id", map_id }
                    });
                return dataTable.Rows[0]["text"] as string;
            });
        }
        public string? LoadGlobalStatistics()
        {
            return Guard(nameof(LoadGlobalStatistics), () =>
            {
                var dataTable = sqlDB.ExecuteQuery("select * from global_statistics");
                if (dataTable.Rows.Count == 0)
                    return null;
                return dataTable.Rows[0]["text"] as string;
            });
        }
        public string? LoadStatistics(int save_id)
        {
            return Guard(nameof(LoadStatistics), () =>
            {
                var dataTable = sqlDB.ExecuteQuery(
                    "select * from statistics where save_id = :save_id",
                    new SQLiteRow
                    {
                        { "save_id", save_id }
                    });
                return dataTable.Rows[0]["text"] as string;
            });
        }
        public Dictionary<string, int> LoadGlobalSettings()
        {
            return Guard(nameof(LoadGlobalSettings), () =>
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
            });
        }
        public Dictionary<string, int> LoadSettings()
        {
            return Guard(nameof(LoadSettings), () =>
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
            });
        }
        public void ClearSave()
        {
            Guard(nameof(ClearSave), () =>
            {
                sqlDB.ExecuteNonQuery("delete from saves");
                sqlDB.ExecuteNonQuery("delete from maps");
            });
        }
    }
}
