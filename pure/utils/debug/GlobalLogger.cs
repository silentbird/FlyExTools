using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using UnityEngine;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;

namespace pure.utils.debug {
	public static class GlobalLogger {
		public static Action enableFrameDebuger;

		[PublicAPI]
		public static GlobalLogger.DebugInfoCallback debugInfoCb = new GlobalLogger.DebugInfoCallback(GlobalLogger.debug_info);

		private static readonly StringBuilder builder = new StringBuilder();
		private static readonly Queue<GlobalLogger.LogMessage> queue = new Queue<GlobalLogger.LogMessage>();
		private const string RETURN = "\r\n";
		private const string HEAD = "[GlobalLog]";
		private static readonly string dataPath = Application.dataPath;

		public static void AppendIndent(this StringBuilder sb, int indent) {
			for (int index = 0; index < indent; ++index)
				sb.Append("\t");
		}

		public static void AppendIndent(this StringBuilder sb, int indent, string k) {
			for (int index = 0; index < indent; ++index)
				sb.Append(k);
		}

		public static void Release() {
			lock (GlobalLogger.queue) {
				while (GlobalLogger.queue.Count > 0) {
					GlobalLogger.LogMessage logMessage = GlobalLogger.queue.Dequeue();
					GlobalLogger.print_info(logMessage.info, logMessage.context, logMessage.type);
				}
			}
		}

		private static void print_info(string message, Object context, LogType t) {
			int num = Application.isPlaying ? 1 : 0;
			Debug.unityLogger.Log(t, (object)message, context);
		}

		private static void debug_info(string msg, Object o) => Debug.Log((object)msg, o);

		public static void DebugInfo(string msg) {
			GlobalLogger.DebugInfoCallback debugInfoCb = GlobalLogger.debugInfoCb;
			if (debugInfoCb == null)
				return;
			debugInfoCb(msg, (Object)null);
		}

		public static void DebugInfo(string msg, Object o) {
			GlobalLogger.DebugInfoCallback debugInfoCb = GlobalLogger.debugInfoCb;
			if (debugInfoCb == null)
				return;
			debugInfoCb(msg, o);
		}

		public static void Log(string args) {
			GlobalLogger.add_log((LogType)3, args, false, (Object)null, string.Empty);
		}

		public static void Log(string args, Object context) {
			GlobalLogger.add_log((LogType)3, args, false, context, string.Empty);
		}

		public static void Log(bool isLua, string args) {
			GlobalLogger.add_log((LogType)3, args, isLua, (Object)null, string.Empty);
		}

		public static void Warn(string args) {
			GlobalLogger.add_log((LogType)2, args, false, (Object)null, string.Empty);
		}

		public static void Warn(string args, Object context) {
			GlobalLogger.add_log((LogType)2, args, false, context, string.Empty);
		}

		public static void Warn(bool islua, string args) {
			GlobalLogger.add_log((LogType)2, args, islua, (Object)null, string.Empty);
		}

		public static void Error(string args) {
			GlobalLogger.add_log((LogType)0, args, false, (Object)null, string.Empty);
		}

		public static void Error(string args, Object context) {
			GlobalLogger.add_log((LogType)0, args, false, context, string.Empty);
		}

		public static void Error(bool islua, string args) {
			GlobalLogger.add_log((LogType)0, args, islua, (Object)null, string.Empty);
		}

		public static void Error(Exception e) {
			GlobalLogger.add_log((LogType)0, e.Message, false, (Object)null, e.StackTrace);
		}

		public static void Error(Exception e, Object context) {
			GlobalLogger.add_log((LogType)0, e.Message, false, context, e.StackTrace);
		}

		public static void Error(bool isLua, Exception e) {
			GlobalLogger.add_log((LogType)0, e.Message, isLua, (Object)null, e.StackTrace);
		}

		private static void add_log(LogType type, string msg, bool isLua, Object obj, string stack) {
			if (string.IsNullOrEmpty(msg) && string.IsNullOrEmpty(stack))
				return;
			lock (GlobalLogger.queue) {
				GlobalLogger.builder.Length = 0;
				if (!isLua)
					GlobalLogger.builder.Append("[GlobalLog]");
				if (!string.IsNullOrEmpty(msg)) {
					GlobalLogger.builder.Append(msg);
					GlobalLogger.builder.Append("\r\n");
				}

				if (!string.IsNullOrEmpty(stack)) {
					GlobalLogger.builder.Append(stack);
					GlobalLogger.builder.Append("\r\n");
				}

				GlobalLogger.push_stack_trace(type, GlobalLogger.builder);
				GlobalLogger.queue.Enqueue(new GlobalLogger.LogMessage() {
					type = type,
					info = GlobalLogger.builder.ToString(),
					context = obj
				});
			}
		}

		private static void push_stack_trace(LogType type, StringBuilder to) {
			GlobalLogger.append_stacktrace(to);
		}

		private static void append_stacktrace(StringBuilder to) {
			StackFrame[] frames = new StackTrace(true).GetFrames();
			if (frames == null)
				return;
			for (int index = 1; index < frames.Length; ++index) {
				StackFrame stackFrame = frames[index];
				MethodBase method = stackFrame.GetMethod();
				Type reflectedType = method.ReflectedType;
				if (!(reflectedType == typeof(GlobalLogger))) {
					string path = GlobalLogger.get_path(stackFrame.GetFileName());
					int fileLineNumber = stackFrame.GetFileLineNumber();
					string str = reflectedType != (Type)null ? reflectedType.FullName : "<unknown>";
					to.AppendLine(string.Format("{0}:{1}() (at {2}:{3})", (object)str, (object)method.Name, (object)path, (object)fileLineNumber));
				}
			}
		}

		private static string get_path(string path) {
			if (string.IsNullOrEmpty(path))
				return string.Empty;
			path = path.Replace("\\", "/");
			return path.Replace(GlobalLogger.dataPath, "Assets");
		}

		private struct LogMessage {
			internal LogType type;
			internal Object context;
			internal string info;
		}

		public delegate void DebugInfoCallback(string msg, Object obj);
	}
}