---
description: 随机抽取一些整数。
---

# 🔢| 随机数字抽取

启动 Randomly NT。初次见面，你应该第一眼看到的就是这个功能。

<figure><img src="../.gitbook/assets/image.png" alt=""><figcaption></figcaption></figure>

## 参数设置区

<figure><img src="../.gitbook/assets/image (1).png" alt=""><figcaption></figcaption></figure>

如你所见，左侧的三个输入框以及一个切换按钮，即为设置随机数抽取参数的几个控件。

### 最小取值

顾名思义，该取值控制随机数可能取值的最小值。

### 最大取值

同理，该值控制随机数可能取值的最大值。

{% hint style="warning" %}
与一般的随机数抽取库/软件 不同，该值**有可能**被取到。

也就是说，随机数取值范围将是 \[最小取值, 最大取值] 。
{% endhint %}

### 抽取数量

该值控制程序应该抽取多少次随机数。

_在启用_ [_避免重复_ ](random-number.md#bi-mian-chong-fu)_的情况下_，该值不应超出可取随机数范围。

也&#x5373;_&#x542F;用_ [_避免重复_](random-number.md#bi-mian-chong-fu) _的情况下_ `抽取数量 < 最大取值 - 最小取值 + 1` 必须满足。

### 避免重复

<figure><img src="../.gitbook/assets/image (6).png" alt=""><figcaption><p>未激活的 避免重复</p></figcaption></figure>

<figure><img src="../.gitbook/assets/image (2).png" alt=""><figcaption><p>被点亮的 避免重复</p></figcaption></figure>

该按钮是一个切换按钮。按下该按钮即可切换避免重复功能的启用状态。

当按钮被点亮时，程序将避免抽取到相同的随机数。

{% hint style="warning" %}
在避免重复被激活时，必须有足够的随机数范围来抽取目标抽取数量。

在启用避免重复的情况下，必须满足 `抽取数量 < 最大取值 - 最小取值 + 1` 。
{% endhint %}

## 控制区

<figure><img src="../.gitbook/assets/image (12).png" alt=""><figcaption></figcaption></figure>

### 开始抽取

按下该按钮后，将按照目标设定抽取随机数。

### 复位

按下该按钮后，将清空[随机数结果区](random-number.md#sui-ji-shu-jie-guo-qu)，但**不会**移除您在参数设置区设置的参数及选项。

## 随机数结果区

<figure><img src="../.gitbook/assets/image (13).png" alt="" width="190"><figcaption></figcaption></figure>

按下[开始抽取](random-number.md#kai-shi-chou-qu)按钮，该区域将显示随机数抽取结果。

在空闲时 (如程序刚启动或[复位](random-number.md#fu-wei)后) , 该区域将不可见。



## 小技巧

### 编号的随机抽取

默认情况下 [避免重复](random-number.md#bi-mian-chong-fu) 功能处于关闭状态。在抽取数字时可能抽取到重复的数字。

您可以开启[避免重复](random-number.md#bi-mian-chong-fu)功能以使程序在抽取号数等场景下正常工作。

{% hint style="success" %}
在一些情况下，您还可以使用[随机姓名抽取](random-name.md)功能，抽取学生更简单明了而不需要过多的设置。
{% endhint %}

