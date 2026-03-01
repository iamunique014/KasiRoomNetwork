using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KasiRoomNetwork.Data.DataAccess
{
    public interface ISqlDataAccess
    {
        Task<IEnumerable<T>> GetData<T, P>(string spName, P parameters, string connectionID = "conn");
        Task SaveData<T>(string spName, T parametrs, string connectionID = "conn");


        Task<IEnumerable<TReturn>> GetMultiData<TFirst, TSecond, TReturn>(
            string spName,
            Func<TFirst, TSecond, TReturn> map,
            object parameters,
            string splitOn,
            string connectionID = "conn");
       
    }
}
