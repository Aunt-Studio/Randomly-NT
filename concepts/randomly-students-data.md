---
description: Randomly Students Data
---

# 📃| Randomly 学生列表数据

## 概述

Randomly 学生列表数据用于存储在 Randomly 应用针对教育行业设计的功能所需的学生数据。

现阶段，该数据仅用于存储一系列学生姓名。



## 格式

Randomly 学生列表数据的一般实现是一段 Json 文本，存储在后缀名为 `.rsd` 的文件中。

该 Json 包含以下内容：

* 标识文件版本信息的 `version`
* 存储学生列表的数组 `students`

例如，这是一段标准的 Randomly 学生列表数据，文件版本为 1.0：

{% code title="StudentsData.rsd" fullWidth="false" %}
```json
{
  "version": "1.0",
  "students": [
    "王伟", "李娜", "张强", "刘芳", "陈伟", "杨秀英", "黄静", "赵敏", "周杰", "吴磊",
    "徐丽", "孙浩", "马婷", "朱军", "胡雪", "郭涛", "何敏", "高飞", "林琳", "郑凯",
    "谢娜", "韩梅梅", "曹阳", "邓超", "许晴", "冯坤", "曾华", "程龙", "蔡琴", "彭宇",
    "潘虹", "田雨", "董洁", "袁泉", "于娜", "蒋欣", "钟汉良", "苏有朋", "魏晨", "吕燕",
    "丁真", "任嘉伦", "卢靖姗", "方文山", "孔雪儿", "崔健", "康辉", "史铁生", "侯勇",
    "龙丹妮", "万茜", "段奕宏", "雷佳音", "白敬亭", "夏雨", "江疏影", "邱淑贞", "秦岚"
  ]
}
```
{% endcode %}

你可以在[此链接](https://drive.auntstudio.com/local0/%E6%B5%8B%E8%AF%95%E5%AD%A6%E7%94%9F%E5%88%97%E8%A1%A8%E6%95%B0%E6%8D%AE.rsd)中下载到本示例。



## 实现

你可以使用 Randomly NT 自带的[学生列表数据编辑器](../features/rsd-editor.md)来编辑文件版本为 1.0 的任意标准 Randomly 学生列表数据。

当然，只要满足格式，你也可以实现自己的学生列表数据编辑器。

你可以参考 [Randomly NT 学生列表数据编辑页面的相关代码](https://github.com/Aunt-Studio/Randomly-NT/blob/7091d696a02f2276502434e90695941f686bf366/Pages/StudentsDataEditorPage.xaml.cs#L214-L268)，来制作自己的实现。

