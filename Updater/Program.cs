using System.Diagnostics;
using System.Security.Cryptography.X509Certificates;
using System.Security;
using System.Security.Cryptography;
using System.Management.Automation;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Drawing.Text;
using System.Management.Automation.Runspaces;


namespace Randomly_NT.Updater
{
    internal class Program
    {
        private const string DefaultCertHash = "A1B2C3D4E5F6";
        /*
         传入 args 规范
            -AF 选项: 从当前位置自动匹配安装（推荐）
            -
            args[0]: 是否包含安装PS脚本 - true: 包含 ; false: 不包含
            -
            <如果包含安装脚本>
            args[1]: 脚本路径
            args[2]: 主程序 AUMID
            -
            <如果不包含安装脚本>
            args[1]: 证书路径
            args[2]: 包路径
            args[3]: 包哈希
            args[4]: 主程序 AUMID
         */
        static void Main(string[] args)
        {
            // 等待300ms ，确保主程序已经关闭
            Thread.Sleep(300);
            bool autoFetch = false;
            if (args.Length == 1 && args[0] == "-AF")
            {
                autoFetch = true;
                Console.WriteLine("自动匹配安装...");
            }
                
            string AUMID = "com.auntstudio.RandomlyNT_ebq4pdwzs4tag!App";
            bool usingScript = false;
            try
            {

                string scriptPath = string.Empty;
                if (args.Length == 2)
                {
                    if (args[0] == "true")
                    {
                        usingScript = true;
                        scriptPath = args[1];
                    }
                    else if (args[0] == "false")
                    {
                        usingScript = false;
                    }
                    else
                    {
                        throw new ArgumentException("预期外的参数[0]: " + args[0]);
                    }
                    AUMID = args[2];
                }
                else if (!autoFetch && args.Length != 3)
                {
                    throw new ArgumentException($"预期外的参数数量: {args.Length}");
                }
                // 检查参数结束
                if (autoFetch)
                {
                    scriptPath = Path.Combine(Environment.CurrentDirectory, "Install.ps1");
                    if (File.Exists(scriptPath))
                    {
                        usingScript = true;
                        Console.WriteLine("匹配到安装脚本，尝试运行...");
                        InstallWithScript(scriptPath);
                    }
                    else
                    {
                        var certPath = Directory.GetFiles(Environment.CurrentDirectory, "Randomly-NT_*.*.*.*_x64.cer").FirstOrDefault();
                        var packagePath = Directory.GetFiles(Environment.CurrentDirectory, "Randomly-NT_*.*.*.*_x64.msix").FirstOrDefault();
                        if (certPath is null)
                        {
                            throw new FileNotFoundException("找不到证书文件。");
                        }
                        if (packagePath is null)
                        {
                            throw new FileNotFoundException("找不到程序包文件。");
                        }
                        Install(certPath, packagePath, DefaultCertHash);
                    }
                }
                else if (usingScript)
                {
                    InstallWithScript(scriptPath);
                }
                else
                {

                    var certPath = args[1];
                    var packagePath = args[2];
                    var expectedHash = args[3];
                    AUMID = args[4];

                    Install(certPath, packagePath, expectedHash);
                }
                LaunchMainApp(AUMID);
                Console.WriteLine("更新完成，按下回车键退出。");
                Console.ReadLine();
            }
            catch (Exception ex)
            {
                ShowExceptionDialog(ex, usingScript);
                Environment.Exit(1);
            }

        }

        private static void Install(string certPath, string packagePath, string expectedHash)
        {
            Console.WriteLine("尝试校验证书...");
            if (!VerifyCertificateHash(certPath, expectedHash))
            {
                throw new SecurityException("证书 Hash 校验失败，请检查证书是否完整、篡改。");
            }
            else
            {
                Console.WriteLine("成功校验证书。");
            }

            InstallCertificate(certPath);
            InstallPackage(packagePath);
        }

