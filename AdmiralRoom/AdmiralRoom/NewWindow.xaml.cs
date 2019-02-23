﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Huoyaoyuan.AdmiralRoom.Composition;
using Huoyaoyuan.AdmiralRoom.Models;
using Meowtrix.WPF.Extend;
using Microsoft.WindowsAPICodePack.Dialogs;
using Xceed.Wpf.AvalonDock.Layout;
using Xceed.Wpf.AvalonDock.Layout.Serialization;

namespace Huoyaoyuan.AdmiralRoom
{
    /// <summary>
    /// NewWindow.xaml 的交互逻辑
    /// </summary>
    public partial class NewWindow : Window
    {
        public NewWindow()
        {
            InitializeComponent();
            ResourceService.CultureChanged += OnCultureChanged;

            foreach (var subview in ModuleHost.Instance.SubViews)
            {
                AddOrShowView(subview, false);
                var menuitem = new MenuItem
                {
                    Header = subview.GetTitle(ResourceService.CurrentCulture)
                };
                menuitem.Click += (_, __) => AddOrShowView(subview, true);
                ResourceService.CultureChanged += (_, e) => menuitem.Header = subview.GetTitle(e.NewValue);
                subviews.Items.Add(menuitem);
            }

            foreach (var subwindow in ModuleHost.Instance.SubWindows)
            {
                var menuitem = new MenuItem
                {
                    Header = subwindow.GetTitle(ResourceService.CurrentCulture)
                };
                var closure = new SubWindowClosure(menuitem, subwindow);
                menuitem.Click += closure.Click;
                ResourceService.CultureChanged += closure.OnCultureChanged;
                switch (subwindow.Category)
                {
                    case SubWindowCategory.Overview:
                        subwindowOverview.Items.Add(menuitem);
                        break;
                    case SubWindowCategory.Statistics:
                        subwindowStatistics.Items.Add(menuitem);
                        break;
                    case SubWindowCategory.Information:
                        subwindowInformation.Items.Add(menuitem);
                        break;
                }
            }

            ResourceService.CultureChanged += (_, __) => viewList[nameof(GameHost)].Title = StringTable.Browser;

            DockCommands = new Config.CommandSet
            {
                Save = new DelegateCommand(() => TrySaveLayout()),
                Load = new DelegateCommand(() => TryLoadLayout()),
                SaveAs = new DelegateCommand(() =>
                {
                    using (var filedialog = new CommonSaveFileDialog())
                    {
                        filedialog.InitialDirectory = Environment.CurrentDirectory;
                        filedialog.DefaultFileName = "config.xml";
                        filedialog.Filters.Add(new CommonFileDialogFilter("Xml Files", "*.xml"));
                        filedialog.Filters.Add(new CommonFileDialogFilter("All Files", "*"));
                        if (filedialog.ShowDialog() == CommonFileDialogResult.Ok)
                            TrySaveLayout(filedialog.FileName);
                    }
                }),
                LoadFrom = new DelegateCommand(() =>
                {
                    using (var filedialog = new CommonOpenFileDialog())
                    {
                        filedialog.InitialDirectory = Environment.CurrentDirectory;
                        filedialog.Filters.Add(new CommonFileDialogFilter("Xml Files", "*.xml"));
                        filedialog.Filters.Add(new CommonFileDialogFilter("All Files", "*"));
                        if (filedialog.ShowDialog() == CommonFileDialogResult.Ok)
                            TryLoadLayout(filedialog.FileName);
                    }
                })
            };
        }

        private class SubWindowClosure
        {
            private MenuItem _menuitem;
            private ISubWindow _isubwindow;
            private Window _window;
            public SubWindowClosure(MenuItem menuitem, ISubWindow isubwindow)
            {
                _menuitem = menuitem;
                _isubwindow = isubwindow;
            }
            public void Click(object sender, RoutedEventArgs e)
            {
                if (_window == null)
                {
                    _window = _isubwindow.CreateWindow();
                    _window.Closed += WindowClosed;
                }
                _window.Show();
                _window.Activate();
            }
            private void WindowClosed(object sender, EventArgs e) => _window = null;
            public void OnCultureChanged(object sender, PropertyChangedEventArgs<CultureInfo> e) => _menuitem.Header = _isubwindow.GetTitle(e.NewValue);
        }

