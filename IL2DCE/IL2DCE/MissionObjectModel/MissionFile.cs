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
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using IL2DCE.Generator;
using maddox.game;

namespace IL2DCE.MissionObjectModel
{
    public class MissionFile
    {
        #region Definition

        public const string SectionParts = "PARTS";
        public const string SectionMain = "MAIN";
        public const string SectionGlobalWind = "GlobalWind";
        public const string SectionSplines = "splines";
        public const string SectionCustomChiefs = "CustomChiefs";
        public const string SectionStationary = "Stationary";
        public const string SectionBuildings = "Buildings";
        public const string SectionBuildingsLinks = "BuildingsLinks";
        public const string SectionFrontMarker = "FrontMarker";
        public const string SectionAirGroups = "AirGroups";
        public const string SectionChiefs = "Chiefs";
        public const string SectionChiefsRoad = "Chiefs_Road";
        public const string SectionRoad = "Road";
        public const string SectionTrigger = "Trigger";
        public const string SectionAction = "Action";
        public const string SectionAirdromes = "Airdromes";
        public const string SectionWay = "_Way";
        public const string KeyRunways = "Runways";
        public const string KeyWeatherIndex = "WeatherIndex";
        public const string KeyCloudsHeight = "CloudsHeight";
        public const string KeyTime = "TIME";
        public const string KeyPoints = "Points";
        public const string KeyPlayer = "player";
        public const string KeyVirtualAirGroupKey = "VirtualAirGroupKey";
        public const string KeyFlight = "Flight";
        public const string KeyClass = "Class";
        public const string KeyFormation = "Formation";
        public const string KeyCallSign = "CallSign";
        public const string KeyFuel = "Fuel";
        public const string KeyWeapons = "Weapons";
        public const string KeyDetonator = "Detonator";
        public const string KeySetOnPark = "SetOnPark";
        public const string KeySpawnFromScript = "SpawnFromScript";
        public const string KeySkill = "Skill";
        public const string KeyAging = "Aging";
        public const string KeySkin = "Skin";
        public const string KeyMarkingsOn = "MarkingsOn";
        public const string KeyBandColor = "BandColor";
        public const string KeyBriefing = "Briefing";
        public const string ValueTTime = "TTime";
        public const string ValueASpawnGroup = "ASpawnGroup";
        public const string KeyPartsCore = "core.100";
        public const string KeyPartsBob = "bob.100";
        public const string KeyPartsTobruk = "tobruk.100";

        public const float DefaultTime = 12.0f;
        public const int DefaulWeatherIndex = 0;
        public const int DefaulCloudsHeight = 1000;

        public static readonly char[] SplitChars = new char[] { ' ' };

        #endregion

        #region Property (& Variable)

        public IEnumerable<string> Parts
        {
            get;
            private set;
        }

        public bool IsTypeDLC
        {
            get
            {
                return DLC.Any();
            }
        }

        public IEnumerable<string> DLC
        {
            get
            {
                return Parts != null ? Parts.Except(new string[] { KeyPartsCore, KeyPartsBob }).Select(x => 
                                                    { 
                                                        int i = x.IndexOf("."); 
                                                        return i != -1 ? x.Substring(0, i) : x;
                                                    }): 
                                       new string [0];
            }
        }

        public float Time
        {
            get;
            private set;
        }

        public int WeatherIndex
        {
            get;
            private set;
        }

        public int CloudsHeight
        {
            get;
            private set;
        }

        public IList<Waterway> Roads
        {
            get
            {
                return _roads;
            }
        }
        private List<Waterway> _roads = new List<Waterway>();

        public IList<Waterway> Waterways
        {
            get
            {
                return _waterways;
            }
        }
        private List<Waterway> _waterways = new List<Waterway>();

        public IList<Waterway> Railways
        {
            get
            {
                return _railways;
            }
        }
        private List<Waterway> _railways = new List<Waterway>();

        public IList<Building> Depots
        {
            get
            {
                return _depots;
            }
        }
        private List<Building> _depots = new List<Building>();

        public IList<Stationary> Radar
        {
            get
            {
                return _radars;
            }
        }
        private List<Stationary> _radars = new List<Stationary>();

        public IList<Stationary> Aircraft
        {
            get
            {
                return _aircrafts;
            }
        }
        private List<Stationary> _aircrafts = new List<Stationary>();

        public IList<Stationary> Artilleries
        {
            get
            {
                return _artilleries;
            }
        }
        private List<Stationary> _artilleries = new List<Stationary>();

        //public IList<Point3d> RedFrontMarkers
        //{
        //    get
        //    {
        //        return _redFrontMarkers;
        //    }
        //}

        //public IList<Point3d> BlueFrontMarkers
        //{
        //    get
        //    {
        //        return _blueFrontMarkers;
        //    }
        //}

