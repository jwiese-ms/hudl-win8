﻿using Caliburn.Micro;
using HudlRT.Models;
using HudlRT.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HudlRT.Parameters
{
    public static class CachedParameter
    {
        public static bool isInitialized = false;
        public static string categoryId { get; set; }
        public static string gameId { get; set; }
        public static BindableCollection<Season> seasonsDropDown { get; set; }
        public static Season seasonSelected { get; set; }
        public static Game hubViewNextGame { get; set; }
        public static Game hubViewPreviousGame { get; set; }
                
        public static BindableCollection<GameViewModel> sectionViewGames { get; set; }
        public static GameViewModel sectionViewGameSelected { get; set; }
                
        public static BindableCollection<CategoryViewModel> sectionViewCategories { get; set; }
        public static CategoryViewModel sectionViewCategorySelected { get; set; }
                
        public static BindableCollection<CutupViewModel> sectionViewCutups { get; set; }
                
        public static CutupViewModel sectionViewCutupSelected { get; set; }
                
        public static BindableCollection<Clip> videoPageClips { get; set; }
                
        public static Cutup selectedCutup { get; set; }


        public static void InitializeForFrontend(){
            isInitialized= false;
            categoryId = null;
            hubViewNextGame = new Game()
            {
                categories = new BindableCollection<Category>()
                {
                    new Category(){categoryId = "4102838", cutups = new BindableCollection<Cutup>(), name ="Game Footage"},
                    new Category(){categoryId = "4102839", cutups = new BindableCollection<Cutup>(), name ="Practice"},
                    new Category(){categoryId = "4102840", cutups = new BindableCollection<Cutup>(), name ="Opponent Scout"}
                },
                date = new DateTime(634865184000000000),
                gameId = "881301",
                isHome = true,
                opponent = "Wichita Southeast"
            };

            hubViewPreviousGame = new Game()
            {
                categories = new BindableCollection<Category>()
                {
                    new Category(){categoryId = "4102818", cutups = new BindableCollection<Cutup>(), name ="Game Footage"},
                    new Category(){categoryId = "4102819", cutups = new BindableCollection<Cutup>(), name ="Practice"},
                    new Category(){categoryId = "4102820", cutups = new BindableCollection<Cutup>(), name ="Opponent Scout"}
                },
                date = new DateTime(634859136000000000),
                gameId = "881296",
                isHome = true,
                opponent = "Wichita east"
            };
            Team team = new Team()
            {
                name = "Varsity Football",
                school = "Wichita Heights High School",
                seasons = new BindableCollection<Season>(),
                teamID = "7"
            };

            seasonsDropDown = new BindableCollection<Season>()
            {
                new Season(){name = "2012 - 2013", games = new BindableCollection<Game>(), owningTeam = team, seasonID = "57083", year = 2012},
                new Season(){name = "2011 - 2012", games = new BindableCollection<Game>(), owningTeam = team, seasonID = "16874", year = 2011},
                new Season(){name = "2010 - 2011", games = new BindableCollection<Game>(), owningTeam = team, seasonID = "4898", year = 2010},
            };
            seasonSelected = seasonsDropDown.First();
        }

        public static void resetCache(){
            isInitialized = false;
            categoryId = null;
            gameId = null;
            seasonsDropDown = null;
            seasonSelected = null;
            hubViewNextGame = null;
            hubViewPreviousGame = null;
            sectionViewGames = null;
            sectionViewGameSelected = null;
            sectionViewCategories = null;
            sectionViewCategorySelected = null;
            sectionViewCutups = null;
            sectionViewCutupSelected = null;
            videoPageClips = null;
            selectedCutup = null;

        }
    }
}
