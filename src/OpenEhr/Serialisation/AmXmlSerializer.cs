using System;
using System.Collections.Generic;
using System.Text;
using OpenEhr.RM.Common.Resource;
using OpenEhr.AM.Archetype.ConstraintModel;
using System.Xml;
using OpenEhr.DesignByContract;
using OpenEhr.RM.Support.Identification;
using OpenEhr.AM.Archetype.Assertion;
using OpenEhr.AM.Archetype.Ontology;
using OpenEhr.AM.Archetype.ConstraintModel.Primitive;
using OpenEhr.AssumedTypes;
using OpenEhr.AM.Archetype;
using OpenEhr.RM.DataTypes.Text;
using OpenEhr.RM.DataTypes.Uri;
using OpenEhr.AM.OpenehrProfile.DataTypes.Text;
using OpenEhr.AM.OpenehrProfile.DataTypes.Quantity;
using OpenEhr.RM.DataTypes.Quantity;
using OpenEhr.AM.OpenehrProfile.DataTypes.Basic;
using OpenEhr.RM.DataTypes.Basic;
using OpenEhr.RM.DataTypes.Quantity.DateTime;
using OpenEhr.RM.Support.Terminology.Impl;
using OpenEhr.RM.Support.Terminology;
using OpenEhr.Resources;
using OpenEhr.Validation;
using OpenEhr.Factories;
using OpenEhr.AM.Impl;

namespace OpenEhr.Serialisation
{
    class AmXmlSerializer: RmXmlSerializer
    {
        private XmlReader reader;
        private XmlWriter writer;

        #region static members

        public const string XsdNamespace = "http://www.w3.org/2001/XMLSchema";

        private static System.Xml.Schema.XmlSchema archetypeSchema = null;

        public static void LoadArchetypeSchema(System.Xml.Schema.XmlSchemaSet xs)
        {
            if (!xs.Contains(OpenEhrNamespace))
            {
                archetypeSchema = GetOpenEhrSchema("Archetype");

                System.Xml.Schema.XmlSchema resourceSchema = GetOpenEhrSchema("Resource");
                System.Xml.Schema.XmlSchemaInclude schemaInclude;

                archetypeSchema.Includes.RemoveAt(0);

                foreach (System.Xml.Schema.XmlSchemaObject item in resourceSchema.Items)
                    archetypeSchema.Items.Add(item);

                System.Xml.Schema.XmlSchema baseTypesSchema = GetOpenEhrSchema("BaseTypes");

                foreach (System.Xml.Schema.XmlSchemaObject item in baseTypesSchema.Items)
                    archetypeSchema.Items.Add(item);

                xs.Add(archetypeSchema);

                xs.Compile();
            }
        }

        #endregion

        #region ReadXml
        private Archetype archetype;

        public void ReadArchetype(XmlReader reader, Archetype archetype)
        {
            DesignByContract.Check.Require(reader != null, string.Format(CommonStrings.XMustNotBeNull, "reader"));
            DesignByContract.Check.Require(archetype != null, string.Format(CommonStrings.XMustNotBeNull, "archetype"));

            this.reader = reader;
            this.archetype = archetype;

            if (reader.NodeType == System.Xml.XmlNodeType.None)
                reader.MoveToContent();

            reader.ReadStartElement();
            reader.MoveToContent();

           ((AuthoredResource)(archetype)).ReadXml(reader);

            if (reader.LocalName == "uid")
            {
                archetype.Uid = new HierObjectId();
                archetype.Uid.ReadXml(reader);
            }

            if (reader.LocalName != "archetype_id")
                throw new InvalidXmlException("archetype_id", reader.LocalName);
            archetype.ArchetypeId = new ArchetypeId();
            archetype.ArchetypeId.ReadXml(reader);

            if (reader.LocalName == "adl_version")
            {
                archetype.AdlVersion = reader.ReadElementContentAsString("adl_version", OpenEhrNamespace);
                reader.MoveToContent();
            }

            if (reader.LocalName != "concept")
                throw new InvalidXmlException("concept", reader.LocalName);
            archetype.Concept = reader.ReadElementContentAsString("concept", OpenEhrNamespace);
            reader.MoveToContent();

            if (reader.LocalName == "parent_archetype_id")
            {
                archetype.ParentArchetypeId = new ArchetypeId();
                archetype.ParentArchetypeId.ReadXml(reader);
            }

            if (reader.LocalName != "definition")
                throw new InvalidXmlException("definition", reader.LocalName);
            archetype.Definition = new CComplexObject();
            this.ReadXml(archetype.Definition);

            if (reader.LocalName == "invariants")
            {
                System.Collections.Generic.List<Assertion> invariantsList =
                    new System.Collections.Generic.List<OpenEhr.AM.Archetype.Assertion.Assertion>();
                do
                {
                    Assertion assertion = new OpenEhr.AM.Archetype.Assertion.Assertion();
                    this.ReadXml(assertion);

                    invariantsList.Add(assertion);
                } while (reader.LocalName == "invariants");

                DesignByContract.Check.Assert(invariantsList.Count > 0, "invariantsList must not be empty.");

                archetype.Invariants = new AssumedTypes.Set<OpenEhr.AM.Archetype.Assertion.Assertion>(invariantsList);
            }

            if (reader.LocalName != "ontology")
                throw new ValidationException(string.Format(CommonStrings.ExpectedLocalNameIsXNotY, "ontology", reader.LocalName));
            archetype.Ontology = new ArchetypeOntology();
            this.ReadXml(archetype.Ontology);
            archetype.Ontology.ParentArchetype = archetype;
            archetype.Ontology.SpecialisationDepth = AM.Archetype.Ontology.ArcheytpeTermCodeTools.SpecialisationDepthFromCode(archetype.Definition.NodeId);

            DesignByContract.Check.Assert(reader.NodeType == System.Xml.XmlNodeType.EndElement,
              "Expected endElement");
            reader.ReadEndElement();

            reader.MoveToContent();
        }

 

        #region ReadXml - ArchetypeConstraint

        public void ReadCObject(XmlReader reader, CObject cObject)
        {
            this.reader = reader;
            ReadXml(cObject);
        }

        private System.Reflection.MethodInfo lastMethodReadCObject = null;
        private CObject lastCObjectRead = null;
        private void ReadXml(CObject cObject)
        {
            if (cObject == null) throw new ArgumentNullException(string.Format(CommonStrings.XIsNull, "cObject"));

            const string methodName = "ReadXml";

            try
            {
                System.Reflection.MethodInfo method = this.GetType().GetMethod(methodName,
                    System.Reflection.BindingFlags.ExactBinding | System.Reflection.BindingFlags.NonPublic
                    | System.Reflection.BindingFlags.Instance, Type.DefaultBinder,
                               new Type[] { cObject.GetType() },
                               new System.Reflection.ParameterModifier[0]);

                if (method != null)
                {
                    if (method != lastMethodReadCObject || cObject!= lastCObjectRead)
                    {
                        lastMethodReadCObject = method;
                        lastCObjectRead = cObject;

                        method.Invoke(this, new Object[] { cObject });

                    }
                    else
                    {
                        string message = string.Format(CommonStrings.LoopingMethodTerminated, methodName, cObject.GetType().ToString());
                        System.Diagnostics.Debug.WriteLine(message);
                        throw new ApplicationException(message);
                    }
                }
                else
                {
                    string message = string.Format(CommonStrings.MethodXNotImplementedForParamTypeY, methodName, cObject.GetType().ToString());
                    System.Diagnostics.Debug.WriteLine(message);
                    throw new ApplicationException(message);
                }
            }
            catch (System.Reflection.TargetInvocationException ex)
            {
                if (ex.InnerException != null)
                    throw new ApplicationException(ex.InnerException.Message, ex.InnerException);
                else
                    throw new ApplicationException(ex.Message, ex);
            }
        }

        private void ReadXml(ArchetypeInternalRef archetypeInternalRef)
        {
            Check.Require(archetypeInternalRef!= null, string.Format(CommonStrings.XMustNotBeNull, "archetypeInternalRef"));

            reader.ReadStartElement();
            reader.MoveToContent();

            this.ReadXmlBase((CObject)archetypeInternalRef);

            if (reader.LocalName != "target_path")
                throw new InvalidXmlException("target_path", reader.LocalName);
            archetypeInternalRef.TargetPath = reader.ReadElementContentAsString("target_path", OpenEhrNamespace);
            reader.MoveToContent();

            DesignByContract.Check.Assert(!string.IsNullOrEmpty(archetypeInternalRef.TargetPath), "TargetPath must not be null or empty.");
            DesignByContract.Check.Assert(reader.NodeType == System.Xml.XmlNodeType.EndElement, "Expected endElement");

            reader.ReadEndElement();
            reader.MoveToContent();

            this.archetype.ConstraintRepository.Add(archetypeInternalRef.Path, this.archetype.GetCDefinedObjectAtPath(archetypeInternalRef.TargetPath));
        }

        private void ReadXml(ConstraintRef constraintRef)
        {
            Check.Require(constraintRef != null, string.Format(CommonStrings.XMustNotBeNull, "constraintRef"));

            reader.ReadStartElement();
            reader.MoveToContent();

            this.ReadXmlBase((CObject)constraintRef);

            string openEhrNamespace = RmXmlSerializer.OpenEhrNamespace;
            if (reader.LocalName != "reference")
                throw new InvalidXmlException("reference", reader.LocalName);
            constraintRef.Reference = reader.ReadElementContentAsString("reference", openEhrNamespace);
            reader.MoveToContent();

            DesignByContract.Check.Assert(!string.IsNullOrEmpty(constraintRef.Reference), "Reference must not be null or empty.");
            DesignByContract.Check.Assert(reader.NodeType == System.Xml.XmlNodeType.EndElement, "Expected endElement");

            reader.ReadEndElement();
            reader.MoveToContent();
        }

        private void ReadXml(CComplexObject cComplexObject)
        {
            DesignByContract.Check.Require(cComplexObject != null, string.Format(CommonStrings.XMustNotBeNull, "cComplexObject"));

            reader.ReadStartElement();
            reader.MoveToContent();

            this.ReadXmlBase((CObject)cComplexObject);

            if (reader.LocalName == "attributes")
            {
                System.Collections.Generic.List<CAttribute> attrList = new System.Collections.Generic.List<CAttribute>();
                do
                {
                    string attributeType = reader.GetAttribute("type", RmXmlSerializer.XsiNamespace);
                    DesignByContract.Check.Assert(!string.IsNullOrEmpty(attributeType), "attributeType must not be null or empty.");

                    CAttribute attri = AmFactory.CAttribute(attributeType);
                    attri.parent = cComplexObject;
                    this.ReadXml(attri);
                    attrList.Add(attri);                   

                } while (reader.LocalName == "attributes" && reader.NodeType != XmlNodeType.EndElement);

                DesignByContract.Check.Assert(attrList.Count > 0, "attrList must not be empty.");

                cComplexObject.Attributes = new OpenEhr.AssumedTypes.Set<CAttribute>(attrList);
            }

            DesignByContract.Check.Assert(reader.NodeType == System.Xml.XmlNodeType.EndElement, "Expected endElement of CComplextObject");

            reader.ReadEndElement();

            reader.MoveToContent();

            this.archetype.ConstraintRepository.Add(cComplexObject.Path, cComplexObject);
        }  

        private void ReadXml(CPrimitiveObject cPrimitiveObj)
        {
            Check.Require(cPrimitiveObj != null, string.Format(CommonStrings.XMustNotBeNull, "cPrimitiveObj"));

            reader.ReadStartElement();
            reader.MoveToContent();

            this.ReadXmlBase((CObject)cPrimitiveObj);
            if (reader.LocalName != "item")
                throw new InvalidXmlException("item", reader.LocalName);
            string itemType = reader.GetAttribute("type", XsiNamespace);
            Check.Assert(!string.IsNullOrEmpty(itemType), "itemType must not be null or empty.");
            cPrimitiveObj.Item = AmFactory.CPrimitive(itemType);

            this.ReadXml(cPrimitiveObj.Item);

            DesignByContract.Check.Assert(reader.NodeType == System.Xml.XmlNodeType.EndElement, "Expected endElement");
            reader.ReadEndElement();

            reader.MoveToContent();

            if (this.archetype != null)
                this.archetype.ConstraintRepository.Add(cPrimitiveObj.Path, cPrimitiveObj);
        }

        private void ReadXml(ArchetypeSlot archetypeSlot)
        {
            Check.Require(archetypeSlot != null, string.Format(CommonStrings.XMustNotBeNull, "archetypeSlot"));

            reader.ReadStartElement();
            reader.MoveToContent();

            this.ReadXmlBase((CObject)archetypeSlot);
            
            if (reader.LocalName == "includes")
            {
                System.Collections.Generic.List<Assertion> includesList = new System.Collections.Generic.List<Assertion>();
                do
                {
                    Assertion assertion = new OpenEhr.AM.Archetype.Assertion.Assertion();
                    this.ReadXml(assertion);
                    includesList.Add(assertion);

                } while (reader.LocalName == "includes");

                DesignByContract.Check.Assert(includesList.Count > 0, "includesList must not be empty.");

                archetypeSlot.Includes = new OpenEhr.AssumedTypes.Set<OpenEhr.AM.Archetype.Assertion.Assertion>(includesList);
            }

            if (reader.LocalName == "excludes")
            {
                System.Collections.Generic.List<Assertion> excludesList = new System.Collections.Generic.List<Assertion>();
                do
                {
                    Assertion assertion = new OpenEhr.AM.Archetype.Assertion.Assertion();
                    this.ReadXml(assertion);
                    excludesList.Add(assertion);

                } while (reader.LocalName == "excludes");

                DesignByContract.Check.Assert(excludesList.Count > 0, "excludesList must not be empty.");

                archetypeSlot.Excludes = new OpenEhr.AssumedTypes.Set<OpenEhr.AM.Archetype.Assertion.Assertion>(excludesList);
            }

            DesignByContract.Check.Assert(reader.NodeType == System.Xml.XmlNodeType.EndElement, "Expected endElement of ArchetypeSlot");
            reader.ReadEndElement();

            reader.MoveToContent();
        }

        private System.Reflection.MethodInfo lastMethodReadCAttribute = null;
        private CAttribute lastCAttributeRead = null;
        private void ReadXml(CAttribute cAttribute)
        {
            if (cAttribute == null) 
                throw new ArgumentNullException(string.Format(CommonStrings.XIsNull, "cAttribute"));

            const string methodName = "ReadXml";

            try
            {
                System.Reflection.MethodInfo method = this.GetType().GetMethod(methodName,
                    System.Reflection.BindingFlags.ExactBinding | System.Reflection.BindingFlags.NonPublic
                    | System.Reflection.BindingFlags.Instance, Type.DefaultBinder,
                               new Type[] { cAttribute.GetType() },
                               new System.Reflection.ParameterModifier[0]);

                if (method != null)
                {
                    if (method != lastMethodReadCAttribute || cAttribute!= lastCAttributeRead)
                    {
                        lastMethodReadCAttribute = method;
                        lastCAttributeRead = cAttribute;

                        method.Invoke(this, new Object[] { cAttribute });

                    }
                    else
                    {
                        string message = string.Format(CommonStrings.LoopingMethodTerminated, 
                            methodName, cAttribute.GetType().ToString());
                        System.Diagnostics.Debug.WriteLine(message);
                        throw new ApplicationException(message);
                    }
                }
                else
                {
                    string message = string.Format(CommonStrings.MethodXNotImplementedForParamTypeY,
                        methodName, cAttribute.GetType().ToString());
                    System.Diagnostics.Debug.WriteLine(message);
                    throw new ApplicationException(message);
                }
            }
            catch (System.Reflection.TargetInvocationException ex)
            {
                if (ex.InnerException != null)
                    throw new ApplicationException(ex.InnerException.Message, ex.InnerException);
                else
                    throw new ApplicationException(ex.Message, ex);
            }
        }

