---
description: Randomly NT 已实现的随机数熵源
---

# ✨| Randomly NT 内置熵源

## 概述

Randomly NT 内部实现了一些易于获取的软件和硬件熵源。这些熵源可以在 Randomly NT 设置页面中通过调整随机化因子来应用这些熵源。

你可以在这里查阅 Randomly NT 熵源的相关实现代码: [https://github.com/Aunt-Studio/Randomly-NT/blob/7091d696a02f2276502434e90695941f686bf366/RandomDrawer.cs#L52-L106](https://github.com/Aunt-Studio/Randomly-NT/blob/7091d696a02f2276502434e90695941f686bf366/RandomDrawer.cs#L52-L106)

## 默认随机数种子

C# System 命名空间中的 Random 类提供的默认构造函数中使用的种子，由系统时间提供。

我们模拟了这一过程，实际上 Randomly NT 在构造随机数生成器时传入了额外获取的系统时钟，但实现效果与使用无参构造函数获得的 Random 实例是等效的。

系统时间的分辨率是有限的，短时间内可能获得到相同的种子值从而生成相同的随机数结果。对于一些特殊场景则不应仅使用此熵源。

### 参考

*   Microsoft Learn 文档：[https://learn.microsoft.com/en-us/dotnet/api/system.random.-ctor?view=net-9.0#system-random-ctor](https://learn.microsoft.com/en-us/dotnet/api/system.random.-ctor?view=net-9.0#system-random-ctor)


* System.Random() 源代码: [https://github.com/dotnet/runtime/blob/1d1bf92fcf43aa6981804dc53c5174445069c9e4/src/libraries/System.Private.CoreLib/src/System/Random.cs#L29C9-L33C102](https://github.com/dotnet/runtime/blob/1d1bf92fcf43aa6981804dc53c5174445069c9e4/src/libraries/System.Private.CoreLib/src/System/Random.cs#L29C9-L33C102)



## 运行时环境噪声

该熵源将在取随机数种子时获取各种可能无规律快速变化状态值，包括：

* 当前 CLR（公共语言运行时）认为已分配在托管堆上的内存总量
* 当前进程的工作集大小
* 当前托管线程 ID

### 参考

* System.GC.GetTotalMemory(Boolean) 方法: [https://learn.microsoft.com/zh-cn/dotnet/api/system.gc.gettotalmemory?view=net-9.0](https://learn.microsoft.com/zh-cn/dotnet/api/system.gc.gettotalmemory?view=net-9.0)
* System.Diagnostics.Process.WorkingSet64 属性: [https://learn.microsoft.com/zh-cn/dotnet/api/system.diagnostics.process.workingset64?view=net-9.0](https://learn.microsoft.com/zh-cn/dotnet/api/system.diagnostics.process.workingset64?view=net-9.0)
* System.Environment.CurrentManagedThreadId 属性: [https://learn.microsoft.com/zh-cn/dotnet/api/system.environment.currentmanagedthreadid?view=net-9.0](https://learn.microsoft.com/zh-cn/dotnet/api/system.environment.currentmanagedthreadid?view=net-9.0)



## 鼠标指针

当前鼠标指针的 X 与 Y 坐标。该数据通过 P/Invoke 从 `user32.dll` (`winuser.h`) 获得。

{% hint style="info" %}
如果无法获得该熵源，该值将会替换为系统时钟的 HashCode。
{% endhint %}

### 参考

* GetCursorPos 函数 (winuser.h): [https://learn.microsoft.com/zh-cn/windows/win32/api/winuser/nf-winuser-getcursorpos](https://learn.microsoft.com/zh-cn/windows/win32/api/winuser/nf-winuser-getcursorpos)



## 真随机数接口 (需要访问网络)

从 [Random.org](https://random.org/) 获得的 1 - 10000 的随机整数。

Random.org 提供了一个便捷的服务，使得互联网上的任何人可以轻松地获取到真随机数。事实上，Random.org 在几个国家架设了若干台无线电接收器来监测大气噪声以获取高熵的比特流作为随机数种子。他们的服务也已经经过了[eCOGRA](http://www.ecogra.org/)、[TST Global](http://www.tstglobal.com/)[Gaming Labs International](http://www.gaminglabs.com/) 等机构的评估或认可。

Randomly NT 将在每次抽取前，访问 Random.org 的公共 API，获取 1 - 10000 的随机整数并参与随机数种子运算。

{% hint style="info" %}
Random.org 对每个 IP 地址每天可以使用的随机数存在一定配额。具体请参见 [他们的FAQ](https://www.random.org/faq/#S4) 以及 [配额查询页面](https://www.random.org/quota/)。
{% endhint %}

### 参考

* Random.org: [https://www.random.org/](https://www.random.org/)

***

## 熵源处理

事实上，在 Randomly NT 从熵源获得值后不会直接将其作为种子传入生成器。

Randomly NT 在每次抽取随机数前初始化一个64位整数  `seed` 。程序获取到熵源后为每个获取到的值生成哈希代码 (除了默认随机数种子)，加入 `seed` 中。最后才将 `seed` 传入随机数生成器中。





