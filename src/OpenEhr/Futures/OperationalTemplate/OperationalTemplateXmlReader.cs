using System;
using System.Xml;

using OpenEhr.DesignByContract;
using OpenEhr.RM.DataTypes.Text;
using OpenEhr.AssumedTypes;
using OpenEhr.AM.Archetype.ConstraintModel;
using OpenEhr.RM.DataTypes.Basic;
using OpenEhr.AM.OpenehrProfile.DataTypes.Text;
using OpenEhr.RM.Support.Identification;
using OpenEhr.RM.Support.Terminology;
using OpenEhr.RM.Support.Terminology.Impl;
using OpenEhr.Resources;
using OpenEhr.Serialisation;
using OpenEhr.Factories;
using OpenEhr.RM.Common.Generic;
using OpenEhr.RM.Common.Resource;

namespace OpenEhr.Futures.OperationalTemplate
{
    class OperationalTemplateXmlReader : RmXmlSerializer
    {
        AmXmlSerializer amSerializer = new AmXmlSerializer();

        public void ReadOperationalTemplate(XmlReader reader, OperationalTemplate template)
        {
            Check.Require(reader != null, string.Format(CommonStrings.XMustNotBeNull, "reader"));
            Check.Require(template != null, string.Format(CommonStrings.XMustNotBeNull, "template"));

            reader.ReadStartElement();  //template
            reader.MoveToContent();

            CodePhrase language = new CodePhrase();
            language.ReadXml(reader);
            template.Language = language;

            if (reader.LocalName == "is_controlled")
            {
                reader.ReadElementContentAsBoolean("is_controlled", RmXmlSerializer.OpenEhrNamespace);
                reader.MoveToContent();
            }

            if (reader.LocalName == "description")
            {
                ResourceDescription description = new ResourceDescription();
                description.ReadXml(reader);
                template.Description = description;
            }

            if (reader.LocalName == "revision_history")
            {
                RevisionHistory revisionHistory = new RevisionHistory();
                revisionHistory.ReadXml(reader);
                template.RevisionHistory = revisionHistory;
            }

            if (reader.LocalName == "uid")
            {
                HierObjectId uid = new HierObjectId();
                uid.ReadXml(reader);
                template.Uid = uid;
            }

            TemplateId templateId = new TemplateId();
            templateId.ReadXml(reader);
            template.TemplateId = templateId;

            template.Concept = reader.ReadElementContentAsString("concept", RmXmlSerializer.OpenEhrNamespace);
            reader.MoveToContent();

            CArchetypeRoot definition = new CArchetypeRoot();
            ReadCArchetypeRoot(reader, definition);
            template.Definition = definition;

            // LMT added 12 May 2010 for TMP-1252
            if (reader.LocalName == "annotations")
            {
                template.Annotations = 
                    new List<OpenEhr.RM.Common.Resource.Annotation>();
                OpenEhr.RM.Common.Resource.Annotation currentAnnotation;
                while (reader.LocalName == "annotations")
                {
                    currentAnnotation = ReadAnnotation(reader);
                    template.Annotations.Add(currentAnnotation);
                    reader.MoveToContent();
                }
            }

            if (reader.LocalName == "constraints")
            {
                TConstraint constraints = new TConstraint();
                ReadTConstraint(reader, constraints);
                template.Constraints = constraints;
            }

            if (reader.LocalName == "view")
            {
                TView view = new TView();
                ReadTView(reader, view);
                template.View = view;
            }

            reader.ReadEndElement();    // template
            reader.MoveToContent();
        }

        // LMT added 12 May 2010 for TMP-1252
        private OpenEhr.RM.Common.Resource.Annotation ReadAnnotation(XmlReader reader)
        {
            string annotationPath = reader.GetAttribute("path"); //read path
            reader.ReadStartElement(); // annotation
            reader.MoveToContent();

            // Read child items
            Check.Assert(reader.LocalName == "items", "Expected at least one 'items' child inside annotation");
            System.Collections.Generic.Dictionary<string, string> annotationsDictionary 
                = new System.Collections.Generic.Dictionary<string,string>();
            while (reader.LocalName == "items")
            {
                string annotationsItemKey = reader.GetAttribute("id");
                string annotationsItemValue = reader.ReadElementContentAsString();
                annotationsDictionary.Add(annotationsItemKey, annotationsItemValue);
                reader.MoveToContent();
            }

            OpenEhr.RM.Common.Resource.Annotation annotation =
                new OpenEhr.RM.Common.Resource.Annotation(annotationsDictionary, annotationPath);
            reader.ReadEndElement();
            return annotation;
        }

