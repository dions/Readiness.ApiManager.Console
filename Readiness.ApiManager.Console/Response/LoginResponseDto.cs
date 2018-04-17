namespace Readiness.ApiManager.Console.Response
{
	public class LoginResponseDto
	{
		public string AccessToken { get; set; }
		public string ExpiresIn { get; set; }
		public string TokenType { get; set; }
	}
}
