using System;
using System.Net.Http;

namespace Docker.DotNet.HR.Extended.Interfaces;

public interface IDockerClientBuilder : IDisposable
{
    /// <summary>
    /// Builds the Docker client Uri based on the provided credentials/host.
    /// </summary>
    /// <returns></returns>
    Uri BuildUri();

    /// <summary>
    /// Builds the Docker client handler based on the provided credentials/protocols.
    /// </summary>
    /// <returns></returns>
    HttpMessageHandler BuildHandler();
}
