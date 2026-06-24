using FluentAssertions;
using System.Diagnostics;

namespace Hector.TemplateTests.TemplateGeneration;

public class TemplateGenerationTests
{
    private static readonly TemplateScenario[] Scenarios =
    [
        new("AppDefault", []),
        new("AppNoSample", ["--sampleModule", "false"]),
        new("AppWithSample", ["--sampleModule", "true"])
    ];

    [Fact(Skip = "Temporarily disabled")]
    public async Task Template_All_Scenarios_Should_Restore_Build_And_Test()
    {
        // Arrange
        var workingDir = CreateShortWorkingDirectory();

        var templateRoot = Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory,
            "../../../../../../"));

        templateRoot = Path.TrimEndingDirectorySeparator(templateRoot);

        await TryRun(
            workingDir,
            "dotnet",
            new[]
            {
                "new",
                "uninstall",
                templateRoot
            });

        await Run(
            workingDir,
            "dotnet",
            new[]
            {
                "new",
                "install",
                templateRoot,
                "--force"
            });

        foreach (var scenario in Scenarios)
        {
            var scenarioWorkingDir = CreateShortWorkingDirectory();

            var projectDir = Path.Combine(scenarioWorkingDir, scenario.Name);
            var solutionPath = Path.Combine(projectDir, $"{scenario.Name}.slnx");

            // Act
            var newArgs = new[]
            {
                "new",
                "hectorddd",
                "-n",
                scenario.Name
            }.Concat(scenario.Args);

            await Run(
                scenarioWorkingDir,
                "dotnet",
                newArgs);

            // Assert
            Directory
                .EnumerateDirectories(projectDir, "obj", SearchOption.AllDirectories)
                .Should()
                .BeEmpty("template output must not contain obj directories");

            Directory
                .EnumerateDirectories(projectDir, "bin", SearchOption.AllDirectories)
                .Should()
                .BeEmpty("template output must not contain bin directories");

            File.Exists(solutionPath).Should().BeTrue();

            await Run(
                projectDir,
                "dotnet",
                new[]
                {
                    "restore",
                    solutionPath
                });

            await Run(
                projectDir,
                "dotnet",
                new[]
                {
                    "build",
                    solutionPath,
                    "--no-restore",
                    "-p:DisableFastUpToDateCheck=true"
                });

            await Run(
                projectDir,
                "dotnet",
                new[]
                {
                    "test",
                    solutionPath,
                    "--no-build",
                    "--no-restore"
                });

            Directory.Exists(projectDir).Should().BeTrue();
        }
    }

    private static string CreateShortWorkingDirectory()
    {
        var root = OperatingSystem.IsWindows()
            ? @"C:\ht"
            : Path.Combine(Path.GetTempPath(), "ht");

        var workingDir = Path.Combine(
            root,
            Guid.NewGuid().ToString("N")[..8]);

        Directory.CreateDirectory(workingDir);

        return workingDir;
    }

    private static async Task Run(
        string workingDir,
        string fileName,
        IEnumerable<string> arguments)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = fileName,
            WorkingDirectory = workingDir,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false
        };

        foreach (var argument in arguments)
        {
            startInfo.ArgumentList.Add(argument);
        }

        using var process = new Process
        {
            StartInfo = startInfo
        };

        process.Start();

        var stdout = await process.StandardOutput.ReadToEndAsync();
        var stderr = await process.StandardError.ReadToEndAsync();

        await process.WaitForExitAsync();

        process.ExitCode.Should().Be(
            0,
            because: $"""
Command failed.

Working Directory:
{workingDir}

Command:
{fileName} {string.Join(" ", arguments)}

STDOUT:
{stdout}

STDERR:
{stderr}
"""
        );
    }

    private static async Task TryRun(
        string workingDir,
        string fileName,
        IEnumerable<string> arguments)
    {
        try
        {
            await Run(workingDir, fileName, arguments);
        }
        catch
        {
        }
    }

    private sealed record TemplateScenario(string Name, string[] Args);
}
