using System.Collections.Generic;

namespace OpenEhr.RM.Support.Terminology
{
    public interface ITerminologyAccess
    {
        string Id { get;}
        List<OpenEhr.RM.DataTypes.Text.CodePhrase> AllCodes { get;}
        List<OpenEhr.RM.DataTypes.Text.CodePhrase> CodesForGroupId(string groupId);
        bool HasCodeForGroupId(string groupId, OpenEhr.RM.DataTypes.Text.CodePhrase code);
        List<OpenEhr.RM.DataTypes.Text.CodePhrase> CodesForGroupName(string name, string lang);
        string RubricForCode(string code, string lang);
    }
}
