using System;
using System.Globalization;
using System.Windows.Data;

namespace MonikDesktop.Common.Converters
{
	public class LevelToNameConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			var _level = (byte) value;

			return _level == 0
				? "Sys"
				: _level == 10
					? "App"
					: _level == 20
						? "Lgc"
						: _level == 30 ? "Sec" : "???";
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}