        private void MakeViewList(ILayoutElement elem)
        {
            if (elem is LayoutContent)
            {
                viewList[(elem as LayoutContent).ContentId] = elem as LayoutContent;
                return;
            }
            if (elem is ILayoutContainer)
                foreach (var child in (elem as ILayoutContainer).Children)
                    MakeViewList(child);
        }
        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            Win32Helper.GetRestoreWindowPosition(this);
            TryLoadLayout();
            foreach (var view in DockMan.Layout.Hidden.Where(x => x.PreviousContainerIndex == -1).ToArray())
                DockMan.Layout.Hidden.Remove(view);
        }
        private void OnClosed(object sender, EventArgs e) => TrySaveLayout();
        private void OnClosing(object sender, CancelEventArgs e)
        {
            string message = "";
            if (Officer.Staff.Current.Homeport.Fleets?.Skip(1)
                .Any(x => x.Status == Officer.FleetStatus.InMission
                && x.BackTime - DateTimeOffset.Now < TimeSpan.FromMinutes(10))
                == true)
                message = StringTable.ExitConfirm_MissionComplete;
            else if (Officer.Staff.Current.Homeport.Ships
                ?.Any(x => !x.HP.IsMax
                && !x.IsRepairing
                && x.InFleet?.CanHomeportRepairing != true)
                == true)
                message = StringTable.ExitConfirm_Repair;
            else if (Officer.Staff.Current.Homeport.Fleets?.Skip(1).Any(x => x.Status == Officer.FleetStatus.Ready) == true)
                message = StringTable.ExitConfirm_MissionStart;
            else if (Officer.Staff.Current.Quests.QuestInProgress?.Any(x => x.Period == Officer.QuestPeriod.Daily) == true)
                message = StringTable.ExitConfirm_Quest;
            message += StringTable.ExitConfirm_Confirm;
            if (MessageBox.Show(message, StringTable.ExitConfirm, MessageBoxButton.YesNo) != MessageBoxResult.Yes)
            {
                e.Cancel = true;
                return;
            }
            Win32Helper.SetRestoreWindowPosition(this);
        }

        #region Layout
        private void TryLoadLayout(string path = "layout.xml")
        {
            XmlLayoutSerializer layoutserializer = new XmlLayoutSerializer(DockMan);
            layoutserializer.LayoutSerializationCallback += (_, args) => args.Model.Content = args.Content;
            try
            {
                layoutserializer.Deserialize(path);
            }
            catch { }
            viewList.Clear();
            MakeViewList(DockMan.Layout);
        }
        private void TrySaveLayout(string path = "layout.xml")
        {
            XmlLayoutSerializer layoutserializer = new XmlLayoutSerializer(DockMan);
            try
            {
                layoutserializer.Serialize(path);
            }
            catch { }
        }
        #endregion

        private readonly Dictionary<string, LayoutContent> viewList = new Dictionary<string, LayoutContent>();
        private readonly Dictionary<string, ISubView> subviewmap = new Dictionary<string, ISubView>();
        private void AddOrShowView(ISubView view, bool show)
        {
            subviewmap[view.ContentID] = view;
            string viewname = view.ContentID;
            LayoutAnchorable targetView;
            viewList.TryGetValue(viewname, out LayoutContent targetContent);
            targetView = targetContent as LayoutAnchorable;
            if (targetView == null)
            {
                targetView = new LayoutAnchorable();
                viewList.Add(viewname, targetView);
                targetView.AddToLayout(DockMan, AnchorableShowStrategy.Most);
                targetView.DockAsDocument();
                targetView.CanClose = false;
                targetView.Hide();
            }
            if (targetView.Content == null)
            {
                targetView.Content = view.View;
                targetView.ContentId = viewname;
                targetView.Title = view.GetTitle(ResourceService.CurrentCulture);
                targetView.CanAutoHide = true;
            }
            if (show) targetView.IsVisible = true;
        }

        private void OnCultureChanged(object sender, PropertyChangedEventArgs<CultureInfo> e)
        {
            foreach (var viewname in viewList.Keys)
            {
                var target = viewList[viewname];
                if (subviewmap.TryGetValue(viewname, out ISubView view))
                    target.Title = view.GetTitle(e.NewValue);
            }
        }

        private void SetBrowserZoomFactor(object sender, RoutedPropertyChangedEventArgs<double> e)
            => this.GameHost.ApplyZoomFactor(e.NewValue);

        private void ShowConfigWindow(object sender, RoutedEventArgs e) => new ConfigWindow { Owner = this }.ShowDialog();

        public Config.CommandSet DockCommands { get; }

        private void ShowAboutWindow(object sender, RoutedEventArgs e) => new AboutWindow { Owner = this }.ShowDialog();

        private void ShowLastException(object sender, MouseButtonEventArgs e)
        {
            var exception = Status.Current.LatestException;
            if (exception != null)
                MessageBox.Show(this, exception.StackTrace, exception.Message);
        }
    }
}
