﻿using Hipda.Client.Models;
using Hipda.Client.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hipda.Client.ViewModels
{
    public class ForumAllCategoryListViewViewModel : NotificationObject
    {
        ForumService _ds;

        private List<ForumCategoryModel> _forumAllCategoryList = new List<ForumCategoryModel>();

        public List<ForumCategoryModel> ForumAllCategoryList
        {
            get { return _forumAllCategoryList; }
            set
            {
                _forumAllCategoryList = value;
                this.RaisePropertyChanged("ForumAllCategoryList");
            }
        }

        public ForumAllCategoryListViewViewModel()
        {
            _ds = new ForumService();
            GetData();
        }

        async void GetData()
        {
            ForumAllCategoryList = await _ds.GetForumData();
        }
    }
}
