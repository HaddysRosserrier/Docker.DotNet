using System.Net.Http;
using System;
using Docker.DotNet.HR.Extended.Interfaces;
using Microsoft.Net.Http.Client;

namespace Docker.DotNet.HR.Extended.Models
{
    public sealed class SshClientBuilder(SshCredentials credentials, Uri uri) : IDockerClientBuilder
    {
        public HttpMessageHandler BuildHandler()
        {
            var username = uri.UserInfo;
            if (username.Contains(":"))
            {
                throw new ArgumentException("ssh:// protocol only supports authentication with private keys");
            }
            
            return credentials.GetHandler(new ManagedHandler(credentials.GetSshStreamOpener(username)));
        }

        public Uri BuildUri()
        {
            return new UriBuilder("http", uri.Host, uri.IsDefaultPort ? 22 : uri.Port).Uri;
        }

        public void Dispose()
        {
            credentials.Dispose();
        }
    }
}
