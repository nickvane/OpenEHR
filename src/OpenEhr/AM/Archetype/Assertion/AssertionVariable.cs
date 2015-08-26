using System;
using OpenEhr.Resources;
using OpenEhr.Attributes;

namespace OpenEhr.AM.Archetype.Assertion
{
    /// <summary>
    /// Definition of a named variable used in an assertion expression. Note: the definition
    /// of named variables may change; still under development in ADL2.
    /// </summary>
    [AmType("ASSERTION_VARIABLE")]
    [Serializable]
    public class AssertionVariable
    {
        #region Constructors        
       
        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="name">Name of variable.</param>
        /// <param name="definition">Formal definition of the variable. (see ADL2 specification; still under development).</param>
        public AssertionVariable(string name, string definition)
        {
            this.Name = name;
            this.Definition = definition;
        }

        public AssertionVariable() { }
        #endregion

        #region Class properties
        private string name;

        /// <summary>
        /// Get or set Name of variable.
        /// </summary>
        public string Name
        {
            get { return name; }
            set
            {
                DesignByContract.Check.Require(!string.IsNullOrEmpty(value), 
                    string.Format(CommonStrings.XMustNotBeNullOrEmpty, "Name value"));
                name = value;
            }
        }

        private string definition;

        /// <summary>
        /// Get or set Formal definition of the variable. (see ADL2 specification; still under development).
        /// </summary>
        public string Definition
        {
            get { return definition; }
            set
            {
                DesignByContract.Check.Require(!string.IsNullOrEmpty(value), 
                    string.Format(CommonStrings.XMustNotBeNullOrEmpty, "Definition value"));
                name = value;
            }
        }
        #endregion
       
    }
}