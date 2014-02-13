using JDoc.Client;
using JDoc.Commands;
using JsonXml;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace JDoc.SqlServer
{
    internal class CommandVisitor : ICommandVisitor
    {
        private readonly string _connectionString;

        public CommandVisitor(string connectionString)
        {
            _connectionString = connectionString;
        }

        public Task<Result> Visit(StoreDocumentCommand storeDocumentCommand)
        {
            var document = storeDocumentCommand.Document;
            if (document == null) throw new ArgumentNullException("document");
            //if (document.Id == Guid.Empty) throw new ArgumentException("Document ID cannot be empty Guid", "document");
            if (document.Meta == null) throw new ArgumentException("Document Meta cannot be null", "document");
            //if (document.Meta.RevisionEtag == Guid.Empty) throw new ArgumentException("Document Meta RevisionEtag cannot be empty Guid", "document");
            if (document.Content == null) throw new ArgumentException("Document Content cannot be null", "document");

            return Task.Factory.StartNew<Result>(() =>
            {                
                var xmlElm = (XElement)document.Content.ToXNode();
                xmlElm.Add(new XAttribute(XNamespace.Xmlns.GetName("jxml"), JsonXml.JsonXmlSettings.JXmlNamespace),
                    new XAttribute(XNamespace.Xmlns.GetName("jsonx"), JsonXml.JsonXmlSettings.JsonNetXmlNamespace));

                Document doc = null;

                using (var conn = new SqlConnection(_connectionString))
                using (var cmd = new SqlCommand())
                {
                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.CommandText = "dbo.StoreDocument";

                    var xmlContent = new SqlXml(xmlElm.CreateReader());

                    cmd.Parameters.Add(new SqlParameter("DocumentId", document.Meta.Id));
                    cmd.Parameters.Add(new SqlParameter("FriendlyId", document.Meta.Name));
                    cmd.Parameters.Add(new SqlParameter("RevisionEtag", document.Meta.RevisionEtag));
                    cmd.Parameters.AddWithValue("Content", xmlContent);

                    using (var sqlReader = cmd.ExecuteReader())
                    {
                        if (!sqlReader.Read()) throw new Exception("Document error");

                        doc = new Document
                        {
                            Meta = new DocumentMeta
                            {
                                Id = sqlReader.GetGuid(0),
                                Name = sqlReader.GetString(1),
                                RevisionEtag = sqlReader.GetGuid(2),
                                Created = DateTime.SpecifyKind(sqlReader.GetDateTime(3), DateTimeKind.Utc),
                                Modified = DateTime.SpecifyKind(sqlReader.GetDateTime(4), DateTimeKind.Utc),                                
                            },

                            Content = document.Content,
                        };

                        sqlReader.Close();
                    }

                    conn.Close();
                }

                return new DocumentResult(doc);
            });
        }

        public Task<Result> Visit(LoadDocumentCommand loadDocumentCommand)
        {
            return Task.Factory.StartNew<Result>(() =>
            {
                Document doc = null;

                using (var conn = new SqlConnection(_connectionString))
                using (var cmd = new SqlCommand())
                {
                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.CommandText = "dbo.LoadDocument";

                    //cmd.Parameters.Add(new SqlParameter("CollectionName", loadDocumentCommand.CollectionName));
                    cmd.Parameters.Add(new SqlParameter("DocumentId", loadDocumentCommand.Id.Actual.Value));

                    if (loadDocumentCommand.Revision.HasValue)
                    {
                        cmd.Parameters.Add(new SqlParameter("Revision", loadDocumentCommand.Revision));
                    }

                    using (var sqlReader = cmd.ExecuteReader())
                    {
                        if (!sqlReader.Read()) throw new Exception("Document error");

                        //SELECT TOP 1 @DbDocId DocumentId, FriendlyID, RevisionEtag, @Created Created, Modified, Content 

                        doc = new Document
                        {
                            Meta = new DocumentMeta
                            {
                                Id = sqlReader.GetGuid(0),
                                Name = sqlReader.GetString(1),
                                RevisionEtag = sqlReader.GetGuid(2),
                                Created = DateTime.SpecifyKind(sqlReader.GetDateTime(3), DateTimeKind.Utc),
                                Modified = DateTime.SpecifyKind(sqlReader.GetDateTime(4), DateTimeKind.Utc),
                            },
                        };

                        var xmlContent = sqlReader.GetSqlXml(5).CreateReader();
                        var xmlDoc = XElement.Load(xmlContent);
                        doc.Content = (JObject)xmlDoc.ToJToken();

                        sqlReader.Close();
                    }

                    conn.Close();
                }

                return new DocumentResult(doc);
            });
        }

        public Task<Result> Visit(PatchCommand patchCommand)
        {
            throw new NotImplementedException();
        }

        public Task<Result> Visit(QueryCommand queryCommand)
        {
            throw new NotImplementedException();
        }


        public Task<Result> CommandTask { get; set; }
    }
}
