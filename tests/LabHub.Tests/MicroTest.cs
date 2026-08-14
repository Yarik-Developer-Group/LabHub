using System.Reflection;

namespace LabHub.Tests;

/// <summary>Помечает метод как тест для MicroTest-раннера.</summary>
[AttributeUsage(AttributeTargets.Method)]
public sealed class TestAttribute : Attribute;

/// <summary>Утверждения. Падение — исключение CheckFailedException.</summary>
public static class Check
{
    public static void True(bool condition, string what)
    {
        if (!condition) throw new CheckFailedException($"Expected true: {what}");
    }

    public static void False(bool condition, string what)
    {
        if (condition) throw new CheckFailedException($"Expected false: {what}");
    }

    public static void Equal<T>(T expected, T actual)
    {
        if (!EqualityComparer<T>.Default.Equals(expected, actual))
            throw new CheckFailedException($"Expected [{expected}], got [{actual}]");
    }

    public static void Contains(string fragment, string? text)
    {
        if (text is null || !text.Contains(fragment, StringComparison.Ordinal))
            throw new CheckFailedException($"Expected fragment [{fragment}] in [{Trim(text)}]");
    }

    public static void DoesNotContain(string fragment, string? text)
    {
        if (text is not null && text.Contains(fragment, StringComparison.Ordinal))
            throw new CheckFailedException($"Unexpected fragment [{fragment}] in [{Trim(text)}]");
    }

    public static void NotNull(object? value, string what)
    {
        if (value is null) throw new CheckFailedException($"Expected not null: {what}");
    }

    private static string Trim(string? text) =>
        text is null ? "<null>" : text.Length <= 300 ? text : text[..300] + "…";
}

public sealed class CheckFailedException : Exception
{
    public CheckFailedException(string message) : base(message)
    {
    }
}

/// <summary>Находит все [Test]-методы в сборке, выполняет и печатает отчёт.
/// Ненулевой код возврата при любом падении — годится как CI-гейт.</summary>
public static class TestRunner
{
    public static async Task<int> RunAllAsync(Assembly assembly)
    {
        var failures = 0;
        var total = 0;
        foreach (var type in assembly.GetTypes().OrderBy(t => t.FullName, StringComparer.Ordinal))
        {
            foreach (var method in FindTests(type))
            {
                total++;
                if (!await RunOneAsync(type, method)) failures++;
            }
        }
        Console.WriteLine($"\n{total - failures}/{total} passed, {failures} failed");
        return failures == 0 ? 0 : 1;
    }

    private static IEnumerable<MethodInfo> FindTests(Type type) =>
        type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .Where(m => m.GetCustomAttribute<TestAttribute>() is not null)
            .OrderBy(m => m.Name, StringComparer.Ordinal);

    private static async Task<bool> RunOneAsync(Type type, MethodInfo method)
    {
        var name = $"{type.Name}.{method.Name}";
        try
        {
            var instance = Activator.CreateInstance(type);
            var result = method.Invoke(instance, null);
            if (result is Task task) await task;
            Console.WriteLine($"  PASS {name}");
            return true;
        }
        catch (Exception ex)
        {
            var reason = ex is TargetInvocationException { InnerException: { } inner } ? inner : ex;
            Console.WriteLine($"  FAIL {name}\n       {reason.GetType().Name}: {reason.Message}");
            return false;
        }
    }
}
