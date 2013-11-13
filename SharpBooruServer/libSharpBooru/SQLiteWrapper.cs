using System;
using System.IO;
using System.Data;
using System.Text;
using System.Data.SQLite;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace TA.SharpBooru
{
    public class SQLiteWrapper : IDisposable
    {
        private SQLiteConnection _Connection = null;

        static SQLiteWrapper()
        {
            SQLiteFunctionEx.RegisterFunction(typeof(MyRegEx));
            SQLiteFunctionEx.RegisterFunction(typeof(MyImageHashComparator));
        }

        public SQLiteWrapper(string Database)
        {
            if (!File.Exists(Database))
                throw new FileNotFoundException("Database file not found");
            string connectionString = string.Format("Data source={0}", Database);
            _Connection = new SQLiteConnection(connectionString);
            _Connection.Open();
        }

        public ulong GetLastInsertedID()
        {
            using (SQLiteCommand command = _Connection.CreateCommand())
            {
                command.CommandText = "SELECT last_insert_rowid()";
                return Convert.ToUInt64(command.ExecuteScalar());
            }
        }

        public DataTable ExecuteTable(string SQL, params object[] Args)
        {
            using (SQLiteCommand command = _Connection.CreateCommand())
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

        public int ExecuteNonQuery(string SQL, params object[] Args)
        {
            using (SQLiteCommand command = _Connection.CreateCommand())
            {
                command.CommandText = SQL;
                command.Prepare();
                foreach (object arg in Args)
                {
                    SQLiteParameter param = command.CreateParameter();
                    param.Value = arg;
                    command.Parameters.Add(param);
                }
                return command.ExecuteNonQuery();
            }
        }

        public ulong ExecuteInsert(string TableName, Dictionary<string, object> Dictionary)
        {
            if (Dictionary.Count > 0)
            {
                //TODO X Test ExecuteInsert
                StringBuilder statement = new StringBuilder();
                statement.AppendFormat("INSERT INTO {0} ({1}) VALUES(", TableName, string.Join(", ", Dictionary.Keys));
                for (int i = 0; i < Dictionary.Count; i++)
                {
                    if (i > 0)
                        statement.Append(", ?");
                    else statement.Append('?');
                }
                statement.Append(')');
                using (SQLiteCommand command = _Connection.CreateCommand())
                {
                    command.CommandText = statement.ToString();
                    command.Prepare();
                    foreach (object arg in Dictionary.Values)
                    {
                        SQLiteParameter param = command.CreateParameter();
                        param.Value = arg;
                        command.Parameters.Add(param);
                    }
                    command.ExecuteNonQuery();
                    return GetLastInsertedID();
                }
            }
            else throw new ArgumentException("Dictionary must contain things");
        }

        public T ExecuteScalar<T>(string SQL, params object[] Args)
        {
            using (SQLiteCommand command = _Connection.CreateCommand())
            {
                command.CommandText = SQL;
                command.Prepare();
                foreach (object arg in Args)
                {
                    SQLiteParameter param = command.CreateParameter();
                    param.Value = arg;
                    command.Parameters.Add(param);
                }
                object retObj = command.ExecuteScalar();
                return (T)Convert.ChangeType(retObj, typeof(T));
            }
        }

        public void Dispose()
        {
            _Connection.Close();
            _Connection.Dispose();
        }

        [SQLiteFunction(Name = "REGEXP", Arguments = 2, FuncType = FunctionType.Scalar)]
        internal class MyRegEx : SQLiteFunction { public override object Invoke(object[] args) { return Regex.IsMatch(Convert.ToString(args[1]), Convert.ToString(args[0])); } }

        [SQLiteFunction(Name = "IHCOMP", Arguments = 2, FuncType = FunctionType.Scalar)]
        internal class MyImageHashComparator : SQLiteFunction
        {
            public override object Invoke(object[] args)
            {
                ulong hash_diff = Convert.ToUInt64(args[0]) ^ Convert.ToUInt64(args[1]);
                byte one_count = 0;
                for (byte i = 0; i < 64; i++)
                    if ((hash_diff & (1UL << i)) > 0)
                        one_count++;
                return one_count;
            }
        }
    }
}