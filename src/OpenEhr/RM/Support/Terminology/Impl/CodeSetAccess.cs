using System;
using System.Collections.Generic;
using OpenEhr.DesignByContract;
using OpenEhr.RM.Support.Terminology.Impl.Data.ref_impl_java;

namespace OpenEhr.RM.Support.Terminology.Impl
{
    /// <summary>
    /// 
    /// </summary>
    public class CodeSetAccess : ICodeSetAccessProvider
    {
        private readonly string _name;
        readonly Lazy<CodesetData> _codesetData;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="xmlFilePath"></param>
        public CodeSetAccess(string name, string xmlFilePath)
        {
            Check.Require(!string.IsNullOrEmpty(name), "name must not be null or empty.");
            this._name = name;

            _codesetData = new Lazy<CodesetData>(delegate()
            {
                return string.IsNullOrEmpty(xmlFilePath) ?
                          new CodesetData() :
                          new CodesetData(xmlFilePath);

            });
        }

        private CodesetData CodesetData
        {
            get { return _codesetData.Value; }
        }

        internal string InternalId
        {
            get
            {
                var id = CodesetData.GetCodesetOpenEhrId(this._name);

                Check.Ensure(!string.IsNullOrEmpty(id), "id must not be null or empty.");

                return id;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public Dictionary<string, string> AllCodesAndDescriptions
        {
            get { return CodesetData.GetCodeDescriptions(_name); }
        }

        #region ICodeSetAccess Members

        /// <summary>
        /// 
        /// </summary>
        public string Id
        {
            get 
            {
                return _name;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public List<OpenEhr.RM.DataTypes.Text.CodePhrase> AllCodes
        {
            get 
            {
                return CodesetData.GetAllCodes(_name);    
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="lang"></param>
        /// <returns></returns>
        public bool HasLang(OpenEhr.RM.DataTypes.Text.CodePhrase lang)
        {
            return CodesetData.HasLanguage(lang.CodeString);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public bool HasCode(OpenEhr.RM.DataTypes.Text.CodePhrase code)
        {
            return CodesetData.HasCode(_name, code.CodeString);           
        }

        #endregion
    }
}
