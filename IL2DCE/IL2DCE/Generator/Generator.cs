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

using System;
using System.Collections.Generic;
using maddox.game;

namespace IL2DCE
{
    /// <summary>
    /// The generator for the next mission.
    /// </summary>
    /// <remarks>
    /// This is a interface for the different generators responsible for different parts in the mission generation.
    /// </remarks>
    class Generator
    {
        internal GeneratorAirOperation GeneratorAirOperation
        {
            get;
            set;
        }

        internal GeneratorGroundOperation GeneratorGroundOperation
        {
            get;
            set;
        }

        internal GeneratorBriefing GeneratorBriefing
        {
            get;
            set;
        }

        internal IRandom Random
        {
            get
            {
                return _core.Random;
            }

        }

        private Core _core;

        internal Core Core
        {
            get
            {
                return _core;
            }
        }

        private IGamePlay GamePlay
        {
            get
            {
                return _core.GamePlay;
            }
        }

        private Config Config
        {
            get
            {
                return _core.Config;
            }

        }

        private Career Career
        {
            get
            {
                return _core.CurrentCareer;
            }
        }

        public Generator(Core core)
        {
            _core = core;
        }

        public void GenerateInitialMissionTempalte(IEnumerable<string> initialMissionTemplateFiles, out ISectionFile initialMissionTemplateFile, AirGroupInfos airGroupInfos = null)
        {
            initialMissionTemplateFile = null;

            foreach (string fileName in initialMissionTemplateFiles)
            {
                // Use the first template file to load the map.
                initialMissionTemplateFile = GamePlay.gpLoadSectionFile(fileName);
                break;
            }

            if (initialMissionTemplateFile != null)
            {
                // Delete everything from the template file.

                if (initialMissionTemplateFile.exist("AirGroups"))
                {
                    // Delete all air groups from the template file.
                    for (int i = 0; i < initialMissionTemplateFile.lines("AirGroups"); i++)
                    {
                        string key;
                        string value;
                        initialMissionTemplateFile.get("AirGroups", i, out key, out value);
                        initialMissionTemplateFile.delete(key);
                        initialMissionTemplateFile.delete(key + "_Way");
                    }
                    initialMissionTemplateFile.delete("AirGroups");
                }

                if (initialMissionTemplateFile.exist("Chiefs"))
                {
                    // Delete all ground groups from the template file.
                    for (int i = 0; i < initialMissionTemplateFile.lines("Chiefs"); i++)
                    {
                        string key;
                        string value;
                        initialMissionTemplateFile.get("Chiefs", i, out key, out value);
                        initialMissionTemplateFile.delete(key + "_Road");
                    }
                    initialMissionTemplateFile.delete("Chiefs");
                }

                MissionFile initialMission = new MissionFile(GamePlay, initialMissionTemplateFiles, airGroupInfos);

                foreach (AirGroup airGroup in initialMission.AirGroups)
                {
                    airGroup.WriteTo(initialMissionTemplateFile, Config);
                }

                foreach (GroundGroup groundGroup in initialMission.GroundGroups)
                {
                    groundGroup.WriteTo(initialMissionTemplateFile);
                }
            }
        }

