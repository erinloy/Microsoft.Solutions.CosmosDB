// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Azure.Cosmos;
using System;
using System.Data.Common;
using System.Diagnostics;

namespace Microsoft.Solutions.CosmosDB.SQL {
    public class SQLEntityCollectionBase<TEntity> : IDataRepositoryProvider<TEntity>
        where TEntity : class, IEntityModel<string> {
        private readonly CosmosContext _context;

        public IRepository<TEntity, string> EntityCollection { get; init; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="DataConnectionString">Connection String</param>
        /// <param name="CollectionName">Your Database Name</param>
        /// <param name="ContainerName">(Optional) If you don't pass it, The Container will be created by Entity Model Class Name + "s", In Model First Dev, You don't need to use it</param>
        public SQLEntityCollectionBase(CosmosContext context, Func<CosmosContext, IRepository<TEntity, string>> repoFactory = null) {
            _context = context;
            this.EntityCollection = repoFactory?.Invoke(_context);
            this.EntityCollection ??= new BusinessTransactionRepository<TEntity, string>(_context);
        }
    }

    public class CosmosClientManager {
        private readonly string _connectionString;
        
        public CosmosClientManager(string connectionString) {
            _connectionString = connectionString;
            _client = new Lazy<CosmosClient>(() => new CosmosClient(_connectionString));
        }

        public CosmosClientManager(CosmosClient client)
        {
            _client = new Lazy<CosmosClient>(() => client);
            _connectionString = _client.Value.Endpoint.ToString();
        }

        static CosmosClientManager()
        {
            //Type defaultTrace = Type.GetType("Microsoft.Azure.Cosmos.Core.Trace.DefaultTrace,Microsoft.Azure.Cosmos.Direct");
            //TraceSource traceSource = (TraceSource)defaultTrace.GetProperty("TraceSource").GetValue(null);
            //traceSource.Switch.Level = SourceLevels.Off;
            //traceSource.Listeners.Clear();
        }

        public string ConnectionString => _connectionString;

        private readonly Lazy<CosmosClient> _client;

        public CosmosClient Client => _client.Value;

        public CosmosContext ContextFor(string collectionName, string? containerName = "")
        {
            return new CosmosContext(_client.Value, collectionName, containerName);
        }
    }

    public class CosmosContext : CosmosClientManager
    {
        public CosmosContext(string connectionString, string collectionName, string containerName = null) : base(connectionString)
        {
            CollectionName = collectionName;
            ContainerName = containerName;
        }

        public CosmosContext(CosmosClient client, string collectionName, string containerName = null) : base(client)
        {
            CollectionName = collectionName;
            ContainerName = containerName;
        }

        public string CollectionName { get; init; }
        public string ContainerName { get; init; }
        public IServiceProvider ServiceProvider { get; set; }
    }
}
