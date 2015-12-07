﻿using Hipda.Client.Uwp.Pro.Services;
using Hipda.Client.Uwp.Pro.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;

// The Templated Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234235

namespace Hipda.Client.Uwp.Pro
{
    public sealed class MyRefLink1 : Control
    {
        public MyRefLink1()
        {
            this.DefaultStyleKey = typeof(MyRefLink1);
        }



        public int PostId
        {
            get { return (int)GetValue(PostIdProperty); }
            set { SetValue(PostIdProperty, value); }
        }

        // Using a DependencyProperty as the backing store for PostId.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PostIdProperty =
            DependencyProperty.Register("PostId", typeof(int), typeof(MyRefLink1), new PropertyMetadata(0));



        public int ThreadId
        {
            get { return (int)GetValue(ThreadIdProperty); }
            set { SetValue(ThreadIdProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ThreadId.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ThreadIdProperty =
            DependencyProperty.Register("ThreadId", typeof(int), typeof(MyRefLink1), new PropertyMetadata(0));



        public string LinkContent
        {
            get { return (string)GetValue(LinkContentProperty); }
            set { SetValue(LinkContentProperty, value); }
        }

        // Using a DependencyProperty as the backing store for LinkContent.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty LinkContentProperty =
            DependencyProperty.Register("LinkContent", typeof(string), typeof(MyRefLink1), new PropertyMetadata(string.Empty));


        protected override void OnTapped(TappedRoutedEventArgs e)
        {
            base.OnTapped(e);

            var parentPage = Common.FindParent<ThreadAndReplyPage>(this);
            if (parentPage != null)
            {
                parentPage.ShowPostDetailByPostId(PostId, ThreadId);
            }
        }
    }
}
