using System.ComponentModel;

namespace OpenEhr.RM.Support.Identification
{
    public class TerminologyIdTypeConverter : ObjectIdTypeConverter
    {
        public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture,
                                           object value)
        {
            string s = value as string;
            if (s != null)
            {
                if (TerminologyId.IsValid(s))
                    return  new TerminologyId(s);
            }
            return base.ConvertFrom(context, culture, value);
        }
    }
}