        /// <summary>
        /// Generates the next mission template based on the previous mission template. 
        /// </summary>
        /// <param name="staticTemplateFiles"></param>
        /// <param name="previousMissionTemplate"></param>
        /// <param name="missionTemplateFile"></param>
        /// <param name="airGroupInfos"></param>
        /// <remarks>
        /// For now it has a simplified implementaiton. It only generated random supply ships and air groups.
        /// </remarks>
        public void GenerateMissionTemplate(IEnumerable<string> staticTemplateFiles, ISectionFile previousMissionTemplate, out ISectionFile missionTemplateFile, AirGroupInfos airGroupInfos = null)
        {
            MissionFile staticTemplateFile = new MissionFile(GamePlay, staticTemplateFiles, airGroupInfos);

            // Use the previous mission template to initialise the next mission template.
            missionTemplateFile = previousMissionTemplate;

            // Remove the ground groups but keep the air groups.
            if (missionTemplateFile.exist("Chiefs"))
            {
                // Delete all ground groups from the template file.
                for (int i = 0; i < missionTemplateFile.lines("Chiefs"); i++)
                {
                    string key;
                    string value;
                    missionTemplateFile.get("Chiefs", i, out key, out value);
                    missionTemplateFile.delete(key + "_Road");
                }
                missionTemplateFile.delete("Chiefs");
            }

            if (missionTemplateFile.exist("Stationary"))
            {
                // Delete all stationaries from the template file.
                missionTemplateFile.delete("Stationary");
            }

            // Generate supply ships and trains.

            // For now generate a random supply ship on one of the routes to a harbour.
            int chiefIndex = 0;
            int stationaryIndex = 0;

            // TODO: Only create a random (or decent) amount of supply ships.
            foreach (Waterway waterway in staticTemplateFile.Waterways)
            {
                // For waterways only the end must be in friendly territory.
                if (GamePlay.gpFrontArmy(waterway.End.X, waterway.End.Y) == 1)
                {
                    string id = chiefIndex.ToString(System.Globalization.CultureInfo.InvariantCulture.NumberFormat) + "_Chief";
                    chiefIndex++;

                    // For red army
                    GroundGroup supplyShip = new GroundGroup(id, "Ship.Tanker_Medium1", ECountry.gb, "/sleep 0/skill 2/slowfire 1", waterway.Waypoints);
                    supplyShip.WriteTo(missionTemplateFile);
                }
                else if (GamePlay.gpFrontArmy(waterway.End.X, waterway.End.Y) == 2)
                {
                    string id = chiefIndex.ToString(System.Globalization.CultureInfo.InvariantCulture.NumberFormat) + "_Chief";
                    chiefIndex++;

                    // For blue army
                    GroundGroup supplyShip = new GroundGroup(id, "Ship.Tanker_Medium1", ECountry.de, "/sleep 0/skill 2/slowfire 1", waterway.Waypoints);
                    supplyShip.WriteTo(missionTemplateFile);
                }
            }

            foreach (Waterway railway in staticTemplateFile.Railways)
            {
                // For railways the start and the end must be in friendly territory.
                if (GamePlay.gpFrontArmy(railway.Start.X, railway.Start.Y) == 1 && GamePlay.gpFrontArmy(railway.End.X, railway.End.Y) == 1)
                {
                    string id = chiefIndex.ToString(System.Globalization.CultureInfo.InvariantCulture.NumberFormat) + "_Chief";
                    chiefIndex++;

                    // For red army
                    GroundGroup supplyShip = new GroundGroup(id, "Train.57xx_0-6-0PT_c0", ECountry.gb, "", railway.Waypoints);
                    supplyShip.WriteTo(missionTemplateFile);
                }
                else if (GamePlay.gpFrontArmy(railway.Start.X, railway.Start.Y) == 2 && GamePlay.gpFrontArmy(railway.End.X, railway.End.Y) == 2)
                {
                    string id = chiefIndex.ToString(System.Globalization.CultureInfo.InvariantCulture.NumberFormat) + "_Chief";
                    chiefIndex++;

                    // For blue army
                    GroundGroup supplyShip = new GroundGroup(id, "Train.BR56-00_c2", ECountry.de, "", railway.Waypoints);
                    supplyShip.WriteTo(missionTemplateFile);
                }
            }

            foreach (Building depot in staticTemplateFile.Depots)
            {
                // For depots the position must be in friendly territory.
                if (GamePlay.gpFrontArmy(depot.X, depot.Y) == 1)
                {
                    string id = stationaryIndex.ToString("Static" + System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
                    stationaryIndex++;

                    // For red army
                    Stationary fuelTruck = new Stationary(id, "Stationary.Morris_CS8_tank", ECountry.gb, depot.X, depot.Y, depot.Direction);
                    fuelTruck.WriteTo(missionTemplateFile);
                }
                else if (GamePlay.gpFrontArmy(depot.X, depot.Y) == 2)
                {
                    string id = stationaryIndex.ToString("Static" + System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
                    stationaryIndex++;

                    // For blue army
                    Stationary fuelTruck = new Stationary(id, "Stationary.Opel_Blitz_fuel", ECountry.de, depot.X, depot.Y, depot.Direction);
                    fuelTruck.WriteTo(missionTemplateFile);
                }
            }

            foreach (Stationary aircraft in staticTemplateFile.Aircraft)
            {
                // For aircraft the position must be in friendly territory.
                if (GamePlay.gpFrontArmy(aircraft.X, aircraft.Y) == 1)
                {
                    string id = stationaryIndex.ToString("Static" + System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
                    stationaryIndex++;

                    // For red army
                    Stationary fuelTruck = new Stationary(id, "Stationary.HurricaneMkI_dH5-20", ECountry.gb, aircraft.X, aircraft.Y, aircraft.Direction);
                    fuelTruck.WriteTo(missionTemplateFile);
                }
                else if (GamePlay.gpFrontArmy(aircraft.X, aircraft.Y) == 2)
                {
                    string id = stationaryIndex.ToString("Static" + System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
                    stationaryIndex++;

                    // For blue army
                    Stationary fuelTruck = new Stationary(id, "Stationary.Bf-109E-1", ECountry.de, aircraft.X, aircraft.Y, aircraft.Direction);
                    fuelTruck.WriteTo(missionTemplateFile);
                }
            }

            foreach (Stationary artillery in staticTemplateFile.Artilleries)
            {
                // For artillery the position must be in friendly territory.
                if (GamePlay.gpFrontArmy(artillery.X, artillery.Y) == 1)
                {
                    string id = stationaryIndex.ToString("Static" + System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
                    stationaryIndex++;

                    // For red army
                    Stationary aaGun = new Stationary(id, "Artillery.Bofors", ECountry.gb, artillery.X, artillery.Y, artillery.Direction, "/timeout 0/radius_hide 0");
                    aaGun.WriteTo(missionTemplateFile);
                }
                else if (GamePlay.gpFrontArmy(artillery.X, artillery.Y) == 2)
                {
                    string id = stationaryIndex.ToString("Static" + System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
                    stationaryIndex++;

                    // For blue army
                    Stationary aaGun = new Stationary(id, "Artillery.4_cm_Flak_28", ECountry.de, artillery.X, artillery.Y, artillery.Direction, "/timeout 0/radius_hide 0");
                    aaGun.WriteTo(missionTemplateFile);
                }
            }

            foreach (Stationary radar in staticTemplateFile.Radar)
            {
                // For artillery the position must be in friendly territory.
                if (GamePlay.gpFrontArmy(radar.X, radar.Y) == 1)
                {
                    string id = stationaryIndex.ToString("Static" + System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
                    stationaryIndex++;

                    // For red army
                    Stationary radarSite = new Stationary(id, "Stationary.Radar.EnglishRadar1", ECountry.gb, radar.X, radar.Y, radar.Direction);
                    radarSite.WriteTo(missionTemplateFile);
                }
                else if (GamePlay.gpFrontArmy(radar.X, radar.Y) == 2)
                {
                    string id = stationaryIndex.ToString("Static" + System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
                    stationaryIndex++;

                    // For blue army
                    Stationary radarSite = new Stationary(id, "Stationary.Radar.Wotan_II", ECountry.de, radar.X, radar.Y, radar.Direction);
                    radarSite.WriteTo(missionTemplateFile);
                }
            }
        }

        public void GenerateMission(string environmentTemplateFile, string missionTemplateFileName, string missionId, out ISectionFile missionFile, out BriefingFile briefingFile)
        {
            MissionFile missionTemplateFile = new MissionFile(GamePlay.gpLoadSectionFile(missionTemplateFileName), Career.CampaignInfo.AirGroupInfos);

            GeneratorAirOperation = new GeneratorAirOperation(this, Career.CampaignInfo, missionTemplateFile, Core.GamePlay, Core.Config);
            GeneratorGroundOperation = new GeneratorGroundOperation(this, Career.CampaignInfo, missionTemplateFile, Core.GamePlay, Core.Config);
            GeneratorBriefing = new GeneratorBriefing(Core, this);

            // Load the environment template file for the generated mission.

            missionFile = GamePlay.gpLoadSectionFile(environmentTemplateFile);
            briefingFile = new BriefingFile();

            briefingFile.MissionName = missionId;
            briefingFile.MissionDescription = "";

            // Delete things from the template file.

            // It is not necessary to delete air groups and ground groups from the missionFile as it 
            // is based on the environment template. If there is anything in it (air groups, ...) it is intentional.
            for (int i = 0; i < missionFile.lines("MAIN"); i++)
            {
                // Delete player from the template file.
                string key;
                string value;
                missionFile.get("MAIN", i, out key, out value);
                if (key == "player")
                {
                    missionFile.delete("MAIN", i);
                    break;
                }
            }

            // Add things to the template file.

            int randomTime = Core.Random.Next(5, 21);
            missionFile.set("MAIN", "TIME", randomTime.ToString(System.Globalization.CultureInfo.InvariantCulture.NumberFormat));

            int randomWeatherIndex = Core.Random.Next(0, 3);
            missionFile.set("MAIN", "WeatherIndex", randomWeatherIndex.ToString(System.Globalization.CultureInfo.InvariantCulture.NumberFormat));

            int randomCloudsHeight = Core.Random.Next(5, 15);
            missionFile.set("MAIN", "CloudsHeight", (randomCloudsHeight * 100).ToString(System.Globalization.CultureInfo.InvariantCulture.NumberFormat));

            string weatherString = "";
            if (randomWeatherIndex == 0)
            {
                weatherString = "Clear";
            }
            else if (randomWeatherIndex == 1)
            {
                weatherString = "Light clouds at " + randomCloudsHeight * 100 + "m";
            }
            else if (randomWeatherIndex == 2)
            {
                weatherString = "Medium clouds at " + randomCloudsHeight * 100 + "m";
            }

            briefingFile.MissionDescription += this.Career.CampaignInfo.Name + "\n";
            briefingFile.MissionDescription += "Date: " + this.Career.Date.Value.ToShortDateString() + "\n";
            briefingFile.MissionDescription += "Time: " + randomTime + ":00\n";
            briefingFile.MissionDescription += "Weather: " + weatherString + "\n";

            // Create a air operation for the player.

            foreach (AirGroup airGroup in GeneratorAirOperation.AvailableAirGroups)
            {
                if ((airGroup.ArmyIndex == Career.ArmyIndex) && (airGroup.AirGroupKey + "." + airGroup.SquadronIndex.ToString(System.Globalization.CultureInfo.InvariantCulture.NumberFormat)) == Career.AirGroup)
                {
                    GeneratorAirOperation.CreateRandomAirOperation(missionFile, briefingFile, airGroup);

                    // Determine the aircraft that is controlled by the player.
                    List<string> aircraftOrder = determineAircraftOrder(airGroup);

                    string playerAirGroupKey = airGroup.AirGroupKey;
                    int playerSquadronIndex = airGroup.SquadronIndex;
                    if (aircraftOrder.Count > 0)
                    {
                        string playerPosition = aircraftOrder[aircraftOrder.Count - 1];

                        double factor = aircraftOrder.Count / 6;
                        int playerPositionIndex = (int)(Math.Floor(Career.RankIndex * factor));

                        playerPosition = aircraftOrder[aircraftOrder.Count - 1 - playerPositionIndex];

                        if (missionFile.exist("MAIN", "player"))
                        {
                            missionFile.set("MAIN", "player", playerAirGroupKey + "." + playerSquadronIndex.ToString(System.Globalization.CultureInfo.InvariantCulture.NumberFormat) + playerPosition);
                        }
                        else
                        {
                            missionFile.add("MAIN", "player", playerAirGroupKey + "." + playerSquadronIndex.ToString(System.Globalization.CultureInfo.InvariantCulture.NumberFormat) + playerPosition);
                        }
                    }
                    break;
                }
            }

            // Add additional air operations.

            if (GeneratorAirOperation.AvailableAirGroups != null && GeneratorAirOperation.AvailableAirGroups.Count > 0)
            {
                for (int i = 0; i < Config.AdditionalAirOperations; i++)
                {
                    if (GeneratorAirOperation.AvailableAirGroups.Count > 0)
                    {
                        int randomAirGroupIndex = Core.Random.Next(GeneratorAirOperation.AvailableAirGroups.Count);
                        AirGroup randomAirGroup = GeneratorAirOperation.AvailableAirGroups[randomAirGroupIndex];
                        GeneratorAirOperation.CreateRandomAirOperation(missionFile, briefingFile, randomAirGroup);
                    }
                }
            }

            // Add additional ground operations.

            if (GeneratorGroundOperation.AvailableGroundGroups != null && GeneratorGroundOperation.AvailableGroundGroups.Count > 0)
            {
                for (int i = 0; i < Config.AdditionalGroundOperations; i++)
                {
                    if (GeneratorGroundOperation.AvailableGroundGroups.Count > 0)
                    {
                        int randomGroundGroupIndex = Core.Random.Next(GeneratorGroundOperation.AvailableGroundGroups.Count);
                        GroundGroup randomGroundGroup = GeneratorGroundOperation.AvailableGroundGroups[randomGroundGroupIndex];
                        GeneratorGroundOperation.CreateRandomGroundOperation(missionFile, randomGroundGroup);
                    }
                }
            }

            // Add all stationaries.
            if (GeneratorGroundOperation.AvailableStationaries != null && GeneratorGroundOperation.AvailableStationaries.Count > 0)
            {
                for (int i = 0; i < GeneratorGroundOperation.AvailableStationaries.Count; i++)
                {
                    Stationary stationary = GeneratorGroundOperation.AvailableStationaries[i];
                    stationary.WriteTo(missionFile);
                }
            }
        }

        private static List<string> determineAircraftOrder(AirGroup airGroup)
        {
            List<string> aircraftOrder = new List<string>();
            AirGroupInfo airGroupInfo = airGroup.AirGroupInfo;
            if (airGroupInfo.FlightSize % 3 == 0)
            {
                for (int i = 0; i < 3; i++)
                {
                    foreach (int key in airGroup.Flights.Keys)
                    {
                        if (airGroup.Flights[key].Count > i)
                        {
                            aircraftOrder.Add(key.ToString(System.Globalization.CultureInfo.InvariantCulture.NumberFormat) + i.ToString(System.Globalization.CultureInfo.InvariantCulture.NumberFormat));
                        }
                    }

                    foreach (int key in airGroup.Flights.Keys)
                    {
                        if (airGroup.Flights[key].Count > i + 3)
                        {
                            aircraftOrder.Add(key.ToString(System.Globalization.CultureInfo.InvariantCulture.NumberFormat) + (i + 3).ToString(System.Globalization.CultureInfo.InvariantCulture.NumberFormat));
                        }
                    }
                }
            }
            else if (airGroupInfo.FlightSize % 2 == 0)
            {
                for (int i = 0; i < 2; i++)
                {
                    foreach (int key in airGroup.Flights.Keys)
                    {
                        if (airGroup.Flights[key].Count > i)
                        {
                            aircraftOrder.Add(key.ToString(System.Globalization.CultureInfo.InvariantCulture.NumberFormat) + i.ToString(System.Globalization.CultureInfo.InvariantCulture.NumberFormat));
                        }
                    }

                    foreach (int key in airGroup.Flights.Keys)
                    {
                        if (airGroup.Flights[key].Count > i + 2)
                        {
                            aircraftOrder.Add(key.ToString(System.Globalization.CultureInfo.InvariantCulture.NumberFormat) + (i + 2).ToString(System.Globalization.CultureInfo.InvariantCulture.NumberFormat));
                        }
                    }
                }
            }
            else if (airGroupInfo.FlightSize % 1 == 0)
            {
                foreach (int key in airGroup.Flights.Keys)
                {
                    if (airGroup.Flights[key].Count == 1)
                    {
                        aircraftOrder.Add(key.ToString(System.Globalization.CultureInfo.InvariantCulture.NumberFormat) + "0");
                    }
                }
            }

            return aircraftOrder;
        }
    }
}