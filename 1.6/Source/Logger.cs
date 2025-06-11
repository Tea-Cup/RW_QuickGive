namespace Foxy.QuickGive {
	internal static class Logger {
		public static void Log(string message, params object[] args) {
			if (args.Length > 0) message = string.Format(message, args);
			Verse.Log.Message("[QuickGive] " + message);
		}
		public static void Log(object message) {
			Log(message ?? "<null>");
		}

		public static void Warn(string message, params object[] args) {
			if (args.Length > 0) message = string.Format(message, args);
			Verse.Log.Warning("[QuickGive] " + message);
		}
		public static void Warn(object message) {
			Warn(message ?? "<null>");
		}

		public static void Error(string message, params object[] args) {
			if (args.Length > 0) message = string.Format(message, args);
			Verse.Log.Error("[QuickGive] " + message);
		}
		public static void Error(object message) {
			Error(message ?? "<null>");
		}
	}
}
