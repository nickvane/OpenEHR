using System;
using OpenEhr.DesignByContract;
using OpenEhr.Attributes;

namespace OpenEhr.RM.Support.Identification
{
    [Serializable]
    [RmType("openEHR", "COMMON", "UID")]
    public abstract class Uid
    {
        protected Uid(string value)
        {
            Check.Require(this.IsValidValue(value), "value must be valid.");
            this.value = value;
        }

        public static bool IsValid(string value)
        {
            if (Uuid.IsValid(value))
                return true;
            if (IsoOid.IsValid(value))
                return true;
            return InternetId.IsValid(value);
        }

        public static Uid Create(string value)
        {
            Check.Require(!string.IsNullOrEmpty(value), "value must not be null or empty.");

            Uid result = null;

            if (Uuid.IsValid(value))
                result = new Uuid(value);
            else if (IsoOid.IsValid(value))
                result = new IsoOid(value);
            else if (InternetId.IsValid(value))
                result = new InternetId(value);

            Check.Ensure(result != null, "value must be either an UUID, INTERNET_ID, or ISO_OID " + value);

            return result;
        }

        private string value;

        public string Value
        {
            get { return this.value; }
        }

        protected abstract bool IsValidValue(string value);

        public override string ToString()
        {
            return this.value;
        }

        public override int GetHashCode()
        {
            return this.value.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            Uid uid = obj as Uid;

            if (uid != null)
                return this.value.Equals(uid.Value);
            else
                return false;
        }
    }
}
