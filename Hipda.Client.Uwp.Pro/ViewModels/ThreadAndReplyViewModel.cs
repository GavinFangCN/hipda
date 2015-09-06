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
        private ListView _threadListView { get; set; }
        private Action _beforeLoad { get; set; }
        private Action _afterLoad { get; set; }
        private DataService _ds { get; set; }

        public DelegateCommand RefreshThreadCommand { get; set; }

        public ICollectionView ThreadItemCollection { get; set; }

        public ThreadAndReplyViewModel(ListView threadListView, Action beforeLoad, Action afterLoad)
        {
            _threadListView = threadListView;
            _beforeLoad = beforeLoad;
            _afterLoad = afterLoad;
            _ds = new DataService();

            RefreshThreadCommand = new DelegateCommand();
            RefreshThreadCommand.ExecuteAction = new Action<object>(RefreshThreadExecute);

            var cv = _ds.GetViewForThreadPage(14, _beforeLoad, _afterLoad);
            if (cv != null)
            {
                ThreadItemCollection = cv;
            }
        }

        private async void RefreshThreadExecute(object parameter)
        {
            _threadListView.ItemsSource = null;
            await _ds.RefreshThreadData(14, new CancellationTokenSource());
            _threadListView.ItemsSource = ThreadItemCollection;
        }
    }
}
