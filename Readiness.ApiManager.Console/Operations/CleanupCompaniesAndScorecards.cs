using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Ansarada.Struse.Gateway.Dto.Company;
using Readiness.ApiManager.Console.Request;
using Readiness.ApiManager.Console.Response;

namespace Readiness.ApiManager.Console.Operations
{
	public class CleanupCompaniesAndScorecards
	{
		public HttpClient Client { get; set; }

		public async Task Execute()
		{
			var url = ConfigurationManager.AppSettings["STAGE_GatewayApiUrl"]; // STAGING

			Client = new HttpClient
			{
				BaseAddress = new Uri(url),
				Timeout = TimeSpan.FromSeconds(30)
			};
			Client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("AdHoc", "0.0.0.0"));

			await SetBearerToken();

			//var response1 = await Client.GetAsync("v1/scorecards?isTemplate=false&includeScore=false&limit=1000");
			//response1.EnsureSuccessStatusCode();
			//var scorecards = await response1.Content.ReadAsAsync<PagedResult<ScorecardDto>>();

			var response2 = await Client.GetAsync("v1/companies?id=455");
			response2.EnsureSuccessStatusCode();
			var companies = await response2.Content.ReadAsAsync<IEnumerable<CompanyDto>>();

			var deleteResponse = await Client.DeleteAsync($"v1/companies/{companies.First().Id}");
			deleteResponse.EnsureSuccessStatusCode();

			//var c = companies.Where(x => x.Name.StartsWith("API Tests - Company"));
		}

		protected async Task SetBearerToken()
		{
			try
			{
				var response = await Client.PostAsJsonAsync("v1/login", 
					new EmailPasswordApiModel { Email = "readiness.admin@ansarada.com", Password = "36Fd1OGLZYwYWQx4pYAa" });
				response.EnsureSuccessStatusCode();
				var content = await response.Content.ReadAsAsync<LoginResponseDto>();
				Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", content.AccessToken);
			}
			catch (Exception e)
			{
				System.Console.WriteLine(e);
				throw;
			}
		}
	}
}
