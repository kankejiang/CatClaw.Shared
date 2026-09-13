namespace CatClaw.Shared.Maui.Helpers;

/// <summary>
/// 跨平台 SafeArea 辅助（猫爪音乐 / 猫爪影视共用）：提供系统栏高度（dp）并通知页面更新 padding。
/// Android 平台由平台代码（MainActivity 的 insets 监听）调用 UpdateInsets；Windows 平台默认为 0。
/// <para>各 App 内保留同名静态包装类（静态类不可继承，用委托成员转发），调用面与命名空间不变。</para>
/// </summary>
public static class SafeAreaHelper
{
    /// <summary>系统栏顶部高度（状态栏），单位 dp</summary>
    public static double TopInset { get; private set; }

    /// <summary>系统栏底部高度（导航栏），单位 dp</summary>
    public static double BottomInset { get; private set; }

    /// <summary>横屏左侧安全区（状态栏/刘海旋转到左侧），单位 dp。竖屏为 0。</summary>
    public static double LeftInset { get; private set; }

    /// <summary>更新系统栏高度并触发事件（由平台代码调用）</summary>
    /// <param name="topDp">状态栏高度（dp）</param>
    /// <param name="bottomDp">导航栏高度（dp）</param>
    /// <param name="leftDp">左侧安全区高度（dp，横屏时状态栏/刘海所在侧）</param>
    public static void UpdateInsets(double topDp, double bottomDp, double leftDp = 0)
    {
        // 直接采用系统回调的真实 insets 值：
        // - 横屏状态栏隐藏 → top=0 是正确值，不应被旧值覆盖
        // - 车机/竖屏状态栏可见 → top=实际高度
        // - 手势导航 → bottom=0 是正确值
        // 旧逻辑会"防 0 覆盖"，导致横屏下 TopInset 永远锁在竖屏的 24dp，造成内容被抬高。
        bool changed = Math.Abs(topDp - TopInset) > 0.5 || Math.Abs(bottomDp - BottomInset) > 0.5
                       || Math.Abs(leftDp - LeftInset) > 0.5;
        TopInset = topDp;
        BottomInset = bottomDp;
        LeftInset = leftDp;

        if (changed)
            SafeAreaChanged?.Invoke(null, EventArgs.Empty);
    }

    /// <summary>系统栏高度变化时触发（页面订阅此事件以更新 padding）</summary>
    public static event EventHandler? SafeAreaChanged;
}
