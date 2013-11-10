using System;
using System.IO;
using System.Data;
using System.Text;
using System.Data.SQLite;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace TA.SharpBooru
{
    public class SQLiteWrapper
    {
        private string _Database;
        private bool _IsOpened = false;
        private object _Lock = new object();
        private SQLiteConnection _Connection;

        public SQLiteWrapper(string Database)
        {
            _Database = Database;
            SQLiteFunction.RegisterFunction(typeof(MyRegEx));
        }

        public SQLiteConnection Connect()
        {
            lock (_Lock)
            {
                if (!_IsOpened)
                {
                    if (!File.Exists(_Database))
                        throw new FileNotFoundException("Database file not found");
                    string connectionString = string.Format("Data source={0}", _Database);
                    if (_Connection != null)
                        _Connection.Dispose();
                    _Connection = new SQLiteConnection(connectionString);
                    _Connection.Open();
                    _IsOpened = true;
                }
                return (SQLiteConnection)_Connection.Clone();
            }
        }

        public int GetLastInsertedID()
        {
            lock (_Lock)
                using (SQLiteConnection connection = Connect())
                using (SQLiteCommand command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT last_insert_rowid()";
                    return Convert.ToInt32(command.ExecuteScalar());
                }
        }

        public DataTable ExecuteTable(string SQL, params object[] Args)
        {
            lock (_Lock)
                using (SQLiteConnection connection = Connect())
                using (SQLiteCommand command = connection.CreateCommand())
                {
                    command.CommandText = SQL;
                    command.Prepare();
                    foreach (object arg in Args)
                    {
                        SQLiteParameter param = command.CreateParameter();
                        param.Value = arg;
                        command.Parameters.Add(param);
                    }
                    DataTable dt = new DataTable();
                    dt.Load(command.ExecuteReader());
                    return dt;
                }
        }

        public DataRow ExecuteRow(string SQL, params object[] Args)
        {
            using (DataTable table = ExecuteTable(SQL, Args))
                if (table.Rows.Count > 0)
                    return table.Rows[0];
                else return null;
        }

        public int ExecuteInt(string SQL, params object[] Args) { return ExecuteInt(false, SQL, Args); }
        public int ExecuteInt(bool ReturnLastInsertedID, string SQL, params object[] Args)
        {
            lock (_Lock)
                using (SQLiteConnection connection = Connect())
                using (SQLiteCommand command = connection.CreateCommand())
                {
                    command.CommandText = SQL;
                    command.Prepare();
                    foreach (object arg in Args)
                    {
                        SQLiteParameter param = command.CreateParameter();
                        param.Value = arg;
                        command.Parameters.Add(param);
                    }
                    int cnt = command.ExecuteNonQuery();
                    return ReturnLastInsertedID ? GetLastInsertedID() : cnt;
                }
        }

        public int ExecuteInsert(string TableName, Dictionary<string, object> Dictionary)
        {
            if (Dictionary.Count > 0)
            {
                //TODO X Test ExecuteInsert
                StringBuilder statement = new StringBuilder();
                statement.AppendFormat("INSERT INTO {0} ({1}) VALUES(", TableName, string.Join(", ", Dictionary.Keys));
                for (int i = 0; i < Dictionary.Count;i++)
                {
                    if (i > 0)
                        statement.Append(", ?");
                    else statement.Append('?');
                }
                statement.Append(')');
                return ExecuteInt(statement.ToString(), Dictionary.Values);
            }
            else throw new ArgumentException("Dictionary must contain things");
        }

        [SQLiteFunction(Name = "REGEXP", Arguments = 2, FuncType = FunctionType.Scalar)]
        internal class MyRegEx : SQLiteFunction
        {
            public override object Invoke(object[] args)
            {
                if (args.Length != 2)
                    throw new Exception("2 args expected");
                else return Regex.IsMatch(Convert.ToString(args[1]), Convert.ToString(args[0]));
            }
        }
    }
}