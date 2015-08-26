using System;
using System.ComponentModel;

namespace OpenEhr.RM.DataTypes.Text.Impl
{
    public class CodePhraseTypeConverter : ExpandableObjectConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            if (sourceType == typeof(string))
                return true;

            return base.CanConvertFrom(context, sourceType);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
        {
            if (value is string)
            {
                try
                {
                    string s = (string)value;

                    string[] parts = s.Split(':');

                    DataTypes.Text.CodePhrase codePhrase = new DataTypes.Text.CodePhrase(parts[2], parts[0]);

                    return codePhrase;
                }
                catch (Exception ex)
                {
                    throw new ArgumentException("Can not convert '" + (string)value + "' to type DV_CODE_PHRASE");
                }
            }

            return base.ConvertFrom(context, culture, value);
        }

        public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture,
                                         object value, Type destinationType)
        {

            DataTypes.Text.CodePhrase codePhrase = value as DataTypes.Text.CodePhrase;
            if (destinationType == typeof(string) && codePhrase != null)
            {

                string s = codePhrase.TerminologyId.Value + "::" + codePhrase.CodeString;
                if (s == "::")
                    return "";

                return s;
            }

            return base.ConvertTo(context, culture, value, destinationType);
        }

        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            if (destinationType == typeof(DataTypes.Text.CodePhrase))
                return true;
            else
                return base.CanConvertTo(context, destinationType);
        }
    }
}