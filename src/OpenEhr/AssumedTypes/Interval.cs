using System;
using OpenEhr.Attributes;

namespace OpenEhr.AssumedTypes
{

    public interface IInterval<T> where T : IComparable
    {
        T Lower
        {
            get;
        }

        T Upper
        {
            get;
        }

        bool LowerUnbounded
        {
            get;
        }

        bool UpperUnbounded
        {
            get;
        }

        bool LowerIncluded
        {
            get;
        }

        bool UpperIncluded
        {
            get;
        }

        bool Has(T e);
    }
    /// <summary>
    /// Interval of ordered items
    /// </summary>
    [Serializable]
    [RmType("openEHR", "SUPPORT", "INTERVAL")]
    public class Interval<T> : IInterval<T> where T : IComparable
    {
        #region constructors
        public Interval(T lower, bool lowerIncluded, T upper, bool upperIncluded)
        {
            this.Lower = lower;
            this.Upper = upper;
            this.LowerIncluded = lowerIncluded;
            this.UpperIncluded = upperIncluded;
        }

        public Interval() { }
        #endregion

        #region class properties
        private T lower;
        public T Lower
        {
            get { return this.lower; }
            set
            {
                this.lower = value;

            }
        }

        private T upper;
        public T Upper
        {
            get { return this.upper; }
            set { this.upper = value; }
        }

        private bool lowerUnboundedSet;
        private bool lowerUnbounded;
        public bool LowerUnbounded
        {
            get
            {
                if (!lowerUnboundedSet)
                    SetLowerUnboundedUpperUnbounded();
                return this.lowerUnbounded;
            }

            set
            {
                this.lowerUnbounded = value;
                this.lowerUnboundedSet = true;
            }
        }

        private bool upperUnboundedSet;
        private bool upperUnbounded;
        public bool UpperUnbounded
        {
            get
            {
                if (!upperUnboundedSet)
                    SetLowerUnboundedUpperUnbounded();
                return this.upperUnbounded;
            }
            set
            {
                this.upperUnbounded = value;
                this.upperUnboundedSet = true;
            }
        }

        internal bool lowerIncludedSet;
        private bool lowerIncluded;
        public bool LowerIncluded
        {
            get { return this.lowerIncluded; }
            set
            {
                this.lowerIncluded = value;
                this.lowerIncludedSet = true;
            }
        }

        internal bool upperIncludedSet;
        private bool upperIncluded;
        public bool UpperIncluded
        {
            get { return this.upperIncluded; }
            set
            {
                this.upperIncluded = value;
                this.upperIncludedSet = true;
            }
        }

        private void SetLowerUnboundedUpperUnbounded()
        {
            if (this.Lower == null)
                this.LowerUnbounded = true;
            else
                this.LowerUnbounded = false;

            if (this.Upper == null)
                this.UpperUnbounded = true;
            else
                this.UpperUnbounded = false;
        }
        #endregion

        #region functions
        public bool Has(T e)
        {
            DesignByContract.Check.Require(e != null);

            if (this.LowerUnbounded && this.UpperUnbounded)
                return true;

            if (!this.LowerUnbounded)
            {
                if (!this.lowerIncludedSet || this.LowerIncluded)
                {
                    if (e.CompareTo(Lower) < 0)
                        return false;
                }
                else
                {
                    if (e.CompareTo(Lower) < 0 || e.CompareTo(Lower) == 0)
                        return false;
                }
            }

            if (!this.UpperUnbounded)
            {
                if (this.UpperIncluded || !this.upperUnboundedSet)
                {
                    if (e.CompareTo(this.Upper) > 0)
                        return false;
                }
                else
                {
                    if (e.CompareTo(this.Upper) > 0 || e.CompareTo(this.Upper) == 0)
                        return false;
                }
            }

            return true;
        }

        #endregion
    }
}
