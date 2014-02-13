namespace JDoc.Typed
{
    public class TypedDocument<T>
    {
        public DocumentMeta Meta { get; set; }

        public T Content { get; set; }
    }
}