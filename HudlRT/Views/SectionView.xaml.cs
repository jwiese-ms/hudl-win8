﻿using HudlRT.Common;
using HudlRT.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace HudlRT.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SectionView : LayoutAwarePage
    {
        public SectionView()
        {
            this.InitializeComponent();
            CategoriesGridView.SelectionMode = ListViewSelectionMode.Multiple;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            LoadingRing.IsActive = false;
        }

        private void GoBack(object sender, RoutedEventArgs e)
        {

        }
    }
}
