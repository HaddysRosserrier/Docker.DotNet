using System.Net.Http;
using System;
using Microsoft.Net.Http.Client;
using Docker.DotNet.HR.Extended.Interfaces;

namespace Docker.DotNet.HR.Extended.Models
{
    public sealed class SshClientBuilderClientBuilder(Credentials credentials, Uri uri) : IDockerClientBuilder
    {
        public HttpMessageHandler BuildHandler()
        {
            return credentials.GetHandler(new ManagedHandler());
        }

        public Uri BuildUri()
        {
            if (uri.Scheme.ToLowerInvariant() == "https")
            {
                return uri;
            }

            var builder = new UriBuilder(uri)
            {
                Scheme = credentials.IsTlsCredentials() ? "https" : "http"
            };

            return builder.Uri;
        }

        public void Dispose()
        {
            credentials.Dispose();
        }
    }
}