        public IList<AirGroup> AirGroups
        {
            get
            {
                List<AirGroup> airGroups = new List<AirGroup>();
                airGroups.AddRange(_redAirGroups);
                airGroups.AddRange(_blueAirGroups);
                return airGroups;
            }
        }

        public IList<AirGroup> RedAirGroups
        {
            get
            {
                List<AirGroup> airGroups = new List<AirGroup>();
                airGroups.AddRange(_redAirGroups);
                return airGroups;
            }
        }
        private List<AirGroup> _redAirGroups = new List<AirGroup>();

        public IList<AirGroup> BlueAirGroups
        {
            get
            {
                List<AirGroup> airGroups = new List<AirGroup>();
                airGroups.AddRange(_blueAirGroups);
                return airGroups;
            }
        }
        private List<AirGroup> _blueAirGroups = new List<AirGroup>();

        public IList<GroundGroup> GroundGroups
        {
            get
            {
                List<GroundGroup> groundGroups = new List<GroundGroup>();
                groundGroups.AddRange(_redGroundGroups);
                groundGroups.AddRange(_blueGroundGroups);
                return groundGroups;
            }
        }

        public IList<GroundGroup> RedGroundGroups
        {
            get
            {
                List<GroundGroup> groundGroups = new List<GroundGroup>();
                groundGroups.AddRange(_redGroundGroups);
                return groundGroups;
            }
        }
        private List<GroundGroup> _redGroundGroups = new List<GroundGroup>();

        public IList<GroundGroup> BlueGroundGroups
        {
            get
            {
                List<GroundGroup> groundGroups = new List<GroundGroup>();
                groundGroups.AddRange(_blueGroundGroups);
                return groundGroups;
            }
        }
        private List<GroundGroup> _blueGroundGroups = new List<GroundGroup>();

        public IList<Stationary> Stationaries
        {
            get
            {
                List<Stationary> stationaries = new List<Stationary>();
                stationaries.AddRange(_redStationaries);
                stationaries.AddRange(_blueStationaries);
                return stationaries;
            }
        }
        private List<Stationary> _redStationaries = new List<Stationary>();
        private List<Stationary> _blueStationaries = new List<Stationary>();

        public string[] AircraftRandomRed
        {
            get;
            private set;
        }

        public string[] AircraftRandomBlue
        {
            get;
            private set;
        }

        #endregion

        //private List<Point3d> _redFrontMarkers = new List<Point3d>();
        //private List<Point3d> _blueFrontMarkers = new List<Point3d>();
        //private List<Point3d> _neutralFrontMarkers = new List<Point3d>();

        private AirGroupInfos airGroupInfos;

        public MissionFile(IGamePlay game, IEnumerable<string> fileNames, AirGroupInfos airGroupInfos = null)
        {
            this.airGroupInfos = airGroupInfos;

            init();
            foreach (string fileName in fileNames)
            {
                load(game.gpLoadSectionFile(fileName));
            }
        }

        public MissionFile(ISectionFile file, AirGroupInfos airGroupInfos = null)
        {
            this.airGroupInfos = airGroupInfos;

            init();
            load(file);
        }

        private void init()
        {
            _roads.Clear();
            _waterways.Clear();
            _railways.Clear();
            _depots.Clear();
            _aircrafts.Clear();
            _artilleries.Clear();

            //_redFrontMarkers.Clear();
            //_blueFrontMarkers.Clear();
            //_neutralFrontMarkers.Clear();

            _redAirGroups.Clear();
            _blueAirGroups.Clear();
            _redGroundGroups.Clear();
            _blueGroundGroups.Clear();

            _redStationaries.Clear();
            _blueStationaries.Clear();
        }

