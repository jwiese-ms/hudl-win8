using Caliburn.Micro;
using HudlRT.Common;
using HudlRT.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.Networking.BackgroundTransfer;
using Windows.UI.ApplicationSettings;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace HudlRT.ViewModels
{
    public class SectionViewModel : ViewModelBase
    {
        INavigationService navigationService;
        public PageParameter Parameter { get; set; }       //Passed in from hub page - contains the game selected.
        public Game gameSelected { get; set; }
        private string _gameId;     //Used to tell if the page needs to be reloaded
        GridView categoriesGrid;
        List<Object> playlistsSelected;

        private Visibility _progressRingVisibility;
        public Visibility ProgressRingVisibility
        {
            get { return _progressRingVisibility; }
            set
            {
                _progressRingVisibility = value;
                NotifyOfPropertyChange(() => ProgressRingVisibility);
            }
        }

        private string _scheduleEntryName;
        public string ScheduleEntryName
        {
            get { return _scheduleEntryName; }
            set
            {
                _scheduleEntryName = value;
                NotifyOfPropertyChange(() => ScheduleEntryName);
            }
        }

        private bool _progressRingIsActive;
        public bool ProgressRingIsActive
        {
            get { return _progressRingIsActive; }
            set
            {
                _progressRingIsActive = value;
                NotifyOfPropertyChange(() => ProgressRingIsActive);
            }
        }

        private string diskSpaceInformation;
        public string DiskSpaceInformation
        {
            get { return diskSpaceInformation; }
            set
            {
                diskSpaceInformation = value;
                NotifyOfPropertyChange(() => DiskSpaceInformation);
            }
        }

        private string _noPlaylistText;
        public string NoPlaylistText
        {
            get { return _noPlaylistText; }
            set
            {
                _noPlaylistText = value;
                NotifyOfPropertyChange(() => NoPlaylistText);
            }
        }

        private bool _pageIsEnabled;
        public bool PageIsEnabled
        {
            get { return _pageIsEnabled; }
            set
            {
                _pageIsEnabled = value;
                NotifyOfPropertyChange(() => PageIsEnabled);
            }
        }

        private string downloadProgressText { get; set; }
        public string DownloadProgressText
        {
            get { return downloadProgressText; }
            set
            {
                downloadProgressText = value;
                NotifyOfPropertyChange(() => DownloadProgressText);
            }
        }

        private double downloadProgress;
        public double DownloadProgress
        {
            get { return downloadProgress; }
            set
            {
                downloadProgress = value;
                NotifyOfPropertyChange(() => DownloadProgress);
            }
        }

        private bool appBarOpen;
        public bool AppBarOpen
        {
            get { return appBarOpen; }
            set
            {
                appBarOpen = value;
                NotifyOfPropertyChange(() => AppBarOpen);
            }
        }

        private Visibility downloading_Visibility;
        public Visibility Downloading_Visibility
        {
            get { return downloading_Visibility; }
            set
            {
                downloading_Visibility = value;
                NotifyOfPropertyChange(() => Downloading_Visibility);
            }
        }

        private Visibility downloadButton_Visibility;
        public Visibility DownloadButton_Visibility
        {
            get { return downloadButton_Visibility; }
            set
            {
                downloadButton_Visibility = value;
                NotifyOfPropertyChange(() => DownloadButton_Visibility);
            }
        }

        private Visibility deleteButton_Visibility;
        public Visibility DeleteButton_Visibility
        {
            get { return deleteButton_Visibility; }
            set
            {
                deleteButton_Visibility = value;
                NotifyOfPropertyChange(() => DeleteButton_Visibility);
            }
        }

        private BindableCollection<CategoryViewModel> _categories;
        public BindableCollection<CategoryViewModel> Categories
        {
            get { return _categories; }
            set
            {
                _categories = value;
                NotifyOfPropertyChange(() => Categories);
            }
        }

        public SectionViewModel(INavigationService navigationService)
            : base(navigationService)
        {
            this.navigationService = navigationService;
            Categories = new BindableCollection<CategoryViewModel>();
        }

        public void UpdateDiskInformation()
        {
            DownloadAccessor.DiskSpaceResponse curentDownloadsSpaceReponse = DownloadAccessor.Instance.DiskSpaceFromDownloads;
            if (curentDownloadsSpaceReponse.formattedSize == "0 MB")
            {
                DiskSpaceInformation = "No playlists downloaded";
            }
            else
            {
                DiskSpaceInformation = "Using " + curentDownloadsSpaceReponse.formattedSize;
            }
        }

        protected override void OnActivate()
        {
            base.OnActivate();
            
            
            SettingsPane.GetForCurrentView().CommandsRequested += CharmsData.SettingCharmManager_HubCommandsRequested;
            //To insure the data shown is fetched if coming from the hub page to a new game
            //But that it doesn't fetch the data again if coming back from the video page.
            gameSelected = Parameter.season.games.FirstOrDefault();

            if (this.gameSelected.opponent.ToLower().Contains("practice") || this.gameSelected.opponent.ToLower().Contains("scrimmage") || this.gameSelected.opponent.ToLower().Contains("camp"))
            {
                ScheduleEntryName = this.gameSelected.opponent;
            }
            else 
            {
                ScheduleEntryName = "vs " + this.gameSelected.opponent;
            }

            PageIsEnabled = true;

            ProgressRingVisibility = Visibility.Collapsed;
            ProgressRingIsActive = false;

            //Categories count is checked against one due to us putting an empty category in.
            if (Categories.Count == 1 && (NoPlaylistText == "" || NoPlaylistText == null))
            {
                ProgressRingVisibility = Visibility.Visible;
                ProgressRingIsActive = true;
            }

            if (gameSelected.gameId != _gameId)
            {
                _gameId = gameSelected.gameId;
                GetGameCategories(_gameId);
            }
            DeleteButton_Visibility = Visibility.Collapsed;
            DownloadButton_Visibility = Visibility.Collapsed;
            Downloading_Visibility = Visibility.Collapsed;


            MarkDownloadedPlaylists();
            LoadActiveDownloadsAsync();
            UpdateDiskInformation();
            if (DownloadAccessor.Instance.Downloading)
            {
                Downloading_Visibility = Visibility.Visible;
                AppBarOpen = true;
            }
        }

        public async Task GetGameCategories(string gameID)
        {
            Categories = new BindableCollection<CategoryViewModel>();

            foreach (Category c in gameSelected.categories)
            {
                CategoryViewModel cat = new CategoryViewModel(c);
                foreach (Playlist p in c.playlists)
                {
                    PlaylistViewModel pvm = new PlaylistViewModel(p);
                    cat.Playlists.Add(pvm);
                    pvm.FetchClips = pvm.FetchClipsAndHeaders();
                }
                if (c.playlists != null && c.playlists.Count() != 0)
                {
                    Categories.Add(cat);
                }
            }

            ProgressRingVisibility = Visibility.Collapsed;
            ProgressRingIsActive = false;

            MarkDownloadedPlaylists();

            if (Categories.Count == 0)
            {
                NoPlaylistText = "There are no playlists for this entry";
            }
            else
            {
                NoPlaylistText = "";
            }
        }

        public async Task AddClipsAndHeadersForPlaylist(Playlist playlist)
        {
            if (ServiceAccessor.ConnectedToInternet())
            {
                playlist.clips = new BindableCollection<Clip>();
                ClipResponse response = await ServiceAccessor.GetPlaylistClipsAndHeaders(playlist.playlistId);
                if (response.status == SERVICE_RESPONSE.SUCCESS)
                {
                    playlist.clips = response.clips;
                    playlist.displayColumns = response.DisplayColumns;
                }
                else
                {
                }
            }
        }

        public async void PlaylistSelected(ItemClickEventArgs eventArgs)
        {
            ProgressRingIsActive = true;
            ProgressRingVisibility = Visibility.Visible;
            PageIsEnabled = false;
            PlaylistViewModel vmClicked = (PlaylistViewModel)eventArgs.ClickedItem;
            Playlist playlistClicked = vmClicked.PlaylistModel;
            Playlist matchingDownload = DownloadAccessor.Instance.downloadedPlaylists.Where(u => u.playlistId == playlistClicked.playlistId).FirstOrDefault();
            if (matchingDownload != null)
            {
                navigationService.NavigateToViewModel<VideoPlayerViewModel>(new PageParameter{ playlist= matchingDownload, hubGroups = Parameter.hubGroups, season = Parameter.season});
            }
            else
            {
                await vmClicked.FetchClips;
                navigationService.NavigateToViewModel<VideoPlayerViewModel>(new PageParameter { playlist = playlistClicked, hubGroups = Parameter.hubGroups, season = Parameter.season });
            }
            Logger.Instance.LogPlaylistSelected(((PlaylistViewModel)eventArgs.ClickedItem).PlaylistModel);
        }

        public async void DeleteButtonClick()
        {
            foreach (PlaylistViewModel playVM in playlistsSelected)
            {
                await DownloadAccessor.Instance.RemoveDownload(playVM.PlaylistModel);
                Logger.Instance.LogPlaylistDownloadRemoved(playVM.PlaylistModel);
            }
            MarkDownloadedPlaylists();
            if(categoriesGrid != null)
            {
                categoriesGrid.SelectedItem = null;
            }
            
            DeleteButton_Visibility = Visibility.Collapsed;
            if (DownloadAccessor.Instance.Downloading)
            {
                AppBarOpen = true;
            }
            else
            {
                AppBarOpen = false;
            }
            UpdateDiskInformation();

        }

        public void CancelButtonClick()
        {
            DownloadProgressText = "Canceling Download";
            if (DownloadAccessor.Instance.Downloading)
            {
                DownloadAccessor.Instance.cts.Cancel();
            }
            if (categoriesGrid != null)
            {
                categoriesGrid.SelectedItem = null;
            }
            AppBarOpen = false;
            UpdateDiskInformation();
            Downloading_Visibility = Visibility.Collapsed;
        }
            
        public async void DownloadButtonClick()
        {
            List<Playlist> playlistsToBeDownloaded = new List<Playlist>();
            foreach (PlaylistViewModel playVM in playlistsSelected)
            {
                if(playVM.PlaylistModel.clips.Count == 0)
                {
                    ClipResponse response = await ServiceAccessor.GetPlaylistClipsAndHeaders(playVM.PlaylistModel.playlistId);
                    playVM.PlaylistModel.clips = response.clips;
                    playVM.PlaylistModel.displayColumns = response.DisplayColumns;
                }
                List<Clip> additionalClips = await ServiceAccessor.GetAdditionalPlaylistClips(playVM.PlaylistModel.playlistId, playVM.PlaylistModel.clips.Count);
                foreach (Clip c in additionalClips)
                {
                    playVM.PlaylistModel.clips.Add(c);
                }
                playlistsToBeDownloaded.Add(playVM.PlaylistModel);
            }
            DownloadButton_Visibility = Visibility.Collapsed;
            Downloading_Visibility = Visibility.Visible;
            DownloadProgressText = "Preparing Download";
            DownloadProgress = 0;
            DownloadAccessor.Instance.cts = new CancellationTokenSource();
            DownloadAccessor.Instance.currentlyDownloadingPlaylists = playlistsToBeDownloaded;
            DownloadAccessor.Instance.progressCallback = new Progress<DownloadOperation>(ProgressCallback);
            DownloadAccessor.Instance.DownloadPlaylists(playlistsToBeDownloaded, Season.DeepCopy(Parameter.season));

        }

        private void CategoriesGridView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            categoriesGrid = (GridView)sender;
            PlaylistViewModel playlistAdded = (PlaylistViewModel)e.AddedItems.FirstOrDefault();
            if (DownloadAccessor.Instance.Downloading)
            {
                if (categoriesGrid.SelectedItems.Count >= 1)
                {
                    if (playlistAdded != null)
                    {
                        if (playlistAdded.DownloadedIcon_Visibility == Visibility.Visible)
                        {
                            DeleteButton_Visibility = Visibility.Visible;
                            playlistsSelected = categoriesGrid.SelectedItems.ToList();
                        }
                        else
                        {
                            categoriesGrid.SelectedItems.Remove(e.AddedItems[0]);
                        }
                    }
                }
                AppBarOpen = true;
            }
            else
            {

                if (categoriesGrid.SelectedItems.Count == 0)
                {
                    DownloadButton_Visibility = Visibility.Collapsed;
                    Downloading_Visibility = Visibility.Collapsed;
                    DeleteButton_Visibility = Visibility.Collapsed;
                }

                if (categoriesGrid.SelectedItems.Count == 1 && playlistAdded != null)
                {
                    if (playlistAdded.DownloadedIcon_Visibility == Visibility.Visible)
                    {
                        DownloadButton_Visibility = Visibility.Collapsed;
                        Downloading_Visibility = Visibility.Collapsed;
                        DeleteButton_Visibility = Visibility.Visible;

                    }
                    else
                    {
                        DownloadButton_Visibility = Visibility.Visible;
                        Downloading_Visibility = Visibility.Collapsed;
                        DeleteButton_Visibility = Visibility.Collapsed;
                    }
                }

                if (categoriesGrid.SelectedItems.Count > 1)
                {
                    PlaylistViewModel firstPlaylist = (PlaylistViewModel)playlistsSelected.ElementAt(0);
                    if (playlistAdded != null)
                    {
                        if (playlistAdded.DownloadedIcon_Visibility != firstPlaylist.DownloadedIcon_Visibility)
                        {
                            categoriesGrid.SelectedItems.Remove(e.AddedItems[0]);
                        }
                    }
                }
                playlistsSelected = categoriesGrid.SelectedItems.ToList();
                AppBarOpen = playlistsSelected.Any() ? true : false;
            }
            
        }

        private async Task LoadActiveDownloadsAsync()
        {
            if(DownloadAccessor.Instance.Downloading)
            {
                await ResumeDownloadAsync();
            }
        }

        private async Task ResumeDownloadAsync()
        {
            DownloadAccessor.Instance.progressCallback = new Progress<DownloadOperation>(ProgressCallback);
            await DownloadAccessor.Instance.Download.AttachAsync().AsTask(DownloadAccessor.Instance.progressCallback);
        }

        private void MarkDownloadedPlaylists()
        {
            if (Categories != null)
            {
                foreach (CategoryViewModel cat in Categories)
                {
                    if (cat.Playlists != null)
                    {
                        foreach (PlaylistViewModel pl in cat.Playlists)
                        {
                            bool downloadFound = DownloadAccessor.Instance.downloadedPlaylists.Any(play => play.playlistId == pl.PlaylistModel.playlistId);
                            if (downloadFound)
                            {
                                pl.DownloadedIcon_Visibility = Visibility.Visible;
                            }
                            else
                            {
                                pl.DownloadedIcon_Visibility = Visibility.Collapsed;
                            }
                        }
                    }

                }
            }
        }

        public void ProgressCallback(DownloadOperation obj)
        {
            UpdateDiskInformation();
            DownloadProgress = 100.0 * (((long)obj.Progress.BytesReceived + DownloadAccessor.Instance.CurrentDownloadedBytes) / (double)DownloadAccessor.Instance.TotalBytes);
            DownloadProgressText = DownloadAccessor.Instance.ClipsComplete + " / " + DownloadAccessor.Instance.TotalClips + " File(s)";
            if (DownloadProgress == 100)
            {
                if (Categories != null)
                {
                    foreach (CategoryViewModel cat in Categories)
                    {
                        if (cat.Playlists != null)
                        {
                            foreach (PlaylistViewModel pl in cat.Playlists)
                            {
                                bool downloadFound = DownloadAccessor.Instance.downloadedPlaylists.Any(play => play.playlistId == pl.PlaylistModel.playlistId);
                                bool currentlyDownloadingFound = DownloadAccessor.Instance.currentlyDownloadingPlaylists.Any(play => play.playlistId == pl.PlaylistModel.playlistId);
                                if (downloadFound || currentlyDownloadingFound)
                                {
                                    pl.DownloadedIcon_Visibility = Visibility.Visible;
                                }
                            }
                        }
                    }
                }
                if (categoriesGrid != null)
                {
                    categoriesGrid.SelectedItem = null;
                }
                DownloadAccessor.Instance.currentlyDownloadingPlaylists = new List<Playlist>();
                DownloadProgressText = "";
                DownloadProgress = 0;
                Downloading_Visibility = Visibility.Collapsed;
                UpdateDiskInformation();
                AppBarOpen = false;
            }
        }

        public void GoBack()
        {
            navigationService.GoBack();
        }
    }   
}