        private static void ShowExceptionDialog(Exception ex, bool usingScript)
        {
            // Show error dialog
            var originalColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"""
                ======== 尝试更新时发生异常 ========
                异常类型: {ex.GetType().Name}
                异常消息: {ex.Message}
                堆栈跟踪: {ex.StackTrace}
                使用脚本: {(usingScript ? "是" : "否")}
                ================================

                如有必要，请保留此消息并在
                https://github.com/Aunt-Studio/Randomly-NT/issues
                提交反馈。

                按回车键退出。
                """);
            Console.ForegroundColor = originalColor;
            Console.ReadLine();
        }

        private static void InstallCertificate(string certPath)
        {
            Console.WriteLine("================================");
            Console.WriteLine($"尝试安装位于 {certPath} 的公钥证书。");
            using var cert = X509CertificateLoader.LoadCertificateFromFile(certPath);
            using var store = new X509Store(StoreName.Root, StoreLocation.LocalMachine);
            store.Open(OpenFlags.ReadWrite);
            store.Add(cert);
            Console.WriteLine("完成。");
            Console.WriteLine("================================");
        }

        private static void InstallPackage(string packagePath)
        {
            Console.WriteLine("================================");
            Console.WriteLine($"尝试安装位于 {packagePath} 的程序包。\n");
            using PowerShell ps = PowerShell.Create();
            ps.AddCommand("Add-AppxPackage")
                .AddParameter("Path", packagePath)
                .AddParameter("ForceUpdateFromAnyVersion");
            var result = ps.Invoke();
            Console.WriteLine(ps.Streams);
            if (ps.HadErrors)
            {
                throw new Exception($"安装程序包时 PowerShell 报告错误:\n{ps.Streams.Error}");
            }
            else
            {
                Console.WriteLine("完成。");
            }
            Console.WriteLine("================================");
        }


        private static bool VerifyCertificateHash(string certPath, string expectedHash)
        {
            using var cert = X509CertificateLoader.LoadCertificateFromFile(certPath);
            var actualHash = BitConverter.ToString(cert.GetCertHash(HashAlgorithmName.SHA256))
                              .Replace("-", "").ToLower();
            Console.WriteLine(actualHash);
            return actualHash.Equals(expectedHash, StringComparison.CurrentCultureIgnoreCase);
        }


        private static void InstallWithScript(string scriptPath)
        {
            Console.WriteLine("================================");
            Console.WriteLine($"尝试运行位于 {scriptPath} 的安装脚本。");
            // 使用 Process 运行脚本
            try
            {
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = "powershell.exe",
                    Arguments = $"-ExecutionPolicy RemoteSigned -File \"{scriptPath}\"",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = false
                };

                using Process process = new Process { StartInfo = startInfo };
                process.Start();

                string output = process.StandardOutput.ReadToEnd();
                string error = process.StandardError.ReadToEnd();

                process.WaitForExit();

                Console.WriteLine(output);

                if (process.ExitCode != 0)
                {
                    Console.WriteLine($"错误: {error}");
                    throw new Exception($"PowerShell 脚本安装运行错误: {error}");
                }
                else
                {
                    Console.WriteLine("完成。");
                }
            }
            catch (ArgumentNullException anex)
            {
                var originalColor = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Magenta;
                Console.WriteLine($"参数{anex.ParamName}为null。");
                Console.WriteLine($"工作目录: {Environment.CurrentDirectory}");
                Console.WriteLine($"AppDomain基目录: {AppDomain.CurrentDomain.BaseDirectory}");
                Console.ForegroundColor = originalColor;
                throw;
            }

            Console.WriteLine("================================");
        }
        private static void LaunchMainApp(string AUMID)
        {
            Console.WriteLine($"更新已完成，尝试启动主程序{AUMID}");
            try
            {
                using PowerShell ps = PowerShell.Create();
                ps.AddCommand("Start-Process")
                  .AddArgument($"shell:AppsFolder\\{AUMID}");
                var result = ps.Invoke();
                if (ps.HadErrors)
                {
                    throw new Exception($"启动程序时 PowerShell 报告错误:\n{ps.Streams.Error}");
                }
                else
                {
                    Console.WriteLine("主程序启动成功。");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("更新已经完成，但主程序启动失败。你可以在稍后手动启动。");
                Console.WriteLine($"错误信息: {ex.Message}");
            }
        }
    }
}
