using System.Globalization;

namespace CatClaw.Shared.Maui.Converters;

/// <summary>布尔取反转换器：true -> false，false -> true</summary>
public class InvertedBoolConverter : IValueConverter
{
    /// <summary>将布尔值取反</summary>
    /// <param name="value">原始布尔值</param>
    /// <param name="targetType">目标类型</param>
    /// <param name="parameter">额外参数（未使用）</param>
    /// <param name="culture">区域性信息</param>
    /// <returns>取反后的布尔值；非布尔值返回 true</returns>
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool b)
            return !b;
        return true;
    }

    /// <summary>反向转换，同样对布尔值取反</summary>
    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool b)
            return !b;
        return false;
    }
}
