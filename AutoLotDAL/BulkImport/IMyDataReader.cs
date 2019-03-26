using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoLotDAL.DataOperations;
using AutoLotDAL.Models;
using System.Data;
using System.Data.SqlClient;

namespace AutoLotDAL.BulkImport
{
    public interface IMyDataReader<T> : IDataReader
    {
        List<T> Records { get; set; }
    }
}