        private void ReadXml(CMultipleAttribute cMultipleAttribute)
        {
            Check.Require(cMultipleAttribute != null, string.Format(CommonStrings.XMustNotBeNull, "cMultipleAttribute"));

            reader.ReadStartElement();
            reader.MoveToContent();

            this.ReadXmlBase((CAttribute)cMultipleAttribute);

            if (reader.LocalName != "cardinality")
                throw new InvalidXmlException("cardinality" + reader.LocalName);

            cMultipleAttribute.Cardinality = new Cardinality();
            this.ReadXml(cMultipleAttribute.Cardinality);

            DesignByContract.Check.Assert(reader.NodeType == System.Xml.XmlNodeType.EndElement, "Expected endElement");
            reader.ReadEndElement();

            reader.MoveToContent();
        }

        private void ReadXml(CSingleAttribute cSingleAttribute)
        {
            Check.Require(cSingleAttribute != null, string.Format(CommonStrings.XMustNotBeNull, "cSingleAttribute"));

            reader.ReadStartElement();
            reader.MoveToContent();

            this.ReadXmlBase((CAttribute)cSingleAttribute);

            DesignByContract.Check.Assert(reader.NodeType == System.Xml.XmlNodeType.EndElement, "Expected endElement");
            reader.ReadEndElement();

            reader.MoveToContent();
        }

        public void ReadCardinality(XmlReader reader, Cardinality cardinality)
        {
            this.reader = reader;
            ReadXml(cardinality);
        }

        private void ReadXml(Cardinality cardinality)
        {
            Check.Require(cardinality != null, string.Format(CommonStrings.XMustNotBeNull, "cardinality"));

            reader.ReadStartElement();
            reader.MoveToContent();

            Check.Assert(reader.LocalName == "is_ordered", "local name must be 'is_ordered' rather than " + reader.LocalName);
            cardinality.IsOrdered = reader.ReadElementContentAsBoolean("is_ordered", OpenEhrNamespace);
            reader.MoveToContent();

            Check.Assert(reader.LocalName == "is_unique", "local name must be 'is_unique' rather than " + reader.LocalName);
            cardinality.IsUnique = reader.ReadElementContentAsBoolean("is_unique", OpenEhrNamespace);
            reader.MoveToContent();

            if (reader.LocalName != "interval")
            {
                throw new InvalidXmlException("interval", reader.LocalName);
            }
            cardinality.Interval = new Interval<int>();
            this.ReadXml(cardinality.Interval);

            DesignByContract.Check.Assert(reader.NodeType == System.Xml.XmlNodeType.EndElement, "Expected endElement of cardinality");
            reader.ReadEndElement();
            reader.MoveToContent();
        }

        #endregion

        #region ReadXml - Ontology

        private void ReadXml(ArchetypeOntology archetypeOntology)
        {
            Check.Require(archetypeOntology!= null, "archetypeOntology must not be null.");

            reader.ReadStartElement();
            reader.MoveToContent();

            if (reader.LocalName == "term_definitions")
            {

                System.Collections.Generic.Dictionary<string, AssumedTypes.Hash<ArchetypeTerm, string>> termDefinitionsDic =
                    new Dictionary<string, OpenEhr.AssumedTypes.Hash<ArchetypeTerm, string>>();
                do
                {
                    string language = reader.GetAttribute("language");

                    reader.ReadStartElement();
                    reader.MoveToContent();

                    Dictionary<string, ArchetypeTerm> termDefimitionItemDic = new Dictionary<string, ArchetypeTerm>();

                    while (reader.LocalName == "items")
                    {
                        ArchetypeTerm archetypeTerm = new ArchetypeTerm();
                        this.ReadXml(archetypeTerm);

                        termDefimitionItemDic.Add(archetypeTerm.Code, archetypeTerm);
                    }

                    reader.ReadEndElement();
                    reader.MoveToContent();

                    AssumedTypes.Hash<ArchetypeTerm, string> archetypeTermHash = new OpenEhr.AssumedTypes.Hash<ArchetypeTerm, string>(termDefimitionItemDic);

                    termDefinitionsDic.Add(language, archetypeTermHash);


                } while (reader.LocalName == "term_definitions");

                DesignByContract.Check.Assert(termDefinitionsDic.Count > 0, "termDefinitionDic must not be emppty.");

                archetypeOntology.TermDefinitions = new OpenEhr.AssumedTypes.Hash<OpenEhr.AssumedTypes.Hash<ArchetypeTerm, string>, string>(termDefinitionsDic);
            }

            if (reader.LocalName == "constraint_definitions")
            {

                System.Collections.Generic.Dictionary<string, AssumedTypes.Hash<ArchetypeTerm, string>> constraintDefinitionsDic =
                    new Dictionary<string, OpenEhr.AssumedTypes.Hash<ArchetypeTerm, string>>();
                do
                {
                    string language = reader.GetAttribute("language");

                    Dictionary<string, ArchetypeTerm> constraintDefItemDic = new Dictionary<string, ArchetypeTerm>();
                    reader.ReadStartElement();
                    reader.MoveToContent();

                    while (reader.LocalName == "items")
                    {
                        ArchetypeTerm archetypeTerm = new ArchetypeTerm();
                        this.ReadXml(archetypeTerm);

                        constraintDefItemDic.Add(archetypeTerm.Code, archetypeTerm);
                    }

                    reader.ReadEndElement();
                    reader.MoveToContent();

                    AssumedTypes.Hash<ArchetypeTerm, string> constraintDefItemHash =
                        new OpenEhr.AssumedTypes.Hash<ArchetypeTerm, string>(constraintDefItemDic);

                    constraintDefinitionsDic.Add(language, constraintDefItemHash);

                } while (reader.LocalName == "constraint_definitions");

                DesignByContract.Check.Assert(constraintDefinitionsDic.Count > 0, "termDefinitionDic must not be emppty.");

                archetypeOntology.ConstraintDefinitions = new OpenEhr.AssumedTypes.Hash<OpenEhr.AssumedTypes.Hash<ArchetypeTerm, string>, string>(constraintDefinitionsDic);
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
                    System.Collections.Generic.Dictionary<string, AssumedTypes.Hash<CodePhrase, string>> termBindingDic =
                        new Dictionary<string, OpenEhr.AssumedTypes.Hash<CodePhrase, string>>();
                    do
                    {
                        Dictionary<string, CodePhrase> termBindingItemDic = new Dictionary<string, CodePhrase>();

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

                        AssumedTypes.Hash<CodePhrase, string> termBindingItemHash =
                            new OpenEhr.AssumedTypes.Hash<CodePhrase, string>(termBindingItemDic);

                        termBindingDic.Add(terminologyString, termBindingItemHash);
                    } while (reader.LocalName == "term_bindings");

                    DesignByContract.Check.Assert(termBindingDic.Count > 0, "termBindingDic must not be empty.");

                    archetypeOntology.TermBindings = new OpenEhr.AssumedTypes.Hash<OpenEhr.AssumedTypes.Hash<CodePhrase, string>, string>(termBindingDic);
                }
            }

            if (reader.LocalName == "constraint_bindings")
            {
                System.Collections.Generic.Dictionary<string, AssumedTypes.Hash<DvUri, string>> constraintBindingDic =
                    new Dictionary<string, OpenEhr.AssumedTypes.Hash<DvUri, string>>();
                do
                {
                    Dictionary<string, DvUri> constraintBindingItemDic = new Dictionary<string, DvUri>();

                    string terminologyString = reader.GetAttribute("terminology");
                    DesignByContract.Check.Assert(!string.IsNullOrEmpty(terminologyString), "terminologyString must not be null or empty.");

                    reader.ReadStartElement();
                    reader.MoveToContent();

                    while (reader.LocalName == "items")
                    {

                        string hashId = reader.GetAttribute("code");
                        DvUri dvUri = new DvUri();
                        dvUri.ReadXml(reader);

                        constraintBindingItemDic.Add(hashId, dvUri);
                    }

                    reader.ReadEndElement();
                    reader.MoveToContent();

                    DesignByContract.Check.Assert(constraintBindingItemDic.Count > 0, "constraintBindingItemDic must not be empty.");

                    AssumedTypes.Hash<DvUri, string> constraintBindingItemHash =
                        new OpenEhr.AssumedTypes.Hash<DvUri, string>(constraintBindingItemDic);

                    constraintBindingDic.Add(terminologyString, constraintBindingItemHash);
                } while (reader.LocalName == "constraint_bindings");

                DesignByContract.Check.Assert(constraintBindingDic.Count > 0, "termBindingDic must not be empty.");

                archetypeOntology.ConstraintBindings = new OpenEhr.AssumedTypes.Hash<OpenEhr.AssumedTypes.Hash<DvUri, string>, string>(constraintBindingDic);
            }


            DesignByContract.Check.Assert(reader.NodeType == System.Xml.XmlNodeType.EndElement,
              "Expected endElement");
            reader.ReadEndElement();
            reader.MoveToContent();
        }

        public void ReadArchetypeTerm(XmlReader reader, ArchetypeTerm archetypeTerm)
        {
            this.reader = reader;
            ReadXml(archetypeTerm);
        }

        private void ReadXml(ArchetypeTerm archetypeTerm)
        {
            Check.Require(archetypeTerm!= null, "archetypeTerm must not be null.");
            
            string code = reader.GetAttribute("code");
            DesignByContract.Check.Assert(!string.IsNullOrEmpty(code), "code must not be null or empty.");
            archetypeTerm.Code = code;

            reader.ReadStartElement();
            reader.MoveToContent();

            if (reader.LocalName != "items")
                throw new InvalidXmlException("items", reader.LocalName);

            System.Collections.Generic.Dictionary<string, string> hashTable = new Dictionary<string, string>();
            do
            {
                string hashId = reader.GetAttribute("id");
                string hashValue = reader.ReadElementContentAsString("items", OpenEhrNamespace);
                reader.MoveToContent();
                hashTable.Add(hashId, hashValue);
            } while (reader.LocalName == "items" && reader.NodeType != XmlNodeType.EndElement);

            archetypeTerm.Items = new OpenEhr.AssumedTypes.Hash<string, string>(hashTable);


            DesignByContract.Check.Assert(reader.NodeType == System.Xml.XmlNodeType.EndElement,
              "Expected endElement");
            reader.ReadEndElement();
            reader.MoveToContent();
        }
        #endregion

        #region ReadXml - Assertion

        private void ReadXml(Assertion assertion)
        {
            Check.Require(assertion!=null, "assertion must not be null.");

            reader.ReadStartElement();
            reader.MoveToContent();

            if (reader.LocalName == "tag")
            {
                assertion.Tag = reader.ReadElementContentAsString("tag", OpenEhrNamespace);
                reader.MoveToContent();
            }

            if (reader.LocalName == "string_expression")
            {
                assertion.StringExpression = reader.ReadElementContentAsString("string_expression", OpenEhrNamespace);
                reader.MoveToContent();

            }

            if (reader.LocalName != "expression")
                throw new InvalidXmlException("expression" + reader.LocalName);
            string expressionType = reader.GetAttribute("type", XsiNamespace);
            ExprItem expression = AmFactory.ExprItem(expressionType);           
            this.ReadXml(expression);
            assertion.Expression = expression;

            if (reader.LocalName == "variables")
            {
                assertion.Variables = new OpenEhr.AssumedTypes.List<AssertionVariable>();
                do
                {
                    AssertionVariable variable = new AssertionVariable();
                    this.ReadXml(variable);

                    assertion.Variables.Add(variable);
                } while (reader.LocalName == "variables");

                DesignByContract.Check.Assert(assertion.Variables.Count > 0, "variableList must not be empty.");
            }

            DesignByContract.Check.Assert(reader.NodeType == System.Xml.XmlNodeType.EndElement,
              "Expected endElement of Assertion");
            reader.ReadEndElement();
            reader.MoveToContent();
        }

        private void ReadXml(AssertionVariable assertionVariable)
        {
            Check.Require(assertionVariable != null, "assertionVariable must not be null.");

            reader.ReadStartElement();
            reader.MoveToContent();

            if (reader.LocalName != "name")
                throw new InvalidXmlException("name" + reader.LocalName);
            assertionVariable.Name = reader.ReadElementContentAsString("name", OpenEhrNamespace);
            reader.MoveToContent();

            if (reader.LocalName != "definition")
                throw new InvalidXmlException("definition" + reader.LocalName);
            assertionVariable.Definition = reader.ReadElementContentAsString("definition", OpenEhrNamespace);
            reader.MoveToContent();

            DesignByContract.Check.Assert(reader.NodeType == System.Xml.XmlNodeType.EndElement,
              "Expected endElement of AssertionVairable");
            reader.ReadEndElement();
            reader.MoveToContent();
        }

        private System.Reflection.MethodInfo lastMethodReadExprItem = null;
        private ExprItem lastExprItemRead = null;
        private void ReadXml(ExprItem exprItem)
        {
            if (exprItem == null) throw new ArgumentNullException("exprItem");

            const string methodName = "ReadXml";

            try
            {
                System.Reflection.MethodInfo method = this.GetType().GetMethod(methodName,
                    System.Reflection.BindingFlags.ExactBinding | System.Reflection.BindingFlags.NonPublic
                    | System.Reflection.BindingFlags.Instance, Type.DefaultBinder,
                               new Type[] { exprItem.GetType() },
                               new System.Reflection.ParameterModifier[0]);

                if (method != null)
                {
                    if (method != lastMethodReadExprItem || exprItem != lastExprItemRead)
                    {
                        lastMethodReadExprItem = method;
                        lastExprItemRead = exprItem;

                        method.Invoke(this, new Object[] { exprItem });

                    }
                    else
                    {
                        string message = string.Format(CommonStrings.LoopingMethodTerminated,
                            methodName, exprItem.GetType().ToString()); 
                        System.Diagnostics.Debug.WriteLine(message);
                        throw new ApplicationException(message);
                    }
                }
                else
                {
                    string message = string.Format(CommonStrings.MethodXNotImplementedForParamTypeY,
                            methodName, exprItem.GetType().ToString()); 
                    System.Diagnostics.Debug.WriteLine(message);
                    throw new ApplicationException(message);
                }
            }
            catch (System.Reflection.TargetInvocationException ex)
            {
                if (ex.InnerException != null)
                    throw new ApplicationException(ex.InnerException.Message, ex.InnerException);
                else
                    throw new ApplicationException(ex.Message, ex);
            }
        }

        private void ReadXml(ExprLeaf exprLeaf)
        {
            reader.ReadStartElement();
            reader.MoveToContent();

            this.ReadXmlBase((ExprItem)exprLeaf);

            if (reader.LocalName != "item")
                throw new InvalidXmlException("item", reader.LocalName);
           
            exprLeaf.Item = GetAnyObject(exprLeaf.Type, reader.LocalName);
            reader.MoveToContent();

            if (reader.LocalName != "reference_type")
                throw new InvalidXmlException("reference_type", reader.LocalName);
            exprLeaf.ReferenceType = reader.ReadElementContentAsString("reference_type", OpenEhrNamespace);
            reader.MoveToContent();

            reader.ReadEndElement();
            reader.MoveToContent();
        }

        private object GetAnyObject(string objectType, string elementName)
        {
            // C_PRIMITIVE types
            if (objectType.StartsWith("C_"))
            {
                CPrimitive cPrimitive = AmFactory.CPrimitive(objectType);
                this.ReadXml(cPrimitive);
                return cPrimitive;
            }

