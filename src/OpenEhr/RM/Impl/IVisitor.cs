using OpenEhr.RM.Composition.Content.Navigation;
using OpenEhr.RM.Composition.Content.Entry;

namespace OpenEhr.RM.Impl
{
    public interface IVisitor
    {
        void VisitComposition(Composition.Composition composition);
        void VisitEventContext(Composition.EventContext context);
        void VisitSection(Section section);
        void VisitObservation(Observation observation);
        void VisitEvaluation(Evaluation evaluation);
        void VisitAdminEntry(AdminEntry adminEntry);
        void VisitInstruction(Instruction instruction);
        void VisitAction(Action action);
        void VisitActivity(Activity activity);
    }
}
