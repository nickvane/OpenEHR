using System;
using System.Collections.Generic;
using System.Text;

namespace OpenEhr.Paths
{
    internal class AssertionContext
    {
        internal AssertionContext(object data, AssertionContext parent)
        {
            this.data = data;
            this.parent = parent;
        }

        #region class properties
        private object data;


        public object Data
        {
            [System.Diagnostics.DebuggerStepThrough]
            get { return data; }
            set { data = value; }
        }
        private AssertionContext parent;

        public AssertionContext Parent
        {
            [System.Diagnostics.DebuggerStepThrough]
            get { return parent; }
            set { parent = value; }
        }

        #endregion
    }
}
