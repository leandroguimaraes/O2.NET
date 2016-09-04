using System;
using System.Collections;
using System.Collections.Specialized;
using System.Reflection;
using System.Data;
using System.Runtime.Serialization.Formatters.Soap;
using System.Xml.Serialization;
using System.IO;
using System.Text;
using System.Configuration;
using System.ComponentModel;
using O2.Includes.DataBaseAccess;
using System.Runtime.Serialization;
using System.Data.Common;
using System.Collections.Generic;
using O2.Includes.Exceptions;

/// <summary>
/// System Object
/// </summary>
namespace O2.Includes
{
    [Serializable, DataObjectAttribute]
    public abstract class SysObject
    {
        public string get_table()
        {
            if (GetType().GetField("table") == null)
                throw new ClassWithoutTableFieldException(GetType());

            return ConfigurationManager.AppSettings["TBPREFIX"] + GetType().GetField("table").GetValue(this);
        }

        public string get_col_id()
        {
            return (GetType().GetField("col_id") == null) ? "id" : GetType().GetField("col_id").GetValue(this).ToString();
        }

        public int get_id_value()
        {
            return Convert.ToInt32(GetType().GetProperty(get_col_id()).GetValue(this, null));
        }

        public enum OPERATION
        {
            Insert,
            Load,
            Update,
            Delete
        }

        [NonSerialized, XmlIgnore, SoapIgnore]
        protected Query _query;
        [XmlIgnore, SoapIgnore]
        public Query query
        {
            get { return _query; }
            set { _query = value; }
        }

        [NonSerialized, XmlIgnore, SoapIgnore]
        protected int? _id;
        public int? id
        {
            get { return _id; }
            set { _id = value; }
        }

        [NonSerialized, XmlIgnore, SoapIgnore]
        protected List<TableMetaData> _dbVars;
        [XmlIgnore, SoapIgnore]
        public List<TableMetaData> dbVars
        {
            get { return _dbVars; }
            set { _dbVars = value; }
        }

        public SysObject() : this(ConfigurationManager.ConnectionStrings[0].Name) { }

        public SysObject(string DBConnName) : this(new DBConn(DBConnName)) { }

        public SysObject(DBConn conn)
        {
            query = new Query(conn);
        }

        public void set_QueryConn(DBConn conn)
        {
            query.conn = conn;
        }

        protected void Load_dbVars()
        {
            Load_dbVars(get_table());
        }

        protected void Load_dbVars(string table)
        {
            if ((dbVars == null) || (dbVars.Count == 0))
                dbVars = get_dbVars(table);
        }

        [XmlInclude(typeof(TableMetaData)), SoapInclude(typeof(TableMetaData))]
        public static List<TableMetaData> get_dbVars(string _table)
        {
            Query query = new Query();
            query.AddParameter("@table", _table);

            string sql = null;
            switch (query.conn.providerName)
            {
                case "System.Data.SqlClient":
                case "System.Data.SqlServerCe.3.5":
                case "MySql.Data.MySqlClient":
                case "PostgreSQL":
                    sql = "SELECT DISTINCT COLUMN_NAME, IS_NULLABLE, DATA_TYPE, CHARACTER_MAXIMUM_LENGTH FROM INFORMATION_SCHEMA.COLUMNS WHERE table_name = @table";
                    break;

                case "FirebirdSql.Data.FirebirdClient":
                    query.parameters["@table"].Value = query.parameters["@table"].Value.ToString().ToUpper();
                    sql = "SELECT TRIM(LOWER(RDB$FIELD_NAME)) AS COLUMN_NAME, (CASE RDB$NULL_FLAG WHEN 1 THEN 'FALSE' ELSE 'TRUE' END) AS IS_NULLABLE FROM RDB$RELATION_FIELDS WHERE RDB$RELATION_NAME = @table";
                    break;

                case "Oracle.DataAccess.Client":
                    break;

                default:
                    break;
            }

            DataTable dtMetaInfo = query.get_DataTable(sql);

            if (dtMetaInfo.Rows.Count > 0)
            {
                List<TableMetaData> dbVars = new List<TableMetaData>(dtMetaInfo.Rows.Count);
                foreach (DataRow row in dtMetaInfo.Rows)
                    dbVars.Add(new TableMetaData(row));

                return dbVars;
            }
            else
                throw new Exception(Languages.Errors.UNEXISTENT_TABLE.Replace("{T}", _table));
        }

