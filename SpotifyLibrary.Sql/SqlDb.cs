using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Newtonsoft.Json;
using SpotifyLibrary.Sql.DBModels;
using SQLite;

namespace SpotifyLibrary.Sql
{
    public static class SqlDb
    {
        private static JsonSerializerSettings _settings;

        public static JsonSerializerSettings Settings
            => _settings ??= new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All,
                NullValueHandling = NullValueHandling.Ignore
            };
        private static string _path;
        public static SQLiteConnection Connection(string path = null)
        {
            _path ??= path;
            var db = new SQLiteConnection(_path);
            db.CreateTable<DbTrack>();
            db.CreateTable<DbAlbum>();
            return db;
        }
        public static void AddTrack(SQLiteConnection db,
            DbTrack track)
        {
            db.Insert(track);
        }

        public static DbTrack GetTrack(SQLiteConnection con, string id)
        {
            return con.Table<DbTrack>().FirstOrDefault(s
                => s.Id == id);
        }

        public static List<DbTrack> GetTracks(SQLiteConnection con, IEnumerable<string> @select)
        {
            var a = @select.ToList();
            var db = new List<DbTrack>();
            for (int i = 0; i < a.Count; i += 999)
            {
                var adaptedA = a.Skip(i).Take(999);
                db.AddRange(con.Table<DbTrack>().Where(s
                        => adaptedA.Contains(s.Id))
                    .ToList());
            }
            return db;
        }

        public static int CreateTracks(SQLiteConnection con, IEnumerable<DbTrack> tracks)
        {
            con.Trace = false;
            return con.InsertAll(tracks);
        }

    }
}