        private void ReadTConstraint(XmlReader reader, TConstraint constraints)
        {
            Check.Require(constraints != null, string.Format(CommonStrings.XMustNotBeNull, "constraints"));
            List<TAttribute> attributeList = new List<TAttribute>();
            reader.ReadStartElement(); // constraints
            reader.MoveToContent();

            while (reader.LocalName == "attributes")
            {
                TAttribute attribute = new TAttribute();
                ReadTAttribute(reader, attribute);
                attributeList.Add(attribute);
                reader.MoveToContent();
                reader.ReadEndElement();
                reader.MoveToContent();
            }

            constraints.Attributes = attributeList;
        }

        private void ReadTAttribute(XmlReader reader, TAttribute attribute)
        {
            Check.Require(attribute != null);

            reader.ReadStartElement(); // attributes
            reader.MoveToContent();

            // Read rm_attribute_name
            Check.Assert(reader.LocalName == "rm_attribute_name");
            attribute.RmAttributeName = reader.ReadElementContentAsString();
            reader.MoveToContent();

            // Read children
            Check.Assert(reader.LocalName == "children", "Expected at least one child in T_ATTRIBUTE");
            attribute.Children = new List<TComplexObject>();
            while (reader.LocalName == "children")
            {
                TComplexObject tComplexObject = new TComplexObject();
                ReadTComplexObject(reader, tComplexObject);
                attribute.Children.Add(tComplexObject);
                reader.ReadEndElement();
                reader.MoveToContent();
            }
            
            // Read differential path
            Check.Assert(reader.LocalName == "differential_path");
            attribute.DifferentialPath = reader.ReadElementContentAsString();
        }

        private void ReadTComplexObject(XmlReader reader, TComplexObject tComplexObject)
        {
            Check.Require(tComplexObject != null, string.Format(CommonStrings.XMustNotBeNull, "tComplexObject"));

            ReadCComplexObject(reader, tComplexObject);

            // Read default_value
            if (reader.LocalName == "default_value")
            {
                string rmType = ReadXsiType(reader);
                Check.Assert(!string.IsNullOrEmpty(rmType), "xsi:type should not be null or empty");
                DataValue defaultValue = RmFactory.DataValue(rmType);
                defaultValue.ReadXml(reader);
                tComplexObject.DefaultValue = defaultValue;
            }
        }

        private void ReadTView(XmlReader reader, TView view)
        {
            Check.Require(view != null, string.Format(CommonStrings.XMustNotBeNull, "view"));
            
            List<TViewConstraint> viewConstraints = new List<TViewConstraint>();

            reader.ReadStartElement(); // view
            reader.MoveToContent();
            while (reader.LocalName == "constraints")
            {
                TViewConstraint viewConstraint = new TViewConstraint();

                viewConstraint.Path = reader.GetAttribute("path");
                Check.Assert(!string.IsNullOrEmpty(viewConstraint.Path), "Path must not be null or empty.");

                reader.ReadStartElement(); // constraints
                reader.MoveToContent();
                if (reader.LocalName != "items")
                    throw new InvalidXmlException("items", reader.LocalName);

                System.Collections.Generic.Dictionary<string, string> hashTable =
                    new System.Collections.Generic.Dictionary<string, string>();
                while (reader.LocalName == "items")
                {
                    string hashId = reader.GetAttribute("id");
                    reader.ReadStartElement();
                    reader.MoveToContent();
                    string hashValue = reader.ReadElementContentAsString();
                    hashTable.Add(hashId, hashValue);
                    reader.ReadEndElement();
                    reader.MoveToContent();
                } 

                viewConstraint.Items = 
                    new OpenEhr.AssumedTypes.Hash<string, string>(hashTable);
                viewConstraints.Add(viewConstraint);

                Check.Assert(reader.NodeType == System.Xml.XmlNodeType.EndElement,
                  "Expected endElement");
                reader.ReadEndElement();
                reader.MoveToContent();
            }

            view.Constraints = viewConstraints;
        }