        public virtual void Insert()
        {
            bool commit_transaction = (query.conn.transaction == null);

            if (commit_transaction)
                query.conn.BeginTransaction();

            try
            {
                Load_dbVars(get_table());

                string error_msg = "";

                BindingFlags bf = BindingFlags.Instance;

                if (query.parameters != null)
                    query.parameters.Clear();

                string sql = "INSERT INTO " + get_table() + " (";
                string values = "";
                for (int i = 0; i < dbVars.Count; i++)
                {
                    //id attribute is auto generated to INSERT statement
                    if (((TableMetaData)dbVars[i]).column_name != get_col_id())
                    {
                        switch (query.conn.providerName)
                        {
                            case "System.Data.SqlClient":
                            case "System.Data.SqlServerCe.3.5":
                                sql += "[" + ((TableMetaData)dbVars[i]).column_name + "],";
                                break;

                            case "MySql.Data.MySqlClient":
                                sql += "`" + ((TableMetaData)dbVars[i]).column_name + "`,";
                                break;

                            case "FirebirdSql.Data.FirebirdClient":
                                sql += "\"" + ((TableMetaData)dbVars[i]).column_name.ToUpper() + "\",";
                                break;

                            default:
                                sql += ((TableMetaData)dbVars[i]).column_name + ",";
                                break;
                        }

                        values += "@" + ((TableMetaData)dbVars[i]).column_name + ",";

                        try
                        {
                            var value = GetType().GetProperty(((TableMetaData)dbVars[i]).column_name.ToLower()).GetValue(this, null);
                            if ((value != null) && (value != DBNull.Value))
                            {
                                // null DateTime value
                                if (value.GetType().FullName.Contains("System.DateTime"))
                                {
                                    if (DateTime.Parse(((DateTime)value).ToString("yyyy-MM-dd HH:mm:ss.fff")) == DateTime.MinValue)
                                        query.AddParameter("@" + ((TableMetaData)dbVars[i]).column_name, DBNull.Value);
                                    else
                                        query.AddParameter("@" + ((TableMetaData)dbVars[i]).column_name, value);
                                }
                                else
                                {
                                    //validate string length against data base column allowed size, avoiding generic messages from SQL SERVER
                                    if ((((TableMetaData)dbVars[i]).character_maximum_length > 0)
                                        && (value.GetType().FullName.Contains("System.String"))
                                        && (value.ToString().Length > ((TableMetaData)dbVars[i]).character_maximum_length))
                                        throw new Exception(Languages.Errors.STRING_FIELD_BIGGER_THAN_DB_COLUMN.Replace("{ATT}", GetType().FullName + "." + ((TableMetaData)dbVars[i]).column_name).Replace("{ATT_SZ}", value.ToString().Length.ToString()).Replace("{DB_SZ}", ((TableMetaData)dbVars[i]).character_maximum_length.ToString()));

                                    query.AddParameter("@" + ((TableMetaData)dbVars[i]).column_name, value);
                                }
                            }
                            else if (((TableMetaData)dbVars[i]).is_nullable)
                                query.AddParameter("@" + ((TableMetaData)dbVars[i]).column_name, DBNull.Value);
                            else
                                error_msg += Environment.NewLine + ((TableMetaData)dbVars[i]).column_name;
                        }
                        catch(NullReferenceException)
                        {
                            throw new NullReferenceException(Languages.Errors.NO_CLASS_PRIVATE_FIELD.Replace("{CF}", GetType().FullName + "." + ((TableMetaData)dbVars[i]).column_name).Replace("{OP}", "SysObject.Insert()"));
                        }
                    }
                }

                if (error_msg != "")
                    throw new Exception(Languages.Errors.FOLLOW_NOT_NULL.Replace("{NOT_NULL}", Environment.NewLine + error_msg));

                sql = sql.Substring(0, sql.Length - 1) + ") VALUES (" + values.Substring(0, values.Length - 1) + ")";

                IDataReader reader = null;
                try
                {
                    switch (query.conn.providerName)
                    {
                        case "System.Data.SqlClient":
                            reader = query.ExecuteReader(sql + "; SELECT SCOPE_IDENTITY() AS id;");
                            break;

                        case "System.Data.SqlServerCe.3.5":
                            query.ExecuteNonQuery(sql);
                            reader = query.ExecuteReader("SELECT @@IDENTITY AS id;");
                            break;

                        case "MySql.Data.MySqlClient":
                            reader = query.ExecuteReader(sql + "; SELECT LAST_INSERT_ID() AS id;");
                            break;

                        case "FirebirdSql.Data.FirebirdClient":
                            id = Convert.ToInt32(query.ExecuteScalar(sql + " RETURNING id;"));
                            break;

                        case "PostgreSQL":
                            reader = query.ExecuteReader(sql + "; SELECT CAST(last_value AS int) AS id FROM organizations_org_id_seq;");
                            break;

                        case "Oracle.DataAccess.Client":
                            reader = query.ExecuteReader(sql + "; SELECT seq_orgs.nextval AS id FROM dual;");
                            break;

                        default:
                            throw new Exception(O2.Languages.Errors.INVALID_PROVIDER_NAME_AT.Replace("{PN}", query.conn.providerName).Replace("{AT}", "SysObject.Insert()"));
                    }

                    if ((reader != null) && (reader.Read()))
                        GetType().GetProperty("id").SetValue(this, Convert.ToInt32(reader["id"]), null);
                }
                finally
                {
                    if (reader != null)
                    {
                        reader.Close();
                        reader.Dispose();
                    }
                }

                if (commit_transaction)
                    query.conn.TransactionCommit();
            }
            catch
            {
                if (commit_transaction)
                    query.conn.TransactionRollback();

                id = null;

                throw;
            }
            finally
            {
                if (commit_transaction)
                    query.conn.Close();
            }
        }

