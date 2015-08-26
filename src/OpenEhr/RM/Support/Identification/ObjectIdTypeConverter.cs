using System;
using System.ComponentModel;
using System.Globalization;

namespace OpenEhr.RM.Support.Identification
{
    public class ObjectIdTypeConverter : ExpandableObjectConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            if (sourceType == typeof(string))
                return true;

            return base.CanConvertFrom(context, sourceType);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            string valueString = value as string;
            if (valueString != null)
            {
                if (HierObjectId.IsValid(valueString))
                    return new HierObjectId(valueString);                    
            }
            return base.ConvertFrom(context, culture, value);
        }

        public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture,
                                         object value, Type destinationType)
        {
            Support.Identification.ObjectId objectId = value as Support.Identification.ObjectId;
            if (destinationType == typeof(string) && objectId != null)
            {
                if (objectId.Value == null)
                    return "";
                else
                    return objectId.Value;
            }

            return base.ConvertTo(context, culture, value, destinationType);
        }

        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            if (destinationType.IsAssignableFrom(typeof(Support.Identification.ObjectId)))
                return true;
            else
                return base.CanConvertTo(context, destinationType);
        }
    }
}