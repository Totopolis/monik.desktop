using System;
using System.Globalization;
using System.Windows.Data;

namespace MonikDesktop.ViewModels
{
	public class SeverityToNameConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			var _severity = (byte) value;

			return _severity == 0
				? "Fat"
				: _severity == 10
					? "Err"
					: _severity == 20
						? "Wrn"
						: _severity == 30
							? "Inf"
							: _severity == 40 ? "Ver" : "???";
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}