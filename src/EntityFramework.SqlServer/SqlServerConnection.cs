// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Data.Common;
using System.Data.SqlClient;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Relational;
using Microsoft.Framework.Logging;

namespace Microsoft.Data.Entity.SqlServer
{
    public class SqlServerConnection : RelationalConnection
    {
        /// <summary>
        ///     This constructor is intended only for use when creating test doubles that will override members
        ///     with mocked or faked behavior. Use of this constructor for other purposes may result in unexpected
        ///     behavior including but not limited to throwing <see cref="NullReferenceException" />.
        /// </summary>
        protected SqlServerConnection()
        {
        }

        public SqlServerConnection([NotNull] IDbContextOptions options, [NotNull] ILoggerFactory loggerFactory)
            : base(options, loggerFactory)
        {
        }

        // TODO: Consider using DbProviderFactory to create connection instance
        // Issue #774
        protected override DbConnection CreateDbConnection() => new SqlConnection(ConnectionString);

        public virtual SqlServerConnection CreateMasterConnection()
        {
            var builder = new SqlConnectionStringBuilder { ConnectionString = ConnectionString };
            builder.InitialCatalog = "master";

            // TODO use clone connection method once implimented see #1406
            var options = new DbContextOptions();
            options.UseSqlServer(builder.ConnectionString).CommandTimeout(CommandTimeout);

            return new SqlServerConnection(options, LoggerFactory);
        }
    }
}
