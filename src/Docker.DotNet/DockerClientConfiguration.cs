using Docker.DotNet.HR.Extended.Factories;
using Docker.DotNet.HR.Extended.Interfaces;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;

namespace Docker.DotNet
{
    public class DockerClientConfiguration : IDisposable
    {
        public DockerClientConfiguration(
            Credentials credentials = null,
            TimeSpan defaultTimeout = default,
            TimeSpan namedPipeConnectTimeout = default,
            IReadOnlyDictionary<string, string> defaultHttpRequestHeaders = null)
            : this(DockerClientFactory.GetDockerClientBuilder(credentials), defaultTimeout, defaultHttpRequestHeaders)
        {
        }

        public DockerClientConfiguration(
            IDockerClientBuilder clientHandler,
            TimeSpan defaultTimeout = default,
            IReadOnlyDictionary<string, string> defaultHttpRequestHeaders = null)
        {
            if (defaultTimeout < Timeout.InfiniteTimeSpan)
            {
                throw new ArgumentException("Default timeout must be greater than -1", nameof(defaultTimeout));
            }

            DefaultTimeout = TimeSpan.Equals(default, defaultTimeout) ? TimeSpan.FromSeconds(100) : defaultTimeout;
            DefaultHttpRequestHeaders = defaultHttpRequestHeaders ?? new Dictionary<string, string>();
        }

        /// <summary>
        /// Gets the collection of default HTTP request headers.
        /// </summary>
        public IReadOnlyDictionary<string, string> DefaultHttpRequestHeaders { get; }

        public IDockerClientBuilder ClientBuilder { get; }

        public TimeSpan DefaultTimeout { get; }

        public TimeSpan NamedPipeConnectTimeout { get; }

        public DockerClient CreateClient(Version requestedApiVersion = null)
        {
            return new DockerClient(this, requestedApiVersion);
        }

        public void Dispose()
        {
            ClientBuilder.Dispose();
        }

    }
}