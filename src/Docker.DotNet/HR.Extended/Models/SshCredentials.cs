using Docker.DotNet.HR.Extended.Utils;
using Microsoft.Net.Http.Client;
using Renci.SshNet;
using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Docker.DotNet.HR.Extended.Models;

public sealed class SshCredentials(string privateKey, string password = null) : Credentials
{
    private readonly PrivateKeyFile _privateKey = BuildPrivateKey(privateKey, password);
    private PrivateKeyAuthenticationMethod _privateKeyAuthMethod;
    private SshClient _client;

    public override void Dispose()
    {
        _privateKeyAuthMethod?.Dispose();
        _privateKeyAuthMethod = null;

        _client?.Disconnect();
        _client?.Dispose();
        _client = null;

        _privateKey.Dispose();
        GC.SuppressFinalize(this);
    }
    public override HttpMessageHandler GetHandler(HttpMessageHandler innerHandler)
    {
        return innerHandler;
    }

    public override bool IsTlsCredentials()
    {
        return false;
    }

    private static PrivateKeyFile BuildPrivateKey(string privateKey, string password)
    {
        var stream = new MemoryStream();
        using var writer = new StreamWriter(stream);
        writer.Write(privateKey);
        writer.Flush();

        stream.Position = 0;
        return new PrivateKeyFile(stream, password);
    }

    public ManagedHandler.StreamOpener GetSshStreamOpener(string username)
    {
        return (string host, int port, CancellationToken cancellationToken) => {
            _privateKeyAuthMethod ??= new PrivateKeyAuthenticationMethod(username, _privateKey);
            var connectionInfo = new ConnectionInfo(host, port, username, _privateKeyAuthMethod);
            _client ??= new SshClient(connectionInfo);
            _client.Connect();

            var cmd = _client.CreateCommand("docker system dial-stdio");
            cmd.BeginExecute();

            var result = new ReadWriteStreamUtils(cmd.OutputStream, cmd.CreateInputStream());
            return Task.FromResult((Stream)result);
        };
    }
}
