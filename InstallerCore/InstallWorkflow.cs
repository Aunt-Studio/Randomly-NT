using System.Diagnostics;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace Randomly_NT.InstallerCore;

public enum InstallMode
{
    Auto,
    Script,
    Package
}

public sealed record InstallCommandLineOptions(
    InstallMode Mode,
    string WorkingDirectory,
    string Aumid,
    string? ScriptPath,
    string? CertificatePath,
    string? PackagePath,
    string? ExpectedPackageHash,
    bool VerifyPackageHash,
    bool PreferAppInstaller)
{
    public static bool IsHelpFlag(string arg)
    {
        return arg is "-h" or "--help" or "/?";
    }

    public static bool ShouldShowHelp(string[] args)
    {
        return args.Length == 1 && IsHelpFlag(args[0]);
    }

    public static InstallCommandLineOptions Parse(string[] args, string defaultAumid, string defaultWorkingDirectory, string executableName)
    {
        if (args.Length == 0 || ShouldShowHelp(args))
        {
            throw new InstallCommandLineException(BuildUsage(executableName)) { IsError = false };
        }

        var command = args[0];
        var options = ParseNamedOptions(args.Skip(1).ToArray(), executableName);

        var workingDirectory = options.TryGetValue("--working-directory", out var workingDir)
            ? Path.GetFullPath(workingDir)
            : Path.GetFullPath(defaultWorkingDirectory);
        var aumid = options.TryGetValue("--aumid", out var aumidValue) ? aumidValue : defaultAumid;

        return command switch
        {
            "auto" => new InstallCommandLineOptions(
                InstallMode.Auto,
                workingDirectory,
                aumid,
                null,
                null,
                null,
                null,
                VerifyPackageHash: false,
                PreferAppInstaller: true),

            "script" => new InstallCommandLineOptions(
                InstallMode.Script,
                workingDirectory,
                aumid,
                RequireOption(options, "--script", executableName),
                null,
                null,
                null,
                VerifyPackageHash: false,
                PreferAppInstaller: false),

            "package" => new InstallCommandLineOptions(
                InstallMode.Package,
                workingDirectory,
                aumid,
                null,
                RequireOption(options, "--cert", executableName),
                RequireOption(options, "--package", executableName),
                options.TryGetValue("--package-sha256", out var packageHash) ? packageHash : null,
                VerifyPackageHash: options.ContainsKey("--package-sha256"),
                PreferAppInstaller: options.ContainsKey("--prefer-appinstaller")),

            _ => throw new InstallCommandLineException($"未知命令: {command}\n\n{BuildUsage(executableName)}")
        };
    }

    public static string BuildUsage(string executableName)
    {
        return $"""
                用法:
                  {executableName} auto [--working-directory <目录>] [--aumid <AUMID>]
                  {executableName} script --script <Install.ps1> [--aumid <AUMID>]
                  {executableName} package --cert <证书.cer> --package <程序包.msix|.appinstaller> [--package-sha256 <SHA256>] [--aumid <AUMID>] [--prefer-appinstaller]
                """;
    }

    private static Dictionary<string, string> ParseNamedOptions(string[] args, string executableName)
    {
        var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        for (var i = 0; i < args.Length; i++)
        {
            var arg = args[i];
            if (!arg.StartsWith("--", StringComparison.Ordinal))
            {
                throw new InstallCommandLineException($"无法识别的参数: {arg}\n\n{BuildUsage(executableName)}");
            }

            if (i + 1 < args.Length && !args[i + 1].StartsWith("--", StringComparison.Ordinal))
            {
                result[arg] = args[i + 1];
                i++;
            }
            else
            {
                result[arg] = string.Empty;
            }
        }

        return result;
    }

    private static string RequireOption(Dictionary<string, string> options, string name, string executableName)
    {
        if (!options.TryGetValue(name, out var value) || string.IsNullOrWhiteSpace(value))
        {
            throw new InstallCommandLineException($"缺少必要参数 {name}。\n\n{BuildUsage(executableName)}");
        }

        return value;
    }
}

public static class InstallWorkflow
{
    public static void Execute(InstallCommandLineOptions options, TextWriter? writer = null)
    {
        var log = writer ?? Console.Out;
        log.WriteLine($"安装模式: {options.Mode}");

        switch (options.Mode)
        {
            case InstallMode.Auto:
                ExecuteAuto(options, log);
                break;
            case InstallMode.Script:
                InstallWithScript(options.ScriptPath!, log);
                break;
            case InstallMode.Package:
                InstallPackageMode(options, log);
                break;
            default:
                throw new InstallCommandLineException($"不支持的安装模式: {options.Mode}");
        }
    }

