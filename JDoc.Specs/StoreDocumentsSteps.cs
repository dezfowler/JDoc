using System;
using System.Data.SqlClient;
using System.Linq;
using JDoc.SqlServer;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using TechTalk.SpecFlow;

namespace JDoc.Specs
{
    [Binding]
    public class StoreDocumentsSteps
    {
        IProviderTestHarness _providerTestHarness = new SqlServerProviderHarnesss();
        IProvider _provider = null;
        private Exception _thrownException;
        private Document _loadResult;
        private Document _expectedCurrentlyStoredDocument;
        private Document _originalStoredDocument;
        
        [BeforeScenario]
        public void InitScenario()
        {
            _providerTestHarness.Reset();
            _provider = _providerTestHarness.CreateProvider();
            
            _originalStoredDocument = null;
            _expectedCurrentlyStoredDocument = null;
        }

        [Given]
        public void Given_I_have_no_existing_matching_document()
        {            
            // No existing documents
            _expectedCurrentlyStoredDocument = new Document
            {
                Meta = new DocumentMeta
                {
                    Id = Guid.NewGuid(),
                    Name = "DoesNotExist",
                },
            };
        }

        [Given]
        public void Given_I_have_an_existing_matching_document()
        {
            _originalStoredDocument = new Document
            {
                Content = new JObject
                {
                    { "TestProp", 5 },
                    { "Other", true },
                    { "Erm", TimeSpan.FromMinutes(15) }
                },

                Meta = new DocumentMeta
                {
                    Name = "DocumentName"
                }
            };

            var result = _provider.StoreDocument(_originalStoredDocument).Result;

            _originalStoredDocument.Meta = result.Meta.Clone();

            _expectedCurrentlyStoredDocument = _originalStoredDocument;
        }

        [When]
        public void When_I_store_a_document_with_no_name()
        {
            _expectedCurrentlyStoredDocument = new Document
            {
                Content = new JObject
                {
                    { "TestProp", 5 },
                    { "Other", true },
                    { "Erm", TimeSpan.FromMinutes(15) }
                },

                Meta = new DocumentMeta
                {
                    // No Id or Name
                }
            };

            var storeResult = _provider.StoreDocument(_expectedCurrentlyStoredDocument).Result;

            Assert.IsNotNull(storeResult);
            Assert.IsNotNull(storeResult.Meta);
                        
            Assert.AreNotEqual(Guid.Empty, storeResult.Meta.Id);
            Assert.AreNotEqual(Guid.Empty, storeResult.Meta.RevisionEtag);

            Assert.AreEqual(storeResult.Meta.Id.ToString(), storeResult.Meta.Name, "Generated name should be the same as the document ID");

            Assert.AreNotEqual(DateTime.MinValue, storeResult.Meta.Created);
            Assert.AreNotEqual(DateTime.MinValue, storeResult.Meta.Modified);

            Assert.IsTrue(JObject.DeepEquals(_expectedCurrentlyStoredDocument.Content, storeResult.Content));

            _expectedCurrentlyStoredDocument = storeResult;
        }

        [When]
        public void When_I_store_a_document_with_a_name()
        {
            _expectedCurrentlyStoredDocument = new Document
            {
                Content = new JObject
                {
                    { "TestProp", 5 },
                    { "Other", true },
                    { "Erm", TimeSpan.FromMinutes(15) }
                },

                Meta = new DocumentMeta
                {
                    Name = "DocumentName"
                }
            };

            var storeResult = _provider.StoreDocument(_expectedCurrentlyStoredDocument).Result;

            Assert.IsNotNull(storeResult);
            Assert.IsNotNull(storeResult.Meta);

            Assert.AreNotEqual(Guid.Empty, storeResult.Meta.Id);
            Assert.AreNotEqual(Guid.Empty, storeResult.Meta.RevisionEtag);

            Assert.AreEqual("DocumentName", storeResult.Meta.Name);

            Assert.AreNotEqual(DateTime.MinValue, storeResult.Meta.Created);
            Assert.AreNotEqual(DateTime.MinValue, storeResult.Meta.Modified);

            Assert.IsTrue(JObject.DeepEquals(_expectedCurrentlyStoredDocument.Content, storeResult.Content));

            _expectedCurrentlyStoredDocument = storeResult;
        }

