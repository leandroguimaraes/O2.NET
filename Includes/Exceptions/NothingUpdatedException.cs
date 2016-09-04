using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Serialization;

namespace O2.Includes.Exceptions
{
    public class NothingUpdatedException : Exception
    {
        public NothingUpdatedException(string message, Exception innerException) : base(message, innerException) { }

        public NothingUpdatedException(string message) : base(message) { }

        public NothingUpdatedException(SerializationInfo info, StreamingContext context) : base(info, context) { }

        public NothingUpdatedException() : base(Languages.Errors.NONE_UPDATED) { }
    }
}
