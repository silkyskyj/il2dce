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

using System.Collections.Generic;
using maddox.GP;
using XLAND;

namespace IL2DCE.Util
{

    public class MapUtil
    {
        public static void Inflate(ref wRECTF rect, float value)
        {
            rect.x1 -= value;
            rect.x2 += value;
            rect.y1 -= value;
            rect.y2 += value;
        }

        public static void InflateRate(ref wRECTF rect, float rate)
        {
            float w = (rect.x2 - rect.x1 + 1) * (rate - 1) / 2;
            float h = (rect.y2 - rect.y1 + 1) * (rate - 1) / 2;
            rect.x1 -= w;
            rect.x2 += w;
            rect.y1 -= h;
            rect.y2 += h;
        }

        public static bool IsInRange(ref wRECTF rect, ref Point3d point)
        {
            return point.x >= rect.x1 && point.x <= rect.x2 && point.y >= rect.y1 && point.y <= rect.y2;
        }

        public static wRECTF GetRange(IEnumerable<Point3d> points)
        {
#if false
            wRECTF range = new wRECTF();
            range.x1 = (float)points.Min(x => x.x);
            range.x2 = (float)points.Max(x => x.x);
            range.y1 = (float)points.Min(x => x.y);
            range.y2 = (float)points.Max(x => x.y);
#else
            wRECTF range = new wRECTF() { x1 = float.MaxValue, x2 = float.MinValue, y1 = float.MaxValue, y2 = float.MinValue };
            foreach (var item in points)
            {
                if (item.x < range.x1)
                {
                    range.x1 = (float)item.x;
                }
                if (item.x > range.x2)
                {
                    range.x2 = (float)item.x;
                }
                if (item.y < range.y1)
                {
                    range.y1 = (float)item.y;
                }
                if (item.y > range.y2)
                {
                    range.y2 = (float)item.y;
                }
            }
#endif
            return range; 
        }

    }
}
