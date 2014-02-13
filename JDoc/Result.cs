using System;
using System.Collections.Generic;
using System.Linq;

namespace JDoc
{
    public abstract class Result
    {
        protected Result(bool success)
        {
            Success = success;
        }

        public bool Success { get; protected set; }
    }

    public abstract class SingleResult : Result
    {
        protected SingleResult(bool success) : base(success)
        {
        }

        public DocumentMeta Meta { get; set; }
    }

    public class DocumentNotFoundResult : SingleResult
    {
        public DocumentNotFoundResult() : base(false)
        {

        }
    }

    public abstract class MultiResult : Result
    {
        protected MultiResult(IEnumerable<SingleResult> results)
            : base(false)
        {
            Results = new List<SingleResult>(results);
            Success = Results.All(r => r.Success);
        }

        public IEnumerable<SingleResult> Results { get; private set; }
    }

    public class DocumentResult : SingleResult
    {
        public Document Document { get; private set; }

        public DocumentResult(Document document) : base(true)
        {
            Document = document;
        }
    }

    public class MultiDocumentResult : SingleResult
    {
        public IEnumerable<Document> Documents { get; set; }

        public MultiDocumentResult(IEnumerable<Document> documents) : base(true)
        {
            Documents = documents;
        }
    }
}