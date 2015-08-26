using System.Collections.Generic;

namespace OpenEhr.RM.Support.Terminology
{
    public interface ICodeSetAccess
    {
        string Id { get;}
        List<OpenEhr.RM.DataTypes.Text.CodePhrase> AllCodes { get;}
        bool HasLang(OpenEhr.RM.DataTypes.Text.CodePhrase lang);
        bool HasCode(OpenEhr.RM.DataTypes.Text.CodePhrase code);
    }
}
