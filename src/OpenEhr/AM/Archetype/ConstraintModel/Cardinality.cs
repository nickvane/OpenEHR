using System;
using OpenEhr.DesignByContract;
using OpenEhr.Resources;
using OpenEhr.Attributes;

namespace OpenEhr.AM.Archetype.ConstraintModel
{
    /// <summary>
    /// Express constraints on the cardinality of container objects which are the values of 
    /// multiply-valued attributes, including uniqueness and ordering, providing the 
    /// means to state that a container acts like a logical list, set or bag. The cardinality 
    /// cannot contradict the cardinality of the corresponding attribute within the relevant
    /// reference model.
    /// </summary>
    [Serializable]
    [AmType("CARDINALITY")]
    public class Cardinality
    {
        #region Constructors
        public Cardinality(bool isOrdered, bool isUnique, AssumedTypes.Interval<int> interval)
        {
            this.IsOrdered = isOrdered;
            this.IsUnique = isUnique;
            this.Interval = interval;
        }

        public Cardinality() { }
        #endregion

        #region Class properties
        private bool isOrdered;
        /// <summary>
        /// True if the members of the container attribute to which this cardinality refers are ordered.
        /// </summary>
        public bool IsOrdered
        {
            get { return this.isOrdered; }
            set { this.isOrdered = value; }
        }

        private bool isUnique;
        /// <summary>
        /// True if the members of the container attribute to which this cardinality refers are unique.
        /// </summary>
        public bool IsUnique
        {
            get { return this.isUnique; }
            set { this.isUnique = value; }
        }

        private AssumedTypes.Interval<int> interval;
        /// <summary>
        /// The interval of this cardinality.
        /// </summary>
        public AssumedTypes.Interval<int> Interval
        {
            get { return this.interval; }
            set
            {
                Check.Require(value != null, string.Format(CommonStrings.XMustNotBeNull, "Interval value"));
                Check.Require(!value.LowerUnbounded, AmValidationStrings.CardinalityMustBeBounded);
                this.interval = value;
            }
        }
        #endregion

        #region Functions
        /// <summary>
        /// True if the semantics of this cardinality represent a set, i.e. unordered, unique membership.
        /// </summary>
        /// <returns></returns>
        public bool IsSet()
        {
            return !this.IsOrdered && this.IsUnique;
        }

        /// <summary>
        /// True if the semantics of this cardinality represent a list, i.e. ordered, non-unique membership.
        /// </summary>
        /// <returns></returns>
        public bool IsList()
        {
            return this.IsOrdered && !this.IsUnique;
        }

        /// <summary>
        /// True if the semantics of this cardinality represent a bag, i.e. unordered, non-unique membership.
        /// </summary>
        /// <returns></returns>
        public bool IsBag()
        {
            return !this.IsOrdered && !this.IsUnique;
        }
        #endregion
    }
}