        void ReadCArchetypeRoot(XmlReader reader, CArchetypeRoot node)
        {
            ReadCComplexObject(reader, node);

            ArchetypeId archetypeId = new ArchetypeId();
            archetypeId.ReadXml(reader);
            node.ArchetypeId = archetypeId;

            if (reader.LocalName == "template_id")
            {
                TemplateId templateId = new TemplateId();
                templateId.ReadXml(reader);
                node.TemplateId = templateId;
            }

            if (reader.LocalName == "default_values")
                throw new NotImplementedException(CommonStrings.DefaultsSerialisationNotImplemented);

            if (reader.LocalName == "term_definitions")
            {
                System.Collections.Generic.Dictionary<string, AM.Archetype.Ontology.ArchetypeTerm> termDefimitionItemDic
                    = new System.Collections.Generic.Dictionary<string, AM.Archetype.Ontology.ArchetypeTerm>();

                while (reader.LocalName == "term_definitions")
                {
                    AM.Archetype.Ontology.ArchetypeTerm archetypeTerm = new AM.Archetype.Ontology.ArchetypeTerm();
                    amSerializer.ReadArchetypeTerm(reader, archetypeTerm);

                    termDefimitionItemDic.Add(archetypeTerm.Code, archetypeTerm);
                }
                node.TermDefinitions
                    = new Hash<AM.Archetype.Ontology.ArchetypeTerm, string>(termDefimitionItemDic);
            }

            if (reader.LocalName == "term_bindings")
            {
                if (reader.IsEmptyElement)
                {
                    reader.Skip();
                    reader.MoveToContent();
                }
                else
                {
                    System.Collections.Generic.Dictionary<string, Hash<CodePhrase, string>> termBindingDic =
                        new System.Collections.Generic.Dictionary<string, Hash<CodePhrase, string>>();
                    do
                    {
                        System.Collections.Generic.Dictionary<string, CodePhrase> termBindingItemDic
                            = new System.Collections.Generic.Dictionary<string, CodePhrase>();

                        string terminologyString = reader.GetAttribute("terminology");
                        DesignByContract.Check.Assert(!string.IsNullOrEmpty(terminologyString), "terminologyString must not be null or empty.");

                        reader.ReadStartElement();
                        reader.MoveToContent();

                        while (reader.LocalName == "items")
                        {
                            string hashId = reader.GetAttribute("code");

                            reader.ReadStartElement();
                            reader.MoveToContent();

                            CodePhrase codePhrase = new CodePhrase();
                            codePhrase.ReadXml(reader);

                            reader.ReadEndElement();
                            reader.MoveToContent();

                            termBindingItemDic.Add(hashId, codePhrase);
                        }

                        reader.ReadEndElement();
                        reader.MoveToContent();

                        DesignByContract.Check.Assert(termBindingItemDic.Count > 0, "termBindingItemDic must not be empty.");

                        Hash<CodePhrase, string> termBindingItemHash
                            = new Hash<CodePhrase, string>(termBindingItemDic);

                        termBindingDic.Add(terminologyString, termBindingItemHash);
                    } while (reader.LocalName == "term_bindings");

                    DesignByContract.Check.Assert(termBindingDic.Count > 0, "termBindingDic must not be empty.");

                    node.TermBindings = new Hash<Hash<CodePhrase, string>, string>(termBindingDic);
                }
            }

            Check.Assert(reader.NodeType == System.Xml.XmlNodeType.EndElement, "Expected endElement of CArchetypeRoot");

            reader.ReadEndElement();
            reader.MoveToContent();
        }

        void ReadCComplexObject(XmlReader reader, AM.Archetype.ConstraintModel.CComplexObject cComplexObject)
        {
            Check.Require(cComplexObject != null, string.Format(CommonStrings.XMustNotBeNull, "cComplexObject"));

            reader.ReadStartElement();
            reader.MoveToContent();

            amSerializer.ReadComplexObject(reader, cComplexObject);

            if (reader.LocalName == "attributes")
            {
                System.Collections.Generic.List<CAttribute> attrList = new System.Collections.Generic.List<CAttribute>();
                do
                {
                    string attributeType = reader.GetAttribute("type", RmXmlSerializer.XsiNamespace);
                    DesignByContract.Check.Assert(!string.IsNullOrEmpty(attributeType), "attributeType must not be null or empty.");

                    CAttribute attri = AmFactory.CAttribute(attributeType);
                    attri.parent = cComplexObject;

                    CMultipleAttribute multipleAttribute = attri as CMultipleAttribute;
                    if (multipleAttribute != null)
                        ReadCMultipleAttribute(reader, multipleAttribute);
                    else
                        ReadCSingleAttribute(reader, attri as CSingleAttribute);

                    //Special attention being paid to media types due to erronous codes in archetypes produced by AE

					// %HYYKA%
                    // TODO: this must be reinstated when ConfigurationSource is able to be specified
                    //if (attri.RmAttributeName == "media_type")
                    //    ProcessMediaType(attri as CSingleAttribute);

                    attrList.Add(attri);

                } while (reader.LocalName == "attributes" && reader.NodeType != XmlNodeType.EndElement);

                if (cComplexObject.RmTypeName == "ITEM_TABLE")
                { 
                    //remove spurious attributes
                    for (int i = attrList.Count-1; i >= 0; i--)
                    { 
                        string rmAttributeName = attrList[i].RmAttributeName;
                        if (rmAttributeName == "rotated" || rmAttributeName == "number_key_columns")
                        {
                            attrList.RemoveAt(i);
                        }
                    }

                }

                Check.Assert(attrList.Count > 0, "attrList must not be empty.");

                cComplexObject.Attributes = new OpenEhr.AssumedTypes.Set<CAttribute>(attrList);
            }


            if (reader.NodeType == System.Xml.XmlNodeType.EndElement)
            {
                reader.ReadEndElement();

                reader.MoveToContent();
            }

        }


