using System;
using OpenEhr.AM.Archetype.ConstraintModel;
using OpenEhr.Attributes;
using OpenEhr.DesignByContract;
using OpenEhr.Paths;

namespace OpenEhr.RM.Impl
{
    [Serializable]
    public abstract class RmType
        : IRmType
    {
        public static string GetRmTypeName(Type rmType)
        {
            Check.Require(rmType != null, "rmType must not be null");

            RmTypeAttribute[] rmTypeAttributes
                = rmType.GetCustomAttributes(typeof(RmTypeAttribute), true) as RmTypeAttribute[];

            if (rmTypeAttributes == null || rmTypeAttributes.Length < 1)
                return null;
            else
                return ((RmTypeAttribute)rmTypeAttributes[0]).RmEntity;
        }

        public static string GetRmTypeName(IRmType rmType)
        {
            Check.Require(rmType != null, "rmType must not be null");

            return rmType.GetRmTypeName();
        }

        protected virtual string RmTypeName
        {
            get
            {
                string name = null;
                string genericTypeName = null;

                Type thisType = this.GetType();

                if (thisType.IsGenericType)
                {
                    Type[] genericArgs = thisType.GetGenericArguments();
                    if (genericArgs != null & genericArgs.Length > 0)
                        genericTypeName = GetRmTypeName(genericArgs[0]);
                }

                name = !string.IsNullOrEmpty(genericTypeName)
                    ? string.Format("{0}<{1}>", GetRmTypeName(thisType), genericTypeName)
                    : GetRmTypeName(thisType);             

                Check.Ensure(!string.IsNullOrEmpty(name), "name must not be null or empty string.");

                return name;
            }
        }

        protected virtual void SetAttributeValue(string attributeName, object value)
        {
            System.ComponentModel.PropertyDescriptorCollection propertyCollection =
                  System.ComponentModel.TypeDescriptor.GetProperties(this);

            foreach (System.ComponentModel.PropertyDescriptor propertyDesc in propertyCollection)
            {
                RmAttributeAttribute rmAttribute
                    = propertyDesc.Attributes[typeof(RmAttributeAttribute)] as RmAttributeAttribute;

                if (rmAttribute != null)
                {
                    if (rmAttribute.AttributeName == attributeName)
                    {
                        System.Reflection.PropertyInfo property
                            = this.GetType().GetProperty(propertyDesc.Name);
                        property.SetValue(this, value, null);
                        break;
                    }
                }
            }
        }

        protected internal virtual object GetAttributeValue(string attributeName)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        protected internal virtual System.Collections.IEnumerable GetAllAttributeValues()
        {
            throw new Exception("The method or operation is not implemented.");
        }

        protected void BuildPath(Path path)
        {
            Check.Require(path != null, "path must not be null");
            Check.Require(path.Current != null, "current path step must not be null");

            string attributeName = path.Current.Attribute;

            object value = GetAttributeValue(attributeName);
            if (value == null)
            {
                CComplexObject complexObjectConstraint = this.Constraint as CComplexObject;
                if (complexObjectConstraint == null)
                    throw new NotImplementedException();

                CAttribute attributeConstraint = complexObjectConstraint.GetAttribute(attributeName);
                if (attributeConstraint == null)
                    throw new ApplicationException("constraint for attribute not found");

                CMultipleAttribute multipleAttributeConstraint = attributeConstraint as CMultipleAttribute;

                if (multipleAttributeConstraint == null)
                {
                    if (attributeConstraint.Children.Count != 1)
                        throw new ApplicationException("Single attribute constraint must have exactly one children");

                    CObject objectConstraint = attributeConstraint.Children[0];
                    CDefinedObject definedObjectConstraint = objectConstraint as CDefinedObject;
                    if (definedObjectConstraint == null)
                        throw new NotImplementedException();

                    value = definedObjectConstraint.DefaultValue;
                    SetAttributeValue(attributeName, value);

                }
                else
                {
                    Type itemType = null;
                    value = multipleAttributeConstraint.CreateAggregate(itemType);
                    SetAttributeValue(attributeName, value);
                }
            }

            if (path.NextStep())
            {
                IRmType rmType = value as IRmType;
                if (rmType == null)
                {
                    AssumedTypes.IAggregate container = value as AssumedTypes.IAggregate;
                    if (container != null)
                        container.BuildPath(path);
                    else
                        throw new ApplicationException("expected IRmType");
                }
                else
                    rmType.BuildPath(path);
            }
        }

        CObject constraint;

        protected CObject Constraint
        {
            get { return this.constraint; }
            set { this.constraint = value; }
        }

        protected bool HasConstraint
        {
            get { return constraint != null; }
        }

        #region IRmType Members

        string IRmType.GetXmlRmTypeName()
        {
            return GetRmTypeName(this.GetType());
        }

        string IRmType.GetRmTypeName()
        {
            string rmTypeName = this.RmTypeName;

            Check.Ensure(!string.IsNullOrEmpty(rmTypeName), "RmTypeName must not be null or empty");
            return rmTypeName;
        }

        void IRmType.SetAttributeValue(string attributeName, object value)
        {
            Check.Require(!string.IsNullOrEmpty(attributeName), "attributeName must not be null or empty");

            this.SetAttributeValue(attributeName, value);
        }

        object IRmType.GetAttributeValue(string attributeName)
        {
            Check.Require(!string.IsNullOrEmpty(attributeName), "attributeName must not be null or empty");

            return this.GetAttributeValue(attributeName);
        }

        void IRmType.BuildPath(Path path)
        {
            Check.Require(path != null, "path must not be null");
            Check.Require(path.Current != null, "current path step must not be null");

            this.BuildPath(path);
        }

        CObject IRmType.Constraint
        {
            get { return this.Constraint; }
            set { this.Constraint = value; }
        }

        #endregion
    }
}
