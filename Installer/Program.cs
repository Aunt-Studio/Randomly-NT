using Randomly_NT.InstallerCore;
using System.Reflection;

namespace Randomly_NT.Installer;

internal static class Program
{
    private const string DefaultAumid = "com.auntstudio.RandomlyNT_ebq4pdwzs4tag!App";
    private const string ExecutableName = "Installer.exe";
    private const string PayloadResourcePrefix = "Payload/";

    private static int Main(string[] args)
    {
        Thread.Sleep(300);
        string? extractedPayloadDirectory = null;
        bool installCompleted = false;

        try
        {
            extractedPayloadDirectory = ExtractEmbeddedPayloadIfPresent();
            var defaultWorkingDirectory = extractedPayloadDirectory ?? Path.GetFullPath(AppContext.BaseDirectory);
            var options = BuildOptions(args, defaultWorkingDirectory);
            InstallWorkflow.Execute(options);
            installCompleted = true;
            TryDeleteTemporaryPayload(extractedPayloadDirectory);
            InstallWorkflow.LaunchMainApp(options.Aumid);
            Console.WriteLine("安装完成，按下回车键退出。");
            Console.ReadLine();
            return 0;
        }
        catch (InstallCommandLineException ex)
        {
            if (!ex.IsError)
            {
                Console.WriteLine(ex.Message);
                return 0;
            }

            ShowExceptionDialog(ex, extractedPayloadDirectory, installCompleted);
            return 1;
        }
        catch (Exception ex)
        {
            ShowExceptionDialog(ex, extractedPayloadDirectory, installCompleted);
            return 1;
        }
    }

    private static InstallCommandLineOptions BuildOptions(string[] args, string defaultWorkingDirectory)
    {
        if (args.Length == 0)
        {
            return new InstallCommandLineOptions(
                InstallMode.Auto,
                defaultWorkingDirectory,
                DefaultAumid,
                null,
                null,
                null,
                null,
                VerifyPackageHash: false,
                PreferAppInstaller: true);
        }

        return InstallCommandLineOptions.Parse(args, DefaultAumid, defaultWorkingDirectory, ExecutableName);
    }

    private static string? ExtractEmbeddedPayloadIfPresent()
    {
        var assembly = Assembly.GetExecutingAssembly();
        var resources = assembly.GetManifestResourceNames()
            .Where(name => name.StartsWith(PayloadResourcePrefix, StringComparison.Ordinal))
            .OrderBy(name => name, StringComparer.Ordinal)
            .ToArray();

        if (resources.Length == 0)
        {
            return null;
        }

        var targetDirectory = Path.Combine(
            Path.GetTempPath(),
            "Randomly-NT-Installer",
            DateTime.UtcNow.ToString("yyyyMMddHHmmss"),
            Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(targetDirectory);

        Console.WriteLine($"检测到内嵌安装载荷，正在提取到临时目录: {targetDirectory}");

        foreach (var resourceName in resources)
        {
            var relativePath = resourceName[PayloadResourcePrefix.Length..]
                .Replace('/', Path.DirectorySeparatorChar)
                .Replace('\\', Path.DirectorySeparatorChar);
            var outputPath = Path.Combine(targetDirectory, relativePath);
            var outputDirectory = Path.GetDirectoryName(outputPath);
            if (!string.IsNullOrWhiteSpace(outputDirectory))
            {
                Directory.CreateDirectory(outputDirectory);
            }

            using var resourceStream = assembly.GetManifestResourceStream(resourceName)
                ?? throw new InvalidOperationException($"无法读取内嵌资源: {resourceName}");
            using var fileStream = File.Create(outputPath);
            resourceStream.CopyTo(fileStream);
        }

        return targetDirectory;
    }

    private static void TryDeleteTemporaryPayload(string? extractedPayloadDirectory)
    {
        if (string.IsNullOrWhiteSpace(extractedPayloadDirectory) || !Directory.Exists(extractedPayloadDirectory))
        {
            return;
        }

        try
        {
            Directory.Delete(extractedPayloadDirectory, recursive: true);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"安装完成后未能删除临时载荷目录，可忽略: {extractedPayloadDirectory}");
            Console.WriteLine($"清理错误: {ex.Message}");
        }
    }

    private static void ShowExceptionDialog(Exception ex, string? extractedPayloadDirectory, bool installCompleted)
    {
        var originalColor = Console.ForegroundColor;
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"""
            ======== 尝试安装时发生异常 ========
            异常类型: {ex.GetType().Name}
            异常消息: {ex.Message}
            堆栈跟踪: {ex.StackTrace}
            =================================

            如有必要，请保留此消息并在
            https://github.com/Aunt-Studio/Randomly-NT/issues
            提交反馈。

            {(string.IsNullOrWhiteSpace(extractedPayloadDirectory) || installCompleted ? string.Empty : $"临时载荷目录: {extractedPayloadDirectory}\n")}
            按回车键退出。
            """);
        Console.ForegroundColor = originalColor;
        Console.ReadLine();
    }
}
