using OpenEhr.DesignByContract;
using OpenEhr.Futures.OperationalTemplate;
using OpenEhr.RM.Composition;

namespace OpenEhr.Validation
{
    public static class ValidatingAgent
    {
        public static bool Validate(Composition composition, OperationalTemplate template, 
            string context, System.Collections.Generic.List<string> validationErrors)
        {
            Check.Require(composition != null, "composition must not be null.");
            Check.Require(template != null, "template must not be null.");

            ErrorLog errorLog = new ErrorLog(context);
            bool isValid = true;

            if (string.IsNullOrEmpty(composition.ArchetypeDetails.TemplateId.Value))
            {
                errorLog.LogError("Composition TemplateId is missing for " + composition.Concept);
                isValid = false;
            }
            if (string.IsNullOrEmpty(template.TemplateId.Value))
            {
                errorLog.LogError("Operational template TemplateId is missing for " + template.Concept);
                isValid = false;
            }
            if (isValid && composition.ArchetypeDetails.TemplateId.Value != template.TemplateId.Value)
            {
                errorLog.LogError(string.Format("Operational TemplateId {0} does not match Composition TemplateId {1}",
                    template.TemplateId.Value, composition.ArchetypeDetails.TemplateId.Value));

                isValid = false;
            }

            isValid &= Validate(composition, template, 
                delegate(object sender, ValidationEventArgs e) { errorLog.LogError(e); });

            if (!isValid && errorLog.Log.Count <= 0)
                errorLog.LogError("The composition was invalid and no validation errors were logged.");

            if (!isValid)
                validationErrors.AddRange(errorLog.Log);

            return isValid;
        }

        public static bool Validate(Composition composition, OperationalTemplate template,
            AcceptValidationError acceptErrorDelegate)
        {
            Check.Require(composition != null, "composition must not be null.");
            Check.Require(template != null, "template must not be null.");

            template.Definition.SetValidationContext(new ValidationContext(acceptErrorDelegate, null));

            return template.Definition.ValidValue(composition);
        }

        private class ErrorLog
        {
            private string context;

            public ErrorLog(string context)
            {
                this.context = context;
            }

            public void LogError(string message)
            {
                Check.Require(!string.IsNullOrEmpty(message), "message must not be null or empty");
                if (!string.IsNullOrEmpty(context))
                    message = string.Format("[{0}] {1}", context, message);

                log.Add(message);

#if TRACE
                System.Diagnostics.Trace.WriteLine(message);
#endif
            }
            
            public void LogError(ValidationEventArgs e)
            {
                LogError( string.Format("{0} {1}",e.Message, e.Path));
            }

            private System.Collections.Generic.List<string> log 
                = new System.Collections.Generic.List<string>();

            public System.Collections.Generic.ICollection<string> Log
            {
                get { return log; }
            }
        }

    }
}
