using Randomly_NT.InstallerCore;

namespace Randomly_NT.Updater;

internal static class Program
{
    private const string DefaultAumid = "com.auntstudio.RandomlyNT_ebq4pdwzs4tag!App";
    private const string ExecutableName = "Updater.exe";

    private static int Main(string[] args)
    {
        Thread.Sleep(300);

        if (args.Length == 0)
        {
            args =
            [
                "auto",
                "--working-directory",
                Environment.CurrentDirectory
            ];
        }

        try
        {
            var options = InstallCommandLineOptions.Parse(args, DefaultAumid, Environment.CurrentDirectory, ExecutableName);
            InstallWorkflow.Execute(options);
            InstallWorkflow.LaunchMainApp(options.Aumid);
            Console.WriteLine("更新完成，按下回车键退出。");
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

            ShowExceptionDialog(ex);
            return 1;
        }
        catch (Exception ex)
        {
            ShowExceptionDialog(ex);
            return 1;
        }
    }

    private static void ShowExceptionDialog(Exception ex)
    {
        var originalColor = Console.ForegroundColor;
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"""
            ======== 尝试更新时发生异常 ========
            异常类型: {ex.GetType().Name}
            异常消息: {ex.Message}
            堆栈跟踪: {ex.StackTrace}
            ================================

            如有必要，请保留此消息并在
            https://github.com/Aunt-Studio/Randomly-NT/issues
            提交反馈。

            按回车键退出。
            """);
        Console.ForegroundColor = originalColor;
        Console.ReadLine();
    }
}