        public virtual void Load(int? _id)
        {
            query.AddParameter("@id", _id);

            IDataReader reader = query.ExecuteReader("SELECT * FROM " + get_table() + " WHERE " + get_col_id() + " = @id");

            try
            {
                if (reader.Read())
                    LoadBy_array(reader);
                else
                    throw new Exception(Languages.Errors.SELECT_ERROR);
            }
            finally
            {
                reader.Close();
                reader.Dispose();
            }
        }

        public virtual void LoadBy_array(NameValueCollection values)
        {
            Load_dbVars(get_table());

            BindingFlags bf = BindingFlags.NonPublic | BindingFlags.Instance;
            for (int i = 0; i < dbVars.Count; i++)
            {
                if (values[((TableMetaData)dbVars[i]).column_name] != null)
                {
                    string field_type = GetType().GetField("_" + ((TableMetaData)dbVars[i]).column_name.ToLower(), bf).ToString();
                    field_type = field_type.Substring(0, field_type.IndexOf(' '));

                    try
                    {
                        switch (field_type)
                        {
                            case "System.String":
                                try
                                {
                                    //string
                                    GetType().GetProperty(((TableMetaData)dbVars[i]).column_name).SetValue(this, values[((TableMetaData)dbVars[i]).column_name], null);
                                }
                                catch
                                {
                                    //char
                                    GetType().GetProperty(((TableMetaData)dbVars[i]).column_name).SetValue(this, values[((TableMetaData)dbVars[i]).column_name].ToString()[0], null);
                                }
                                break;

                            case "Byte":
                                GetType().GetProperty(((TableMetaData)dbVars[i]).column_name).SetValue(this, (Byte)int.Parse(values[((TableMetaData)dbVars[i]).column_name].ToString()), null);
                                break;

                            case "UInt16":
                            case "Int16":
                                if (!String.IsNullOrEmpty(values[((TableMetaData)dbVars[i]).column_name]))
                                    GetType().GetProperty(((TableMetaData)dbVars[i]).column_name).SetValue(this, Convert.ToInt16(values[((TableMetaData)dbVars[i]).column_name].Replace(',', '.').ToString()), null);
                                else
                                    GetType().GetProperty(((TableMetaData)dbVars[i]).column_name).SetValue(this, 0, null);
                                break;

                            case "UInt32":
                            case "Int32":
                                if (!String.IsNullOrEmpty(values[((TableMetaData)dbVars[i]).column_name]))
                                    GetType().GetProperty(((TableMetaData)dbVars[i]).column_name).SetValue(this, Convert.ToInt32(values[((TableMetaData)dbVars[i]).column_name].Replace(',', '.').ToString()), null);
                                else
                                    GetType().GetProperty(((TableMetaData)dbVars[i]).column_name).SetValue(this, 0, null);
                                break;

                            case "Single":
                                try
                                {
                                    // single
                                    GetType().GetProperty(((TableMetaData)dbVars[i]).column_name).SetValue(this, Convert.ToSingle(values[((TableMetaData)dbVars[i]).column_name]), null);
                                }
                                catch
                                {
                                    // ToDo: convert to decimal
                                    // GetType().GetProperty(field.ItemArray[0].ToString()).SetValue(this, Convert.ToDecimal(reader[field.ItemArray[0].ToString()]));
                                }
                                break;

                            case "Double":
                                GetType().GetProperty(((TableMetaData)dbVars[i]).column_name).SetValue(this, Convert.ToDouble(values[((TableMetaData)dbVars[i]).column_name]), null);
                                break;

                            case "System.DateTime":
                                if (!String.IsNullOrEmpty(values[((TableMetaData)dbVars[i]).column_name]))
                                    GetType().GetProperty(((TableMetaData)dbVars[i]).column_name).SetValue(this, DateTime.Parse(values[((TableMetaData)dbVars[i]).column_name]), null);
                                break;

                            default:
                                GetType().GetProperty(((TableMetaData)dbVars[i]).column_name).SetValue(this, values[((TableMetaData)dbVars[i]).column_name], null);
                                break;
                        }
                    }
                    catch (Exception excp)
                    {
                        throw new Exception(this.ToString() + "." + ((TableMetaData)dbVars[i]).column_name + Environment.NewLine + Environment.NewLine + excp.Message);
                    }
                }
            }
        }

