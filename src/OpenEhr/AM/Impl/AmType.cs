using System;
using System.Collections.Generic;
using System.Text;
using OpenEhr.DesignByContract;
using OpenEhr.Resources;
using OpenEhr.Attributes;

namespace OpenEhr.AM.Impl
{

    public static class AmType
    {
        public static string GetName(object openEhrV1AmType)
        {
            Type openEhrObjType = openEhrV1AmType.GetType();

            AmTypeAttribute amTypeAttribute =
                Attribute.GetCustomAttribute(openEhrObjType, typeof(AmTypeAttribute), false) as AmTypeAttribute;

            if (amTypeAttribute == null)
                throw new ArgumentException(string.Format(CommonStrings.OpenEhrTypeMissingAmTypeAttribute, openEhrObjType.ToString())); 

            return amTypeAttribute.AmTypeName;

        }
    }
}