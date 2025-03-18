using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Randomly_NT.ClassMode;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Randomly_NT
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ClassPreparePage : Page
    {
        public string ClassFilePath { get; set; } = string.Empty;
        private StudentSelector? selector = null;
        public ClassPreparePage()
        {
            this.InitializeComponent();
        }
        
        // 包括 "导入课程文件" 按钮 和 "重新选择课程文件" 按钮
        private void ImportButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button senderButton)
            {
                senderButton.IsEnabled = false;
            }
            // 解析元数据

            
        }

        // 包括 "打开编辑器" 按钮 和 "编辑课程文件" 按钮
        private void OpenEditorButton_Click(object sender, RoutedEventArgs e)
        {
            var editor = new ClassMode.ClassEditor();
            editor.Activate();
        }
    }
}
