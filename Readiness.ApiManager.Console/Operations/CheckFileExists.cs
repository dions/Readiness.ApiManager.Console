using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Ansarada.Struse.Gateway.Dto.Company;
using Ansarada.Struse.Gateway.Dto.Document;
using Readiness.ApiManager.Console.Request;
using Readiness.ApiManager.Console.Response;

namespace Readiness.ApiManager.Console.Operations
{
	public class CheckFileExists
	{
		public HttpClient Client { get; set; }

		public async Task Run()
		{
			var url = ConfigurationManager.AppSettings["PROD_GatewayApiUrl"]; // PRODUCTION URL

			Client = new HttpClient
			{
				BaseAddress = new Uri(url),
				Timeout = TimeSpan.FromSeconds(10)
			};
			Client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("AdHoc", "0.0.0.0"));

			await SetBearerToken();

			var response1 = await Client.GetAsync("v1/documents");
			response1.EnsureSuccessStatusCode();
			var docs = await response1.Content.ReadAsAsync<IEnumerable<DocumentDto>>();
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
