// Copyright 2018 Confluent Inc.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
// Refer to LICENSE for more information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using AvroMultipleTypesPerTopic.MultipleTypeSerdes;
using Confluent.Kafka;
using Confluent.Kafka.Admin;
using Confluent.Kafka.SyncOverAsync;
using Confluent.SchemaRegistry;
using Confluent.SchemaRegistry.Serdes;
using MessageTypes;

namespace AvroMultipleTypesPerTopic
{
    /// <summary>
    /// Example of producing and consuming multiple message types on a single topic using the
    /// Schema Registry and Avro serialization. CTRL-C to exit.
    /// </summary>
    class Program
    {

        const string TopicName = "multiple-types-avro";

        // Specify supported message types here. Support is restricted to types generated via avrogen.exe
        // tool. Being explicit makes this a lot simpler as we can use Avro Schema objects rather than messing
        // around with Type objects.
        private static readonly MultipleTypeConfig MultipleTypeConfig = new MultipleTypeConfigBuilder()
            .AddType<TaskRequested>(TaskRequested._SCHEMA)
            .AddType<TaskStarted>(TaskStarted._SCHEMA)
            .AddType<TaskCompleted>(TaskCompleted._SCHEMA)
            .Build();

        static async Task Main(string[] args)
        {
            if (args.Length != 2)
            {
                PrintUsage();
                return;
            }

            var bootstrapServers = args[0];
            var schemaRegistryUrl = args[1];

            var cts = new CancellationTokenSource();

            Console.CancelKeyPress += (_, e) => {
                e.Cancel = true; 
                Console.WriteLine("Exiting...");
                cts.Cancel();
            };

            await RecreateTopic(TopicName, bootstrapServers);

#pragma warning disable 4014
            Task.Run(() => Consume(bootstrapServers, schemaRegistryUrl, cts.Token), cts.Token);
#pragma warning restore 4014

            try
            {
                await Produce(bootstrapServers, schemaRegistryUrl, cts.Token);
            }
            catch (OperationCanceledException) { }
        }
        
        private static async Task RecreateTopic(string topicName, string bootstrapServers)
        {
            using (var adminClient = new AdminClientBuilder(new AdminClientConfig { BootstrapServers = bootstrapServers }).Build())
            {
                try
                {
                    await adminClient.DeleteTopicsAsync(new[] { topicName }, new DeleteTopicsOptions { OperationTimeout = TimeSpan.FromSeconds(30)});
                }
                catch (DeleteTopicsException e) when (e.Results.Any(x => x.Error.Code == ErrorCode.UnknownTopicOrPart))
                {
                    
                }

                await Task.Delay(TimeSpan.FromSeconds(1));
                try
                {
                    await adminClient.CreateTopicsAsync(new TopicSpecification[] {
                        new TopicSpecification { Name = topicName, ReplicationFactor = 1, NumPartitions = 1 } });
                }
                catch (CreateTopicsException e)
                {
                    Console.WriteLine($"An error occurred creating topic {e.Results[0].Topic}: {e.Results[0].Error.Reason}");
                }
            }
        }

