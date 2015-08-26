using System;
using OpenEhr.AM;
using System.Xml;
using OpenEhr.DesignByContract;
using OpenEhr.AM.Archetype.Ontology;
using OpenEhr.RM.DataTypes.Text;
using OpenEhr.AM.Archetype.ConstraintModel;
using OpenEhr.Resources;
using OpenEhr.Serialisation;
using OpenEhr.RM.Impl;
using OpenEhr.AM.Impl;

namespace OpenEhr.Futures.OperationalTemplate
{
    class OperationalTemplateXmlWriter : RmXmlSerializer
    {
        AmXmlSerializer amSerializer = new AmXmlSerializer();

        public void WriteOperationalTemplate(XmlWriter writer, OperationalTemplate operationalTemplate)
        {
            Check.Require(writer != null, string.Format(CommonStrings.XMustNotBeNull, "writer"));
            Check.Require(operationalTemplate != null, string.Format(CommonStrings.XMustNotBeNull, "operationalTemplate"));

            string openEhrPrefix = RmXmlSerializer.UseOpenEhrPrefix(writer);
            string xsiPrefix = RmXmlSerializer.UseXsiPrefix(writer);
            string xsdPrefix = RmXmlSerializer.UseXsdPrefix(writer);

            writer.WriteStartElement(openEhrPrefix, "language", OpenEhrNamespace);
            operationalTemplate.Language.WriteXml(writer);
            writer.WriteEndElement();

            if (operationalTemplate.IsControlled.HasValue)
            {
                writer.WriteElementString(openEhrPrefix,
                    "is_controlled", OpenEhrNamespace, operationalTemplate.IsControlled.ToString());
            }

            if (operationalTemplate.Description != null)
            {
                writer.WriteStartElement(openEhrPrefix, "description", OpenEhrNamespace);
                operationalTemplate.Description.WriteXml(writer);
                writer.WriteEndElement();
            }

            if (operationalTemplate.RevisionHistory != null)
            {
                writer.WriteStartElement(openEhrPrefix, "revision_history", OpenEhrNamespace);
                operationalTemplate.RevisionHistory.WriteXml(writer);
                writer.WriteEndElement();
            }

            if (operationalTemplate.Uid != null)
            {
                writer.WriteStartElement(openEhrPrefix, "uid", OpenEhrNamespace);
                operationalTemplate.Uid.WriteXml(writer);
                writer.WriteEndElement();
            }

            writer.WriteStartElement(openEhrPrefix, "template_id", OpenEhrNamespace);
            operationalTemplate.TemplateId.WriteXml(writer);
            writer.WriteEndElement();

            writer.WriteElementString(openEhrPrefix, 
                "concept", OpenEhrNamespace, operationalTemplate.Concept);

            //Edited by LMT 28/Apr/2009
            writer.WriteStartElement(openEhrPrefix, "definition", OpenEhrNamespace);
            WriteCArchetypeRoot(writer, operationalTemplate.Definition);
            writer.WriteEndElement();

            // LMT added 12 May 2010 for TMP-1252
            if(operationalTemplate.Annotations != null)
                foreach(OpenEhr.RM.Common.Resource.Annotation note in operationalTemplate.Annotations)
                    WriteAnnotation(writer, openEhrPrefix, note);
            
            if (operationalTemplate.Constraints != null && operationalTemplate.Constraints.Attributes != null && operationalTemplate.Constraints.Attributes.Count > 0)
            {
                writer.WriteStartElement(openEhrPrefix, "constraints", OpenEhrNamespace);
                WriteTConstraint(writer, operationalTemplate.Constraints);
                writer.WriteEndElement();
            }

            if(operationalTemplate.View != null)
            {
                Check.Assert(operationalTemplate.View.Constraints != null);
                writer.WriteStartElement(openEhrPrefix, "view", OpenEhrNamespace);
                WriteTView(writer, operationalTemplate.View);
                writer.WriteEndElement();
            }

        }

        // LMT added 12 May 2010 for TMP-1252
        private static void WriteAnnotation(XmlWriter writer, string openEhrPrefix,
            OpenEhr.RM.Common.Resource.Annotation annotation)
        {
            writer.WriteStartElement(openEhrPrefix, "annotations", OpenEhrNamespace);
            writer.WriteAttributeString("path", annotation.Path);

            foreach (string key in annotation.Items.Keys)
            {
                writer.WriteStartElement(openEhrPrefix, "items", OpenEhrNamespace);
                writer.WriteAttributeString("id", key);
                writer.WriteElementString("value", annotation.Items.Item(key));
                writer.WriteEndElement();
            }
            writer.WriteEndElement();
        }

