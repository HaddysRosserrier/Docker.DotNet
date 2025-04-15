
using Docker.DotNet.HR.Extended.Interfaces;
using Microsoft.Net.Http.Client;
using System;
using System.Linq;
using System.Net.Http;
using System.Net.Sockets;
using System.Runtime.InteropServices;

namespace Docker.DotNet.HR.Extended.Models;

public sealed class UnixClientBuilder(Credentials credentials, Uri uri) : IDockerClientBuilder
{
    public HttpMessageHandler BuildHandler()
    {
        var pipeString = uri.LocalPath;
        return credentials.GetHandler(new ManagedHandler(async (host, port, cancellationToken) =>
        {
            var sock = new Socket(AddressFamily.Unix, SocketType.Stream, ProtocolType.Unspecified);

            await sock.ConnectAsync(new Microsoft.Net.Http.Client.UnixDomainSocketEndPoint(pipeString))
                .ConfigureAwait(false);

            return sock;
        }));
    }

    public Uri BuildUri()
    {
        return new UriBuilder("http", uri.Segments.Last()).Uri;
    }

    public void Dispose()
    {
        credentials.Dispose();
    }
}