            // primitive types
            switch (objectType.ToLower())
            {
                case "string":
                    return reader.ReadElementString(elementName, OpenEhrNamespace);
                case "integer":
                    return reader.ReadElementContentAsInt(elementName, OpenEhrNamespace);
                case "boolean":
                    return reader.ReadElementContentAsBoolean(elementName, OpenEhrNamespace);
                case "double":
                    return reader.ReadElementContentAsDouble(elementName, OpenEhrNamespace);
                case "decimal":
                    return reader.ReadElementContentAsDecimal(elementName, OpenEhrNamespace);
              
                default:
                    throw new NotSupportedException("type is not supported: "+objectType);
            }
        }

        private void ReadXml(ExprUnaryOperator exprUnaryOperator)
        {
            reader.ReadStartElement();
            reader.MoveToContent();

            this.ReadXmlBase((ExprOperator)exprUnaryOperator);

            if (reader.LocalName != "operand")
                throw new InvalidXmlException("operand" + reader.LocalName);
            string operandType = reader.GetAttribute("type", XsiNamespace);
            Check.Assert(!string.IsNullOrEmpty(operandType), "operandType must not be null or empty.");
            exprUnaryOperator.Operand = AmFactory.ExprItem(operandType);
            this.ReadXml(exprUnaryOperator.Operand);

            if (reader.LocalName != "precedence_overridden")
                throw new InvalidXmlException("precedence_overridden" + reader.LocalName);
            exprUnaryOperator.PrecedenceOverriden = reader.ReadElementContentAsBoolean("precedence_overridden", OpenEhrNamespace);

            DesignByContract.Check.Assert(reader.NodeType == System.Xml.XmlNodeType.EndElement,
              "Expected endElement of AssertionVairable");
            reader.ReadEndElement();
            reader.MoveToContent();

        }

        private void ReadXml(ExprBinaryOperator exprBinaryOperator)
        {
            reader.ReadStartElement();
            reader.MoveToContent();

            this.ReadXmlBase((ExprOperator)exprBinaryOperator);

            if (reader.LocalName != "left_operand")
                throw new InvalidXmlException("left_operand" + reader.LocalName);
            string leftOperandType = reader.GetAttribute("type", XsiNamespace);
            Check.Assert(!string.IsNullOrEmpty(leftOperandType), "leftOperandType must not be null or empty.");
            exprBinaryOperator.LeftOperand = AmFactory.ExprItem(leftOperandType);
            this.ReadXml(exprBinaryOperator.LeftOperand);

            if (reader.LocalName != "right_operand")
                throw new InvalidXmlException("right_operand" + reader.LocalName);
            string rightOperandType = reader.GetAttribute("type", XsiNamespace);
            Check.Assert(!string.IsNullOrEmpty(rightOperandType), "rightOperandType must not be null or empty.");
            exprBinaryOperator.RightOperand = AmFactory.ExprItem(rightOperandType);
            this.ReadXml(exprBinaryOperator.RightOperand);

            DesignByContract.Check.Assert(reader.NodeType == System.Xml.XmlNodeType.EndElement,
              "Expected endElement of AssertionVairable");
            reader.ReadEndElement();
            reader.MoveToContent();

        }
        #endregion

        #region ReadXml - Interval

        public void ReadExistence(XmlReader reader, AssumedTypes.Interval<int> interval)
        {
            this.reader = reader;
            ReadXml(interval);
        }

        private void ReadXml(AssumedTypes.Interval<int> interval)
        {
            Check.Require(interval != null, "interval must not be null.");

            reader.ReadStartElement();
            reader.MoveToContent();

            if (reader.LocalName == "lower_included")
            {
                interval.LowerIncluded = reader.ReadElementContentAsBoolean("lower_included", OpenEhrNamespace);
                reader.MoveToContent();
            }

            if (reader.LocalName == "upper_included")
            {
                interval.UpperIncluded = reader.ReadElementContentAsBoolean("upper_included", OpenEhrNamespace);
                reader.MoveToContent();
            }

            if (reader.LocalName != "lower_unbounded")
                throw new ValidationException("expected node name is 'lower_unbounded', not " + reader.LocalName);
            interval.LowerUnbounded = reader.ReadElementContentAsBoolean("lower_unbounded", OpenEhrNamespace);
            reader.MoveToContent();

            if (reader.LocalName != "upper_unbounded")
                throw new ValidationException("expected node name is 'upper_unbounded', not " + reader.LocalName);
            interval.UpperUnbounded = reader.ReadElementContentAsBoolean("upper_unbounded", OpenEhrNamespace);
            reader.MoveToContent();

            if (reader.LocalName == "lower")
            {
                interval.Lower = reader.ReadElementContentAsInt("lower", OpenEhrNamespace);
                reader.MoveToContent();
            }

            if (reader.LocalName == "upper")
            {
                interval.Upper = reader.ReadElementContentAsInt("upper", OpenEhrNamespace);
                reader.MoveToContent();
            }

            DesignByContract.Check.Assert(reader.NodeType == System.Xml.XmlNodeType.EndElement,
            "Expected endElement of interval");
            reader.ReadEndElement();

            reader.MoveToContent();
        }

        private void ReadXml(AssumedTypes.Interval<float> interval)
        {
            Check.Require(interval != null, "interval must not be null.");

            reader.ReadStartElement();
            reader.MoveToContent();

            if (reader.LocalName == "lower_included")
            {
                interval.LowerIncluded = reader.ReadElementContentAsBoolean("lower_included", OpenEhrNamespace);
                reader.MoveToContent();
            }

            if (reader.LocalName == "upper_included")
            {
                interval.UpperIncluded = reader.ReadElementContentAsBoolean("upper_included", OpenEhrNamespace);
                reader.MoveToContent();
            }

            if (reader.LocalName != "lower_unbounded")
                throw new ValidationException("expected node name is 'lower_unbounded', not " + reader.LocalName);
            interval.LowerUnbounded = reader.ReadElementContentAsBoolean("lower_unbounded", OpenEhrNamespace);
            reader.MoveToContent();

            if (reader.LocalName != "upper_unbounded")
                throw new ValidationException("expected node name is 'upper_unbounded', not " + reader.LocalName);
            interval.UpperUnbounded = reader.ReadElementContentAsBoolean("upper_unbounded", OpenEhrNamespace);
            reader.MoveToContent();


            if (reader.LocalName == "lower")
            {
                interval.Lower = reader.ReadElementContentAsFloat("lower", OpenEhrNamespace);
                reader.MoveToContent();
            }

            if (reader.LocalName == "upper")
            {
                interval.Upper = reader.ReadElementContentAsFloat("upper", OpenEhrNamespace);
                reader.MoveToContent();
            }

            DesignByContract.Check.Assert(reader.NodeType == System.Xml.XmlNodeType.EndElement,
            "Expected endElement of interval");
            reader.ReadEndElement();

            reader.MoveToContent();
        }

        private void ReadXml(AssumedTypes.Interval<Iso8601Date> interval)
        {
            Check.Require(interval != null, "interval must not be null.");

            reader.ReadStartElement();
            reader.MoveToContent();

            if (reader.LocalName == "lower_included")
            {
                interval.LowerIncluded = reader.ReadElementContentAsBoolean("lower_included", OpenEhrNamespace);
                reader.MoveToContent();
            }

            if (reader.LocalName == "upper_included")
            {
                interval.UpperIncluded = reader.ReadElementContentAsBoolean("upper_included", OpenEhrNamespace);
                reader.MoveToContent();
            }

            if (reader.LocalName != "lower_unbounded")
                throw new ValidationException("expected node name is 'lower_unbounded', not " + reader.LocalName);
            interval.LowerUnbounded = reader.ReadElementContentAsBoolean("lower_unbounded", OpenEhrNamespace);
            reader.MoveToContent();

            if (reader.LocalName != "upper_unbounded")
                throw new ValidationException("expected node name is 'upper_unbounded', not " + reader.LocalName);
            interval.UpperUnbounded = reader.ReadElementContentAsBoolean("upper_unbounded", OpenEhrNamespace);
            reader.MoveToContent();

            if (reader.LocalName == "lower")
            {
                interval.Lower = new Iso8601Date(reader.ReadElementContentAsString("lower", OpenEhrNamespace));
                reader.MoveToContent();
            }

            if (reader.LocalName == "upper")
            {
                interval.Upper = new Iso8601Date(reader.ReadElementContentAsString("upper", OpenEhrNamespace));
                reader.MoveToContent();
            }

            DesignByContract.Check.Assert(reader.NodeType == System.Xml.XmlNodeType.EndElement,
            "Expected endElement of interval");
            reader.ReadEndElement();

            reader.MoveToContent();
        }

        private void ReadXml(AssumedTypes.Interval<Iso8601DateTime> interval)
        {
            Check.Require(interval != null, "interval must not be null.");

            reader.ReadStartElement();
            reader.MoveToContent();

            if (reader.LocalName == "lower_included")
            {
                interval.LowerIncluded = reader.ReadElementContentAsBoolean("lower_included", OpenEhrNamespace);
                reader.MoveToContent();
            }

            if (reader.LocalName == "upper_included")
            {
                interval.UpperIncluded = reader.ReadElementContentAsBoolean("upper_included", OpenEhrNamespace);
                reader.MoveToContent();
            }

            if (reader.LocalName != "lower_unbounded")
                throw new ValidationException("expected node name is 'lower_unbounded', not " + reader.LocalName);
            interval.LowerUnbounded = reader.ReadElementContentAsBoolean("lower_unbounded", OpenEhrNamespace);
            reader.MoveToContent();

            if (reader.LocalName != "upper_unbounded")
                throw new ValidationException("expected node name is 'upper_unbounded', not " + reader.LocalName);
            interval.UpperUnbounded = reader.ReadElementContentAsBoolean("upper_unbounded", OpenEhrNamespace);
            reader.MoveToContent();

            if (reader.LocalName == "lower")
            {
                interval.Lower = new Iso8601DateTime(reader.ReadElementContentAsString("lower", OpenEhrNamespace));
                reader.MoveToContent();
            }

            if (reader.LocalName == "upper")
            {
                interval.Upper = new Iso8601DateTime(reader.ReadElementContentAsString("upper", OpenEhrNamespace));
                reader.MoveToContent();
            }

            DesignByContract.Check.Assert(reader.NodeType == System.Xml.XmlNodeType.EndElement,
            "Expected endElement of interval");
            reader.ReadEndElement();

            reader.MoveToContent();
        }

        private void ReadXml(AssumedTypes.Interval<Iso8601Time> interval)
        {
            Check.Require(interval != null, "interval must not be null.");

            reader.ReadStartElement();
            reader.MoveToContent();

            if (reader.LocalName == "lower_included")
            {
                interval.LowerIncluded = reader.ReadElementContentAsBoolean("lower_included", OpenEhrNamespace);
                reader.MoveToContent();
            }

            if (reader.LocalName == "upper_included")
            {
                interval.UpperIncluded = reader.ReadElementContentAsBoolean("upper_included", OpenEhrNamespace);
                reader.MoveToContent();
            }

            if (reader.LocalName != "lower_unbounded")
                throw new ValidationException("expected node name is 'lower_unbounded', not " + reader.LocalName);
            interval.LowerUnbounded = reader.ReadElementContentAsBoolean("lower_unbounded", OpenEhrNamespace);
            reader.MoveToContent();

            if (reader.LocalName != "upper_unbounded")
                throw new ValidationException("expected node name is 'upper_unbounded', not " + reader.LocalName);
            interval.UpperUnbounded = reader.ReadElementContentAsBoolean("upper_unbounded", OpenEhrNamespace);
            reader.MoveToContent();

            if (reader.LocalName == "lower")
            {
                interval.Lower = new Iso8601Time(reader.ReadElementContentAsString("lower", OpenEhrNamespace));
                reader.MoveToContent();
            }

            if (reader.LocalName == "upper")
            {
                interval.Upper = new Iso8601Time(reader.ReadElementContentAsString("upper", OpenEhrNamespace));
                reader.MoveToContent();
            }

            DesignByContract.Check.Assert(reader.NodeType == System.Xml.XmlNodeType.EndElement,
            "Expected endElement of interval");
            reader.ReadEndElement();

            reader.MoveToContent();
        }

        private void ReadXml(AssumedTypes.Interval<Iso8601Duration> interval)
        {
            Check.Require(interval != null, "interval must not be null.");

            reader.ReadStartElement();
            reader.MoveToContent();

            if (reader.LocalName == "lower_included")
            {
                interval.LowerIncluded = reader.ReadElementContentAsBoolean("lower_included", OpenEhrNamespace);
                reader.MoveToContent();
            }

            if (reader.LocalName == "upper_included")
            {
                interval.UpperIncluded = reader.ReadElementContentAsBoolean("upper_included", OpenEhrNamespace);
                reader.MoveToContent();
            }

            if (reader.LocalName != "lower_unbounded")
                throw new ValidationException("expected node name is 'lower_unbounded', not " + reader.LocalName);
            interval.LowerUnbounded = reader.ReadElementContentAsBoolean("lower_unbounded", OpenEhrNamespace);
            reader.MoveToContent();

            if (reader.LocalName != "upper_unbounded")
                throw new ValidationException("expected node name is 'upper_unbounded', not " + reader.LocalName);
            interval.UpperUnbounded = reader.ReadElementContentAsBoolean("upper_unbounded", OpenEhrNamespace);
            reader.MoveToContent();

            if (reader.LocalName == "lower")
            {
                interval.Lower = new Iso8601Duration(reader.ReadElementContentAsString("lower", OpenEhrNamespace));
                reader.MoveToContent();
            }

            if (reader.LocalName == "upper")
            {
                interval.Upper = new Iso8601Duration(reader.ReadElementContentAsString("upper", OpenEhrNamespace));
                reader.MoveToContent();
            }

            DesignByContract.Check.Assert(reader.NodeType == System.Xml.XmlNodeType.EndElement,
            "Expected endElement of interval");
            reader.ReadEndElement();

            reader.MoveToContent();
        }
            
        #endregion

        #region ReadXml - CPrimitive
        private System.Reflection.MethodInfo lastMethodReadCPrimitive = null;
        private CPrimitive lastCPrimitiveRead = null;
        private void ReadXml(CPrimitive cPrimitive)
        {
            if (cPrimitive == null) 
                throw new ArgumentNullException("cPrimitive must not be null.");

            const string methodName = "ReadXml";

            try
            {
                System.Reflection.MethodInfo method = this.GetType().GetMethod(methodName,
                    System.Reflection.BindingFlags.ExactBinding | System.Reflection.BindingFlags.NonPublic
                    | System.Reflection.BindingFlags.Instance, Type.DefaultBinder,
                               new Type[] { cPrimitive.GetType() },
                               new System.Reflection.ParameterModifier[0]);

                if (method != null)
                {
                    // Avoid StackOverflow exceptions by executing only if the method and visitable  
                    // are different from the last parameters used.
                    if (method != lastMethodReadCPrimitive || cPrimitive!= lastCPrimitiveRead)
                    {
                        lastMethodReadCPrimitive = method;
                        lastCPrimitiveRead = cPrimitive;

                        method.Invoke(this, new Object[] { cPrimitive });

                    }
                    else
                    {
                        string message = string.Format(CommonStrings.LoopingMethodTerminated,
                            methodName, cPrimitive.GetType().ToString());
                        System.Diagnostics.Debug.WriteLine(message);
                        throw new ApplicationException(message);
                    }
                }
                else
                {
                    string message = string.Format(CommonStrings.MethodXNotImplementedForParamTypeY,
                        methodName, cPrimitive.GetType().ToString());
                    System.Diagnostics.Debug.WriteLine(message);
                    throw new ApplicationException(message);
                }
            }
            catch (System.Reflection.TargetInvocationException ex)
            {
                if (ex.InnerException != null)
                    throw new ApplicationException(ex.InnerException.Message, ex.InnerException);
                else
                    throw new ApplicationException(ex.Message, ex);
            }
        }

