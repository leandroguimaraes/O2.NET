using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Reflection;
using System.IO;
using System.Xml.Serialization;
using System.Data.Common;

namespace O2.Includes.DataBaseAccess
{
    public class DBConn
    {
        protected string _connectionName;
        public string connectionName
        {
            get { return _connectionName; }
            set { _connectionName = value; }
        }

        protected string _connectionString;
        public string connectionString
        {
            get { return _connectionString; }
            set { _connectionString = value; }
        }

        protected string _providerName;
        public string providerName
        {
            get { return _providerName; }
            set { _providerName = value; }
        }

        [NonSerialized, XmlIgnore, SoapIgnore]
        protected DbConnection _conn;
        [XmlIgnore, SoapIgnore]
        public DbConnection conn
        {
            get
            {
                if (_conn == null)
                    _conn = DbProviderFactories.GetFactory(providerName).CreateConnection();

                return _conn;
            }

            set { _conn = value; }
        }

        [NonSerialized, XmlIgnore, SoapIgnore]
        protected DbTransaction _transaction;
        [XmlIgnore, SoapIgnore]
        public DbTransaction transaction
        {
            get { return _transaction; }
            set { _transaction = value; }
        }

        public DBConn() : this(ConfigurationManager.ConnectionStrings[0].Name, ConfigurationManager.ConnectionStrings[0].ConnectionString, ConfigurationManager.ConnectionStrings[0].ProviderName) {}

        public DBConn(string _connectionName)
        {
            bool found = false;

            for (int i = 0; i < ConfigurationManager.ConnectionStrings.Count; i++)
            {
                if (ConfigurationManager.ConnectionStrings[i].Name == _connectionName)
                {
                    Constructor(ConfigurationManager.ConnectionStrings[i].Name, ConfigurationManager.ConnectionStrings[i].ConnectionString, ConfigurationManager.ConnectionStrings[i].ProviderName);
                    found = true;
                    break;
                }
            }

            if (!found)
                throw new Exception(Languages.Errors.CONNECTION_STRING_NOT_FOUND.Replace("{STR}", _connectionName));
        }

        public DBConn(string _connectionName, string _connectionString, string _providerName)
        {
            Constructor(_connectionName, _connectionString, _providerName);
        }

        private void Constructor(string _connectionName, string _connectionString, string _providerName)
        {
            connectionName = _connectionName;
            connectionString = _connectionString;
            providerName = _providerName;
        }

        public void ChangeDatabase(string DBName)
        {
            conn.ChangeDatabase(DBName);
        }

        public DbDataAdapter CreateDataAdapter()
        {
            return DbProviderFactories.GetFactory(providerName).CreateDataAdapter();
        }

        public DbDataAdapter CreateDataAdapter(DbCommand cmd)
        {
            DbDataAdapter da = CreateDataAdapter();
            da.SelectCommand = cmd;

            return da;
        }

        public void BeginTransaction()
        {
            if ((conn == null) || (conn.State == ConnectionState.Closed))
                Open();

            if (transaction == null)
                transaction = conn.BeginTransaction();
        }

        public void TransactionCommit()
        {
            transaction.Commit();
            transaction = null;
            Close();
        }

        public void TransactionRollback()
        {
            transaction.Rollback();
            transaction = null;
            Close();
        }

        public void Open()
        {
            if ((conn == null) || (conn.State == ConnectionState.Closed))
            {
                conn.ConnectionString = connectionString;
                conn.Open();
            }
        }

        public void Close()
        {
            if (conn.State == ConnectionState.Open)
                conn.Close();
        }
    }
}