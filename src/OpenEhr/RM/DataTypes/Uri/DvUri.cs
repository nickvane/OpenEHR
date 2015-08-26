using System;
using OpenEhr.DesignByContract;
using System.Text.RegularExpressions;
using OpenEhr.Attributes;
using OpenEhr.Serialisation;

namespace OpenEhr.RM.DataTypes.Uri
{
    [System.Xml.Serialization.XmlSchemaProvider("GetXmlSchema")]
    [Serializable]
    [RmType("openEHR", "DATA_TYPES", "DV_URI")]
    public class DvUri : Basic.DataValue, System.Xml.Serialization.IXmlSerializable
    {
        public DvUri()
        { }

        public DvUri(string value)
            : this()
        {
            SetBaseData(value);
            this.CheckInvariants();
        }

        // Generic URI syntax 
        // <scheme>://<authority><path>?<query>
        //
        // absoluteURI   = scheme ":" ( hier_part | opaque_part )
        // path          = [ abs_path | opaque_part ]
        // scheme        = alpha *( alpha | digit | "+" | "-" | "." )
        // hier_part     = ( net_path | abs_path ) [ "?" query ]
        // net_path      = "//" authority [ abs_path ]
        // authority     = server | reg_name
        // server        = [ [ userinfo "@" ] hostport ]
        // userinfo      = *( unreserved | escaped | ";" | ":" | "&" | "=" | "+" | "$" | "," )
        // hostport      = host [ ":" port ]
        // abs_path      = "/"  path_segments
        // path_segments = segment *( "/" segment )
        // opaque_part   = uric_no_slash *uric
        // uric_no_slash = unreserved | escaped | ";" | "?" | ":" | "@" | "&" | "=" | "+" | "$" | ","
        // uric          = reserved | unreserved | escaped
        // reserved      = ";" | "/" | "?" | ":" | "@" | "&" | "=" | "+" | "$" | ","
        // unreserved    = alphanum | mark
        // mark          = "-" | "_" | "." | "!" | "~" | "*" | "'" | "(" | ")"
        // escaped       = "%" hex hex
        // hex           = digit | "A" | "B" | "C" | "D" | "E" | "F" | "a" | "b" | "c" | "d" | "e" | "f"

        const string schemePattern = @"((?<scheme>[^:/\?#]+):)?";
        const string authorityPattern = @"(//(?<authority>[^/\?#]*))?";
        const string pathPattern = @"(?<path>[^\?#]*)?";
        const string queryPattern = @"(?<query>(\?[^#]*))?";
        const string fragmentPattern = @"(\#(?<fragment>.*))?";

        const string uriPattern = @"^" + schemePattern + authorityPattern + pathPattern + queryPattern + fragmentPattern;

        // CM: 31/10/08 as Peter indicated that functions used in preconditions must be public so that the 
        // exception can be caught. Since it is a static function, so it still adheres the openEHR RM spec.        
        public static bool IsValidUri(string uriValue)
        {
            return GetMatch(uriValue).Success;
        }


        private string value;

        public string Value
        {
            get
            {
                return this.value;
            }
        }

        string scheme;

        public string Scheme
        {
            get
            {
                Check.Require(!string.IsNullOrEmpty(this.Value), "Value must not be null or empty.");
                if (scheme == null)
                    SetProperties(GetMatch(this.value));
                return scheme;
            }
        }

        string path;

        public string Path
        {
            get
            {
                Check.Require(!string.IsNullOrEmpty(this.Value), "Value must not be null or empty.");
                if (path == null)
                    SetProperties(GetMatch(this.value));
                return this.path;
            }
            
        }

        string fragmentId;

        public string FragmentId
        {
            get
            {
                Check.Require(!string.IsNullOrEmpty(this.Value), "Value must not be null or empty.");
                if (fragmentId == null)
                    SetProperties(GetMatch(this.value));
                return this.fragmentId;
            }
        }

