using System;
using System.IO.Pipes;
using System.Net.Http;
using Microsoft.Net.Http.Client;
using Docker.DotNet.HR.Extended.Interfaces;

namespace Docker.DotNet.HR.Extended.Models
{
    public class NPipeClientBuilder(Credentials credentials, Uri uri, TimeSpan namedPipeConnectTimeout = default) : IDockerClientBuilder
    {
        private readonly string _pipeName = ExtractPipeName(uri);
        private readonly TimeSpan _namedPipeConnectTimeout = namedPipeConnectTimeout == default ? TimeSpan.FromMilliseconds(100) : namedPipeConnectTimeout;

        public HttpMessageHandler BuildHandler()
        {
            if (credentials.IsTlsCredentials())
            {
                throw new Exception("TLS not supported over npipe");
            }

            var serverName = uri.Host;
            if (string.Equals(serverName, "localhost", StringComparison.OrdinalIgnoreCase))
            {
                // npipe schemes dont work with npipe://localhost/... and need npipe://./... so fix that for a client here.
                serverName = ".";
            }

            return credentials.GetHandler(new ManagedHandler(async (host, port, cancellationToken) =>
            {
                var timeout = (int)_namedPipeConnectTimeout.TotalMilliseconds;
                var stream = new NamedPipeClientStream(serverName, _pipeName, PipeDirection.InOut, PipeOptions.Asynchronous);
                var dockerStream = new DockerPipeStream(stream);

                await stream.ConnectAsync(timeout, cancellationToken)
                    .ConfigureAwait(false);

                return dockerStream;
            }));
        }

        public Uri BuildUri()
        {
            return new UriBuilder("http", _pipeName).Uri;
        }

        public void Dispose()
        {
            credentials.Dispose();
        }

        private static string ExtractPipeName(Uri uri)
        {
            var segments = uri.Segments;

            if (segments.Length != 3 || !segments[1].Equals("pipe/", StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException($"{uri} is not a valid npipe URI");
            }

            return uri.Segments[2];
        }
    }
}
