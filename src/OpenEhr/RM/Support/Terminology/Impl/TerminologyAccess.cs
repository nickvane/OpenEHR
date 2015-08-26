using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.XPath;
using OpenEhr.RM.Support.Terminology.Impl.Data;
using OpenEhr.RM.DataTypes.Text;

namespace OpenEhr.RM.Support.Terminology.Impl
{
    /// <summary>
    /// 
    /// </summary>
    public class TerminologyAccess : ITerminologyAccessProvider
    {
        readonly string _defaultLanguage = System.Globalization.CultureInfo.CurrentCulture.TwoLetterISOLanguageName;
        
        readonly Lazy<XPathDocument> _terminologyDoc = null;
        
        /// <summary>
        /// 
        /// </summary>
        public TerminologyAccess() : this(null,null) { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="defaultLanguage"></param>
        public TerminologyAccess(string defaultLanguage) : this(null, defaultLanguage) { }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="xmlFilePath"></param>
        /// <param name="defaultLanguage"></param>
        public TerminologyAccess(string xmlFilePath, string defaultLanguage)
        {
            if (!string.IsNullOrEmpty(xmlFilePath))
            {
                _terminologyDoc = new Lazy<XPathDocument>(delegate()
                    {
                        return new XPathDocument(xmlFilePath);
                    });
            }

            if (!string.IsNullOrEmpty(defaultLanguage))
                this._defaultLanguage = defaultLanguage;
        }

        private XPathDocument TerminologyDoc
        {
            get 
            {
                return _terminologyDoc != null ? _terminologyDoc.Value : TerminologyDocument.Value;
            }
        }

        #region ITerminologyAccess Members

        /// <summary>
        /// 
        /// </summary>
        public string Id
        {
            get { return OpenEhrTerminologyIdentifiers.TerminologyIdOpenehr; }
        }

        /// <summary>
        /// 
        /// </summary>
        public List<CodePhrase> AllCodes
        {
            get 
            {
                var results = new List<CodePhrase>();
                try
                {
                    var navigator = TerminologyDoc.CreateNavigator();
                    var expression = navigator.Compile("/terminology/group/concept/@id");
                    expression.AddSort("id", XmlSortOrder.Ascending,XmlCaseOrder.None,"",XmlDataType.Number);

                    results.AddRange(from XPathNavigator idNav in navigator.Select(expression) select new CodePhrase(idNav.Value, OpenEhrTerminologyIdentifiers.TerminologyIdOpenehr));
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Trace.WriteLine(
                    string.Format("Could not read codes {0}", ex.Message));
                }
                return results;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="groupId"></param>
        /// <returns></returns>
        public List<CodePhrase> CodesForGroupId(string groupId)
        {
            var results = new List<CodePhrase>();
            try
            {
                var navigator = TerminologyDoc.CreateNavigator();
                var expression = navigator.Compile("/terminology/group[@name='"+ groupId +"']/concept/@id");
                expression.AddSort("id", XmlSortOrder.Ascending,XmlCaseOrder.None,"",XmlDataType.Number);

                results.AddRange(from XPathNavigator idNav in navigator.Select(expression) select new CodePhrase(idNav.Value, OpenEhrTerminologyIdentifiers.TerminologyIdOpenehr));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(
                string.Format("Could not read codes {0}", ex.Message));
            }
            return results;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="groupId"></param>
        /// <param name="code"></param>
        /// <returns></returns>
        public bool HasCodeForGroupId(string groupId, CodePhrase code)
        {
            if (code.TerminologyId.Value != OpenEhrTerminologyIdentifiers.TerminologyIdOpenehr) return false;
            return TerminologyDoc.CreateNavigator().Select("/terminology/group[@name='" + groupId + "']/concept[@id='" + code.CodeString + "']").Count > 0;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="lang"></param>
        /// <returns></returns>
        public List<CodePhrase> CodesForGroupName(string name, string lang)
        {
            return CodesForGroupId(name);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="code"></param>
        /// <param name="lang"></param>
        /// <returns></returns>
        public string RubricForCode(string code, string lang)
        {
            var navigator = TerminologyDoc.CreateNavigator();
            foreach (XPathNavigator nav in navigator.Select("/terminology/group/concept[@id='" + code + "']/@rubric"))
            {
                return nav.Value;
            }
            return string.Empty;
        }

        #endregion
    }
}
