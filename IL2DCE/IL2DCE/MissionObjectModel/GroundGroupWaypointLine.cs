﻿// IL2DCE: A dynamic campaign engine for IL-2 Sturmovik: Cliffs of Dover
// Copyright (C) 2016 Stefan Rothdach
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

using System.Globalization;
using System.Text.RegularExpressions;
using maddox.game;
using maddox.GP;

namespace IL2DCE.MissionObjectModel
{
    public class GroundGroupWaypointLine : GroundGroupWaypoint
    {

        #region Public properties

        public double Z
        {
            get;
            set;
        }

        #endregion

        // Example: 321223.63 175654.06 38.40  0 2 6.67
        // X, Y, Z, ?, SubCount+2, V
        private Regex waypointLong = new Regex(@"^([0-9]+[.0-9]*) ([0-9]+[.0-9]*)  ([0-9]+) ([0-9]+) ([0-9]+[.0-9]*)$");
        // Example: 321714.44 175710.25 38.40
        // X, Y, Z
        private Regex waypointShort = new Regex(@"^([0-9]+[.0-9]*) ([0-9]+[.0-9]*)$");

        public override bool IsSubWaypoint(ISectionFile sectionFile, string id, int line)
        {
            string key;
            string value;
            sectionFile.get(id + "_Road", line, out key, out value);

            if (waypointShort.IsMatch(value) && !waypointLong.IsMatch(value))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        #region Public constructors

        public GroundGroupWaypointLine(Point3d position, double? v)
        {
            X = position.x;
            Y = position.y;
            Z = position.z;
            V = v;
        }

        public GroundGroupWaypointLine(double x, double y, double z, double? v)
        {
            X = x;
            Y = y;
            Z = z;
            V = v;
        }

        public GroundGroupWaypointLine(ISectionFile sectionFile, string id, int line)
        {
            string key;
            string value;
            sectionFile.get(id + "_Road", line, out key, out value);

            if (waypointLong.IsMatch(value))
            {
                Match match = waypointLong.Match(value);

                if (match.Groups.Count == 6)
                {
                    double x;
                    if (double.TryParse(key, NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat, out x))
                    {
                        X = x;
                    }
                    double y;
                    if (double.TryParse(match.Groups[1].Value, NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat, out y))
                    {
                        Y = y;
                    }
                    double z;
                    if (double.TryParse(match.Groups[2].Value, NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat, out z))
                    {
                        Z = z;
                    }
                    double v;
                    if (double.TryParse(match.Groups[5].Value, NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat, out v))
                    {
                        V = v;
                    }
                }
            }
            else if (waypointShort.IsMatch(value))
            {
                Match match = waypointShort.Match(value);

                if (match.Groups.Count == 3)
                {
                    double x;
                    if (double.TryParse(key, NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat, out x))
                    {
                        X = x;
                    }
                    double y;
                    if (double.TryParse(match.Groups[1].Value, NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat, out y))
                    {
                        Y = y;
                    }
                    double z;
                    if (double.TryParse(match.Groups[2].Value, NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat, out z))
                    {
                        Z = z;
                    }
                    V = null;
                }
            }
        }

        #endregion
    }
}