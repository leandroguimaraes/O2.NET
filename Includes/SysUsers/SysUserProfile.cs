using System;
using System.Collections.Generic;
using System.Text;
using O2.Includes.DataBaseAccess;
using System.Data;
using O2.Includes.SysUsers.Exceptions;
using O2.Includes.Exceptions;

namespace O2.Includes.SysUsers
{
    public abstract class SysUserProfile : SysObject
    {
        public SysUserProfile() : base() { }

        public SysUserProfile(string DBConnName) : base(DBConnName) { }

        public SysUserProfile(DBConn conn) : base(conn) { }

        public void LoadBy_sysuser(SysUser user)
        {
            try
            {
                query.AddParameter("@sysuser_id", user.id);
                DataTable dt = query.get_DataTable("SELECT * FROM " + get_table() + " WHERE sysuser_id = @sysuser_id");
                if (dt.Rows.Count == 1)
                    LoadBy_array(dt.Rows[0]);
                else
                    throw new SysUserProfileNotFoundException(this, user);
            }
            catch (ClassWithoutTableFieldException) { }
        }
    }
}
