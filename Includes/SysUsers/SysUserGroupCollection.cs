using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
using O2.Includes.DataBaseAccess;
using O2.Includes.SysUsers.Exceptions;
using System.Data;

namespace O2.Includes.SysUsers
{
    [Serializable, XmlInclude(typeof(SysUserGroup)), SoapInclude(typeof(SysUserGroup))]
    public class SysUserGroupCollection : SysCollection<SysUserGroup>
    {
        protected int? _sysuser_id;
        public int? sysuser_id
        {
            get { return _sysuser_id; }
            set { _sysuser_id = value; }
        }

        public SysUserGroup get_group(string _group_name)
        {
            foreach (SysUserGroup g in this)
                if (g.group == _group_name)
                    return g;

            throw new SysUserGroupNotFoundForSysUserException(_group_name);
        }

        public override void Add(SysUserGroup item)
        {
            foreach (SysUserGroup g in this)
                if (item.id == g.id)
                    throw new Exception(O2.Languages.Errors.SYSUSERGROUP_ALREADY_EXISTS.Replace("{G}", item.group));

            if (sysuser_id > 0)
            {
                SysUserSysUserGroup obj = new SysUserSysUserGroup();

                if (query != null)
                    obj.query = query;

                obj.sysuser_id = sysuser_id;
                obj.sysuser_group_id = item.id;

                obj.Insert();
            }

            innerList.Add(item);
        }

        public override void InsertAll()
        {
            SysUserSysUserGroup sysusers_sysusers_group = new SysUserSysUserGroup();

            string sysuser_group_ids = String.Empty;
            foreach (SysUserGroup g in this)
                sysuser_group_ids += "," + g.id;

            StringBuilder sql = new StringBuilder();
            sql.Append("DELETE FROM " + sysusers_sysusers_group.get_table());
            sql.Append(" WHERE sysuser_id = @sysuser_id");

            if (!String.IsNullOrEmpty(sysuser_group_ids))
                sql.Append(" AND sysuser_group_id NOT IN (" + sysuser_group_ids.Substring(1) + ")");

            query.AddParameter("@sysuser_id", sysuser_id);
            query.ExecuteNonQuery(sql.ToString());

            query.AddParameter("@sysuser_id", sysuser_id);
            DataTable dt = query.get_DataTable("SELECT * FROM " + sysusers_sysusers_group.get_table() + " WHERE sysuser_id = @sysuser_id");

            foreach (SysUserGroup g in this)
            {
                bool exists_in_db = false;
                foreach (DataRow row in dt.Rows)
                    if (g.id == Convert.ToInt32(row["sysuser_group_id"]))
                    {
                        exists_in_db = true;
                        break;
                    }

                if (!exists_in_db)
                {
                    SysUserSysUserGroup obj = new SysUserSysUserGroup();

                    if (query != null)
                        obj.query = query;

                    obj.sysuser_id = sysuser_id;
                    obj.sysuser_group_id = g.id;

                    obj.Insert();
                }
            }
        }

        public override void Insert(int index, SysUserGroup item)
        {
            if (sysuser_id > 0)
            {
                SysUserSysUserGroup obj = new SysUserSysUserGroup();

                if (query != null)
                    obj.query = query;

                obj.sysuser_id = sysuser_id;
                obj.sysuser_group_id = item.id;

                obj.Insert();
            }

            innerList.Insert(index, item);
        }

        public override void RemoveAt(int index)
        {
            if (sysuser_id > 0)
            {
                SysUserSysUserGroup obj = new SysUserSysUserGroup();

                if (query != null)
                    obj.query = query;

                obj.LoadBy_sysuser_id_sysuser_group_id(sysuser_id, innerList[index].id);
                obj.Delete();
            }

            innerList.RemoveAt(index);
        }

        public override bool Remove(SysUserGroup item)
        {
            bool result = innerList.Remove(item);

            if (sysuser_id > 0)
            {
                SysUserSysUserGroup obj = new SysUserSysUserGroup();

                if (query != null)
                    obj.query = query;

                obj.LoadBy_sysuser_id_sysuser_group_id(sysuser_id, item.id);
                obj.Delete();
            }

            return result;
        }
    }
}
