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

using maddox.game;

namespace IL2DCE
{
    namespace Game
    {
        public class GameSingle : maddox.game.GameSingleDef, IGameSingle
        {
            public Core Core
            {
                get
                {
                    return _core;
                }
            }
            private Core _core;

            public EBattleResult BattleSuccess
            {
                get
                {
                    return _battleSuccess;
                }
                set
                {
                    _battleSuccess = value;
                }
            }
            EBattleResult _battleSuccess;

            public GameSingle(GameSingleIterface game)
                : base(game)
            {
                _core = new Core(this);
                _battleSuccess = EBattleResult.DRAW;
            }

            public override maddox.game.play.PageInterface getStartPage()
            {
                return new Pages.SelectCareerPage();
            }
        }
    }
}