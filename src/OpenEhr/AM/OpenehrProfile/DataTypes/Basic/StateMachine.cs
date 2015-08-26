using System;
using OpenEhr.AssumedTypes;
using OpenEhr.Resources;
using OpenEhr.Attributes;

namespace OpenEhr.AM.OpenehrProfile.DataTypes.Basic
{
    /// <summary>
    /// Definition of a state machine in terms of states, transition events and outputs, and next states.
    /// </summary>
    [Serializable]
    [AmType("STATE_MACHINE")]
    public class StateMachine
    {
        #region Constructors
        public StateMachine() { }

        public StateMachine(Set<State> states)
        {
            this.States = states;
        }

        #endregion

        #region Class properties

        private Set<State> states;

        public Set<State> States
        {
            get { return states; }
            set
            {
                DesignByContract.Check.Require(value != null && value.Count > 0,
                    string.Format(CommonStrings.XMustNotBeNullOrEmpty,"States value"));
                states = value;
            }
        }

        #endregion
    }
}