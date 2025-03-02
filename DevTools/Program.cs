using System.Security.Cryptography;
using System.Text.Json;

namespace DevTools
{
    internal class Program
    {
        public record VersionMeta(string packageVer,
                                  string assemblyVer,
                                  string downloadUrl,
                                  string hash,
                                  string hasScript);

        private static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("""
                Randomly NT 开发者发行工具包
                -h <文件路径>  |  获取文件 Hash
                -v <打包路径>  |  生成 version.json
                """);
            }
            else if (args.Length == 2)
            {
                if (args[0] == "-h")
                {
                    if (File.Exists(args[1]))
                    {
                        string filePath = args[1];
                        string hash = CalculateSHA256(filePath);
                        Console.WriteLine($"SHA256 hash of {filePath}:\n{hash}");

                    }
                }
                else if (args[0] == "-v")
                {
                    if (!File.Exists(args[1]))
                    {
                        Console.WriteLine("找不到文件" + args[1]);
                        return;
                    }
                    Console.WriteLine("输入包版本号 (packageVer):");
                    var packageVer = Console.ReadLine();
                    Console.WriteLine("输入程序集版本号 (assemblyVer):");
                    var assemblyVer = Console.ReadLine();
                    Console.WriteLine("输入下载地址:");
                    var downloadUrl = Console.ReadLine();
                    Console.WriteLine("包含 Install.ps1 ? (y/n)");
                    var hasScriptInput = Console.ReadLine();

                    if (!string.IsNullOrWhiteSpace(packageVer)
                        && !string.IsNullOrWhiteSpace(assemblyVer)
                        && !string.IsNullOrWhiteSpace(downloadUrl)
                        && !string.IsNullOrWhiteSpace(hasScriptInput))
                    {
                        var hasScript = "true";
                        if (hasScriptInput == "n" || hasScriptInput == "N")
                        {
                            hasScript = "false";
                        }
                        var hash = CalculateSHA256(args[1]);
                        VersionMeta versionMeta = new VersionMeta(packageVer, assemblyVer, downloadUrl, hash, hasScript);
                        string json = JsonSerializer.Serialize(versionMeta, new JsonSerializerOptions
                        {
                            WriteIndented = true,
                        });
                        Console.WriteLine("\n\n");
                        Console.WriteLine(json);
                    }
                }
            }

        }
        public static string CalculateSHA256(string filePath)
        {
            using (FileStream stream = File.OpenRead(filePath))
            {
                SHA256 sha256 = SHA256.Create();
                byte[] hashBytes = sha256.ComputeHash(stream);
                return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
            }
        }
    }

}
