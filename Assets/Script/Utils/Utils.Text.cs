namespace CloverGame.Utils
{
	using System;

	public partial class Utils
	{
		/// <summary>
		/// 字符串相关。
		/// </summary>
		public static class Text
		{
			#region Format

			/// <summary>
			/// 格式化字符串。
			/// </summary>
			/// <remarks>仿照 .NET 较新的官方实现，调用 <see cref="DJCommon.StringBuilderCache"/>，以其减少 GC。参见 https://referencesource.microsoft.com/#mscorlib/system/string.cs </remarks>
			public static string Format(string format, object arg0)
			{
				return FormatHelper(format, arg0);
			}

			/// <summary>
			/// 格式化字符串。
			/// </summary>
			/// <remarks>仿照 .NET 较新的官方实现，调用 <see cref="DJCommon.StringBuilderCache"/>，以其减少 GC。参见 https://referencesource.microsoft.com/#mscorlib/system/string.cs </remarks>
			public static string Format(string format, object arg0, object arg1)
			{
				return FormatHelper(format, arg0, arg1);
			}

			/// <summary>
			/// 格式化字符串。
			/// </summary>
			/// <remarks>仿照 .NET 较新的官方实现，调用 <see cref="DJCommon.StringBuilderCache"/>，以其减少 GC。参见 https://referencesource.microsoft.com/#mscorlib/system/string.cs </remarks>
			public static string Format(string format, object arg0, object arg1, object arg2)
			{
				return FormatHelper(format, arg0, arg1, arg2);
			}

			/// <summary>
			/// 格式化字符串。
			/// </summary>
			/// <remarks>仿照 .NET 较新的官方实现，调用 <see cref="DJCommon.StringBuilderCache"/>，以其减少 GC。参见 https://referencesource.microsoft.com/#mscorlib/system/string.cs </remarks>
			public static string Format(string format, params object[] args)
			{
				if (args == null)
				{
					// To preserve the original exception behavior, throw an exception about format if both
					// args and format are null. The actual null check for format is in FormatHelper.
					throw new ArgumentNullException((format == null) ? "format" : "args");
				}

				return FormatHelper(format, args);
			}

			private static string FormatHelper(string format, params object[] args)
			{
				if (format == null)
					throw new ArgumentNullException("format");

				return StringBuilderCache.GetStringAndRelease(
					StringBuilderCache
						.Acquire(format.Length + args.Length * 8)
						.AppendFormat(format, args));
			}

			#endregion Format

			#region Conversion

			/// <summary>
			/// 这个函数轻易不要用，只在打印一些调试日志时用，效率较低
			/// </summary>
			/// <returns></returns>
			public static string BytesToString(byte[] byteData, int nStartIndex, int nCount)
			{				
				string strResult = "";
				if (nStartIndex < 0 || nStartIndex >= byteData.Length)
				{
					return strResult;
				}

				for (int i = nStartIndex; i < nCount && i < byteData.Length; i++)
				{
					strResult += Convert.ToString(byteData[i]);
				}
				return strResult;
			}

			#endregion Conversion

			#region Type

			/// <summary>
			/// 根据类型和名称获取完整名称。
			/// </summary>
			/// <typeparam name="T">类型。</typeparam>
			/// <param name="name">名称。</param>
			/// <returns>完整名称。</returns>
			public static string GetFullName<T>(string name)
			{
				return GetFullName(typeof(T), name);
			}

			/// <summary>
			/// 根据类型和名称获取完整名称。
			/// </summary>
			/// <param name="type">类型。</param>
			/// <param name="name">名称。</param>
			/// <returns>完整名称。</returns>
			public static string GetFullName(Type type, string name)
			{
				string typeName = type.FullName;
				return string.IsNullOrEmpty(name) ? typeName : string.Format("{0}.{1}", typeName, name);
			}

			#endregion Type
		}
	}
}
