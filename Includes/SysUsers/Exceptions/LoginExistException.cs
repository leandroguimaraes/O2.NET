using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Serialization;

namespace O2.Includes.SysUsers.Exceptions
{
    public class LoginExistException : ArgumentException
    {
        public LoginExistException(string message, Exception innerException) : base(message, innerException) { }

        public LoginExistException(string message) : base(message) { }

        public LoginExistException(SerializationInfo info, StreamingContext context) : base(info, context) { }

        public LoginExistException(SysUser _sysuser) : base(O2.Languages.Errors.LOGIN_EXIST.Replace("{L}", _sysuser.login), "_login") { }

        public LoginExistException(SysUser _sysuser, Exception innerException) : base(O2.Languages.Errors.LOGIN_EXIST.Replace("{L}", _sysuser.login), "_login", innerException) { }
    }
}
