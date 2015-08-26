using System.Collections.Generic;

namespace OpenEhr.RM.Support.Terminology
{
    public interface ITerminologyService
    {
        ITerminologyAccess Terminology(string name);
        ICodeSetAccess CodeSet(string name);
        ICodeSetAccess CodeSetForId(string id);
        bool HasTerminology(string name);
        bool HasCodeSet(string name);
        List<string> TerminologyIdentifiers { get;}
        Dictionary<string, string> OpenehrCodeSets { get;}
        List<string> CodeSetIdentifiers { get;}
    }
}