        static async Task Produce(string bootstrapServers, string schemaRegistryUrl,
            CancellationToken cancellationToken)
        {
            
            // Important. Use either SubjectNameStrategy.Record or SubjectNameStrategy.TopicRecord.
            // SubjectNameStrategy.Topic (default) would result in the topic schema being set based on
            // the first message produced.
            //
            // Note that you can restrict the range of message types for a topic by setting up the
            // topic schema with schema references:
            // https://www.confluent.io/blog/multiple-event-types-in-the-same-kafka-topic/
            var serializerConfig = new AvroSerializerConfig
            {
                SubjectNameStrategy = SubjectNameStrategy.Record,
                AutoRegisterSchemas = true
            };
            using (var schemaRegistryClient = new CachedSchemaRegistryClient(new SchemaRegistryConfig { Url = schemaRegistryUrl }))
            {
                var serializer = new MultipleTypeSerializer(MultipleTypeConfig, schemaRegistryClient, serializerConfig);
                // We create a Producer with an object value type and MultipleTypeSerializer to serialize values.
                // MultipleTypeSerializer delegates to a different AvroSerializer instance for each message type
                // configured within MultipleTypesConfig.
                //
                // Note that all serializers share the same AvroSerializerConfig - a separate producer could
                // be used for each logical set of message types (e.g. all messages produced to a certain topic)
                // to support different configuration
                using (var producer =
                    new ProducerBuilder<string, object>(new ProducerConfig {BootstrapServers = bootstrapServers})
                        .SetKeySerializer(new AvroSerializer<string>(schemaRegistryClient))
                        .SetValueSerializer(serializer)
                        .Build())
                {
                    var random = new Random();
                    async Task ProduceMessage(Guid key, object value) => await producer.ProduceAsync(TopicName,
                        new Message<string, object>
                        {
                            Key = key.ToString(),
                            Value = value
                        }, cancellationToken);

                    async Task Wait(int min, int max) => await Task.Delay(random.Next(min, max), cancellationToken);

                    
                    while (true)
                    {
                        var id = Guid.NewGuid();
                        await ProduceMessage(id, new TaskRequested()
                        {
                            Id = id,
                            RequestedDate = DateTime.Now,
                            RequestedBy = "test"
                        });
                        await Wait(250, 500);
                        await ProduceMessage(id, new TaskStarted()
                        {
                            Id = id,
                            StartedDate = DateTime.Now,
                            StartedOn = "DEV001"
                        });
                        await Wait(1000, 2500);
                        await ProduceMessage(id, new TaskCompleted()
                        {
                            Id = id,
                            CompletedDate = DateTime.Now
                        });
                        await Wait(1000, 2000);
                    }
                }
            }
        }

        
        static void Consume(string bootstrapServers, string schemaRegistryUrl, CancellationToken cancellationToken)
        {
            var consumerConfig = new ConsumerConfig
            {
                GroupId = Guid.NewGuid().ToString(),
                BootstrapServers = bootstrapServers,
                AutoOffsetReset = AutoOffsetReset.Earliest
            };

            using (var schemaRegistry = new CachedSchemaRegistryClient(new SchemaRegistryConfig { Url = schemaRegistryUrl }))
            using (var consumer =
                new ConsumerBuilder<string, object>(consumerConfig)
                    .SetKeyDeserializer(new AvroDeserializer<string>(schemaRegistry).AsSyncOverAsync())
                    .SetValueDeserializer(new MultipleTypeDeserializer(MultipleTypeConfig, schemaRegistry).AsSyncOverAsync())
                    .Build())
            {
                consumer.Subscribe(TopicName);
                void LogEvent(Message<string,object> message, string description) =>
                    Console.WriteLine("Received {1} event (key {0}) - {2}", message.Key, message.Value.GetType().Name, description);
                
                try
                {
                    while (true)
                    {
                        try
                        {
                            var consumeResult = consumer.Consume(cancellationToken);
                            var message = consumeResult.Message;
                            switch (message.Value)
                            {
                                case TaskRequested requested:
                                    LogEvent(message,$"Task requested by {requested.RequestedBy} at {requested.RequestedDate:s}");
                                    break;
                                case TaskStarted started:
                                    LogEvent(message, $"Task started on {started.StartedOn} at {started.StartedDate:s}");
                                    break;
                                case TaskCompleted completed:
                                    LogEvent(message, $"Task completed at {completed.CompletedDate:s}");
                                    break;
                            }
                        }
                        catch (ConsumeException e)
                        {
                            Console.WriteLine($"Consumer error: {e.Error.Reason}");
                        }
                    }
                }
                catch (OperationCanceledException)
                {
                    // commit final offsets and leave the group.
                    consumer.Close();
                }
            }
        }

        private static void PrintUsage()
            => Console.WriteLine(@"Usage: .. <bootstrap-servers> <schema-registry-url>
Example: .. localhost:29092 http://localhost:8081");
    }
}
