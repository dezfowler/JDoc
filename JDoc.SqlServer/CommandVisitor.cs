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
            if (document.Content == null) throw new ArgumentException("Document Content cannot be null", "document");

            return Task.Factory.StartNew<Result>(() =>
            {                
                var xmlElm = (XElement)document.Content.ToXNode();
                xmlElm.Add(new XAttribute(XNamespace.Xmlns.GetName("jxml"), JsonXml.JsonXmlSettings.JXmlNamespace),
                    new XAttribute(XNamespace.Xmlns.GetName("jsonx"), JsonXml.JsonXmlSettings.JsonNetXmlNamespace));

                using (var conn = new SqlConnection(_connectionString))
                using (var cmd = new SqlCommand())
                {
                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.CommandText = "dbo.StoreDocument";
                                        
                    if (document.Meta != null)
                    {
                        cmd.Parameters.Add(new SqlParameter("DocumentId", document.Meta.Id));
                        cmd.Parameters.Add(new SqlParameter("FriendlyId", document.Meta.Name));
                        cmd.Parameters.Add(new SqlParameter("RevisionEtag", document.Meta.RevisionEtag));

                    }

                    var xmlContent = new SqlXml(xmlElm.CreateReader());
                    cmd.Parameters.AddWithValue("Content", xmlContent);

                    using (var sqlReader = cmd.ExecuteReader())
                    {
                        return MapRecord(sqlReader);
                    }
                }                
            });
        }

        private Result MapRecord(SqlDataReader sqlReader)
        {
            if (!sqlReader.Read()) throw new Exception("Document error");

            var recordType = sqlReader.GetString(0);
            
            switch(recordType)
            {
                case "Document":
                    var doc = new Document
                    {
                        Meta = new DocumentMeta
                        {
                            Id = sqlReader.GetGuid(1),
                            Name = sqlReader.GetString(2),
                            RevisionEtag = sqlReader.GetGuid(3),
                            Created = DateTime.SpecifyKind(sqlReader.GetDateTime(4), DateTimeKind.Utc),
                            Modified = DateTime.SpecifyKind(sqlReader.GetDateTime(5), DateTimeKind.Utc),
                        },
                    };

                    var xmlContent = sqlReader.GetSqlXml(6).CreateReader();
                    var xmlDoc = XElement.Load(xmlContent);
                    doc.Content = (JObject)xmlDoc.ToJToken();

                    return new DocumentResult(doc);
                case "NotFound":
                    return new DocumentNotFoundResult();
                case "RevisionMismatch":
                    return new RevisionConflictResult
                    {
                        ExpectedRevision = sqlReader.GetGuid(1),
                        ActualRevision = sqlReader.GetGuid(2),
                    };
                case "IdentifierMismatch":
                    return new IdentifierMismatchResult();
                default:
                    throw new Exception("Not recognised");
            }            
        }

        public Task<Result> Visit(LoadDocumentCommand loadDocumentCommand)
        {
            return Task.Factory.StartNew<Result>(() =>
            {
                using (var conn = new SqlConnection(_connectionString))
                using (var cmd = new SqlCommand())
                {
                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.CommandText = "dbo.LoadDocument";

                    if (loadDocumentCommand.Id.Actual.HasValue)
                    {
                        cmd.Parameters.Add(new SqlParameter("DocumentId", loadDocumentCommand.Id.Actual.Value));
                    }

                    if (loadDocumentCommand.Id.Friendly != null)
                    {
                        cmd.Parameters.Add(new SqlParameter("FriendlyId", loadDocumentCommand.Id.Friendly));
                    }

                    if (loadDocumentCommand.Id.Revision.HasValue)
                    {
                        cmd.Parameters.Add(new SqlParameter("Revision", loadDocumentCommand.Id.Revision));
                    }

                    using (var sqlReader = cmd.ExecuteReader())
                    {
                        return MapRecord(sqlReader);
                    }

                }
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
