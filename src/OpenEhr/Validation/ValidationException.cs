using System;

namespace OpenEhr.Validation
{
    public class ValidationException: ApplicationException
    {
        public ValidationException(string message) : base(message) { }
        public ValidationException(string message, Exception innerException) : base(message, innerException) { }
    }

    public class RmInvariantException : ValidationException
    {
         public RmInvariantException(string message) : base(message) { }
        public RmInvariantException(string message, Exception innerException) : base(message, innerException) { }

    }
}