        private void ReadXml(CBoolean cBoolean)
        {
            Check.Require(cBoolean != null, "cBoolean must not be null.");

            reader.ReadStartElement();
            reader.MoveToContent();

            if (reader.LocalName != "true_valid")
                throw new InvalidXmlException("true_valid", reader.LocalName);
            cBoolean.TrueValid = reader.ReadElementContentAsBoolean("true_valid", OpenEhrNamespace);
            reader.MoveToContent();

            if (reader.LocalName != "false_valid")
                throw new InvalidXmlException("false_valid", reader.LocalName);
            cBoolean.FalseValid = reader.ReadElementContentAsBoolean("false_valid", OpenEhrNamespace);
            reader.MoveToContent();

            if (reader.LocalName == "assumed_value")
            {
                cBoolean.AssumedValue = reader.ReadElementContentAsBoolean("assumed_value", OpenEhrNamespace);
                reader.MoveToContent();
                cBoolean.assumedValueSet = true;
            }

            reader.ReadEndElement();
            reader.MoveToContent();
        }

        private void ReadXml(CDate cDate)
        {
            Check.Require(cDate != null, "cDate must not be null.");

            reader.ReadStartElement();
            reader.MoveToContent();

            if (reader.LocalName == "pattern")
            {
                cDate.Pattern = reader.ReadElementContentAsString("pattern", OpenEhrNamespace);
                reader.MoveToContent();
            }

            if (reader.LocalName == "timezone_validity")
            {
                cDate.TimezoneValidity = new ValidityKind(reader.ReadElementContentAsInt("timezone_validity", OpenEhrNamespace));
                reader.MoveToContent();
            }

            if (reader.LocalName == "range")
            {
                cDate.Range = new Interval<Iso8601Date>();
                this.ReadXml(cDate.Range);
            }

            if (reader.LocalName == "assumed_value")
            {
                cDate.AssumedValue = new AssumedTypes.Iso8601Date(reader.ReadElementContentAsString("assumed_value", OpenEhrNamespace));
                reader.MoveToContent();
            }

            reader.ReadEndElement();
            reader.MoveToContent();
        }

        private void ReadXml(CDateTime cDateTime)
        {
            reader.ReadStartElement();
            reader.MoveToContent();

            if (reader.LocalName == "pattern")
            {
                cDateTime.Pattern = reader.ReadElementContentAsString("pattern", OpenEhrNamespace);
                reader.MoveToContent();
            }

            if (reader.LocalName == "timezone_validity")
            {
                cDateTime.TimezoneValidity = new ValidityKind(reader.ReadElementContentAsInt("timezone_validity", OpenEhrNamespace));
                reader.MoveToContent();
            }

            if (reader.LocalName == "range")
            {
                cDateTime.Range = new Interval<Iso8601DateTime>();
                this.ReadXml(cDateTime.Range);
            }

            if (reader.LocalName == "assumed_value")
            {
                cDateTime.AssumedValue = new AssumedTypes.Iso8601DateTime(reader.ReadElementContentAsString("assumed_value", OpenEhrNamespace));
                reader.MoveToContent();
            }

            reader.ReadEndElement();
            reader.MoveToContent();
        }

        private void ReadXml(CTime cTime)
        {
            reader.ReadStartElement();
            reader.MoveToContent();

            string openEhrNamespace = RmXmlSerializer.OpenEhrNamespace;

            if (reader.LocalName == "pattern")
            {
                cTime.Pattern = reader.ReadElementContentAsString("pattern", openEhrNamespace);
                reader.MoveToContent();
            }

            if (reader.LocalName == "timezone_validity")
            {
                cTime.TimezoneValidity = new ValidityKind(reader.ReadElementContentAsInt("timezone_validity", openEhrNamespace));
                reader.MoveToContent();
            }

            if (reader.LocalName == "range")
            {
                cTime.Range = new Interval<Iso8601Time>();
                this.ReadXml(cTime.Range);
            }

            if (reader.LocalName == "assumed_value")
            {
                cTime.AssumedValue = new AssumedTypes.Iso8601Date(reader.ReadElementContentAsString("assumed_value", openEhrNamespace));
                reader.MoveToContent();
            }

            reader.ReadEndElement();
            reader.MoveToContent();
        }

        private void ReadXml(CDuration cDuration)
        {
            if (reader.IsEmptyElement)
            {
                cDuration.AllowAny();
                reader.Skip();
            }
            else
            {
                reader.ReadStartElement();
                reader.MoveToContent();

                string openEhrNamespace = RmXmlSerializer.OpenEhrNamespace;

                if (reader.LocalName == "pattern")
                {
                    cDuration.Pattern = reader.ReadElementContentAsString("pattern", openEhrNamespace);
                    reader.MoveToContent();
                }

                if (reader.LocalName == "range")
                {
                    cDuration.Range = new Interval<Iso8601Duration>();
                    this.ReadXml(cDuration.Range);
                }

                if (reader.LocalName == "assumed_value")
                {
                    cDuration.AssumedValue = new AssumedTypes.Iso8601Duration(reader.ReadElementContentAsString("assumed_value", openEhrNamespace));
                    reader.MoveToContent();
                }

                reader.ReadEndElement();
            }

            reader.MoveToContent();
        }

        private void ReadXml(CInteger cInteger)
        {
            reader.ReadStartElement();
            reader.MoveToContent();

            if (reader.LocalName == "list")
            {
                System.Collections.Generic.List<int> intList = new System.Collections.Generic.List<int>();
                do
                {
                    intList.Add(reader.ReadElementContentAsInt("list", OpenEhrNamespace));
                    reader.MoveToContent();
                } while (reader.LocalName == "list");

                if (intList.Count > 0)
                    cInteger.List = new Set<int>(intList);
            }

            if (reader.LocalName == "range")
            {
                cInteger.Range = new Interval<int>();
                this.ReadXml(cInteger.Range);
            }

            if (reader.LocalName == "assumed_value")
            {
                cInteger.AssumedValue = reader.ReadElementContentAsInt("assumed_value", OpenEhrNamespace);
                reader.MoveToContent();
            }

            reader.ReadEndElement();
            reader.MoveToContent();
        }

        private void ReadXml(CReal cReal)
        {
            reader.ReadStartElement();
            reader.MoveToContent();

            if (reader.LocalName == "list")
            {
                System.Collections.Generic.List<float> floatList = new System.Collections.Generic.List<float>();
                do
                {
                    floatList.Add(reader.ReadElementContentAsFloat("list", OpenEhrNamespace));
                    reader.MoveToContent();
                } while (reader.LocalName == "list");

                if (floatList.Count > 0)
                    cReal.List = new Set<float>(floatList);
            }

            if (reader.LocalName == "range")
            {
                cReal.Range = new Interval<float>();
                this.ReadXml(cReal.Range);
            }

            if (reader.LocalName == "assumed_value")
            {
                cReal.AssumedValue = reader.ReadElementContentAsInt("assumed_value", OpenEhrNamespace);
                reader.MoveToContent();
            }

            reader.ReadEndElement();
            reader.MoveToContent();
        }

        private void ReadXml(CString cString)
        {
            reader.ReadStartElement();
            reader.MoveToContent();

            if (reader.LocalName == "pattern")
            {
                cString.Pattern = reader.ReadElementContentAsString("pattern", OpenEhrNamespace);
                reader.MoveToContent();
            }

            if (reader.LocalName == "list")
            {
                System.Collections.Generic.List<string> stringList = new System.Collections.Generic.List<string>();
                do
                {
                    stringList.Add(reader.ReadElementContentAsString("list", OpenEhrNamespace));
                    reader.MoveToContent();
                } while (reader.LocalName == "list");

                if (stringList.Count > 0)
                    cString.List = new Set<string>(stringList);
            }

            if (reader.LocalName == "list_open")
            {
                cString.ListOpen = reader.ReadElementContentAsBoolean("list_open", OpenEhrNamespace);
                reader.MoveToContent(); 
                cString.listOpenSet = true;
            }

            if (reader.LocalName == "assumed_value")
            {
                cString.AssumedValue = reader.ReadElementContentAsString("assumed_value", OpenEhrNamespace);
                reader.MoveToContent();
            }

            reader.ReadEndElement();
            reader.MoveToContent();
        }

        #endregion

        #region ReadXml - CDomaintype & OpenehrProfile
        private System.Reflection.MethodInfo lastMethodReadCDomainType = null;
        private CDomainType lastCDomainTypeRead = null;
        private void ReadXml(CDomainType cDomainType)
        {
            if (cDomainType == null)
                throw new ArgumentNullException("cDomainType must not be null.");

            const string methodName = "ReadXml";

            try
            {
                System.Reflection.MethodInfo method = this.GetType().GetMethod(methodName,
                    System.Reflection.BindingFlags.ExactBinding | System.Reflection.BindingFlags.NonPublic
                    | System.Reflection.BindingFlags.Instance, Type.DefaultBinder,
                               new Type[] { cDomainType.GetType() },
                               new System.Reflection.ParameterModifier[0]);

                if (method != null)
                {
                    // Avoid StackOverflow exceptions by executing only if the method and visitable  
                    // are different from the last parameters used.
                    if (method != lastMethodReadCDomainType || cDomainType != lastCDomainTypeRead)
                    {
                        lastMethodReadCDomainType = method;
                        lastCDomainTypeRead = cDomainType;

                        method.Invoke(this, new Object[] { cDomainType });

                    }
                    else
                    {
                        string message = string.Format(CommonStrings.LoopingMethodTerminated,
                            methodName, cDomainType.GetType().ToString());
                        System.Diagnostics.Debug.WriteLine(message);
                        throw new ApplicationException(message);
                    }
                }
                else
                {
                    string message = string.Format(CommonStrings.MethodXNotImplementedForParamTypeY,
                        methodName, cDomainType.GetType().ToString());
                    System.Diagnostics.Debug.WriteLine(message);
                    throw new ApplicationException(message);
                }
            }
            catch (System.Reflection.TargetInvocationException ex)
            {
                if (ex.InnerException != null)
                    throw new ApplicationException(ex.InnerException.Message, ex.InnerException);
                else
                    throw new ApplicationException(ex.Message, ex);
            }

            this.archetype.ConstraintRepository.Add(cDomainType.Path, cDomainType);
        }

        private void ReadXml(CCodePhrase cDomainType)
        {
            reader.ReadStartElement();
            reader.MoveToContent();

            this.ReadXmlBase((CObject)cDomainType);

            if (reader.LocalName == "assumed_value")
            {
                CodePhrase assumedValue = new CodePhrase();
                assumedValue.ReadXml(reader);
                cDomainType.AssumedValue = assumedValue;
            }

            if (reader.LocalName == "terminology_id")
            {
                cDomainType.TerminologyId = new TerminologyId();
                cDomainType.TerminologyId.ReadXml(reader);
            }


            if (reader.LocalName == "code_list")
            {
                OpenEhr.AssumedTypes.List<string> codeList = new OpenEhr.AssumedTypes.List<string>();
                do
                {
                    string codeString = reader.ReadElementContentAsString("code_list", OpenEhrNamespace);
                    reader.MoveToContent();                    
                    codeList.Add(codeString);
                } while (reader.LocalName == "code_list");

                Check.Assert(!codeList.IsEmpty(), "codeList must not be empty.");

                cDomainType.CodeList = codeList;
            }

            reader.ReadEndElement();
            reader.MoveToContent();
        }

        private void ReadXml(CDvOrdinal cDomainType)
        {
            reader.ReadStartElement();
            reader.MoveToContent();

            this.ReadXmlBase((CObject)cDomainType);

            if (reader.LocalName == "assumed_value")
            {
                DvOrdinal assumedValue = new DvOrdinal();
                assumedValue.ReadXml(reader);
                cDomainType.AssumedValue = assumedValue;
            }
            
            if (reader.LocalName == "list")
            {
                System.Collections.Generic.List<DvOrdinal> ordinalList =
                    new System.Collections.Generic.List<DvOrdinal>();
                do
                {
                    DvOrdinal ordinal = new DvOrdinal();
                    ordinal.ReadXml(reader);
                    ordinalList.Add(ordinal);
                } while (reader.LocalName == "list" && reader.NodeType== XmlNodeType.Element);

                Check.Assert(ordinalList.Count>0, "ordinalList may not be empty.");
                cDomainType.List = new Set<DvOrdinal>(ordinalList);
            }

            reader.ReadEndElement();
            reader.MoveToContent();
        }

        private void ReadXml(CDvQuantity cDomainType)
        {
            reader.ReadStartElement();
            reader.MoveToContent();

            this.ReadXmlBase((CObject)cDomainType);

            if (reader.LocalName == "assumed_value")
            {
                DvQuantity assumedValue = new DvQuantity();
                assumedValue.ReadXml(reader);
                cDomainType.AssumedValue = assumedValue;
            }

            if (reader.LocalName == "property")
            {
                cDomainType.Property = new CodePhrase();
                cDomainType.Property.ReadXml(reader);
            }

            if (reader.LocalName == "list")
            {
                cDomainType.List = new OpenEhr.AssumedTypes.List<CQuantityItem>();
                do
                {
                    CQuantityItem quantityItem = new CQuantityItem();
                    this.ReadXml(quantityItem);
                    cDomainType.List.Add(quantityItem);
                } while (reader.LocalName == "list" && reader.NodeType != XmlNodeType.EndElement);

                Check.Assert(cDomainType.List.Count > 0, "CDvQuantity.List may not be empty.");
            }

            reader.ReadEndElement();
            reader.MoveToContent();
        }

        private void ReadXml(CQuantityItem quantityItem)
        {
            Check.Require(quantityItem != null, "quantityItem must not be null.");
            reader.ReadStartElement();
            reader.MoveToContent();

            if (reader.LocalName == "magnitude")
            {
                quantityItem.Magnitude = new Interval<float>();
                this.ReadXml(quantityItem.Magnitude);
            }

            if (reader.LocalName == "precision")
            {
                quantityItem.Precision = new Interval<int>();
                this.ReadXml(quantityItem.Precision);
            }

            if(reader.LocalName!="units")
                throw new InvalidXmlException("units", reader.LocalName);
            quantityItem.Units = reader.ReadElementContentAsString("units", OpenEhrNamespace);
            reader.MoveToContent();

            reader.ReadEndElement();
            reader.MoveToContent();
        }

        private void ReadXml(CDvState cDvState)
        {
            Check.Require(cDvState != null, "cDvState must not be null.");
            reader.ReadStartElement();
            reader.MoveToContent();

            this.ReadXmlBase((CObject)cDvState);

            if (reader.LocalName == "assumed_value")
            {
                DvState assumedValue = new DvState();
                assumedValue.ReadXml(reader);
                cDvState.AssumedValue = assumedValue;
            }

           if (reader.LocalName != "value")
                throw new InvalidXmlException("value", reader.LocalName);
            cDvState.Value = new StateMachine();
            this.ReadXml(cDvState.Value);

            reader.ReadEndElement();
            reader.MoveToContent();
        }

