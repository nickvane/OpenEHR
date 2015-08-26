using System;
using OpenEhr.RM.DataTypes.Text;
using OpenEhr.RM.Support.Identification;
using OpenEhr.AM;
using OpenEhr.RM.Common.Resource;
using OpenEhr.RM.Common.Generic;
using OpenEhr.AssumedTypes;
using OpenEhr.Serialisation;

namespace OpenEhr.Futures.OperationalTemplate
{
    [System.Xml.Serialization.XmlSchemaProvider("GetXmlSchema")]
    public class OperationalTemplate : System.Xml.Serialization.IXmlSerializable
    {
        CodePhrase language;
        public CodePhrase Language
        {
            get { return language; }
            set { language = value; }
        }

        //BJP: isControlled may be null
        bool? isControlled = null;
        public bool? IsControlled
        {
            get { return isControlled; }
            set { isControlled = value; }
        }

        ResourceDescription description;
        public ResourceDescription Description
        {
            get { return description; }
            set { description = value; }
        }

        RevisionHistory revisionHistory;
        public RevisionHistory RevisionHistory
        {
            get { return revisionHistory; }
            set { revisionHistory = value; }
        }

        HierObjectId uid;
        public HierObjectId Uid
        {
            get { return uid; }
            set { uid = value; }
        }

        TemplateId templateId;
        public TemplateId TemplateId
        {
            get { return templateId; }
            set { templateId = value; }
        }

        string concept;
        public string Concept
        {
            get { return concept; }
            set { concept = value; }
        }

        CArchetypeRoot definition;
        public CArchetypeRoot Definition
        {
            get { return definition; }
            set { definition = value; }
        }

        private List<OpenEhr.RM.Common.Resource.Annotation> annotations;
        public List<OpenEhr.RM.Common.Resource.Annotation> Annotations
        {
            get { return this.annotations; }
            set { this.annotations = value; }
        }

        private TConstraint constraints;
        public TConstraint Constraints
        {
            get { return this.constraints; }
            set { this.constraints = value; }
        }

        private TView view;
        public TView View
        {
            get { return this.view; }
            set { this.view = value; }
        }

        #region IXmlSerializable Members

        System.Xml.Schema.XmlSchema System.Xml.Serialization.IXmlSerializable.GetSchema()
        {
            throw new Exception("The method or operation is not implemented.");
        }

        void System.Xml.Serialization.IXmlSerializable.ReadXml(System.Xml.XmlReader reader)
        {
            OperationalTemplateXmlReader templateReader = new OperationalTemplateXmlReader();
            templateReader.ReadOperationalTemplate(reader, this);
        }

        void System.Xml.Serialization.IXmlSerializable.WriteXml(System.Xml.XmlWriter writer)
        {
            OperationalTemplateXmlWriter templateWriter = new OperationalTemplateXmlWriter();
            templateWriter.WriteOperationalTemplate(writer, this);
        }

        public static System.Xml.XmlQualifiedName GetXmlSchema(System.Xml.Schema.XmlSchemaSet xs)
        {
            LoadOperationalTemplateSchemas(xs);
            return new System.Xml.XmlQualifiedName("OPERATIONAL_TEMPLATE", RmXmlSerializer.OpenEhrNamespace);
        }

        static void LoadOperationalTemplateSchemas(System.Xml.Schema.XmlSchemaSet xs)
        {
            if (!xs.Contains(RmXmlSerializer.OpenEhrNamespace))
            {
                System.Xml.Schema.XmlSchema baseTypesSchema = RmXmlSerializer.GetOpenEhrSchema("BaseTypes");

                System.Xml.Schema.XmlSchema resourceSchema = RmXmlSerializer.GetOpenEhrSchema("Resource");
                resourceSchema.Includes.Clear();
                System.Xml.Schema.XmlSchemaInclude include = new System.Xml.Schema.XmlSchemaInclude();
                include.Schema = baseTypesSchema;
                resourceSchema.Includes.Add(include);

                System.Xml.Schema.XmlSchema archetypeSchema = RmXmlSerializer.GetOpenEhrSchema("Archetype");
                archetypeSchema.Includes.Clear();
                include = new System.Xml.Schema.XmlSchemaInclude();
                include.Schema = resourceSchema;
                archetypeSchema.Includes.Add(include);

                System.Xml.Schema.XmlSchema openEhrProfileSchema = RmXmlSerializer.GetOpenEhrSchema("OpenehrProfile");
                openEhrProfileSchema.Includes.Clear();
                include = new System.Xml.Schema.XmlSchemaInclude();
                include.Schema = archetypeSchema;
                openEhrProfileSchema.Includes.Add(include);

                System.Xml.Schema.XmlSchema templateSchema = RmXmlSerializer.GetOpenEhrSchema("Template");
                templateSchema.Includes.Clear();
                include = new System.Xml.Schema.XmlSchemaInclude();
                include.Schema = openEhrProfileSchema;
                templateSchema.Includes.Add(include);
                xs.Add(templateSchema);

                xs.Compile();
            }
        }
        #endregion
    }
}
