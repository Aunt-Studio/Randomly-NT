---
description: 从学生名单里，随机抽取一些姓名。
---

# 👥| 随机姓名抽取

<figure><img src="../.gitbook/assets/image (11).png" alt=""><figcaption></figcaption></figure>

## 待抽取学生名单

<figure><img src="../.gitbook/assets/image (14).png" alt=""><figcaption></figcaption></figure>

### 学生名单

该列表展示所有待抽取学生的名单。

<figure><img src="../.gitbook/assets/image (18).png" alt="" width="120"><figcaption></figcaption></figure>

你可以按住 <kbd>Ctrl</kbd> 或 <kbd>Shift</kbd> 按键，像多选文件一样多选名称，然后从选定的姓名中抽取。

### 学生名单编辑区

<figure><img src="../.gitbook/assets/image (19).png" alt=""><figcaption></figcaption></figure>

#### 导入

按下导入按钮，程序将打开一个文件选择器。请在文件选择器中，导入您的学生列表数据 (.rsd 文件) 。

有关学生列表数据 (.rsd) 文件的更多信息，请参阅学生列表数据编辑器。

#### 编辑

按下该按钮将启动学生列表数据编辑器。

如果当前已经载入了某个学生列表数据文件，编辑器将默认载入该文件。

{% hint style="info" %}
如果您第一次使用或尚未为您的班级创建学生列表数据文件，请按下本按钮打开学生列表数据编辑器，然后参阅学生列表数据编辑器手册创建您的学生列表数据。
{% endhint %}

{% hint style="info" %}
如果您想体验该功能，您单击此链接下载 [测试学生列表数据](https://drive.auntstudio.com/local0/%E6%B5%8B%E8%AF%95%E5%AD%A6%E7%94%9F%E5%88%97%E8%A1%A8%E6%95%B0%E6%8D%AE.rsd)。
{% endhint %}

## 抽取控制

<figure><img src="../.gitbook/assets/image (21).png" alt=""><figcaption></figcaption></figure>

### 抽取数

该控件控制程序每次抽取多少个姓名。

{% hint style="warning" %}
抽取姓名功能默认不会抽到相同的姓名。

因此您必须确保学生名单总数足以抽取目标数量的姓名。
{% endhint %}

{% hint style="warning" %}
在使用[从选定中抽选](random-name.md#cong-xuan-ding-zhong-chou-xuan)功能时，您也必须确保您选定的学生足以抽取目标数量的姓名。
{% endhint %}

### 重新抽取

该按钮将清空原有抽取结果，并重新抽取目标抽取数量的姓名。

### 从选定中抽选

{% hint style="info" %}
该按钮仅在学生名单列表中选取了两个及以上姓名时才显示。
{% endhint %}

该按钮将在学生名单列表中已选取的学生中抽取。

例如：我现在选择了若干个姓名：

<figure><img src="../.gitbook/assets/image (8).png" alt=""><figcaption></figcaption></figure>

按下该按钮后，抽取结果只会在选取的几个名字中取得：

<figure><img src="../.gitbook/assets/image (9).png" alt=""><figcaption></figcaption></figure>

## 抽取结果

<figure><img src="../.gitbook/assets/image (10).png" alt=""><figcaption></figcaption></figure>

该区域展示抽取后的姓名。
