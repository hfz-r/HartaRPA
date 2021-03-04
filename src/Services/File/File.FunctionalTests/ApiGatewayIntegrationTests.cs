using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Harta.Services.Ordering.Grpc;
using Newtonsoft.Json;
using Xunit;

namespace File.FunctionalTests
{
    public class ApiGatewayIntegrationTests
    {
        public ApiGatewayIntegrationTests()
        {
            //make sure test container run?
        }

        [Fact]
        public async Task Format_test_with_json_transcoder()
        {
            //Arrange
            var client1 = new HttpClient() {BaseAddress = new Uri("http://localhost:8080/")};
            var client2 = new HttpClient(GetHttpClientHandler()) {BaseAddress = new Uri("https://localhost:8443/")};
            var expected = new CreateOrderResponse
            {
                Status = true,
                Message = "Ok"
            };

            //Act - Client1
            client1.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var response1A = await client1.GetAsync("format/invalid1_spec1");
            var response1B = await client1.GetAsync("format/invalid1_spec2.csv?file_type=D365");

            //Assert - Client1
            response1A.EnsureSuccessStatusCode();
            response1B.EnsureSuccessStatusCode();

            var result1A = await GetActualResult(response1A);
            var result1B = await GetActualResult(response1B);

            Assert.Equal(result1A, result1B);
            Assert.Equal(expected, result1A);
            Assert.Equal(expected, result1B);

            //Act - Client2
            client2.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var response2A = await client2.GetAsync("format/invalid2_spec1.csv");
            var response2B = await client2.GetAsync("format/invalid2_spec2");

            //Assert - Client2
            response2A.EnsureSuccessStatusCode();
            response2B.EnsureSuccessStatusCode();

            var result2A = await GetActualResult(response2A);
            var result2B = await GetActualResult(response2B);

            Assert.Equal(result2A, result2B);
            Assert.Equal(expected, result2A);
            Assert.Equal(expected, result2B);

            HttpClientHandler GetHttpClientHandler()
            {
                return new HttpClientHandler
                {
                    ClientCertificateOptions = ClientCertificateOption.Manual,
                    ServerCertificateCustomValidationCallback =
                        (httpRequestMessage, cert, cetChain, policyErrors) => true
                };
            }

            async Task<CreateOrderResponse> GetActualResult(HttpResponseMessage response)
            {
                var responseString = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<CreateOrderResponse>(responseString);

                return result;
            }
        }
    }
}