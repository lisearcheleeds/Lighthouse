using System;
using UnityEngine;

namespace Lighthouse
{
    public static class LHLogger
    {
        const string Tag = "Lighthouse";

        public static ILogger Logger { get; private set; } = Debug.unityLogger;

        public static event Action<LogType, string> OnLog;

        public static void SetLogger(ILogger logger)
        {
            Logger = logger ?? Debug.unityLogger;
        }

        public static void Log(object message)
        {
            var msg = message?.ToString();
            Logger.Log(LogType.Log, Tag, msg);
            OnLog?.Invoke(LogType.Log, msg);
        }

        public static void LogWarning(object message)
        {
            var msg = message?.ToString();
            Logger.LogWarning(Tag, msg);
            OnLog?.Invoke(LogType.Warning, msg);
        }

        public static void LogError(object message)
        {
            var msg = message?.ToString();
            Logger.LogError(Tag, msg);
            OnLog?.Invoke(LogType.Error, msg);
        }
    }
}
