using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using LabGuard.Common;

namespace LabGuard.Host
{
    public class StatusToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is ClientStatus s)
            {
                return s switch
                {
                    ClientStatus.Normal => Brushes.Green,
                    ClientStatus.Misuse => Brushes.Red,
                    ClientStatus.Offline => Brushes.Gray,
                    _ => Brushes.Gray
                };
            }
            return Brushes.Gray;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
