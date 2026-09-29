# HR.Extended.Docker.DotNet

[![NuGet latest release](https://img.shields.io/nuget/v/HR.Extended.Docker.DotNet.svg)](https://www.nuget.org/packages/HR.Extended.Docker.DotNet)

An extended fork of [Docker.DotNet](https://github.com/dotnet/Docker.DotNet), the .NET client for the [Docker Remote API][docker-remote-api].

It is fully asynchronous, designed to be non-blocking and object-oriented way to interact with your Docker daemon programmatically.

On top of the upstream library, this package adds:

- **SSH connections** (`ssh://user@host`) to remote Docker daemons, tunnelled through `docker system dial-stdio`
- **Pluggable transports** via `IDockerClientBuilder`, with builders for npipe, http/https/tcp, unix and ssh

The public API and namespaces (`Docker.DotNet`) are the same as upstream, so existing code keeps working. Because the assembly is also named `Docker.DotNet`, use this package **instead of** `Docker.DotNet`, not alongside it.

## Versioning

Version of this package uses [SemVer](https://semver.org/) format: `MAJOR.MINOR.PATCH`. `MINOR` segment indicates
the [Docker Remote API][docker-remote-api] version support. For instance `v2.124.0` of this library supports
[Docker Remote API][docker-remote-api] `v1.24`. This does not guarantee backwards compatibility as [Docker Remote API][docker-remote-api] does not guarantee that either.

`MAJOR` is reserved for major breaking changes we make to the library itself such as how
the calls are made or how authentication is made. `PATCH` is just for incremental bug fixes
or non-breaking feature additions.

## Installation

You can add this library to your project using [NuGet][nuget].

**.NET Command Line Interface**
Run the following command from your favorite shell or terminal:

> dotnet add package HR.Extended.Docker.DotNet

**Package Manager Console**
Run the following command in the “Package Manager Console”:

> PM> Install-Package HR.Extended.Docker.DotNet

**Visual Studio**
Right click to your project in Visual Studio, choose “Manage NuGet Packages”, search for ‘HR.Extended.Docker.DotNet’ and click ‘Install’.
([see NuGet Gallery][nuget-gallery].)

## Usage

You can initialize the client like the following:

```csharp
using Docker.DotNet;
DockerClient client = new DockerClientConfiguration(
    new Uri("http://ubuntu-docker.cloudapp.net:4243"))
     .CreateClient();
```
or to connect to your local [Docker for Windows](https://docs.docker.com/docker-for-windows/) daemon using named pipes or your local [Docker for Mac](https://docs.docker.com/docker-for-mac/) daemon using Unix sockets:

```csharp
using Docker.DotNet;
DockerClient client = new DockerClientConfiguration()
     .CreateClient();
```

For a custom endpoint, you can also pass a named pipe or a Unix socket to the `DockerClientConfiguration` constructor. For example:

```csharp
// Default Docker Engine on Windows
using Docker.DotNet;
DockerClient client = new DockerClientConfiguration(
    new Uri("npipe://./pipe/docker_engine"))
     .CreateClient();
// Default Docker Engine on Linux
using Docker.DotNet;
DockerClient client = new DockerClientConfiguration(
    new Uri("unix:///var/run/docker.sock"))
     .CreateClient();
```

## Extended features

### SSH connections

Connect to a remote Docker daemon over SSH using private key authentication. The remote user must be able to run `docker system dial-stdio`.

```csharp
using Docker.DotNet;
using Docker.DotNet.HR.Extended.Interfaces;
using Docker.DotNet.HR.Extended.Models;

// Pass the private key *contents* (not a file path); the passphrase is optional.
var privateKey = File.ReadAllText("/home/me/.ssh/id_ed25519");
var credentials = new SshCredentials(privateKey, "key-passphrase");

// Port defaults to 22. Password authentication (user:password@host) is not supported.
var uri = new Uri("ssh://user@docker-host");

IDockerClientBuilder builder = new SshClientBuilder(credentials, uri);
DockerClient client = new DockerClientConfiguration(builder).CreateClient();
```

### Custom transports with `IDockerClientBuilder`

`DockerClientConfiguration` accepts any `IDockerClientBuilder`, so you can pick the transport explicitly or supply your own implementation. Built-in builders (namespace `Docker.DotNet.HR.Extended.Models`):

| Builder | Schemes |
| --- | --- |
| `NPipeClientBuilder` | `npipe://` |
| `ClientBuilder` | `http://`, `https://`, `tcp://` |
| `UnixClientBuilder` | `unix://` |
| `SshClientBuilder` | `ssh://` |


## Accessing Docker API 

#### Example: List containers

```csharp
IList<ContainerListResponse> containers = await client.Containers.ListContainersAsync(
	new ContainersListParameters(){
		Limit = 10,
    });
```

#### Example: Create an image by pulling from Docker Registry

The code below pulls `fedora/memcached` image to your Docker instance using your Docker Hub account. You can
anonymously download the image as well by passing `null` instead of AuthConfig object:

```csharp
await client.Images.CreateImageAsync(
    new ImagesCreateParameters
    {
        FromImage = "fedora/memcached",
        Tag = "alpha",
    },
    new AuthConfig
    {
        Email = "test@example.com",
        Username = "test",
        Password = "pa$$w0rd"
    },
    new Progress<JSONMessage>());
```


#### Example: Create a container

The following code will create a new container of the previously fetched image.

```csharp
await client.Containers.CreateContainerAsync(new CreateContainerParameters()
    {
        Image = "fedora/memcached",
        HostConfig = new HostConfig()
        {
            DNS = new[] { "8.8.8.8", "8.8.4.4" }
        }
    });
```

#### Example: Start a container

The following code will start the created container.

```csharp
await client.Containers.StartContainerAsync(
    "39e3317fd258",
    new ContainerStartParameters()
    );
```

#### Example: Stop a container

The following code will stop a running container.

*Note: `WaitBeforeKillSeconds` field is of type `uint?` which means optional. This code will wait 30 seconds before
killing it. If you like to cancel the waiting, you can use the CancellationToken parameter.*

```csharp
var stopped = await client.Containers.StopContainerAsync(
    "39e3317fd258",
    new ContainerStopParameters
    {
        WaitBeforeKillSeconds = 30
    },
    CancellationToken.None);
```

#### Example: Dealing with Stream responses

Some Docker API endpoints are designed to return stream responses. For example
[Monitoring Docker events](https://docs.docker.com/engine/reference/api/docker_remote_api_v1.24/#/monitor-docker-s-events)
continuously streams the status in a format like :

```json
{"status":"create","id":"dfdf82bd3881","from":"base:latest","time":1374067924}
{"status":"start","id":"dfdf82bd3881","from":"base:latest","time":1374067924}
{"status":"stop","id":"dfdf82bd3881","from":"base:latest","time":1374067966}
{"status":"destroy","id":"dfdf82bd3881","from":"base:latest","time":1374067970}
...
```

To obtain this stream you can use:

```csharp
CancellationTokenSource cancellation = new CancellationTokenSource();
Stream stream = await client.System.MonitorEventsAsync(new ContainerEventsParameters(), new Progress<JSONMessage>(), cancellation.Token);
// Initialize a StreamReader...
```

You can cancel streaming using the CancellationToken. On the other hand, if you wish to continuously stream, you can simply pass `CancellationToken.None`.

#### Example: HTTPS Authentication to Docker

> **Note:** `Docker.DotNet.X509` and `Docker.DotNet.BasicAuth` are published by the upstream project and depend on the upstream `Docker.DotNet` package, not on this one. They are not currently published for HR.Extended.Docker.DotNet.

If you are [running Docker with TLS (HTTPS)][docker-tls], you can authenticate to the Docker instance using the [**`Docker.DotNet.X509`**][Docker.DotNet.X509] package. You can get this package from NuGet or by running the following command in the “Package Manager Console”:

    PM> Install-Package Docker.DotNet.X509

Once you add `Docker.DotNet.X509` to your project, use `CertificateCredentials` type:

```csharp
var credentials = new CertificateCredentials (new X509Certificate2 ("CertFile", "Password"));
var config = new DockerClientConfiguration("http://ubuntu-docker.cloudapp.net:4243", credentials);
DockerClient client = config.CreateClient();
```

If you don't want to authenticate you can omit the `credentials` parameter, which defaults to an `AnonymousCredentials` instance.

The `CertFile` in the example above should be a .pfx file (PKCS12 format), if you have .pem formatted certificates which Docker normally uses you can either convert it programmatically or use `openssl` tool to generate a .pfx:

    openssl pkcs12 -export -inkey key.pem -in cert.pem -out key.pfx

(Here, your private key is key.pem, public key is cert.pem and output file is named key.pfx.) This will prompt a password for PFX file and then you can use this PFX file on Windows. If the certificate is self-signed, your application may reject the server certificate, in this case you might want to disable server certificate validation:
```c#
//
// There are two options to do this.
//

// You can do this globally for all certificates:
ServicePointManager.ServerCertificateValidationCallback += (o, c, ch, er) => true;

// Or you can do this on a credential by credential basis:
var creds = new CertificateCredentials(...);
creds.ServerCertificateValidationCallback += (o, c, ch, er) => true;

```

#### Example: Basic HTTP Authentication to Docker

If the Docker instance is secured with Basic HTTP Authentication, you can use the [**`Docker.DotNet.BasicAuth`**][Docker.DotNet.BasicAuth] package. Get this package from NuGet or by running the following command in the “Package Manager Console”:

    PM> Install-Package Docker.DotNet.BasicAuth

Once you added `Docker.DotNet.BasicAuth` to your project, use `BasicAuthCredentials` type:

```csharp
var credentials = new BasicAuthCredentials ("YOUR_USERNAME", "YOUR_PASSWORD");
var config = new DockerClientConfiguration("tcp://ubuntu-docker.cloudapp.net:4243", credentials);
DockerClient client = config.CreateClient();
```

`BasicAuthCredentials` also accepts `SecureString` for username and password arguments.

#### Example: Specifying Remote API Version

By default this client does not specify version number to the API for the requests it makes. However, if you would like to make use of versioning feature of Docker Remote API You can initialize the client like the following.

```csharp
var config = new DockerClientConfiguration(...);
DockerClient client = config.CreateClient(new Version(1, 16));
```

### Error Handling

Here are typical exceptions thrown from the client library:

* **`DockerApiException`** is thrown when Docker API responds with a non-success result. Subclasses:
    * **``DockerContainerNotFoundException``**
    * **``DockerImageNotFoundException``**
* **`TaskCanceledException`** is thrown from `System.Net.Http.HttpClient` library by design. It is not a friendly exception, but it indicates your request has timed out. (default request timeout is 100 seconds.)
    * Long-running methods (e.g. `WaitContainerAsync`, `StopContainerAsync`) and methods that return Stream (e.g. `CreateImageAsync`, `GetContainerLogsAsync`) have timeout value overridden with infinite timespan by this library.
* **`ArgumentNullException`** is thrown when one of the required parameters are missing/empty.
    * Consider reading the [Docker Remote API reference][docker-remote-api] and source code of the corresponding method you are going to use in from this library. This way you can easily find out which parameters are required and their format.

## Contributing

Issues and pull requests are welcome at [HaddysRosserrier/Docker.DotNet](https://github.com/HaddysRosserrier/Docker.DotNet).
Changes that aren't specific to the extended features are better proposed upstream at [dotnet/Docker.DotNet](https://github.com/dotnet/Docker.DotNet).

## Credits

This package is a fork of [Docker.DotNet](https://github.com/dotnet/Docker.DotNet), a [.NET Foundation](https://www.dotnetfoundation.org) project.

## License

Licensed under the [MIT](LICENSE) license.

---------------
Copyright (c) .NET Foundation and Contributors

[docker-remote-api]: https://docs.docker.com/engine/reference/api/docker_remote_api/
[docker-tls]: https://docs.docker.com/articles/https/
[nuget]: https://www.nuget.org
[nuget-gallery]: https://www.nuget.org/packages/HR.Extended.Docker.DotNet/
[Docker.DotNet.X509]: https://www.nuget.org/packages/Docker.DotNet.X509/
[Docker.DotNet.BasicAuth]: https://www.nuget.org/packages/Docker.DotNet.BasicAuth/
