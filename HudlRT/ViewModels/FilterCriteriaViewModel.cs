﻿using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HudlRT.ViewModels
{
    public class FilterCriteriaViewModel : PropertyChangedBase
    {
        public string Name { get; set; }
        private int id { get; set; }
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

        public FilterCriteriaViewModel(int id, string name)
        {
            this.id = id;
            Name = name;
            IsChecked = false;
        }
    }
}