        #region Mapping of openEHR media codes to IANA codeset codes Mulimedia

		// TODO: this must be reinstated when ConfigurationSource is able to be specified

        #endregion



        void ReadCAttribute(XmlReader reader, CAttribute cAttribute)
        {
            Check.Require(cAttribute != null, string.Format(CommonStrings.XMustNotBeNull, "cAttribute"));

            if (reader.LocalName != "rm_attribute_name")
                throw new InvalidXmlException("rm_attribute_name", reader.LocalName);
            cAttribute.RmAttributeName = reader.ReadElementContentAsString("rm_attribute_name", OpenEhrNamespace);
            reader.MoveToContent();

            if (reader.LocalName != "existence")
                throw new InvalidXmlException("existence", reader.LocalName);
            cAttribute.Existence = new Interval<int>();
            amSerializer.ReadExistence(reader, cAttribute.Existence);

            if (reader.LocalName == "children")
            {
                cAttribute.Children = new OpenEhr.AssumedTypes.List<CObject>();
                do
                {
                    string cObjectType = reader.GetAttribute("type", RmXmlSerializer.XsiNamespace);
                    DesignByContract.Check.Assert(!string.IsNullOrEmpty(cObjectType), "cObjectType must not be null or empty.");

                    CObject cObj;
                    switch (cObjectType)
                    {
                        case "C_ARCHETYPE_ROOT":

                            CArchetypeRoot archetypeRoot = new CArchetypeRoot();
                            ReadCArchetypeRoot(reader, archetypeRoot);
                            cObj = archetypeRoot;
                            break;

                        case "C_COMPLEX_OBJECT":
                            CComplexObject complexObject = new CComplexObject();
                            ReadCComplexObject(reader, complexObject);
                            cObj = complexObject;
                            break;

                        default:
                            cObj = AmFactory.CObject(cObjectType);
                            amSerializer.ReadCObject(reader, cObj);
                            break;
                    }

                    cObj.Parent = cAttribute;
                    cAttribute.Children.Add(cObj);

                } while (reader.LocalName == "children" && reader.NodeType != XmlNodeType.EndElement);
            }
        }

        void ReadCMultipleAttribute(XmlReader reader, CMultipleAttribute cMultipleAttribute)
        {
            Check.Require(cMultipleAttribute != null, string.Format(CommonStrings.XMustNotBeNull, "cMultipleAttribute"));
            
            reader.ReadStartElement();
            reader.MoveToContent();

            ReadCAttribute(reader, cMultipleAttribute);

            if (reader.LocalName != "cardinality")
                throw new InvalidXmlException("cardinality" + reader.LocalName);

            cMultipleAttribute.Cardinality = new Cardinality();
            amSerializer.ReadCardinality(reader, cMultipleAttribute.Cardinality);

            DesignByContract.Check.Assert(reader.NodeType == System.Xml.XmlNodeType.EndElement,
             "Expected endElement");
            reader.ReadEndElement();

            reader.MoveToContent();
        }

        void ReadCSingleAttribute(XmlReader reader, CSingleAttribute cSingleAttribute)
        {
            Check.Require(cSingleAttribute != null, string.Format(CommonStrings.XMustNotBeNull, "cSingleAttribute"));

            reader.ReadStartElement();
            reader.MoveToContent();

            ReadCAttribute(reader, cSingleAttribute);

            DesignByContract.Check.Assert(reader.NodeType == System.Xml.XmlNodeType.EndElement, "Expected endElement");
            reader.ReadEndElement();

            reader.MoveToContent();
        }
    }
}