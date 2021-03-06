﻿using Moq;
using NBi.Core.Query.Resolver;
using NBi.Core.Scalar.Resolver;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NBi.Testing.Integration.Core.Scalar.Resolver
{
    public class QueryScalarResolverTest
    {
        [Test]
        public void Execute_Query_IsExecuted()
        {
            var args = new QueryScalarResolverArgs(
                new DbCommandQueryResolverArgs(
                    new SqlCommand()
                    {
                        Connection = new SqlConnection(ConnectionStringReader.GetSqlClient()),
                        CommandText = "select 10;"
                    }
                    )
                );

            var resolver = new QueryScalarResolver<int>(args);
            Assert.That(resolver.Execute(), Is.EqualTo(10));
        }
    }
}