        [When]
        public void When_I_store_a_document_with_a_matching_ID_and_revision()
        {
            var expectedDocument = new Document
            {
                Content = new JObject
                {
                    // this property changed
                    { "TestProp", 7 },
                    { "Other", true },
                    { "Erm", TimeSpan.FromMinutes(15) },
                },

                Meta = _originalStoredDocument.Meta.Clone(),
            };

            // Existing document has a name - make sure it's not overwriten
            expectedDocument.Meta.Name = null;

            var storeResult = _provider.StoreDocument(expectedDocument).Result;

            Assert.IsNotNull(storeResult);
            Assert.IsNotNull(storeResult.Meta);

            Assert.AreEqual(_originalStoredDocument.Meta.Id, storeResult.Meta.Id);
            Assert.AreNotEqual(_originalStoredDocument.Meta.RevisionEtag, storeResult.Meta.RevisionEtag, "Revision should have changed");

            Assert.AreEqual(_originalStoredDocument.Meta.Name, storeResult.Meta.Name, "Name should match the existing document, i.e. not overwritten with null");

            Assert.AreEqual(_originalStoredDocument.Meta.Created, storeResult.Meta.Created, "Should have same created date");
            Assert.AreNotEqual(_originalStoredDocument.Meta.Modified, storeResult.Meta.Modified);

            Assert.IsTrue(JObject.DeepEquals(expectedDocument.Content, storeResult.Content));

            _expectedCurrentlyStoredDocument = storeResult;
        }

        [When]
        public void When_I_store_a_document_with_a_matching_name_and_revision()
        {
            var expectedDocument = new Document
            {
                Content = new JObject
                {
                    // this property changed
                    { "TestProp", 7 },
                    { "Other", true },
                    { "Erm", TimeSpan.FromMinutes(15) },
                },

                Meta = _originalStoredDocument.Meta.Clone(),
            };

            // Existing document has a name - make sure it's not overwriten
            expectedDocument.Meta.Id = Guid.Empty;

            var storeResult = _provider.StoreDocument(expectedDocument).Result;

            Assert.IsNotNull(storeResult);
            Assert.IsNotNull(storeResult.Meta);

            Assert.AreEqual(_originalStoredDocument.Meta.Id, storeResult.Meta.Id);
            Assert.AreNotEqual(_originalStoredDocument.Meta.RevisionEtag, storeResult.Meta.RevisionEtag, "Revision should have changed");

            Assert.AreEqual(_originalStoredDocument.Meta.Name, storeResult.Meta.Name, "Name should match the existing document, i.e. not overwritten with null");

            Assert.AreEqual(_originalStoredDocument.Meta.Created, storeResult.Meta.Created, "Should have same created date");
            Assert.AreNotEqual(_originalStoredDocument.Meta.Modified, storeResult.Meta.Modified);

            Assert.IsTrue(JObject.DeepEquals(expectedDocument.Content, storeResult.Content));

            _expectedCurrentlyStoredDocument = storeResult;
        }

        [When]
        public void When_I_store_a_document_with_a_matching_name_and_non_matching_revision()
        {
            var expectedDocument = new Document
            {
                Content = new JObject
                {
                    // this property changed
                    { "TestProp", 7 },
                    { "Other", true },
                    { "Erm", TimeSpan.FromMinutes(15) },
                },

                Meta = _originalStoredDocument.Meta.Clone(),
            };

            // Empty the revision - we should get an exception
            expectedDocument.Meta.RevisionEtag = Guid.Empty;

            try
            {
                var result = _provider.StoreDocument(expectedDocument).Result;
            }
            catch (AggregateException aggEx)
            {
                _thrownException = aggEx.InnerExceptions.Single();
            }
        }