        public void LoadBy_array(IDataReader reader)
        {
            DataTable schemaTable = reader.GetSchemaTable();

            BindingFlags bf = BindingFlags.NonPublic | BindingFlags.Instance;
            foreach (DataRow field in schemaTable.Rows)
            {
                try
                {
                    PropertyInfo property = GetType().GetProperty(field.ItemArray[0].ToString().ToLower());
                    if (!Convert.IsDBNull(reader[field.ItemArray[0].ToString()]))
                    {
                        switch (reader[field.ItemArray[0].ToString()].GetType().ToString())
                        {
                            case "System.String":
                                try
                                {
                                    //string
                                    property.SetValue(this, reader[field.ItemArray[0].ToString().ToLower()], null);
                                }
                                catch
                                {
                                    //char
                                    property.SetValue(this, reader[field.ItemArray[0].ToString().ToLower()].ToString()[0], null);
                                }
                                break;

                            case "System.UInt16":
                            case "System.Int16":
                                property.SetValue(this, Convert.ToInt16(reader[field.ItemArray[0].ToString().ToLower()]), null);
                                break;

                            case "System.UInt32":
                            case "System.Int32":
                                property.SetValue(this, Convert.ToInt32(reader[field.ItemArray[0].ToString().ToLower()]), null);
                                break;

                            case "System.Decimal":
                                try
                                {
                                    // decimal
                                    property.SetValue(this, Convert.ToDecimal(reader[field.ItemArray[0].ToString().ToLower()]), null);
                                }
                                catch
                                {
                                    // single
                                    property.SetValue(this, Convert.ToSingle(reader[field.ItemArray[0].ToString().ToLower()]), null);
                                }
                                break;

                            case "System.Double":
                                property.SetValue(this, Convert.ToDouble(reader[field.ItemArray[0].ToString().ToLower()]), null);
                                break;

                            case "System.DateTime":
                                if (reader[field.ItemArray[0].ToString().ToLower()] != DBNull.Value)
                                    property.SetValue(this, reader[field.ItemArray[0].ToString().ToLower()], null);
                                break;

                            default:
                                property.SetValue(this, reader[field.ItemArray[0].ToString().ToLower()], null);
                                break;
                        }
                    }
                    else
                        property.SetValue(this, null, null);
                }
                catch (Exception e)
                {
                    throw new Exception(this.ToString() + "." + field.ItemArray[0].ToString() + Environment.NewLine + Environment.NewLine + e.Message);
                }
            }
        }