        private System.Reflection.MethodInfo lastMethodReadStateType = null;
        private State lastStateRead = null;
        private void ReadXml(State state)
        {
            if (state == null)
                throw new ArgumentNullException("state must not be null.");

            const string methodName = "ReadXml";

            try
            {
                System.Reflection.MethodInfo method = this.GetType().GetMethod(methodName,
                    System.Reflection.BindingFlags.ExactBinding | System.Reflection.BindingFlags.NonPublic
                    | System.Reflection.BindingFlags.Instance, Type.DefaultBinder,
                               new Type[] { state.GetType() },
                               new System.Reflection.ParameterModifier[0]);

                if (method != null)
                {
                    // Avoid StackOverflow exceptions by executing only if the method and visitable  
                    // are different from the last parameters used.
                    if (method != lastMethodReadStateType || state != lastStateRead)
                    {
                        lastMethodReadStateType = method;
                        lastStateRead = state;

                        method.Invoke(this, new Object[] { state });

                    }
                    else
                    {
                        string message = string.Format(CommonStrings.LoopingMethodTerminated,
                            methodName, state.GetType().ToString());
                        System.Diagnostics.Debug.WriteLine(message);
                        throw new ApplicationException(message);
                    }
                }
                else
                {
                    string message = string.Format(CommonStrings.MethodXNotImplementedForParamTypeY,
                        methodName, state.GetType().ToString());
                    System.Diagnostics.Debug.WriteLine(message);
                    throw new ApplicationException(message);
                }
            }
            catch (System.Reflection.TargetInvocationException ex)
            {
                if (ex.InnerException != null)
                    throw new ApplicationException(ex.InnerException.Message, ex.InnerException);
                else
                    throw new ApplicationException(ex.Message, ex);
            }
        }

        private void ReadXml(NonTerminalState state)
        {
            Check.Require(state != null, "nonTerminalState must not be null.");

            reader.ReadStartElement();
            reader.MoveToContent();

            this.ReadXmlBase((State)state);

            if (reader.LocalName != "transitions")
                throw new InvalidXmlException("transitions", reader.LocalName);
            System.Collections.Generic.List<Transition> transitions = new System.Collections.Generic.List<Transition>();
            do
            {
                Transition transition = new Transition();
                this.ReadXml(transition);
                transitions.Add(transition);
            } while (reader.LocalName != "transitions");

            Check.Assert(transitions.Count>0, "transitions must not be empty.");

            state.Transitions = new Set<Transition>(transitions);

            reader.ReadEndElement();
            reader.MoveToContent();
        }

        private void ReadXml(TerminalState state)
        {
            Check.Require(state != null, "terminalState must not be null.");

            reader.ReadStartElement();
            reader.MoveToContent();

            this.ReadXmlBase((State)state);
          
            reader.ReadEndElement();
            reader.MoveToContent();
        }

        private void ReadXml(Transition transition)
        {
            Check.Require(transition != null, "transition must not be null.");

            reader.ReadStartElement();
            reader.MoveToContent();
           
            if (reader.LocalName != "event")
                throw new InvalidXmlException("event", reader.LocalName);
            transition.Event = reader.ReadElementContentAsString("event", OpenEhrNamespace);
            reader.MoveToContent();

            if (reader.LocalName == "action")
            {
                transition.Action = reader.ReadElementContentAsString("action", OpenEhrNamespace);
                reader.MoveToContent();
            }

            if (reader.LocalName == "guard")
            {
                transition.Guard = reader.ReadElementContentAsString("guard", OpenEhrNamespace);
                reader.MoveToContent();
            }


            if (reader.LocalName == "next_state")
            {
                string stateType = reader.GetAttribute("type", XsiNamespace);
                Check.Assert(!string.IsNullOrEmpty(stateType), "stateType must not be null or empty.");

                transition.NextState = AmFactory.State(stateType);
                this.ReadXml(transition.NextState);
            }

            reader.ReadEndElement();
            reader.MoveToContent();
        }

        private void ReadXml(StateMachine stateMachine)
        {
            Check.Require(stateMachine != null, "StateMachine must not be null.");

            reader.ReadStartElement();
            reader.MoveToContent();

            if (reader.LocalName != "states")
                throw new InvalidXmlException("states", reader.LocalName);

            System.Collections.Generic.List<State> statesList = new System.Collections.Generic.List<State>();
            do{
                string stateType = reader.GetAttribute("type", XsiNamespace);
                Check.Assert(!string.IsNullOrEmpty(stateType), "stateType must not be null or empty.");

                State state = AmFactory.State(stateType);
                this.ReadXml(state);

                statesList.Add(state);
            }while (reader.LocalName != "states");

            Check.Assert(statesList.Count>0, "statesList must not be empty.");

            stateMachine.States = new Set<State>(statesList);

            reader.ReadEndElement();
            reader.MoveToContent();
        }

        #endregion

        #region ReadXmlBase
       
        public void ReadComplexObject(XmlReader reader, CComplexObject cObject)
        {
            this.reader = reader;
            ReadXmlBase(cObject);
        }

        private void ReadXmlBase(CObject cObject)
        {
            DesignByContract.Check.Require(cObject != null, "cObject must not be null.");

            if (reader.LocalName != "rm_type_name")
            {
                throw new ValidationException("expected local name is rm_type_name, but it is " + reader.LocalName);
            }
            cObject.RmTypeName = reader.ReadElementContentAsString("rm_type_name", OpenEhrNamespace);
            reader.MoveToContent();

            if (reader.LocalName != "occurrences")
                throw new InvalidXmlException("occurrences", reader.LocalName);
            cObject.Occurrences = new OpenEhr.AssumedTypes.Interval<int>();
            this.ReadXml(cObject.Occurrences);

            if (reader.LocalName != "node_id")
                throw new InvalidXmlException("node_id", reader.LocalName);
            cObject.NodeId = reader.ReadElementContentAsString("node_id", OpenEhrNamespace);
            reader.MoveToContent();
        }

        private void ReadXmlBase(CAttribute cAttribute)
        {
            Check.Require(cAttribute != null, "cAttribute must not be null.");

            if (reader.LocalName != "rm_attribute_name")
                throw new InvalidXmlException("rm_attribute_name", reader.LocalName);
            cAttribute.RmAttributeName = reader.ReadElementContentAsString("rm_attribute_name", OpenEhrNamespace);

            if (reader.LocalName != "existence")
                throw new InvalidXmlException("existence", reader.LocalName);
            cAttribute.Existence = new Interval<int>();
            this.ReadXml(cAttribute.Existence);

            if (reader.LocalName == "children")
            {
                cAttribute.Children = new OpenEhr.AssumedTypes.List<CObject>();
                do
                {
                    string cObjectType = reader.GetAttribute("type", RmXmlSerializer.XsiNamespace);
                    DesignByContract.Check.Assert(!string.IsNullOrEmpty(cObjectType), "cObjectType must not be null or empty.");
                    CObject cObj = AmFactory.CObject(cObjectType);

                    cObj.Parent = cAttribute;
                    
                    this.ReadXml(cObj);                    

                    cAttribute.Children.Add(cObj);

                } while (reader.LocalName == "children" && reader.NodeType != XmlNodeType.EndElement);
            }

        }

        private void ReadXmlBase(ExprItem exprItem)
        {
            Check.Require(exprItem != null, "exprItem must not be null.");

            if (reader.LocalName != "type")
                throw new InvalidXmlException("type", reader.LocalName);
            exprItem.Type = reader.ReadElementContentAsString("type", OpenEhrNamespace);
            reader.MoveToContent();
        }

        private void ReadXmlBase(ExprOperator exprOperator)
        {
            this.ReadXmlBase((ExprItem)exprOperator);

            if (reader.LocalName != "operator")
                throw new InvalidXmlException("operator", reader.LocalName);
            exprOperator.Operator = new OperatorKind(reader.ReadElementContentAsInt("operator", OpenEhrNamespace));
            reader.MoveToContent();

            if (reader.LocalName != "precedence_overridden")
                throw new InvalidXmlException("precedence_overridden", reader.LocalName);
            exprOperator.PrecedenceOverriden = reader.ReadElementContentAsBoolean("precedence_overridden", OpenEhrNamespace);
            reader.MoveToContent();
        }

        private void ReadXmlBase(State state)
        {
            if(reader.LocalName !="name")
                throw new InvalidXmlException("name", reader.LocalName);
            state.Name = reader.ReadElementContentAsString("name", OpenEhrNamespace);
        }
        #endregion

        #endregion

        #region WriteXml

        public void WriteArchetype(XmlWriter writer, Archetype archetype)
        {
            DesignByContract.Check.Require(writer != null, string.Format(CommonStrings.XMustNotBeNull, "writer"));
            DesignByContract.Check.Require(archetype.ArchetypeId != null, string.Format(CommonStrings.XMustNotBeNull, "archetype.ArchetypeId"));
            DesignByContract.Check.Require(!string.IsNullOrEmpty(archetype.Concept), string.Format(CommonStrings.XMustNotBeNullOrEmpty, "archetype.Concept"));
            DesignByContract.Check.Require(archetype.Definition != null, string.Format(CommonStrings.XMustNotBeNull, "archetype.Definition"));
            DesignByContract.Check.Require(archetype.Ontology != null, string.Format(CommonStrings.XMustNotBeNull, "archetype.Ontology"));

            this.archetype = archetype;
            this.writer = writer;

            ((AuthoredResource)archetype).WriteXml(writer);
           
            if (archetype.Uid != null)
            {
                writer.WriteStartElement(UseOpenEhrPrefix(writer), "uid", OpenEhrNamespace);
                archetype.Uid.WriteXml(writer);
                writer.WriteEndElement();
            }

            writer.WriteStartElement(UseOpenEhrPrefix(writer), "archetype_id", OpenEhrNamespace);
            archetype.ArchetypeId.WriteXml(writer);
            writer.WriteEndElement();

            if (!string.IsNullOrEmpty(archetype.AdlVersion))
            {
                writer.WriteStartElement(UseOpenEhrPrefix(writer), "adl_version", OpenEhrNamespace);
                writer.WriteString(archetype.AdlVersion);
                writer.WriteEndElement();
            }

            writer.WriteElementString(UseOpenEhrPrefix(writer), "concept", OpenEhrNamespace, archetype.Concept);

            if (archetype.ParentArchetypeId != null)
            {
                writer.WriteStartElement(UseOpenEhrPrefix(writer), "parent_archetype_id", OpenEhrNamespace);
                archetype.ParentArchetypeId.WriteXml(writer);
                writer.WriteEndElement();
            }

            writer.WriteStartElement(UseOpenEhrPrefix(writer), "definition", OpenEhrNamespace);
            this.WriteXml(archetype.Definition);
            writer.WriteEndElement();

            if (archetype.Invariants != null)
            {
                foreach (Assertion assertion in archetype.Invariants)
                {
                    writer.WriteStartElement(UseOpenEhrPrefix(writer), "invariants", OpenEhrNamespace);
                    this.WriteXml(assertion);
                    writer.WriteEndElement();
                }
            }

            writer.WriteStartElement(UseOpenEhrPrefix(writer), "ontology", OpenEhrNamespace);
            this.WriteXml(archetype.Ontology);
            writer.WriteEndElement();
        }

        #region WriteXml - ConstraintModel
        private System.Reflection.MethodInfo lastMethodWriteXmlArchetypeConstraint = null;
        private ArchetypeConstraint lastArchetypeConstraintWrite = null;
        private void WriteXml(ArchetypeConstraint archetypeConstraint)
        {
            if (archetypeConstraint == null) 
                throw new ArgumentNullException(string.Format(CommonStrings.XIsNull, "archetypeConstraint"));

            const string methodName = "WriteXml";

            try
            {
                System.Reflection.MethodInfo method = this.GetType().GetMethod(methodName,
                    System.Reflection.BindingFlags.ExactBinding | System.Reflection.BindingFlags.NonPublic
                    | System.Reflection.BindingFlags.Instance, Type.DefaultBinder,
                               new Type[] { archetypeConstraint.GetType() },
                               new System.Reflection.ParameterModifier[0]);

                if (method != null)
                {
                    // Avoid StackOverflow exceptions by executing only if the method and visitable  
                    // are different from the last parameters used.
                    if (method != lastMethodWriteXmlArchetypeConstraint || archetypeConstraint != lastArchetypeConstraintWrite)
                    {
                        lastMethodWriteXmlArchetypeConstraint = method;
                        lastArchetypeConstraintWrite = archetypeConstraint;

                        method.Invoke(this, new Object[] { archetypeConstraint });

                    }
                    else
                    {
                        string message = string.Format(CommonStrings.LoopingMethodTerminated, 
                            methodName, archetypeConstraint.GetType().ToString());
                        System.Diagnostics.Debug.WriteLine(message);
                        throw new ApplicationException(message);
                    }
                }
                else
                {
                    string message = string.Format(CommonStrings.MethodXNotImplementedForParamTypeY,
                        methodName, archetypeConstraint.GetType().ToString());
                    System.Diagnostics.Debug.WriteLine(message);
                    throw new ApplicationException(message);
                }
            }
            catch (System.Reflection.TargetInvocationException ex)
            {
                if (ex.InnerException != null)
                    throw new ApplicationException(ex.InnerException.Message, ex.InnerException);
                else
                    throw new ApplicationException(ex.Message, ex);
            }
        }

        public void WriteArchetypeTerm(XmlWriter writer, ArchetypeTerm archetypeTerm)
        {
            this.writer = writer;
            WriteXml(archetypeTerm);
        }

        public void WriteCObject(XmlWriter writer, CObject cObject)
        {
            this.writer = writer;

            //Changed by LMT 29/Apr/2009 
            if(cObject is CComplexObject)
                WriteXmlBase(cObject);
            else
                this.WriteXml(cObject);
        }
        
        public void WriteCardinality(XmlWriter writer, Cardinality cardinality)
        {
            this.writer = writer;
            WriteXml(cardinality);
        }

        public void WriteExistence(XmlWriter writer, AssumedTypes.Interval<int> interval)
        {
            this.writer = writer;
            WriteXml(interval);
        }

        private void WriteXml(CComplexObject cComplexObj)
        {
            Check.Require(cComplexObj != null, string.Format(CommonStrings.XMustNotBeNull, "cComplexObj"));

            this.WriteXmlBase((CObject)cComplexObj);

            if (cComplexObj.Attributes != null)
            {
                foreach (CAttribute attr in cComplexObj.Attributes)
                {
                    writer.WriteStartElement(UseOpenEhrPrefix(writer), "attributes", OpenEhrNamespace);

                    string attributeType = AmType.GetName(attr);
                    if (!string.IsNullOrEmpty(UseOpenEhrPrefix(writer)))
                        attributeType = UseOpenEhrPrefix(writer) + ":" + attributeType;
                    writer.WriteAttributeString(UseXsiPrefix(writer), "type", XsiNamespace, attributeType);

                    this.WriteXml(attr);
                    writer.WriteEndElement();
                }
            }
        }

        private void WriteXml(CPrimitiveObject cPrimitiveObj)
        {
            Check.Require(cPrimitiveObj != null, string.Format(CommonStrings.XMustNotBeNull, "cPrimitiveObj"));

            this.WriteXmlBase((CObject)cPrimitiveObj);

            writer.WriteStartElement(UseOpenEhrPrefix(writer), "item", OpenEhrNamespace);
            string itemType = AmType.GetName(cPrimitiveObj.Item);
            writer.WriteAttributeString(UseXsiPrefix(writer), "type", XsiNamespace, itemType);
            if (cPrimitiveObj.Item != null)
                this.WriteXml(cPrimitiveObj.Item);
            writer.WriteEndElement();
        }

