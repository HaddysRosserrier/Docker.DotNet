using Docker.DotNet.HR.Extended.Interfaces;
using Docker.DotNet.HR.Extended.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Docker.DotNet.HR.Extended.Factories
{
    public static class DockerClientFactory
    {
        public static IDockerClientBuilder GetDockerClientBuilder(Credentials credentials = null, Uri uri = null)
        {
            uri ??= GetLocalDockerEndpoint();
            credentials ??= new AnonymousCredentials();
            switch (uri.Scheme.ToLowerInvariant())
            {
                case "npipe":
                    return new NPipeClientBuilder(credentials, uri);
                case "tcp":
                case "http":
                case "https":
                    return new ClientBuilder(credentials, uri);
                case "unix":              
                    return new UnixClientBuilder(credentials, uri);

                default:
                    throw new Exception($"Unknown URL scheme {uri.Scheme}");
            }
        }

        private static Uri GetLocalDockerEndpoint()
        {
            var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
            return isWindows ? new Uri("npipe://./pipe/docker_engine") : new Uri("unix:/var/run/docker.sock");
        }

    }
}
