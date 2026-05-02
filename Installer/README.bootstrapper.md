单文件 Bootstrapper 用法：

1. 将需要随 Installer.exe 一起内嵌分发的文件放入 `Installer/Payload/`。
2. 支持的自动识别载荷与 `InstallWorkflow` 一致：
   - `Install.ps1`
   - `*.appinstaller`
   - `*.msix`
   - `*.msixbundle`
   - `*.appx`
   - `*.appxbundle`
   - `*.cer`
3. 发布单文件安装器示例：

```powershell
dotnet publish .\Installer\Installer.csproj -c Release -r win-x64
```

4. 发布结果中的 `Installer.exe` 为单文件 Bootstrapper。运行后会将内嵌载荷解压到临时目录，再自动执行安装流程。

建议：
- 如果目标是首次安装，优先把 `msix/appinstaller + cer` 都放进 `Payload`。
- 如果有自定义安装脚本，也可以直接放入 `Payload\Install.ps1`，程序会优先执行它。
