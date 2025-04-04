using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BinanceBotStrategy
{
	public abstract class Candle
	{

		/// <summary>
		/// Open time.
		/// </summary>
		public DateTimeOffset OpenTime { get; set; }

		/// <summary>
		/// Close time.
		/// </summary>
		public DateTimeOffset CloseTime { get; set; }

		/// <summary>
		/// High time.
		/// </summary>
		public DateTimeOffset HighTime { get; set; }

		/// <summary>
		/// Low time.
		/// </summary>
		public DateTimeOffset LowTime { get; set; }

		/// <summary>
		/// Opening price.
		/// </summary>
		public decimal OpenPrice { get; set; }

		/// <summary>
		/// Closing price.
		/// </summary>
		public decimal ClosePrice { get; set; }

		/// <summary>
		/// Highest price.
		/// </summary>
		public decimal HighPrice { get; set; }

		/// <summary>
		/// Lowest price.
		/// </summary>

		public decimal LowPrice { get; set; }
	}
}
