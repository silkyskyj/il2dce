﻿// IL2DCE: A dynamic campaign engine for IL-2 Sturmovik: Cliffs of Dover Blitz + Desert Wings
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
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using IL2DCE.Util;
using maddox.game;

namespace IL2DCE.Generator
{
#if false
    interface IAirGroupInfo
    {
        #region Public properties

        List<string> Aircrafts
        {
            get;
        }

        List<string> AirGroupKeys
        {
            get;
        }

        int SquadronCount
        {
            get;
        }

        int FlightCount
        {
            get;
        }

        int FlightSize
        {
            get;
        }

        int AircraftMaxCount
        {
            get;
        }

        int ArmyIndex
        {
            get;
        }

        int AirForceIndex
        {
            get;
        }

        #endregion
    }
#endif

    public class AirGroupInfo/* : IAirGroupInfo*/
    {
        public const string FileInfo = "AirGroupinfo";

        public const string SectionMain = "Main";
        public const string SectionAircrafts = "Aircrafts";
        public const string SectionAirGroupKeys = "AirGroupKeys";

        public const string KeySquadronCount = "SquadronCount";
        public const string KeyFlightCount = "FlightCount";
        public const string KeyFlightSize = "FlightSize";
        public const string KeyArmyIndex = "ArmyIndex";
        public const string KeyAirForceIndex = "AirForceIndex";

        #region Public properties

        public string Name
        {
            get;
            set;
        }

        public List<string> Aircrafts
        {
            get;
            set;
        }

        public List<string> AirGroupKeys
        {
            get;
            set;
        }

        public int SquadronCount
        {
            get;
            set;
        }

        public int FlightCount
        {
            get;
            set;
        }

        public int FlightSize
        {
            get;
            set;
        }

        public int AircraftMaxCount
        {
            get
            {
                return FlightCount * FlightSize;
            }
        }

        public int ArmyIndex
        {
            get;
            set;
        }

        public int AirForceIndex
        {
            get;
            set;
        }

        #endregion

        public bool Read(ISectionFile file)
        {
            return true;
        }

        public void Write(ISectionFile file, string airGroupKey = null, string aircraftClass = null)
        {
            SectionFileUtil.Write(file, Name, KeySquadronCount, SquadronCount.ToString(), false);
            SectionFileUtil.Write(file, Name, KeyFlightCount, FlightCount.ToString(), false);
            SectionFileUtil.Write(file, Name, KeyFlightSize, FlightSize.ToString(), false);
            SectionFileUtil.Write(file, Name, KeyArmyIndex, ArmyIndex.ToString(), false);
            SectionFileUtil.Write(file, Name, KeyAirForceIndex, AirForceIndex.ToString(), false);
            if (string.IsNullOrEmpty(aircraftClass))
            {
                Aircrafts.ForEach(x => SectionFileUtil.Write(file, string.Format("{0}.{1}", Name, SectionAircrafts), x, string.Empty, false)); // All
            }
            else
            {
                foreach (var item in Aircrafts.Where(x => string.Compare(x, aircraftClass, true) == 0))
                {
                    SectionFileUtil.Write(file, string.Format("{0}.{1}", Name, SectionAircrafts), item, string.Empty, false);
                }
            }
            if (string.IsNullOrEmpty(airGroupKey))
            {
                AirGroupKeys.ForEach(x => SectionFileUtil.Write(file, string.Format("{0}.{1}", Name, SectionAirGroupKeys), x, string.Empty, false)); // All
            }
            else
            {
                foreach (var item in AirGroupKeys.Where(x => string.Compare(x, airGroupKey, true) == 0))
                {
                    SectionFileUtil.Write(file, string.Format("{0}.{1}", Name, SectionAirGroupKeys), item, string.Empty, false);
                }
            }
        }

        public static AirGroupInfo Create(ISectionFile file, string section, string secAircrafts, string secAirGroupKeys)
        {
            if (file.exist(section) && file.exist(secAircrafts) && file.exist(secAirGroupKeys))
            {
                string key;
                string value;

                // Aircraft
                List<string> aircrafts = new List<string>();
                int lines = file.lines(secAircrafts);
                for (int j = 0; j < lines; j++)
                {
                    file.get(secAircrafts, j, out key, out value);
                    aircrafts.Add(key);
                }

                // AirGroup
                List<string> airGroupKeys = new List<string>();
                lines = file.lines(secAirGroupKeys);
                for (int j = 0; j < lines; j++)
                {
                    file.get(secAirGroupKeys, j, out key, out value);
                    airGroupKeys.Add(key);
                }

                return new AirGroupInfo()
                {
                    Name = section,
                    Aircrafts = aircrafts,
                    AirGroupKeys = airGroupKeys,
                    SquadronCount = SectionFileUtil.ReadNumeric(file, section, KeySquadronCount, FileInfo),
                    FlightCount = SectionFileUtil.ReadNumeric(file, section, KeyFlightCount, FileInfo),
                    FlightSize = SectionFileUtil.ReadNumeric(file, section, KeyFlightSize, FileInfo),
                    ArmyIndex = SectionFileUtil.ReadNumeric(file, section, KeyArmyIndex, FileInfo),
                    AirForceIndex = SectionFileUtil.ReadNumeric(file, section, KeyAirForceIndex, FileInfo)
                };
            }

            return null;
        }
    }

