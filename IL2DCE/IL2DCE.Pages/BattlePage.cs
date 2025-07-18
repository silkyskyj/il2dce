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

using maddox.game.page;
using maddox.game.play;

namespace IL2DCE
{
    namespace Pages
    {
        public class BattlePage : PageDefImpl
        {
            private IGame Game
            {
                get
                {
                    return _game;
                }
            }
            private IGame _game;

            public BattlePage()
                : base("Battle", new CampaignBattleIntro())
            {
            }

            public override void _enter(maddox.game.IGame play, object arg)
            {
                base._enter(play, arg);

                _game = play as IGame;

                if (Game is IGameSingle)
                {
                    IGameSingle gameSingle = Game as IGameSingle;

                    if (gameSingle.BattleResult == EBattleResult.NONE)
                    {
                        gameSingle.BattleResult = EBattleResult.DRAW;

                        string missionFileName = Game.Core.CurrentCareer.MissionFileName;
                        if (missionFileName != null)
                        {
                            PageInterface page = Game.gameInterface.PageGet("SingleMissGame");
                            Game.gameInterface.PagePush(page, "mission " + missionFileName);
                        }
                    }
                    else
                    {
                        if (gameSingle.BattleResult == EBattleResult.SUCCESS)
                        {
                            Game.gameInterface.PageChange(new BattleSuccessPage(), null);
                        }
                        else if (gameSingle.BattleResult == EBattleResult.DRAW)
                        {
                            Game.gameInterface.PageChange(new BattleSuccessPage(), null);
                        }
                        else if (gameSingle.BattleResult == EBattleResult.FAIL)
                        {
                            Game.gameInterface.PageChange(new BattleFailurePage(), null);
                        }
                        else
                        {
                            Game.gameInterface.PageChange(new BattleFailurePage(), null);
                        }
                    }
                }
            }

            public override void _leave(maddox.game.IGame play, object arg)
            {
                base._leave(play, arg);

                _game = null;
            }
        }
    }
}