using System;
using System.Collections;
using System.Data;
using System.Xml.Serialization;
using System.Data.SqlClient;
using System.Data.Common;

namespace O2.Includes.DataBaseAccess
{
    public class Query
    {
        protected DBConn _conn;
        public DBConn conn
        {
            get { return _conn; }
            set { _conn = value; }
        }

        protected DbCommand _cmd;
        public DbCommand cmd
        {
            get { return _cmd; }
            set { _cmd = value; }
        }

        public DbParameterCollection parameters
        {
            get 
            {
                if (_cmd == null)
                    CreateCommand();

                return _cmd.Parameters;
            }
        }

        protected DataSet _dataSet;
        public DataSet dataSet
        {
            get { return _dataSet; }
            set { _dataSet = value; }
        }

        protected string _sql;
        public string sql
        {
            get { return _sql; }
            set { _sql = value; }
        }

        public Query() : this(new DBConn()) { }

        public Query(string dbConnName) : this(new DBConn(dbConnName)) { }

        public Query(DBConn _conn)
        {
            conn = _conn;
        }

        public DbParameter CreateParameter()
        {
            return cmd.CreateParameter();
        }

        public void AddParameter(DbParameter _parameter)
        {
            CheckParameter(_parameter);

            CreateCommand();
            cmd.Parameters.Add(_parameter);
        }

        public void AddParameter(string _parameterName, object _value)
        {
            CreateCommand();

            DbParameter p = cmd.CreateParameter();
            p.ParameterName = _parameterName;
            p.Value = _value;

            CheckParameter(p);
            cmd.Parameters.Add(p);
        }

        protected void CheckParameter(DbParameter _parameter)
        {
            if (conn.providerName == "MySql.Data.MySqlClient")
                _parameter.ParameterName = _parameter.ParameterName.Replace("@", "?");

            if (_parameter.Value == null)
                _parameter.Value = DBNull.Value;
        }

        public IDataReader ExecuteReader(string _sql)
        {
            sql = _sql;

            conn.Open();

            try
            {
                if (conn.transaction == null)
                    return BuildCommand().ExecuteReader(CommandBehavior.CloseConnection);
                else
                    return BuildCommand().ExecuteReader();
            }
            finally
            {
                parameters.Clear();
            }
        }
        
        public int ExecuteNonQuery(string _sql)
        {
            sql = _sql;

            conn.Open();
            
            try
            {
                return BuildCommand().ExecuteNonQuery();
            }
            finally
            {
                DisposeCommand();
            }
        }

        public object ExecuteScalar(string _sql)
        {
            sql = _sql;

            conn.Open();

            try
            {
                return BuildCommand().ExecuteScalar();
            }
            finally
            {
                DisposeCommand();
            }
        }

        public void CreateCommand()
        {
            if (cmd == null)
                cmd = conn.conn.CreateCommand();
        }
        
        private DbCommand BuildCommand()
        {
            if (cmd == null)
                cmd = conn.conn.CreateCommand();

            if (conn.providerName == "MySql.Data.MySqlClient")
                cmd.CommandText = sql.Replace("@", "?");
            else
                cmd.CommandText = sql;

            if (conn.transaction != null)
                cmd.Transaction = conn.transaction;
            
            if (cmd.Connection.State != ConnectionState.Open)
                cmd.Connection.Open();
            
            return cmd;
        }

        public void FillDataSet(string _sql, string table)
        {
            sql = _sql;
            FillDataSet(table);
        }

        public void FillDataSet(string table)
        {
            conn.Open();

            try
            {
                dataSet = new DataSet();

                DbDataAdapter da = conn.CreateDataAdapter(BuildCommand());
                da.Fill(dataSet, table);
            }
            finally
            {
                DisposeCommand();
            }
        }

        public DataTable get_DataTable(string _sql)
        {
            sql = _sql;

            conn.Open();
            
            try
            {
                DataTable dt = new DataTable();

                DbDataAdapter da = conn.CreateDataAdapter(BuildCommand());
                da.Fill(dt);
                
                return dt;
            }
            finally
            {
                DisposeCommand();
            }
        }

        protected void DisposeCommand()
        {
            cmd.Dispose();
            cmd = null;

            if (conn.transaction == null)
                conn.Close();
        }
    }
}