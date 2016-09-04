using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Configuration;
using System.Reflection;
using O2.Includes.DataBaseAccess;
using O2.Includes.SysUsers.Exceptions;

namespace O2.Includes.SysUsers
{
    [Serializable]
    public class Session : SysObject
    {
        public string table = "sessions";

        protected string _session_id;
        public string session_id
        {
            get { return _session_id; }
            set { _session_id = value; }
        }

        protected string _culture;
        public string culture
        {
            get { return _culture; }
            set { _culture = value; }
        }

        protected string _last_url;
        public string last_url
        {
            get { return _last_url; }
            set { _last_url = value; }
        }
        
        protected DateTime _last_visit;
        public DateTime last_visit
        {
            get { return _last_visit; }
            set { _last_visit = value; }
        }

        protected string _last_ip;
        public string last_ip
        {
            get { return _last_ip; }
            set { _last_ip = value; }
        }

        protected int? _sysuser_id;
        public int? sysuser_id
        {
            get { return _sysuser_id; }
            set { _sysuser_id = value; }
        }

        protected bool _logged;
        public bool logged
        {
            get { return _logged; }
            set { _logged = value; }
        }

        public override void Update()
        {
            if (id > 0)
                base.Update();
            else
                Insert();
        }

        public override void Insert()
        {
            if (id > 0)
                Update();
            else
                base.Insert();
        }

        public void set_UserInfo(SysUser _user)
        {
            sysuser_id = _user.id;
            last_visit = DateTime.Now;
        }

        public static void GarbageCollection()
        {
            //clear expired sessions
            Session session = new Session();
            session.query.AddParameter("@dateTime", DateTime.Now.AddSeconds(-Convert.ToInt32(ConfigurationManager.AppSettings["SESSION_TIMEOUT"])));

            session.query.ExecuteNonQuery("DELETE FROM " + session.get_table() + " WHERE last_visit < @dateTime");
        }
    }
}
