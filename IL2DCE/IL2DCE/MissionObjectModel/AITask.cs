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

namespace IL2DCE.MissionObjectModel
{
    internal class AITask
    {
    }

    enum EAirGroupAITask
    {
        Default = 0,
        RTB,
        LandingWays,
        AttackAirFighter,
        AttackAirBomber,
        AttackGround,
        Follow,
        Cover,
        Recon,
        Count,
    }

    internal class AirGroupAITask : AITask
    {

    }
}
