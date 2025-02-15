using System.Diagnostics;
using System.Security.Cryptography.X509Certificates;
using System.Security;
using System.Security.Cryptography;
using System.Management.Automation;
using static System.Runtime.InteropServices.JavaScript.JSType;


namespace Randomly_NT.Updater
{
    internal class Program
    {
        /*
         传入 args 规范
            args[0]: 是否包含安装PS脚本 - true: 包含 ; false: 不包含
            -
            <如果包含安装脚本>
            args[1]: 脚本路径
            args[2]: 主程序路径
            -
            <如果不包含安装脚本>
            args[1]: 证书路径
            args[2]: 包路径
            args[3]: 包哈希
            args[4]: 主程序路径
         */
        static void Main(string[] args)
        {
            string appPath = string.Empty;
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
                    appPath = args[2];
                }
                else if (args.Length != 3)
                {
                    throw new ArgumentException($"预期外的参数数量: {args.Length}");
                }
                // 检查参数结束

                if (usingScript)
                {
                    InstallWithScript(scriptPath);
                }
                else
                {

                    var certPath = args[1];
                    var packagePath = args[2];
                    var expectedHash = args[3];
                    appPath = args[4];

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
                LaunchMainApp(appPath);
                Console.WriteLine("更新完成，按下回车键退出。");
                Console.ReadLine();
            }
            catch (Exception ex)
            {
                ShowExceptionDialog(ex, usingScript);
                Environment.Exit(1);
            }

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
                .AddParameter("-Path", packagePath)
                .AddParameter("-ForceUpdateFromAnyVersion");
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
            return actualHash.Equals(expectedHash, StringComparison.CurrentCultureIgnoreCase);
        }


        private static void InstallWithScript(string scriptPath)
        {
            Console.WriteLine("================================");
            Console.WriteLine($"尝试运行位于 {scriptPath} 的安装脚本。");
            // 读取脚本内容
            string scriptContent = File.ReadAllText(scriptPath);
            using PowerShell ps = PowerShell.Create();
            ps.AddScript(scriptContent);
            var result = ps.Invoke();
            Console.WriteLine(ps.Streams);
            // 检查是否有错误
            if (ps.HadErrors)
            {
                throw new Exception($"PowerShell 脚本安装运行错误: {ps.Streams.Error}");
            }
            else
            {
                Console.WriteLine("完成。");
            }
            Console.WriteLine("================================");
        }
        private static void LaunchMainApp(string appPath)
        {
            Console.WriteLine($"更新已完成，尝试启动主程序{appPath}");
            if (Process.Start(appPath) is null)
            {
                Console.WriteLine("更新已经完成，但主程序启动失败。你可以在稍后手动启动。");
            }
        }
    }
}
