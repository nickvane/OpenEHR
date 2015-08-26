using System;

namespace OpenEhr.Serialisation
{
    public class InvalidXmlException : System.ApplicationException
    {
        public InvalidXmlException(string message) : base(message) { }
        public InvalidXmlException(string message, Exception innerException) : base(message, innerException) { }
        public InvalidXmlException(string expectedElementName, string elementName) : 
            base("Expected element name is '"+ expectedElementName +"', but it is "+elementName) { }
    }
}