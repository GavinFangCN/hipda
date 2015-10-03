﻿using Hipda.Client.Uwp.Pro.Commands;
using Hipda.Client.Uwp.Pro.Models;
using Hipda.Client.Uwp.Pro.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;

namespace Hipda.Client.Uwp.Pro.ViewModels
{
    public class ThreadAndReplyViewModel : NotificationObject
    {
        private int _forumId { get; set; }
        private ListView _threadListView { get; set; }
        private Action _beforeLoad { get; set; }
        private Action _afterLoad { get; set; }
        private DataService _ds { get; set; }

        public DelegateCommand RefreshThreadCommand { get; set; }

        private ICollectionView _threadItemCollection;

        public ICollectionView ThreadItemCollection
        {
            get { return _threadItemCollection; }
            set
            {
                _threadItemCollection = value;
                this.RaisePropertyChanged("ThreadItemCollection");
            }
        }

        private void LoadData()
        {
            var cv = _ds.GetViewForThreadPage(_forumId, _beforeLoad, _afterLoad);
            if (cv != null)
            {
                ThreadItemCollection = cv;
            }
        }

        public ThreadAndReplyViewModel(int forumId, ListView threadListView, Action beforeLoad, Action afterLoad)
        {
            _forumId = forumId;
            _threadListView = threadListView;
            _beforeLoad = beforeLoad;
            _afterLoad = afterLoad;
            _ds = new DataService();

            RefreshThreadCommand = new DelegateCommand();
            RefreshThreadCommand.ExecuteAction = new Action<object>(RefreshThreadExecute);

            LoadData();
        }

        private void RefreshThreadExecute(object parameter)
        {
            _threadListView.ItemsSource = null;
            _ds.ClearThreadData(_forumId);

            LoadData();
            _threadListView.ItemsSource = ThreadItemCollection;
        }

        public void LoadPrevPage()
        {
            // 先获取当前数据中已存在的最小页码

        }
    }
}