    public static void LaunchMainApp(string aumid, TextWriter? writer = null)
    {
        var log = writer ?? Console.Out;
        log.WriteLine($"安装已完成，尝试启动主程序 {aumid}");

        try
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = $"shell:AppsFolder\\{aumid}",
                UseShellExecute = true
            };

            Process.Start(startInfo);
            log.WriteLine("主程序启动成功。");
        }
        catch (Exception ex)
        {
            log.WriteLine("安装已经完成，但主程序启动失败。你可以在稍后手动启动。");
            log.WriteLine($"错误信息: {ex.Message}");
        }
    }

    private static void ExecuteAuto(InstallCommandLineOptions options, TextWriter log)
    {
        if (!Directory.Exists(options.WorkingDirectory))
        {
            throw new DirectoryNotFoundException($"找不到工作目录: {options.WorkingDirectory}");
        }

        var scriptPath = Path.Combine(options.WorkingDirectory, "Install.ps1");
        if (File.Exists(scriptPath))
        {
            log.WriteLine("匹配到 Install.ps1，优先执行安装脚本。");
            InstallWithScript(scriptPath, log);
            return;
        }

        var appInstallerPath = FindLatestFile(options.WorkingDirectory, "*.appinstaller");
        if (appInstallerPath is not null)
        {
            log.WriteLine($"匹配到 App Installer 文件: {appInstallerPath}");
            var certForAppInstaller = FindLatestFile(options.WorkingDirectory, "*.cer");
            if (certForAppInstaller is not null)
            {
                InstallCertificate(certForAppInstaller, log);
            }

            InstallByAppInstaller(appInstallerPath, log);
            return;
        }

        var packagePath = FindLatestFile(options.WorkingDirectory, "*.msix")
            ?? FindLatestFile(options.WorkingDirectory, "*.msixbundle")
            ?? FindLatestFile(options.WorkingDirectory, "*.appx")
            ?? FindLatestFile(options.WorkingDirectory, "*.appxbundle");
        if (packagePath is null)
        {
            throw new FileNotFoundException($"在目录 {options.WorkingDirectory} 中找不到可安装的 MSIX/AppX 包。");
        }

        var certPath = FindLatestFile(options.WorkingDirectory, "*.cer");
        if (certPath is null)
        {
            throw new FileNotFoundException($"在目录 {options.WorkingDirectory} 中找不到证书文件。");
        }

        InstallPackageMode(options with
        {
            PackagePath = packagePath,
            CertificatePath = certPath,
            VerifyPackageHash = false,
            ExpectedPackageHash = null
        }, log);
    }

    private static void InstallPackageMode(InstallCommandLineOptions options, TextWriter log)
    {
        var packagePath = EnsureExistingFile(options.PackagePath, "程序包");
        var certPath = EnsureExistingFile(options.CertificatePath, "证书");

        if (options.VerifyPackageHash)
        {
            VerifyFileHash(packagePath, options.ExpectedPackageHash!, "程序包", log);
        }

        InstallCertificate(certPath, log);

        if (options.PreferAppInstaller && string.Equals(Path.GetExtension(packagePath), ".appinstaller", StringComparison.OrdinalIgnoreCase))
        {
            InstallByAppInstaller(packagePath, log);
            return;
        }

        InstallPackage(packagePath, log);
    }

    private static string EnsureExistingFile(string? path, string label)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new InstallCommandLineException($"{label}路径不能为空。");
        }

        var fullPath = Path.GetFullPath(path);
        if (!File.Exists(fullPath))
        {
            throw new FileNotFoundException($"找不到指定的{label}文件: {fullPath}", fullPath);
        }

        return fullPath;
    }

    private static string? FindLatestFile(string directory, string pattern)
    {
        if (!Directory.Exists(directory))
        {
            throw new DirectoryNotFoundException($"找不到工作目录: {directory}");
        }

        var candidates = Directory.EnumerateFiles(directory, pattern, SearchOption.TopDirectoryOnly)
            .Select(path => new FileInfo(path))
            .OrderByDescending(info => info.LastWriteTimeUtc)
            .ThenBy(info => info.Name, StringComparer.OrdinalIgnoreCase)
            .Select(info => info.FullName);

        return candidates.FirstOrDefault();
    }

    private static void VerifyFileHash(string filePath, string expectedHash, string label, TextWriter log)
    {
        log.WriteLine($"尝试校验{label} SHA256...");
        var actualHash = ComputeSha256(filePath);
        log.WriteLine($"实际 SHA256: {actualHash}");

        if (!actualHash.Equals(expectedHash, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException($"{label} SHA256 校验失败。预期 {expectedHash}，实际 {actualHash}。");
        }

        log.WriteLine($"{label} SHA256 校验通过。");
    }

    private static string ComputeSha256(string filePath)
    {
        using var stream = File.OpenRead(filePath);
        using var sha256 = SHA256.Create();
        var hashBytes = sha256.ComputeHash(stream);
        return Convert.ToHexStringLower(hashBytes);
    }

    private static void InstallCertificate(string certPath, TextWriter log)
    {
        log.WriteLine("================================");
        log.WriteLine($"尝试安装位于 {certPath} 的公钥证书。");
        using var cert = X509CertificateLoader.LoadCertificateFromFile(certPath);
        using var store = new X509Store(StoreName.Root, StoreLocation.LocalMachine);
        store.Open(OpenFlags.ReadWrite);

        var existing = store.Certificates
            .Find(X509FindType.FindByThumbprint, cert.Thumbprint, validOnly: false)
            .OfType<X509Certificate2>()
            .Any(existingCert => string.Equals(existingCert.Thumbprint, cert.Thumbprint, StringComparison.OrdinalIgnoreCase));

        if (existing)
        {
            log.WriteLine("证书已存在于本地计算机受信任的根证书存储中，跳过导入。");
        }
        else
        {
            store.Add(cert);
            log.WriteLine("证书导入完成。");
        }

        log.WriteLine("================================");
    }

    private static void InstallPackage(string packagePath, TextWriter log)
    {
        log.WriteLine("================================");
        log.WriteLine($"尝试安装程序包: {packagePath}");
        var arguments = $"Add-AppxPackage -Path '{EscapePowerShellLiteral(packagePath)}' -ForceUpdateFromAnyVersion";
        InvokePowerShellCommand(arguments, "安装程序包", log);
        log.WriteLine("程序包安装完成。");
        log.WriteLine("================================");
    }

    private static void InstallByAppInstaller(string appInstallerPath, TextWriter log)
    {
        log.WriteLine("================================");
        log.WriteLine($"尝试通过 App Installer 工作流安装: {appInstallerPath}");
        var arguments = $"Add-AppxPackage -AppInstallerFile '{EscapePowerShellLiteral(appInstallerPath)}'";
        InvokePowerShellCommand(arguments, "安装 App Installer 文件", log);
        log.WriteLine("App Installer 安装完成。");
        log.WriteLine("================================");
    }

    private static void InstallWithScript(string scriptPath, TextWriter log)
    {
        log.WriteLine("================================");
        log.WriteLine($"尝试运行位于 {scriptPath} 的安装脚本。");
        InvokePowerShellFile(scriptPath, "PowerShell 脚本安装", log);
        log.WriteLine("脚本执行完成。");
        log.WriteLine("================================");
    }

    private static void InvokePowerShellCommand(string command, string operation, TextWriter log)
    {
        var finalCommand = $"Import-Module Appx; $ErrorActionPreference = 'Stop'; {command}";
        var result = RunPowerShell(
            $"-NoLogo -NoProfile -NonInteractive -ExecutionPolicy Bypass -Command \"{EscapeDoubleQuotedArgument(finalCommand)}\"",
            log);
        EnsureProcessSucceeded(result, operation);
    }

    private static void InvokePowerShellFile(string scriptPath, string operation, TextWriter log)
    {
        var result = RunPowerShell(
            $"-NoLogo -NoProfile -ExecutionPolicy RemoteSigned -File \"{scriptPath}\"",
            log);
        EnsureProcessSucceeded(result, operation);
    }

    private static ProcessResult RunPowerShell(string arguments, TextWriter log)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = "powershell.exe",
            Arguments = arguments,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = false
        };

        using var process = new Process { StartInfo = startInfo };
        process.Start();

        var output = process.StandardOutput.ReadToEnd();
        var error = process.StandardError.ReadToEnd();

        process.WaitForExit();

        if (!string.IsNullOrWhiteSpace(output))
        {
            log.WriteLine(output);
        }

        if (!string.IsNullOrWhiteSpace(error))
        {
            log.WriteLine(error);
        }

        return new ProcessResult(process.ExitCode, output, error);
    }

    private static void EnsureProcessSucceeded(ProcessResult result, string operation)
    {
        if (result.ExitCode == 0)
        {
            return;
        }

        throw new InvalidOperationException(
            $"{operation}失败，退出码 {result.ExitCode}。\n标准输出:\n{result.StandardOutput}\n标准错误:\n{result.StandardError}");
    }

    private static string EscapePowerShellLiteral(string value)
    {
        return value.Replace("'", "''", StringComparison.Ordinal);
    }

    private static string EscapeDoubleQuotedArgument(string value)
    {
        return value.Replace("`", "``", StringComparison.Ordinal).Replace("\"", "`\"", StringComparison.Ordinal);
    }

    private sealed record ProcessResult(int ExitCode, string StandardOutput, string StandardError);
}

public sealed class InstallCommandLineException : Exception
{
    public bool IsError { get; init; } = true;

    public InstallCommandLineException(string message)
        : base(message)
    {
    }
}
