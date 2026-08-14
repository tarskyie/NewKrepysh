using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using NewKrepysh.WinUI.Models;
using NewKrepysh.WinUI.Views;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;

namespace NewKrepysh.WinUI
{
    public sealed partial class MainWindow : Window
    {
        public MainWindow()
        {
            AppWindow.TitleBar.PreferredTheme = TitleBarTheme.UseDefaultAppMode;
            
            InitializeComponent();

            Main_Frame.Navigate(typeof(Views.FileMenu));
        }

        public void Navigate(Type type)
        {
            Main_Frame.Navigate(type);
        }

        public void OpenProjectInEditor(Project project)
        {
            Main_Frame.Navigate(typeof(EditorPage), project);
        }
    }
}
