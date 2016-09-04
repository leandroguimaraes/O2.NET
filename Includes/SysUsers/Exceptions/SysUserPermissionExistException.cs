using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Serialization;

namespace O2.Includes.SysUsers.Exceptions
{
    public class SysUserPermissionExistException : ArgumentException
    {
        public SysUserPermissionExistException(string message, Exception innerException) : base(message, innerException) { }

        public SysUserPermissionExistException(string message) : base(message) { }

        public SysUserPermissionExistException(SerializationInfo info, StreamingContext context) : base(info, context) { }

        public SysUserPermissionExistException(SysUserPermission _permission) : base(O2.Languages.Errors.SYSUSERPERMISSION_ALREADY_EXISTS.Replace("{K}", _permission.key), "_key") { }

        public SysUserPermissionExistException(SysUserPermission _permission, Exception innerException) : base(O2.Languages.Errors.SYSUSERPERMISSION_ALREADY_EXISTS.Replace("{K}", _permission.key), "_key", innerException) { }
    }
}
