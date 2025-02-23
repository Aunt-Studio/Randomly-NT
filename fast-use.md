---
description: 放轻松。本页面将带着你从安装到使用。
---

# ➡️| 快速入门



## 方式一 (推荐) : 从 GitHub 下载 Release 程序包并安装

### 1. 获得最新 Release 程序包

#### 1.1 打开该项目最近版本的 GitHub Release 页面。

{% embed url="https://github.com/Aunt-Studio/Randomly-NT/releases/latest" %}

#### 1.2 下载软件包

如果您位于中国大陆，可以下载 Release Note 中提供的镜像包 (并非所有版本都有)。

如果您的网络可以正常连接 GitHub ，您可以直接单击 Assets 内的 `Randomly-NT_*.*.*.*.zip` 下载软件包。

### 2. 从脚本安装软件包

#### 2.1 解压

使用任意解压软件解压软件包中的所有文件到某个您记得住的目录。

2.2 允许 PowerShell 本地脚本运行

使用管理员身份打开 PowerShell 终端，输入指令:

```powershell
Set-ExecutionPolicy RemoteSigned
```

按下回车运行。

#### 2.3 运行安装脚本

找到您解压软件包的目录，在该目录中应该存在一个脚本 `Install.ps1` 。

右键单击该脚本，选择 "使用 PowerShell 运行"。

如在安装过程中提示需要提升 UAC 权限来安装证书，请允许。

### 3. 运行

如果安装成功，你应该能在 Windows 开始菜单中，找到 Randomly NT。

点击即可运行。

