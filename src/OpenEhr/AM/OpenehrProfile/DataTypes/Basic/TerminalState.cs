using System;
using OpenEhr.Attributes;

namespace OpenEhr.AM.OpenehrProfile.DataTypes.Basic
{
    /// <summary>
    /// Definition of a terminal state in a state machine, i.e. a state with no exit transitions.
    /// </summary>
    [Serializable]
    [AmType("TERMINAL_STATE")]
    public class TerminalState: State
    {
        public TerminalState() : base() { }

        public TerminalState(string name)
            : base(name)
        {
        }

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }
    }
}
