﻿using Aggregates.Integrations;
using NEventStore;
using NEventStore.Dispatcher;
using NEventStore.Persistence;
using NEventStore.Persistence.RavenDB;
using NEventStore.Serialization;
using NServiceBus;
using NServiceBus.Logging;
using NServiceBus.MessageInterfaces;
using NServiceBus.ObjectBuilder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aggregates
{
    public class NEventStore : Wireup
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(NEventStore));

        public IBuilder Builder { get; private set; }

        public NEventStore(Wireup inner) : base(inner) { }

        public NEventStore(Wireup wireup, IBuilder builder)
            : base(wireup)
        {
            Builder = builder;
        }

        public override IStoreEvents Build()
        {
            Container.Register<IDispatchCommits>(c => Builder.Build<IDispatchCommits>());

            Container.Register<IScheduleDispatches>(c =>
            {
                var dispatchScheduler = new SynchronousDispatchScheduler(
                    c.Resolve<IDispatchCommits>(),
                    c.Resolve<IPersistStreams>());

                dispatchScheduler.Start();
                return dispatchScheduler;
            });
            var store = base.Build();
            Container.Register(store);

            return store;
        }
    }

    public class NEventStoreRavenPersistence : PersistenceWireup
    {
        public NEventStoreRavenPersistence(Wireup wireup, IBuilder builder, string connectionName)
            : this(wireup, builder, connectionName, new RavenPersistenceOptions())
        { }

        public NEventStoreRavenPersistence(Wireup wireup, IBuilder builder, string connectionName, RavenPersistenceOptions options)
            : base(wireup)
        {

            var aggregateOptions = new RavenPersistenceOptions(
                pageSize: options.PageSize,
                databaseName: options.DatabaseName,
                consistentQueries: options.ConsistentQueries,
                scopeOption: options.ScopeOption,
                serializerCustomizations: s =>
                {
                    s.Binder = new EventSerializationBinder(builder.Build<IMessageMapper>());
                    s.ContractResolver = new EventContractResolver(builder.Build<IMessageMapper>(), builder.Build<IMessageCreator>());
                    if (options.SerializerCustomizations != null)
                        options.SerializerCustomizations(s);
                }
            );

            var persistance =  (new RavenPersistenceFactory(connectionName, new DocumentObjectSerializer(), aggregateOptions)).Build();
            Container.Register(persistance);
        }
    }

    public static class WireupExtension
    {
        public static NEventStore UseAggregates(this Wireup wireup, IBuilder builder)
        {
            return new NEventStore(wireup, builder);
        }

        public static NEventStoreRavenPersistence UsingRavenPersistence(
            this NEventStore wireup,
            string connectionName)
        {
            return new NEventStoreRavenPersistence(wireup, wireup.Builder, connectionName);
        }

        public static NEventStoreRavenPersistence UsingRavenPersistence(
            this NEventStore wireup,
            string connectionName,
            RavenPersistenceOptions options)
        {
            return new NEventStoreRavenPersistence(wireup, wireup.Builder, connectionName, options);
        }
    }
}
