using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using Newtonsoft.Json;

namespace JDoc.Typed
{
    public class DocumentReferenceConverter : JsonConverter
    {
        private static readonly Type DocumentReferenceType = typeof(DocumentReference);

        private readonly List<DocumentReferencePosition> _references = new List<DocumentReferencePosition>();

        private readonly JsonSerializer _serializer = JsonSerializer.CreateDefault();

        public ReadOnlyCollection<DocumentReferencePosition> References
        {
            get
            {
                return new ReadOnlyCollection<DocumentReferencePosition>(this._references);
            }
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var documentReference = (DocumentReference)value;

            _references.Add(new DocumentReferencePosition { Path = writer.Path, Reference = documentReference, });

            _serializer.Serialize(writer, value);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            return _serializer.Deserialize(reader, objectType);
        }

        public override bool CanConvert(Type objectType)
        {
            return DocumentReferenceType.IsAssignableFrom(objectType);
        }
    }
}