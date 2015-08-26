using System;
using System.Collections.Generic;
using System.Text;

namespace OpenEhr.Paths
{
    public class PathException: System.ApplicationException
    {
        public PathException(string message) : base(message) { }
        public PathException(string message, Exception innerException) : base(message, innerException) { }
    }

    public class PathNotUniqueException : PathException
    {
         public PathNotUniqueException(string message) : base(message) { }
        public PathNotUniqueException(string message, Exception innerException) : base(message, innerException) { }

    }

    public class PathNotExistException : PathException
    {
        public PathNotExistException(string message) : base(message) { }
        public PathNotExistException(string message, Exception innerException) : base(message, innerException) { }

    }

    public class PathUniqueException : PathException
    {
        public PathUniqueException(string message) : base(message) { }
        public PathUniqueException(string message, Exception innerException) : base(message, innerException) { }

    }

    public class InvalidPathException : PathException
    {
        public InvalidPathException(string message) : base(message) { }
        public InvalidPathException(string message, Exception innerException) : base(message, innerException) { }

    }
}
