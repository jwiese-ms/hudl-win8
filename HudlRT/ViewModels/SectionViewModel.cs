﻿using Caliburn.Micro;
using HudlRT.Common;
using HudlRT.Models;
using HudlRT.Parameters;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.ApplicationSettings;
using Windows.UI.Xaml.Controls;

namespace HudlRT.ViewModels
{
    public class SectionViewModel : ViewModelBase
    {
        private readonly INavigationService navigationService;
        public CachedParameter Parameter { get; set; }

        private BindableCollection<GameViewModel> _schedule { get; set; }
        public BindableCollection<GameViewModel> Schedule
        {
            get { return _schedule; }
            set
            {
                _schedule = value;
                NotifyOfPropertyChange(() => Schedule);
            }
        }

        private BindableCollection<CategoryViewModel> _categories { get; set; }
        public BindableCollection<CategoryViewModel> Categories
        {
            get { return _categories; }
            set
            {
                _categories = value;
                NotifyOfPropertyChange(() => Categories);
            }
        }
        
        private BindableCollection<CutupViewModel> _cutups { get; set; }
        public BindableCollection<CutupViewModel> Cutups
        {
            get { return _cutups; }
            set
            {
                _cutups = value;
                NotifyOfPropertyChange(() => Cutups);
            }
        }

        // Maps to the selected game in the game list
        private GameViewModel selectedGame;
        public GameViewModel SelectedGame
        {
            get { return selectedGame; }
            set
            {
                selectedGame = value;
                NotifyOfPropertyChange(() => SelectedGame);
            }
        }

        private CategoryViewModel selectedCategory;
        public CategoryViewModel SelectedCategory
        {
            get { return selectedCategory; }
            set
            {
                selectedCategory = value;
                NotifyOfPropertyChange(() => SelectedCategory);
            }
        }

        private Season selectedSeason;
        public Season SelectedSeason
        {
            get { return selectedSeason; }
            set
            {
                selectedSeason = value;
                NotifyOfPropertyChange(() => SelectedSeason);
            }
        }

        private BindableCollection<Season> seasonsForDropDown;
        public BindableCollection<Season> SeasonsDropDown
        {
            get { return seasonsForDropDown; }
            set
            {
                seasonsForDropDown = value;
                NotifyOfPropertyChange(() => SeasonsDropDown);
            }
        }

        public SectionViewModel(INavigationService navigationService) : base(navigationService)
        {
            this.navigationService = navigationService;
            CharmsData.navigationService = navigationService;
            SettingsPane.GetForCurrentView().CommandsRequested += CharmsData.SettingCharmManager_HubCommandsRequested;
        }

        protected override void OnActivate()
        {
            base.OnActivate();

            // Get the team and season ID
            long teamID;
            long seasonID;
            try
            {
                TeamContextResponse response = AppDataAccessor.GetTeamContext();
                teamID = (long)response.teamID;
                seasonID = (long)response.seasonID;
            }
            catch (Exception ex)
            {
                teamID = 0;
                seasonID = 0;
            }

            if (Parameter != null)
            {
                SeasonsDropDown = Parameter.seasonsDropDown;
                SelectedSeason = Parameter.seasonSelected;
                Cutups = Parameter.sectionViewCutups;
                if (Parameter.categoryId != 0 && Parameter.gameId != 0)
                {
                    LoadPageFromParamter(SelectedSeason.seasonID, SelectedSeason.owningTeam.teamID, Parameter.gameId, Parameter.categoryId, Parameter.sectionViewGames);
                }
                else
                {
                    LoadPageFromDefault(SelectedSeason.seasonID, SelectedSeason.owningTeam.teamID, Parameter.sectionViewGames);
                }
            }
            if (Cutups != null)
            {
                var currentViewState = Windows.UI.ViewManagement.ApplicationView.Value;
                if (currentViewState == Windows.UI.ViewManagement.ApplicationViewState.Snapped)
                {
                    foreach (var cutup in Cutups)
                    {
                        cutup.Name_Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                        cutup.Thumbnail_Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                        cutup.Width = new Windows.UI.Xaml.GridLength(0);
                        cutup.FontSize = 24;
                    }
                }
                else
                {
                    foreach (var cutup in Cutups)
                    {
                        cutup.Name_Visibility = Windows.UI.Xaml.Visibility.Visible;
                        cutup.Thumbnail_Visibility = Windows.UI.Xaml.Visibility.Visible;
                        cutup.Width = new Windows.UI.Xaml.GridLength(180);
                        cutup.FontSize = 28;
                    }
                }
            }
        }

