namespace Hector.ArchitectureTests.Framework;

public sealed class ArchitectureRuleSet
{
    private readonly List<ArchitectureRule> _rules = [];

    public ArchitectureRuleSet Add(ArchitectureRule rule)
    {
        ArgumentNullException.ThrowIfNull(rule);
        _rules.Add(rule);
        return this;
    }

    public ArchitectureEvaluationReport Evaluate()
    {
        var violations = new List<ArchitectureViolation>();

        foreach (var rule in _rules.OrderBy(r => r.Id, StringComparer.Ordinal))
        {
            var result = rule.EvaluateWithResult();
            if (!result.HasViolations)
                continue;

            foreach (var diagnostic in result.Diagnostics.OrderBy(d => d, StringComparer.Ordinal))
            {
                violations.Add(new ArchitectureViolation(
                    rule.Id,
                    rule.Name,
                    rule.Reason,
                    Sanitize(diagnostic))); // اصلاح املای متد به Sanitize
            }
        }

        return new ArchitectureEvaluationReport(violations);
    }


    private static string Sanitize(string diagnostic)
    {
        if (string.IsNullOrEmpty(diagnostic))
        {
            return string.Empty;
        }

        // ۱. جایگزینی خطوط جدید برای یکپارچه‌سازی خروجی
        var sanitized = diagnostic
            .Replace(Environment.NewLine, " ", StringComparison.Ordinal)
            .Replace("\n", " ", StringComparison.Ordinal)
            .Replace("\r", " ", StringComparison.Ordinal);

        // ۲. ماسک کردن و پاک‌سازی ارجاع‌های سیستمی و Reflection internals جهت عدم افشا
        sanitized = sanitized.Replace("System.Reflection", "[Redacted System Details]", StringComparison.Ordinal);
        sanitized = sanitized.Replace("StackTrace", "[Redacted StackTrace]", StringComparison.Ordinal);

        // ۳. حذف عبارات شبیه به استک‌ترس (مانند " at Namespace.Class.Method")
        if (sanitized.Contains(" at ", StringComparison.Ordinal))
        {
            // با استفاده از یک جایگزینی ساده، الگوهای استک‌ترس شبیه‌سازی شده را حذف می‌کنیم
            sanitized = sanitized.Replace(" at ", " [Redacted Call] ", StringComparison.Ordinal);
        }

        return sanitized.Trim();
    }
}