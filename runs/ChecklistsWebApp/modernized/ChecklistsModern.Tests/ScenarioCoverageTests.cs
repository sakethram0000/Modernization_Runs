using System.Reflection;
using System.Text.RegularExpressions;

namespace ChecklistsModern.Tests;

public class ScenarioCoverageTests
{
    [Fact(DisplayName = "Every Scenario title in backlog.md has a test with that title")]
    public void Every_scenario_has_a_test()
    {
        var backlog = File.ReadAllLines(Path.Combine(AppContext.BaseDirectory, "backlog.md"));
        var scenarios = backlog
            .Select(line => Regex.Match(line, @"^\s*Scenario:\s*(.+?)\s*$"))
            .Where(m => m.Success)
            .Select(m => m.Groups[1].Value)
            .ToList();

        var testTitles = typeof(ScenarioCoverageTests).Assembly.GetTypes()
            .SelectMany(t => t.GetMethods())
            .Select(m => m.GetCustomAttribute<FactAttribute>()?.DisplayName)
            .OfType<string>()
            .Select(name => name.Split(" | ", 2).Last())
            .ToHashSet();

        var missing = scenarios.Where(s => !testTitles.Contains(s)).ToList();

        Assert.True(scenarios.Count > 0, "No Scenario lines found in backlog.md");
        Assert.True(missing.Count == 0, "Scenarios with no test:\n" + string.Join("\n", missing));
    }
}
