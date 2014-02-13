using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using JDoc.Test;
using Newtonsoft.Json.Linq;

namespace JDoc.SqlServer.Test
{
    [TestClass]
    public class StoreDocumentTest : StoreDocumentIntegrationTestBase
    {
        protected override IProvider GetProvider()
        {
            //string databaseFilePath = Path.Combine(TestContext.TestDeploymentDir, "DocumentDatabase.mdf");
            //string connectionString = string.Format(@"Data Source=.\SQLEXPRESS;Integrated Security=true;AttachDBFilename={0};Initial Catalog=DocumentDatabase;", databaseFilePath);

            string connectionString = string.Format(@"Data Source=.\SQLEXPRESS;Initial Catalog=DocumentDatabase;Integrated Security=true;");

            return new SqlServerProvider(connectionString);
        }

        [TestInitialize]
        public void Init()
        {
            _provider = GetProvider();
            
            // TODO: Rollback to a snapshot

        }
    }

    public abstract class StoreDocumentIntegrationTestBase
    {
        protected IProvider _provider;

        protected abstract IProvider GetProvider();

        public TestContext TestContext { get; set; }
        
        [TestMethod]
        public void StoreNewDocument_DocumentShouldBeReturnedOnLoad()
        {
            var doc = new Document
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

            var storeResult = _provider.StoreDocument(doc).Result;

            Assert.IsNotNull(storeResult);
            Assert.IsNotNull(storeResult.Meta);

            var docId = storeResult.Meta.Id;
            Assert.AreNotEqual(Guid.Empty, docId);
            
            Assert.AreEqual(docId.ToString(), storeResult.Meta.Name);

            Assert.AreNotEqual(DateTime.MinValue, storeResult.Meta.Created);
            Assert.AreNotEqual(DateTime.MinValue, storeResult.Meta.Modified);
            
            var loadResult = _provider.LoadDocument(docId).Result;
            Assert.IsNotNull(loadResult);
            
            Assert.IsNotNull(loadResult.Meta);
            Assert.AreEqual(docId, loadResult.Meta.Id);

            Assert.AreEqual(storeResult.Meta.Name, loadResult.Meta.Name);
            Assert.AreEqual(storeResult.Meta.RevisionEtag, loadResult.Meta.RevisionEtag);
            Assert.AreEqual(storeResult.Meta.Created, loadResult.Meta.Created);
            Assert.AreEqual(storeResult.Meta.Modified, loadResult.Meta.Modified);

            Assert.IsNotNull(loadResult.Content);
            Assert.IsTrue(JObject.DeepEquals(doc.Content, loadResult.Content));

        }
    }
}
