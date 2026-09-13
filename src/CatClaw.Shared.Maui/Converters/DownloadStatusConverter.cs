using System.Globalization;

namespace CatClaw.Shared.Maui.Converters;

/// <summary>
/// 下载状态可见性转换器（猫爪音乐 / 猫爪影视共用，源自两 App 原实现逐字节归一后合并）：
/// value 为 DownloadStatus，parameter 为逗号分隔的期望状态集合，
/// 值命中任一状态时返回 true（控件可见）。
/// 用于按任务状态显示暂停/继续/重试/删除等操作按钮。
/// <para>
/// ⚠ XAML 中多状态参数必须用单引号包裹：Binding 标记扩展内逗号是属性分隔符。
/// </para>
/// </summary>
/// <remarks>
/// 各 App 内保留同名子类（CatClawX.Maui.Converters.DownloadStatusConverter : 本类）
/// 作为 XAML/C# 的兼容占位——既有 xmlns 与资源字典无需改动。
/// </remarks>
public class DownloadStatusConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value == null || parameter == null) return false;
        var states = parameter.ToString()?.Split(',', StringSplitOptions.RemoveEmptyEntries)
            ?? Array.Empty<string>();
        return states.Contains(value.ToString(), StringComparer.OrdinalIgnoreCase);
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
