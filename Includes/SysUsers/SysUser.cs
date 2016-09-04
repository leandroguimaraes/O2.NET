using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;
using System.Data;
using System.Xml.Serialization;
using System.Configuration;
using O2.Utils;
using System.Reflection;
using O2.Includes.DataBaseAccess;
using System.Data.SqlClient;
using O2.Includes.Exceptions;
using O2.Includes.SysUsers.Exceptions;

namespace O2.Includes.SysUsers
{
    [Serializable]
    public class SysUser : SysObject
    {
        public string table = "sysusers";

        protected SysUserPermissionCollection _permissions;
        public SysUserPermissionCollection permissions
        {
            get
            {
                if (_permissions == null)
                    _permissions = (id > 0) ? SysUserPermission.get_Permissions(this) : new SysUserPermissionCollection();

                return _permissions; 
            }

            set
            {
                _permissions = value;
                if (_permissions != null)
                    _permissions.sysuser_id = id;
            }
        }

        protected SysUserGroupCollection _groups;
        public SysUserGroupCollection groups
        {
            get
            {
                if (_groups == null)
                    _groups = (id > 0) ? SysUserGroup.get_Groups(this) : new SysUserGroupCollection();

                return _groups;
            }

            set
            {
                _groups = value;
                if (_groups != null)
                    _groups.sysuser_id = id;
            }
        }

        protected string _login;
        public string login
        {
            get { return _login; }
            set { _login = value; }
        }

        protected string _password;
        public string password
        {
            get { return _password; }
            set { _password = value; }
        }

        protected int? _culture_id;
        public int? culture_id
        {
            get { return _culture_id; }
            set { _culture_id = value; }
        }

        protected CultureCode _culture;
        public CultureCode culture
        {
            get
            {
                if (culture_id == 0)
                    throw new Exception(O2.Languages.Errors.ID_ZERO_IN_CLASS.Replace("{A}", "culture_id").Replace("{C}", "SysUser.culture"));

                if ((_culture == null) && (culture_id != null))
                {
                    _culture = new CultureCode();
                    _culture.Load(culture_id);
                }

                return _culture;
            }

            set
            {
                _culture = value;
                if (_culture != null)
                    culture_id = _culture.id;
                else
                    culture_id = null;
            }
        }

        protected string _last_url;
        public string last_url
        {
            get { return _last_url; }
            
            set
            { 
                _last_url = value;
                session.last_url = _last_url;
            }
        }

        protected DateTime _last_visit;
        public DateTime last_visit
        {
            get { return _last_visit; }
            
            set
            {
                _last_visit = value;
                session.last_visit = _last_visit;
            }
        }

        protected string _last_ip;
        public string last_ip
        {
            get { return _last_ip; }
            set
            {
                _last_ip = value;
                session.last_ip = value;
            }
        }

        protected DateTime _register_date;
        public DateTime register_date
        {
            get { return _register_date; }
            set { _register_date = value; }
        }

        protected Session _session;
        public Session session
        {
            get
            {
                if (_session == null)
                {
                    _session = new Session();
                    
                    if (id > 0)
                        _session.sysuser_id = id;
                }

                return _session; 
            }

            set
            {
                _session = value;

                if (id > 0)
                    _session.sysuser_id = id;
                else
                    _session.sysuser_id = null;
            }
        }

        public enum STATUS
        {
            Inactive,
            Active
        }

        protected STATUS _status = STATUS.Inactive;
        public STATUS status
        {
            get { return _status; }
            set { _status = value; }
        }

        public SysUser() : base() { }

        public SysUser(string DBConnName) : base(new DBConn(DBConnName)) { }

        public SysUser(DBConn conn) : base(conn) { }

        public override void Insert()
        {
            bool commit_transaction = (query.conn.transaction == null) ? true : false;

            query.conn.BeginTransaction();

            try
            {
                register_date = DateTime.Now;
                last_visit = DateTime.Now;

                base.Insert();

                if (groups.Count > 0)
                {
                    groups.sysuser_id = id;
                    groups.query = query;
                    groups.InsertAll();
                }

                if (permissions.Count > 0)
                {
                    permissions.sysuser_id = id;
                    permissions.query = query;
                    permissions.InsertAll();
                }

                if (commit_transaction)
                    query.conn.TransactionCommit();
            }
            catch (Exception excp)
            {
                if (commit_transaction)
                    query.conn.TransactionRollback();

                id = 0;

                if (excp.Message.Contains("UK_sysusers_login"))
                    throw new ArgumentException(Languages.Errors.LOGIN_EXIST.Replace("{L}", login));
                else
                    throw;
            }
        }

