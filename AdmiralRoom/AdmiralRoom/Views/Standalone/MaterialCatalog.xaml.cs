﻿using System.ComponentModel.Composition;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Huoyaoyuan.AdmiralRoom.Composition;

namespace Huoyaoyuan.AdmiralRoom.Views.Standalone
{
    /// <summary>
    /// MaterialCatalog.xaml 的交互逻辑
    /// </summary>
    public partial class MaterialCatalog : Window
    {
        public MaterialCatalog()
        {
            InitializeComponent();
        }

        private void CheckBox_Click(object sender, RoutedEventArgs e) => Update();

        private void CheckBox_Mouse(object sender, MouseEventArgs e) => Update();

        private void Update()
        {
            var check = checks.Children.OfType<CheckBox>();
            chart.UpdateShown(
                check.Select(x => x.IsChecked.Value).ToArray(),
                check.FirstOrDefault(x => x.IsMouseOver)?.Tag as int?);
        }
    }

    [Export(typeof(ISubWindow))]
    public class MaterialCatalogSubWindow : ISubWindow
    {
        public SubWindowCategory Category => SubWindowCategory.Statistics;

        public Window CreateWindow() => new MaterialCatalog { DataContext = new Logger.MaterialProvider(Logger.Loggers.MaterialLogger) };

        public string GetTitle(CultureInfo culture) => StringTable.Material;
    }
}
