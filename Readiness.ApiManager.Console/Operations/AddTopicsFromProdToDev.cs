using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Ansarada.Struse.Gateway.Dto.Scorecard;
using AutoMapper;

namespace Readiness.ApiManager.Console.Operations
{
	public class AddTopicsFromProdToDev
	{
		public AddTopicsFromProdToDev()
		{
			Mapper.Initialize(cfg =>
			{
				cfg.CreateMap<TopicDto, TopicRequestDto>(MemberList.Source);
				cfg.CreateMap<RequirementDto, RequirementRequestDto>(MemberList.Source);
			});
		}

		public HttpClient SourceClient { get; set; }

		public HttpClient DestClient { get; set; }

		public async Task Run()
		{
			var sourceUrl = ConfigurationManager.AppSettings["PROD_GatewayApiUrl"];

			SourceClient = new HttpClient
			{
				BaseAddress = new Uri(sourceUrl),
				Timeout = TimeSpan.FromSeconds(10)
			};
			SourceClient.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("AdHoc", "0.0.0.0"));

			await SetSourceBearerToken();

			var destUrl = ConfigurationManager.AppSettings["DEV_GatewayApiUrl"];

			DestClient = new HttpClient
			{
				BaseAddress = new Uri(destUrl),
				Timeout = TimeSpan.FromSeconds(10)
			};
			DestClient.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("AdHoc", "0.0.0.0"));

			await SetDestBearerToken();

			var response = await SourceClient.GetAsync("v1/topics");
			response.EnsureSuccessStatusCode();
			var prodTopics = await response.Content.ReadAsAsync<IEnumerable<TopicDto>>();

			//var xxx = prodTopics.FirstOrDefault(x => x.Title == "Related Party Arrangements");

			foreach (var t in prodTopics)
			{
				var r2 = await SourceClient.GetAsync($"v1/topics/{t.Id}/content");
				r2.EnsureSuccessStatusCode();
				var topicContent = await r2.Content.ReadAsAsync<TopicContentDto>();

				var topicRequest = Mapper.Map<TopicRequestDto>(t);

				if (t.Id < 65) continue;

				var newTopicResponse = await DestClient.PostAsJsonAsync("v1/topics", topicRequest);
				newTopicResponse.EnsureSuccessStatusCode();
				var newTopic = await newTopicResponse.Content.ReadAsAsync<TopicDto>();

				var newContentResponse = await DestClient.PutAsJsonAsync($"v1/topics/{newTopic.Id}/content", topicContent);
				newContentResponse.EnsureSuccessStatusCode();
				var newContent = await newContentResponse.Content.ReadAsAsync<TopicContentDto>();
			}
		}

		protected async Task SetSourceBearerToken()
		{
			var response = await SourceClient.PostAsJsonAsync("v1/login",
				new EmailPasswordApiModel { Email = "d.scher@ansarada.com", Password = "" });
			response.EnsureSuccessStatusCode();
			var content = await response.Content.ReadAsAsync<LoginResponseDto>();
			SourceClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", content.AccessToken);
		}
		protected async Task SetDestBearerToken()
		{
			var response = await DestClient.PostAsJsonAsync("v1/login",
				new EmailPasswordApiModel { Email = "readiness.admin@ansarada.com", Password = "pa$$w0rd1" });
			response.EnsureSuccessStatusCode();
			var content = await response.Content.ReadAsAsync<LoginResponseDto>();
			DestClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", content.AccessToken);
		}

		protected class EmailPasswordApiModel
		{
			public string Email { get; set; }
			public string Password { get; set; }
		}

		protected class LoginResponseDto
		{
			public string AccessToken { get; set; }
			public string ExpiresIn { get; set; }
			public string TokenType { get; set; }
		}
	}
}
