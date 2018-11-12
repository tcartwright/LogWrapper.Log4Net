// ***********************************************************************
// Assembly         : LogWrapper.MSMQ.Plugins.Common
// Author           : tdcart
// Created          : 02-26-2016
//
// Last Modified By : tdcart
// Last Modified On : 03-16-2016
// ***********************************************************************
// <copyright file="ILogger.cs" company="Tim Cartwright">
//     Copyright © Tim Cartwright 2016
// </copyright>
// <summary></summary>
// ***********************************************************************
using System;

namespace LogWrapper.Log4net
{

	/// <summary>
	/// Interface ILogger
	/// </summary>
	public interface ILogger
	{

		/// <summary>
		/// Gets the root logger.
		/// </summary>
		/// <value>The root logger.</value>
		ILogger ParentLogger { get; }

		/// <summary>
		/// Logs a message to the log level if enabled, and formats the message using the <paramref name="args" />.
		/// </summary>
		/// <param name="format">The message format.</param>
		/// <param name="loglevel">The loglevel to use if enabled.</param>
		/// <param name="args">The arguments to format the message with.</param>
		void Log(string format, AppLogLevel loglevel, params object[] args);

		/// <summary>
		/// Logs the exception as either a ERROR or FATAL depending upon log level.
		/// </summary>
		/// <param name="format">The format.</param>
		/// <param name="exception">The exception.</param>
		/// <param name="loglevel">The loglevel.</param>
		/// <param name="args">The arguments to format the message with.</param>
		void LogException(string format, Exception exception, AppLogLevel loglevel, params object[] args);

		/// <summary>
		/// Logs the exception as either a ERROR or FATAL depending upon log level.
		/// </summary>
		/// <param name="format">The format.</param>
		/// <param name="loglevel">The loglevel.</param>
		/// <param name="args">The arguments to format the message with.</param>
		void LogException(string format, AppLogLevel loglevel, params object[] args);

		/// <summary>
		/// Gets the log file.
		/// </summary>
		/// <value>The log file.</value>
		string LogFile { get; }
	}
}
