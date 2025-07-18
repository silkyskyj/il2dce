﻿// IL2DCE: A dynamic campaign engine & quick mission for IL-2 Sturmovik: Cliffs of Dover Blitz + DLC
// Copyright (C) 2016 Stefan Rothdach & 2025 silkysky
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
using maddox.game.play;

namespace IL2DCE
{
    namespace Pages
    {
        public class BattleIntroPage : PageDefImpl
        {
            private CampaignBattleIntro FrameworkElement
            {
                get
                {
                    return FE as CampaignBattleIntro;
                }
            }

            private IGame Game
            {
                get
                {
                    return _game;
                }
            }
            private IGame _game;
 
            public BattleIntroPage()
                : base("Battle Intro", new CampaignBattleIntro())
            {
                FrameworkElement.Fly.Click += new RoutedEventHandler(Fly_Click);
                FrameworkElement.Back.Click += new RoutedEventHandler(Back_Click);
            }

            public override void _enter(maddox.game.IGame play, object arg)
            {
                base._enter(play, arg);

                _game = play as IGame;

                // TODO:                
                // Skip this page
                if (Game is IGameSingle)
                {
                    IGameSingle gameSingle = Game as IGameSingle;
                    gameSingle.BattleResult = EBattleResult.NONE;
                }

                Game.gameInterface.PageChange(new BattlePage(), null);
            }

            public override void _leave(maddox.game.IGame play, object arg)
            {
                base._leave(play, arg);

                _game = null;
            }

            void Back_Click(object sender, RoutedEventArgs e)
            {
                Career career = Game.Core.CurrentCareer;
                if (career.BattleType == EBattleType.QuickMission)
                {
                    Game.gameInterface.PageChange(new QuickMissionPage(), null);
                }
                else if (career.BattleType == EBattleType.Campaign)
                {
                    Game.gameInterface.PageChange(new SelectCareerPage(), null);
                }
                else
                {
                    Game.gameInterface.PagePop(null);
                }
            }

            void Fly_Click(object sender, RoutedEventArgs e)
            {
                if (Game is IGameSingle)
                {
                    IGameSingle gameSingle = Game as IGameSingle;
                    gameSingle.BattleResult = EBattleResult.DRAW;
                }

                Game.gameInterface.PageChange(new BattlePage(), null);
            }
        }
    }
}