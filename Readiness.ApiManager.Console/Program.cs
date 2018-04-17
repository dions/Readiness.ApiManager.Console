using System.Threading.Tasks;
using Readiness.ApiManager.Console.Operations;

namespace Readiness.ApiManager.Console
{
	class Program
	{
		static void Main(string[] args)
		{
			//var x = new AddTopicsFromProdToDev();
			//var t = new CleanupScorecards();

			//var chkFiles = new CheckFileExists();
			//Task.Run(chkFiles.Run).Wait();

			var linfox = new Linfox_20180416();
			Task.Run(linfox.Run).Wait();

			System.Console.ReadLine();
		}
	}
}
