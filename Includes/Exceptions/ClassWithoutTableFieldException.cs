using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Serialization;

namespace O2.Includes.Exceptions
{
    public class ClassWithoutTableFieldException : Exception
    {
        public ClassWithoutTableFieldException(string message, Exception innerException) : base(message, innerException) { }

        public ClassWithoutTableFieldException(string message) : base(message) { }

        public ClassWithoutTableFieldException(SerializationInfo info, StreamingContext context) : base(info, context) { }

        public ClassWithoutTableFieldException(Type type) : base(Languages.Errors.CLASS_WITHOUT_TABLE_FIELD.Replace("{C}", type.FullName)) { }
    }
}
