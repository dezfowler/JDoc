using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using JDoc.Commands;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Collections;

namespace JDoc.Test
{
    [TestClass]
    public class InMemoryProviderTest
    {
        [TestMethod]
        public void TestStore()
        {
            var expectedJson = @"{""TestProp"": 5 }";
            var provider = new InMemoryProvider();
            var doc = new Document
            {
                Content = JObject.Parse(expectedJson),
                Meta = new DocumentMeta
                {
                    Name = "DocName",
                }
            };

            var result = provider.StoreDocument(doc).Result;
            Assert.AreNotEqual(Guid.Empty, doc.Meta.Id);

            Assert.IsTrue(provider.Documents.Contains(doc));

            //var doc2 = provider.LoadDocument(doc.Meta.Id).Result;
            //Assert.IsNotNull(doc2);
            //Assert.AreNotSame(doc, doc2);
            //Assert.AreEqual(expectedJson, doc2.Content.ToString());
        }

        [TestMethod]
        public void TestLoad()
        {
            var docId = new Guid("6a0088f0-06fa-46c9-8f3f-30e09bec1cc7");
            var expectedJson = @"{""TestProp"": 5 }";
            var provider = new InMemoryProvider 
            {
                Documents = 
                {
                    new Document
                    {
                        Content = JObject.Parse(expectedJson),
                        Meta = new DocumentMeta
                        {
                            Id = docId,
                            Name = "DocName",
                        }
                    }
                }
            };
                        
            var result = provider.LoadDocument(docId).Result;
            Assert.IsNotNull(result);
            Assert.AreEqual("DocName", result.Meta.Name);
        }
    }

    public class MetaComparer : EqualityComparer<DocumentMeta>
    {
        public override bool Equals(DocumentMeta x, DocumentMeta y)
        {
            if (x == null && y == null)
            {
                return true;
            }

            if (x == null || y == null)
            {
                return false;
            }

            return Guid.Equals(x.Id, y.Id)
                && string.Equals(x.Name, y.Name)
                && Guid.Equals(x.RevisionEtag, y.RevisionEtag)
                && DateTime.Equals(x.Created, y.Created)
                && DateTime.Equals(x.Modified, y.Modified)
                && StructuralComparisons.StructuralEqualityComparer.Equals(x.Tags, y.Tags);
        }

        public override int GetHashCode(DocumentMeta obj)
        {
            throw new NotImplementedException();
        }
    }

    public class DocumentComparer : EqualityComparer<Document>
    {
        public override bool Equals(Document x, Document y)
        {
            if (x == null && y == null)
            {
                return true;
            }

            if (x == null || y == null)
            {
                return false;
            }

            return new MetaComparer().Equals(x.Meta, y.Meta)
                && JObject.DeepEquals(x.Content, y.Content);
        }

        public override int GetHashCode(Document obj)
        {
            throw new NotImplementedException();
        }
    }

    public class InMemoryProvider : IProvider
    {
        private List<Document> _docs = new List<Document>();

        public System.Threading.Tasks.Task<Result> ExecuteCommand(Command command)
        {
            return command.Accept(new CommandVisitor(this));
        }

        public System.Threading.Tasks.Task<System.Collections.Generic.IEnumerable<Result>> ExecuteBatch(System.Collections.Generic.IEnumerable<Command> commands)
        {
            throw new NotImplementedException();
        }

        public System.Collections.Generic.IEnumerable<CommandType> SupportedCommands
        {
            get { throw new NotImplementedException(); }
        }

        class CommandVisitor : ICommandVisitor
        {
            private InMemoryProvider inMemoryProvider;

            public CommandVisitor(InMemoryProvider inMemoryProvider)
            {
                // TODO: Complete member initialization
                this.inMemoryProvider = inMemoryProvider;
            }

            public System.Threading.Tasks.Task<Result> Visit(StoreDocumentCommand storeDocumentCommand)
            {
                return Task.Factory.StartNew<Result>(() =>
                {
                    var document = storeDocumentCommand.Document;
                    if (document.Meta.Id == Guid.Empty)
                    {
                        document.Meta.Id = Guid.NewGuid();
                    }

                    inMemoryProvider._docs.Add(document);

                    return new DocumentResult(document);
                });
            }

            public System.Threading.Tasks.Task<Result> Visit(LoadDocumentCommand loadDocumentCommand)
            {
                return Task.Factory.StartNew<Result>(() =>
                {
                    var id = loadDocumentCommand.Id;

                    var doc = inMemoryProvider._docs.FirstOrDefault(d => d.Meta.Id == id.Actual.Value);

                    if (doc != null)
                    {
                        return new DocumentResult(doc);
                    }

                    return new DocumentNotFoundResult();
                });
            }

            public System.Threading.Tasks.Task<Result> Visit(PatchCommand patchCommand)
            {
                throw new NotImplementedException();
            }

            public System.Threading.Tasks.Task<Result> Visit(QueryCommand queryCommand)
            {
                throw new NotImplementedException();
            }
        }

        public List<Document> Documents { get { return _docs; } }
    }    
}