        public void LoadBy_array(DataRow row)
        {
            BindingFlags bf = BindingFlags.NonPublic | BindingFlags.Instance;

            for (int i = 0; i < row.Table.Columns.Count; i++)
                if (GetType().GetField("_" + row.Table.Columns[i].ColumnName.ToLower(), bf) != null)
                {
                    switch (row[row.Table.Columns[i].ColumnName].GetType().ToString())
                    {
                        case "System.DateTime":
                            if (row[row.Table.Columns[i].ColumnName] != DBNull.Value)
                                GetType().GetField("_" + row.Table.Columns[i].ColumnName.ToLower(), bf).SetValue(this, row[row.Table.Columns[i].ColumnName]);
                            break;

                        case "System.UInt16":
                        case "System.Int16":
                            GetType().GetField("_" + row.Table.Columns[i].ColumnName.ToLower(), bf).SetValue(this, Convert.ToInt16(row[row.Table.Columns[i].ColumnName]));
                            break;

                        case "System.UInt32":
                        case "System.Int32":
                            GetType().GetField("_" + row.Table.Columns[i].ColumnName.ToLower(), bf).SetValue(this, Convert.ToInt32(row[row.Table.Columns[i].ColumnName]));
                            break;

                        case "System.DBNull":
                            string field_type = GetType().GetField("_" + row.Table.Columns[i].ColumnName.ToLower(), bf).ToString();
                            field_type = field_type.Substring(0, field_type.IndexOf(' '));

                            switch (field_type)
                            {
                                case "Int32":
                                    break;

                                default:
                                    GetType().GetField("_" + row.Table.Columns[i].ColumnName.ToLower(), bf).SetValue(this, null);
                                    break;
                            }
                            break;

                        default:
                            GetType().GetField("_" + row.Table.Columns[i].ColumnName.ToLower(), bf).SetValue(this, row[row.Table.Columns[i].ColumnName]);
                            break;
                    }
                }
        }

