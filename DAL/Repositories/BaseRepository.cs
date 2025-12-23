using Median.Authentication.Simple.Common;
using Microsoft.Extensions.Options;
using Npgsql;
using System.Data;

namespace Profakt.Intranet.DAL.Repositories
{
    public class BaseRepository
    {
        private readonly string _connectionString;

        protected BaseRepository(IOptions<DatabaseSettings> dbSettings)
        {
            _connectionString = dbSettings.Value.DefaultConnection;
        }

        protected IDbConnection CreateConnection()
        {
            var conn = new NpgsqlConnection(_connectionString);
            Console.WriteLine("Opening DB connection to " + _connectionString);
            conn.Open();
            return conn;
        }
    }
}
