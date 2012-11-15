﻿using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HudlRT.Parameters;
using HudlRT.Models;
using HudlRT.Common;
using Newtonsoft.Json;
using Windows.UI.Xaml.Controls;
using Windows.Storage;
using Windows.Networking.BackgroundTransfer;
using Windows.Foundation;
using Windows.UI.Xaml.Input;

namespace HudlRT.ViewModels
{
    public class VideoPlayerViewModel : ViewModelBase
    {
        private readonly INavigationService navigationService;
        private PlaybackType playbackType;
        public PagePassParameter Parameter { get; set; }
        private BindableCollection<Clip> clips;
        public BindableCollection<Clip> Clips
        {
            get { return clips; }
            set
            {
                clips = value;
                NotifyOfPropertyChange(() => Clips);
            }
        }
        private Angle selectedAngle;
        public Angle SelectedAngle
        {
            get { return selectedAngle; }
            set
            {
                selectedAngle = value;
                NotifyOfPropertyChange(() => SelectedAngle);
            }
        }

        private string[] gridHeaders;
        public string[] GridHeaders
        {
            get { return gridHeaders; }
            set
            {
                gridHeaders = value;
                NotifyOfPropertyChange(() => GridHeaders);
            }
        }
        private string cutupName;
        public string CutupName
        {
            get { return cutupName; }
            set
            {
                cutupName = value;
                NotifyOfPropertyChange(() => CutupName);
            }
        }
        private Clip selectedClip;
        public Clip SelectedClip
        {
            get { return selectedClip; }
            set
            {
                selectedClip = value;
                NotifyOfPropertyChange(() => SelectedClip);
            }
        }
        private string toggleButtonContent;
        public string ToggleButtonContent
        {
            get { return toggleButtonContent; }
            set
            {
                toggleButtonContent = value;
                NotifyOfPropertyChange(() => ToggleButtonContent);
            }
        }
        private BindableCollection<AngleType> angleNames;
        public BindableCollection<AngleType> AngleTypes
        {
            get { return angleNames; }
            set
            {
                angleNames = value;
                NotifyOfPropertyChange(() => AngleTypes);
            }
        }

        private int index = 0;
        Point initialPoint = new Point();
        Point currentPoint;
        bool isFullScreenGesture = false;

        public VideoPlayerViewModel(INavigationService navigationService) : base(navigationService)
        {
            this.navigationService = navigationService;
        }

        protected override void OnActivate()
        {
            base.OnActivate();
            Clips = Parameter.selectedCutup.clips;
            GridHeaders = Parameter.selectedCutup.displayColumns;
            if (Clips.Count > 0)
            {
                GetAngleNames();
                SelectedClip = Clips.First();
                List<Angle> filteredAngles = SelectedClip.angles.Where(angle => angle.angleType.IsChecked).ToList<Angle>();
                SelectedAngle = filteredAngles.Any() ? filteredAngles[0] : null;
            }
            CutupName = Parameter.selectedCutup.name;
            
            Windows.Storage.ApplicationDataContainer roamingSettings = Windows.Storage.ApplicationData.Current.RoamingSettings;
            if (roamingSettings.Values["hudl-playbackType"] == null)	
            {
                roamingSettings.Values["hudl-playbackType"] = (int)PlaybackType.once;	
            }
            playbackType = (PlaybackType)roamingSettings.Values["hudl-playbackType"];
            setToggleButtonContent();

            //GetAngleNames();
        }

        //private async void GetAngleNames()
        //{
        //    long teamID = (long)ApplicationData.Current.RoamingSettings.Values["hudl-teamID"];
        //    var roamingSettings = Windows.Storage.ApplicationData.Current.RoamingSettings;
        //    var names = await ServiceAccessor.MakeApiCallGet("teams/#/angleNames".Replace("#", teamID.ToString()));
        //    if (!string.IsNullOrEmpty(names))
        //    {
        //        List<string> returnedNames = JsonConvert.DeserializeObject<List<string>>(names);
        //        BindableCollection<AngleType> nameObjects = new BindableCollection<AngleType>();
        //        foreach (string s in returnedNames)
        //        {
        //            nameObjects.Add(new AngleType(s, this));
        //        }
        //        AngleTypes = nameObjects;
        //        foreach (Clip clip in Clips)
        //        {
        //            foreach (Angle angle in clip.angles)
        //            {
        //                angle.angleType = AngleTypes.Where(angleType => angleType.Name.Equals(angle.angleName)).FirstOrDefault();
        //            }
        //        }
        //    }

