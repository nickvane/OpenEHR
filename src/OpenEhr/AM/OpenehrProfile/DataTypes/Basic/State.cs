using System;
using System.Collections.Generic;
using System.Text;
using OpenEhr.Resources;

namespace OpenEhr.AM.OpenehrProfile.DataTypes.Basic
{
    /// <summary>
    /// Abstract definition of one state in a state machine
    /// </summary>
    [Serializable]
    public abstract class State//: IRmType
    {
        #region Constructors
        protected State() { }
        protected State(string name)
        {
            this.Name = name;
        }
        #endregion

        #region Class properties
        private string name;

        /// <summary>
        /// name of this state
        /// </summary>
        public string Name
        {
            get { return name; }
            set
            {
                DesignByContract.Check.Require(value == null || value != string.Empty,
                    string.Format(CommonStrings.IfXIsNotNullMustBeEmpty, "Name value"));
                name = value;
            }
        }

        #endregion

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            State objState = obj as State;
            if (objState == null)
                return false;

            return this.Name == objState.Name;
        }
    }
}