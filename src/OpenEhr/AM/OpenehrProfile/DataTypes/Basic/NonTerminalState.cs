using System;
using OpenEhr.AssumedTypes;
using OpenEhr.Resources;
using OpenEhr.Attributes;

namespace OpenEhr.AM.OpenehrProfile.DataTypes.Basic
{
    /// <summary>
    /// Definition of a non-terminal state in a state machine, i.e. one that has transitions
    /// </summary>
    [Serializable]
    [AmType("NON_TERMINAL_STATE")]
    public class NonTerminalState: State
    {
        #region Constructors
        public NonTerminalState():base() { }
        public NonTerminalState(string name, Set<Transition> transitions):base(name)
        {
            this.Transitions = transitions;
        }
        #endregion

        #region Class properties
        private Set<Transition> transitions;

        public Set<Transition> Transitions
        {
            get { return transitions; }
            set
            {
                DesignByContract.Check.Require(value != null && value.Count > 0,
                    string.Format(CommonStrings.XMustNotBeNullOrEmpty, "Transitions value"));
                transitions = value;
            }
        }

        #endregion

        public override bool Equals(object obj)
        {
            if(! base.Equals(obj))
                return false;

            NonTerminalState objNonTerminalState = obj as NonTerminalState;
            if(objNonTerminalState == null)
                return false;

            foreach (Transition transition in this.Transitions)
            {
                if (!objNonTerminalState.Transitions.Has(transition))
                    return false;
            }

            return true;
        }
    }
}
