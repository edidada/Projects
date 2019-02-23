﻿using System.ComponentModel.Composition;
using System.Globalization;
using System.Windows;
using Huoyaoyuan.AdmiralRoom.Composition;

namespace Huoyaoyuan.AdmiralRoom.Logger
{
    [Export(typeof(ISubWindow))]
    public class CreateShipLoggerView : ISubWindow
    {
        public SubWindowCategory Category => SubWindowCategory.Statistics;

        public Window CreateWindow()
            => new Views.Standalone.LogView { DataContext = new ViewProvider<CreateShipLog>(Loggers.CreateShipLogger), TitleKey = "Logger_CreateShip" };

        public string GetTitle(CultureInfo culture) => StringTable.Logger_CreateShip;
    }
    [Export(typeof(ISubWindow))]
    public class CreateItemLoggerView : ISubWindow
    {
        public SubWindowCategory Category => SubWindowCategory.Statistics;

        public Window CreateWindow()
            => new Views.Standalone.LogView { DataContext = new ViewProvider<CreateItemLog>(Loggers.CreateItemLogger), TitleKey = "Logger_CreateItem" };

        public string GetTitle(CultureInfo culture) => StringTable.Logger_CreateItem;
    }
    [Export(typeof(ISubWindow))]
    public class BattleDropLoggerView : ISubWindow
    {
        public SubWindowCategory Category => SubWindowCategory.Statistics;

        public Window CreateWindow()
            => new Views.Standalone.BattleDropLogViewWindow { DataContext = new ViewProvider<BattleDropLog>(Loggers.BattleDropLogger), TitleKey = "Logger_Drop" };

        public string GetTitle(CultureInfo culture) => StringTable.Logger_Drop;
    }
    [Export(typeof(ISubWindow))]
    public class MissionLoggerView : ISubWindow
    {
        public SubWindowCategory Category => SubWindowCategory.Statistics;

        public Window CreateWindow()
            => new Views.Standalone.LogView { DataContext = new ViewProvider<MissionLog>(Loggers.MissionLogger), TitleKey = "Expedition" };

        public string GetTitle(CultureInfo culture) => StringTable.Expedition;
    }
}
