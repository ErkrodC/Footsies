using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Footsies {
	public class FootsiesLogger : ILogger {
		public ILogHandler logHandler { get; set; }
		public bool logEnabled { get; set; }
		public LogType filterLogType { get; set; }

#if DEBUG
		public bool IsLogTypeAllowed(LogType logType) {
			return Debug.unityLogger.IsLogTypeAllowed(logType);
		}

		public void Log(LogType logType, object message) {
			Debug.unityLogger.Log(logType, message);
		}

		public void Log(LogType logType, object message, Object context) {
			Debug.unityLogger.Log(logType, message, context);
		}

		public void Log(LogType logType, string tag, object message) {
			Debug.unityLogger.Log(logType, tag, message);
		}

		public void Log(LogType logType, string tag, object message, Object context) {
			Debug.unityLogger.Log(logType, tag, message, context);
		}

		public void Log(object message) {
			Debug.unityLogger.Log(message);
		}

		public void Log(string tag, object message) {
			Debug.unityLogger.Log(tag, message);
		}

		public void Log(string tag, object message, Object context) {
			Debug.unityLogger.Log(tag, message, context);
		}

		public void LogWarning(object message) {
			Debug.LogWarning(message);
		}

		public void LogWarning(string tag, object message) {
			Debug.unityLogger.LogWarning(tag, message);
		}

		public void LogWarning(string tag, object message, Object context) {
			Debug.unityLogger.LogWarning(tag, message, context);
		}

		public void LogError(object message) {
			Debug.LogError(message);
		}

		public void LogError(string tag, object message) {
			Debug.unityLogger.LogError(tag, message);
		}

		public void LogError(string tag, object message, Object context) {
			Debug.unityLogger.LogError(tag, message, context);
		}

		public void LogFormat(LogType logType, string format, params object[] args) {
			Debug.unityLogger.LogFormat(logType, format, args);
		}

		public void LogFormat(LogType logType, Object context, string format, params object[] args) {
			Debug.unityLogger.LogFormat(logType, context, format, args);
		}

		public void LogException(Exception exception) {
			Debug.unityLogger.LogException(exception);
		}

		public void LogException(Exception exception, Object context) {
			Debug.unityLogger.LogException(exception);
		}
#else
		public bool IsLogTypeAllowed(LogType logType) { return true; }
		public void Log(LogType logType, object message) { }
		public void Log(LogType logType, object message, Object context) { }
		public void Log(LogType logType, string tag, object message) { }
		public void Log(LogType logType, string tag, object message, Object context) { }
		public void Log(object message) { }
		public void Log(string tag, object message) { }
		public void Log(string tag, object message, Object context) { }
		public void LogWarning(object message) { }
		public void LogWarning(string tag, object message) { }
		public void LogWarning(string tag, object message, Object context) { }
		public void LogError(object message) { }
		public void LogError(string tag, object message) { }
		public void LogError(string tag, object message, Object context) { }
		public void LogFormat(LogType logType, string format, params object[] args) { }
		public void LogFormat(LogType logType, Object context, string format, params object[] args) { }
		public void LogException(Exception exception) { }
		public void LogException(Exception exception, Object context) { }
#endif
	}
}