        public virtual void Update()
        {
            if (get_id_value() <= 0)
                throw new Exception(Languages.Errors.UPDATE_ERROR_ID_ZERO);

            bool commit_transaction = (query.conn.transaction == null);

            if (commit_transaction)
                query.conn.BeginTransaction();

            try
            {
                Load_dbVars(get_table());

                string error_msg = "";

                BindingFlags bf = BindingFlags.NonPublic | BindingFlags.Instance;

                string sql = "UPDATE " + get_table() + " SET ";
                for (int i = 0; i < dbVars.Count; i++)
                {
                    //id attribute is not updatable
                    if (((TableMetaData)dbVars[i]).column_name != get_col_id())
                    {
                        switch (query.conn.providerName)
                        {
                            case "System.Data.SqlClient":
                            case "System.Data.SqlServerCe.3.5":
                                sql += "[" + ((TableMetaData)dbVars[i]).column_name + "]";
                                break;

                            case "MySql.Data.MySqlClient":
                                sql += "`" + ((TableMetaData)dbVars[i]).column_name + "`";
                                break;

                            case "FirebirdSql.Data.FirebirdClient":
                                sql += "\"" + ((TableMetaData)dbVars[i]).column_name.ToUpper() + "\"";
                                break;

                            default:
                                sql += ((TableMetaData)dbVars[i]).column_name;
                                break;
                        }

                        sql += "=@" + ((TableMetaData)dbVars[i]).column_name + ",";

                        try
                        {
                            var value = GetType().GetProperty(((TableMetaData)dbVars[i]).column_name.ToLower()).GetValue(this, null);
                            if ((value != null) && (value != DBNull.Value))
                            {
                                // null DateTime value
                                if (value.GetType().FullName.Contains("System.DateTime"))
                                {
                                    if (DateTime.Parse(((DateTime)value).ToString("yyyy-MM-dd HH:mm:ss.fff")) == DateTime.MinValue)
                                        query.AddParameter("@" + ((TableMetaData)dbVars[i]).column_name, DBNull.Value);
                                    else
                                        query.AddParameter("@" + ((TableMetaData)dbVars[i]).column_name, value);
                                }
                                else
                                {
                                    //validate string length against data base column allowed size, avoiding generic messages from SQL SERVER
                                    if ((((TableMetaData)dbVars[i]).character_maximum_length > 0)
                                        && (value.GetType().FullName.Contains("System.String"))
                                        && (value.ToString().Length > ((TableMetaData)dbVars[i]).character_maximum_length))
                                        throw new Exception(Languages.Errors.STRING_FIELD_BIGGER_THAN_DB_COLUMN.Replace("{ATT}", GetType().FullName + "." + ((TableMetaData)dbVars[i]).column_name).Replace("{ATT_SZ}", value.ToString().Length.ToString()).Replace("{DB_SZ}", ((TableMetaData)dbVars[i]).character_maximum_length.ToString()));

                                    query.AddParameter("@" + ((TableMetaData)dbVars[i]).column_name, value);
                                }
                            }
                            else if (((TableMetaData)dbVars[i]).is_nullable)
                                query.AddParameter("@" + ((TableMetaData)dbVars[i]).column_name, DBNull.Value);
                            else
                                error_msg += Environment.NewLine + ((TableMetaData)dbVars[i]).column_name;
                        }
                        catch (NullReferenceException)
                        {
                            throw new NullReferenceException(Languages.Errors.NO_CLASS_PRIVATE_FIELD.Replace("{CF}", GetType().FullName + "." + ((TableMetaData)dbVars[i]).column_name).Replace("{OP}", "SysObject.Update()"));
                        }
                    }
                }

                if (error_msg != "")
                    throw new Exception(Languages.Errors.FOLLOW_NOT_NULL.Replace("{NOT_NULL}", Environment.NewLine + error_msg));

                sql = sql.Substring(0, sql.Length - 1) + " WHERE " + get_col_id() + " = @id";
                query.AddParameter("@id", get_id_value());

                query.ExecuteNonQuery(sql);

                if (commit_transaction)
                    query.conn.TransactionCommit();
            }
            catch
            {
                if (commit_transaction)
                    query.conn.TransactionRollback();

                throw;
            }
            finally
            {
                if (commit_transaction)
                    query.conn.Close();
            }
        }

        public virtual void Delete()
        {
            query.AddParameter("@id", get_id_value());
            
            int affectedRows = query.ExecuteNonQuery("DELETE FROM " + get_table() + " WHERE " + get_col_id() + " = @id");
            
            if (affectedRows == 0)
                throw new Exception(Languages.Errors.DELETE_ERROR);

            id = 0;
            /*
             ToDo: setar os campos como nulo
             
            Load_dbVars(get_table());

            for (int i = 0; i < dbVars.Count; i++)
                GetType().GetField(((TableMetaData)dbVars[i]).column_name).SetValue(this, null);
             */
        }

        public DataTable get_DataTable(string orderBy)
        {
            return get_DataTable("*", null, orderBy);
        }

        public DataTable get_DataTable(string cols, string orderBy)
        {
            return get_DataTable(cols, null, orderBy);
        }

        public virtual DataTable get_DataTable(string cols, string where, string orderBy)
        {
            string sql = "SELECT " + cols + " FROM " + get_table();

            if (!String.IsNullOrEmpty(where))
                sql += " WHERE " + where;

            if (!String.IsNullOrEmpty(orderBy))
                sql += " ORDER BY " + orderBy;

            return query.get_DataTable(sql);
        }

