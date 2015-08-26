using OpenEhr.DesignByContract;

namespace OpenEhr.RM.Composition.Impl
{
    public delegate void CompositionAdapterDelegate(CompositionAdapterContext context);

    public class CompositionAdapterContext
    {
        public CompositionAdapterContext(Composition composition)
        {
            Check.Require(composition != null, "composition must not be null");
            this.composition = composition;
        }

        Composition composition;

        public Composition Composition
        {
            get { return composition; }
            set { composition = value; }
        }
    }
}