        public override void Load(int? _id)
        {
            base.Load(_id);

            session.sysuser_id = id;
            groups.sysuser_id = id;
            permissions.sysuser_id = id;
        }

        public void LoadBy_sysuser_id(int? _sysuser_id)
        {
            query.AddParameter("@id", _sysuser_id);

            IDataReader reader = query.ExecuteReader("SELECT * FROM " + get_table() + " WHERE id = @id");
            try
            {
                if (reader.Read())
                    LoadBy_array(reader);
                else
                    throw new Exception(O2.Languages.Errors.SELECT_ERROR);

                //sysuser_id = id;
                id = 0;
            }
            finally
            {
                reader.Close();
                reader.Dispose();
            }
        }

        //public SysUserProfile get_profile(string _group_name)
        //{
        //    SysUserGroup group = null;
        //    foreach (SysUserGroup g in groups)
        //        if (g.group == _group_name)
        //        {
        //            group = g;
        //            break;
        //        }

        //    if (group == null)
        //        throw new ArgumentException(O2.Languages.Errors.SYSUSERGROUP_NOT_FOUND_FOR_USER.Replace("{G}", _group_name), "_group_name");

        //    return group.profile;
        //}

        public void CreateLogin(string _login)
        {
            _login = _login.ToLower();

            ValidateLogin(_login);

            login = _login;
        }

        protected void ValidateLogin(string _login)
        {
            if (String.IsNullOrEmpty(_login) || (_login.Length < Convert.ToInt32(ConfigurationManager.AppSettings["USER_LOGIN_MIN"])) || (_login.Length > Convert.ToInt32(ConfigurationManager.AppSettings["USER_LOGIN_MAX"])))
                throw new ArgumentException(O2.Languages.Errors.LOGIN_LENGTH.Replace("{MIN}", ConfigurationManager.AppSettings["USER_LOGIN_MIN"]).Replace("{MAX}", ConfigurationManager.AppSettings["USER_LOGIN_MAX"]), "_login");

            query.AddParameter("@login", _login);

            IDataReader reader = query.ExecuteReader("SELECT login FROM " + get_table() + " WHERE login = @login");

            try
            {
                if (reader.Read())
                {
                    login = _login;
                    throw new LoginExistException(this);
                }
            }
            finally
            {
                reader.Close();
                reader.Dispose();
            }
        }

        public void CreatePassword(string _password1, string _password2, Crypto.TYPE _crypto_type)
        {
            ValidatePassword(_password1, _password2);
            password = Crypto.MD5(_password1);

            switch (_crypto_type)
            {
                case Crypto.TYPE.MD5:
                    password = Crypto.MD5(_password1);
                    break;

                case Crypto.TYPE.SHA1:
                    password = Crypto.SHA1(_password1);
                    break;

                case Crypto.TYPE.SHA256:
                    password = Crypto.SHA256(_password1);
                    break;

                case Crypto.TYPE.SHA512:
                    password = Crypto.SHA512(_password1);
                    break;

                default:
                    throw new NotImplementedException();
            }
        }

        public void CreatePassword(string _password1, string _password2)
        {
            CreatePassword(_password1, _password2, Crypto.TYPE.SHA512);
        }

        protected void ValidatePassword(string _password1, string _password2)
        {
            if (_password1 != _password2)
                throw new DistinctPasswordException();
            
            if ((_password1.Length < Convert.ToInt32(ConfigurationManager.AppSettings["USER_PASSW_MIN"])) || (_password1.Length > Convert.ToInt32(ConfigurationManager.AppSettings["USER_PASSW_MAX"])))
                throw new ArgumentException(Languages.Errors.PASSWORD_LENGTH_ERROR.Replace("{MIN}", ConfigurationManager.AppSettings["USER_PASSW_MIN"]).Replace("{MAX}", ConfigurationManager.AppSettings["USER_PASSW_MAX"]), "_password1");
        }

