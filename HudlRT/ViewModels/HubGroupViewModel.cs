﻿using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HudlRT.ViewModels
{
    public class HubGroupViewModel : PropertyChangedBase
    {
        private string _name;
        public string Name
        {
            get { return _name; }
            set
            {
                if (value == _name) return;
                _name = value;
                NotifyOfPropertyChange(() => Name);
            }
        }

        private BindableCollection<GameViewModel> _games;

        public BindableCollection<GameViewModel> Games
        {
            get { return _games; }
            set
            {
                if (value == _games) return;
                _games = value;
                NotifyOfPropertyChange(() => Games);
            }
        }

    }
}
