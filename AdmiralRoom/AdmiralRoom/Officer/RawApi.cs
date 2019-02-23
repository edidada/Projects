﻿using System.Collections.Generic;
using System.Collections.ObjectModel;
using Huoyaoyuan.AdmiralRoom.Models;
using Meowtrix.ComponentModel;
using Newtonsoft.Json.Linq;

namespace Huoyaoyuan.AdmiralRoom.Officer
{
    class RawApi : NotificationObject
    {
        public static RawApi Instance { get; } = new RawApi();
        private RawApi()
        {
            Staff.API("").Subscribe(AddSession);
        }
        private void AddSession(CachedSession session)
        {
            _list.Insert(0, new ApiModel(session));
            while (_list.Count > SaveCount)
                _list.RemoveAt(SaveCount);
        }

        #region SaveCount
        private int _savecount = 20;
        public int SaveCount
        {
            get { return _savecount; }
            set
            {
                if (value <= 1) value = 1;
                if (_savecount != value)
                {
                    _savecount = value;
                    OnPropertyChanged();
                }
            }
        }
        #endregion

        private ObservableCollection<ApiModel> _list = new ObservableCollection<ApiModel>().WithSyncBindingEnabled();
        public IReadOnlyList<ApiModel> List => _list;

        #region SelectedIndex
        private int _selectedindex = -1;
        public int SelectedIndex
        {
            get { return _selectedindex; }
            set
            {
                if (_selectedindex != value)
                {
                    _selectedindex = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(SelectedItem));
                    OnPropertyChanged(nameof(SelectedJTokens));
                }
            }
        }
        #endregion

        public ApiModel SelectedItem => SelectedIndex < 0 || SelectedIndex >= List.Count ? null : List[SelectedIndex];
        public JToken[] SelectedJTokens => SelectedItem == null ? new JToken[0] : new[] { SelectedItem.Json };
    }
}
