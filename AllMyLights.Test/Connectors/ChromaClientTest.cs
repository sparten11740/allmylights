using Moq;
using NUnit.Framework;
using AllMyLights.Connectors.Sinks.Chroma;
using System.Net.Http;
using Moq.Protected;
using System.Threading.Tasks;
using System.Threading;
using System.Net;
using System.Text;
using System.Linq.Expressions;
using System.Drawing;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace AllMyLights.Test
{
    public class ChromaClientTest
    {
        private const string SESSION_URI = "http://localhost:12345/chromasdk";

        Mock<HttpMessageHandler> Handler { get; set; }
        HttpClient HttpClient { get; set; }

        [SetUp]
        public void Setup()
        {
            Handler = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            HttpClient = new HttpClient(Handler.Object);
        }

        [Test]
        public async Task Should_initialize_a_chroma_session()
        {
            var subject = new ChromaClient(HttpClient);

            ExpectRequest(
               method: HttpMethod.Post,
               uri: ChromaClient.CHROMA_INITIALIZATION_ENDPOINT,
               body: ChromaClient.CHROMA_INITIALIZATION_BODY,
               responseBody: $@"{{""sessionid"": 12345, ""uri"": ""{SESSION_URI}""}}"
            );
            await subject.InitializeAsync();

            Assert.AreEqual(SESSION_URI, subject.SessionUri);
        }

        [Test]
        public async Task Should_throw_exception_on_failed_initialization()
        {
            var subject = new ChromaClient(HttpClient);

            ExpectRequest(
               method: HttpMethod.Post,
               uri: ChromaClient.CHROMA_INITIALIZATION_ENDPOINT,
               body: ChromaClient.CHROMA_INITIALIZATION_BODY,
               responseBody: $@"{{""error"": ""went to destination fucked"", ""result"": 87}}"
            );

            Assert.ThrowsAsync<ChromaException>(() => subject.InitializeAsync());
            Handler.Verify();
        }

        [Test]
        public async Task Should_send_heartbeat()
        {

            var subject = new ChromaClient(HttpClient){ SessionUri = SESSION_URI };

            ExpectRequest(HttpMethod.Put, $"{SESSION_URI}/heartbeat");
            await subject.SendHeartbeatAsync();

            Handler.Verify();
        }

        [Test]
        public async Task Should_apply_a_static_effect()
        {
            var color = Color.FromArgb(139, 0, 0);
            var bgr = 139;
            var device = "mouse";

            var content = ToStringContent(JsonConvert.SerializeObject(new ChromaEffect() { Param = new Dictionary<string, int>() {
                { "color", bgr }
            }}));


            var subject = new ChromaClient(HttpClient) { SessionUri = SESSION_URI };

            ExpectRequest(HttpMethod.Put, $"{SESSION_URI}/{device}", content);
            await subject.ApplyStaticEffectAsync(device, color);

            Handler.Verify();
        }

        [Test]
        public void Should_throw_exception_on_request_failure_to_apply_static_effect()
        {
            var color = Color.FromArgb(139, 0, 0);
            var bgr = 139;
            var device = "mouse";

            var content = ToStringContent(JsonConvert.SerializeObject(new ChromaEffect(){Param = new Dictionary<string, int>() {{ "color", bgr }}}));

            var subject = new ChromaClient(HttpClient){ SessionUri = SESSION_URI };

            ExpectRequest(HttpMethod.Put, $"{SESSION_URI}/{device}", content, @"{""result"": 76}", HttpStatusCode.InternalServerError);

            Assert.ThrowsAsync<HttpRequestException>(() => subject.ApplyStaticEffectAsync(device, color));
            Handler.Verify();

        }

        [Test]
        public void Should_throw_exception_when_apply_static_effect_returns_non_zero_exit_code()
        {
            var color = Color.FromArgb(139, 0, 0);
            var bgr = 139;
            var device = "mouse";

            var content = ToStringContent(JsonConvert.SerializeObject(new ChromaEffect() { Param = new Dictionary<string, int>() { { "color", bgr } } }));

            var subject = new ChromaClient(HttpClient) { SessionUri = SESSION_URI };

            ExpectRequest(HttpMethod.Put, $"{SESSION_URI}/{device}", content, @"{""result"": 76, ""error"": ""failed big times""}");

            Assert.ThrowsAsync<ChromaException>(() => subject.ApplyStaticEffectAsync(device, color));
            Handler.Verify();

        }

        [Test]
        public async Task Should_update()
        {
            var device = "keyboard";
            var payload = "some payload";
            var content = ToStringContent(payload);

            var subject = new ChromaClient(HttpClient){SessionUri = SESSION_URI};

            ExpectRequest(HttpMethod.Put, $"{SESSION_URI}/{device}", content);
            await subject.UpdateAsync(device, payload);

            Handler.Verify();
        }

        [Test]
        public void Should_throw_execption_on_failure_to_update()
        {
            var device = "keyboard";
            var payload = "some payload";
            var content = ToStringContent(payload);

            var subject = new ChromaClient(HttpClient) { SessionUri = SESSION_URI };

            ExpectRequest(HttpMethod.Put, $"{SESSION_URI}/{device}", content, @"{""result"": 76}", HttpStatusCode.InternalServerError);
            Assert.ThrowsAsync<HttpRequestException>(() => subject.UpdateAsync(device, payload));

            Handler.Verify();
        }

        [Test]
        public void Should_throw_exception_when_update_returns_non_zero_exit_code()
        {
            var device = "keyboard";
            var payload = "some payload";
            var content = ToStringContent(payload);

            var subject = new ChromaClient(HttpClient) { SessionUri = SESSION_URI };

            ExpectRequest(HttpMethod.Put, $"{SESSION_URI}/{device}", content, @"{""result"": 81, ""error"": ""failed big times""}");
            Assert.ThrowsAsync<ChromaException>(() => subject.UpdateAsync(device, payload));

            Handler.Verify();
        }

        private void ExpectRequest(
            HttpMethod method,
            string uri,
            StringContent body = null,
            string responseBody = @"{""result"": 0}",
            HttpStatusCode responseStatus = HttpStatusCode.OK
           )
        {
            Handler.Protected().Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                IsMatchingRequest(method, uri, body),
                ItExpr.IsAny<CancellationToken>()
            ).ReturnsAsync(new HttpResponseMessage()
            {
                StatusCode = responseStatus,
                Content = ToStringContent(responseBody),
            }).Verifiable();
        }

        private StringContent ToStringContent(string body) => new StringContent(body, Encoding.UTF8, "application/json");

        private Expression IsMatchingRequest(HttpMethod method, string uri, StringContent body = null)
        {
            return ItExpr.Is<HttpRequestMessage>((it) => 
                it.Method == method 
                && it.RequestUri.ToString() == uri 
                && (body == null || it.Content.Headers.ContentType.MediaType.Contains("application/json"))
                && (body == null || it.Content.ReadAsStringAsync().Result == body.ReadAsStringAsync().Result)
            );
        }
    }
}