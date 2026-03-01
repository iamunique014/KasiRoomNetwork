using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KasiRoomNetwork.Data.DataAccess
{
    public class SqlDataAccess : ISqlDataAccess
    {
        public readonly IConfiguration _config;

        public SqlDataAccess(IConfiguration config)
        {
            _config = config;
        }

        //Method to retrive all records from the databaase including singular records
        public async Task<IEnumerable<T>> GetData<T, P>(string spName, P parameters, string connectionID = "conn")
        {
            using IDbConnection connection = new SqlConnection(_config.GetConnectionString(connectionID));
            return await connection.QueryAsync<T>(spName, parameters, commandType: CommandType.StoredProcedure);
        }

        //Method For executing the stored procedures that insert, delete and update data in the database.
        public async Task SaveData<T>(string spName, T parameters, string connectionID = "conn")
        {
            using IDbConnection connection = new SqlConnection(_config.GetConnectionString(connectionID));
            await connection.ExecuteAsync(spName, parameters, commandType: CommandType.StoredProcedure);
        }

        public async Task<IEnumerable<TReturn>> GetMultiData<TFirst, TSecond, TReturn>(
        string spName,
        Func<TFirst, TSecond, TReturn> map,
        object parameters,
        string splitOn,
        string connectionID = "conn")
        {
            using IDbConnection connection =
                new SqlConnection(_config.GetConnectionString(connectionID));

            return await connection.QueryAsync<TFirst, TSecond, TReturn>(
                spName,
                map,
                parameters,
                commandType: CommandType.StoredProcedure,
                splitOn: splitOn);
        }
    }
}
