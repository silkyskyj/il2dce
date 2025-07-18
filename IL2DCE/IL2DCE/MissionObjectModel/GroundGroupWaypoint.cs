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

using System.Collections.Generic;

using maddox.game;
using maddox.GP;

namespace IL2DCE.MissionObjectModel
{
    public abstract class GroundGroupWaypoint : GroupWaypoint
    {
        public abstract bool IsSubWaypoint(ISectionFile sectionFile, string id, int line);

        #region Public properties

        public List<GroundGroupWaypoint> SubWaypoints
        {
            get;
            private set;
        }

        public Point2d Position
        {
            get
            {
                return new Point2d(X, Y);
            }
        }

        public double? V
        {
            get;
            set;
        }

        public double Count
        {
            get
            {
                return SubWaypoints.Count + 2;
            }
        }

        #endregion

        public GroundGroupWaypoint()
        {
            SubWaypoints = new List<GroundGroupWaypoint>();
        }
    }
}