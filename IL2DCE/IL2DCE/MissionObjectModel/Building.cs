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

using System;
using System.Globalization;
using maddox.game;
using maddox.GP;

namespace IL2DCE.MissionObjectModel
{
    public class Building
    {
        public string Id
        {
            get
            {
                return _id;
            }
        }
        private string _id;

        public double X;

        public double Y;

        public double Direction;

        public int Status;

        public string Class
        {
            get;
            set;
        }

        public Point2d Position
        {
            get
            {
                return new Point2d(this.X, this.Y);
            }
        }

        public Building(ISectionFile sectionFile, string id)
        {
            _id = id;

            string value = sectionFile.get("Buildings", id);

            string[] valueParts = value.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (valueParts.Length > 4)
            {
                Class = valueParts[0];
                int.TryParse(valueParts[1], NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat, out Status);
                double.TryParse(valueParts[2], NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat, out X);
                double.TryParse(valueParts[3], NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat, out Y);
                double.TryParse(valueParts[4], NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat, out Direction);
            }
        }

        public Building(string id, string @class, int status, double x, double y, double direction)
        {
            _id = id;
            Class = @class;
            Status = status;
            X = x;
            Y = y;
            Direction = direction;
        }

        public void WriteTo(ISectionFile sectionFile)
        {
            string value = Class + " " + Status.ToString(CultureInfo.InvariantCulture.NumberFormat) + " " + X.ToString(CultureInfo.InvariantCulture.NumberFormat) + " " + Y.ToString(CultureInfo.InvariantCulture.NumberFormat) + " " + Direction.ToString(CultureInfo.InvariantCulture.NumberFormat);
            sectionFile.add("Buildings", Id, value);
        }
    }
}