        private void load(ISectionFile file)
        {
            // Main
            Parts = SilkySkyCloDFile.ReadSectionKeies(file, SectionParts);
            Time = file.get(SectionMain, KeyTime, DefaultTime);
            WeatherIndex = file.get(SectionMain, KeyWeatherIndex, DefaulWeatherIndex);
            CloudsHeight = file.get(SectionMain, KeyCloudsHeight, DefaulCloudsHeight);

            // Stationary
            int lines = file.lines(SectionStationary);
            for (int i = 0; i < lines; i++)
            {
                string key;
                string value;
                file.get(SectionStationary, i, out key, out value);

                Stationary stationary = new Stationary(file, key);
                if (stationary.Army == (int)EArmy.Red)
                {
                    _redStationaries.Add(stationary);
                }
                else if (stationary.Army == (int)EArmy.Blue)
                {
                    _blueStationaries.Add(stationary);
                }
                else
                {
                    if (stationary.Type == EStationaryType.Radar)
                    {
                        _radars.Add(stationary);
                    }
                    else if (stationary.Type == EStationaryType.Artillery)
                    {
                        _artilleries.Add(stationary);
                    }
                    else if (stationary.Type == EStationaryType.Aircraft)
                    {
                        _aircrafts.Add(stationary);
                    }
                }
            }

            // Buildings
            lines = file.lines(SectionBuildings);
            for (int i = 0; i < lines; i++)
            {
                string key;
                string value;
                file.get(SectionBuildings, i, out key, out value);

                string[] valueParts = value.Split(SplitChars, StringSplitOptions.RemoveEmptyEntries);
                if (valueParts.Length > 4)
                {
                    // Depots
                    if (valueParts[0] == "buildings.House$Oil_Bunker-Small" || valueParts[0] == "buildings.House$Oil_Bunker-Middle" || valueParts[0] == "buildings.House$Oil_Bunker-Big")
                    {
                        Building building = new Building(file, key);
                        _depots.Add(building);
                    }

                    // Other buldings ...
                }
            }

            // FrontMaker
            //for (int i = 0; i < file.lines(SectionFrontMarker); i++)
            //{
            //    string key;
            //    string value;
            //    file.get(SectionFrontMarker, i, out key, out value);

            //    string[] valueParts = value.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            //    if (valueParts.Length == 3)
            //    {
            //        double x;
            //        double y;
            //        int army;
            //        if (double.TryParse(valueParts[0], NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat, out x)
            //            && double.TryParse(valueParts[1], NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat, out y)
            //            && int.TryParse(valueParts[2], NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat, out army))
            //        {
            //            if (army == 0)
            //            {
            //                _neutralFrontMarkers.Add(new Point3d(x, y, 0.0));
            //            }
            //            else if (army == 1)
            //            {
            //                _redFrontMarkers.Add(new Point3d(x, y, 0.0));
            //            }
            //            else if (army == 2)
            //            {
            //                _blueFrontMarkers.Add(new Point3d(x, y, 0.0));
            //            }
            //        }
            //    }
            //}

            // AirGroups
            lines = file.lines(SectionAirGroups);
            for (int i = 0; i < lines; i++)
            {
                string key;
                string value;
                file.get(SectionAirGroups, i, out key, out value);

                AirGroup airGroup = new AirGroup(file, key);
                string airGoupKey = string.IsNullOrEmpty(airGroup.VirtualAirGroupKey) ? airGroup.AirGroupKey : airGroup.VirtualAirGroupKey;
                IEnumerable<AirGroupInfo> airGroupInfo = GetAirGroupInfo(airGoupKey, airGroup.Class, false);
                if (airGroupInfo.Any())
                {
                    AirGroupInfo airGroupInfoTarget = airGroupInfo.FirstOrDefault();
                    airGroup.SetAirGroupInfo(airGroupInfoTarget);
                    if (airGroup.ArmyIndex == (int)EArmy.Red)
                    {
                        _redAirGroups.Add(airGroup);
                    }
                    else if (airGroup.ArmyIndex == (int)EArmy.Blue)
                    {
                        _blueAirGroups.Add(airGroup);
                    }
                    else
                    {
                        Debug.Assert(false);
                    }
                }
                else
                {
                    Debug.WriteLine("no AirGroup info[{0}] and aircraft [{1}] in the file[{2}]", airGoupKey, airGroup.Class, "AirGroupInfo.ini");
                    Debug.Assert(false);
                }
            }

            // Chiefs
            lines = file.lines(SectionChiefs);
            for (int i = 0; i < lines; i++)
            {
                string key;
                string value;
                file.get(SectionChiefs, i, out key, out value);

                GroundGroup groundGroup = GroundGroup.Create(file, key);
                if (groundGroup != null && groundGroup.Army != 0)
                {
                    if (groundGroup.Army == (int)EArmy.Red)
                    {
                        _redGroundGroups.Add(groundGroup);
                    }
                    else if (groundGroup.Army == (int)EArmy.Blue)
                    {
                        _blueGroundGroups.Add(groundGroup);
                    }
                }
                else
                {
                    Waterway road = Waterway.Create(file, key);
                    if (road != null)
                    {
                        if (value.StartsWith("Vehicle") || value.StartsWith("Armor"))
                        {
                            _roads.Add(road);
                        }
                        else if (value.StartsWith("Ship"))
                        {
                            _waterways.Add(road);
                        }
                        else if (value.StartsWith("Train"))
                        {
                            _railways.Add(road);
                        }
                    }
                }
            }

            // Trigger & Action  (Support: Time & Spawn only)
            IList<AirGroup> airGroups = AirGroups;
            lines = file.lines(SectionAction);
            for (int i = 0; i < lines; i++)
            {
                string key;
                string value;
                file.get(SectionAction, i, out key, out value);
                if (file.exist(SectionTrigger, key) && !string.IsNullOrEmpty(value))
                {
                    string[] actions = value.Split(SplitChars, StringSplitOptions.RemoveEmptyEntries);
                    string trigger = file.get(SectionTrigger, key);
                    string[] triggers = trigger.Split(SplitChars, StringSplitOptions.RemoveEmptyEntries);
                    if (actions.Length >= 3 && string.Compare(actions[0], ValueASpawnGroup, true) == 0 && string.Compare(actions[1], "1") == 0
                        && triggers.Length >= 2 && string.Compare(triggers[0], ValueTTime, true) == 0)
                    {
                        var airGroup = airGroups.Where(x => string.Compare(x.Id, actions[2], true) == 0).FirstOrDefault();
                        if (airGroup != null)
                        {
                            int time;
                            if (int.TryParse(triggers[1], out time) && time >= 0)
                            {
                                airGroup.SetSpawn(new Spawn((int)ESpawn.Default, false, false, new Spawn.SpawnLocation(), false, false, new Spawn.SpawnTime(true, time)));
                            }
                        }
                    }
                }
            }

            if (file.exist(Config.SectionAircraft, Config.KeyRandomRed))
            {
                string value = file.get(Config.SectionAircraft, Config.KeyRandomRed);
                AircraftRandomRed = value.Split(Config.SplitSpace, StringSplitOptions.RemoveEmptyEntries);
            }
            else
            {
                AircraftRandomRed = new string[0];
            }

            if (file.exist(Config.SectionAircraft, Config.KeyRandomBlue))
            {
                string value = file.get(Config.SectionAircraft, Config.KeyRandomBlue);
                AircraftRandomBlue = value.Split(Config.SplitSpace, StringSplitOptions.RemoveEmptyEntries);
            }
            else
            {
                AircraftRandomBlue = new string[0];
            }
        }

