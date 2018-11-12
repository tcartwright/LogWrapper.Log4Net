using System.Diagnostics;
using LogWrapper.Log4net;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LogWrapper.Log4NetTests
{
	[TestClass()]
	public class LoggerTests
	{

		public TestContext TestContext { get; set; }
		[TestMethod()]
		public void LogTest()
		{
			ILogger logger = new Logger("Sample.Log4Net.config");

			logger.Log("this is a test", AppLogLevel.Debug);

            logger.LogException("this is a test exception", new System.Exception("Test Exception"), AppLogLevel.Fatal);

            Debug.WriteLine(this.TestContext.DeploymentDirectory);
		}
	}
}
