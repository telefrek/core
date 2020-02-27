using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cassandra;
using Docker.DotNet;
using Docker.DotNet.Models;
using Xunit;

namespace Telefrek.Core.Cassandra.Tests
{
    /// <summary>
    /// Provides the collection definition
    /// </summary>
    [CollectionDefinition("Cassandra")]
    public class CassandraFixtureCollection : ICollectionFixture<CassandraFixture> { }

    /// <summary>
    /// Fixture class for running integration tests with Cassandra/Docker
    /// </summary>
    /// <remarks>
    /// Inspired by https://www.meziantou.net/integration-testing-using-a-docker-container.htm
    /// </remarks>
    public class CassandraFixture : IDisposable
    {
        readonly DockerClientConfiguration _config;
        readonly DockerClient _client;

        const string ContainerName = "cass-test";
        const string ImageName = "cassandra";
        const string ImageTag = "3";

        /// <summary>
        /// Constructor for setup
        /// </summary>
        public CassandraFixture()
        {
            Console.WriteLine("Creating fixture");

            // Setup the core components
            _config = new DockerClientConfiguration(new Uri("unix:///var/run/docker.sock"));
            _client = _config.CreateClient();

            // Cleanup any old instances of the Cassandra database
            CleanupDB().Wait();

            // Start the new container
            StartDB().Wait();

            // Create test resources
        }

        /// <summary>
        /// Implements disposable, cleanup all database resources here
        /// </summary>
        public void Dispose()
        {
            // Cleanup any old instances of the Cassandra database
            using (_config)
            using (_client)
                CleanupDB().Wait();
        }

        /// <summary>
        /// SHelper method to ensure the image is setup correctly
        /// </summary>
        /// <returns>A Task for tracking completion.</returns>
        async Task StartDB()
        {
            Console.WriteLine("Starting fixture");

            Console.WriteLine("Downloading image");
            // Download image
            await _client.Images.CreateImageAsync(new ImagesCreateParameters() { FromImage = ImageName, Tag = ImageTag }, new AuthConfig(), new Progress<JSONMessage>()).ConfigureAwait(false);

            // Create the container
            var config = new Config()
            {
                Hostname = "localhost"
            };

            // Configure the ports to expose
            var hostConfig = new HostConfig
            {
                PortBindings = new Dictionary<string, IList<PortBinding>>
                        {
                            { "9042/tcp", new List<PortBinding> {
                                new PortBinding {
                                    HostIP = "127.0.0.1",
                                    HostPort = "9042"
                                }
                            }
                        }
                    },
                NetworkMode = "host"
            };

            Console.WriteLine("Creating container");
            // Create the container
            var response = await _client.Containers.CreateContainerAsync(new CreateContainerParameters(config)
            {
                Image = ImageName + ":" + ImageTag,
                Name = ContainerName,
                Tty = false,
                HostConfig = hostConfig,
            }).ConfigureAwait(false);

            // Get the container object
            Console.WriteLine("Searching for container");
            var containers = await _client.Containers.ListContainersAsync(new ContainersListParameters() { All = true }).ConfigureAwait(false);
            var container = containers.First(c => c.ID == response.ID);

            Console.WriteLine("Checking state {0}", container.State);
            // Start the container is needed
            if (container.State != "running")
            {
                Console.WriteLine("Starting container");
                var started = await _client.Containers.StartContainerAsync(container.ID, new ContainerStartParameters()).ConfigureAwait(false);
                if (!started)
                    throw new InvalidOperationException("Failed to start the container");
            }

            // Wait for the cassandra database to be available
            Console.Write("Waiting for Cassandra to startup");
            var cluster = new CassandraConnectionStringBuilder
            {
                Port = 9042,
                ContactPoints = new string[] { "localhost" }
            }.MakeClusterBuilder().Build();

            // Wait for Cassandra to start (up to 50 seconds, more is silly)    
            for (var i = 0; i < 25; ++i)
            {
                try
                {
                    using (var session = await cluster.ConnectAsync().ConfigureAwait(false))
                        break;
                }
                catch (Exception)
                {
                    Console.Write(".");
                    await Task.Delay(2000).ConfigureAwait(false);
                }

                if (i == 24) throw new InvalidOperationException("Failed to start Cassandra!");
            }

            Console.WriteLine("Ready");
        }

        /// <summary>
        /// Helper method for cleaning up database instances
        /// </summary>
        /// <returns>A Task for tracking completion.</returns>
        async Task CleanupDB()
        {
            Console.WriteLine("Cleaning up fixture");

            Console.WriteLine("Searching for container");
            var containers = await _client.Containers.ListContainersAsync(new ContainersListParameters() { All = true }).ConfigureAwait(false);
            var container = containers.FirstOrDefault(c => c.Names.Contains("/" + ContainerName));

            if (container != null)
            {
                Console.WriteLine("Container found, stopping");
                await _client.Containers.StopContainerAsync(container.ID, new ContainerStopParameters()).ConfigureAwait(false);

                Console.WriteLine("Cleaning up container resources");
                await _client.Containers.RemoveContainerAsync(container.ID, new ContainerRemoveParameters { Force = true, RemoveVolumes = true }).ConfigureAwait(false);
            }
        }
    }
}
