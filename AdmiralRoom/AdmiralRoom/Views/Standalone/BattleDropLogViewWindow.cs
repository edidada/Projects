﻿using System.Windows.Controls;
using Huoyaoyuan.AdmiralRoom.Logger;

namespace Huoyaoyuan.AdmiralRoom.Views.Standalone
{
    class BattleDropLogViewWindow : LogView
    {
        protected override void ItemDetail(ListViewItem item)
        {
            var log = item.DataContext as BattleDropLog;
            var detail = Loggers.BattleDetailLogger.FindLog(log.DateTime);
            if (detail?.IsValid == true)
                new BattleDetailView { DataContext = detail.ToViewModel(log) }.Show();
        }
    }
}
