using System.Runtime.InteropServices;
using Docker.DotNet.HR.Extended.Factories;
using Docker.DotNet.HR.Extended.Models;

namespace Docker.DotNet.Extended.Tests.Factories
{
    [Trait("Category", "UnitTest")]
    public class DockerClientFactoryTest
    {
        //private readonly AutoMocker _mocker;

        [Fact]
        public void LocalDockerEndpoint()
        {
            var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

            var clientBuilder = DockerClientFactory.GetDockerClientBuilder();
            if (isWindows)
            {
                Assert.IsType<NPipeClientBuilder>(clientBuilder);
            }
            else
            {
                Assert.IsType<UnixClientBuilder>(clientBuilder);
            }         
        }

        [Fact]
        public void NPipe()
        {
            var uri = new Uri("npipe://./pipe/docker_engine");
            var clientBuilder = DockerClientFactory.GetDockerClientBuilder(null, uri);
            Assert.IsType<NPipeClientBuilder>(clientBuilder);
        }

        [Fact]
        public void Unix()
        {
            var uri = new Uri("unix:/var/run/docker.sock");
            var clientBuilder = DockerClientFactory.GetDockerClientBuilder(null, uri);
            Assert.IsType<UnixClientBuilder>(clientBuilder);
        }

        [Fact]
        public void Tcp()
        {
            var uri = new Uri("tcp://localhost:2375");
            var clientBuilder = DockerClientFactory.GetDockerClientBuilder(null, uri);
            Assert.IsType<ClientBuilder>(clientBuilder);
        }

        [Fact]
        public void Http()
        {
            var uri = new Uri("http://localhost:2375");
            var clientBuilder = DockerClientFactory.GetDockerClientBuilder(null, uri);
            Assert.IsType<ClientBuilder>(clientBuilder);
        }

        [Fact]
        public void Https()
        {
            var uri = new Uri("https://localhost:2375");
            var clientBuilder = DockerClientFactory.GetDockerClientBuilder(null, uri);
            Assert.IsType<ClientBuilder>(clientBuilder);
        }

        [Fact]
        public void UnknownScheme()
        {
            var uri = new Uri("ftp://localhost:2375");
            Assert.Throws<Exception>(() => DockerClientFactory.GetDockerClientBuilder(null, uri));
        }


    }
}