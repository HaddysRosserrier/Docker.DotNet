using System.Text;
using Docker.DotNet.Models;
using Xunit;

namespace Docker.DotNet.Tests
{
    public class JsonEnumMemberConverterTests
    {
        private readonly JsonSerializer _jsonSerializer = new JsonSerializer();

        [Theory]
        [InlineData(RestartPolicyKind.Undefined, "")]
        [InlineData(RestartPolicyKind.No, "no")]
        [InlineData(RestartPolicyKind.OnFailure, "on-failure")]
        [InlineData(RestartPolicyKind.UnlessStopped, "unless-stopped")]
        public void RestartPolicyKind_SerializesToEnumMemberValue(RestartPolicyKind kind, string expected)
        {
            var json = Encoding.UTF8.GetString(_jsonSerializer.SerializeObject(new RestartPolicy { Name = kind }));

            Assert.Equal($"{{\"Name\":\"{expected}\",\"MaximumRetryCount\":0}}", json);
        }

        [Theory]
        [InlineData("\"\"", RestartPolicyKind.Undefined)]
        [InlineData("\"on-failure\"", RestartPolicyKind.OnFailure)]
        [InlineData("\"ALWAYS\"", RestartPolicyKind.Always)]
        [InlineData("3", RestartPolicyKind.OnFailure)]
        public void RestartPolicyKind_Deserializes(string value, RestartPolicyKind expected)
        {
            var policy = _jsonSerializer.DeserializeObject<RestartPolicy>(Encoding.UTF8.GetBytes($"{{\"Name\":{value}}}"));

            Assert.Equal(expected, policy.Name);
        }

        [Fact]
        public void TaskState_RoundTrips()
        {
            var json = _jsonSerializer.SerializeObject(TaskState.Running);

            Assert.Equal("\"running\"", Encoding.UTF8.GetString(json));
            Assert.Equal(TaskState.Running, _jsonSerializer.DeserializeObject<TaskState>(json));
        }

        [Fact]
        public void UnknownValue_ThrowsJsonException()
        {
            Assert.Throws<System.Text.Json.JsonException>(() => _jsonSerializer.DeserializeObject<TaskState>(Encoding.UTF8.GetBytes("\"not-a-state\"")));
        }
    }
}