        string query;

        public string Query
        {
            get
            {
                Check.Require(!string.IsNullOrEmpty(this.Value), "Value must not be null or empty.");
                if (query == null)
                    SetProperties(GetMatch(this.value));
                return this.query;
            }
        }

        private void SetProperties(Match thisMatch)
        {
            GroupCollection gCollection = thisMatch.Groups;

            // assign values           
            this.scheme = gCollection["scheme"].Value;
            this.authority = gCollection["authority"].Value;
            this.path = gCollection["authority"].Value + gCollection["path"].Value;
            this.query = gCollection["query"].Value;
            this.fragmentId = gCollection["fragment"].Value;
        }

        static Match GetMatch(string uriValue)
        {
            return Regex.Match(uriValue, uriPattern, RegexOptions.Compiled | RegexOptions.Singleline);
        }

        public override string ToString()
        {
            return this.Value;
        }

        protected void SetBaseData(string uriValue)
        {
            Match thisMatch = GetMatch(uriValue);
            Check.Require(thisMatch.Success, "value must be valid uri value: " + uriValue);

            this.value = uriValue;
            SetProperties(thisMatch);
        }

        string authority;

        protected string Authority
        {
            get
            {
                Check.Require(!string.IsNullOrEmpty(this.Value), "Value must not be null or empty.");
                if (authority == null)
                    SetProperties(GetMatch(this.value));
                return authority;
        }
        }

        protected override void ReadXmlBase(System.Xml.XmlReader reader)
        {
            // Get value
            Check.Assert(reader.LocalName == "value", "reader.LocalName must be 'value' rathern than " + reader.LocalName);
            this.value = reader.ReadElementString("value",
                RmXmlSerializer.OpenEhrNamespace);

            reader.MoveToContent();

        }

        protected override void WriteXmlBase(System.Xml.XmlWriter writer)
        {
            DesignByContract.Check.Require(!string.IsNullOrEmpty(this.Value), "value must not be null or empty.");

            string prefix = RmXmlSerializer.UseOpenEhrPrefix(writer);           
            writer.WriteElementString(prefix, "value", RmXmlSerializer.OpenEhrNamespace, this.Value);
        }

        public static System.Xml.XmlQualifiedName GetXmlSchema(System.Xml.Schema.XmlSchemaSet xs)
        {
            RmXmlSerializer.LoadBaseTypesSchema(xs);
            return new System.Xml.XmlQualifiedName("DV_URI", RmXmlSerializer.OpenEhrNamespace);
        }

        protected override void CheckInvariants()
        {
            Check.Invariant(!string.IsNullOrEmpty(this.Value), "value must not be null or empty.");
        }

        #region IXmlSerializable Members

        System.Xml.Schema.XmlSchema System.Xml.Serialization.IXmlSerializable.GetSchema()
        {
            throw new Exception("The method or operation is not implemented.");
        }

        void System.Xml.Serialization.IXmlSerializable.ReadXml(System.Xml.XmlReader reader)
        {
            this.ReadXml(reader);
        }

        void System.Xml.Serialization.IXmlSerializable.WriteXml(System.Xml.XmlWriter writer)
        {
            this.WriteXml(writer);
        }

        #endregion

        public static bool operator ==(DvUri a, DvUri b)
        {
            if ((object)a != null)
                return a.Equals(b);

            // a is null, check if b is null
            if ((object)b != null)
                return false;
            else
                return true;
    }

        public static bool operator !=(DvUri a, DvUri b)
        {            
            return !(a == b);
        }

        public override bool Equals(System.Object obj)
        {
            // If parameter is null or not a DvUri return false.
            DvUri uri = obj as DvUri;
            if ((object)uri == null)
                return false;

            // Return true if the value match
            if (this.GetType() == uri.GetType())
                return this.Value == uri.Value;
            else
                return false;
        }
    }
}
