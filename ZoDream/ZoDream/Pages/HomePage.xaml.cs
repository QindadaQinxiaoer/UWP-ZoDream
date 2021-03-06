﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using ZoDream.Helpers;
using ZoDream.Models;
using ZoDream.Models.Api;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace ZoDream.Pages
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class HomePage : Page, ISubPage
    {
        private IncrementalLoadingCollection<Blog> Blogs;

        private uint page = 1;

        private BlogApi blogApi = new BlogApi();

        public string NavTitile => "博客";

        public HomePage()
        {
            this.InitializeComponent();
            Blogs = new IncrementalLoadingCollection<Blog>(count => {
                Log.Info(count);
                return blogApi.GetListAsync(page = count);
            });
            ListView.ItemsSource = Blogs;
        }


        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            Refresh();
        }

        private async void Refresh()
        {
            Blogs.Clear();
            page = 1;
            await Fetch(page);
        }

        private async Task Fetch(uint page)
        {
            LoadingRing.IsActive = true;

            var data = await blogApi.GetListAsync(page);
            if (data.Item1 != null)
            {
                foreach (var blog in data.Item1)
                {
                    Blogs.Add(blog);
                }
            }
            MoreBtn.Visibility = data.Item2 ? Visibility.Visible : Visibility.Collapsed;
            LoadingRing.IsActive = false;
        }

        private void RefreshContainer_RefreshRequested(RefreshContainer sender, RefreshRequestedEventArgs args)
        {
            Refresh();
        }

        private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ListView.SelectedItem is Blog blog)
            {
                Blog.PageTitle = blog.Title;
                Frame.Navigate(typeof(BlogPage), blog.Id);
            };
        }

        private void MoreBtn_Tapped(object sender, TappedRoutedEventArgs e)
        {
            Fetch(++page);
        }
    }
}
