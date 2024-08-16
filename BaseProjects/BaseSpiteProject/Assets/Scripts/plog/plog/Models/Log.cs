using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace plog.Models
{
	public struct Log
	{
		public string Message;

		public DateTime Timestamp;

		public Level Level;

		[Nullable(new byte[] { 2, 1 })]
		public IEnumerable<Tag> ExtraTags;

		public string StackTrace;

		public Log(string message, Level level, [Nullable(new byte[] { 2, 1 })] IEnumerable<Tag> extraTags = null, [Nullable(2)] string stackTrace = null)
		{
			Message = null;
			Timestamp = default(DateTime);
			Level = default(Level);
			ExtraTags = null;
			StackTrace = null;
		}
	}
}
