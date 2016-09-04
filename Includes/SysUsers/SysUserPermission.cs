using System;
using System.Collections.Generic;
using System.Text;
using System.Configuration;
using O2.Includes.SysUsers.Exceptions;
using O2.Includes.DataBaseAccess;
using System.Data;

namespace O2.Includes.SysUsers
{
    [Serializable]
    public class SysUserPermission : SysObject
    {
        public string table = "sysusers_permissions";

        protected int? _sysuser_id;
        public int? sysuser_id
        {
            get { return _sysuser_id; }
            set { _sysuser_id = value; }
        }

        protected int? _module_id;
        public int? module_id
        {
            get { return _module_id; }
            set { _module_id = value; }
        }

        protected SysModule _module;
        public SysModule module
        {
            get
            {
                if (module_id == 0)
                    throw new Exception(Languages.Errors.ID_ZERO_IN_CLASS.Replace("{A}", "module_id").Replace("{C}", "SysUserPermission.module"));

                if (_module == null)
                {
                    _module = new SysModule();
                    _module.Load(module_id);
                }

                return _module;
            }

            set { _module = value; }
        }

        protected string _key;
        public string key
        {
            get { return _key; }
            set { _key = value; }
        }

        protected string _value;
        public string value
        {
            get { return _value; }
            set { _value = value; }
        }

        public SysUserPermission() : base() { }

        public SysUserPermission(string DBConnName) : base(DBConnName) { }

        public SysUserPermission(DBConn conn) : base(conn) { }

        public override void Insert()
        {
            try
            {
                base.Insert();
            }
            catch (Exception ex)
            {
                if (ex.Message.ToUpper().Contains("FK_SYSUSERS_PERMISSIONS_MODULES"))
                    throw new Exception(Languages.Errors.SYSUSER_PERMISSIONS_FK_MODULE_ID.Replace("{ID}", module_id.ToString()));
                else if (ex.Message.ToUpper().Contains("UK_SYSUSERS_PERMISSIONS_SYSUSER_MODULE_KEY"))
                    throw new SysUserPermissionExistException(this, ex);
                else
                    throw new Exception(Languages.Errors.ERROR_AT.Replace("{L}", "SysUserPermission.Insert()").Replace("{E}", ex.Message));
            }
        }

        public override void Update()
        {
            try
            {
                base.Update();
            }
            catch (Exception ex)
            {
                if (ex.Message.ToUpper().Contains("FK_SYSUSERS_PERMISSIONS_MODULES"))
                    throw new Exception(Languages.Errors.SYSUSER_PERMISSIONS_FK_MODULE_ID.Replace("{ID}", module_id.ToString()));
                else if (ex.Message.ToUpper().Contains("UK_SYSUSERS_PERMISSIONS_SYSUSER_MODULE_KEY"))
                    throw new SysUserPermissionExistException(this, ex);
                else
                    throw new Exception(Languages.Errors.ERROR_AT.Replace("{L}", "SysUserPermission.Insert()").Replace("{E}", ex.Message));
            }
        }

        public static SysUserPermissionCollection get_Permissions(SysUser _user)
        {
            SysUserPermission obj = new SysUserPermission();
            SysUserPermissionCollection result = new SysUserPermissionCollection();

            obj.query.AddParameter("@sysuser_id", _user.id);
            DataTable dt = obj.query.get_DataTable("SELECT * FROM " + obj.get_table() + " WHERE sysuser_id = @sysuser_id");
            foreach (DataRow row in dt.Rows)
            {
                obj = new SysUserPermission();
                obj.LoadBy_array(row);

                result.Add(obj);
            }

            return result;
        }
    }
}