        [When]
        public void When_I_load_the_document_by_ID()
        {
            try
            {
                _loadResult = _provider.LoadDocument(_expectedCurrentlyStoredDocument.Meta.Id).Result;
            }
            catch (AggregateException aggEx)
            {
                _thrownException = aggEx.InnerExceptions.Single();
            }
        }

        [When]
        public void When_I_load_the_document_by_name()
        {
            try
            {
                _loadResult = _provider.LoadDocument(_expectedCurrentlyStoredDocument.Meta.Name).Result;
            }
            catch (AggregateException aggEx)
            {
                _thrownException = aggEx.InnerExceptions.Single();
            }
        }

        [Then]
        public void Then_I_should_encounter_a_revision_mismatch_error()
        {
            var unwrappedException = _thrownException.Unwrap();
            Assert.IsInstanceOf<ProviderException>(unwrappedException);
            var providerException = (ProviderException)unwrappedException;
            var revisionError = providerException.ErrorResult as RevisionConflictResult;
            Assert.IsInstanceOf<RevisionConflictResult>(revisionError);
            Assert.AreEqual(_originalStoredDocument.Meta.RevisionEtag, revisionError.ExpectedRevision);
            Assert.AreNotEqual(_originalStoredDocument.Meta.RevisionEtag, revisionError.ActualRevision);
        }

        [Then]
        public void Then_the_correct_document_should_be_returned()
        {
            AssertDocuments(_expectedCurrentlyStoredDocument, _loadResult);
        }

        [Then]
        public void Then_I_should_encounter_a_not_found_error()
        {
            var unwrappedException = _thrownException.Unwrap();
            Assert.IsInstanceOf<ProviderException>(unwrappedException);
            var providerException = (ProviderException)unwrappedException;
            Assert.IsInstanceOf<DocumentNotFoundResult>(providerException.ErrorResult);
        }

        private void AssertDocuments(Document expected, Document actual)
        {
            Assert.IsNotNull(actual);
            Assert.IsNotNull(actual.Meta);
            Assert.AreEqual(expected.Meta.Id, actual.Meta.Id);

            Assert.AreEqual(expected.Meta.Name, actual.Meta.Name);
            Assert.AreEqual(expected.Meta.RevisionEtag, actual.Meta.RevisionEtag);
            Assert.AreEqual(expected.Meta.Created, actual.Meta.Created);
            Assert.AreEqual(expected.Meta.Modified, actual.Meta.Modified);

            Assert.IsNotNull(actual.Content);
            Assert.IsTrue(JObject.DeepEquals(expected.Content, actual.Content));
        }
    }

    public static class ExceptionExtensions
    {
        public static Exception Unwrap(this Exception exception)
        {
            var aggEx = exception as AggregateException;
            if (aggEx != null)
            {
                return aggEx.InnerException;
            }

            return exception;
        }
    }

    public interface IProviderTestHarness
    {
        void Reset();

        IProvider CreateProvider();
    }

    public class SqlServerProviderHarnesss : IProviderTestHarness
    {
        string _connectionString = @"Data Source=.\SQLEXPRESS;Initial Catalog=DocumentDatabase;Integrated Security=true;";

        public void Reset()
        {
            using (var conn = new SqlConnection(_connectionString))
            using(var cmd = conn.CreateCommand())
            {
                cmd.CommandText = @"
TRUNCATE TABLE dbo.DocumentReference
TRUNCATE TABLE dbo.DocumentRevision
TRUNCATE TABLE dbo.Document";

                conn.Open();
                cmd.ExecuteNonQuery();                
            }
        }

        public IProvider CreateProvider()
        {
            return new SqlServerProvider(_connectionString);
        }
    }
}
