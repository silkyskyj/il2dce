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

using System.Diagnostics;

namespace IL2DCE
{
    namespace Mission
    {
        public class MissionSingle : Mission
        {
            protected override Core Core
            {
                get
                {
                    if (Game != null)
                    {
                        return Game.Core;
                    }

                    return null;
                }
            }

            new IGameSingle Game
            {
                get
                {
                    return GamePlay as IGameSingle;
                }
            }

            public MissionSingle()
            {
                Debug.WriteLine("MissionSingle.MissionSingle()");
            }

            public override void OnSingleBattleSuccess(bool success)
            {
                base.OnSingleBattleSuccess(success);
                if (Game != null)
                {
                    if (success == true)
                    {
                        Game.BattleResult = EBattleResult.SUCCESS;
                    }
                    else
                    {
                        Game.BattleResult = EBattleResult.FAIL;
                    }
                }
            }
        }
    }
}