        //    getAnglePreferences();
        //}

        private void GetAngleNames()
        {
            List<string> types = new List<string>();
            foreach (Clip clip in Clips)
            {
                foreach (Angle angle in clip.angles)
                {
                    if(!types.Contains(angle.angleName))
                    {
                        types.Add(angle.angleName);
                    }
                }
            }

            BindableCollection<AngleType> typeObjects = new BindableCollection<AngleType>();
            foreach (string s in types)
            {
                typeObjects.Add(new AngleType(s, this));
            }

            AngleTypes = typeObjects;
            foreach (Clip clip in Clips)
            {
                foreach (Angle angle in clip.angles)
                {
                    angle.angleType = AngleTypes.Where(angleType => angleType.Name.Equals(angle.angleName)).FirstOrDefault();
                }
            }
 
            getAnglePreferences();
        }

        private void getAnglePreferences()
        {
            long teamID = (long)ApplicationData.Current.RoamingSettings.Values["hudl-teamID"];
            var roamingSettings = Windows.Storage.ApplicationData.Current.RoamingSettings;
            foreach (AngleType angleName in AngleTypes)
            {
                if (roamingSettings.Values[String.Concat(teamID.ToString(), "-", angleName.Name)] == null)
                {
                    angleName.IsChecked = true;
                }
                else
                {
                    angleName.IsChecked = (bool)roamingSettings.Values[String.Concat(teamID.ToString(), "-", angleName.Name)];
                }
            }
        }

        private void saveAnglePreferences()
        {
            long teamID = (long)ApplicationData.Current.RoamingSettings.Values["hudl-teamID"];
            var roamingSettings = Windows.Storage.ApplicationData.Current.RoamingSettings;
            foreach (AngleType angleName in AngleTypes)
            {
                roamingSettings.Values[String.Concat(teamID.ToString(), "-", angleName.Name)] = angleName.IsChecked;
            }
        }

        public void ClipSelected(ItemClickEventArgs eventArgs)
        {
            var clip = (Clip)eventArgs.ClickedItem;
            SelectedClip = clip;
            index = (int)clip.order;

            List<Angle> filteredAngles = clip.angles.Where(angle => angle.angleType.IsChecked).ToList<Angle>();
            SelectedAngle = filteredAngles.Any() ? filteredAngles[0] : null;
        }

        public void NextClip(NextAngleEvent eventType)
        {
            if (SelectedAngle == null)
            {
                goToNextClip();
            }
            else
            {
                List<Angle> filteredAngles = SelectedClip.angles.Where(angle => angle.angleType.IsChecked).ToList<Angle>();
                Angle currentAngle = SelectedClip.angles.Where(a => a.fileLocation.Equals(SelectedAngle.fileLocation)).FirstOrDefault();

                int angleIndex = filteredAngles.IndexOf(currentAngle);
                if (angleIndex < filteredAngles.Count - 1)
                {
                    SelectedAngle = filteredAngles[angleIndex + 1];
                }
                else
                {
                    if (eventType == NextAngleEvent.mediaEnded && playbackType == PlaybackType.loop)
                    {
                        SelectedAngle = filteredAngles.Any() ? new Angle(filteredAngles[0].fileLocation) : null;
                    }
                    else if(eventType == NextAngleEvent.buttonClick || playbackType == PlaybackType.next)
                    {
                        goToNextClip();
                    }
                }
            }
        }

        private void goToNextClip()
        {
            if (Clips.Count > 1)
            {
                index = (index + 1) % Clips.Count;

                SelectedClip = Clips[index];
                List<Angle> filteredAngles = SelectedClip.angles.Where(angle => angle.angleType.IsChecked).ToList<Angle>();
                SelectedAngle = filteredAngles.Any() ? filteredAngles[0] : null;
            }
        }

        public void PreviousClip(ItemClickEventArgs eventArgs)
        {
            if (SelectedAngle == null)
            {
                goToPreviousClip();
            }
            else
            {
                List<Angle> filteredAngles = SelectedClip.angles.Where(angle => angle.angleType.IsChecked).ToList<Angle>();
                Angle currentAngle = SelectedClip.angles.Where(a => a.fileLocation.Equals(SelectedAngle.fileLocation)).FirstOrDefault();

                int angleIndex = filteredAngles.IndexOf(currentAngle);
                if (angleIndex > 0)
                {
                    SelectedAngle = filteredAngles[angleIndex - 1];
                }
                else
                {
                    goToPreviousClip();
                }
            }
        }

