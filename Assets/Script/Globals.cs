using UnityEngine;

namespace Footsies {
	public interface ILogger : UnityEngine.ILogger {
		void LogWarning(object message);
		void LogError(object message);
	}
	
	public static class Globals {
		public static readonly ILogger Logger = new FootsiesLogger(); 
	}
}