    public class AirGroupInfos
    {
        public static AirGroupInfos Default;

        public AirGroupInfo[] AirGroupInfo
        {
            get;
            protected set;
        }

        public static AirGroupInfos Create(ISectionFile file)
        {
            return new AirGroupInfos() { AirGroupInfo = CreateAirGroupInfo(file) };
        }

        public static AirGroupInfo[] CreateAirGroupInfo(ISectionFile file)
        {
            List<AirGroupInfo> infos = new List<AirGroupInfo>();

            string key;
            string value;
            int lines = file.lines(IL2DCE.Generator.AirGroupInfo.SectionMain);
            // Debug.WriteLine("{0}, Count={1}", SectionMain, lines);
            for (int i = 0; i < lines; i++)
            {
                file.get(IL2DCE.Generator.AirGroupInfo.SectionMain, i, out key, out value);
                if (!string.IsNullOrEmpty(key))
                {
                    string secAircrafts = string.Format("{0}.{1}", key, IL2DCE.Generator.AirGroupInfo.SectionAircrafts);
                    string secAirGroupKeys = string.Format("{0}.{1}", key, IL2DCE.Generator.AirGroupInfo.SectionAirGroupKeys);
                    AirGroupInfo airGroupInfo = IL2DCE.Generator.AirGroupInfo.Create(file, key, secAircrafts, secAirGroupKeys);
                    if (airGroupInfo != null)
                    {
                        infos.Add(airGroupInfo);
                    }
                    else
                    {
                        Debug.WriteLine("No AirGroupInfo[{0}, {1}, {2}]", key, secAircrafts, secAirGroupKeys);
                    }
                }
            }

            return infos.ToArray();
        }

        //public AirGroupInfo[] GetAirGroupInfos(int armyIndex)
        //{
        //    return AirGroupInfo.Where(x => x.ArmyIndex == armyIndex).ToArray();
        //}

        //public AirGroupInfo GetAirGroupInfo(int armyIndex, string airGroupKey)
        //{
        //    return AirGroupInfo.Where(x => x.ArmyIndex == armyIndex && x.AirGroupKeys.Contains(airGroupKey)).FirstOrDefault();
        //}

        public IEnumerable<AirGroupInfo> GetAirGroupInfoGroupKey(string airGroupKey, bool ignoreCase = false)
        {
            return ignoreCase ? AirGroupInfo.Where(x => x.AirGroupKeys.Any(y => string.Compare(y, airGroupKey, true, CultureInfo.InvariantCulture) == 0)) :
                                AirGroupInfo.Where(x => x.AirGroupKeys.Contains(airGroupKey));
        }

        public IEnumerable<AirGroupInfo> GetAirGroupInfoAircraft(string aircraft, bool ignoreCase = false)
        {
            return ignoreCase ? AirGroupInfo.Where(x => x.Aircrafts.Any(y => string.Compare(y, aircraft, true, CultureInfo.InvariantCulture) == 0)) :
                                AirGroupInfo.Where(x => x.Aircrafts.Contains(aircraft));
        }

        public IEnumerable<AirGroupInfo> GetAirGroupInfo(string airGroupKey, string aircraft, bool ignoreCase = false, bool addGroupKey = true)
        {
            var result = GetAirGroupInfoGroupKey(airGroupKey, ignoreCase);
            if (result.Count() == 0)
            {
                result = GetAirGroupInfoAircraft(aircraft, ignoreCase);
                if (addGroupKey)
                {
                    foreach (var item in result)
                    {
                        item.AirGroupKeys.Add(airGroupKey);
                    }
                }
            }
            return result;
        }

        //public int GetArmyIndex(string airGroupKey)
        //{
        //    AirGroupInfo airGroupInfo = GetAirGroupInfo(airGroupKey).FirstOrDefault();
        //    return airGroupInfo != null ? airGroupInfo.ArmyIndex : 0;
        //}

        public void Write(ISectionFile file)
        {
            foreach (var item in AirGroupInfo)
            {
                SectionFileUtil.Write(file, IL2DCE.Generator.AirGroupInfo.SectionMain, item.Name, string.Empty);
                item.Write(file);
            }
        }
    }
}