        private void WriteTConstraint(XmlWriter writer, TConstraint tConstraint)
        {
            Check.Require(tConstraint != null);

            if (tConstraint.Attributes != null && tConstraint.Attributes.Count > 0)
            {
                foreach (TAttribute attribute in tConstraint.Attributes)
                {
                    writer.WriteStartElement(UseOpenEhrPrefix(writer), "attributes", OpenEhrNamespace);
                    WriteTAttribute(writer, attribute);
                    writer.WriteEndElement();
                }
            }
        }

        private void WriteTAttribute(XmlWriter writer, TAttribute attribute)
        {
            Check.Require(attribute != null);

            writer.WriteElementString("rm_attribute_name", attribute.RmAttributeName);
            foreach (TComplexObject tComplexObject in attribute.Children)
            {
                writer.WriteStartElement(UseOpenEhrPrefix(writer), "children", OpenEhrNamespace);
                WriteTComplexObject(writer, tComplexObject);
                writer.WriteEndElement();
            }
            writer.WriteElementString("differential_path", attribute.DifferentialPath);           
        }

        private void WriteTComplexObject(XmlWriter writer, TComplexObject tComplexObject)
        {
            Check.Require(tComplexObject != null);
         
            WriteCComplexObject(writer, tComplexObject);
            if(tComplexObject.DefaultValue != null)
            {
                writer.WriteStartElement(UseOpenEhrPrefix(writer), "default_value", OpenEhrNamespace);
                string rmTypeName = ((IRmType)tComplexObject.DefaultValue).GetRmTypeName();
                writer.WriteAttributeString("type", XsiNamespace, rmTypeName);
                tComplexObject.DefaultValue.WriteXml(writer);
                writer.WriteEndElement();
            }
        }

        private void WriteTView(XmlWriter writer, TView tView)
        {
            Check.Require(tView != null);

            string openEhrPrefix = UseOpenEhrPrefix(writer);
            if (tView.Constraints != null && tView.Constraints.Count > 0)
            {
                foreach (TViewConstraint viewConstraint in tView.Constraints)
                {
                    writer.WriteStartElement(openEhrPrefix, "constraints", OpenEhrNamespace);
                    writer.WriteAttributeString("path", viewConstraint.Path);

                    foreach (string key in viewConstraint.Items.Keys)
                    {
                        writer.WriteStartElement(openEhrPrefix, "items", OpenEhrNamespace);
                        writer.WriteAttributeString("id", key);
                        writer.WriteElementString("value", viewConstraint.Items.Item(key));
                        writer.WriteEndElement();
                    }
                    writer.WriteEndElement();
                }
            }
        }

        void WriteCArchetypeRoot(XmlWriter writer, CArchetypeRoot cArchetypeRoot)
        {
            Check.Require(cArchetypeRoot != null, string.Format(CommonStrings.XMustNotBeNull, "cArchetypeRoot"));
            Check.Require(cArchetypeRoot.ArchetypeId != null, string.Format(CommonStrings.XMustNotBeNull, "cArchetypeRoot.ArchetypeId"));

            string openEhrPrefix = UseOpenEhrPrefix(writer);
            WriteCComplexObject(writer, cArchetypeRoot);

            writer.WriteStartElement(RmXmlSerializer.UseOpenEhrPrefix(writer), "archetype_id", OpenEhrNamespace);
            cArchetypeRoot.ArchetypeId.WriteXml(writer);
            writer.WriteEndElement();

            if (cArchetypeRoot.TemplateId != null)
            {
                writer.WriteStartElement(RmXmlSerializer.UseOpenEhrPrefix(writer), "template_id", OpenEhrNamespace);
                cArchetypeRoot.TemplateId.WriteXml(writer);
                writer.WriteEndElement();
            }

            if (cArchetypeRoot.TermDefinitions != null && cArchetypeRoot.TermDefinitions.Count > 0)
            {
                foreach (System.Collections.Generic.KeyValuePair<string, ArchetypeTerm> item in  cArchetypeRoot.TermDefinitions)
                {
                    writer.WriteStartElement(openEhrPrefix, "term_definitions", OpenEhrNamespace);
                    amSerializer.WriteArchetypeTerm(writer, item.Value);
                    writer.WriteEndElement();
              
                }
            }

            if (cArchetypeRoot.TermBindings != null && cArchetypeRoot.TermBindings.Count > 0)
            {
                
                foreach (string terminology in cArchetypeRoot.TermBindings.Keys)
                {
                    writer.WriteStartElement(openEhrPrefix, "term_bindings", OpenEhrNamespace);

                    writer.WriteAttributeString("terminology", terminology);

                    AssumedTypes.Hash<CodePhrase, string> eachTermBindingItem = cArchetypeRoot.TermBindings.Item(terminology);
                    
                    foreach (string code in eachTermBindingItem.Keys)
                    {
                        writer.WriteStartElement(openEhrPrefix, "items", OpenEhrNamespace);

                        writer.WriteAttributeString("code", code);

                        
                        writer.WriteStartElement(openEhrPrefix, "value", OpenEhrNamespace);
                        
                        eachTermBindingItem.Item(code).WriteXml(writer);
                        
                        writer.WriteEndElement();
                        writer.WriteEndElement();
                    }
                    writer.WriteEndElement();
                }
            }
    
        }

