using System;

namespace JDoc
{
    public class RevisionConflictResult : SingleResult
    {
        public RevisionConflictResult() : base(false)
        {
        }

        public Guid ExpectedRevision { get; set; }

        public Guid ActualRevision { get; set; }
    }

    /// <summary>
    /// Indicates that both ID and Name identifiers were specified for a command 
    /// but that they corresponded to different documents.
    /// </summary>
    public class IdentifierMismatchResult : SingleResult
    {
        public IdentifierMismatchResult() : base(false)
        {
        }
    }

    [Serializable]
    public class ProviderException : Exception
    {
        public ProviderException() { }
        public ProviderException(string message) : base(message) { }
        public ProviderException(string message, Exception inner) : base(message, inner) { }

        public ProviderException(Result errorResult)
        {
            ErrorResult = errorResult;
        }

        protected ProviderException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }

        public Result ErrorResult { get; private set; }
    }
}