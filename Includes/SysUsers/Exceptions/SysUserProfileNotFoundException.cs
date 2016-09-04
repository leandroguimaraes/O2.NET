using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Serialization;

namespace O2.Includes.SysUsers.Exceptions
{
    public class SysUserProfileNotFoundException : Exception
    {
        public SysUserProfileNotFoundException(string message, Exception innerException) : base(message, innerException) { }

        public SysUserProfileNotFoundException(string message) : base(message) { }

        public SysUserProfileNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context) { }

        public SysUserProfileNotFoundException(SysUserProfile profile, SysUser user) : base(Languages.Errors.SYSUSERPROFILE_NOT_FOUND.Replace("{P}", profile.GetType().ToString()).Replace("{ID}", user.id.ToString())) { }
    }
}
