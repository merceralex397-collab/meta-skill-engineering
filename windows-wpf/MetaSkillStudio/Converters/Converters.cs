using System;
using System.Collections;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace MetaSkillStudio.Converters
{
    [ValueConversion(typeof(bool), typeof(Visibility))]
    public class BoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is true ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is Visibility.Visible;
        }
    }

    [ValueConversion(typeof(bool), typeof(Visibility))]
    public class InverseBoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is true ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is not Visibility.Visible;
        }
    }

    [ValueConversion(typeof(string), typeof(Visibility))]
    public class StringEmptyToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return string.IsNullOrWhiteSpace(value as string) ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Converts a collection count to Visibility. Returns Visible if count is 0, Collapsed otherwise.
    /// Used for showing empty state messages when collections are empty.
    /// </summary>
    [ValueConversion(typeof(int), typeof(Visibility))]
    public class CountToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Handle ICollection for Count property
            if (value is ICollection collection)
            {
                return collection.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
            }
            
            // Handle direct int values
            if (value is int count)
            {
                return count == 0 ? Visibility.Visible : Visibility.Collapsed;
            }
            
            // Try to get Count property via reflection for other collection types
            if (value != null)
            {
                var countProperty = value.GetType().GetProperty("Count");
                if (countProperty != null)
                {
                    var countValue = countProperty.GetValue(value);
                    if (countValue is int c)
                    {
                        return c == 0 ? Visibility.Visible : Visibility.Collapsed;
                    }
                }
            }
            
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Converts a collection count to Visibility. Returns Collapsed if count is 0, Visible otherwise.
    /// Used for hiding content when collections are empty.
    /// </summary>
    [ValueConversion(typeof(int), typeof(Visibility))]
    public class InverseCountToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Handle ICollection for Count property
            if (value is ICollection collection)
            {
                return collection.Count == 0 ? Visibility.Collapsed : Visibility.Visible;
            }
            
            // Handle direct int values
            if (value is int count)
            {
                return count == 0 ? Visibility.Collapsed : Visibility.Visible;
            }
            
            // Try to get Count property via reflection for other collection types
            if (value != null)
            {
                var countProperty = value.GetType().GetProperty("Count");
                if (countProperty != null)
                {
                    var countValue = countProperty.GetValue(value);
                    if (countValue is int c)
                    {
                        return c == 0 ? Visibility.Collapsed : Visibility.Visible;
                    }
                }
            }
            
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Returns Visible when the bound value equals the converter parameter, Collapsed otherwise.
    /// Used for page switching: Visibility="{Binding SelectedPage, Converter={StaticResource EqualityToVisibilityConverter}, ConverterParameter=Dashboard}"
    /// </summary>
    public class EqualityToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || parameter == null) return Visibility.Collapsed;
            return value.ToString() == parameter.ToString() ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Returns true when the bound value equals the converter parameter.
    /// Used for nav rail active state: IsChecked="{Binding SelectedPage, Converter={StaticResource EqualityConverter}, ConverterParameter=Dashboard}"
    /// </summary>
    public class EqualityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || parameter == null) return false;
            return value.ToString() == parameter.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter == null) return Binding.DoNothing;
            if (value is true && targetType.IsEnum)
                return Enum.Parse(targetType, parameter.ToString()!);
            return Binding.DoNothing;
        }
    }
}
