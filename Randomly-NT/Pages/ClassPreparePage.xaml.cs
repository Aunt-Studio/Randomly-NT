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

        private void DebugButton_Click(object sender, RoutedEventArgs e)
        {
            if (selector is null)
            {
                var rawStudents = new List<RawStudent>
            {
                new RawStudent { Name = "张三", Score = 90 },
                new RawStudent { Name = "李四", Score = 80 },
                new RawStudent { Name = "王五", Score = 70 },
                new RawStudent { Name = "赵六", Score = 60 },
                new RawStudent { Name = "钱七", Score = 50 },
                new RawStudent { Name = "张dx", Score = 100 },
                new RawStudent { Name = "sssc", Score = 25 },
                new RawStudent { Name = "王qw五", Score = 36 },
                new RawStudent { Name = "scv", Score = 38 },
                new RawStudent { Name = "rwer", Score = 72 },
                new RawStudent { Name = "张dxsx", Score = 100 },
                new RawStudent { Name = "swsc", Score = 26 },
                new RawStudent { Name = "王qwww五", Score = 36 },
                new RawStudent { Name = "sclv", Score = 70 },
                new RawStudent { Name = "rwewr", Score = 72 },
            };
                var service = new StudentClusterService();
                var students = service.ClusterStudents(rawStudents);
                foreach (var s in students)
                {
                    Debug.WriteLine($"{s.Name} => {s.Level}");
                }

                selector = new StudentSelector(students);
            }

            var selectedStudent = selector.Select(Difficulty.Hard, RandomEntropySource.SystemClock);
            Debug.WriteLine($"Selected {selectedStudent.Name} as Level{selectedStudent.Level}");
        }
    }
}
