using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace O2.Includes.Exceptions
{
    public class CollectionWithoutFKColumnNameFieldException : Exception
    {
        public CollectionWithoutFKColumnNameFieldException(string message, Exception innerException) : base(message, innerException) { }

        public CollectionWithoutFKColumnNameFieldException(string message) : base(message) { }

        public CollectionWithoutFKColumnNameFieldException(SerializationInfo info, StreamingContext context) : base(info, context) { }

        public CollectionWithoutFKColumnNameFieldException(Type type) : base(Languages.Errors.COLLECTION_WITHOUT_FK_COLUMN_NAME_FIELD.Replace("{C}", type.FullName)) { }
    }
}