        private void WriteXml(ArchetypeInternalRef archetypeInternalRef)
        {
            Check.Require(archetypeInternalRef != null, string.Format(CommonStrings.XMustNotBeNull, "archeytpeInternalRef"));
            Check.Require(!string.IsNullOrEmpty(archetypeInternalRef.TargetPath), string.Format(CommonStrings.XMustNotBeNullOrEmpty, "archetypeInternalRef.TargetPath"));

            this.WriteXmlBase((CObject)archetypeInternalRef);

            writer.WriteElementString(UseOpenEhrPrefix(writer), "target_path", OpenEhrNamespace, archetypeInternalRef.TargetPath);
        }

        private void WriteXml(ConstraintRef constraintRef)
        {
            Check.Require(constraintRef != null, string.Format(CommonStrings.XMustNotBeNull, "constraintRef"));
            Check.Require(!string.IsNullOrEmpty(constraintRef.Reference), string.Format(CommonStrings.XMustNotBeNullOrEmpty, "constraintRef.Reference"));

            this.WriteXmlBase((CObject)constraintRef);

            writer.WriteElementString(UseOpenEhrPrefix(writer), "reference", OpenEhrNamespace, constraintRef.Reference);
        }

        private void WriteXml(ArchetypeSlot archetypeSlot)
        {
            Check.Require(archetypeSlot != null, string.Format(CommonStrings.XMustNotBeNull, "archetypeSlot"));

            this.WriteXmlBase((CObject)archetypeSlot);

            if (archetypeSlot.Includes != null)
            {
                foreach (Assertion assertion in archetypeSlot.Includes)
                {
                    writer.WriteStartElement(UseOpenEhrPrefix(writer), "includes", OpenEhrNamespace);
                    this.WriteXml(assertion);
                    writer.WriteEndElement();
                }
            }

            if (archetypeSlot.Excludes != null)
            {
                foreach (Assertion assertion in archetypeSlot.Excludes)
                {
                    writer.WriteStartElement(UseOpenEhrPrefix(writer), "excludes", OpenEhrNamespace);
                    this.WriteXml(assertion);
                    writer.WriteEndElement();
                }
            }
        }

        private void WriteXml(CMultipleAttribute cMultipleAttribute)
        {
            this.WriteXmlBase((CAttribute)cMultipleAttribute);

            writer.WriteStartElement(UseOpenEhrPrefix(writer), "cardinality", RmXmlSerializer.OpenEhrNamespace);
            this.WriteXml(cMultipleAttribute.Cardinality);
            writer.WriteEndElement();
        }

        private void WriteXml(CSingleAttribute cSingleAttribute)
        {
            this.WriteXmlBase((CAttribute)cSingleAttribute);
        }

        private void WriteXml(Cardinality cardinality)
        {
            Check.Require(cardinality != null, string.Format(CommonStrings.XMustNotBeNull, "cardinality"));
            Check.Require(cardinality.Interval != null, string.Format(CommonStrings.XMustNotBeNull, "cardinality.Interval"));

            writer.WriteElementString(UseOpenEhrPrefix(writer), "is_ordered", OpenEhrNamespace, cardinality.IsOrdered.ToString().ToLower());

            writer.WriteElementString(UseOpenEhrPrefix(writer), "is_unique", OpenEhrNamespace, cardinality.IsUnique.ToString().ToLower());

            writer.WriteStartElement(UseOpenEhrPrefix(writer), "interval", OpenEhrNamespace);
            this.WriteXml(cardinality.Interval);
            writer.WriteEndElement();

        }
        #endregion

        #region WriteXml - Assertion
        private void WriteXml(Assertion assertion)
        {
            Check.Require(assertion != null, string.Format(CommonStrings.XMustNotBeNull, "assertion"));
            Check.Require(assertion.Expression != null, string.Format(CommonStrings.XMustNotBeNull, "assertion.Expression"));

            if (!string.IsNullOrEmpty(assertion.Tag))
                writer.WriteElementString(UseOpenEhrPrefix(writer), "tag", OpenEhrNamespace, assertion.Tag);
           
            if (!string.IsNullOrEmpty(assertion.StringExpression))
                writer.WriteElementString(UseOpenEhrPrefix(writer), "string_expression", OpenEhrNamespace, assertion.StringExpression);

            writer.WriteStartElement(UseOpenEhrPrefix(writer), "expression", OpenEhrNamespace);
            string expressionType = AmType.GetName(assertion.Expression);
            writer.WriteAttributeString(UseXsiPrefix(writer), "type", XsiNamespace, expressionType);
            this.WriteXml(assertion.Expression);
            writer.WriteEndElement();

            if (assertion.Variables != null)
            {
                foreach (AssertionVariable var in assertion.Variables)
                {
                    writer.WriteStartElement(UseOpenEhrPrefix(writer), "variables", OpenEhrNamespace);
                    this.WriteXml(var);
                    writer.WriteEndElement();
                }
            }
        }

        private void WriteXml(AssertionVariable assertionVariable)
        {
            Check.Require(assertionVariable != null, string.Format(CommonStrings.XMustNotBeNull, "assertionVariable"));
            Check.Require(!string.IsNullOrEmpty(assertionVariable.Name), string.Format(CommonStrings.XMustNotBeNullOrEmpty, "assertionVariable.Name"));
            Check.Require(!string.IsNullOrEmpty(assertionVariable.Definition), string.Format(CommonStrings.XMustNotBeNullOrEmpty, "assertionVariable.Definition"));

            writer.WriteElementString(UseOpenEhrPrefix(writer), "name", OpenEhrNamespace, assertionVariable.Name);
            writer.WriteElementString(UseOpenEhrPrefix(writer), "definition", OpenEhrNamespace, assertionVariable.Definition);
        }

        private System.Reflection.MethodInfo lastMethodWriteXmlExprItem = null;
        private ExprItem lastExprItemWrite = null;
        private void WriteXml(ExprItem exprItem)
        {
            if (exprItem == null) throw new ArgumentNullException(string.Format(CommonStrings.XIsNull, "exprItem"));

            const string methodName = "WriteXml";

            try
            {
                System.Reflection.MethodInfo method = this.GetType().GetMethod(methodName,
                    System.Reflection.BindingFlags.ExactBinding | System.Reflection.BindingFlags.NonPublic
                    | System.Reflection.BindingFlags.Instance, Type.DefaultBinder,
                               new Type[] { exprItem.GetType() },
                               new System.Reflection.ParameterModifier[0]);

                if (method != null)
                {
                    // Avoid StackOverflow exceptions by executing only if the method and visitable  
                    // are different from the last parameters used.
                    if (method != lastMethodWriteXmlExprItem || exprItem != lastExprItemWrite)
                    {
                        lastMethodWriteXmlExprItem = method;
                        lastExprItemWrite = exprItem;

                        method.Invoke(this, new Object[] { exprItem });

                    }
                    else
                    {
                        string message = string.Format(CommonStrings.LoopingMethodTerminated, 
                            methodName, exprItem.GetType().ToString());
                        System.Diagnostics.Debug.WriteLine(message);
                        throw new ApplicationException(message);
                    }
                }
                else
                {
                    string message = string.Format(CommonStrings.MethodXNotImplementedForParamTypeY, 
                            methodName, exprItem.GetType().ToString());
                    System.Diagnostics.Debug.WriteLine(message);
                    throw new ApplicationException(message);
                }
            }
            catch (System.Reflection.TargetInvocationException ex)
            {
                if (ex.InnerException != null)
                    throw new ApplicationException(ex.InnerException.Message, ex.InnerException);
                else
                    throw new ApplicationException(ex.Message, ex);
            }
        }
 
        private void WriteXml(ExprLeaf exprItem)
        {
            Check.Require(exprItem.Item != null, string.Format(CommonStrings.XMustNotBeNull, "exprItem.Item"));
            Check.Require(!string.IsNullOrEmpty(exprItem.ReferenceType), string.Format(CommonStrings.XMustNotBeNullOrEmpty, "exprItem.ReferenceType"));

            this.WriteXmlBase((ExprItem)exprItem);

            writer.WriteStartElement(UseOpenEhrPrefix(writer), "item", OpenEhrNamespace);
            string itemType = exprItem.Type;

            if (itemType.StartsWith("C_"))
            {
                writer.WriteAttributeString(UseXsiPrefix(writer), "type", XsiNamespace, itemType);
            }
            else
            {
                itemType = UseXsdPrefix(writer) + ":" + itemType.ToLower();
                writer.WriteAttributeString(UseXsiPrefix(writer), "type", XsiNamespace, itemType);
            }

            if (exprItem.Item is CPrimitive)
                this.WriteXml((CPrimitive)(exprItem.Item));
            else
            {
                if (exprItem.Type.ToLower() == "boolean")
                    writer.WriteString(exprItem.Item.ToString().ToLower());
                else
                    writer.WriteString(exprItem.Item.ToString());
            }
            writer.WriteEndElement();

            writer.WriteElementString(UseOpenEhrPrefix(writer), "reference_type", OpenEhrNamespace, exprItem.ReferenceType);
        }

        private void WriteXml(ExprUnaryOperator exprUnaryOperator)
        {
            Check.Require(exprUnaryOperator != null, string.Format(CommonStrings.XMustNotBeNull, "exprUnaryOperator"));
            Check.Require(exprUnaryOperator.Operand != null, string.Format(CommonStrings.XMustNotBeNull, "exprUnaryOperator.Operand"));

            this.WriteXml((ExprOperator)exprUnaryOperator);

            writer.WriteStartElement(UseOpenEhrPrefix(writer), "operand", OpenEhrNamespace);
            string operandType = AmType.GetName(exprUnaryOperator.Operand);
            writer.WriteAttributeString(UseXsiPrefix(writer), "type", XsiNamespace, operandType);

            this.WriteXml(exprUnaryOperator.Operand);
            writer.WriteEndElement();
        }

        private void WriteXml(ExprBinaryOperator exprBinaryOperator)
        {
            Check.Require(exprBinaryOperator != null, string.Format(CommonStrings.XMustNotBeNull, "exprBinaryOperator"));
            Check.Require(exprBinaryOperator.LeftOperand != null, string.Format(CommonStrings.XMustNotBeNull, "exprBinaryOperator.LeftOperand"));
            Check.Require(exprBinaryOperator.RightOperand != null, string.Format(CommonStrings.XMustNotBeNull, "exprBinaryOperator.RightOperand"));

            this.WriteXmlBase((ExprOperator)exprBinaryOperator);

            writer.WriteStartElement(UseOpenEhrPrefix(writer), "left_operand", OpenEhrNamespace);
            string leftOperandType = AmType.GetName(exprBinaryOperator.LeftOperand);
            writer.WriteAttributeString(UseXsiPrefix(writer), "type", XsiNamespace, leftOperandType);
            this.WriteXml(exprBinaryOperator.LeftOperand);
            writer.WriteEndElement();

            writer.WriteStartElement(UseOpenEhrPrefix(writer), "right_operand", OpenEhrNamespace);
            string rightOperandType = AmType.GetName(exprBinaryOperator.RightOperand);
            writer.WriteAttributeString(UseXsiPrefix(writer), "type", XsiNamespace, rightOperandType);
            this.WriteXml(exprBinaryOperator.RightOperand);
            writer.WriteEndElement();
        }

        #endregion 

        #region WriteXml - Ontology

        private void WriteXml(ArchetypeOntology archetypeOntology)
        {
            Check.Require(archetypeOntology != null, string.Format(CommonStrings.XMustNotBeNull, "archetypeOntology"));

            if (archetypeOntology.TermDefinitions != null && archetypeOntology.TermDefinitions.Count > 0)
            {
                foreach (string language in archetypeOntology.TermDefinitions.Keys)
                {
                    writer.WriteStartElement(UseOpenEhrPrefix(writer), "term_definitions", OpenEhrNamespace);
                    writer.WriteAttributeString("language", language);

                    AssumedTypes.Hash<ArchetypeTerm, string> eachTermDefinitions = archetypeOntology.TermDefinitions.Item(language);
                    foreach (string code in eachTermDefinitions.Keys)
                    {
                        writer.WriteStartElement(UseOpenEhrPrefix(writer), "items", OpenEhrNamespace);
                        this.WriteXml(eachTermDefinitions.Item(code));                        
                        writer.WriteEndElement();
                    }

                    writer.WriteEndElement();
                }
            }

            if (archetypeOntology.ConstraintDefinitions != null && archetypeOntology.ConstraintDefinitions.Count > 0)
            {
                foreach (string language in archetypeOntology.ConstraintDefinitions.Keys)
                {
                    writer.WriteStartElement(UseOpenEhrPrefix(writer), "constraint_definitions", OpenEhrNamespace);
                    writer.WriteAttributeString("language", language);

                    AssumedTypes.Hash<ArchetypeTerm, string> eachTermDefinitions = archetypeOntology.ConstraintDefinitions.Item(language);
                    foreach (string code in eachTermDefinitions.Keys)
                    {
                        writer.WriteStartElement(UseOpenEhrPrefix(writer), "items", OpenEhrNamespace);
                        this.WriteXml(eachTermDefinitions.Item(code));
                        writer.WriteEndElement();
                    }

                    writer.WriteEndElement();
                }
            }

            if (archetypeOntology.TermBindings != null && archetypeOntology.TermBindings.Count > 0)
            {
                foreach (string terminology in archetypeOntology.TermBindings.Keys)
                {
                    writer.WriteStartElement(UseOpenEhrPrefix(writer), "term_bindings", OpenEhrNamespace);
                    writer.WriteAttributeString("terminology", terminology);

                    AssumedTypes.Hash<CodePhrase, string> eachTermBindingItem = archetypeOntology.TermBindings.Item(terminology);
                    foreach (string code in eachTermBindingItem.Keys)
                    {
                        writer.WriteStartElement(UseOpenEhrPrefix(writer), "items", OpenEhrNamespace);
                        writer.WriteAttributeString("code", code);
                        writer.WriteStartElement(UseOpenEhrPrefix(writer), "value", OpenEhrNamespace);
                        eachTermBindingItem.Item(code).WriteXml(writer);
                        writer.WriteEndElement();
                        writer.WriteEndElement();
                    }

                    writer.WriteEndElement();

                }
            }

            if (archetypeOntology.ConstraintBindings != null && archetypeOntology.ConstraintBindings.Count > 0)
            {
                foreach (string terminology in archetypeOntology.ConstraintBindings.Keys)
                {
                    writer.WriteStartElement(UseOpenEhrPrefix(writer), "constraint_bindings", OpenEhrNamespace);
                    writer.WriteAttributeString("terminology", terminology);

                    AssumedTypes.Hash<DvUri, string> eachConstraintBindingItem = archetypeOntology.ConstraintBindings.Item(terminology);
                    foreach (string code in eachConstraintBindingItem.Keys)
                    {
                        writer.WriteStartElement(UseOpenEhrPrefix(writer), "items", OpenEhrNamespace);
                        writer.WriteAttributeString("code", code);
                        eachConstraintBindingItem.Item(code).WriteXml(writer);
                        writer.WriteEndElement();
                    }

                    writer.WriteEndElement();

                }
            }
        }

        private void WriteXml(ArchetypeTerm archetypTerm)
        {
            Check.Require(archetypTerm != null, string.Format(CommonStrings.XMustNotBeNull, "archetypTerm"));
            Check.Require(!string.IsNullOrEmpty(archetypTerm.Code), string.Format(CommonStrings.XMustNotBeNullOrEmpty, "archetypTerm.Code"));
            Check.Require(archetypTerm.Items != null && archetypTerm.Items.Count > 0, string.Format(CommonStrings.XMustNotBeNullOrEmpty, "archetypTerm.Items"));

            writer.WriteAttributeString("code", archetypTerm.Code);
            foreach (string eachId in archetypTerm.Items.Keys)
            {
                writer.WriteStartElement(UseOpenEhrPrefix(writer), "items", OpenEhrNamespace);
                writer.WriteAttributeString("id", eachId);
                writer.WriteString(archetypTerm.Items.Item(eachId));
                writer.WriteEndElement();
            }

        }

