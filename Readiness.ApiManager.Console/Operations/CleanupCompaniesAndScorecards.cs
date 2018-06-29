using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Ansarada.Struse.Gateway.Core.Paging;
using Ansarada.Struse.Gateway.Dto;
using Ansarada.Struse.Gateway.Dto.Company;
using Ansarada.Struse.Gateway.Dto.Scorecard;
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

			System.Console.WriteLine("Authorising...");
			await SetBearerToken();

			System.Console.WriteLine("Getting companies");
			var response2 = await Client.GetAsync("v1/companies");
			response2.EnsureSuccessStatusCode();
			var companies = await response2.Content.ReadAsAsync<IEnumerable<CompanyDto>>();

			var listOfCompanies = (companies.Where(x => x.Name.StartsWith("Smoketest Company "))).ToList();
			//var listOfCompanies = (companies.Where(x => x.Name.StartsWith("Smoketest advising company "))).ToList();
			
			System.Console.WriteLine("Total companies in filter: " + listOfCompanies.Count);

			var i = 0;

			var tasks = new List<Task>();

			while (i <= listOfCompanies.Count-1)
			{
				var c = listOfCompanies[i];

				tasks.Add(Task.Run(async () => await DeleteCompanyAndScorecards(c)));

				i++;

				if (i % 25 == 0)
				{
					Task.WaitAll(tasks.ToArray());
					tasks = new List<Task>();

					System.Threading.Thread.Sleep(100);
				}
			}

			System.Console.WriteLine("-end-");
		}

		private async Task<DeletedDto> DeleteCompanyAndScorecards(CompanyDto company)
		{
			System.Console.WriteLine($"Company '{company.Name}' = {company.Id}");

			var scs = await GetScorecards(company.Id);

			foreach (var sc in scs.Data)
			{
				var del = await DeleteScorecard(sc.Id);
			}

			var deleteResponse = await DeleteCompany(company.Id);

			System.Console.WriteLine($"Deleted company '{company.Name}' [{deleteResponse.Id}]");

			return deleteResponse;
		}

		private async Task<PagedResult<ScorecardDto>> GetScorecards(int companyId)
		{
			var response = await Client.GetAsync($"v1/scorecards?companyId={companyId}&includeScore=false");
			response.EnsureSuccessStatusCode();
			return await response.Content.ReadAsAsync<PagedResult<ScorecardDto>>();
		}

		private async Task<DeletedDto> DeleteCompany(int companyId)
		{
			HttpResponseMessage deleteResponse;

			try
			{
				deleteResponse = await Client.DeleteAsync($"v1/companies/{companyId}");
				deleteResponse.EnsureSuccessStatusCode();
				return await deleteResponse.Content.ReadAsAsync<DeletedDto>();
			}
			catch (Exception e)
			{
				System.Console.WriteLine("ERROR:" + e.Message);
			}

			return null;
		}

		private async Task<DeletedDto> DeleteScorecard(int scorecardId)
		{
			var deleteResponse = await Client.DeleteAsync($"v1/scorecards/{scorecardId}");
			deleteResponse.EnsureSuccessStatusCode();
			return await deleteResponse.Content.ReadAsAsync<DeletedDto>();
		}

		protected async Task SetBearerToken()
		{
			try
			{
				var response = await Client.PostAsJsonAsync("v1/login", 
					new EmailPasswordApiModel { Email = "readiness.admin@ansarada.com", Password = "" });
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
