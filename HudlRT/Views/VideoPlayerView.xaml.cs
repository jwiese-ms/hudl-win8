﻿using HudlRT.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI.ApplicationSettings;
using Windows.UI.Popups;

namespace HudlRT.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class VideoPlayerView : LayoutAwarePage
    {
        private bool _isFullscreenToggle = false;
        public bool IsFullscreen
        {
            get { return _isFullscreenToggle; }
            set { _isFullscreenToggle = value; }
        }

        private Size _previousVideoContainerSize = new Size();
        private double _previousVolValue = 0;

        private DispatcherTimer _timer;
        private bool _sliderpressed = false;

        private string _rootNamespace;

        public string RootNamespace
        {
            get { return _rootNamespace; }
            set { _rootNamespace = value; }
        }

        public VideoPlayerView()
        {
            this.InitializeComponent();
            SettingsPane.GetForCurrentView().CommandsRequested += CharmsData.SettingCharmManager_CommandsRequested;

            Loaded += new RoutedEventHandler(MainPage_Loaded);
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.  The Parameter
        /// property is typically used to configure the page.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            var library = Windows.Storage.KnownFolders.VideosLibrary;
            var queryOptions = new Windows.Storage.Search.QueryOptions();
            queryOptions.FolderDepth = Windows.Storage.Search.FolderDepth.Deep;
            queryOptions.IndexerOption = Windows.Storage.Search.IndexerOption.UseIndexerWhenAvailable;

            var fileQuery = library.CreateFileQueryWithOptions(queryOptions);

            var fif = new Windows.Storage.BulkAccess.FileInformationFactory(fileQuery,
                Windows.Storage.FileProperties.ThumbnailMode.VideosView, 190,
                Windows.Storage.FileProperties.ThumbnailOptions.UseCurrentScale, false);

            var dataSource = fif.GetVirtualizedFilesVector();
            
            //videosList.ItemsSource = dataSource;
        }

        private void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            //videoMediaElement.CurrentStateChanged += videoMediaElement_CurrentStateChanged;
            //timelineSlider.ValueChanged += timelineSlider_ValueChanged;

            //PointerEventHandler pointerpressedhandler = new PointerEventHandler(slider_PointerEntered);
            //timelineSlider.AddHandler(Control.PointerPressedEvent, pointerpressedhandler, true);

            //PointerEventHandler pointerreleasedhandler = new PointerEventHandler(slider_PointerCaptureLost);
            //timelineSlider.AddHandler(Control.PointerCaptureLostEvent, pointerreleasedhandler, true);
        }

        private async void VideosList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                Windows.Storage.BulkAccess.FileInformation fileInfo = e.AddedItems[0] as Windows.Storage.BulkAccess.FileInformation;
                if (fileInfo != null)
                {

                    Windows.Storage.StorageFile file = await Windows.Storage.StorageFile.GetFileFromPathAsync(fileInfo.Path);

                    var stream = await file.OpenAsync(Windows.Storage.FileAccessMode.Read);

                    videoMediaElement.SetSource(stream, file.ContentType);

                }
            }
            btnPause.Visibility = Visibility.Collapsed;
            btnPlay.Visibility = Visibility.Visible;
        }

        private void FullscreenToggle()
        {
            this.IsFullscreen = !this.IsFullscreen;

            if (this.IsFullscreen)
            {
                // Hide all non full screen controls
                header.Visibility = Visibility.Collapsed;
                header.UpdateLayout();
                Clips.Visibility = Visibility.Collapsed;
                Clips.UpdateLayout();
                TransportControlsPanel_Left.Visibility = Visibility.Collapsed;
                TransportControlsPanel_Right.Visibility = Visibility.Collapsed;
                gridHeaders.Visibility = Visibility.Collapsed;

                // Show the full screen controls
                full_mainGrid.Visibility = Visibility.Visible;

                // Save the video containers size
                _previousVideoContainerSize.Width = videoContainer.ActualWidth;
                _previousVideoContainerSize.Height = videoContainer.ActualHeight;

                // Set the video container to fullscreen
                videoContainer.Width = Window.Current.Bounds.Width;
                videoContainer.Height = Window.Current.Bounds.Height;
                videoMediaElement.Width = Window.Current.Bounds.Width;
                videoMediaElement.Height = Window.Current.Bounds.Height;
                VideoGrid.Margin = new Thickness(0, 0, 0, 0);
            }
            else
            {
                // Show the non full screen controls
                header.Visibility = Visibility.Visible;
                Clips.Visibility = Visibility.Visible;
                TransportControlsPanel_Left.Visibility = Visibility.Visible;
                TransportControlsPanel_Right.Visibility = Visibility.Visible;
                gridHeaders.Visibility = Visibility.Visible;

                // Hide the full screen controls
                full_mainGrid.Visibility = Visibility.Collapsed;

                // Reset the video container to it's original height
                videoContainer.Width = _previousVideoContainerSize.Width;
                videoContainer.Height = _previousVideoContainerSize.Height;
                videoMediaElement.Width = _previousVideoContainerSize.Width;
                videoMediaElement.Height = _previousVideoContainerSize.Height;
                VideoGrid.Margin = new Thickness(0, 70, 0, 0);
            }
        }

        private void btnFullScreenToggle_Click(object sender, RoutedEventArgs e)
        {
            FullscreenToggle();
        }

        private void VideoContainer_KeyUp(object sender, KeyRoutedEventArgs e)
        {
            if (IsFullscreen && e.Key == Windows.System.VirtualKey.Escape)
            {
                FullscreenToggle();
            }

            e.Handled = true;
        }

        private void btnPlay_Click(object sender, RoutedEventArgs e)
        {
            if (videoMediaElement.DefaultPlaybackRate != 1)
            {
                videoMediaElement.DefaultPlaybackRate = 1.0;
                videoMediaElement.PlaybackRate = 1.0;
            }

            //SetupTimer();
            videoMediaElement.Play();

            // Here we need to collapse and expand both full and non full screen buttons
            btnPlay.Visibility = Visibility.Collapsed;
            full_btnPlay.Visibility = Visibility.Collapsed;
            btnPause.Visibility = Visibility.Visible;
            full_btnPause.Visibility = Visibility.Visible;
        }

        private void btnPause_Click(object sender, RoutedEventArgs e)
        {
            videoMediaElement.Pause();

            // Here we need to collapse and expand both full and non full screen buttons
            btnPause.Visibility = Visibility.Collapsed;
            full_btnPause.Visibility = Visibility.Collapsed;
            btnPlay.Visibility = Visibility.Visible;
            full_btnPlay.Visibility = Visibility.Visible;
        }

        private void btnStop_Click(object sender, RoutedEventArgs e)
        {
            videoMediaElement.Stop();
            btnPause.Visibility = Visibility.Collapsed;
            btnPlay.Visibility = Visibility.Visible;
        }

        private void btnFastForward_Click(object sender, RoutedEventArgs e)
        {
            videoMediaElement.DefaultPlaybackRate = 2.0;
            videoMediaElement.Play();
            btnPlay.Visibility = Visibility.Collapsed;
            btnPause.Visibility = Visibility.Visible;
        }

        private void btnFastReverse_Click(object sender, RoutedEventArgs e)
        {
            videoMediaElement.DefaultPlaybackRate = -2.0;
            videoMediaElement.Play();
            btnPlay.Visibility = Visibility.Collapsed;
            btnPause.Visibility = Visibility.Visible;
        }

        private void btnSlowReverse_Click(object sender, RoutedEventArgs e)
        {
            videoMediaElement.DefaultPlaybackRate = -0.5;
            videoMediaElement.Play();
            btnPlay.Visibility = Visibility.Collapsed;
            btnPause.Visibility = Visibility.Visible;
        }

        private void btnSlowForward_Click(object sender, RoutedEventArgs e)
        {
            videoMediaElement.DefaultPlaybackRate = 0.5;
            videoMediaElement.Play();
            btnPlay.Visibility = Visibility.Collapsed;
            btnPause.Visibility = Visibility.Visible;
        }

        private void btnForward_Click(object sender, RoutedEventArgs e)
        {
        }

        private void btnReverse_Click(object sender, RoutedEventArgs e)
        {
        }

        private void btnVolumeDown_Click(object sender, RoutedEventArgs e)
        {
            if (videoMediaElement.IsMuted)
            {
                videoMediaElement.IsMuted = false;
                //vol.Value = _previousVolValue;
            }

            if (videoMediaElement.Volume < 1)
            {
                videoMediaElement.Volume += .1;
            }
            //vol.Value -= 10;
        }

        private void btnMute_Click(object sender, RoutedEventArgs e)
        {
            if (videoMediaElement.IsMuted)
            {
            }
            else
            {
                //_previousVolValue = vol.Value;
                //vol.Value = 0;
            }
            videoMediaElement.IsMuted = !videoMediaElement.IsMuted;

        }

        private void btnVolumeUp_Click(object sender, RoutedEventArgs e)
        {
            if (videoMediaElement.IsMuted)
            {
                videoMediaElement.IsMuted = false;
                //vol.Value = _previousVolValue;
            }

            if (videoMediaElement.Volume > 0)
            {
                videoMediaElement.Volume -= .1;
            }
            //vol.Value += 10;
        }

        void videoElement_MediaOpened(object sender, RoutedEventArgs e)
        {
            //double absvalue = Math.Round(
            //    videoMediaElement.NaturalDuration.TimeSpan.TotalSeconds,
            //    2);
            btnPause.Visibility = Visibility.Collapsed;
            btnPlay.Visibility = Visibility.Visible;

            //double videoWidth = videoMediaElement.ActualWidth;
            //double videoHeight = videoMediaElement.ActualHeight;

            //double cellWidth = Container2.ColumnDefinitions.ElementAt(1).ActualWidth;
            ////double cellHeight = Container1.RowDefinitions.ElementAt(1).ActualHeight;

            //double widthDifference = cellWidth - videoWidth;
            ////double heightDifference = cellHeight - videoHeight;


            //Canvas.SetLeft(playerCanvas, 300);
            //playerCanvas.UpdateLayout();

            //timelineSlider.Maximum = absvalue;

            //timelineSlider.StepFrequency =
            //    SliderFrequency(videoMediaElement.NaturalDuration.TimeSpan);

            //SetupTimer();
        }

        void videoMediaElement_MediaEnded(object sender, RoutedEventArgs e)
        {
            //StopTimer();
            btnPause.Visibility = Visibility.Collapsed;
            btnPlay.Visibility = Visibility.Visible;
            //timelineSlider.Value = 0.0;
        }

        private void videoMediaElement_MediaFailed(object sender, ExceptionRoutedEventArgs e)
        {
            // get HRESULT from event args 
            string hr = GetHresultFromErrorMessage(e);

            // Handle media failed event appropriately 
        }

        private string GetHresultFromErrorMessage(ExceptionRoutedEventArgs e)
        {
            String hr = String.Empty;
            String token = "HRESULT - ";
            const int hrLength = 10;     // eg "0xFFFFFFFF"

            int tokenPos = e.ErrorMessage.IndexOf(token, StringComparison.Ordinal);
            if (tokenPos != -1)
            {
                hr = e.ErrorMessage.Substring(tokenPos + token.Length, hrLength);
            }

            return hr;
        }

        //void slider_PointerEntered(object sender, PointerRoutedEventArgs e)
        //{
        //    System.Diagnostics.Debug.WriteLine("Pointer entered event fired");
        //    _sliderpressed = true;
        //}

        //void slider_PointerCaptureLost(object sender, PointerRoutedEventArgs e)
        //{
        //    System.Diagnostics.Debug.WriteLine("Pointer capture lost event fired");
        //    System.Diagnostics.Debug.WriteLine("Slider value at capture lost {0}", timelineSlider.Value);
        //    //myMediaElement.PlaybackRate = 1;
        //    videoMediaElement.Position = TimeSpan.FromSeconds(timelineSlider.Value);
        //    _sliderpressed = false;
        //}

        //void timelineSlider_ValueChanged(object sender, Windows.UI.Xaml.Controls.Primitives.RangeBaseValueChangedEventArgs e)
        //{
        //    System.Diagnostics.Debug.WriteLine("Slider old value = {0} new value = {1}.", e.OldValue, e.NewValue);
        //    if (!_sliderpressed)
        //    {
        //        videoMediaElement.Position = TimeSpan.FromSeconds(e.NewValue);
        //    }
        //}

        //private void SetupTimer()
        //{
        //    _timer = new DispatcherTimer();
        //    _timer.Interval = TimeSpan.FromSeconds(timelineSlider.StepFrequency);
        //    StartTimer();
        //}

        //private void _timer_Tick(object sender, object e)
        //{
        //    if (!_sliderpressed)
        //    {
        //        timelineSlider.Value = videoMediaElement.Position.TotalSeconds;
        //    }
        //}

        //private void StartTimer()
        //{
        //    _timer.Tick += _timer_Tick;
        //    _timer.Start();
        //}

        //private void StopTimer()
        //{
        //    _timer.Stop();
        //    _timer.Tick -= _timer_Tick;
        //}

        //void videoMediaElement_CurrentStateChanged(object sender, RoutedEventArgs e)
        //{
        //    if (videoMediaElement.CurrentState == MediaElementState.Playing)
        //    {
        //        if (_sliderpressed)
        //        {
        //            _timer.Stop();
        //        }
        //        else
        //        {
        //            _timer.Start();
        //        }
        //        MediaControl.IsPlaying = true;
        //    }

        //    if (videoMediaElement.CurrentState == MediaElementState.Paused)
        //    {
        //        _timer.Stop();
        //        MediaControl.IsPlaying = false;
        //    }

        //    if (videoMediaElement.CurrentState == MediaElementState.Stopped)
        //    {
        //        _timer.Stop();
        //        timelineSlider.Value = 0;
        //        MediaControl.IsPlaying = false;
        //    }
        //}

        //private double SliderFrequency(TimeSpan timevalue)
        //{
        //    double stepfrequency = -1;

        //    double absvalue = (int)Math.Round(timevalue.TotalSeconds, MidpointRounding.AwayFromZero);
        //    stepfrequency = (int)(Math.Round(absvalue / 100));

        //    if (timevalue.TotalMinutes >= 10 && timevalue.TotalMinutes < 30)
        //    {
        //        stepfrequency = 10;
        //    }
        //    else if (timevalue.TotalMinutes >= 30 && timevalue.TotalMinutes < 60)
        //    {
        //        stepfrequency = 30;
        //    }
        //    else if (timevalue.TotalHours >= 1)
        //    {
        //        stepfrequency = 60;
        //    }

        //    if (stepfrequency == 0) stepfrequency += 1;

        //    if (stepfrequency == 1)
        //    {
        //        stepfrequency = absvalue / 100;
        //    }

        //    return stepfrequency;
        //}
    }
}
