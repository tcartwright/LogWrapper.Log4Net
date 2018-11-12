using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using LogWrapper.Log4net;

namespace TestApp
{
	class Program
	{
		static void Main(string[] args)
		{
			bool runParallel = true;
			if (args.Length >= 1) { Boolean.TryParse(Convert.ToString(args[0]), out runParallel); }

			int runcount = 50;
			if (args.Length >= 2) { Int32.TryParse(Convert.ToString(args[1]), out runcount); }

			if (runParallel)
			{
				ParallelOptions options = new ParallelOptions() { MaxDegreeOfParallelism = 5 };
				Parallel.For(0, runcount, options, (x) =>
				{
					LogStuff(x);
				});

			}
			else
			{
				for (int x = 0; x < runcount; x++)
				{
					LogStuff(x);
				}
			}

			Console.ForegroundColor = ConsoleColor.Yellow;
			Console.WriteLine("Done. Press any key to continue . . .");
			Console.ResetColor();
			Console.ReadKey(true);
		}

		private static void LogStuff(int x)
		{
			Guid guid = Guid.NewGuid();
			Random rand = new Random(guid.GetHashCode());

			ILogger logger = new Logger(ConfigurationManager.AppSettings["Log4NetFile"]);

			//try to throw some randomness into the brakes of the logging
			Thread.Sleep(rand.Next(100, 1000));
			logger.Log("this is a test 2 on process : {0}", AppLogLevel.Warn, guid.ToString());

			//try to throw some randomness into the brakes of the logging
			Thread.Sleep(rand.Next(100, 1000));
			logger.Log("this is a test 3 on process : {0}", AppLogLevel.Info, guid.ToString());

			logger.Log("Loop ({0}) done on process : {1}", AppLogLevel.Error, x, guid.ToString());
		}
	}
}
