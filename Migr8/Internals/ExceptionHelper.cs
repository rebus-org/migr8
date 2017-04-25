using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Text;

namespace Migr8.Internals
{
	public static class ExceptionHelper
	{
		/// <summary>
		/// Builds up a message, using the Message field of the specified exception
		/// as well as any InnerExceptions. 
		/// </summary>
		/// <param name="exception">The exception.</param>
		/// <returns>A combined message string.</returns>
		public static string BuildMessage(Exception exception)
		{
			const bool isFriendlyMessage = false;
			return BuildMessage(exception, isFriendlyMessage);
		}

		/// <summary>
		/// Builds up a message, using the Message field of the specified exception
		/// as well as any InnerExceptions. Excludes exception names, creating more readable message
		/// </summary>
		/// <param name="exception">The exception.</param>
		/// <returns>A combined message string.</returns>
		public static string BuildFriendlyMessage(Exception exception)
		{
			const bool isFriendlyMessage = true;
			return BuildMessage(exception, isFriendlyMessage);
		}

		/// <summary>
		/// Builds up a message, using the Message field of the specified exception
		/// as well as any InnerExceptions.
		/// </summary>
		/// <param name="exception">The exception.</param>
		/// <returns>A combined stack trace.</returns>
		public static string BuildStackTrace(Exception exception)
		{
			StringBuilder sb = new StringBuilder(GetStackTrace(exception));

			foreach (Exception inner in FlattenExceptionHierarchy(exception))
			{
				sb.Append(Environment.NewLine);
				sb.Append("--");
				sb.Append(inner.GetType().Name);
				sb.Append(Environment.NewLine);
				sb.Append(GetStackTrace(inner));
			}

			return sb.ToString();
		}

		/// <summary>
		/// Gets the stack trace of the exception.
		/// </summary>
		/// <param name="exception">The exception.</param>
		/// <returns>A string representation of the stack trace.</returns>
		public static string GetStackTrace(Exception exception)
		{
			try
			{
				return exception.StackTrace;
			}
			catch (Exception)
			{
				return "No stack trace available";
			}
		}

		private static string BuildMessage(Exception exception, bool isFriendlyMessage)
		{
			StringBuilder sb = new StringBuilder();
			WriteException(sb, exception, isFriendlyMessage);

			foreach (Exception inner in FlattenExceptionHierarchy(exception))
			{
				sb.Append(Environment.NewLine);
				sb.Append("  ----> ");
				WriteException(sb, inner, isFriendlyMessage);
			}

			return sb.ToString();
		}

		private static void WriteException(StringBuilder sb, Exception inner, bool isFriendlyMessage)
		{
			if (!isFriendlyMessage)
			{
				sb.AppendFormat(CultureInfo.CurrentCulture, "{0} : ", inner.GetType().ToString());
			}
			sb.Append(inner.Message);
		}

		private static List<Exception> FlattenExceptionHierarchy(Exception exception)
		{
			var result = new List<Exception>();

			if (exception is ReflectionTypeLoadException)
			{
				var reflectionException = exception as ReflectionTypeLoadException;
				result.AddRange(reflectionException.LoaderExceptions);

				foreach (var innerException in reflectionException.LoaderExceptions)
					result.AddRange(FlattenExceptionHierarchy(innerException));
			}

			if (exception is AggregateException)
            {
                var aggregateException = (exception as AggregateException);
                result.AddRange(aggregateException.InnerExceptions);

                foreach (var innerException in aggregateException.InnerExceptions)
                    result.AddRange(FlattenExceptionHierarchy(innerException));
            }
            else

			if (exception.InnerException != null)
			{
				result.Add(exception.InnerException);
				result.AddRange(FlattenExceptionHierarchy(exception.InnerException));
			}

			return result;
		}
	}
}