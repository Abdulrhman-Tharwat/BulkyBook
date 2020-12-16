using BulkyBook.DataAccess.Data;
using BulkyBook.DataAccess.Repository.IRepository;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BulkyBook.DataAccess.Repository
{
    public class SP_Call : ISP_Call
    {
        private readonly ApplicationDbContext _db;
        private static string ConnectionString = "";
        public SP_Call(ApplicationDbContext db)
        {
            _db = db;
            ConnectionString = _db.Database.GetDbConnection().ConnectionString;
        }
        public void Dispose()
        {
            _db.Dispose();
        }

        public void Execute(string ProcedureName, DynamicParameters Param = null)
        {
            using (SqlConnection sqlcon = new SqlConnection(ConnectionString))
            {
                sqlcon.Open();
                sqlcon.Execute(ProcedureName, Param, commandType: System.Data.CommandType.StoredProcedure);
            }
        }

        public IEnumerable<T> List<T>(string ProcedureName, DynamicParameters Param = null)
        {
            using (SqlConnection sqlcon = new SqlConnection(ConnectionString))
            {
                sqlcon.Open();
                return sqlcon.Query<T>(ProcedureName, Param, commandType: System.Data.CommandType.StoredProcedure);
            }
        }

        public Tuple<IEnumerable<T1>, IEnumerable<T2>> List<T1, T2>(string ProcedureName, DynamicParameters Param = null)
        {
            using (SqlConnection sqlcon = new SqlConnection(ConnectionString))
            {
                sqlcon.Open();
                var result = SqlMapper.QueryMultiple(sqlcon, ProcedureName, Param, commandType: System.Data.CommandType.StoredProcedure);
                var i1 = result.Read<T1>().ToList();
                var i2 = result.Read<T2>().ToList();
                if (i1 != null && i2 != null)
                {
                    return new Tuple<IEnumerable<T1>, IEnumerable<T2>>(i1, i2);
                }
            }
            return new Tuple<IEnumerable<T1>, IEnumerable<T2>>(new List<T1>(), new List<T2>());
        }

        public T OneRecord<T>(string ProcedureName, DynamicParameters Param = null)
        {
            using (SqlConnection sqlcon = new SqlConnection(ConnectionString))
            {
                sqlcon.Open();
                var val = sqlcon.Query<T>(ProcedureName, Param, commandType: System.Data.CommandType.StoredProcedure);
                return (T)Convert.ChangeType(val.FirstOrDefault(), typeof(T));
            }
        }

        public T Single<T>(string ProcedureName, DynamicParameters Param = null)
        {
            using (SqlConnection sqlcon = new SqlConnection(ConnectionString))
            {
                sqlcon.Open();
                return (T)Convert.ChangeType(sqlcon.ExecuteScalar<T>(ProcedureName, Param, commandType: System.Data.CommandType.StoredProcedure), typeof(T));
            }
        }
    }
}
