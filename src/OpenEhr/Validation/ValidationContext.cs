using OpenEhr.AM.Archetype.ConstraintModel;
using OpenEhr.Futures.OperationalTemplate;
using OpenEhr.RM.Support.Identification;
using OpenEhr.DesignByContract;
using OpenEhr.RM.Support.Terminology;

namespace OpenEhr.Validation
{
    public delegate void AcceptValidationError(object sender, ValidationEventArgs e);
    public delegate CArchetypeRoot FetchOperationalObject(object sender, FetchOperationalObjectEventArgs e); 

    public class ValidationContext
    {
        readonly ITerminologyService terminologyService;

        internal ValidationContext() { }

        public ValidationContext(AcceptValidationError acceptErrorDelegate, FetchOperationalObject fetchObjectDelegate, ITerminologyService terminologyService)
            : this(acceptErrorDelegate, fetchObjectDelegate)
        {
            Check.Require(terminologyService != null, "terminologyService must not be null.");
            this.terminologyService = terminologyService;
        }

        public ValidationContext(AcceptValidationError acceptErrorDelegate, FetchOperationalObject fetchObjectDelegate)
        {
            AcceptError = acceptErrorDelegate;
            FetchObject = fetchObjectDelegate;
        }

        /// <summary>
        /// 
        /// </summary>
        public ITerminologyService TerminologyService
        {
            get
            {
                return terminologyService;
            }
        }

        public virtual FetchOperationalObject FetchObject
        {
            get; set;
        }

        public virtual AcceptValidationError AcceptError
        {
            get; set;
        }

        public virtual bool IsSuppressingAcceptErrors
        {
            get; set;
        }

        internal void AcceptValidationError(ArchetypeConstraint constraint, string message)
        {
            if (AcceptError != null && !IsSuppressingAcceptErrors)
                AcceptError(constraint, new ValidationEventArgs(constraint, message));
        }

        internal CArchetypeRoot FetchOperationalObject(ArchetypeConstraint constraint, ObjectId id)
        {
            return FetchObject != null ? FetchObject(constraint, new FetchOperationalObjectEventArgs(id)) : null;
        }
    }
}
