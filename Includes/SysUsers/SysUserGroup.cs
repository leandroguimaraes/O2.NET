using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Configuration;
using O2.Includes.SysUsers.Exceptions;
using O2.Includes.DataBaseAccess;
using System.Xml.Serialization;

namespace O2.Includes.SysUsers
{
    [Serializable]
    public class SysUserGroup : SysObject
    {
        public string table = "sysusers_groups";

        protected string _group;
        public string group
        {
            get { return _group; }
            set { _group = value; }
        }

        protected int? _subgroup_id;
        public int? subgroup_id
        {
            get { return _subgroup_id; }
            set { _subgroup_id = value; }
        }

        protected SysUserProfile _profile;
        public SysUserProfile profile
        {
            get { return _profile; }
            set { _profile = value; }
        }

        public SysUserGroup() : base() { }

        public SysUserGroup(string DBConnName) : base(DBConnName) { }

        public SysUserGroup(DBConn conn) : base(conn) { }

        public override void Insert()
        {
            try
            {
                base.Insert();
            }
            catch (Exception ex)
            {
                if (ex.Message.ToUpper().Contains("UK_SYSUSERS_GROUPS_GROUP"))
                    throw new SysUserGroupExistException(this, ex);
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
                if (ex.Message.ToUpper().Contains("UK_SYSUSERS_GROUPS_GROUP"))
                    throw new SysUserGroupExistException(this, ex);
            }
        }

        public override void Load(int? _id)
        {
            try
            {
                base.Load(_id);
            }
            catch (Exception excp)
            {
                if (excp.Message == O2.Languages.Errors.SELECT_ERROR)
                    throw new Exception(O2.Languages.Errors.INVALID_USER_GROUP.Replace("{G}", "id = " + _id.ToString()));
                else
                    throw;
            }
        }

        public void LoadBy_group(string _group)
        {
            query.AddParameter("@group", _group);

            string sql = "SELECT * FROM " + get_table() + " WHERE ";
            switch (query.conn.providerName)
            {
                case "System.Data.SqlClient":
                    sql += "[group] = @group";
                    break;

                case "MySql.Data.MySqlClient":
                    sql += "`group` = @group";
                    break;

                default:
                    sql += "\"group\" = @group";
                    break;
            }

            IDataReader reader = query.ExecuteReader(sql);

            try
            {
                if (reader.Read())
                    LoadBy_array(reader);
                else
                    throw new Exception(Languages.Errors.INVALID_USER_GROUP.Replace("{G}", _group));
            }
            finally
            {
                reader.Close();
                reader.Dispose();
            }
        }

        public static SysUserGroupCollection get_Groups(SysUser _user)
        {
            SysUserGroup grp = new SysUserGroup();
            SysUserSysUserGroup sugrp = new SysUserSysUserGroup();
            SysUserGroupCollection result = new SysUserGroupCollection();

            string sql = "SELECT grp.* FROM " + grp.get_table() + " AS grp";
            sql += "   INNER JOIN " + sugrp.get_table() + " AS sugrp";
            sql += "     ON grp.id = sugrp.sysuser_group_id";
            sql += " WHERE sugrp.sysuser_id = @sysuser_id";

            grp.query.AddParameter("@sysuser_id", _user.id);
            DataTable dt = grp.query.get_DataTable(sql);
            foreach (DataRow row in dt.Rows)
            {
                grp = new SysUserGroup();
                grp.LoadBy_array(row);

                result.Add(grp);
            }

            return result;
        }
    }
}