        private void goToPreviousClip()
        {
            if (Clips.Count > 1)
            {
                index = (index == 0) ? Clips.Count - 1 : index - 1;

                SelectedClip = Clips[index];
                List<Angle> filteredAngles = SelectedClip.angles.Where(angle => angle.angleType.IsChecked).ToList<Angle>();
                SelectedAngle = filteredAngles.Any() ? filteredAngles[0] : null;
            }
        }

        public void angleFilter()
        {
            List<Angle> filteredAngles = SelectedClip.angles.Where(angle => angle.angleType.IsChecked).ToList<Angle>();
            //If the current angle has been filtered out, reset the clip to the first unfiltered angle, or null
            if (SelectedAngle != null)
            {
                if (filteredAngles.Where(angle => angle.fileLocation.Equals(SelectedAngle.fileLocation)).FirstOrDefault() == null)
                {
                    SelectedAngle = filteredAngles.Any() ? filteredAngles[0] : null;
                }
            }
            else
            {
                SelectedAngle = filteredAngles.Any() ? filteredAngles[0] : null;
            }
        }

        public void playbackToggle()
        {
            playbackType = (PlaybackType)(((int)playbackType + 1) % 3);
            
            setToggleButtonContent();
            Windows.Storage.ApplicationDataContainer roamingSettings = Windows.Storage.ApplicationData.Current.RoamingSettings;
            roamingSettings.Values["hudl-playbackType"] = (int)playbackType;
        }

        public void setToggleButtonContent()
        {
            if (playbackType == PlaybackType.once)
            {
                ToggleButtonContent = "Playback: Once";
            }
            else if (playbackType == PlaybackType.loop)
            {
                ToggleButtonContent = "Playback: Loop";
            }
            else
            {
                ToggleButtonContent = "Playback: Next";
            }
        }

        void videoMediaElement_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            if ((currentPoint.X == 0 && currentPoint.Y == 0) || (currentPoint.X - e.Position.X <= 50 && currentPoint.X - e.Position.X >= -50))
                currentPoint = e.Position;

            if (e.Delta.Scale >= 1.1 || e.Delta.Scale <= .92)
                isFullScreenGesture = true;
        }

        void videoMediaElement_ManipulationStarted(object sender, ManipulationStartedRoutedEventArgs e)
        {
            isFullScreenGesture = false;
            initialPoint = e.Position;
            currentPoint = new Point();
        }

        void videoMediaElement_ManipulationInertiaStarting(object sender, ManipulationInertiaStartingEventHandler e)
        {
            if (initialPoint.X - currentPoint.X >= 50 && !isFullScreenGesture)
            {
                NextClip(0);
            }

            else if (initialPoint.X - currentPoint.X <= -50 && !isFullScreenGesture)
            {
                PreviousClip(null);
            }
        }

        async void save_myFile(string uri)
        {
            var localFolder = Windows.Storage.ApplicationData.Current.LocalFolder;
            try
            {
                Uri source = new Uri(uri);
                string destination = uri.Substring(uri.LastIndexOf('/')+1, uri.IndexOf('?') - uri.LastIndexOf('/')-1);

                StorageFile destinationFile = await localFolder.CreateFileAsync(destination, CreationCollisionOption.GenerateUniqueName);

                BackgroundDownloader downloader = new BackgroundDownloader();
                DownloadOperation download = downloader.CreateDownload(source, destinationFile);

                // Attach progress and completion handlers.
                HandleDownloadAsync(download, true);
            }
            catch (Exception)
            {
            }

        }

        private async void HandleDownloadAsync(DownloadOperation download, bool start)
        {
            try
            {
                // Store the download so we can pause/resume.

                Progress<DownloadOperation> progressCallback = new Progress<DownloadOperation>();
                if (start)
                {
                    // Start the download and attach a progress handler.
                    await download.StartAsync().AsTask(progressCallback);
                }
                else
                {
                    // The download was already running when the application started, re-attach the progress handler.
                    await download.AttachAsync().AsTask(progressCallback);
                }

                ResponseInformation response = download.GetResponseInformation();
            }
            catch (TaskCanceledException)
            {
            }
            catch (Exception)
            {
            }
        }

        public void GoBack()
        {
            saveAnglePreferences();
            navigationService.NavigateToViewModel<SectionViewModel>();
        }
    }
}
