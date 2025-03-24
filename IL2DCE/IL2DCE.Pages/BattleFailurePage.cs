﻿// IL2DCE: A dynamic campaign engine & dynamic mission for IL-2 Sturmovik: Cliffs of Dover Blitz + Desert Wings
// Copyright (C) 2016 Stefan Rothdach & 2025 silkyskyj
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as
// published by the Free Software Foundation, either version 3 of the
// License, or (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
//
// You should have received a copy of the GNU Affero General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.

using System.Windows;
using IL2DCE.MissionObjectModel;
using maddox.game.page;

namespace IL2DCE
{
    namespace Pages
    {
        public class BattleFailurePage : BattleResultPage
        {
            private CampaignBattleFailure FrameworkElement
            {
                get
                {
                    return FE as CampaignBattleFailure;
                }
            }

            public BattleFailurePage()
                : base("Battle Failure", new CampaignBattleFailure())
            {
                FrameworkElement.Fly.Click += new RoutedEventHandler(Fly_Click);
                FrameworkElement.ReFly.Click += new RoutedEventHandler(ReFly_Click);
                FrameworkElement.Back.Click += new RoutedEventHandler(Back_Click);

#if false       // 2025.02.22 < User Request
                FrameworkElement.Fly.IsEnabled = false;
                FrameworkElement.Fly.Visibility = Visibility.Hidden;
#endif
            }

            public override void _enter(maddox.game.IGame play, object arg)
            {
                base._enter(play, arg);

                string result = GetResultSummary() + GetPlayerStat();

                FrameworkElement.textBoxDescription.Text = result;
                if (Game.Core.CurrentCareer.BattleType == EBattleType.Campaign)
                {
                    FrameworkElement.textBoxSlide.Text = GetTotalPlayerStat();
                }
                else
                {
                    FrameworkElement.textBoxSlide.Visibility = Visibility.Hidden;
                }
            }
        }
    }
}