        #region Serialization
        public string SerializeXML()
        {
            return SerializeXML(this);
        }

        public static string SerializeXML(Object _obj)
        {
            XmlSerializer xmlSerializer = new XmlSerializer(_obj.GetType());

            MemoryStream ms = new MemoryStream();
            xmlSerializer.Serialize(ms, _obj);

            ms.Seek(0, SeekOrigin.Begin);
            using (StreamReader sr = new StreamReader(ms))
            {
                return sr.ReadToEnd();
            }
        }

        public static SysObject DeserializeXML(Type objType, string strObj)
        {
            XmlSerializer xmlSerializer = new XmlSerializer(objType);

            MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(strObj));

            return (SysObject)xmlSerializer.Deserialize(ms);
        }

        public string SerializeSOAP()
        {
            return SerializeSOAP(this);
        }

        public static string SerializeSOAP(Object _obj)
        {
            SoapFormatter soapFormat = new SoapFormatter();

            MemoryStream ms = new MemoryStream();
            soapFormat.Serialize(ms, _obj);

            ms.Seek(0, SeekOrigin.Begin);
            using (StreamReader sr = new StreamReader(ms))
            {
                return sr.ReadToEnd();
            }
        }

        public static SysObject DeserializeSOAP(string strObj)
        {
            SoapFormatter soapFormat = new SoapFormatter();

            MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(strObj));

            return (SysObject)soapFormat.Deserialize(ms);
        }
        #endregion

        [OnDeserializing]
        internal void OnDeserializing(StreamingContext context)
        {
            query = new Query();
        }

        [DataObjectMethodAttribute(DataObjectMethodType.Insert, true)]
        public void odsInsert(SysObject obj)
        {
            obj.Insert();
        }

        [NonSerialized, XmlIgnore, SoapIgnore]
        private int _odsSelectCnt = 0;
        [XmlIgnore, SoapIgnore]
        public int odsSelectCnt
        {
            get { return _odsSelectCnt; }
            set { _odsSelectCnt = value; }
        }

        public int odsSelectCount(string sortedBy, int startRowIndex, int maximumRows)
        {
            return odsSelectCnt;
        }

        [DataObjectMethodAttribute(DataObjectMethodType.Select, true)]
        public virtual void odsLoad(int _idEx)
        {

        }

        [DataObjectMethodAttribute(DataObjectMethodType.Select, true)]
        public virtual DataTable odsSelect(string sortedBy, int startRowIndex, int maximumRows)
        {
            return odsSelect(sortedBy, startRowIndex, maximumRows, null);
        }

        public virtual DataTable odsSelect(string sortedBy, int startRowIndex, int maximumRows, string filteredBy)
        {
            string sql = "SELECT * FROM " + get_table();

            if (!String.IsNullOrEmpty(filteredBy))
                sql += " WHERE " + filteredBy;

            if (!String.IsNullOrEmpty(sortedBy))
            {
                query.AddParameter("@sortedBy", sortedBy);
                sql += " ORDER BY " + sortedBy;
            }

            return MountPagination(query.get_DataTable(sql), startRowIndex, maximumRows);
        }

        protected DataTable MountPagination(DataTable dt, int startRowIndex, int maximumRows)
        {
            DataTable result = dt.Clone();
            int dtNumRows = dt.Rows.Count;

            if (maximumRows <= 0)
                maximumRows = dtNumRows;

            for (int i = startRowIndex; i < (startRowIndex + maximumRows); i++)
            {
                if (i < dtNumRows)
                    result.ImportRow(dt.Rows[i]);
                else
                    break;
            }

            odsSelectCnt = dt.Rows.Count;

            return result;
        }

        [DataObjectMethodAttribute(DataObjectMethodType.Update, true)]
        public void odsUpdate(SysObject obj)
        {
            obj.Update();
        }

        [DataObjectMethodAttribute(DataObjectMethodType.Delete, true)]
        public void odsDelete(SysObject obj)
        {
            obj.Delete();
        }
    }
}