﻿using Caliburn.Micro;
using HudlRT.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HudlRT.Models
{
    public class Model
    {
        public BindableCollection<Team> teams { get; set; }

        public Model()
        {
            teams = new BindableCollection<Team>();
        }
    }

    public class SectionModel
    {
        public BindableCollection<Game> games { get; set; }

        public SectionModel()
        {
            games = new BindableCollection<Game>();
        }
    }

    public class Team
    {
        public string school { get; set; }
        public string name { get; set; }
        public long teamID { get; set; }
        public BindableCollection<Season> seasons { get; set; }

        public Team ()
        {
            seasons = new BindableCollection<Season>();
        }

        public static Team FromDTO(TeamDTO teamDTO)
        {
            Team team = new Team();
            team.name = teamDTO.Name;
            team.school = teamDTO.School.Name;
            team.teamID = teamDTO.TeamId;
            foreach (SeasonDTO seasonDTO in teamDTO.Seasons)
            {
                team.seasons.Add(Season.FromDTO(seasonDTO, team));
            }
            return team;
        }
    }

    public class Season
    {
        public string name { get; set; }
        public string FullName 
        { 
            get 
            { 
                return owningTeam.school + " - " + owningTeam.name + " - " + name; 
            }  
        }

        public long seasonID { get; set; }
        public int year { get; set; }
        public BindableCollection<Game> games { get; set; }
        public Team owningTeam { get; set; }

        public Season()
        {
            games = new BindableCollection<Game>();
        }

        public static Season FromDTO(SeasonDTO seasonDTO, Team team)
        {
            Season s = new Season();
            s.owningTeam = team;
            s.name = seasonDTO.Name;
            s.seasonID = seasonDTO.SeasonId;
            s.year = seasonDTO.Year;
            return s;
        }
    }

    public class Game
    {
        public string opponent { get; set; }
        public DateTime date { get; set; }
        public bool isHome { get; set; }
        public BindableCollection<Category> categories { get; set; }
        public long gameId { get; set; }

        public Game()
        {
            categories = new BindableCollection<Category>();
        }
        
        public static Game FromDTO(GameDTO gameDTO)
        {
            Game game = new Game();
            game.gameId = gameDTO.GameId;
            game.isHome = gameDTO.Ishome;
            game.opponent = gameDTO.Opponent;
            game.date = gameDTO.Date;
            return game;
        }
    }

    public class Category
    {
        public string name { get; set; }
        public BindableCollection<Cutup> cutups { get; set; }
        public long categoryId { get; set; }

        public Category()
        {
            cutups = new BindableCollection<Cutup>();
        }

        public static Category FromDTO(CategoryDTO categoryDTO)
        {
            Category category = new Category();
            category.categoryId = categoryDTO.CategoryId;
            category.name = categoryDTO.Name;
            return category;
        }
    }

    public class Cutup
    {
        public string name { get; set; }
        public int clipCount { get; set; }
        public long cutupId { get; set; }
        public BindableCollection<Clip> clips { get; set; }
        public string[] displayColumns { get; set; }
        public Cutup()
        {
            clips = new BindableCollection<Clip>();
        }

        public static Cutup FromDTO(CutupDTO cutupDTO)
        {
            Cutup cutup = new Cutup();
            cutup.cutupId = cutupDTO.CutupID;
            cutup.clipCount = cutupDTO.ClipCount;
            cutup.name = cutupDTO.Name;
            return cutup;
        }
    }

    public class Clip
    {
        public long clipId { get; set; }
        public long order { get; set; }
        public BindableCollection<Angle> angles { get; set; }
        //public Dictionary<string, string> breakdownData { get; set; }
        public string[] breakDownData { get; set; }




        public Clip()
        {
            //breakdownData = new Dictionary<string, string>();
            angles = new BindableCollection<Angle>();
        }

        public static Clip FromDTO(ClipDTO clipDTO, string[] displayColumns)
        {
            Clip clip = new Clip();
            clip.breakDownData = new string[displayColumns.Length];
            clip.clipId = clipDTO.ClipID;
            clip.order = clipDTO.OriginalOrder;
            for (int i = 0; i < clip.breakDownData.Length; i++)
            {
                clip.breakDownData[i] = "-";
            }
            foreach(string key in clipDTO.breakdownData.Keys)
            {
                for (int i = 0; i < displayColumns.Length; i++)
                {
                    if (key.ToUpper() == displayColumns[i].ToUpper())
                    {
                        clip.breakDownData[i] = clipDTO.breakdownData[key];
                    }
                }
            }
            //clip.breakdownData = upperedBDD;
            foreach (AngleDTO angleDTO in clipDTO.Angles)
            {
                Angle a = Angle.FromDTO(angleDTO);
                if (a != null)
                {
                    clip.angles.Add(a);
                }
                else
                {
                    return null;
                }
            }
            return clip;
        }
    }

    public class Angle
    {
        public long clipAngleId { get; set; }
        public string angleName { get; set; }
        public string fileLocation { get; set; }
        public string thumbnailLocation { get; set; }
        public long duration { get; set; }
        public AngleType angleType { get; set; }

        public Angle()
        {

        }
        public Angle(string fileLocation)
        {
            this.fileLocation = fileLocation;
        }

        public static Angle FromDTO(AngleDTO angleDTO)
        {
            if (angleDTO.Files.Count == 0)
            {
                return null;
            }
            else
            {
                Angle angle = new Angle();
                angle.angleName = angleDTO.AngleName;
                angle.clipAngleId = angleDTO.ClipAngleID;
                angle.duration = angleDTO.Duration;
                if (angleDTO.Files.FirstOrDefault() != null)
                {
                    angle.fileLocation = angleDTO.Files.FirstOrDefault().FileName;//throws error if there is no filename
                }
                else
                {
                    return null;
                }
                angle.thumbnailLocation = angleDTO.LargeThumbnailFileName;
                return angle;
            }
        }
    }

    public class BreakdownData
    {
        public float dist { get; set; }
        public long play { get; set; }//needs to be changed
        public long qtr { get; set; }
    }

    public class AngleType : PropertyChangedBase
    {
        public VideoPlayerViewModel viewModel { get; set; }
        public string Name { get; set; }
        private bool isChecked;
        public bool IsChecked
        {
            get { return isChecked; }
            set
            {
                isChecked = value;
                NotifyOfPropertyChange(() => IsChecked);
            }
        }

        public AngleType(string name, VideoPlayerViewModel vm)
        {
            Name = name;
            IsChecked = true;
            viewModel = vm;
        }

        public void checkBox(bool isChecked, Windows.UI.Xaml.RoutedEventArgs eventArgs)
        {
            IsChecked = isChecked;
            viewModel.angleFilter();
        }
    }

    public enum PlaybackType
    {
        once,
        loop,
        next
    }

    public enum NextAngleEvent
    {
        buttonClick,
        mediaEnded
    }
}
