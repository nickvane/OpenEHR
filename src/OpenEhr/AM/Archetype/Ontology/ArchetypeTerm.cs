using System;
using System.Collections.Generic;
using System.Text;
using OpenEhr.DesignByContract;
using OpenEhr.Resources;

namespace OpenEhr.AM.Archetype.Ontology
{
    /// <summary>
    /// Representation of any coded entity (term or constraint) in the archetype ontology.
    /// </summary>
    [Serializable]
    public class ArchetypeTerm
    {
        #region Constructors
        public ArchetypeTerm(string aCode, AssumedTypes.Hash<string, string> items)
        {
            this.Code = aCode;
            this.Items = items;
        }

        public ArchetypeTerm() { }
        #endregion

        #region Class properties
        private string code;
        /// <summary>
        /// Code of this term.
        /// </summary>
        public string Code
        {
            get { return this.code; }
            set
            {
                Check.Require(!string.IsNullOrEmpty(value), string.Format(
                    CommonStrings.XIsNullOrEmpty, "Code value"));
                this.code = value;
            }
        }

        private AssumedTypes.Hash<string, string> items;
        /// <summary>
        /// Hash of keys (“text”, “description” etc) and corresponding values.
        /// </summary>
        public AssumedTypes.Hash<string, string> Items
        {
            get { return this.items; }
            set
            {
                Check.Require(value != null, string.Format(
                    CommonStrings.XIsNull, "Items value"));
                this.items = value;
            }
        }
        #endregion

        #region Functions
        /// <summary>
        /// List of all keys used in this term.
        /// </summary>
        /// <returns></returns>
        public AssumedTypes.Set<string> Keys()
        {
            Check.Require(this.Items != null, string.Format(CommonStrings.XMustNotBeNull, "ArchetypeTerm.Items"));

            AssumedTypes.List<string> itemsKeys = this.Items.Keys;
            Check.Ensure(itemsKeys != null, "Ensure keys must not be null.");

            System.Collections.Generic.List<string> genericList = new List<string>();
            foreach (string eachKey in itemsKeys)
                genericList.Add(eachKey);

            AssumedTypes.Set<string> keys = new OpenEhr.AssumedTypes.Set<string>(genericList);

            Check.Ensure(keys != null, "keys must not be null.");

            return keys;
        }

        #endregion

    }
}