        public void Login(string _login, string _cryptoPassword)
        {
            _login = _login.ToLower();

            //garbage collection
            query.AddParameter("@date", DateTime.Now);

            query.ExecuteNonQuery("DELETE FROM " + ConfigurationManager.AppSettings["TBPREFIX"] + "sysusers_lost_passwords WHERE date <= @date");

            query.AddParameter("@login", _login);
            query.AddParameter("@password", _cryptoPassword);
            query.AddParameter("@passwordEmpty", String.Empty);

            DataTable dt = query.get_DataTable("SELECT * FROM " + get_table() +
                                                       " WHERE login = @login" +
                                                         " AND password = @password AND password IS NOT NULL AND password <> @passwordEmpty");

            /*
            Boolean logged = false;
            //logged
            if (reader.Read())
                logged = true;
            //not logged
            //check sysusers_lost_passwords table (just for web applications)
            else if (SystemClass.config.is_web_app())
            {
                reader.Close();
                reader.Dispose();

                reader = query.ExecuteReader("SELECT sysuser_id FROM " + ConfigurationManager.AppSettings["TBPREFIX"] + "sysusers_lost_passwords" +
                                                       " WHERE login = @login" +
                                                         " AND new_password = @password", parameters);

                if (reader.Read())
                { 
                    //update SYSUSERS table with new password
                    query.ExecuteNonQuery("UPDATE " + get_table() + " SET password = @password WHERE login = @login", parameters);

                    reader.Close();
                    reader.Dispose();

                    reader = query.ExecuteReader("SELECT id FROM " + get_table() +
                                                       " WHERE login = @login", parameters);

                    reader.Read();
                    logged = true;
                }
            }
            */

            if (dt.Rows.Count == 1)
            {
                LoadBy_array(dt.Rows[0]);
                session.sysuser_id = id;
                session.logged = true;
                permissions = null;
                groups = null;
            }
            else
            {
                session.logged = false;
                throw new Exception(Languages.Errors.SYSTEM_LOGIN_ERROR);
            }
        }

        public void Logout()
        {
            session.logged = false;
        }

        public Boolean is_logged()
        {
            return ((id > 0) && session.logged);
        }

        public void ChangePassword(string oldCryptoPassword, string newPlainPassword1, string newPlainPassword2, Crypto.TYPE _crypto_type)
        {
            if (id <= 0)
                throw new Exception(Languages.Errors.CHANGE_PASSWORD_ERROR_SYSUSER_ID);

            if (password != oldCryptoPassword)
                throw new ArgumentException(Languages.Errors.PRESENT_PASSWORD_ERROR, "oldPassword");

            if (newPlainPassword1 != newPlainPassword2)
                throw new DistinctPasswordException();

            string newCryptoPassword1 = String.Empty;

            switch (_crypto_type)
            {
                case Crypto.TYPE.MD5:
                    newCryptoPassword1 = Crypto.MD5(newPlainPassword1);
                    break;

                case Crypto.TYPE.SHA1:
                    newCryptoPassword1 = Crypto.SHA1(newPlainPassword1);
                    break;

                case Crypto.TYPE.SHA256:
                    newCryptoPassword1 = Crypto.SHA256(newPlainPassword1);
                    break;

                case Crypto.TYPE.SHA512:
                    newCryptoPassword1 = Crypto.SHA512(newPlainPassword1);
                    break;

                default:
                    throw new NotImplementedException();
            }

            if (oldCryptoPassword == newCryptoPassword1)
                throw new NothingUpdatedException();

            password = newCryptoPassword1;

            query.AddParameter("@password", newCryptoPassword1);
            query.AddParameter("@id", id);
            query.ExecuteNonQuery("UPDATE " + get_table() + " SET password = @password WHERE id = @id");
        }

        public void ChangePassword(string oldCryptoPassword, string newPlainPassword1, string newPlainPassword2)
        {
            ChangePassword(oldCryptoPassword, newPlainPassword1, newPlainPassword2, Crypto.TYPE.SHA512);
        }

        public static SysUser MergeUser(SysUser user1, SysUser user2)
        {
            if (user2.id > 0)
                user1.LoadBy_sysuser_id(user2.id);

            user1.login = user2.login;
            user1.password = user2.password;
            user1.culture = user2.culture;
            user1.last_visit = user2.last_visit;
            user1.last_ip = user2.last_ip;
            user1.register_date = user2.register_date;
            user1.status = user2.status;

            user1.session = user2.session;

            return user1;
        }

        public bool BelongsToGroup(SysUserGroup _group)
        {
            return groups.Contains(_group);
        }

        public bool BelongsToGroup(string _group_name)
        {
            foreach (SysUserGroup _group in groups)
                if (_group.group == _group_name)
                    return true;

            return false;
        }
    }
}
