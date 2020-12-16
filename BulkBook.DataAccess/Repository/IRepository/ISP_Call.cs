using Dapper;
using System;
using System.Collections.Generic;
using System.Text;

namespace BulkyBook.DataAccess.Repository.IRepository
{
    public interface ISP_Call : IDisposable
    {
        T Single<T>(string ProcedureName, DynamicParameters Param = null);
        void Execute(string ProcedureName, DynamicParameters Param = null);
        T OneRecord<T>(string ProcedureName, DynamicParameters Param = null);
        IEnumerable<T> List<T>(string ProcedureName, DynamicParameters Param = null);
        Tuple<IEnumerable<T1>,IEnumerable<T2>> List<T1,T2>(string ProcedureName, DynamicParameters Param = null);
    }
}
