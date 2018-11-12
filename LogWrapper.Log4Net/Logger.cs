// ***********************************************************************
// Assembly         : LogWrapper.MSMQ.Plugins.Common
// Author           : tdcart
// Created          : 02-26-2016
//
// Last Modified By : tdcart
// Last Modified On : 03-30-2016
// ***********************************************************************
// <copyright file="AppLog.cs" company="Tim Cartwright">
//     Copyright © Tim Cartwright 2016
// </copyright>
// <summary></summary>
// ***********************************************************************
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using log4net;
using log4net.Config;
using log4net.Core;

namespace LogWrapper.Log4net
{
	/// <summary>
	/// Application logging utility
	/// </summary>
	/// <seealso cref="LogWrapper.Log4net.ILogger" />
	/// <remarks>This class is a wrapper of the log4net library.</remarks>
	public class Logger : ILogger
	{
		#region Fields

		/// <summary>
		/// The _loggers
		/// </summary>
		private readonly ILog[] _loggers;
		/// <summary>
		/// The _lock object
		/// </summary>
		// ReSharper disable once InconsistentNaming
		private static readonly object _lockObj = new object();

		/// <summary>
		/// Gets the log file.
		/// </summary>
		/// <value>The log file.</value>
		public string LogFile { get; private set; }

		/// <summary>
		/// The Root logger. Used to wrap a base logger. Will be null if this logger is not wrapping another logger.
		/// </summary>
		public ILogger ParentLogger { get; protected set; }

		#endregion

		#region Ctor(s)
		/// <summary>
		/// Prevents a default instance of the <see cref="Logger" /> class from being created.
		/// </summary>
		// ReSharper disable once UnusedMember.Local
		private Logger() { }

		/// <summary>
		/// Initializes a new instance of the <see cref="Logger" /> class.
		/// </summary>
		/// <param name="configFile">The configuration file.</param>
		public Logger(string configFile)
			: this(null, configFile)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Logger"/> class. Important Note: When using a parent logger, the Repository attribute should be added to the AssemblyInfo.cs like so: [assembly: log4net.Config.Repository("***UniqueName***")]. 
		/// </summary>
		/// <param name="configFile">The configuration file.</param>
		/// <param name="parentLogger">The root logger.</param>
		public Logger(string configFile, ILogger parentLogger)
			: this(null, configFile, parentLogger)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Logger"/> class. Important Note: When using a parent logger, the Repository attribute should be added to the AssemblyInfo.cs like so: [assembly: log4net.Config.Repository("***UniqueName***")]. 
		/// </summary>
		/// <param name="properties">The properties.</param>
		/// <param name="configFile">The configuration file.</param>
		/// <param name="parentLogger">The root logger.</param>
		public Logger(IDictionary<string, string> properties, string configFile, ILogger parentLogger)
			: this(properties, configFile)
		{
			this.ParentLogger = parentLogger;
			this.CheckRootDepth();
		}


