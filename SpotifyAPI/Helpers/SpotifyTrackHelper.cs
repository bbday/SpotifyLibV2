using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using MusicLibrary.Interfaces;
using SpotifyLibrary.Sql;
using SpotifyLibrary.Sql.DBModels;

namespace SpotifyLibrary.Helpers
{
    public static class SpotifyTrackHelper
    {
        public static async Task<DbTrack> GetOrFetchTrack(
            SpotifyClient client,
            string id)
        {
            using var sqlCon = SqlDb.Connection(client.SqlPath);
            var tem = SqlDb.GetTrack(sqlCon, id);
            if (tem != null) return tem;

            var fetch = await (await client.TracksClient).GetTrack(id);
            var dbTrack = SpotifyPlaylistHelper.FullTrackToDbTrack(fetch);
            SqlDb.AddTrack(sqlCon, dbTrack);
            return dbTrack;
        }
    }
}
