using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Serialization;

namespace O2.Includes.SysUsers.Exceptions
{
    public class SysUserGroupExistException : ArgumentException
    {
        public SysUserGroupExistException(string message, Exception innerException) : base(message, innerException) { }

        public SysUserGroupExistException(string message) : base(message) { }

        public SysUserGroupExistException(SerializationInfo info, StreamingContext context) : base(info, context) { }

        public SysUserGroupExistException(SysUserGroup _group) : base(O2.Languages.Errors.SYSUSERGROUP_ALREADY_EXISTS_IN_DB.Replace("{G}", _group.group), "_group") { }

        public SysUserGroupExistException(SysUserGroup _group, Exception innerException) : base(O2.Languages.Errors.SYSUSERGROUP_ALREADY_EXISTS_IN_DB.Replace("{G}", _group.group), "_group", innerException) { }
    }
}
