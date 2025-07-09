using System.Data;

using NSubstitute;

using NUnit.Framework;

using Xperience.Manager.Services;

namespace Xperience.Manager.Tests.Services
{
    /// <summary>
    /// Tests for <see cref="SqlExecutor"/>.
    /// </summary>
    public class SqlExecutorTests
    {
        private const string COL_1 = "First";
        private const string COL_2 = "Second";
        private const string VAL_1 = "Value1";
        private const string VAL_2 = "Value2";
        private const string CONN_STRING = "TestConnectionString";
        private const string VALID_SCRIPT = "EnabledUsersWithAdminAccess.sql";
        private const string SCRIPT_TEXT = @"SELECT
	UserID,
	UserName
FROM
	CMS_User
WHERE
	UserEnabled = 1
	AND UserAdministrationAccess = 1";


        private readonly IDbCommand command = Substitute.For<IDbCommand>();
        private readonly IDbConnection connection = Substitute.For<IDbConnection>();
        private readonly IDbDataParameter parameter = Substitute.For<IDbDataParameter>();
        private readonly IDataParameterCollection parameterCollection = Substitute.For<IDataParameterCollection>();


        [TearDown]
        public void TearDown() => connection.ClearReceivedCalls();


        [Test]
        public async Task ExecuteQuery_ValidScript_ExecutesAndReturnsData()
        {
            var factory = GetFactory();
            var sqlExecutor = new SqlExecutor(factory);

            var data = await sqlExecutor.ExecuteQuery(CONN_STRING, VALID_SCRIPT);

            Assert.Multiple(() =>
            {
                connection.Received().Open();
                command.Received().ExecuteReader();
                Assert.That(command.CommandText, Is.EqualTo(SCRIPT_TEXT));
                Assert.That(data.Count(), Is.EqualTo(2));
                Assert.That(data.FirstOrDefault()?.Property(COL_1)?.Value.ToString(), Is.EqualTo(VAL_1));
                Assert.That(data.FirstOrDefault()?.Property(COL_2)?.Value.ToString(), Is.EqualTo(VAL_2));

            });
        }


        [Test]
        public void ExecuteQuery_InvalidScript_Throws()
        {
            var factory = GetFactory();
            var sqlExecutor = new SqlExecutor(factory);
            string queryName = "InvalidScript.sql";

            Assert.Multiple(() =>
            {
                Assert.ThrowsAsync<InvalidOperationException>(async () => await sqlExecutor.ExecuteQuery(CONN_STRING, queryName));
                connection.DidNotReceive().Open();
                command.DidNotReceive().ExecuteReader();
            });
        }


        [Test]
        public async Task ExecuteNonQuery_ValidScript_Executes()
        {
            var factory = GetFactory();
            var sqlExecutor = new SqlExecutor(factory);

            await sqlExecutor.ExecuteNonQuery(CONN_STRING, VALID_SCRIPT);

            Assert.Multiple(() =>
            {
                connection.Received().Open();
                command.Received().ExecuteNonQuery();
                Assert.That(command.CommandText, Is.EqualTo(SCRIPT_TEXT));

            });
        }


        [Test]
        public async Task ExecuteNonQuery_WithParameters_AddsParameters()
        {
            var factory = GetFactory();
            var sqlExecutor = new SqlExecutor(factory);
            string param1Name = "@K1";
            string param1Value = "V1";
            Dictionary<string, object> parameters = new()
            {
                { param1Name, param1Value }
            };

            await sqlExecutor.ExecuteNonQuery(CONN_STRING, VALID_SCRIPT, parameters);

            Assert.Multiple(() =>
            {
                connection.Received().Open();
                command.Received().ExecuteNonQuery();
                Assert.That(parameter.ParameterName, Is.EqualTo(param1Name));
                Assert.That(parameter.Value, Is.EqualTo(param1Value));

            });
        }


        [Test]
        public void ExecuteNonQuery_InvalidScript_Throws()
        {
            var factory = GetFactory();
            var sqlExecutor = new SqlExecutor(factory);
            string queryName = "InvalidScript.sql";

            Assert.Multiple(() =>
            {
                Assert.ThrowsAsync<InvalidOperationException>(async () => await sqlExecutor.ExecuteNonQuery(CONN_STRING, queryName));
                connection.DidNotReceive().Open();
                command.DidNotReceive().ExecuteReader();
            });
        }


        private IConnectionFactory GetFactory()
        {
            var reader = Substitute.For<IDataReader>();
            reader.FieldCount.Returns(2);
            reader.GetName(0).Returns(COL_1);
            reader.GetName(1).Returns(COL_2);
            reader.GetFieldType(Arg.Any<int>()).Returns(typeof(string));
            reader.GetValue(0).Returns(VAL_1);
            reader.GetValue(1).Returns(VAL_2);
            reader.Read().Returns(
              x => true,
              x => true,
              x => false
            );

            command.ExecuteReader().Returns(reader);
            command.CreateParameter().Returns(parameter);
            command.Parameters.Returns(parameterCollection);
            connection.CreateCommand().Returns(command);

            var factory = Substitute.For<IConnectionFactory>();
            factory.GetConnection(Arg.Any<string>()).Returns(connection);

            return factory;
        }
    }
}
