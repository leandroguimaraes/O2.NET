using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace O2.Includes
{
    public class SysModule : SysObject
    {
        public string table = "modules";

        protected string _module;
        public string module
        {
            get { return _module; }
            set { _module = value; }
        }

        public void LoadBy_module(string module)
        {
            query.AddParameter("@module", module);
            DataTable dt = query.get_DataTable("SELECT * FROM " + get_table() + " WHERE module = @module");
            if (dt.Rows.Count == 1)
                LoadBy_array(dt.Rows[0]);
            else
                throw new Exception("Módulo inexistente: " + module);
        }
    }
}