        #endregion

        #region WriteXml - Interval
        private void WriteXml(AssumedTypes.Interval<int> interval)
        {
            Check.Require(interval != null, string.Format(CommonStrings.XMustNotBeNull, "interval"));

            string openEhrPrefix = UseOpenEhrPrefix(writer);
            if (interval.lowerIncludedSet)
                writer.WriteElementString(openEhrPrefix, "lower_included", OpenEhrNamespace, interval.LowerIncluded.ToString().ToLower());

            if (interval.upperIncludedSet)
                writer.WriteElementString(openEhrPrefix, "upper_included", OpenEhrNamespace, interval.UpperIncluded.ToString().ToLower());

            writer.WriteElementString(openEhrPrefix, "lower_unbounded", OpenEhrNamespace, interval.LowerUnbounded.ToString().ToLower());
            writer.WriteElementString(openEhrPrefix, "upper_unbounded", OpenEhrNamespace, interval.UpperUnbounded.ToString().ToLower());

            if (!interval.LowerUnbounded)
                writer.WriteElementString(openEhrPrefix, "lower", OpenEhrNamespace, interval.Lower.ToString());

            if (!interval.UpperUnbounded)
                writer.WriteElementString(openEhrPrefix, "upper", OpenEhrNamespace, interval.Upper.ToString());
        }

        private void WriteXml(AssumedTypes.Interval<float> interval)
        {
            Check.Require(interval != null, string.Format(CommonStrings.XMustNotBeNull, "interval"));

            string openEhrPrefix = UseOpenEhrPrefix(writer);
            if (interval.lowerIncludedSet)
                writer.WriteElementString(openEhrPrefix, "lower_included", OpenEhrNamespace, interval.LowerIncluded.ToString().ToLower());

            if (interval.upperIncludedSet)
                writer.WriteElementString(openEhrPrefix, "upper_included", OpenEhrNamespace, interval.UpperIncluded.ToString().ToLower());

            writer.WriteElementString(openEhrPrefix, "lower_unbounded", OpenEhrNamespace, interval.LowerUnbounded.ToString().ToLower());
            writer.WriteElementString(openEhrPrefix, "upper_unbounded", OpenEhrNamespace, interval.UpperUnbounded.ToString().ToLower());

            if (!interval.LowerUnbounded)
                writer.WriteElementString(openEhrPrefix, "lower", OpenEhrNamespace, interval.Lower.ToString());

            if (!interval.UpperUnbounded)
                writer.WriteElementString(openEhrPrefix, "upper", OpenEhrNamespace, interval.Upper.ToString());
        }

        private void WriteXml(AssumedTypes.Interval<Iso8601Date> interval)
        {
            Check.Require(interval != null, string.Format(CommonStrings.XMustNotBeNull, "interval"));

            string openEhrPrefix = UseOpenEhrPrefix(writer);
            if (interval.lowerIncludedSet)
                writer.WriteElementString(openEhrPrefix, "lower_included", OpenEhrNamespace, interval.LowerIncluded.ToString().ToLower());

            if (interval.upperIncludedSet)
                writer.WriteElementString(openEhrPrefix, "upper_included", OpenEhrNamespace, interval.UpperIncluded.ToString().ToLower());

            writer.WriteElementString(openEhrPrefix, "lower_unbounded", OpenEhrNamespace, interval.LowerUnbounded.ToString().ToLower());
            writer.WriteElementString(openEhrPrefix, "upper_unbounded", OpenEhrNamespace, interval.UpperUnbounded.ToString().ToLower());

            if (!interval.LowerUnbounded)
                writer.WriteElementString(openEhrPrefix, "lower", OpenEhrNamespace, interval.Lower.ToString());

            if (!interval.UpperUnbounded)
                writer.WriteElementString(openEhrPrefix, "upper", OpenEhrNamespace, interval.Upper.ToString());
        }

        private void WriteXml(AssumedTypes.Interval<Iso8601DateTime> interval)
        {
            Check.Require(interval != null, string.Format(CommonStrings.XMustNotBeNull, "interval"));

            string openEhrPrefix = UseOpenEhrPrefix(writer);
            if (interval.lowerIncludedSet)
                writer.WriteElementString(openEhrPrefix, "lower_included", OpenEhrNamespace, interval.LowerIncluded.ToString().ToLower());

            if (interval.upperIncludedSet)
                writer.WriteElementString(openEhrPrefix, "upper_included", OpenEhrNamespace, interval.UpperIncluded.ToString().ToLower());

            writer.WriteElementString(openEhrPrefix, "lower_unbounded", OpenEhrNamespace, interval.LowerUnbounded.ToString().ToLower());
            writer.WriteElementString(UseOpenEhrPrefix(writer), "upper_unbounded", OpenEhrNamespace, interval.UpperUnbounded.ToString().ToLower());

            if (!interval.LowerUnbounded)
                writer.WriteElementString(openEhrPrefix, "lower", OpenEhrNamespace, interval.Lower.ToString());

            if (!interval.UpperUnbounded)
                writer.WriteElementString(openEhrPrefix, "upper", OpenEhrNamespace, interval.Upper.ToString());
        }

        private void WriteXml(AssumedTypes.Interval<Iso8601Time> interval)
        {
            Check.Require(interval != null, string.Format(CommonStrings.XMustNotBeNull, "interval"));

            string openEhrPrefix = UseOpenEhrPrefix(writer);
            if (interval.lowerIncludedSet)
                writer.WriteElementString(openEhrPrefix, "lower_included", OpenEhrNamespace, interval.LowerIncluded.ToString().ToLower());

            if (interval.upperIncludedSet)
                writer.WriteElementString(openEhrPrefix, "upper_included", OpenEhrNamespace, interval.UpperIncluded.ToString().ToLower());

            writer.WriteElementString(openEhrPrefix, "lower_unbounded", OpenEhrNamespace, interval.LowerUnbounded.ToString().ToLower());
            writer.WriteElementString(openEhrPrefix, "upper_unbounded", OpenEhrNamespace, interval.UpperUnbounded.ToString().ToLower());

            if (!interval.LowerUnbounded)
                writer.WriteElementString(openEhrPrefix, "lower", OpenEhrNamespace, interval.Lower.ToString());

            if (!interval.UpperUnbounded)
                writer.WriteElementString(openEhrPrefix, "upper", OpenEhrNamespace, interval.Upper.ToString());
        }

        private void WriteXml(AssumedTypes.Interval<Iso8601Duration> interval)
        {
            Check.Require(interval != null, string.Format(CommonStrings.XMustNotBeNull, "interval"));

            string openEhrPrefix = UseOpenEhrPrefix(writer);
            if (interval.lowerIncludedSet)
                writer.WriteElementString(openEhrPrefix, "lower_included", OpenEhrNamespace, interval.LowerIncluded.ToString().ToLower());

            if (interval.upperIncludedSet)
                writer.WriteElementString(openEhrPrefix, "upper_included", OpenEhrNamespace, interval.UpperIncluded.ToString().ToLower());

            writer.WriteElementString(openEhrPrefix, "lower_unbounded", OpenEhrNamespace, interval.LowerUnbounded.ToString().ToLower());
            writer.WriteElementString(openEhrPrefix, "upper_unbounded", OpenEhrNamespace, interval.UpperUnbounded.ToString().ToLower());

            if (!interval.LowerUnbounded)
                writer.WriteElementString(openEhrPrefix, "lower", OpenEhrNamespace, interval.Lower.ToString());

            if (!interval.UpperUnbounded)
                writer.WriteElementString(openEhrPrefix, "upper", OpenEhrNamespace, interval.Upper.ToString());
        }
        #endregion

        #region WriteXml - CPrimitive package
        private System.Reflection.MethodInfo lastMethodWriteXmlCPrimitive = null;
        private CPrimitive lastCPrimitiveWrite = null;
        private void WriteXml(CPrimitive cPrimitiveObj)
        {
            if (cPrimitiveObj == null) throw new ArgumentNullException(string.Format(CommonStrings.XIsNull, "cPrimitiveObj"));

            const string methodName = "WriteXml";

            try
            {
                System.Reflection.MethodInfo method = this.GetType().GetMethod(methodName,
                    System.Reflection.BindingFlags.ExactBinding | System.Reflection.BindingFlags.NonPublic
                    | System.Reflection.BindingFlags.Instance, Type.DefaultBinder,
                               new Type[] { cPrimitiveObj.GetType() },
                               new System.Reflection.ParameterModifier[0]);

                if (method != null)
                {
                    // Avoid StackOverflow exceptions by executing only if the method and visitable  
                    // are different from the last parameters used.
                    if (method != lastMethodWriteXmlCPrimitive || cPrimitiveObj != lastCPrimitiveWrite)
                    {
                        lastMethodWriteXmlCPrimitive = method;
                        lastCPrimitiveWrite = cPrimitiveObj;

                        method.Invoke(this, new Object[] { cPrimitiveObj });

                    }
                    else
                    {
                        string message = string.Format(CommonStrings.LoopingMethodTerminated,
                            methodName, cPrimitiveObj.GetType().ToString());
                        System.Diagnostics.Debug.WriteLine(message);
                        throw new ApplicationException(message);
                    }
                }
                else
                {
                    string message = string.Format(CommonStrings.MethodXNotImplementedForParamTypeY,
                        methodName, cPrimitiveObj.GetType().ToString());
                    System.Diagnostics.Debug.WriteLine(message);
                    throw new ApplicationException(message);
                }
            }
            catch (System.Reflection.TargetInvocationException ex)
            {
                if (ex.InnerException != null)
                    throw new ApplicationException(ex.InnerException.Message, ex.InnerException);
                else
                    throw new ApplicationException(ex.Message, ex);
            }
        }

        private void WriteXml(CBoolean cPrimitive)
        {
            string openEhrPrefix = UseOpenEhrPrefix(writer);
            writer.WriteElementString(openEhrPrefix, "true_valid", RmXmlSerializer.OpenEhrNamespace, cPrimitive.TrueValid.ToString().ToLower());
            writer.WriteElementString(openEhrPrefix, "false_valid", RmXmlSerializer.OpenEhrNamespace, cPrimitive.FalseValid.ToString().ToLower());

            if (cPrimitive.assumedValueSet)
                writer.WriteElementString(openEhrPrefix, "assumed_value", RmXmlSerializer.OpenEhrNamespace, cPrimitive.AssumedValue.ToString().ToLower());

        }

        private void WriteXml(CDate cPrimitive)
        {
            if (!string.IsNullOrEmpty(cPrimitive.Pattern))
                writer.WriteElementString("pattern", OpenEhrNamespace, cPrimitive.Pattern);

            if (cPrimitive.TimezoneValidity != null)
                writer.WriteElementString("timezone_validity", OpenEhrNamespace, cPrimitive.TimezoneValidity.Value.ToString());

            if (cPrimitive.Range != null)
                this.WriteXml(cPrimitive.Range);

            if (cPrimitive.AssumedValue != null)
            {
                writer.WriteStartElement(UseOpenEhrPrefix(writer), "assumed_value", OpenEhrNamespace);
                writer.WriteString(cPrimitive.AssumedValue.ToString());
                writer.WriteEndElement();
            }
        }

        private void WriteXml(CDateTime cPrimitive)
        {
            if (!string.IsNullOrEmpty(cPrimitive.Pattern))
                writer.WriteElementString("pattern", OpenEhrNamespace, cPrimitive.Pattern);

            if (cPrimitive.TimezoneValidity != null)
                writer.WriteElementString("timezone_validity", OpenEhrNamespace, cPrimitive.TimezoneValidity.Value.ToString());

            if (cPrimitive.Range != null)
            {
                writer.WriteStartElement("range");
                this.WriteXml(cPrimitive.Range);
                writer.WriteEndElement();
            }

            if (cPrimitive.AssumedValue != null)
            {
                writer.WriteStartElement(UseOpenEhrPrefix(writer), "assumed_value", OpenEhrNamespace);
                writer.WriteString(cPrimitive.AssumedValue.ToString());
                writer.WriteEndElement();
            }
        }

        private void WriteXml(CDuration cPrimitive)
        {
            string OpenEhrNamespace = RmXmlSerializer.OpenEhrNamespace;
            string openEhrPrefix = RmXmlSerializer.UseOpenEhrPrefix(writer);

            if (!string.IsNullOrEmpty(cPrimitive.Pattern))
                writer.WriteElementString("pattern", OpenEhrNamespace, cPrimitive.Pattern);

            if (cPrimitive.Range != null)
            {
                writer.WriteStartElement(openEhrPrefix, "range", OpenEhrNamespace);
                this.WriteXml(cPrimitive.Range);
                writer.WriteEndElement();
            }

            if (cPrimitive.AssumedValue != null)
            {
                writer.WriteStartElement(openEhrPrefix, "assumed_value", OpenEhrNamespace);
                writer.WriteString(cPrimitive.AssumedValue.ToString());
                writer.WriteEndElement();
            }
        }

        private void WriteXml(CInteger cPrimitive)
        {
            if (cPrimitive.List != null && cPrimitive.List.Count > 0)
            {
                foreach (int item in cPrimitive.List)
                {
                    writer.WriteElementString("list", OpenEhrNamespace, item.ToString());
                }
            }

            if (cPrimitive.Range != null)
            {
                writer.WriteStartElement(UseOpenEhrPrefix(writer), "range", OpenEhrNamespace);
                this.WriteXml(cPrimitive.Range);
                writer.WriteEndElement();
            }

            if (cPrimitive.assumedValueSet)
                writer.WriteElementString("assumed_value", OpenEhrNamespace, cPrimitive.AssumedValue.ToString());
        }

        private void WriteXml(CReal cPrimitive)
        {
           if (cPrimitive.List != null && cPrimitive.List.Count > 0)
            {
                foreach (float item in cPrimitive.List)
                {
                    writer.WriteElementString("list", OpenEhrNamespace, item.ToString());
                }
            }

            if (cPrimitive.Range != null)
            {
                writer.WriteStartElement(UseOpenEhrPrefix(writer), "range", OpenEhrNamespace);
                this.WriteXml(cPrimitive.Range);
                writer.WriteEndElement();
            }

            if (cPrimitive.assumedValueSet)
                writer.WriteElementString("assumed_value", OpenEhrNamespace, cPrimitive.AssumedValue.ToString());
        }

        private void WriteXml(CString cPrimitive)
        {
            if (!string.IsNullOrEmpty(cPrimitive.Pattern))
                writer.WriteElementString("pattern", OpenEhrNamespace, cPrimitive.Pattern);

            if (cPrimitive.List != null && cPrimitive.List.Count > 0)
            {
                foreach (string item in cPrimitive.List)
                {
                    writer.WriteElementString("list", OpenEhrNamespace, item);
                }
            }

            if (cPrimitive.listOpenSet)
                writer.WriteElementString("list_open", OpenEhrNamespace, cPrimitive.ListOpen.ToString().ToLower());

            if (cPrimitive.AssumedValue != null && cPrimitive.AssumedValue.ToString() != string.Empty)
                writer.WriteElementString("assumed_value", OpenEhrNamespace, cPrimitive.AssumedValue.ToString());
        }

