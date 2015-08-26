using System;
using System.Collections.Generic;
using System.Text;
using OpenEhr.DesignByContract;
using OpenEhr.Resources;

namespace OpenEhr.AM.Archetype
{
    [Serializable]
    public class ValidityKind
    {
        public ValidityKind(int value)
        {
            Check.Require(ValidityKind.ValidValidity(value), 
                string.Format(AmValidationStrings.XMustBeValidY, "value", "ValidityKind"));
            this.value = value;
        }

        public const int mandatory = 1001;
        public const int optional = 1002;
        public const int disallowed = 1003;

        public int value;
        public int Value
        {
            get
            {
                return value;
            }
        }

        /// <summary>
        /// Function to test validity values.
        /// </summary>
        /// <param name="validity"></param>
        /// <returns></returns>
        public static bool ValidValidity(int validity)
        {
            return validity >= mandatory && validity <= disallowed;
        }
    }
}