        private async void LoadPageFromParamter(long seasonID, long teamID, long gameID, long categoryID, BindableCollection<GameViewModel> games)
        {
            Cutups = null;
            if (games != null)
            {
                Schedule = games;
                foreach (var g in Schedule.ToList())
                {
                    g.TextColor = "#E0E0E0";
                }
            }
            else
            {
                await GetGames(teamID, seasonID);
            }

            // Make sure there are game entries for the season.
            if (Schedule.Any())
            {
                // Find the passed in game
                SelectedGame = Schedule.FirstOrDefault(game => game.GameId == gameID);

                // If the game isn't found set the first one as the default
                if (SelectedGame == null)
                {
                    SelectedGame = Schedule.FirstOrDefault();
                }
                await GetGameCategories(SelectedGame);

                // Make sure there are categories for the selected game
                if (Categories.Any())
                {
                    // Find the selected category
                    SelectedCategory = Categories.FirstOrDefault(cat => cat.CategoryId == categoryID);

                    // If the category isn't found set the first as the default
                    if (SelectedCategory == null)
                    {
                        SelectedCategory = Categories.FirstOrDefault();
                    }
                }
                else
                {
                    Categories = null;
                }
            }
            else
            {
                Schedule = null;
            }
        }

        private async void LoadPageFromDefault(long seasonID, long teamID, BindableCollection<GameViewModel> games)
        {
            Cutups = null;
            if (games != null)
            {
                Schedule = games;
                foreach (var g in Schedule.ToList())
                {
                    g.TextColor = "#E0E0E0";
                }
            }
            else
            {
                await GetGames(teamID, seasonID);
            }
            if (Schedule.Any())
            {
                if (Schedule.Contains(Parameter.sectionViewGameSelected))
                {
                    SelectedGame = Parameter.sectionViewGameSelected;
                    SelectedGame.TextColor = "#0099FF";
                    Categories = Parameter.sectionViewCategories;
                    SelectedCategory = Parameter.sectionViewCategorySelected;
                    Cutups = Parameter.sectionViewCutups;
                }
                else
                {
                    SelectedGame = Schedule.First();
                    await GetGameCategories(SelectedGame);
                    if (Categories.Any())
                    {
                        SelectedCategory = Categories.First();
                    }
                    else
                    {
                        Categories = null;
                    }
                }
            }
            else
            {
                Schedule = null;
            }
        }

        public async Task GetGames(long teamID, long seasonID)
        {
            GameResponse response = await ServiceAccessor.GetGames(teamID.ToString(), seasonID.ToString());
            if (response.status == SERVICE_RESPONSE.SUCCESS)
            {
                var schedule = new BindableCollection<GameViewModel>();
                foreach (Game game in response.games)
                {
                    schedule.Add(GameViewModel.FromGame(game));
                }
                Schedule = new BindableCollection<GameViewModel>();
                for (int i = schedule.Count() - 1; i >= 0; i--)
                {
                    Schedule.Add(schedule[i]);
                }
                //Parameter.sectionViewGames = Schedule;
            }
            /*else if (games.status == SERVICE_RESPONSE.NULL_RESPONSE)
            {
            }*/
            else
            {
                Schedule = null;
            }
        }

        public async Task GetGameCategories(GameViewModel game)
        {
            Categories = null;
            game.TextColor = "#0099FF";
            CategoryResponse response = await ServiceAccessor.GetGameCategories(game.GameId.ToString());
            if (response.status == SERVICE_RESPONSE.SUCCESS)
            {
                var cats = new BindableCollection<CategoryViewModel>();
                foreach (Category category in response.categories)
                {
                    cats.Add(CategoryViewModel.FromCategory(category));
                }
                Categories = cats;
            }
            else
            {
                Categories = null;
            }
        }

        public async Task GetCutupsByCategory(CategoryViewModel category)
        {
            Cutups = null;
            SelectedCategory.TextColor = "#0099FF";
            CutupResponse response = await ServiceAccessor.GetCategoryCutups(category.CategoryId.ToString());
            if (response.status == SERVICE_RESPONSE.SUCCESS)
            {
                var cuts = new BindableCollection<CutupViewModel>();
                foreach (Cutup cutup in response.cutups)
                {
                    cuts.Add(CutupViewModel.FromCutup(cutup));
                }
                Cutups = cuts;
            }
            var currentViewState = Windows.UI.ViewManagement.ApplicationView.Value;
            if (currentViewState == Windows.UI.ViewManagement.ApplicationViewState.Snapped)
            {
                foreach (var cutup in Cutups)
                {
                    cutup.Name_Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                    cutup.Thumbnail_Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                    cutup.Width = new Windows.UI.Xaml.GridLength(0);
                    cutup.FontSize = 24;
                }
            }

        }

