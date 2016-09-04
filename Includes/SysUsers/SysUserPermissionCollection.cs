using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
using O2.Includes.DataBaseAccess;
using O2.Includes.SysUsers.Exceptions;

namespace O2.Includes.SysUsers
{
    [Serializable, XmlInclude(typeof(SysUserPermission)), SoapInclude(typeof(SysUserPermission))]
    public class SysUserPermissionCollection : SysCollection<SysUserPermission>
    {
        public string fk_column_name = "sysuser_id";

        protected int? _sysuser_id;
        public int? sysuser_id
        {
            get { return _sysuser_id; }
            set { _sysuser_id = value; }
        }

        public override void Add(SysUserPermission _permission)
        {
            bool updated = false;
            foreach (SysUserPermission p in this)
                if ((_permission.key == p.key) && (_permission.module_id == p.module_id))
                {
                    p.value = _permission.value;
                    if (p.id > 0)
                        p.Update();

                    updated = true;
                    break;
                }

            if (!updated)
            {
                if (sysuser_id > 0)
                {
                    _permission.sysuser_id = sysuser_id;
                    base.Add(_permission);
                }
                else
                    innerList.Add(_permission);
            }
        }

        public override void InsertAll()
        {
            SysUserPermission permission = new SysUserPermission();

            string permission_ids = String.Empty;
            foreach (SysUserPermission item in this)
                if (item.id > 0)
                    permission_ids += "," + item.id;

            StringBuilder sql = new StringBuilder();
            sql.Append("DELETE FROM " + permission.get_table());
            sql.Append(" WHERE sysuser_id = @sysuser_id");

            if (!String.IsNullOrEmpty(permission_ids))
                sql.Append(" AND id NOT IN (" + permission_ids.Substring(1) + ")");

            query.AddParameter("@sysuser_id", sysuser_id);
            query.ExecuteNonQuery(sql.ToString());

            foreach (SysUserPermission item in this)
            {
                item.query = query;
                item.sysuser_id = sysuser_id;

                if (item.id > 0)
                    item.Update();
                else
                    item.Insert();
            }
        }

        public override void Insert(int index, SysUserPermission item)
        {
            if (sysuser_id > 0)
                item.sysuser_id = sysuser_id;

            base.Insert(index, item);
        }

        public string get_PermissionValue(int module_id, string _key)
        {
            foreach (SysUserPermission p in this)
                if (p.key == _key)
                    return p.value;

            throw new Exception(O2.Languages.Errors.SYSUSERPERMISSION_NOT_FOUND.Replace("{K}", _key));
        }

        public void Update(SysUserPermission permission)
        {
            Update(permission.module_id.Value, permission.key, permission.value);
        }

        public void Update(int module_id, string key, string value)
        {
            bool updated = false;
            foreach (SysUserPermission item in this)
                if ((item.module_id == module_id) && (item.key == key))
                {
                    item.value = value;
                    item.Update();
                    updated = true;
                    break;
                }

            if (!updated)
            {
                if ((sysuser_id == 0) || (sysuser_id == null))
                    throw new Exception("sysuser_id = null");

                SysUserPermission permission = new SysUserPermission();
                permission.sysuser_id = sysuser_id;
                permission.module_id = module_id;
                permission.key = key;
                permission.value = value;
                permission.Insert();
            }
        }
    }
}
