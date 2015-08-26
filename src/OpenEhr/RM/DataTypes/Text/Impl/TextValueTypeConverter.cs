using System;
using System.ComponentModel;

namespace OpenEhr.RM.DataTypes.Text.Impl
{
    public class TextValueTypeConverter : ExpandableObjectConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            if (sourceType == typeof(string))
                return true;

            if (sourceType.IsAssignableFrom(typeof(DataTypes.Text.DvText)))
                return true;

            return base.CanConvertFrom(context, sourceType);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture,
            object value)
        {
            if (value != null)
            {
                try
                {
                    if (value is string)
                    {
                        string s = (string)value;
                        string[] parts = s.Split(new string[] { "::" }, StringSplitOptions.RemoveEmptyEntries);

                        if (parts.Length == 3)
                        {
                            DataTypes.Text.DvCodedText codedValue
                                = new DataTypes.Text.DvCodedText(parts[2], parts[1], parts[0]);

                            return codedValue;
                        }

                        if (parts.Length == 2 && parts[0] == "local")
                        {
                            DataTypes.Text.DvCodedText codedValue
                                = new DataTypes.Text.DvCodedText("", parts[1], parts[0]);

                            return codedValue;
                        }

                        DesignByContract.Check.Assert(parts.Length <= 1, "Invalid text string: " + s);

                        DataTypes.Text.DvText textValue = new DataTypes.Text.DvText(s);

                        return textValue;
                    }

                    if (value.GetType().IsAssignableFrom(typeof(DataTypes.Text.DvText)))
                    {
                        return value;
                    }
                }
                catch (Exception ex)
                {
                    throw new ArgumentException("Can not convert '" + (string)value + "' to type DvText", ex);
                }
            }
            return base.ConvertFrom(context, culture, value);
        }

        public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture,
            object value, Type destinationType)
        {
            DataTypes.Text.DvText textValue = value as DataTypes.Text.DvText;
            if (textValue != null)
            {
                if (destinationType == typeof(string))
                {
                    DataTypes.Text.DvCodedText codedValue = textValue as DataTypes.Text.DvCodedText;

                    if (codedValue == null)
                        return textValue.Value;

                    else
                    {
                        string s = codedValue.DefiningCode.TerminologyId.Value + "::" + codedValue.DefiningCode.CodeString
                            + "::" + codedValue.Value;

                        s = s.TrimEnd(':');

                        return s;
                    }
                }

                if (destinationType == typeof(System.Object))
                {
                    return textValue as object;
                }

                if (destinationType == textValue.GetType())
                {
                    return textValue;
                }
            }

            return base.ConvertTo(context, culture, value, destinationType);
        }

        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            if (destinationType.IsAssignableFrom(typeof(DataTypes.Text.DvText)))
                return true;
            else
                return base.CanConvertTo(context, destinationType);
        }
    }
}