using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace MonikDesktop.ViewModels
{
  public class SeverityToNameConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      var _severity = (byte)value;

      return _severity == 0 ? "Fat" :
        _severity == 10 ? "Err" :
        _severity == 20 ? "Wrn" :
        _severity == 30 ? "Inf" :
        _severity == 40 ? "Ver" : "???";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
      throw new NotImplementedException();
    }
  }

  public class LevelToNameConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      var _level = (byte)value;

      return _level == 0 ? "Sys" :
        _level == 10 ? "App" :
        _level == 20 ? "Lgc" :
        _level == 30 ? "Sec" : "???";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
      throw new NotImplementedException();
    }
  }
}
