using System;
using System.Collections.Generic;
using OpenEhr.DesignByContract;
using System.Xml.XPath;
using OpenEhr.RM.DataTypes.Text;
using OpenEhr.Utilities.PathHelper;

namespace OpenEhr.RM.Support.Terminology.Impl.Data.ref_impl_java
{
    internal sealed class CodesetData
    {
        private readonly Lazy<XPathDocument> _termDocument ;

        public CodesetData()
        {
            _termDocument = new Lazy<XPathDocument>(delegate()
            {
                return TerminologyDocument.Value;
            });
        }
        public CodesetData(string xmlFilePath)
        {
            Check.Require(!string.IsNullOrEmpty(xmlFilePath), "xmlFilePath must not be null or empty.");
            _termDocument = new Lazy<XPathDocument>(delegate()
            {
                return new XPathDocument(PathHelper.AbsolutePath(xmlFilePath));
            });
        }

        public List<CodePhrase> GetAllCodes(string name)
        {
            var results = new List<CodePhrase>();
            try
            {
                var navigator = _termDocument.Value.CreateNavigator();
                foreach (XPathNavigator codeNav in navigator.Select("/terminology/codeset[@external_id='" + name + "']/*"))
                {
                    var selectSingleNode = codeNav.SelectSingleNode("@value");
                    if (selectSingleNode == null) continue;
                    var code = selectSingleNode.Value;

                    var codePhrase = new CodePhrase(code, name);

                    results.Add(codePhrase);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(string.Format("Could not read codes for {0} \n {1}", name, ex.Message));
            }

            return results;
        }

        public Dictionary<string, string> GetCodeDescriptions(string name)
        {
            var results = new Dictionary<string, string>();

            try
            {
                var navigator = _termDocument.Value.CreateNavigator();
                foreach (XPathNavigator codeNav in navigator.Select("/terminology/codeset[@external_id='" + name + "']/*"))
                {
                    var selectSingleNode = codeNav.SelectSingleNode("@value");
                    if (selectSingleNode == null) continue;
                    var code = selectSingleNode.Value;
                    var xPathNavigator = codeNav.SelectSingleNode("@Description");
                    if (xPathNavigator == null) continue;
                    var description = xPathNavigator.Value;

                    results.Add(code, description);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(string.Format("Could not read codes for {0} \n {1}", name, ex.Message));
            }

            return results;
        }

        public string GetCodesetOpenEhrId(string name)
        {
            var result = string.Empty;

            try
            {
                var navigator = _termDocument.Value.CreateNavigator();
                var selectSingleNode = navigator.SelectSingleNode("/terminology/codeset[@external_id='" + name + "']/@openehr_id");
                if (selectSingleNode != null) result = selectSingleNode.Value;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(string.Format("Could not read OpenEhrId for {0} \n {1}", name, ex.Message));
            }

            return result;
        }

        public bool HasLanguage(string codeString)
        {
            var result = false;

            try
            {
                var navigator = _termDocument.Value.CreateNavigator();
                var selectSingleNode = navigator.SelectSingleNode("/terminology/@language");
                if (selectSingleNode != null)
                {
                    var lanuage = selectSingleNode.Value;
                    result = lanuage == codeString;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(string.Format("Error reading lanuage {0}", ex.Message));
            }
            return result;
        }

        public bool HasCode(string name, string code)
        {
            XPathNavigator result = null;

            try
            {
                var navigator = _termDocument.Value.CreateNavigator();
                result = navigator.SelectSingleNode("/terminology/codeset[@external_id='" + name + "']/code[@value='" + code + "']");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(string.Format("Error reading code {0}", ex.Message));
            }

            return result != null;
        }
    }
}
