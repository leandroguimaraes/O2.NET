using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Serialization;

namespace O2.Includes.SysUsers.Exceptions
{
    public class DistinctPasswordException : Exception
    {
        public DistinctPasswordException(string message, Exception innerException) : base(message, innerException) { }

        public DistinctPasswordException(string message) : base(message) { }

        public DistinctPasswordException(SerializationInfo info, StreamingContext context) : base(info, context) { }

        public DistinctPasswordException() : base(Languages.Errors.DISTINCT_PASSWORD) { }
    }
}
