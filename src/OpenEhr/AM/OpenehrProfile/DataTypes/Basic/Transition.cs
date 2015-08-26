using System;
using OpenEhr.Resources;
using OpenEhr.Attributes;

namespace OpenEhr.AM.OpenehrProfile.DataTypes.Basic
{
    /// <summary>
    /// Definition of a state machine transition.
    /// </summary>
    [Serializable]
    [AmType("TRANSITION")]
    public class Transition
    {
        #region Constructors
        public Transition() { }
        public Transition(string _event, string guard, string action, State nextState)
        {
            this.Event = _event;
            this.Guard = guard;
            this.Action = action;
            this.NextState = nextState;
        }

        #endregion

        #region Class properties
        private string _event;

        /// <summary>
        /// Event which fires this transition
        /// </summary>
        public string Event
        {
            get { return _event; }
            set
            {
                DesignByContract.Check.Require(!string.IsNullOrEmpty(value),
                    string.Format(CommonStrings.XMustNotBeNullOrEmpty, "Event value"));
                _event = value;
            }
        }

        private string guard;

        /// <summary>
        /// Guard condition which must be true for this transition to fire
        /// </summary>
        public string Guard
        {
            get { return guard; }
            set
            {
                DesignByContract.Check.Require(!string.IsNullOrEmpty(value),
                    string.Format(CommonStrings.XMustNotBeNullOrEmpty, "Guard value"));
                guard = value;
            }
        }

        private string action;

        /// <summary>
        /// Side-effect action to execute during the firing of this transition
        /// </summary>
        public string Action
        {
            get { return action; }
            set
            {
                DesignByContract.Check.Require(!string.IsNullOrEmpty(value),
                    string.Format(CommonStrings.XMustNotBeNullOrEmpty, "Action value"));
                action = value;
            }
        }

        private State nextState;

        /// <summary>
        /// Target state of transition
        /// </summary>
        public State NextState
        {
            get { return nextState; }
            set
            {
                DesignByContract.Check.Require(value != null,
                  string.Format(CommonStrings.XMustNotBeNull, "NextState value"));
                nextState = value;
            }
        }

        #endregion

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            Transition objTransition = obj as Transition;
            if (objTransition == null)
                return false;

            return this.Event == objTransition.Event && this.Action == objTransition.Action
            && this.Guard == objTransition.Guard;
        }
    }
}