        void WriteCComplexObject(XmlWriter writer, CComplexObject cComplexObject)
        { 
            amSerializer.WriteCObject(writer, cComplexObject);

            string openEhrPrefix = UseOpenEhrPrefix(writer);
            if (cComplexObject.Attributes != null && cComplexObject.Attributes.Count > 0)
            {
                foreach(CAttribute attribute in cComplexObject.Attributes)
                {
                    string typeAttribute = AmType.GetName(attribute);
                    
                    if (!string.IsNullOrEmpty(openEhrPrefix))
                        typeAttribute = string.Format("{0}:{1}",
                            openEhrPrefix, typeAttribute);

                    writer.WriteStartElement(openEhrPrefix, "attributes", OpenEhrNamespace);

                    writer.WriteAttributeString(
                        UseXsiPrefix(writer), "type", XsiNamespace, typeAttribute);

                    if (AmType.GetName(attribute) == "C_MULTIPLE_ATTRIBUTE")
                        WriteCMulitpleAttribute(writer, attribute);
                    else
                        WriteCSingleAttribute(writer, attribute);

                    writer.WriteEndElement();
                }
            }
        }

        private void WriteCSingleAttribute(XmlWriter writer, CAttribute attribute)
        {
            WriteCAttribute(writer, attribute);   
        }

        private void WriteCMulitpleAttribute(XmlWriter writer, CAttribute attribute)
        {
            WriteCAttribute(writer, attribute);
            writer.WriteStartElement(
                UseOpenEhrPrefix(writer), "cardinality", RmXmlSerializer.OpenEhrNamespace);
            
            amSerializer.WriteCardinality(writer, ((CMultipleAttribute)attribute).Cardinality);
            
            writer.WriteEndElement();

        }
        
        private void WriteCAttribute(XmlWriter writer, CAttribute cAttribute)
        {
            Check.Require(cAttribute != null, string.Format(CommonStrings.XMustNotBeNull, "cAttribute"));
            Check.Require(!string.IsNullOrEmpty(cAttribute.RmAttributeName), string.Format(CommonStrings.XMustNotBeNullOrEmpty, "cAttribute.RmAttributeName"));
            Check.Require(cAttribute.Existence != null, string.Format(CommonStrings.XMustNotBeNull, "cAttribute.Existence"));

            string openEhrPrefix = UseOpenEhrPrefix(writer);
            writer.WriteElementString(openEhrPrefix, "rm_attribute_name",
                OpenEhrNamespace, cAttribute.RmAttributeName);

            writer.WriteStartElement(openEhrPrefix, "existence", OpenEhrNamespace);
            amSerializer.WriteExistence(writer, cAttribute.Existence);
            
            writer.WriteEndElement();

            if (cAttribute.Children != null)
            {
                foreach (CObject child in cAttribute.Children)
                {
                    string cChildTypeName = AmType.GetName(child);
                    if (!string.IsNullOrEmpty(openEhrPrefix))
                        cChildTypeName = string.Format("{0}:{1}", openEhrPrefix, cChildTypeName);

                    writer.WriteStartElement(openEhrPrefix, "children", OpenEhrNamespace);
                    writer.WriteAttributeString(
                        UseXsiPrefix(writer), "type", RmXmlSerializer.XsiNamespace, cChildTypeName);

                    switch (cChildTypeName)
                    { 
                        case "C_ARCHETYPE_ROOT" :
                            WriteCArchetypeRoot(writer, (CArchetypeRoot)child);
                            break;

                        case "C_COMPLEX_OBJECT" :
                            WriteCComplexObject(writer, (CComplexObject)child);
                            break;

                        default:
                            amSerializer.WriteCObject(writer, child);
                            break;
                    }
                    writer.WriteEndElement();
                }
            }
        }
    }
}