        private void WriteXml(CTime cPrimitive)
        {
            if (!string.IsNullOrEmpty(cPrimitive.Pattern))
                writer.WriteElementString("pattern", OpenEhrNamespace, cPrimitive.Pattern);

            if (cPrimitive.TimezoneValidity != null)
                writer.WriteElementString("timezone_validity", OpenEhrNamespace, cPrimitive.TimezoneValidity.Value.ToString());

            if (cPrimitive.Range != null)
                this.WriteXml(cPrimitive.Range);

            if (cPrimitive.AssumedValue != null)
            {
                writer.WriteStartElement(UseOpenEhrPrefix(writer), "assumed_value", OpenEhrNamespace);
                writer.WriteString(cPrimitive.AssumedValue.ToString());
                writer.WriteEndElement();
            }
        }
        #endregion

        #region WriteXml - CDomainType & OpenehrProfile

        private System.Reflection.MethodInfo lastMethodWriteXmlCDomainType = null;
        private CDomainType lastCDomainWrite = null;
        private void WriteXml(CDomainType cDomainType)
        {
            if (cDomainType == null) 
                throw new ArgumentNullException(string.Format(CommonStrings.XIsNull, cDomainType));

            const string methodName = "WriteXml";

            try
            {
                System.Reflection.MethodInfo method = this.GetType().GetMethod(methodName,
                    System.Reflection.BindingFlags.ExactBinding | System.Reflection.BindingFlags.NonPublic
                    | System.Reflection.BindingFlags.Instance, Type.DefaultBinder,
                               new Type[] { cDomainType.GetType() },
                               new System.Reflection.ParameterModifier[0]);

                if (method != null)
                {
                    // Avoid StackOverflow exceptions by executing only if the method and visitable  
                    // are different from the last parameters used.
                    if (method != lastMethodWriteXmlCDomainType || cDomainType != lastCDomainWrite)
                    {
                        lastMethodWriteXmlCDomainType = method;
                        lastCDomainWrite = cDomainType;

                        method.Invoke(this, new Object[] { cDomainType });

                    }
                    else
                    {
                        string message = string.Format(CommonStrings.LoopingMethodTerminated,
                            methodName, cDomainType.GetType().ToString());
                        System.Diagnostics.Debug.WriteLine(message);
                        throw new ApplicationException(message);
                    }
                }
                else
                {
                    string message = string.Format(CommonStrings.MethodXNotImplementedForParamTypeY,
                        methodName, cDomainType.GetType().ToString());
                    System.Diagnostics.Debug.WriteLine(message);
                    throw new ApplicationException(message);
                }
            }
            catch (System.Reflection.TargetInvocationException ex)
            {
                if (ex.InnerException != null)
                    throw new ApplicationException(ex.InnerException.Message, ex.InnerException);
                else
                    throw new ApplicationException(ex.Message, ex);
            }
        }

        private void WriteXml(CDvOrdinal cDomainType)
        {           
            this.WriteXmlBase((CObject)cDomainType);
            if (cDomainType.AssumedValue != null)
            {
                writer.WriteStartElement(UseOpenEhrPrefix(writer), "assumed_value", OpenEhrNamespace);
                ((DvOrdinal)(cDomainType.AssumedValue)).WriteXml(writer);
                writer.WriteEndElement();
            }

            if (cDomainType.List == null) //Added by LMT 05/Apr/2009 EHR-900
                return;

            foreach (DvOrdinal listItem in cDomainType.List)
            {
                writer.WriteStartElement(UseOpenEhrPrefix(writer), "list", OpenEhrNamespace);
                listItem.WriteXml(writer);
                writer.WriteEndElement();
            }
        }

        private void WriteXml(CDvQuantity cDomainType)
        {
            string openEhrPrefix = UseOpenEhrPrefix(writer);
            this.WriteXmlBase((CObject)cDomainType);
            if (cDomainType.AssumedValue != null)
            {
                writer.WriteStartElement(openEhrPrefix, "assumed_value", OpenEhrNamespace);
                ((DvQuantity)(cDomainType.AssumedValue)).WriteXml(writer);
                writer.WriteEndElement();
            }

            if (cDomainType.Property != null)
            {
                writer.WriteStartElement(openEhrPrefix, "property", OpenEhrNamespace);
                cDomainType.Property.WriteXml(writer);
                writer.WriteEndElement();
            }

            if (cDomainType.List != null)
            {
                foreach (CQuantityItem listItem in cDomainType.List)
                {
                    writer.WriteStartElement(openEhrPrefix, "list", OpenEhrNamespace);
                    this.WriteXml(listItem);
                    writer.WriteEndElement();
                }
            }
        }

        private void WriteXml(CCodePhrase cDomainType)
        {
            string openEhrPrefix = UseOpenEhrPrefix(writer);
            this.WriteXmlBase((CObject)cDomainType);
            if (cDomainType.AssumedValue != null)
            {
                writer.WriteStartElement(openEhrPrefix, "assumed_value", OpenEhrNamespace);
                ((CodePhrase)(cDomainType.AssumedValue)).WriteXml(writer);
                writer.WriteEndElement();
            }

            if (cDomainType.TerminologyId != null)
            {
                writer.WriteStartElement(openEhrPrefix, "terminology_id", OpenEhrNamespace);
                cDomainType.TerminologyId.WriteXml(writer);
                writer.WriteEndElement();
            }

            if (cDomainType.CodeList != null)
            {
                foreach (string listItem in cDomainType.CodeList)
                {
                    writer.WriteElementString(openEhrPrefix, "code_list", OpenEhrNamespace, listItem);
                }
            }
        }

        private void WriteXml(CQuantityItem cQuantityItem)
        {
            Check.Require(cQuantityItem != null, string.Format(CommonStrings.XMustNotBeNull, "cQuantityItem"));
            // %HYYKA%
            // Commented by LMT 05/Apr/2009 EHR-900 because assumption is invalid in case of certain archetypes (eg. EVALUATION.medication in EHR/test repository which doesn't specify any units in the C_QUANTITY_ITEM).
            //Check.Require(!string.IsNullOrEmpty(cQuantityItem.Units), string.Format(CommonStrings.XMustNotBeNullOrEmpty, "cQuantityItem.Units"));

            string openEhrPrefix = UseOpenEhrPrefix(writer);
            if (cQuantityItem.Magnitude != null)
            {
                writer.WriteStartElement(openEhrPrefix, "magnitude", OpenEhrNamespace);
                this.WriteXml(cQuantityItem.Magnitude);
                writer.WriteEndElement();
            }

            if (cQuantityItem.Precision != null)
            {
                writer.WriteStartElement(openEhrPrefix, "precision", OpenEhrNamespace);
                this.WriteXml(cQuantityItem.Precision);
                writer.WriteEndElement();
            }

            writer.WriteElementString(openEhrPrefix, "units", OpenEhrNamespace, cQuantityItem.Units);
        }

        private void WriteXml(CDvState cDvState)
        {
            Check.Require(cDvState.Value != null, string.Format(CommonStrings.XMustNotBeNull, "cDvState.Value"));

            this.WriteXmlBase((CObject)cDvState);
            if (cDvState.AssumedValue != null)
            {
                writer.WriteStartElement(UseOpenEhrPrefix(writer), "assumed_value", OpenEhrNamespace);
                ((DvState)(cDvState.AssumedValue)).WriteXml(writer);
                writer.WriteEndElement();
            }

            writer.WriteStartElement(UseOpenEhrPrefix(writer), "value", OpenEhrNamespace);
            this.WriteXml(cDvState.Value);
            writer.WriteEndElement();
        }

        private System.Reflection.MethodInfo lastMethodWriteXmlState = null;
        private State lastStateWrite = null;
        private void WriteXml(State state)
        {
            if (state == null) 
                throw new ArgumentNullException(string.Format(CommonStrings.XIsNull, "state"));

            const string methodName = "WriteXml";

            try
            {
                System.Reflection.MethodInfo method = this.GetType().GetMethod(methodName,
                    System.Reflection.BindingFlags.ExactBinding | System.Reflection.BindingFlags.NonPublic
                    | System.Reflection.BindingFlags.Instance, Type.DefaultBinder,
                               new Type[] { state.GetType() },
                               new System.Reflection.ParameterModifier[0]);

                if (method != null)
                {
                    if (method != lastMethodWriteXmlState || state != lastStateWrite)
                    {
                        lastMethodWriteXmlState = method;
                        lastStateWrite = state;

                        method.Invoke(this, new Object[] { state });

                    }
                    else
                    {
                        string message = string.Format(CommonStrings.LoopingMethodTerminated,
                            methodName, state.GetType().ToString());
                        System.Diagnostics.Debug.WriteLine(message);
                        throw new ApplicationException(message);
                    }
                }
                else
                {
                    string message = string.Format(CommonStrings.MethodXNotImplementedForParamTypeY,
                            methodName, state.GetType().ToString());
                    System.Diagnostics.Debug.WriteLine(message);
                    throw new ApplicationException(message);
                }
            }
            catch (System.Reflection.TargetInvocationException ex)
            {
                if (ex.InnerException != null)
                    throw new ApplicationException(ex.InnerException.Message, ex.InnerException);
                else
                    throw new ApplicationException(ex.Message, ex);
            }
        }

        private void WriteXml(NonTerminalState state)
        {
            Check.Require(state.Transitions != null && !state.Transitions.IsEmpty(),
                string.Format(CommonStrings.XMustNotBeNullOrEmpty, "state.Transitions"));

            this.WriteXmlBase((State)state);
            foreach (Transition transition in state.Transitions)
            {
                writer.WriteStartElement(UseOpenEhrPrefix(writer), "transitions", OpenEhrNamespace);
                this.WriteXml(transition);
                writer.WriteEndElement();
            }
        }

        private void WriteXml(TerminalState state)
        {
            this.WriteXmlBase((State)state);
        }

        private void WriteXml(StateMachine stateMachine)
        {
            Check.Require(stateMachine != null, string.Format(CommonStrings.XMustNotBeNull, "stateMachine"));
            Check.Require(stateMachine.States!= null && !stateMachine.States.IsEmpty(),
                string.Format(CommonStrings.XMustNotBeNullOrEmpty, "stateMachine.States"));

            foreach (State state in stateMachine.States)
            {
                string stateType = AmType.GetName(state);
                writer.WriteStartElement(UseOpenEhrPrefix(writer), "states", OpenEhrNamespace);
                writer.WriteAttributeString(UseXsiPrefix(writer), "type", XsiNamespace, stateType);
                this.WriteXml(state);
                writer.WriteEndElement();
            }
        }

        private void WriteXml(Transition transition)
        {
            Check.Require(transition != null, string.Format(CommonStrings.XMustNotBeNull, "transition"));
            Check.Require(!string.IsNullOrEmpty(transition.Event),
                string.Format(CommonStrings.XMustNotBeNullOrEmpty, "transition.Event"));

            string openEhrPrefix = UseOpenEhrPrefix(writer);
            writer.WriteElementString(openEhrPrefix, "event", OpenEhrNamespace, transition.Event);
            if(!string.IsNullOrEmpty(transition.Action))
                writer.WriteElementString(openEhrPrefix, "action", OpenEhrNamespace, transition.Action);
            if (!string.IsNullOrEmpty(transition.Guard))
                writer.WriteElementString(openEhrPrefix, "guard", OpenEhrNamespace, transition.Guard);
            if (transition.NextState != null)
            {
                writer.WriteStartElement(openEhrPrefix, "next_state", OpenEhrNamespace);
                string stateType = AmType.GetName(transition.NextState);
                writer.WriteAttributeString(UseXsiPrefix(writer), "type", OpenEhrNamespace, stateType);
                this.WriteXml(transition.NextState);
                writer.WriteEndElement();
            }

        }
        #endregion

        #region WriteXmlBase

        private void WriteXmlBase(ExprItem exprItem)
        {
            Check.Require(exprItem != null, string.Format(CommonStrings.XMustNotBeNull, "exprItem"));
            Check.Require(!string.IsNullOrEmpty(exprItem.Type), string.Format(CommonStrings.XMustNotBeNullOrEmpty, "exprItem.Type"));

            writer.WriteElementString(UseOpenEhrPrefix(writer), "type", OpenEhrNamespace, exprItem.Type);
        }

        private void WriteXmlBase(State state)
        {
            Check.Require(state != null, string.Format(CommonStrings.XMustNotBeNull, "state"));
            Check.Require(!string.IsNullOrEmpty(state.Name), string.Format(CommonStrings.XMustNotBeNullOrEmpty, "state.Name"));

            writer.WriteElementString(UseOpenEhrPrefix(writer), "name", OpenEhrNamespace, state.Name);
        }

        private void WriteXmlBase(ExprOperator exprOperator)
        {
            Check.Require(exprOperator != null, string.Format(CommonStrings.XMustNotBeNull, "exprOperator"));
            Check.Require(exprOperator.Operator != null, string.Format(CommonStrings.XMustNotBeNull, "exprOperator.Operator"));

            this.WriteXmlBase((ExprItem)exprOperator);
            writer.WriteElementString(UseOpenEhrPrefix(writer), "operator", OpenEhrNamespace, exprOperator.Operator.Value.ToString());
            writer.WriteElementString(UseOpenEhrPrefix(writer), "precedence_overridden", OpenEhrNamespace, exprOperator.PrecedenceOverriden.ToString().ToLower());
        }

        private void WriteXmlBase(CObject cObject)
        {
            Check.Require(cObject != null, string.Format(CommonStrings.XMustNotBeNull, "cObject"));
            Check.Require(!string.IsNullOrEmpty(cObject.RmTypeName), string.Format(CommonStrings.XMustNotBeNullOrEmpty, "cObject.RmTypeName"));
            Check.Require(cObject.Occurrences != null, string.Format(CommonStrings.XMustNotBeNull, "cObject.Occurrences"));

            string openEhrPrefix = UseOpenEhrPrefix(writer);
            writer.WriteElementString(openEhrPrefix, "rm_type_name", OpenEhrNamespace, cObject.RmTypeName);
            writer.WriteStartElement(openEhrPrefix, "occurrences", OpenEhrNamespace);
            this.WriteXml(cObject.Occurrences);
            writer.WriteEndElement();
            writer.WriteElementString(openEhrPrefix, "node_id", OpenEhrNamespace, cObject.NodeId);
        }

        private void WriteXmlBase(CAttribute cAttribute)
        {
            Check.Require(cAttribute != null, string.Format(CommonStrings.XMustNotBeNull, "cAttribute"));
            Check.Require(!string.IsNullOrEmpty(cAttribute.RmAttributeName), string.Format(CommonStrings.XMustNotBeNullOrEmpty, "cAttribute.RmAttributeName"));
            Check.Require(cAttribute.Existence != null, string.Format(CommonStrings.XMustNotBeNull, "cAttribute.Existence"));

            string openEhrPrefix = UseOpenEhrPrefix(writer);
            writer.WriteElementString(openEhrPrefix, "rm_attribute_name", OpenEhrNamespace, cAttribute.RmAttributeName);

            writer.WriteStartElement(openEhrPrefix, "existence", OpenEhrNamespace);
            this.WriteXml(cAttribute.Existence);
            writer.WriteEndElement();

            if (cAttribute.Children != null)
            {
                foreach (CObject cObj in cAttribute.Children)
                {
                    string cObjType = AmType.GetName(cObj);
                    if (!string.IsNullOrEmpty(openEhrPrefix))
                        cObjType = openEhrPrefix + ":" + cObjType;
                    writer.WriteStartElement(openEhrPrefix, "children", OpenEhrNamespace);
                    writer.WriteAttributeString(UseXsiPrefix(writer), "type", RmXmlSerializer.XsiNamespace, cObjType);

                    this.WriteXml(cObj);
                    writer.WriteEndElement();
                }
            }
        }
        #endregion

        #endregion
   
    }
}
