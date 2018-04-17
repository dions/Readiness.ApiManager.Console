using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Ansarada.Struse.Gateway.Core.Paging;
using Ansarada.Struse.Gateway.Dto.Company;
using Ansarada.Struse.Gateway.Dto.Scorecard;
using Readiness.ApiManager.Console.Request;
using Readiness.ApiManager.Console.Response;

namespace Readiness.ApiManager.Console.Operations
{
	public class CleanupScorecards
	{
		public HttpClient Client { get; set; }

		public async Task Run()
		{
			var url = ConfigurationManager.AppSettings["STAGE_GatewayApiUrl"]; // STAGING

			Client = new HttpClient
			{
				BaseAddress = new Uri(url),
				Timeout = TimeSpan.FromSeconds(10)
			};
			Client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("AdHoc", "0.0.0.0"));

			await SetBearerToken();

			//var response1 = await Client.GetAsync("v1/scorecards?isTemplate=false&includeScore=false&limit=1000");
			//response1.EnsureSuccessStatusCode();
			//var scorecards = await response1.Content.ReadAsAsync<PagedResult<ScorecardDto>>();

			//var sc = scorecards.Data.Where(x => x.Name.StartsWith("Smoketest"));

			//foreach (var s in sc)
			//{
			//	var responseDelSc = await Client.DeleteAsync($"v1/scorecards/{s.Id}");
			//	responseDelSc.EnsureSuccessStatusCode();
			//}

			var response2 = await Client.GetAsync("v1/companies");
			response2.EnsureSuccessStatusCode();
			var companies = await response2.Content.ReadAsAsync<IEnumerable<CompanyDto>>();

			var c = companies.Where(x => x.Name.StartsWith("CrudScorecardReturnsOk"));
		}

		protected async Task SetBearerToken()
		{
			var response = await Client.PostAsJsonAsync("v1/login", 
				new EmailPasswordApiModel { Email = "readiness.admin@ansarada.com", Password = "36Fd1OGLZYwYWQx4pYAa" });
			response.EnsureSuccessStatusCode();
			var content = await response.Content.ReadAsAsync<LoginResponseDto>();
			Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", content.AccessToken);
		}
	}
}
