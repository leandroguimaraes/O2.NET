using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Collections;
using System.Threading;
using System.Globalization;
using O2.Includes.SysUsers;

namespace O2.Includes
{
    public class CultureCode : SysObject
    {
        public string table = "cultures";

        protected string _culture;
        public string culture
        {
            get { return _culture; }
            set { _culture = value; }
        }

        protected string _code;
        public string code
        {
            get { return _code; }
            set { _code = value; }
        }

        public void LoadBy_code(string _code)
        {
            query.AddParameter("@code", _code);

            IDataReader reader = query.ExecuteReader("SELECT * FROM " + get_table() + " WHERE code = @code");
            try
            {
                if (reader.Read())
                    LoadBy_array(reader);
                else
                    throw new Exception(O2.Languages.Errors.CULTURE_NOT_FOUND.Replace("{C}", _code));
            }
            finally
            {
                reader.Close();
                reader.Dispose();
            }
        }

        public static DataTable get_cultures()
        {
            CultureCode culture = new CultureCode();
            return culture.get_DataTable("culture");
        }

        public static void UpdateCulture(SysUser user)
        {
            UpdateCulture(user, Thread.CurrentThread.CurrentCulture.ToString());
        }

        public static void UpdateCulture(SysUser user, string culture)
        {
            // ToDo:
            //if (!String.IsNullOrEmpty(culture))
            //{
            //    Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture(culture);
            //    Thread.CurrentThread.CurrentUICulture = new CultureInfo(culture);

            //    user.culture = culture;
            //}
        }
    }
}
