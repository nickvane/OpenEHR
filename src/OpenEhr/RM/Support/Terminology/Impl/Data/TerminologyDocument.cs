using System;
using System.Xml.XPath;
using System.IO;
using System.Reflection;
using OpenEhr.Utilities;

namespace OpenEhr.RM.Support.Terminology.Impl.Data
{
    internal static class TerminologyDocument
    {
        const string resourcePath = @"OpenEhr.Support.Terminology.Realisation.TerminologyData.openehr_terminology_en.xml";

        static Lazy<XPathDocument> lazyTerminologyDocument = new Lazy<XPathDocument>(delegate()
            {
                using (Stream stream =
                    Assembly.GetExecutingAssembly().GetManifestResourceStream(resourcePath))
                    return new XPathDocument(stream);
            });

        public static XPathDocument Value
        {
            get { return lazyTerminologyDocument.Value; }
        }
    }
}
