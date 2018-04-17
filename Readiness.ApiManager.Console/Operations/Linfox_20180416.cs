using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Ansarada.Struse.Gateway.Core.Paging;
using Ansarada.Struse.Gateway.Dto.Scorecard;
using Readiness.ApiManager.Console.Request;
using Readiness.ApiManager.Console.Response;

namespace Readiness.ApiManager.Console.Operations
{
	public class Linfox_20180416
	{
		public HttpClient Client { get; set; }

		private readonly string _username = "<username>";
		private readonly string _password = "<password>";
		private readonly string _apiUrlKey = "<apiUrl>";

		public async Task Run()
		{
			var url = ConfigurationManager.AppSettings[_apiUrlKey]; // API_URL

			Client = new HttpClient
			{
				BaseAddress = new Uri(url),
				Timeout = TimeSpan.FromSeconds(10)
			};
			Client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("AdHoc", "0.0.0.0"));

			await SetBearerToken();

			await PerformTask();
		}

		private async Task PerformTask()
		{
			var response1 = await Client.GetAsync("v1/scorecards?ownerCompanyId=754&limit=500");
			//var response1 = await Client.GetAsync("v1/scorecards?id=1428");
			response1.EnsureSuccessStatusCode();
			var scorecards = await response1.Content.ReadAsAsync<PagedResult<ScorecardDto>>();

			foreach (var sc in scorecards.Data)
			{
				System.Console.WriteLine($"ScorecardId = {sc.Id}");

				var response2 = await Client.GetAsync($"v1/scorecards/{sc.Id}/topics");
				response2.EnsureSuccessStatusCode();
				var scTopics = await response2.Content.ReadAsAsync<IEnumerable<ScorecardTopicDto>>();

				foreach (var scTopic in scTopics)
				{
					var response3 = await Client.DeleteAsync($"/v1/scorecards/{sc.Id}/topics/{scTopic.Id}");
					response3.EnsureSuccessStatusCode();
					Thread.Sleep(100);
				}

				System.Console.WriteLine($"ScorecardId = {sc.Id}. Delete completed.");

				var newTopics = new [] { 95, 1301, 1344, 1346, 1337, 1347, 1345 };

				foreach (var topicId in newTopics)
				{
					var response4 = await Client.PostAsync($"v1/scorecards/{sc.Id}/topics/{topicId}", null);
					response4.EnsureSuccessStatusCode();
					var scTopicDto = await response4.Content.ReadAsAsync<ScorecardTopicDto>();
					Thread.Sleep(100);
				}

				System.Console.WriteLine($"ScorecardId = {sc.Id}. Add completed.");
			}
		}

		protected async Task SetBearerToken()
		{
			var response = await Client.PostAsJsonAsync("v1/login", new EmailPasswordApiModel { Email = _username, Password = _password });
			response.EnsureSuccessStatusCode();
			var content = await response.Content.ReadAsAsync<LoginResponseDto>();
			Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", content.AccessToken);
		}
	}
}
