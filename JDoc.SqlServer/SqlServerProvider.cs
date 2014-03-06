using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Threading.Tasks;
using System.Xml.Linq;
using JsonXml;
using Newtonsoft.Json.Linq;

namespace JDoc.SqlServer
{
    public class SqlServerProvider : IProvider
    {
        private readonly string _connectionString;

        public SqlServerProvider(string connectionString)
        {
            _connectionString = connectionString;
        }

        public Task<Result> ExecuteCommand(Command command)
        {
            var visitor = new CommandVisitor(_connectionString);
            return command.Accept(visitor);
        }

        public Task<IEnumerable<Result>> ExecuteBatch(IEnumerable<Command> commands)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<CommandType> SupportedCommands
        {
            get { throw new NotImplementedException(); }
        }
    }
}