using System;
using System.Data;

namespace O2.Includes.DataBaseAccess
{
    [Serializable]
    public class TableMetaData
    {
        protected string _column_name;
        public string column_name
        {
            get { return _column_name; }
            set { _column_name = value; }
        }

        protected bool _is_nullable;
        public bool is_nullable
        {
            get { return _is_nullable; }
            set { _is_nullable = value; }
        }

        protected string _data_type;
        public string data_type
        {
            get { return _data_type; }
            set { _data_type = value; }
        }

        protected int? _character_maximum_length;
        public int? character_maximum_length
        {
            get { return _character_maximum_length; }
            set { _character_maximum_length = value; }
        }

        public TableMetaData(DataRow row)
        {
            column_name = row["COLUMN_NAME"].ToString();
            is_nullable = (row["IS_NULLABLE"].ToString().ToUpper() == "NO") ? false : true;

            try
            {
                data_type = row["DATA_TYPE"].ToString();
            }
            catch (ArgumentException)
            {
                data_type = null;
            } // ToDo: ignora DATA_TYPE para bancos de dados que não possuam esse tratamento implementado

            try
            {
                character_maximum_length = Convert.ToInt32(row["CHARACTER_MAXIMUM_LENGTH"]);
            }
            catch (InvalidCastException)
            {
                character_maximum_length = null;
            } // ToDo: ignora CHARACTER_MAXIMUM_LENGTH para bancos de dados que não possuam esse tratamento implementado
            catch (ArgumentException)
            {
                character_maximum_length = null;
            } // ToDo: ignora CHARACTER_MAXIMUM_LENGTH para bancos de dados que não possuam esse tratamento implementado
        }
    }
}