using Jint;

namespace CatClaw.Shared.Js;

/// <summary>
/// 猫爪音乐 / 猫爪影视共用的 JS 运行时服务基类（Jint）：
/// 宿主统一持有 Jint/Acornima（普通 NuGet 依赖，随宿主进 APK/应用目录，由默认 ALC 原生解析），
/// 单一版本来源 + 统一执行约束（递归上限 / 超时）。
/// <para>
/// 各 App 声明 <c>sealed class JsRuntimeService : JsRuntimeServiceBase, 本应用 IJsRuntimeService</c>
/// 并通过构造参数传入约束——插件 SDK 接口（IJsRuntimeService）保留在各宿主 Core 内不动，
/// 保证已发布插件的二进制兼容（类型标识不变）。
/// </para>
/// <para>
/// 实现细节：EnsureLoaded 与 Jint 类型引用分属两个方法体——宿主缺失 JS 运行时时，
/// JIT 编译 TouchAssemblies 抛出的 FileNotFoundException 会被 EnsureLoaded 捕获并
/// 转成明确的升级提示，而不是让调用方收到晦涩的程序集解析异常。
/// </para>
/// </summary>
public abstract class JsRuntimeServiceBase
{
    private readonly int _recursionLimit;
    private readonly TimeSpan _defaultTimeout;
    private readonly string _unavailableMessage;
    private int _ensured;

    /// <param name="recursionLimit">脚本递归深度上限（音乐 5000 / 影视 2000）</param>
    /// <param name="defaultTimeout">CreateEngine 未显式传超时时的默认单次执行超时</param>
    /// <param name="unavailableMessage">宿主缺失 Jint 时抛出的异常文案（含应用名与升级指引）</param>
    protected JsRuntimeServiceBase(int recursionLimit, TimeSpan defaultTimeout, string unavailableMessage)
    {
        _recursionLimit = recursionLimit;
        _defaultTimeout = defaultTimeout;
        _unavailableMessage = unavailableMessage;
    }

    /// <summary>确保 Jint/Acornima 程序集已加载进默认 ALC（幂等）。必须在首次引用
    /// Jint 类型的代码前调用；宿主缺少 JS 运行时时抛出带升级提示的异常。</summary>
    public void EnsureLoaded()
    {
        if (Interlocked.Exchange(ref _ensured, 1) == 1)
            return;
        try
        {
            TouchAssemblies();
        }
        catch (Exception ex)
        {
            Interlocked.Exchange(ref _ensured, 0);
            throw new InvalidOperationException(_unavailableMessage, ex);
        }
    }

    /// <summary>触碰 Jint/Acornima 类型强制程序集解析（本方法体是唯一引用 JS 类型的地方）</summary>
    private static void TouchAssemblies()
    {
        _ = typeof(Jint.Engine).Assembly;
        _ = typeof(Acornima.Parser).Assembly;
    }

    /// <summary>创建统一约束的 Jint Engine（递归上限 + 超时控制）。</summary>
    /// <param name="timeout">单次脚本执行超时（缺省用构造时传入的默认值）</param>
    public Jint.Engine CreateEngine(TimeSpan? timeout = null)
    {
        return new Jint.Engine(options => options
            .LimitRecursion(_recursionLimit)
            .TimeoutInterval(timeout ?? _defaultTimeout));
    }
}
