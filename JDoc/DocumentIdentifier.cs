using System;

namespace JDoc
{
    public class DocumentIdentifier
    {
        private readonly object _value;
        
        public DocumentIdentifier(Guid id, Guid? revision = null)
        {
            Actual = id;
            Revision = revision;
        }

        public DocumentIdentifier(string name, Guid? revision = null)
        {
            Friendly = name;
            Revision = revision;
        }

        public Guid? Actual { get; set; }

        public string Friendly { get; set; }

        public Guid? Revision { get; set; }

        public static implicit operator DocumentIdentifier(string id)
        {
            return new DocumentIdentifier(id);
        }

        public static implicit operator DocumentIdentifier(Guid id)
        {
            return new DocumentIdentifier(id);
        }
    }
}