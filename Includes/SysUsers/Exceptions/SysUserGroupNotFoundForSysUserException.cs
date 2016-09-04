using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Serialization;

namespace O2.Includes.SysUsers.Exceptions
{
    public class SysUserGroupNotFoundForSysUserException : ArgumentException
    {
        public SysUserGroupNotFoundForSysUserException(SerializationInfo info, StreamingContext context) : base(info, context) { }

        public SysUserGroupNotFoundForSysUserException(string _group_name) : base(O2.Languages.Errors.SYSUSERGROUP_NOT_FOUND_FOR_USER.Replace("{G}", _group_name), "_group_name") { }

        public SysUserGroupNotFoundForSysUserException(string _group_name, Exception innerException) : base(O2.Languages.Errors.SYSUSERGROUP_NOT_FOUND_FOR_USER.Replace("{G}", _group_name), "_group_name", innerException) { }
    }
}
