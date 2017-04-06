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

        /// <summary>
        /// Logs the provided message only if built in Debug mode
        /// </summary>
        /// <param name="msg">The object to log</param>
        /// <param name="type">The type of message being logged (severity)</param>
        internal static void DebugLog(object msg, LogType type = LogType.INFO)
        {
#if DEBUG
            Log(msg, type);
#endif
        }

        /// <summary>
        /// Logs the provided message
        /// </summary>
        /// <param name="msg">The object to log</param>
        /// <param name="type">The type of message being logged (severity)</param>
        internal static void Log(object msg, LogType type = LogType.INFO)
        {
            string final = "[ScrapYard] " + msg.ToString();
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
                if (msg is Exception)
                {
                    LogException(msg as Exception);
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
