using System;
using UnityEngine;

namespace ScrapYard
{
    internal static class Logging
    {
        internal enum LogType
        {
            INFO,
            WARNING,
            ERROR,
            EXCEPTION
        }

        internal class Timer : IDisposable
        {
            private string _msg = string.Empty;
            System.Diagnostics.Stopwatch _watch = null;
            public Timer(string message)
            {
                _msg = message;
                _watch = System.Diagnostics.Stopwatch.StartNew();
            }

            public void Dispose()
            {
                _watch.Stop();
                DebugLog($"{_msg}: {_watch.ElapsedMilliseconds}ms");
            }

            internal static Timer StartNew(string message)
            {
                return new Timer(message);
            }

            internal TimeSpan Elapsed()
            {
                return _watch.Elapsed;
            }
        }

        /// <summary>
        /// Logs the provided message only if built in Debug mode
        /// </summary>
        /// <param name="msg">The object to log</param>
        /// <param name="type">The type of message being logged (severity)</param>
        internal static void DebugLog(object msg, LogType type = LogType.INFO)
        {
            bool shouldLog = ScrapYard.Instance?.Settings?.CurrentSaveSettings?.DebugLogging ?? false;
#if DEBUG
            shouldLog = true;
#endif
            if (shouldLog)
            {
                Log(msg, type);
            }
        }

        /// <summary>
        /// Logs the provided message
        /// </summary>
        /// <param name="msg">The object to log</param>
        /// <param name="type">The type of message being logged (severity)</param>
        internal static void Log(object msg, LogType type = LogType.INFO)
        {
            string final = "[ScrapYard] " + msg?.ToString();
            if (type == LogType.INFO)
            {
                Debug.Log(final);
            }
            else if (type == LogType.WARNING)
            {
                Debug.LogWarning(final);
            }
            else if (type == LogType.ERROR)
            {
                Debug.LogError(final);
            }
            else if (type == LogType.EXCEPTION)
            {
                Exception ex;
                if ((ex = msg as Exception) != null)
                {
                    LogException(ex);
                }
                else
                {
                    Debug.LogError(msg);
                }
            }
        }

        /// <summary>
        /// Logs the provided Exception
        /// </summary>
        /// <param name="ex">The exception to log</param>
        internal static void LogException(Exception ex)
        {
            Debug.LogException(ex);
        }
    }
}
