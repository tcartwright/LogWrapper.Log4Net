// ***********************************************************************
// Assembly         : LogWrapper.MSMQ.Plugins.Common
// Author           : tdcart
// Created          : 03-07-2016
//
// Last Modified By : tdcart
// Last Modified On : 03-07-2016
// ***********************************************************************
// <copyright file="AppLogLevel.cs" company="Tim Cartwright">
//     Copyright © Tim Cartwright 2016
// </copyright>
// <summary></summary>
// ***********************************************************************

namespace LogWrapper.Log4net
{
	/// <summary>
	/// Enum AppLogLevel
	/// </summary>
	public enum AppLogLevel
	{
		/// <summary>
		/// debug log messages (chatty)
		/// </summary>
		Debug,
		/// <summary>
		/// informational log messages
		/// </summary>
		Info,
		/// <summary>
		/// warnings log messages
		/// </summary>
		Warn,
		/// <summary>
		/// handled exceptions
		/// </summary>
		Error,
		/// <summary>
		/// un-handled exceptions
		/// </summary>
		Fatal
	}
}
