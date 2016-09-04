using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Serialization;

namespace O2.Includes.SysUsers.Exceptions
{
    public class UnexistentSessionException : ArgumentException
    {
        public UnexistentSessionException(string message, Exception innerException) : base(message, innerException) { }

        public UnexistentSessionException(string message) : base(message) { }

        public UnexistentSessionException(SerializationInfo info, StreamingContext context) : base(info, context) { }

        public UnexistentSessionException() : base(O2.Languages.Errors.SESSION_UNEXISTENT, "session_id") { }

        public UnexistentSessionException(Exception innerException) : base(O2.Languages.Errors.SESSION_UNEXISTENT, "session_id", innerException) { }
    }
}