		/// <summary>
		/// Initializes the <see cref="Logger" /> class.
		/// </summary>
		/// <param name="properties">The properties.</param>
		/// <param name="configFile">The configuration file.</param>
		public Logger(IDictionary<string, string> properties, string configFile)
		{
			if (_loggers != null) { return; } //should not happen. just cya

			lock (_lockObj)
			{
				if (properties != null)
				{
					//the properties MUST be set before the GetLogger calls
					foreach (var item in properties)
					{
						GlobalContext.Properties[item.Key] = item.Value;
					}
				}
				//add the process id variable
				var process = Process.GetCurrentProcess();
				// ReSharper disable once ConditionIsAlwaysTrueOrFalse
				ThreadContext.Properties["pid"] = process != null ? process.Id : -1;
				
				//get the immediate calling assembly that called this method
				var frames = new StackTrace().GetFrames();
				var stackTraceAssemblies = (
					from f in frames
					let reflectedType = f.GetMethod().ReflectedType
					// ReSharper disable once ConditionIsAlwaysTrueOrFalse
					where !(reflectedType == null || reflectedType.Assembly == null)
					select reflectedType.Assembly
				).Distinct();

				var callerAssembly = stackTraceAssemblies.ElementAt(1);

				var assemblyName = callerAssembly.GetName().Name;

				//add the current directory path
				if (!Path.IsPathRooted(configFile))
				{
					// ReSharper disable once AssignNullToNotNullAttribute
					configFile = Path.Combine(Path.GetDirectoryName(callerAssembly.Location), configFile);
				}

				this.LogFile = configFile;
				//add the log directory
				var dir = Path.GetDirectoryName((Assembly.GetEntryAssembly() ?? callerAssembly).Location);
				dir = (dir ?? @"C:\Temp\").TrimEnd('\\') + '\\';
				GlobalContext.Properties["LogDirectory"] = dir;

				//check to see if the assembly has a repo attribute
				var repoAttribute = callerAssembly.GetCustomAttributes(typeof(RepositoryAttribute), false).FirstOrDefault();
				var repository = repoAttribute != null ? LogManager.GetRepository(callerAssembly) : LogManager.GetRepository();

				XmlConfigurator.ConfigureAndWatch(repository, new FileInfo(configFile));
				var loggers = repository.GetCurrentLoggers();

				if (loggers.Length == 0)
				{
					_loggers = new ILog[loggers.Length + 1];
					_loggers[0] = LogManager.GetLogger(callerAssembly, assemblyName);
					loggers.CopyTo(_loggers, 1);
				}
				else
				{
					_loggers = new ILog[loggers.Length];
					for (var i = 0; i < loggers.Length; i++)
					{
						_loggers[i] = loggers[i] == null ?
							LogManager.GetLogger(callerAssembly, assemblyName) : 
							new LogImpl(loggers[i]);
					}
				}
				repository.Configured = true;
			}
		}

		#endregion

		#region Methods


		/// <summary>
		/// Logs a message to the log level if enabled, and formats the message using the <paramref name="args" />.
		/// </summary>
		/// <param name="format">The message format.</param>
		/// <param name="loglevel">The log level to use if enabled.</param>
		/// <param name="args">The arguments to format the message with.</param>
		public virtual void Log(string format, AppLogLevel loglevel, params object[] args)
		{
			if (this.ParentLogger != null) { this.ParentLogger.Log(format, loglevel, args); }

			var message = string.Format(format, args);

			this.WriteToLogger(loglevel, message);
		}

		/// <summary>
		/// Logs the exception as either a ERROR or FATAL depending upon log level.
		/// </summary>
		/// <param name="format">The format.</param>
		/// <param name="exception">The exception.</param>
		/// <param name="loglevel">The loglevel.</param>
		/// <param name="args">The arguments to format the message with.</param>
		public virtual void LogException(string format, Exception exception, AppLogLevel loglevel, params object[] args)
		{
			if (this.ParentLogger != null) { this.ParentLogger.LogException(format, exception, loglevel, args); }

			string exceptionMessage = null;
			if (exception != null) { exceptionMessage = exception.ToString(); }
			var message = string.Format(format, args);
			message += string.Format("\r\n{0}", exceptionMessage);

			this.LogException(message, loglevel);
		}

		/// <summary>
		/// Logs the exception as either a ERROR or FATAL depending upon log level.
		/// </summary>
		/// <param name="format">The format.</param>
		/// <param name="loglevel">The loglevel.</param>
		/// <param name="args">The arguments to format the message with.</param>
		public virtual void LogException(string format, AppLogLevel loglevel, params object[] args)
		{
			if (this.ParentLogger != null) { this.ParentLogger.LogException(format, loglevel, args); }

			var message = string.Format(format, args);
			this.WriteToLogger(loglevel, message);
		}

		/// <summary>
		/// Writes to logger.
		/// </summary>
		/// <param name="loglevel">The loglevel.</param>
		/// <param name="message">The message.</param>
		protected virtual void WriteToLogger(AppLogLevel loglevel, string message)
		{
			//used by the asf appender to log detailed or standard traces
			ThreadContext.Properties["LogLevel"] = loglevel.ToString();

			foreach (var log in _loggers)
			{
				if (loglevel == AppLogLevel.Info && log.IsInfoEnabled) //informational messages
				{
					log.Info(message);
				}
				else if (loglevel == AppLogLevel.Warn && log.IsWarnEnabled) //warnings
				{
					log.Warn(message);
				}
				else if (loglevel == AppLogLevel.Error && log.IsErrorEnabled) //handled exceptions
				{
					log.Error(message);
				}
				else if (loglevel == AppLogLevel.Fatal && log.IsFatalEnabled) //un-handled exceptions, or dire exceptions
				{
					log.Fatal(message);
				}
			}

			ThreadContext.Properties["LogLevel"] = null;
		}

		/// <summary>
		/// Checks the root depth and block too many levels of root logging to occur.
		/// </summary>
		/// <exception cref="System.OverflowException">Too many nested levels of root loggers.</exception>
		private void CheckRootDepth()
		{
			//don't allow too many nested loggers
			var depth = 0;
			var root = this.ParentLogger;
			while (root != null)
			{
				if (depth++ >= 3) { throw new OverflowException("Too many nested levels of root loggers."); }
				root = root.ParentLogger;
			}
		}
		#endregion
	}
}


