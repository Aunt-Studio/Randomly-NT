# ✏️| 学生列表数据编辑器

<figure><img src="../.gitbook/assets/image (2).png" alt=""><figcaption></figcaption></figure>

## 学生列表编辑区

### 学生列表

该列表展示当前工作区域的所有学生姓名。

在第一次启动且没有加载任何学生数据文件时，该列表包含两条数据：

<figure><img src="../.gitbook/assets/image (3).png" alt=""><figcaption></figcaption></figure>

将鼠标放置在任意一条数据上，按下鼠标右键，可以在弹出的菜单中删除该数据。

{% hint style="info" %}
如果右键没反应可以多尝试几次。
{% endhint %}

### 姓名数据编辑与添加控制区

<figure><img src="../.gitbook/assets/image (5).png" alt=""><figcaption></figcaption></figure>

#### 添加姓名到末尾

该按钮将追加左侧输入框中的学生姓名到工作区域学生列表的末尾。

#### 编辑姓名

单击任意姓名，你应该能看到底部的编辑框。在编辑按钮左侧你可以对当前选中的姓名进行修改。按下编辑按钮将改动应用到工作区域学生列表。

## 文件处理区

<figure><img src="../.gitbook/assets/image (6).png" alt=""><figcaption></figcaption></figure>

### 保存到文件

该按钮将保存当前工作区域学生列表到一个[学生列表数据 (.rsd)](rsd-editor.md#xue-sheng-lie-biao-shu-ju-randomly-students-data) 文件中。

### 载入文件

按下该按钮，选择已有的[学生列表数据 (.rsd)](rsd-editor.md#xue-sheng-lie-biao-shu-ju-randomly-students-data) 文件以编辑该文件。

{% hint style="danger" %}
载入另一个文件将直接清空当前工作区域的所有内容。

载入前请务必保存当前工作区域。
{% endhint %}

***



## 学生列表数据 | Randomly Students Data

学生列表数据是一类以 `.rsd` 为后缀的文件。

当前文件版本 (1.0) 仅支持存储学生列表。

学生列表数据实质上是包含一个名为 `version` 的 _key_ 及其对应的 _value_ 来标识文件版本以及一个名为 `students` 的数组来存储学生列表。因而在此基础上，您可以自行开发学生列表数据编辑器。
