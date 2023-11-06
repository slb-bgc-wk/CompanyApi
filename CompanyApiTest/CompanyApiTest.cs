using CompanyApi;
using Microsoft.AspNetCore.Mvc.Testing;
using Newtonsoft.Json;
using System.Net;
using System.Net.Http.Json;
using System.Text;

namespace CompanyApiTest
{
    public class CompanyApiTest
    {
        private HttpClient httpClient;
        private string url = "/api/companies";

        public CompanyApiTest()
        {
            WebApplicationFactory<Program> webApplicationFactory = new WebApplicationFactory<Program>();
            httpClient = webApplicationFactory.CreateClient();
        }

        [Fact]
        public async Task Should_return_created_company_with_status_201_when_create_cpmoany_given_a_company_name()
        {
            // Given
            await ClearDataAsync();
            Company companyGiven = new Company("BlueSky Digital Media");
            
            // When
            HttpResponseMessage httpResponseMessage = await httpClient.PostAsync(
                "/api/companies", 
                SerializeObjectToContent(companyGiven)
            );
           
            // Then
            Assert.Equal(HttpStatusCode.Created, httpResponseMessage.StatusCode);
            Company? companyCreated = await DeserializeTo<Company>(httpResponseMessage);
            Assert.NotNull(companyCreated);
            Assert.NotNull(companyCreated.Id);
            Assert.Equal(companyGiven.Name, companyCreated.Name);
        }

        [Fact]
        public async Task Should_return_bad_reqeust_when_create_company_given_a_existed_company_name()
        {
            // Given
            await ClearDataAsync();
            Company companyGiven = new Company("BlueSky Digital Media");

            // When
            await httpClient.PostAsync("/api/companies", SerializeObjectToContent(companyGiven));
            HttpResponseMessage httpResponseMessage = await httpClient.PostAsync(
                "/api/companies", 
                SerializeObjectToContent(companyGiven)
            );
            // Then
            Assert.Equal(HttpStatusCode.BadRequest, httpResponseMessage.StatusCode);
        }

        [Fact]
        public async Task Should_return_bad_reqeust_when_create_company_given_a_company_with_unknown_field()
        {
            // Given
            await ClearDataAsync();
            StringContent content = new StringContent("{\"unknownField\": \"BlueSky Digital Media\"}", Encoding.UTF8, "application/json");
          
            // When
            HttpResponseMessage httpResponseMessage = await httpClient.PostAsync("/api/companies", content);
           
            // Then
            Assert.Equal(HttpStatusCode.BadRequest, httpResponseMessage.StatusCode);
        }

        [Fact]
        public async Task Should_return_all_companies_with_status_201_when_getAll_companies_Given_without_query()
        {
            // Given
            await ClearDataAsync();

            //When
            HttpResponseMessage httpResponseMessage = await httpClient.GetAsync(url);
            List<Company>? companies = await httpResponseMessage.Content.ReadFromJsonAsync<List<Company>>();
            
            //Then
            Assert.Equal(HttpStatusCode.OK, httpResponseMessage.StatusCode);
            Assert.NotNull(companies);
        }

        [Fact]
        public async Task Should_return_specific_companies_with_200_When_get_Given_company_ID()
        {
            //Given
            await ClearDataAsync();
            CompanyRequest companyGiven = new CompanyRequest("West Digital Media");
            HttpResponseMessage httpResponseMessage = await httpClient.PostAsJsonAsync(url, companyGiven);
            Company? createdCompany = await httpResponseMessage.Content.ReadFromJsonAsync<Company>();
            string id = createdCompany.Id;

            //When

            HttpResponseMessage gotCompanyMessage = await httpClient.GetAsync(url+ $"/{id}");
            Company? gotCompany = await gotCompanyMessage.Content.ReadFromJsonAsync<Company>();

            //Then
            Assert.Equal(HttpStatusCode.OK , gotCompanyMessage.StatusCode);
            Assert.NotNull(gotCompany);
        }

        [Fact]
        public async Task Should_return_not_found_with_404_When_get_Given_ID()
        {
            //Given
            await ClearDataAsync();
            CompanyRequest companyGiven = new CompanyRequest("West Digital Media");
            HttpResponseMessage httpResponseMessage = await httpClient.PostAsJsonAsync(url, companyGiven);
            Company? createdCompany = await httpResponseMessage.Content.ReadFromJsonAsync<Company>();
            string id = createdCompany.Id;

            //When
            string fakeURL = $"{url}/99";
            HttpResponseMessage gotCompanyMessage = await httpClient.GetAsync(fakeURL);

            //Then
            Assert.Equal(HttpStatusCode.NotFound, gotCompanyMessage.StatusCode);
        }

        private async Task<T?> DeserializeTo<T>(HttpResponseMessage httpResponseMessage)
        {
            string response = await httpResponseMessage.Content.ReadAsStringAsync();
            T? deserializedObject = JsonConvert.DeserializeObject<T>(response);
            return deserializedObject;
        }

        private static StringContent SerializeObjectToContent<T>(T objectGiven)
        {
            return new StringContent(JsonConvert.SerializeObject(objectGiven), Encoding.UTF8, "application/json");
        }

        private async Task ClearDataAsync()
        {
            await httpClient.DeleteAsync("/api/companies");
        }
    }
}