        public async Task GetClipsByCutup(CutupViewModel cutup)
        {
            ClipResponse response = await ServiceAccessor.GetCutupClips(cutup);
            if (response.status == SERVICE_RESPONSE.SUCCESS)
            {
                cutup.Clips = response.clips;
                string[] clipCount = cutup.ClipCount.ToString().Split(' ');
                UpdateCachedParameter();
                Parameter.selectedCutup = new Cutup { cutupId = cutup.CutupId, clips = cutup.Clips, displayColumns = cutup.DisplayColumns, clipCount = Int32.Parse(clipCount[0]), name = cutup.Name };
                Parameter.sectionViewCutupSelected = cutup;
                //Parameter.videoPageClips = Parameter.selectedCutup.clips;
                navigationService.NavigateToViewModel<VideoPlayerViewModel>(Parameter);
            }
            else
            {
                Common.APIExceptionDialog.ShowExceptionDialog(null, null);
            }
        }

        public async void GameSelected(ItemClickEventArgs eventArgs)
        {
            if (Schedule != null)
            {
                var game = (GameViewModel)eventArgs.ClickedItem;
                SelectedGame = game;
                ListView x = (ListView)eventArgs.OriginalSource;
                x.SelectedItem = game;
                Cutups = null;
                foreach (var g in Schedule.ToList())
                {
                    g.TextColor = "#E0E0E0";
                }
                await GetGameCategories(game);

                if (Categories.Any())
                {
                    SelectedCategory = Categories.First();
                }
                else
                {
                    Categories = null;
                }
            }
        }

        public void CategorySelected(SelectionChangedEventArgs eventArgs)
        {
            if (Categories != null)
            {
                var category = (CategoryViewModel)eventArgs.AddedItems.FirstOrDefault();
                List<CategoryViewModel> categories = Categories.ToList();
                foreach (var cat in categories)
                {
                    cat.TextColor = "#E0E0E0";
                }

                SelectedCategory = category;
                GetCutupsByCategory(category);
            }
        }

        public async void CutupSelected(ItemClickEventArgs eventArgs)
        {
            var cutup = (CutupViewModel)eventArgs.ClickedItem;
            await GetClipsByCutup(cutup);
        }

        internal void SeasonSelected(object p)
        {
            Schedule = null;
            var selectedSeason = (Season)p;
            AppDataAccessor.SetTeamContext(selectedSeason.seasonID, selectedSeason.owningTeam.teamID);
            UpdateParameterOnSeasonChange();
            Categories = null;
            LoadPageFromDefault(selectedSeason.seasonID, selectedSeason.owningTeam.teamID, null);
        }

        public void GoBack()
        {
            UpdateCachedParameter();
            navigationService.NavigateToViewModel<HubViewModel>(Parameter);
        }

        public void LogOut()
        {
            navigationService.NavigateToViewModel<LoginViewModel>();
        }

        public void UpdateCachedParameter()
        {
            Parameter.seasonsDropDown = SeasonsDropDown;
            Parameter.seasonSelected = SelectedSeason;
            Parameter.sectionViewCutups = Cutups;
            Parameter.sectionViewCategorySelected = SelectedCategory;
            Parameter.sectionViewCategories = Categories;
            Parameter.sectionViewGames = Schedule;
            Parameter.sectionViewGameSelected = SelectedGame;
            Parameter.gameId = 0;
            Parameter.categoryId = 0;
        }

        public void UpdateParameterOnSeasonChange()
        {
            if (Parameter != null)
            {
                Parameter.hubViewNextGame = null;
                Parameter.hubViewPreviousGame = null;
            }
        }

        public void OnWindowSizeChanged()
        {
            if (Cutups != null)
            {
                var currentViewState = Windows.UI.ViewManagement.ApplicationView.Value;
                if (currentViewState == Windows.UI.ViewManagement.ApplicationViewState.Snapped)
                {
                    foreach (var cutup in Cutups)
                    {
                        cutup.Name_Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                        cutup.Thumbnail_Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                        cutup.Width = new Windows.UI.Xaml.GridLength(0);
                        cutup.FontSize = 24;
                    }
                }
                else
                {
                    foreach (var cutup in Cutups)
                    {
                        cutup.Name_Visibility = Windows.UI.Xaml.Visibility.Visible;
                        cutup.Thumbnail_Visibility = Windows.UI.Xaml.Visibility.Visible;
                        cutup.Width = new Windows.UI.Xaml.GridLength(180);
                        cutup.FontSize = 28;
                    }
                }
            }
        }


    }
}
