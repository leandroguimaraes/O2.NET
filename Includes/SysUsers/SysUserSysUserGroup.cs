using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using O2.Includes.DataBaseAccess;

namespace O2.Includes.SysUsers
{
    [Serializable]
    public class SysUserSysUserGroup : SysObject
    {
        public string table = "sysusers_sysusers_groups";

        protected int? _sysuser_id;
        public int? sysuser_id
        {
            get { return _sysuser_id; }
            set { _sysuser_id = value; }
        }

        protected int? _sysuser_group_id;
        public int? sysuser_group_id
        {
            get { return _sysuser_group_id; }
            set { _sysuser_group_id = value; }
        }

        public SysUserSysUserGroup() : base() { }

        public SysUserSysUserGroup(string DBConnName) : base(DBConnName) { }

        public SysUserSysUserGroup(DBConn conn) : base(conn) { }

        public void LoadBy_sysuser_id_sysuser_group_id(int? _sysuser_id, int? _sysuser_group_id)
        {
            query.AddParameter("@sysuser_id", _sysuser_id);
            query.AddParameter("@sysuser_group_id", _sysuser_group_id);

            DataTable dt = query.get_DataTable("SELECT * FROM " + get_table() + " WHERE sysuser_id = @sysuser_id AND sysuser_group_id = @sysuser_group_id");

            if (dt.Rows.Count == 0)
                throw new Exception(O2.Languages.Errors.SELECT_ERROR);

            LoadBy_array(dt.Rows[0]);
        }
    }
}
