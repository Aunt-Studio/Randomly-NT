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
        
        // ���� "����γ��ļ�" ��ť �� "����ѡ��γ��ļ�" ��ť
        private void ImportButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button senderButton)
            {
                senderButton.IsEnabled = false;
            }
            // ����Ԫ����

            
        }

        // ���� "�򿪱༭��" ��ť �� "�༭�γ��ļ�" ��ť
        private void OpenEditorButton_Click(object sender, RoutedEventArgs e)
        {
            var editor = new ClassMode.ClassEditor();
            editor.Activate();
        }
    }
}