        private IEnumerable<AirGroupInfo> GetAirGroupInfo(string airGroupKey, string aircraft, bool ignoreCase)
        {
            IEnumerable<AirGroupInfo> airGroupInfo;
            if (airGroupInfos != null && (airGroupInfo = airGroupInfos.GetAirGroupInfo(airGroupKey, aircraft, ignoreCase)).Any())
            {
                return airGroupInfo;
            }
            else if (AirGroupInfos.Default != null && (airGroupInfo = AirGroupInfos.Default.GetAirGroupInfo(airGroupKey, aircraft, ignoreCase)).Any())
            {
                return airGroupInfo;
            }

            return new AirGroupInfo [0];
        }

        public IList<GroundGroup> GetGroundGroups(int armyIndex)
        {
            if (armyIndex == (int)EArmy.Red)
            {
                return _redGroundGroups;
            }
            else if (armyIndex == (int)EArmy.Blue)
            {
                return _blueGroundGroups;
            }
            else
            {
                return new List<GroundGroup>();
            }
        }

        public IList<AirGroup> GetAirGroups(int armyIndex)
        {
            if (armyIndex == (int)EArmy.Red)
            {
                return _redAirGroups;
            }
            else if (armyIndex == (int)EArmy.Blue)
            {
                return _blueAirGroups;
            }
            else
            {
                return new List<AirGroup>();
            }
        }

        public IList<Stationary> GetFriendlyStationaries(int armyIndex)
        {
            if (armyIndex == (int)EArmy.Red)
            {
                return _redStationaries;
            }
            else if (armyIndex == (int)EArmy.Blue)
            {
                return _blueStationaries;
            }
            else
            {
                return new List<Stationary>();
            }
        }

        public IList<Stationary> GetEnemyStationaries(int armyIndex)
        {
            if (armyIndex == (int)EArmy.Red)
            {
                return _blueStationaries;
            }
            else if (armyIndex == (int)EArmy.Blue)
            {
                return _redStationaries;
            }
            else
            {
                return new List<Stationary>();
            }
        }

        //public IList<Point3d> GetFriendlyMarkers(int armyIndex)
        //{
        //    if (armyIndex == (int)EArmy.Red)
        //    {
        //        return _redFrontMarkers;
        //    }
        //    else if (armyIndex == (int)EArmy.Blue)
        //    {
        //        return _blueFrontMarkers;
        //    }
        //    else
        //    {
        //        return new List<Point3d>();
        //    }
        //}

        //public IList<Point3d> GetEnemyMarkers(int armyIndex)
        //{
        //    if (armyIndex == (int)EArmy.Red)
        //    {
        //        return _blueFrontMarkers;
        //    }
        //    else if (armyIndex == (int)EArmy.Blue)
        //    {
        //        return _redFrontMarkers;
        //    }
        //    else
        //    {
        //        return new List<Point3d>();
        //    }
        //}
    }
}