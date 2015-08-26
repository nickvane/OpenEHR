using System.Collections.Generic;
using OpenEhr.DesignByContract;

namespace OpenEhr.RM.Support.Terminology.Impl
{
    /// <summary>
    /// 
    /// </summary>
    public class TerminologyService : OpenEhrTerminologyIdentifiers, ITerminologyServiceProvider
    {
        readonly Dictionary<string, ITerminologyAccess> terminologyAccessDictionary;
        readonly Dictionary<string, ICodeSetAccess> codeSetAccessDictionary;

        public TerminologyService(
            Dictionary<string, ITerminologyAccess> terminologyAccessDictionary,
            Dictionary<string, ICodeSetAccess> codeSetAccessDictionary
            )
        {
            this.terminologyAccessDictionary = terminologyAccessDictionary;
            this.codeSetAccessDictionary = codeSetAccessDictionary;
        }

        #region ITerminologyService Members

        public ITerminologyAccess Terminology(string name)
        {
            Check.Require(!string.IsNullOrEmpty(name), "name must not be null or empty string.");
            Check.Require(terminologyAccessDictionary.ContainsKey(name), string.Format("Terminology {0} is unknown.", name));
            ITerminologyAccess result = terminologyAccessDictionary[name];
            Check.Ensure(result != null, "Result must not be null.");
            return result;
        }

        public ICodeSetAccess CodeSet(string name)
        {
            Check.Require(!string.IsNullOrEmpty(name), "name must not be null or empty string.");
            Check.Require(codeSetAccessDictionary.ContainsKey(name), string.Format("CodeSet {0} is unknown.", name));
            ICodeSetAccess result = codeSetAccessDictionary[name];
            Check.Ensure(result != null, "Result must not be null.");
            return result;
        }

        public ICodeSetAccess CodeSetForId(string id)
        {
            Check.Require(!string.IsNullOrEmpty(id), "id must not be null or empty.");

            //TODO: ValidCodeSetId(id) checks for a valid openEhr CodeSet identifer.
            //Need a check for a valid id that is not an openEhr identifer.

            ICodeSetAccess result = FindCodeSetId(id);
            Check.Ensure(result != null, "Result must not be null");
            return result;
        }

        public bool HasTerminology(string name)
        {
            Check.Require(!string.IsNullOrEmpty(name), "name must not be null or empty string.");
            return terminologyAccessDictionary.ContainsKey(name);
        }

        public bool HasCodeSet(string name)
        {
            Check.Require(!string.IsNullOrEmpty(name), "name must not be null or empty string.");
            return codeSetAccessDictionary.ContainsKey(name);
        }

        public List<string> TerminologyIdentifiers
        {
            get
            {
                List<string> terminologyIdentifiers = new List<string>();
                foreach (ITerminologyAccess terminologyAccess in terminologyAccessDictionary.Values)
                {
                    terminologyIdentifiers.Add(terminologyAccess.Id);
                }
                return terminologyIdentifiers;
            }
        }

        public Dictionary<string, string> OpenehrCodeSets
        {
            get
            {
                Dictionary<string, string> openehrCodeSets = new Dictionary<string, string>();
                foreach (ICodeSetAccess codesetAccess in codeSetAccessDictionary.Values)
                {
                    openehrCodeSets.Add((codesetAccess as CodeSetAccess).InternalId, codesetAccess.Id);
                }
                return openehrCodeSets;
            }
        }

        public List<string> CodeSetIdentifiers
        {
            get
            {
                List<string> codeSetIdentifiers = new List<string>();
                foreach (ICodeSetAccess codeSetAccess in codeSetAccessDictionary.Values)
                {
                    codeSetIdentifiers.Add(codeSetAccess.Id);
                }
                return codeSetIdentifiers;
            }
        }

        #endregion

        #region Helpers


        private ICodeSetAccess FindCodeSetId(string id)
        {
            foreach (ICodeSetAccess codeSetAccess in codeSetAccessDictionary.Values)
            {
                if ((codeSetAccess as CodeSetAccess).InternalId == id)
                    return codeSetAccess;
            }
            return null;
        }

        #endregion

    }
}
