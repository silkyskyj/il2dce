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

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using IL2DCE.MissionObjectModel;
using IL2DCE.Util;
using maddox.game;
using maddox.game.world;
using maddox.GP;
using XLAND;
using static IL2DCE.MissionObjectModel.MissionStatus;
using static IL2DCE.MissionObjectModel.Skill;

namespace IL2DCE.Generator
{
    class GeneratorAirOperation : GeneratorBase
    {
        #region Definition

        private const float SpawnRangeInflateRate = 1.33f;
        private const int MaxSpawnDifDistanceAirstart = 500;
        private const int MaxSpawnDifDistanceAirport = 1500;
        private const int SpawnNeedParkCountFree = 12;
        private const int MaxRetrySpawnRandomChange = 100;
        private const int MaxRetryCreateOneAirGroup = 10;
        private const int MaxRetryCreateAllAirGroups = 250;
        private const int MaxRetryCreateOperation = 25;

        #endregion

        #region Property & Variable

        private GeneratorGroundOperation GeneratorGroundOperation
        {
            get;
            set;
        }

        private GeneratorBriefing GeneratorBriefing
        {
            get;
            set;
        }

        private CampaignInfo CampaignInfo
        {
            get;
            set;
        }

        private wRECTF RangeBattleArea
        {
            get;
            set;
        }

        private IMissionStatus MissionStatus
        {
            get;
            set;
        }

        private AirGroup AirGroupPlayer
        {
            get;
            set;
        }

        public bool HasAvailableAirGroup
        {
            get
            {
                return AvailableAirGroups.Count > 0;
            }
        }

        public bool HasAssignedAirGroup
        {
            get
            {
                return AssignedAirGroups.Count > 0;
            }
        }

        private ESkillSet AISkill
        {
            get;
            set;
        }

        private Skills AISkills
        {
            get;
            set;
        }

        private Skills Skills
        {
            get;
            set;
        }

        private bool EnableMissionMultiAssign
        {
            get;
            set;
        }

        private double FlightCount
        {
            get;
            set;
        }

        private double FlightSize
        {
            get;
            set;
        }

        private bool SpawnParked
        {
            get;
            set;
        }

        private bool EnableCreateTargetOperation
        {
            get;
            set;
        }

        //public IEnumerable<AirGroup> AssignedAirGroups
        //{
        //    get
        //    {
        //        return AllAirGroups.Where(x => x.MissionAssigned);
        //    }
        //}

        //public IEnumerable<AirGroup> AvailableAirGroups
        //{
        //    get
        //    {
        //        return AllAirGroups.Where(x => !x.MissionAssigned);
        //    }
        //}

        private List<AirGroup> AssignedAirGroups = new List<AirGroup>();
        private List<AirGroup> AvailableAirGroups = new List<AirGroup>();
        private List<AirGroup> AllAirGroups = new List<AirGroup>();

        private wRECTF Range;

        #endregion

        #region Constructor

        public GeneratorAirOperation(IGamePlay gamePlay, IRandom random, GeneratorGroundOperation generatorGroundOperation, GeneratorBriefing generatorBriefing, CampaignInfo campaignInfo, IMissionStatus missionStatus, wRECTF rangeBattleArea, IEnumerable<AirGroup> airGroups, AirGroup airGroupPalyer, ESkillSet aISkill, Skills skills, bool enableMissionMultiAssign, double flightCount, double flightSize, bool spawnParked, bool enableCreateTargetOperation)
            : base(gamePlay, random)
        {
            GeneratorGroundOperation = generatorGroundOperation;
            GeneratorBriefing = generatorBriefing;
            CampaignInfo = campaignInfo;
            MissionStatus = missionStatus;
            RangeBattleArea = rangeBattleArea;

            AllAirGroups.AddRange(airGroups);
            AvailableAirGroups.AddRange(airGroups);
            SetRange(AvailableAirGroups.Select(x => x.Position));
            Range = MapUtil.Sum(Range, generatorGroundOperation.Range);

            AirGroupPlayer = airGroupPalyer;
            AISkill = aISkill;
            Skills = skills;
            EnableMissionMultiAssign = enableMissionMultiAssign;
            FlightCount = flightCount;
            FlightSize = flightSize;
            SpawnParked = spawnParked;
            EnableCreateTargetOperation = enableCreateTargetOperation;

            AISkills = CreateAISkills(aISkill);
        }

        #endregion

        #region Get Random object

        private double getRandomAltitude(AircraftParametersInfo missionParameters)
        {
            if (missionParameters.MinAltitude != null && missionParameters.MinAltitude.HasValue && missionParameters.MaxAltitude != null && missionParameters.MaxAltitude.HasValue)
            {
                return (double)Random.Next((int)missionParameters.MinAltitude.Value, (int)missionParameters.MaxAltitude.Value + 1);
            }
            else
            {
                GamePlay.gpLogServer(new Player[] { GamePlay.gpPlayer() }, "No altitude defined for: " + missionParameters.LoadoutId + ". Using default altitude.", null);
                // Use some default altitudes
                return (double)Random.Next(300, 7000 + 1);
            }
        }

        /// <remarks>
        /// 
        ///     // Tweaked AI settings http://bobgamehub.blogspot.de/2012/03/ai-settings-in-cliffs-of-dover.html
        ///
        ///     // Fighter (the leading space is important)
        ///     string frookie = " 0.30 0.11 0.78 0.40 0.64 0.85 0.85 0.21";
        ///     string faverage = " 0.32 0.12 0.87 0.60 0.74 0.90 0.90 0.31";
        ///     string fexperienced = " 0.52 0.13 0.89 0.70 0.74 0.95 0.92 0.31";
        ///     string fveteran = " 0.73 0.14 0.92 0.80 0.74 1 0.95 0.41";
        ///     string face = " 0.93 0.15 0.96 0.92 0.84 1 1 0.51";
        ///
        ///     // Fighter Bomber (the leading space is important)
        ///     string xrookie = " 0.30 0.11 0.78 0.30 0.74 0.85 0.90 0.40";
        ///     string xaverage = " 0.32 0.12 0.87 0.35 0.74 0.90 0.95 0.52";
        ///     string xexperienced = " 0.52 0.13 0.89 0.38 0.74 0.92 0.95 0.52";
        ///     string xveteran = " 0.73 0.14 0.92 0.40 0.74 0.95 0.95 0.55";
        ///     string xace = " 0.93 0.15 0.96 0.45 0.74 1 1 0.65";
        ///
        ///     // Bomber (the leading space is important)
        ///     string brookie = " 0.30 0.11 0.78 0.20 0.74 0.85 0.90 0.88";
        ///     string baverage = " 0.32 0.12 0.87 0.25 0.74 0.90 0.95 0.91";
        ///     string bexperienced = " 0.52 0.13 0.89 0.28 0.74 0.92 0.95 0.91";
        ///     string bveteran = " 0.73 0.14 0.92 0.30 0.74 0.95 0.95 0.95";
        ///     string bace = " 0.93 0.15 0.96 0.35 0.74 1 1 0.97";
        /// 
        ///     Min 0 - Max 1
        ///     Rookie:     0.79 0.26 0.26 0.16 0.16 0.26 0.37 0.26
        ///     Avarage:    0.84 0.53 0.53 0.37 0.37 0.53 0.53 0.53
        ///     Veteran:    1,0.74 0.84 0.63 0.84 0.84 0.74 0.74
        ///     Ace:        1 0.95 0.95 0.84 0.84 0.95 0.89 0.89
        ///     Min:        0 0 0 0 0 0 0 0
        ///     Max:        1 1 1 1 1 1 1 1
        /// </remarks>
        private string getTweakedSkill(EMissionType missionType, int level)
        {
            if (missionType == EMissionType.COVER || missionType == EMissionType.ESCORT
                || missionType == EMissionType.INTERCEPT || missionType == EMissionType.HUNTING)
            {
                // Fighter
                return Skill.TweakedSkills[level].ToString();
            }
            // TODO: Find a way to identify that aircraft is fighter bomber.
            //else if()
            //{
            //    // FighterBomber
            //    string[] skills = new string[] {
            //        "0.30 0.11 0.78 0.30 0.74 0.85 0.90 0.40",
            //        "0.32 0.12 0.87 0.35 0.74 0.90 0.95 0.52",
            //        "0.52 0.13 0.89 0.38 0.74 0.92 0.95 0.52",
            //        "0.73 0.14 0.92 0.40 0.74 0.95 0.95 0.55",
            //        "0.93 0.15 0.96 0.45 0.74 1 1 0.6",
            //    };

            //    return skills[level];
            //}
            else
            {
                // Bomber
                return Skill.TweakedSkills[5 + level].ToString();
            }
        }

        private string getTweakedSkill(EAircraftType aircraftType, int level)
        {
            int levelIdx = (int)ETweakedAircraftType.count * level;
            if (aircraftType == EAircraftType.Fighter)
            {
                // Fighter
                return AISkills[levelIdx + (int)ETweakedAircraftType.Fighter].ToString();
            }
            else if (aircraftType == EAircraftType.FighterBomber || aircraftType == EAircraftType.Bomber)
            {
                // Fighter Bomber
                return AISkills[levelIdx + (int)ETweakedAircraftType.FighterBomber].ToString();
            }
            else if (aircraftType == EAircraftType.BomberSub || aircraftType == EAircraftType.FighterSub ||
                aircraftType == EAircraftType.FighterBomberSub || aircraftType == EAircraftType.OtherSub)
            {
                // Bomber
                return AISkills[levelIdx + (int)ETweakedAircraftType.Bomber].ToString();
            }

            return AISkills[levelIdx + (int)ETweakedAircraftType.Bomber].ToString();
        }

        private string getRandomSkill(EAircraftType aircraftType)
        {
            int randomLevel = Random.Next(0, AISkills.Count / (int)ETweakedAircraftType.count);
            return getTweakedSkill(aircraftType, randomLevel);
        }

        private string getRandomSkill()
        {
            int randomIdx = Random.Next(0, AISkills.Count);
            return AISkills[randomIdx].ToString();
        }

        private AirGroup getRandomAirGroupBasedOnDistance(IEnumerable<AirGroup> availableAirGroups, AirGroup referenceAirGroup)
        {
            return getRandomAirGroupBasedOnDistance(availableAirGroups, referenceAirGroup.Position);
        }

        private AirGroup getRandomAirGroupBasedOnDistance(IEnumerable<AirGroup> availableAirGroups, Point2d targetPosition)
        {
            return getRandomAirGroupBasedOnDistance(availableAirGroups, new Point3d(targetPosition.x, targetPosition.y, 0.0));
        }

        private AirGroup getRandomAirGroupBasedOnDistance(IEnumerable<AirGroup> availableAirGroups, Point3d targetPosition)
        {
            AirGroup selectedAirGroup = null;

            if (availableAirGroups.Any())
            {
                var copy = new List<AirGroup>(availableAirGroups);
                foreach (var item in availableAirGroups.Where(x => x.Position.Equals(ref targetPosition)))
                {
                    copy.Remove(item);
                }

                copy.Sort(new DistanceComparer(targetPosition));

                Point3d position = targetPosition;
                Point3d last = copy.Last().Position;
                double maxDistance = last.distance(ref position);

                List<KeyValuePair<AirGroup, int>> elements = new List<KeyValuePair<AirGroup, int>>();

                int previousWeight = 0;

                foreach (AirGroup airGroup in copy)
                {
                    double distance = airGroup.Position.distance(ref position);
                    int weight = Convert.ToInt32(Math.Ceiling(maxDistance - distance));
                    int cumulativeWeight = previousWeight + weight;
                    elements.Add(new KeyValuePair<AirGroup, int>(airGroup, cumulativeWeight));
                    previousWeight = cumulativeWeight;
                }

                int diceRoll = Random.Next(0, previousWeight);
                int cumulative = 0;
                for (int i = 0; i < elements.Count; i++)
                {
                    cumulative += elements[i].Value;
                    if (diceRoll <= cumulative)
                    {
                        selectedAirGroup = elements[i].Key;
                        break;
                    }
                }
            }
            else if (availableAirGroups.Count() == 1)
            {
                selectedAirGroup = availableAirGroups.First();
            }

            return selectedAirGroup;
        }

        private AiAirport GetRandomAiAirportBasedOnDistance(IEnumerable<AiAirport> aiAirports, Point3d targetPosition)
        {
            return GetRandomAiAirportBasedOnDistance(Random, aiAirports, targetPosition);
        }

        public static AiAirport GetRandomAiAirportBasedOnDistance(IRandom random, IEnumerable<AiAirport> aiAirports, Point3d targetPosition)
        {
            if (aiAirports.Count() >= 2)
            {
                var copy = new List<AiAirport>(aiAirports);
                foreach (var item in aiAirports.Where(x => x.Pos().Equals(ref targetPosition)))
                {
                    copy.Remove(item);
                }

                copy.Sort(new DistanceComparer(targetPosition));

                Point3d position = targetPosition;
                Point3d last = copy.Last().Pos();
                double maxDistance = last.distance(ref position);

                List<KeyValuePair<AiAirport, int>> elements = new List<KeyValuePair<AiAirport, int>>();

                int previousWeight = 0;
                foreach (AiAirport aiAirport in copy)
                {
                    double distance = aiAirport.Pos().distance(ref position);
                    int weight = Convert.ToInt32(Math.Ceiling(maxDistance - distance));
                    int cumulativeWeight = previousWeight + weight;
                    elements.Add(new KeyValuePair<AiAirport, int>(aiAirport, cumulativeWeight));
                    previousWeight = cumulativeWeight;
                }

                int diceRoll = random.Next(0, previousWeight);
                int cumulative = 0;
                for (int i = 0; i < elements.Count; i++)
                {
                    cumulative += elements[i].Value;
                    if (diceRoll <= cumulative)
                    {
                        return elements[i].Key;
                    }
                }
            }

            return aiAirports.FirstOrDefault();
        }

#if false

        private int getRandomIndex(ref List<AirGroup> airGroups, Point3d position)
        {
            // Sort the air groups by their distance to the position.
            airGroups.Sort(new DistanceComparer(position));

            List<AirGroup> selectedElement = null;
            AirGroup selectedAirGroup = null;

            int numberOfChunks = 4;
            if (airGroups.Count < 4)
            {
                // Split the air groups into chunks based on their distance to the position.
                List<List<AirGroup>> airGroupChunks = Collection.SplitList(airGroups, numberOfChunks);

                List<KeyValuePair<List<AirGroup>, double>> elements = new List<KeyValuePair<List<AirGroup>, double>>();

                // The closer the chunk to the position, the higher is the propability for the selection.
                // Cumulative propability: 
                // http://www.vcskicks.com/random-element.php

                elements.Add(new KeyValuePair<List<AirGroup>, double>(airGroupChunks[3], 5));                //  5%
                elements.Add(new KeyValuePair<List<AirGroup>, double>(airGroupChunks[2], 5 + 15));           // 15%
                elements.Add(new KeyValuePair<List<AirGroup>, double>(airGroupChunks[1], 5 + 15 + 30));      // 30%
                elements.Add(new KeyValuePair<List<AirGroup>, double>(airGroupChunks[0], 5 + 15 + 30 + 50)); // 50%

                int diceRoll = Random.Next(0, 100);

                double cumulative = 0.0;
                for (int i = 0; i < elements.Count; i++)
                {
                    cumulative += elements[i].Value;
                    if (diceRoll < cumulative)
                    {
                        selectedElement = elements[i].Key;
                        break;
                    }
                }
            }
            else
            {
                // Fallback if there are not enough air groups to split them into chunks.              
                selectedElement = airGroups;
            }

            // Randomly choose one air group within the chunk.
            selectedAirGroup = selectedElement[Random.Next(0, selectedElement.Count - 1)];

            // Return the global index of the selected air group (and not the index within the chunk).
            return airGroups.IndexOf(selectedAirGroup);
        }

#endif

        private EMissionType? GetRandomMissionType(AirGroup airGroup)
        {
            IEnumerable<EMissionType> availableMissionTypes = GetAvailableMissionTypes(airGroup);
            if (availableMissionTypes.Any())
            {
                int randomMissionTypeIndex = Random.Next(availableMissionTypes.Count());
                return availableMissionTypes.ElementAt(randomMissionTypeIndex);
            }
            return null;
        }

        #endregion

        #region Create air Operation

        public bool CreateRandomAirOperation(ISectionFile sectionFile, BriefingFile briefingFile, AirGroup airGroup, Spawn spawn, Skill[] skill = null, int speed = -1, int fuel = -1, int flight = -1, EFormation formation = EFormation.Default)
        {
            bool result = false;
            IEnumerable<EMissionType> availableMissionTypes = GetAvailableMissionTypes(airGroup);
            if (availableMissionTypes.Any())
            {
                int randomMissionTypeIndex = Random.Next(availableMissionTypes.Count());
                EMissionType randomMissionType = availableMissionTypes.ElementAt(randomMissionTypeIndex);
                result = CreateAirOperation(sectionFile, briefingFile, airGroup, randomMissionType, true, null, null, null, null, null, spawn, skill, speed, fuel, flight, formation);
            }
            else
            {
                AvailableAirGroups.Remove(airGroup);
            }
            return result;
        }

        public bool CreateAirOperation(ISectionFile sectionFile, BriefingFile briefingFile, AirGroup airGroup, EMissionType missionType, bool allowDefensiveOperation,
            AirGroup forcedEscortAirGroup, AirGroup forcedEscortedAirGroup, AirGroup forcedOffensiveAirGroup, GroundGroup forcedTargetGroundGroup, Stationary forcedTargetStationary, Spawn spawn, Skill[] skill = null, int speed = -1, int fuel = -1, int flight = -1, EFormation formation = EFormation.Default)
        {
            bool result = false;
            if (!airGroup.MissionAssigned && AvailableAirGroups.Contains(airGroup) && isMissionTypeAvailable(airGroup, missionType))
            {
                AvailableAirGroups.Remove(airGroup);
                // jamRunway(airGroup);
                // Debug.WriteLine("AirGroup Name={0}, LandTypes={1}", airGroup.DisplayName, GamePlay.gpLandType(airGroup.Position.x, airGroup.Position.y).ToString());
                AircraftInfo aircraftInfo = CampaignInfo.GetAircraftInfo(airGroup.Class);
                AircraftParametersInfo aircraftParametersInfo = getAvailableRandomAircratParametersInfo(aircraftInfo, missionType);
                AircraftLoadoutInfo aircraftLoadoutInfo = aircraftInfo.GetAircraftLoadoutInfo(aircraftParametersInfo.LoadoutId);
                airGroup.Weapons = aircraftLoadoutInfo.Weapons;
                airGroup.Detonator = aircraftLoadoutInfo.Detonator;
                // airGroup.TraceLoadoutInfo();

                airGroup.SetOnParked = SpawnParked;
                OptimizeSpawn(airGroup, spawn, aircraftParametersInfo);
                AssignedAirGroups.Add(airGroup);

                Spawn spawnDefault = Spawn.Create((int)ESpawn.Default, spawn);
                AirGroup escortAirGroup = forcedEscortAirGroup;
                if (escortAirGroup == null && isMissionTypeEscorted(missionType))  // if Mission is Attack ground actor or Attack static aircraft ... 
                {
                    escortAirGroup = getAvailableRandomEscortAirGroup(airGroup);
                }

                if (missionType == EMissionType.RECON || missionType == EMissionType.MARITIME_RECON)
                {
                    result = CreateAirOperationRecon(sectionFile, airGroup, missionType, aircraftParametersInfo, forcedTargetGroundGroup, forcedTargetStationary, escortAirGroup);
                }
                else if (missionType == EMissionType.ATTACK_ARTILLERY || missionType == EMissionType.ATTACK_RADAR
                        || missionType == EMissionType.ATTACK_AIRCRAFT || missionType == EMissionType.ATTACK_DEPOT)
                {
                    result = CreateAirOperationAttackArtillery(airGroup, missionType, aircraftParametersInfo, forcedTargetStationary, escortAirGroup);
                }
                else if (missionType == EMissionType.ATTACK_ARMOR || missionType == EMissionType.ATTACK_VEHICLE ||
                        missionType == EMissionType.ATTACK_TRAIN || missionType == EMissionType.ATTACK_SHIP ||
                        missionType == EMissionType.ARMED_RECON || missionType == EMissionType.ARMED_MARITIME_RECON)
                {
                    result = CreateAirOperationAttackArmor(sectionFile, airGroup, missionType, aircraftParametersInfo, forcedTargetGroundGroup, forcedTargetStationary, escortAirGroup);
                }
                else if (missionType == EMissionType.INTERCEPT)
                {
                    result = CreateAirOperationIntercept(sectionFile, briefingFile, airGroup, forcedOffensiveAirGroup, spawnDefault);     // Enemy Air Group
                }
                else if (missionType == EMissionType.ESCORT)
                {
                    result = CreateAirOperationEscort(sectionFile, briefingFile, airGroup, forcedEscortedAirGroup, spawnDefault);        // Friendly Air Group
                }
                else if (missionType == EMissionType.COVER)
                {
                    result = CreateAirOperationCover(sectionFile, briefingFile, airGroup, forcedOffensiveAirGroup, spawnDefault);         // Enemy Air Group
                }
                else if (missionType == EMissionType.FOLLOW)
                {
                    result = CreateAirOperationFollow(sectionFile, briefingFile, airGroup, spawnDefault);        // Friendly Air Group
                }
                else if (missionType == EMissionType.HUNTING)
                {
                    result = CreateAirOperationHunting(sectionFile, briefingFile, airGroup, spawnDefault);       // Enemy Air Group
                }
                else if (missionType == EMissionType.TRANSFER)
                {
                    result = CreateAirOperationTransfer(sectionFile, briefingFile, airGroup);
                }

                // Debug.Assert(result);

                if (result)
                {
                    // Flight Size
                    SetFlight(airGroup, missionType, flight);

                    // Skill
                    SetSkill(airGroup, aircraftInfo.AircraftType, skill);

                    // Spawn
                    airGroup.SetSpawn(spawn);

                    // Speed
                    airGroup.SetSpeed(speed);

                    // Fuel
                    airGroup.SetFuel(fuel);

                    // Formation
                    airGroup.SetFormation(formation);

                    // Create Briefing 
                    GeneratorBriefing.CreateBriefing(briefingFile, airGroup, missionType, escortAirGroup);

                    // Write to Mission file
                    airGroup.WriteTo(sectionFile);

                    // create relational operation 1 (Friendly group Operation)
                    if (forcedEscortAirGroup == null && escortAirGroup != null && !escortAirGroup.MissionAssigned && EnableCreateTargetOperation)
                    {
                        CreateAirOperation(sectionFile, briefingFile, escortAirGroup, EMissionType.ESCORT, false, null, airGroup, null, null, null, spawnDefault);
                    }

                    // create relational operation 2 (Enemy group Operation)
                    if (isMissionTypeOffensive(missionType) && allowDefensiveOperation)
                    {
                        AirGroup defensiveAirGroup = getAvailableRandomDefensiveAirGroup(airGroup);     // Enemy Air Group
                        if (defensiveAirGroup != null && !defensiveAirGroup.MissionAssigned && EnableCreateTargetOperation)
                        {
                            CreateAirOperationDefensiv(sectionFile, briefingFile, defensiveAirGroup, airGroup, spawnDefault);   // Enemy Air Group
                        }
                    }
                }
                else
                {
                    AssignedAirGroups.Remove(airGroup);
                }
            }
            else
            {
                // throw new ArgumentException(string.Format("no available MissionType[{0}]", missionType.ToDescription()));
            }

            return result;
        }

        #endregion

        #region Create air Operation Detail

        private bool CreateAirOperationRecon(ISectionFile sectionFile, AirGroup airGroup, EMissionType missionType, AircraftParametersInfo aircraftParametersInfo, GroundGroup forcedTargetGroundGroup, Stationary forcedTargetStationary, AirGroup escortAirGroup)
        {
            bool result = false;
            GroundGroup groundGroup = forcedTargetGroundGroup;
            Stationary stationary = forcedTargetStationary;

            int reTry = -1;
            do
            {
                if (groundGroup == null)
                {
                    groundGroup = GeneratorGroundOperation.getAvailableRandomEnemyGroundGroup(airGroup, missionType);
                    if (groundGroup == null)
                    {
                        if (stationary == null)
                        {
                            stationary = GeneratorGroundOperation.getAvailableRandomEnemyStationary(airGroup, missionType);
                            if (stationary == null)
                            {
                                break;
                            }
                        }
                    }
                }
                double altitude = getRandomAltitude(aircraftParametersInfo);
                if (groundGroup != null)
                {
                    if (!EnableCreateTargetOperation || !groundGroup.MissionAssigned && GeneratorGroundOperation.CreateRandomGroundOperation(sectionFile, groundGroup))
                    {
                        int range = Random.Next((int)Math.Floor(AirGroup.ReconRange / 1.5), (int)Math.Ceiling(AirGroup.ReconRange * 1.5));
                        airGroup.Recon(groundGroup, altitude, escortAirGroup, null, range);
                        result = true;
                    }
                    else
                    {
                        // airGroup = null;
                        groundGroup = null;
                    }
                }
                else if (stationary != null)
                {
                    airGroup.Recon(stationary, altitude, escortAirGroup, null);
                    result = true;
                }
                reTry++;
            } while (!result && reTry < MaxRetryCreateOperation);
            return result;
        }

        private bool CreateAirOperationAttackArtillery(AirGroup airGroup, EMissionType missionType, AircraftParametersInfo aircraftParametersInfo, Stationary forcedTargetStationary, AirGroup escortAirGroup)
        {
            bool result = false;
            Stationary stationary = forcedTargetStationary == null ? GeneratorGroundOperation.getAvailableRandomEnemyStationary(airGroup, missionType) : forcedTargetStationary;
            if (stationary != null)
            {
                // No need to generate a random ground operation for the stationary as the stationary objects are always generated
                // into the file.
                //GeneratorGroundOperation.CreateRandomGroundOperation(sectionFile, stationary);
                double altitude = getRandomAltitude(aircraftParametersInfo);

                airGroup.GroundAttack(stationary, altitude, escortAirGroup);
                result = true;
            }
            return result;
        }

        private bool CreateAirOperationAttackArmor(ISectionFile sectionFile, AirGroup airGroup, EMissionType missionType, AircraftParametersInfo aircraftParametersInfo, GroundGroup forcedTargetGroundGroup, Stationary forcedTargetStationary, AirGroup escortAirGroup)
        {
            bool result = false;
            GroundGroup groundGroup = forcedTargetGroundGroup;
            Stationary stationary = forcedTargetStationary;

            int reTry = -1;
            do
            {
                if (groundGroup == null)
                {
                    groundGroup = GeneratorGroundOperation.getAvailableRandomEnemyGroundGroup(airGroup, missionType);
                    if (groundGroup == null)
                    {
                        if (stationary == null)
                        {
                            stationary = GeneratorGroundOperation.getAvailableRandomEnemyStationary(airGroup, missionType);
                            if (stationary == null)
                            {
                                break;
                            }
                        }
                    }
                }
                double altitude = getRandomAltitude(aircraftParametersInfo);
                if (groundGroup != null)
                {
                    if (!EnableCreateTargetOperation || !groundGroup.MissionAssigned && GeneratorGroundOperation.CreateRandomGroundOperation(sectionFile, groundGroup))
                    {
                        int range = Random.Next((int)Math.Floor(AirGroup.GroundAttackRange / 1.5), (int)Math.Ceiling(AirGroup.GroundAttackRange * 1.5));
                        airGroup.GroundAttack(groundGroup, altitude, escortAirGroup, null, range);
                        result = true;
                    }
                    else
                    {
                        // airGroup = null;
                        groundGroup = null;
                    }
                }
                else if (stationary != null)
                {
                    airGroup.GroundAttack(stationary, altitude, escortAirGroup);
                    result = true;
                }
                reTry++;
            } while (!result && reTry < MaxRetryCreateOperation);
            return result;
        }

        private bool CreateAirOperationIntercept(ISectionFile sectionFile, BriefingFile briefingFile, AirGroup airGroup, AirGroup forcedOffensiveAirGroup, Spawn spawn)
        {
            bool result = false;
            AirGroup offensiveAirGroup = forcedOffensiveAirGroup;
            if (offensiveAirGroup == null)
            {
                GroundGroup targetGroundGroup;
                Stationary targetStationary;
                EMissionType offensiveMissionType;
                offensiveAirGroup = getAvailableRandomOffensiveAirGroup(airGroup, out offensiveMissionType, out targetGroundGroup, out targetStationary);
                if (offensiveAirGroup != null && !offensiveAirGroup.MissionAssigned && EnableCreateTargetOperation)
                {
                    CreateAirOperation(sectionFile, briefingFile, offensiveAirGroup, offensiveMissionType, false, null, null, null, targetGroundGroup, targetStationary, spawn);
                }
            }
            if (offensiveAirGroup != null)
            {
                airGroup.Intercept(offensiveAirGroup);
                result = true;
            }
            return result;
        }

        private bool CreateAirOperationEscort(ISectionFile sectionFile, BriefingFile briefingFile, AirGroup airGroup, AirGroup forcedEscortedAirGroup, Spawn spawn)
        {
            bool result = false;
            AirGroup escortedAirGroup = forcedEscortedAirGroup;
            if (escortedAirGroup == null)
            {
                escortedAirGroup = getAvailableRandomEscortedAirGroup(airGroup);
                if (escortedAirGroup != null && !escortedAirGroup.MissionAssigned)
                {
                    List<EMissionType> availableEscortedMissionTypes = new List<EMissionType>();
                    IEnumerable<EMissionType> missionTypes = CampaignInfo.GetAircraftInfo(escortedAirGroup.Class).MissionTypes;
                    foreach (EMissionType targetMissionType in missionTypes)
                    {
                        if (isMissionTypeAvailable(escortedAirGroup, targetMissionType) && isMissionTypeEscorted(targetMissionType))
                        {
                            availableEscortedMissionTypes.Add(targetMissionType);
                        }
                    }
                    if (availableEscortedMissionTypes.Count > 0 && EnableCreateTargetOperation)
                    {
                        int escortedMissionTypeIndex = Random.Next(availableEscortedMissionTypes.Count);
                        EMissionType randomEscortedMissionType = availableEscortedMissionTypes[escortedMissionTypeIndex];
                        CreateAirOperation(sectionFile, briefingFile, escortedAirGroup, randomEscortedMissionType, true, airGroup, null, null, null, null, spawn);
                    }
                }
            }
            if (escortedAirGroup != null)
            {
                airGroup.Escort(escortedAirGroup);
                result = true;
            }
            return result;
        }

        private bool CreateAirOperationCover(ISectionFile sectionFile, BriefingFile briefingFile, AirGroup airGroup, AirGroup forcedOffensiveAirGroup, Spawn spawn)
        {
            bool result = false;
            AirGroup offensiveAirGroup = forcedOffensiveAirGroup;
            if (offensiveAirGroup == null)
            {
                GroundGroup targetGroundGroup;
                Stationary targetStationary;
                EMissionType offensiveMissionType;
                offensiveAirGroup = getAvailableRandomOffensiveAirGroup(airGroup, out offensiveMissionType, out targetGroundGroup, out targetStationary);
                if (offensiveAirGroup != null && !offensiveAirGroup.MissionAssigned && EnableCreateTargetOperation)
                {
                    CreateAirOperation(sectionFile, briefingFile, offensiveAirGroup, offensiveMissionType, false, null, null, null, targetGroundGroup, targetStationary, spawn);
                }
            }
            if (offensiveAirGroup != null)
            {
                if (offensiveAirGroup.Altitude.HasValue && (offensiveAirGroup.TargetGroundGroup != null || offensiveAirGroup.TargetStationary != null))
                {
                    int range = Random.Next((int)Math.Floor(AirGroup.CoverRange / 1.5), (int)Math.Ceiling(AirGroup.CoverRange * 1.5));
                    airGroup.Cover(offensiveAirGroup, offensiveAirGroup.Altitude.Value, null, range);
                    result = true;
                }
            }
            return result;
        }

        private bool CreateAirOperationFollow(ISectionFile sectionFile, BriefingFile briefingFile, AirGroup airGroup, Spawn spawn)
        {
            bool result = false;
            IEnumerable<AirGroup> airGroupList = EnableMissionMultiAssign ? AllAirGroups : AvailableAirGroups;
            var friendlyAirGroups = airGroupList.Where(x => x.ArmyIndex == airGroup.ArmyIndex && x != airGroup).ToArray();
            AirGroup followedAirGroup = null;
            foreach (var item in friendlyAirGroups)
            {
                AirGroup targetAirGroup = getRandomAirGroupBasedOnDistance(friendlyAirGroups, airGroup);
                if (targetAirGroup.MissionAssigned)
                {
                    if (GetTargetPoint(targetAirGroup) != null)
                    {
                        followedAirGroup = targetAirGroup;
                        break;
                    }
                }
                else
                {
                    EMissionType? missionType = GetRandomMissionType(targetAirGroup);
                    if (missionType != null)
                    {
                        if (EnableCreateTargetOperation)
                        {
                            CreateAirOperation(sectionFile, briefingFile, targetAirGroup, missionType.Value, true, null, null, null, null, null, spawn);
                            if (GetTargetPoint(targetAirGroup) != null)
                            {
                                followedAirGroup = targetAirGroup;
                                break;
                            }
                        }
                        else
                        {
                            followedAirGroup = targetAirGroup;
                            break;
                        }
                    }
                }
            }
            if (followedAirGroup != null)
            {
                airGroup.Follow(followedAirGroup);
                result = true;
            }

            return result;
        }

        private bool CreateAirOperationHunting(ISectionFile sectionFile, BriefingFile briefingFile, AirGroup airGroup, Spawn spawn)
        {
            bool result = false;
            IEnumerable<AirGroup> airGroupList = EnableMissionMultiAssign ? AllAirGroups : AvailableAirGroups;
            var enemyAirGroups = airGroupList.Where(x => x.ArmyIndex == Army.Enemy(airGroup.ArmyIndex)).ToArray();
            AirGroup huntingAirGroup = null;
            foreach (var item in enemyAirGroups)
            {
                AirGroup targetAirGroup = getRandomAirGroupBasedOnDistance(enemyAirGroups, airGroup);
                if (targetAirGroup.MissionAssigned)
                {
                    if (GetTargetPoint(targetAirGroup) != null && GetTargetAltitude(targetAirGroup) != null)
                    {
                        huntingAirGroup = targetAirGroup;
                        break;
                    }
                }
                else
                {
                    EMissionType? missionType = GetRandomMissionType(targetAirGroup);
                    if (missionType != null)
                    {
                        if (EnableCreateTargetOperation)
                        {
                            CreateAirOperation(sectionFile, briefingFile, targetAirGroup, missionType.Value, true, null, null, null, null, null, spawn);
                            if (GetTargetPoint(targetAirGroup) != null && GetTargetAltitude(targetAirGroup) != null)
                            {
                                huntingAirGroup = targetAirGroup;
                                break;
                            }
                        }
                        else
                        {
                            huntingAirGroup = targetAirGroup;
                            break;
                        }
                    }
                }
            }

            if (huntingAirGroup != null)
            {
                Point2d? targetPoint = GetTargetPoint(huntingAirGroup);
                double? targetAltitude = GetTargetAltitude(huntingAirGroup);
                airGroup.Hunting(targetPoint.Value, targetAltitude.Value);
                result = true;
            }

            return result;
        }

        private bool CreateAirOperationTransfer(ISectionFile sectionFile, BriefingFile briefingFile, AirGroup airGroup)
        {
            AiAirport[] aiAirports = GamePlay.gpAirports();
            Point3d point;
            IEnumerable<AiAirport> aiAirportFriendly = aiAirports.Where(x => { point = x.Pos(); return (GamePlay.gpFrontArmy(point.x, point.y) == airGroup.ArmyIndex); });
            AiAirport aiAirportTarget = GetRandomAiAirportBasedOnDistance(aiAirportFriendly, airGroup.Position);
            if (aiAirportTarget != null)
            {
                AircraftInfo aircraftInfo = CampaignInfo.GetAircraftInfo(airGroup.Class);
                AircraftParametersInfo aircraftParam = getAvailableRandomAircratParametersInfo(aircraftInfo, EMissionType.TRANSFER);
                int startAlt = aircraftParam != null && aircraftParam.MinAltitude != null ? Math.Max((int)aircraftParam.MinAltitude.Value, Spawn.SelectStartAltitude) : Spawn.SelectStartAltitude;
                int endAlt = aircraftParam != null && aircraftParam.MaxAltitude != null ? Math.Min((int)aircraftParam.MaxAltitude.Value, Spawn.SelectEndAltitude) : Spawn.SelectEndAltitude;
                double altitude = Random.Next(startAlt, endAlt + 1);
                airGroup.Transfer(altitude, aiAirportTarget);
                return true;
            }
            return false;
        }

#if false

        private bool CreateAirOperationEscorted(ISectionFile sectionFile, BriefingFile briefingFile, AirGroup airGroup, AirGroup airGroupEscorted, Spawn spawn)
        {
            bool result = false;
            // TODO: Consider calling CreateAirOperation with a forcedEscortedAirGroup.
            AvailableAirGroups.Remove(airGroup);
            // jamRunway(airGroup);

            AircraftInfo aircraftInfo = CampaignInfo.GetAircraftInfo(airGroup.Class);
            AircraftParametersInfo aircraftParametersInfo = getAvailableRandomAircratParametersInfo(aircraftInfo, EMissionType.ESCORT);
            AircraftLoadoutInfo aircraftLoadoutInfo = aircraftInfo.GetAircraftLoadoutInfo(aircraftParametersInfo.LoadoutId);
            airGroup.Weapons = aircraftLoadoutInfo.Weapons;
            OptimizeSpawn(airGroup, spawn, aircraftParametersInfo);                                        // Friendly Air Group
            AssignedAirGroups.Add(airGroup);

            airGroup.Escort(airGroupEscorted);

            getRandomFlightSize(airGroup, EMissionType.ESCORT);
            airGroup.Skill = getRandomSkill(aircraftInfo.AircraftType);
            airGroup.SetSpawn(spawn);
            GeneratorBriefing.CreateBriefing(briefingFile, airGroup, EMissionType.ESCORT, null);
            airGroup.WriteTo(sectionFile);
            return result;
        }
#endif

        private bool CreateAirOperationDefensiv(ISectionFile sectionFile, BriefingFile briefingFile, AirGroup defensiveAirGroup, AirGroup offensiveAirGroup, Spawn spawn)
        {
            bool result = false;

            List<EMissionType> availableDefensiveMissionTypes = new List<EMissionType>();
            IEnumerable<EMissionType> missionTypes = CampaignInfo.GetAircraftInfo(defensiveAirGroup.Class).MissionTypes;
            foreach (EMissionType targetMissionType in missionTypes)
            {
                // "isMissionTypeAvailable" checks if there is any air group available for 
                // a offensive mission. As the air group for the offensive mission is already 
                // determined, the defensive mission type is always available.
                if (/*isMissionTypeAvailable(defensiveAirGroup, targetMissionType) &&*/ isMissionTypeDefensive(targetMissionType))
                {
                    availableDefensiveMissionTypes.Add(targetMissionType);
                }
            }

            if (availableDefensiveMissionTypes.Count > 0)
            {
                int defensiveMissionTypeIndex = Random.Next(availableDefensiveMissionTypes.Count);
                EMissionType randomDefensiveMissionType = availableDefensiveMissionTypes[defensiveMissionTypeIndex];
                if (EnableCreateTargetOperation)
                {
                    CreateAirOperation(sectionFile, briefingFile, defensiveAirGroup, randomDefensiveMissionType, false, null, null, offensiveAirGroup, null, null, spawn);
                }
                result = true;
            }
            return result;
        }

        #endregion

        #region Get Available AirGroup

        public AirGroup GetAvailableRandomAirGroup()
        {
            if (HasAvailableAirGroup)
            {
                return AvailableAirGroups[Random.Next(AvailableAirGroups.Count)];
            }
            return null;
        }

        private IEnumerable<AirGroup> getAvailableOffensiveAirGroups(int opposingArmyIndex)
        {
            List<AirGroup> airGroups = new List<AirGroup>();
            IEnumerable<AirGroup> airGroupList = EnableMissionMultiAssign ? AllAirGroups : AvailableAirGroups;
            foreach (AirGroup airGroup in airGroupList)
            {
                if (airGroup.ArmyIndex == Army.Enemy(opposingArmyIndex))
                {
                    IEnumerable<EMissionType> missionTypes = CampaignInfo.GetAircraftInfo(airGroup.Class).MissionTypes;
                    foreach (EMissionType missionType in missionTypes)
                    {
                        if (isMissionTypeOffensive(missionType) && isMissionTypeAvailable(airGroup, missionType))
                        {
                            airGroups.Add(airGroup);
                            break;
                        }
                    }
                }
            }
            return airGroups;
        }

        private bool HasAvailableOffensiveAirGroups(int opposingArmyIndex)
        {
            IEnumerable<AirGroup> airGroupList = EnableMissionMultiAssign ? AllAirGroups : AvailableAirGroups;
            foreach (AirGroup airGroup in airGroupList)
            {
                if (airGroup.ArmyIndex == Army.Enemy(opposingArmyIndex))
                {
                    IEnumerable<EMissionType> missionTypes = CampaignInfo.GetAircraftInfo(airGroup.Class).MissionTypes;
                    foreach (EMissionType missionType in missionTypes)
                    {
                        if (isMissionTypeOffensive(missionType) && isMissionTypeAvailable(airGroup, missionType))
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        public AirGroup getAvailableRandomOffensiveAirGroup(AirGroup defensiveAirGroup, out EMissionType offensiveMissionType, out GroundGroup targetGroundGroup, out Stationary targetStationary)
        {
            IEnumerable<AirGroup> airGroups = getAvailableOffensiveAirGroups(defensiveAirGroup.ArmyIndex).Except(new AirGroup[] { defensiveAirGroup });
            if (airGroups.Any())
            {
                List<GroundGroup> possibleTargetGroundGroups = new List<GroundGroup>();
                List<Stationary> possibleTargetStationaries = new List<Stationary>();
                Dictionary<GroundGroup, Dictionary<AirGroup, EMissionType>> possibleOffensiveAirGroups = new Dictionary<GroundGroup, Dictionary<AirGroup, EMissionType>>();
                Dictionary<Stationary, Dictionary<AirGroup, EMissionType>> possibleOffensiveAirGroupsStationary = new Dictionary<Stationary, Dictionary<AirGroup, EMissionType>>();

                foreach (AirGroup possibleOffensiveAirGroup in airGroups)
                {
                    List<EMissionType> availableOffensiveMissionTypes = new List<EMissionType>();
                    IEnumerable<EMissionType> missionTypes = CampaignInfo.GetAircraftInfo(possibleOffensiveAirGroup.Class).MissionTypes;
                    foreach (EMissionType targetMissionType in missionTypes)
                    {
                        if (isMissionTypeAvailable(possibleOffensiveAirGroup, targetMissionType) && isMissionTypeOffensive(targetMissionType))
                        {
                            availableOffensiveMissionTypes.Add(targetMissionType);
                        }
                    }

                    if (availableOffensiveMissionTypes.Any())
                    {
                        int offensiveMissionTypeIndex = Random.Next(availableOffensiveMissionTypes.Count);
                        EMissionType possibleOffensiveMissionType = availableOffensiveMissionTypes[offensiveMissionTypeIndex];

                        GroundGroup possibleTargetGroundGroup = GeneratorGroundOperation.getAvailableRandomEnemyGroundGroup(possibleOffensiveAirGroup, possibleOffensiveMissionType);
                        Stationary possibleTargetStationary = GeneratorGroundOperation.getAvailableRandomEnemyStationary(possibleOffensiveAirGroup, possibleOffensiveMissionType);

                        if (possibleTargetGroundGroup != null)
                        {
                            possibleTargetGroundGroups.Add(possibleTargetGroundGroup);

                            if (!possibleOffensiveAirGroups.ContainsKey(possibleTargetGroundGroup))
                            {
                                possibleOffensiveAirGroups.Add(possibleTargetGroundGroup, new Dictionary<AirGroup, EMissionType>());
                            }
                            possibleOffensiveAirGroups[possibleTargetGroundGroup].Add(possibleOffensiveAirGroup, possibleOffensiveMissionType);
                        }
                        else if (possibleTargetStationary != null)
                        {
                            possibleTargetStationaries.Add(possibleTargetStationary);

                            if (!possibleOffensiveAirGroupsStationary.ContainsKey(possibleTargetStationary))
                            {
                                possibleOffensiveAirGroupsStationary.Add(possibleTargetStationary, new Dictionary<AirGroup, EMissionType>());
                            }
                            possibleOffensiveAirGroupsStationary[possibleTargetStationary].Add(possibleOffensiveAirGroup, possibleOffensiveMissionType);
                        }
                    }
                }

                // Select target considering the distance to the defensiveAirGroup
                targetGroundGroup = GeneratorGroundOperation.getRandomTargetBasedOnRange(possibleTargetGroundGroups, defensiveAirGroup);
                targetStationary = GeneratorGroundOperation.getRandomTargetBasedOnRange(possibleTargetStationaries, defensiveAirGroup);

                if (targetGroundGroup != null && targetStationary != null)
                {
                    // Randomly select one of them
                    int type = Random.Next(2);
                    if (type == 0)
                    {
                        targetStationary = null;
                    }
                    else
                    {
                        targetGroundGroup = null;
                    }
                }

                // Now select the offensiveAirGroup for the selected target, also considering the distance to the target
                if (targetGroundGroup != null && possibleOffensiveAirGroups.ContainsKey(targetGroundGroup))
                {
                    targetStationary = null;

                    var offensiveAirGroups = possibleOffensiveAirGroups[targetGroundGroup].Keys;
                    AirGroup offensiveAirGroup = getRandomAirGroupBasedOnDistance(offensiveAirGroups, targetGroundGroup.Position);
                    offensiveMissionType = possibleOffensiveAirGroups[targetGroundGroup][offensiveAirGroup];
                    return offensiveAirGroup;
                }
                else if (targetStationary != null && possibleOffensiveAirGroupsStationary.ContainsKey(targetStationary))
                {
                    targetGroundGroup = null;

                    var offensiveAirGroups = possibleOffensiveAirGroupsStationary[targetStationary].Keys;
                    AirGroup offensiveAirGroup = getRandomAirGroupBasedOnDistance(offensiveAirGroups, targetStationary.Position);
                    offensiveMissionType = possibleOffensiveAirGroupsStationary[targetStationary][offensiveAirGroup];
                    return offensiveAirGroup;
                }
                else
                {
                    targetGroundGroup = null;
                    targetStationary = null;
                    offensiveMissionType = EMissionType.RECON;
                    return null;
                }
            }
            else
            {
                targetGroundGroup = null;
                targetStationary = null;
                offensiveMissionType = EMissionType.RECON;
                return null;
            }
        }

        private IEnumerable<AirGroup> getAvailableDefensiveAirGroups(int opposingArmyIndex)
        {
            List<AirGroup> airGroups = new List<AirGroup>();
            IEnumerable<AirGroup> airGroupList = EnableMissionMultiAssign ? AllAirGroups : AvailableAirGroups;
            foreach (AirGroup airGroup in airGroupList)
            {
                if (airGroup.ArmyIndex == Army.Enemy(opposingArmyIndex))
                {
                    IEnumerable<EMissionType> missionTypes = CampaignInfo.GetAircraftInfo(airGroup.Class).MissionTypes;
                    foreach (EMissionType missionType in missionTypes)
                    {
                        if (isMissionTypeDefensive(missionType) && isMissionTypeAvailable(airGroup, missionType))
                        {
                            airGroups.Add(airGroup);
                            break;
                        }
                    }
                }
            }
            return airGroups;
        }

        public AirGroup getAvailableRandomDefensiveAirGroup(AirGroup offensiveAirGroup)
        {
            IEnumerable<AirGroup> airGroups = getAvailableDefensiveAirGroups(offensiveAirGroup.ArmyIndex).Except(new AirGroup[] { offensiveAirGroup });
            if (airGroups.Any())
            {
                if (offensiveAirGroup.Altitude != null && offensiveAirGroup.Altitude.HasValue)
                {
                    if (offensiveAirGroup.TargetGroundGroup != null)
                    {
                        Point2d pos = offensiveAirGroup.TargetGroundGroup.Position;
                        Point3d targetPosition = new Point3d(pos.x, pos.y, 0.0);
                        AirGroup defensiveAirGroup = getRandomAirGroupBasedOnDistance(airGroups, targetPosition);
                        return defensiveAirGroup;
                    }
                    else if (offensiveAirGroup.TargetStationary != null)
                    {
                        Point2d pos = offensiveAirGroup.TargetStationary.Position;
                        Point3d targetPosition = new Point3d(pos.x, pos.y, 0.0);
                        AirGroup defensiveAirGroup = getRandomAirGroupBasedOnDistance(airGroups, targetPosition);
                        return defensiveAirGroup;
                    }
                    else if (offensiveAirGroup.TargetArea != null && offensiveAirGroup.TargetArea.HasValue)
                    {
                        Point2d pos = offensiveAirGroup.TargetArea.Value;
                        Point3d targetPosition = new Point3d(pos.x, pos.y, 0.0);
                        AirGroup defensiveAirGroup = getRandomAirGroupBasedOnDistance(airGroups, targetPosition);
                        return defensiveAirGroup;
                    }
                }
            }

            return null;
        }

        private IEnumerable<AirGroup> getAvailableEscortedAirGroups(int armyIndex)
        {
            List<AirGroup> airGroups = new List<AirGroup>();
            IEnumerable<AirGroup> airGroupList = EnableMissionMultiAssign ? AllAirGroups : AvailableAirGroups;
            foreach (AirGroup airGroup in airGroupList)
            {
                if (airGroup.ArmyIndex == armyIndex)
                {
                    IEnumerable<EMissionType> missionTypes = CampaignInfo.GetAircraftInfo(airGroup.Class).MissionTypes;
                    foreach (EMissionType missionType in missionTypes)
                    {
                        if (isMissionTypeEscorted(missionType) && isMissionTypeAvailable(airGroup, missionType))
                        {
                            airGroups.Add(airGroup);
                            break;
                        }
                    }
                }
            }
            return airGroups;
        }

        private bool HasAvailableEscortedAirGroups(int armyIndex)
        {
            IEnumerable<AirGroup> airGroupList = EnableMissionMultiAssign ? AllAirGroups : AvailableAirGroups;
            foreach (AirGroup airGroup in airGroupList)
            {
                if (airGroup.ArmyIndex == armyIndex)
                {
                    IEnumerable<EMissionType> missionTypes = CampaignInfo.GetAircraftInfo(airGroup.Class).MissionTypes;
                    foreach (EMissionType missionType in missionTypes)
                    {
                        if (isMissionTypeEscorted(missionType) && isMissionTypeAvailable(airGroup, missionType))
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        private AirGroup getAvailableRandomEscortedAirGroup(AirGroup escortAirGroup)
        {
            IEnumerable<AirGroup> airGroups = getAvailableEscortedAirGroups(escortAirGroup.ArmyIndex).Except(new AirGroup[] { escortAirGroup });
            if (airGroups.Any())
            {
                return getRandomAirGroupBasedOnDistance(airGroups, escortAirGroup);
            }
            else
            {
                return null;
            }
        }

        private AirGroup getAvailableRandomEscortAirGroup(AirGroup escortedAirGroup)
        {
            List<AirGroup> airGroups = new List<AirGroup>();
            IEnumerable<AirGroup> airGroupList = EnableMissionMultiAssign ? AllAirGroups : AvailableAirGroups;
            foreach (AirGroup airGroup in airGroupList)
            {
                if (airGroup.ArmyIndex == escortedAirGroup.ArmyIndex && airGroup != escortedAirGroup)
                {
                    if (CampaignInfo.GetAircraftInfo(airGroup.Class).MissionTypes.Contains(EMissionType.ESCORT))
                    {
                        airGroups.Add(airGroup);
                    }
                }
            }

            if (airGroups.Any())
            {
                return getRandomAirGroupBasedOnDistance(airGroups, escortedAirGroup);
            }
            else
            {
                return null;
            }
        }

        private AircraftParametersInfo getAvailableRandomAircratParametersInfo(AircraftInfo aircraftInfo, EMissionType missionType)
        {
            IList<AircraftParametersInfo> aircraftParametersInfos = aircraftInfo.GetAircraftParametersInfo(missionType);
            int aircraftParametersInfoIndex = Random.Next(aircraftParametersInfos.Count);
            return aircraftParametersInfos[aircraftParametersInfoIndex];
        }

        //private AirGroup getAvailableRandomInterceptAirGroup(AirGroup interceptedAirUnit)
        //{
        //    List<AirGroup> airGroups = new List<AirGroup>();
        //    foreach (AirGroup airGroup in AvailableAirGroups)
        //    {
        //        if (airGroup.ArmyIndex == Army.Enemy(interceptedAirUnit.ArmyIndex))
        //        {
        //            if (CampaignInfo.GetAircraftInfo(airGroup.Class).MissionTypes.Contains(EMissionType.INTERCEPT))
        //            {
        //                airGroups.Add(airGroup);
        //            }
        //        }
        //    }

        //    if (airGroups.Count > 0)
        //    {
        //        int interceptAirGroupIndex = getRandomIndex(ref airGroups, interceptedAirUnit.Position);
        //        AirGroup interceptAirGroup = airGroups[interceptAirGroupIndex];

        //        return interceptAirGroup;
        //    }
        //    else
        //    {
        //        return null;
        //    }
        //}

        #endregion

        #region Check Mission Type 

        private bool isMissionTypeEscorted(EMissionType missionType)
        {
            return missionType == EMissionType.ATTACK_ARMOR
                || missionType == EMissionType.ATTACK_SHIP
                || missionType == EMissionType.ATTACK_VEHICLE
                || missionType == EMissionType.ATTACK_TRAIN

                || missionType == EMissionType.ATTACK_RADAR
                || missionType == EMissionType.ATTACK_AIRCRAFT
                || missionType == EMissionType.ATTACK_ARTILLERY
                || missionType == EMissionType.ATTACK_DEPOT;
        }

        public bool isMissionTypeOffensive(EMissionType missionType)
        {
            return missionType == EMissionType.ARMED_MARITIME_RECON
                || missionType == EMissionType.ARMED_RECON
                || missionType == EMissionType.ATTACK_ARMOR
                || missionType == EMissionType.ATTACK_SHIP
                || missionType == EMissionType.ATTACK_VEHICLE
                || missionType == EMissionType.ATTACK_TRAIN
                || missionType == EMissionType.MARITIME_RECON
                || missionType == EMissionType.RECON

                || missionType == EMissionType.ATTACK_RADAR
                || missionType == EMissionType.ATTACK_AIRCRAFT
                || missionType == EMissionType.ATTACK_ARTILLERY
                || missionType == EMissionType.ATTACK_DEPOT;
        }

        public bool isMissionTypeDefensive(EMissionType missionType)
        {
            return missionType == EMissionType.INTERCEPT
                || missionType == EMissionType.COVER;
        }

        public bool isMissionTypeAvailable(AirGroup airGroup, EMissionType missionType)
        {
            if (missionType == EMissionType.COVER)
            {
                return HasAvailableOffensiveAirGroups(airGroup.ArmyIndex);
            }
            else if (missionType == EMissionType.ARMED_MARITIME_RECON)
            {
                return GeneratorGroundOperation.HasAvailableEnemyGroundGroups(airGroup.ArmyIndex, new EGroundGroupType[] { EGroundGroupType.Ship }) ||
                        GeneratorGroundOperation.HasAvailableEnemyStationaries(airGroup.ArmyIndex, new EStationaryType[] { EStationaryType.Ship });
            }
            else if (missionType == EMissionType.ARMED_RECON)
            {
                return GeneratorGroundOperation.HasAvailableEnemyGroundGroups(airGroup.ArmyIndex, new EGroundGroupType[] { EGroundGroupType.Armor, EGroundGroupType.Vehicle, EGroundGroupType.Train }) ||
                        GeneratorGroundOperation.HasAvailableEnemyStationaries(airGroup.ArmyIndex, new EStationaryType[] { EStationaryType.Artillery, EStationaryType.Flak, EStationaryType.Ammo, EStationaryType.Weapons,
                                                                EStationaryType.Aircraft, EStationaryType.Radar, EStationaryType.Depot, EStationaryType.Car, EStationaryType.ConstCar, });
            }
            else if (missionType == EMissionType.ATTACK_ARMOR)
            {
                return GeneratorGroundOperation.HasAvailableEnemyGroundGroups(airGroup.ArmyIndex, new EGroundGroupType[] { EGroundGroupType.Armor });
            }
            else if (missionType == EMissionType.ATTACK_RADAR)
            {
                return GeneratorGroundOperation.HasAvailableEnemyStationaries(airGroup.ArmyIndex, new EStationaryType[] { EStationaryType.Radar });
            }
            else if (missionType == EMissionType.ATTACK_AIRCRAFT)
            {
                return GeneratorGroundOperation.HasAvailableEnemyStationaries(airGroup.ArmyIndex, new EStationaryType[] { EStationaryType.Aircraft });
            }
            else if (missionType == EMissionType.ATTACK_ARTILLERY)
            {
                return GeneratorGroundOperation.HasAvailableEnemyStationaries(airGroup.ArmyIndex, new EStationaryType[] { EStationaryType.Artillery, EStationaryType.Flak, EStationaryType.Ammo, EStationaryType.Weapons, });
            }
            else if (missionType == EMissionType.ATTACK_DEPOT)
            {
                return GeneratorGroundOperation.HasAvailableEnemyStationaries(airGroup.ArmyIndex, new EStationaryType[] { EStationaryType.Depot });
            }
            else if (missionType == EMissionType.ATTACK_SHIP)
            {
                return GeneratorGroundOperation.HasAvailableEnemyGroundGroups(airGroup.ArmyIndex, new EGroundGroupType[] { EGroundGroupType.Ship }) ||
                        GeneratorGroundOperation.HasAvailableEnemyStationaries(airGroup.ArmyIndex, new EStationaryType[] { EStationaryType.Ship });
            }
            else if (missionType == EMissionType.ATTACK_VEHICLE)
            {
                return GeneratorGroundOperation.HasAvailableEnemyGroundGroups(airGroup.ArmyIndex, new EGroundGroupType[] { EGroundGroupType.Vehicle });
            }
            else if (missionType == EMissionType.ATTACK_TRAIN)
            {
                return GeneratorGroundOperation.HasAvailableEnemyGroundGroups(airGroup.ArmyIndex, new EGroundGroupType[] { EGroundGroupType.Train });
            }
            else if (missionType == EMissionType.ESCORT)
            {
                return HasAvailableEscortedAirGroups(airGroup.ArmyIndex);
            }
            else if (missionType == EMissionType.INTERCEPT)
            {
                return HasAvailableOffensiveAirGroups(airGroup.ArmyIndex);
            }
            else if (missionType == EMissionType.MARITIME_RECON)
            {
                return GeneratorGroundOperation.HasAvailableEnemyGroundGroups(airGroup.ArmyIndex, new EGroundGroupType[] { EGroundGroupType.Ship }) ||
                        GeneratorGroundOperation.HasAvailableEnemyStationaries(airGroup.ArmyIndex, new EStationaryType[] { EStationaryType.Ship });
            }
            else if (missionType == EMissionType.RECON)
            {
                return GeneratorGroundOperation.HasAvailableEnemyGroundGroups(airGroup.ArmyIndex, new EGroundGroupType[] { EGroundGroupType.Armor, EGroundGroupType.Vehicle, EGroundGroupType.Train }) ||
                        GeneratorGroundOperation.HasAvailableEnemyStationaries(airGroup.ArmyIndex, new EStationaryType[] { EStationaryType.Aircraft, EStationaryType.Artillery, EStationaryType.Flak, EStationaryType.Radar });
            }
            else if (missionType == EMissionType.FOLLOW)
            {
                return EnableMissionMultiAssign ? AllAirGroups.Any(x => x.ArmyIndex == airGroup.ArmyIndex && x != airGroup) : AvailableAirGroups.Any(x => x.ArmyIndex == airGroup.ArmyIndex && x != airGroup);
            }
            else if (missionType == EMissionType.HUNTING)
            {
                return EnableMissionMultiAssign ? AllAirGroups.Any(x => x.ArmyIndex == Army.Enemy(airGroup.ArmyIndex)) : AvailableAirGroups.Any(x => x.ArmyIndex == Army.Enemy(airGroup.ArmyIndex));
            }
            else if (missionType == EMissionType.TRANSFER)
            {
                return true;
            }
            else
            {
                throw new NotImplementedException(string.Format("Invalid MissionType[{0}]", missionType.ToString()));
            }
        }

        private bool isMissionTypePassivelyGenerated(EMissionType missionType)
        {
            return missionType == EMissionType.FOLLOW
                || missionType == EMissionType.ESCORT
                /*|| missionType == EMissionType.HUNTING
                || missionType == EMissionType.INTERCEPT
                || missionType == EMissionType.COVER*/;
        }

        #endregion

        public IEnumerable<EMissionType> GetAvailableMissionTypes(AirGroup airGroup)
        {
            return CampaignInfo.GetAircraftInfo(airGroup.Class).MissionTypes.Where(x => isMissionTypeAvailable(airGroup, x));
        }

        private Skills CreateAISkills(ESkillSet aISkill)
        {
            Skills skills;
            if (aISkill == ESkillSet.Default)
            {
                skills = new Skills();
            }
            else if (aISkill == ESkillSet.Random)
            {
                skills = new Skills(Skill.TweakedSkills);
            }
            else if (aISkill == ESkillSet.UserSettings)
            {
                skills = new Skills(Skills.Except(Skill.TweakedSkills).Except(Skill.SystemSkills));
            }
            else
            {
                skills = Skills.Create(aISkill);
            }
            Skills.UpdateSkillValue(skills, Skills);
            return skills;
        }

        private void SetSkill(AirGroup airGroup, EAircraftType aircraftType, Skill[] skill)
        {
            if (skill != null && skill.Length > 0)
            {
                if (skill.Length == 1)
                {
                    if (skill.First() != Skill.Default)
                    {
                        airGroup.Skill = skill.First().ToString();
                        airGroup.Skills.Clear();
                    }
                }
                else
                {
                    airGroup.Skill = string.Empty;
                    airGroup.Skills.Clear();
                    int s = 0;
                    for (int i = 0; i < airGroup.Flights.Count(); i++)
                    {
                        IList<string> list = airGroup.Flights[i];
                        for (int j = 0; j < list.Count; j++)
                        {
                            airGroup.Skills.Add(i * list.Count + j, skill[s].ToString());
                            if (s + 1 < skill.Length)
                            {
                                s++;
                            }
                        }
                    }
                }
            }
            else
            {
                if (AISkill == ESkillSet.Default)
                {
                    ;
                }
                else if (AISkill >= ESkillSet.Rookie && AISkill <= ESkillSet.Ace || AISkill == ESkillSet.UserSettings)
                {
                    airGroup.Skill = getRandomSkill();
                    airGroup.Skills.Clear();
                }
                else/* if (AISkill == ESkillSet.Random)*/
                {
                    airGroup.Skill = getRandomSkill(aircraftType);
                    airGroup.Skills.Clear();
                }
            }
        }

        private void SetRange(IEnumerable<Point3d> points)
        {
            wRECTF range = MapUtil.GetRange(points);
            Debug.WriteLine("SetRange({0},{1})-({2},{3})[{4},{5}]", range.x1, range.y1, range.x2, range.y2, range.x2 - range.x1 + 1, range.y2 - range.y1 + 1);
            Range = range;
        }

        private void SetFlight(AirGroup airGroup, EMissionType missionType, int flight)
        {
            if (flight == (int)EFlight.Default)
            {
                SetFlight(airGroup, missionType);
            }
            else if (flight == (int)EFlight.MissionDefault)
            {
                int flightCount;
                int flightSize;
                airGroup.GetFlight(out flightCount, out flightSize);
                if (SetFlightRate(airGroup, ref flightCount, ref flightSize))
                {
                    airGroup.SetFlights(flightCount, flightSize);
                }
            }
            else if (flight > 0)
            {
                int flightCount = Flight.Count(flight);
                int flightSize = Flight.Size(flight);

                SetFlightRate(airGroup, ref flightCount, ref flightSize);

                airGroup.SetFlights(flightCount, flightSize);
            }
        }

        private bool SetFlightRate(AirGroup airGroup, ref int flightCount, ref int flightSize)
        {
            double rate = MissionStatusRate(airGroup);      // 4 -> 2  = 0.5, 8 -> 6, 0.75
            if (rate < 1)
            {
                int oldCount = flightCount;
                flightCount = Math.Min(flightCount, Math.Max(1, (int)Math.Ceiling(flightCount * rate)));
                flightSize = Math.Min(flightSize, Math.Max(1, (int)Math.Ceiling(flightSize * rate * oldCount / flightCount)));
                return true;
            }
            return false;
        }

        private double MissionStatusRate(AirGroup airGroup)
        {
            var result = MissionStatus != null ? MissionStatus.AirGroups.Where(x => string.Compare(x.SquadronName, airGroup.SquadronName) == 0).FirstOrDefault() : null;
            return result != null && result.InitNums > 0 ? (result.InitNums - result.DiedNums) / (float)result.InitNums : 1.0;
        }

        private void SetFlight(AirGroup airGroup, EMissionType missionType)
        {
            AirGroupInfo airGroupInfo = airGroup.AirGroupInfo;
            int flightCount;
            int flightSize;
            Flight.GetOptimaizeValue(out flightCount, out flightSize, missionType, airGroupInfo.FlightCount, airGroupInfo.FlightSize,
                                        FlightCount, FlightSize, airGroup.TargetAirGroup != null ? airGroup.TargetAirGroup.Flights.Count : 0);

            SetFlightRate(airGroup, ref flightCount, ref flightSize);

            airGroup.SetFlights(flightCount, flightSize);
        }

        private Point2d? GetTargetPoint(AirGroup airGroup)
        {
            if (airGroup.TargetArea != null && airGroup.TargetArea.HasValue)
            {
                return airGroup.TargetArea.Value;
            }
            else if (airGroup.TargetAirGroup != null && airGroup.TargetAirGroup.Waypoints != null && airGroup.TargetAirGroup.Waypoints.Count > 0)
            {
                int wayIdx = airGroup.TargetAirGroup.Waypoints.Count / 2;
                AirGroupWaypoint way = airGroup.TargetAirGroup.Waypoints[wayIdx];
                return new Point2d(way.X, way.Y);
            }
            else if (airGroup.TargetGroundGroup != null && airGroup.TargetGroundGroup.Waypoints != null && airGroup.TargetGroundGroup.Waypoints.Count > 0)
            {
                int wayIdx = airGroup.TargetGroundGroup.Waypoints.Count / 2;
                GroundGroupWaypoint way = airGroup.TargetGroundGroup.Waypoints[wayIdx];
                return new Point2d(way.X, way.Y);
            }
            else if (airGroup.TargetStationary != null)
            {
                return new Point2d(airGroup.TargetStationary.X, airGroup.TargetStationary.Y);
            }
            else if (airGroup.Waypoints != null && airGroup.Waypoints.Count > 0)
            {
                int wayIdx = airGroup.Waypoints.Count / 2;
                AirGroupWaypoint way = airGroup.Waypoints[wayIdx];
                return new Point2d(way.X, way.Y);
            }

            return null;
        }

        private double? GetTargetAltitude(AirGroup airGroup)
        {
            if (airGroup.Altitude != null && airGroup.Altitude.HasValue)
            {
                return airGroup.Altitude.Value;
            }
            else if (airGroup.TargetAirGroup != null && airGroup.TargetAirGroup.Waypoints != null && airGroup.TargetAirGroup.Waypoints.Count > 0)
            {
                int wayIdx = airGroup.TargetAirGroup.Waypoints.Count / 2;
                AirGroupWaypoint way = airGroup.TargetAirGroup.Waypoints[wayIdx];
                return way.Z;
            }
            else if (airGroup.Waypoints != null && airGroup.Waypoints.Count > 0)
            {
                int wayIdx = airGroup.Waypoints.Count / 2;
                AirGroupWaypoint way = airGroup.Waypoints[wayIdx];
                return way.Z;
            }

            return null;
        }

        #region Spawn Settings

        private void OptimizeSpawn(AirGroup airGroup, Spawn spawn, AircraftParametersInfo aircraftParametersInfo)
        {
            Debug.Write(string.Format("Optimize Spawn AirGroup Name={0} Pos={1}", airGroup.DisplayDetailName, airGroup.Position.ToString()));
            AirGroupWaypoint way = airGroup.Waypoints.FirstOrDefault();
            if (way != null)
            {
                // Altitude
                if (IsSpawnRandomAltitude(airGroup, spawn))
                {
                    SetSpawnRandomAltitude(airGroup, aircraftParametersInfo);
                }

                // Location
                if (IsSpawnRandomLocation(airGroup, spawn.Location))
                {
                    SetSpawnRandomLocation(airGroup);
                }
                else
                {
                    SetSpawnOptimaizeLocation(airGroup);
                }
            }

            // Time
            if (IsSpawnRandomTime(airGroup, spawn))
            {
                spawn.Time.IsRandom = true;
                spawn.Time.Value = Random.Next(spawn.Time.BeginSec, spawn.Time.EndSec + 1);
                Debug.WriteLine(string.Format(" => Time={0}", spawn.Time.Value));
            }
            else
            {
                spawn.Time.IsRandom = false;
                spawn.Time.Value = Spawn.SpawnTime.DefaultSec;
                Debug.WriteLine(string.Format(" => Time={0}", spawn.Time.Value));
            }
        }

        private bool IsSpawnRandomLocation(AirGroup airGroup, Spawn.SpawnLocation spawnLocation)
        {
            if (airGroup != null && spawnLocation != null)
            {
                if (string.Compare(airGroup.AirGroupKey, AirGroupPlayer.AirGroupKey, true) == 0)
                {
                    return spawnLocation.IsRandomizePlayer;
                }
                else
                {
                    return airGroup.ArmyIndex == AirGroupPlayer.ArmyIndex ? spawnLocation.IsRandomizeFriendly : spawnLocation.IsRandomizeEnemy;
                }
            }
            Debug.Assert(false);
            return false;
        }

        private bool IsSpawnRandomTime(AirGroup airGroup, Spawn spawn)
        {
            if (airGroup != null && spawn != null)
            {
                if (string.Compare(airGroup.AirGroupKey, AirGroupPlayer.AirGroupKey, true) == 0)
                {
                    return false;
                }
                else
                {
                    return airGroup.ArmyIndex == AirGroupPlayer.ArmyIndex ? spawn.IsRandomizeTimeFriendly : spawn.IsRandomizeTimeEnemy;
                }
            }
            Debug.Assert(false);
            return false;
        }

        private bool IsSpawnRandomAltitude(AirGroup airGroup, Spawn spawn)
        {
            if (airGroup != null && spawn != null)
            {
                if (string.Compare(airGroup.AirGroupKey, AirGroupPlayer.AirGroupKey, true) == 0)
                {
                    return spawn.Altitude == (int)ESpawn.Random;
                    // return false;
                }
                else
                {
                    return airGroup.ArmyIndex == AirGroupPlayer.ArmyIndex ? spawn.IsRandomizeAltitudeFriendly : spawn.IsRandomizeAltitudeEnemy;
                }
            }
            Debug.Assert(false);
            return false;
        }

        private void SetSpawnRandomAltitude(AirGroup airGroup, AircraftParametersInfo aircraftParametersInfo)
        {
            Point3d point = airGroup.Position;
            point.z = Random.Next((int)aircraftParametersInfo.MinAltitude.Value, (int)aircraftParametersInfo.MaxAltitude.Value + 1);  // + AircraftParameter Altitude Range
            airGroup.UpdateStartPoint(ref point, AirGroupWaypoint.AirGroupWaypointTypes.NORMFLY);
            Debug.Write(string.Format(" => Altitude Changed[AirStart]={0}", airGroup.Position.ToString()));
        }

        private void SetSpawnRandomLocation(AirGroup airGroup)
        {
            Point3d point = airGroup.Position;
            wRECTF range = Range;
            // Debug.WriteLine("Range({0},{1})-({2},{3})[{4},{5}]", range.x1, range.y1, range.x2, range.y2, range.x2 - range.x1 + 1, range.y2 - range.y1 + 1);
            if (airGroup.Airstart)
            {
                int reTry = -1;
                do
                {
                    point.x = Random.Next((int)range.x1, (int)range.x2 + 1);
                    point.y = Random.Next((int)range.y1, (int)range.y2 + 1);
                } while (GamePlay.gpFrontArmy(point.x, point.y) == Army.Enemy(airGroup.ArmyIndex) && reTry++ < MaxRetrySpawnRandomChange);
                airGroup.UpdateStartPoint(ref point);
                Debug.Write(string.Format(" => AirStart Pos Changed={0}", airGroup.Position.ToString()));
            }
            else
            {
                MapUtil.InflateRate(ref range, SpawnRangeInflateRate);
                IEnumerable<Point3d> posAirGroups = AssignedAirGroups.Select(x => x.Position);
                Point3d pointAirport;
                IEnumerable<AiAirport> aiAirports = GamePlay.gpAirports().Where(x =>
                {
                    pointAirport = x.Pos();
                    return GamePlay.gpFrontArmy(pointAirport.x, pointAirport.y) == airGroup.ArmyIndex &&
                            /*x.ParkCountFree() > SpawnNeedParkCountFree && */MapUtil.IsInRange(ref range, ref pointAirport) &&
                            !posAirGroups.Any(y => y.distance(ref pointAirport) <= x.FieldR());
                });
                if (aiAirports.Any())
                {
                    AiAirport aiAirport = aiAirports.ElementAt(Random.Next(aiAirports.Count()));
                    pointAirport = aiAirport.Pos();
                    airGroup.UpdateStartPoint(ref pointAirport);
                    Debug.Write(string.Format(" => Airport Name={0} => Pos Changed={1}", aiAirport.Name(), airGroup.Position.ToString()));
                }
                else
                {
                    SetSpawnRandomType(airGroup, ref point);
                }
            }
        }

        private void SetSpawnOptimaizeLocation(AirGroup airGroup)
        {
            Point3d point = airGroup.Position;
            if (airGroup.Airstart)
            {
                bool updated = SetSpawnRandomAltitude(airGroup, ref point);
                if (updated)
                {
                    airGroup.UpdateStartPoint(ref point);
                }
                Debug.Write(updated ? string.Format(" => Altitude Changed={0}, Pos={1}", updated, airGroup.Position.ToString()) : string.Empty);
            }
            else
            {
                if (AssignedAirGroups.Select(x => x.Position).Any(x => x.distance(ref point) < MaxSpawnDifDistanceAirport))
                {
                    SetSpawnRandomType(airGroup, ref point);
                }
            }
        }

        private void SetSpawnRandomType(AirGroup airGroup, ref Point3d point)
        {
            if (Random.Next(2) == 1)
            {
                airGroup.SetOnParked = true;
                Debug.Write(airGroup.SetOnParked ? string.Format(" => Pos no Changed. SetOnParked={0}", airGroup.SetOnParked) : string.Empty);
            }
            else if (SetSpawnRandomAltitude(airGroup, ref point))
            {
                airGroup.UpdateStartPoint(ref point, AirGroupWaypoint.AirGroupWaypointTypes.NORMFLY);
                Debug.Write(string.Format(" => Spawn Type Changed[AirStart]={0}, Pos={1}", true, airGroup.Position.ToString()));
            }
            else
            {
                Debug.Write(" => Pos no Changed.");
            }
        }

        private bool SetSpawnRandomAltitude(AirGroup airGroup, ref Point3d point)
        {
            bool updated = false;
            IEnumerable<Point3d> posAirGroups = AssignedAirGroups.Select(x => x.Position);
            Point3d p = point;
            int reTry = -1;
            while (posAirGroups.Where(x => x.distance(ref p) < MaxSpawnDifDistanceAirstart).Any() && reTry < MaxRetrySpawnRandomChange)
            {
                p.z += Random.Next(MaxSpawnDifDistanceAirstart / 2, MaxSpawnDifDistanceAirstart + 1);   // + minimum Altitude 
                updated = true;
                reTry++;
            }
            point = p;
            return updated;
        }

        #endregion

        public void OptimizeMissionObjects(IMissionStatus missionStatus = null)
        {
            if (missionStatus == null)
            {
                missionStatus = MissionStatus;
            }

            for (int i = AvailableAirGroups.Count - 1; i >= 0; i--)
            {
                AirGroup airGroup = AvailableAirGroups[i];
                AirGroupObj airGroupObject = missionStatus.AirGroups.Where(x => string.Compare(x.SquadronName, airGroup.SquadronName, true) == 0).FirstOrDefault();
                if (airGroupObject != null)
                {
                    AvailableAirGroups.Remove(airGroup);
                    AssignedAirGroups.Add(airGroup);
                }
            }
        }

        public int AddRandomAirGroupsByOperations(int additionalAirOperations, IEnumerable<string> aircraftsRed, IEnumerable<string> aircraftsBlue)
        {
            int needAirGroups = additionalAirOperations * Config.AverageAirOperationAirGroupCount - AvailableAirGroups.Count;
            return AddRandomAirGroups(needAirGroups, aircraftsRed, aircraftsBlue);
        }

        public int AddRandomAirGroups(int needAirGroups, IEnumerable<string> aircraftsRed, IEnumerable<string> aircraftsBlue)
        {
            if (needAirGroups > 0)
            {
                IEnumerable<AirGroup> airGroups = GetRandomAirGroups(needAirGroups, CampaignInfo.AirGroupInfos, aircraftsRed, aircraftsBlue);
                AvailableAirGroups.AddRange(airGroups);
                AllAirGroups.AddRange(airGroups);
                return airGroups.Count();
            }
            return 0;
        }

        private IEnumerable<AirGroup> GetRandomAirGroups(int needAirGroups, AirGroupInfos airGroupInfosLocal, IEnumerable<string> aircraftsRed, IEnumerable<string> aircraftsBlue)
        {
            List<AirGroup> airGroups = new List<AirGroup>();
            if (aircraftsRed.Any() && aircraftsBlue.Any())
            {
                wRECTF rangeRed = MapUtil.GetRange(AvailableAirGroups.Where(x => x.ArmyIndex == (int)EArmy.Red).Select(x => x.Position));
                wRECTF rangeBlue = MapUtil.GetRange(AvailableAirGroups.Where(x => x.ArmyIndex == (int)EArmy.Blue).Select(x => x.Position));
                CampaignInfo campaign = CampaignInfo;
                int reTries = -1;
                while (airGroups.Count < needAirGroups && reTries < MaxRetryCreateAllAirGroups)
                {
                    int army = Random.Next((int)EArmy.Red, (int)EArmy.Blue + 1);
                    string aircraftClass = (army == (int)EArmy.Red) ? aircraftsRed.ElementAt(Random.Next(aircraftsRed.Count())) : aircraftsBlue.ElementAt(Random.Next(aircraftsBlue.Count()));
                    IEnumerable<AirGroupInfo> airGroupInfos =
                        (airGroupInfos = airGroupInfosLocal != null ? airGroupInfosLocal.GetAirGroupInfoAircraft(aircraftClass).Where(x => x.ArmyIndex == army && AirForce.IsTrust(x.ArmyIndex, x.AirForceIndex)) : new AirGroupInfo[0]).Any() ?
                            airGroupInfos : AirGroupInfos.Default.GetAirGroupInfoAircraft(aircraftClass).Where(x => x.ArmyIndex == army && AirForce.IsTrust(x.ArmyIndex, x.AirForceIndex));
                    if (airGroupInfos.Any())
                    {
                        AirGroupInfo airGroupInfo = airGroupInfos.ElementAt(Random.Next(airGroupInfos.Count()));
                        AircraftInfo aircraftInfo = campaign.GetAircraftInfo(aircraftClass);
                        AircraftParametersInfo paramInfo = GetRandomParametersInfo(Random, aircraftInfo);
                        int reTry = -1;
                        string airGroupSquadron;
                        do
                        {
                            string airGroupKey = airGroupInfo.AirGroupKeys[Random.Next(airGroupInfo.AirGroupKeys.Count)];
                            int squadronIndex = Random.Next(airGroupInfo.SquadronCount);
                            airGroupSquadron = string.Format(Config.NumberFormat, AirGroup.SquadronFormat, airGroupKey, squadronIndex);
                        }
                        while ((airGroups.Any(x => string.Compare(x.SquadronName, airGroupSquadron) == 0) || AvailableAirGroups.Any(x => string.Compare(x.SquadronName, airGroupSquadron) == 0)) && reTry++ <= MaxRetryCreateOneAirGroup);
                        reTries += reTry;
                        if (reTry <= MaxRetryCreateOneAirGroup)
                        {
                            Point3d point = CreateRandomPoint(Random, ref (army == (int)EArmy.Red ? ref rangeRed : ref rangeBlue), paramInfo.MinAltitude != null ? (int)paramInfo.MinAltitude.Value : Spawn.SelectStartAltitude, paramInfo.MaxAltitude != null ? (int)paramInfo.MaxAltitude.Value : Spawn.SelectEndAltitude);
                            AirGroup airGroup = CreateAirGroup(ref point, airGroupSquadron, airGroupInfo, aircraftInfo, paramInfo);
                            if (airGroup != null)
                            {
                                airGroups.Add(airGroup);
                            }
                        }
                    }
                }
            }
            return airGroups;
        }

        public static AircraftParametersInfo GetRandomParametersInfo(IRandom random, AircraftInfo aircraftInfo)
        {
            EMissionType missionType = aircraftInfo.MissionTypes[random.Next(aircraftInfo.MissionTypes.Count)];
            IList<AircraftParametersInfo> aircraftParametersInfos = aircraftInfo.GetAircraftParametersInfo(missionType);
            return aircraftParametersInfos[random.Next(aircraftParametersInfos.Count)];
        }

        public AircraftLoadoutInfo GetRandomLoadoufInfo(AircraftInfo aircraftInfo)
        {
            return GetRandomLoadoufInfo(Random, aircraftInfo);
        }

        public static AircraftLoadoutInfo GetRandomLoadoufInfo(IRandom random, AircraftInfo aircraftInfo)
        {
            AircraftParametersInfo aircraftParametersInfo = GetRandomParametersInfo(random, aircraftInfo);
            return aircraftInfo.GetAircraftLoadoutInfo(aircraftParametersInfo.LoadoutId);
        }

        private Point3d CreateRandomPoint(ref wRECTF rect, int minAltitude, int maxAltitude)
        {
            return CreateRandomPoint(Random, ref rect, minAltitude, maxAltitude);
        }

        public static Point3d CreateRandomPoint(IRandom random, ref wRECTF rect, int minAltitude, int maxAltitude)
        {
            return new Point3d(random.Next((int)rect.x1, (int)rect.x2 + 1), random.Next((int)rect.y1, (int)rect.y2 + 1), random.Next(minAltitude, maxAltitude + 1));
        }

        #region Create AirGroup

        public static AirGroup CreateAirGroup(IGamePlay gamePlay, IRandom random, CampaignInfo campaignInfo, IEnumerable<AirGroup> airGroups, int army, string airGroupSquadron, string aircraft, int spawn)
        {
            AirGroupInfo airGroupInfo = campaignInfo.AirGroupInfos.GetAirGroupInfoGroupKey(AirGroup.CreateAirGroupKey(airGroupSquadron)).FirstOrDefault();
            string aircraftClass = airGroupInfo.Aircrafts.Where(X => X.LastIndexOf(aircraft) != -1).FirstOrDefault();
            if (airGroupInfo != null && !string.IsNullOrEmpty(aircraftClass))
            {
                AircraftInfo aircraftInfo = campaignInfo.GetAircraftInfo(aircraftClass);
                AircraftParametersInfo paramInfo = aircraftInfo.GetDefaultAircraftParametersInfo();
                wRECTF range = MapUtil.GetRange(airGroups.Where(x => x.ArmyIndex == army).Select(x => x.Position));
                Point3d point = CreateRandomPoint(random, ref range, paramInfo.MinAltitude != null ? (int)paramInfo.MinAltitude.Value : Spawn.SelectStartAltitude, paramInfo.MaxAltitude != null ? (int)paramInfo.MaxAltitude.Value : Spawn.SelectEndAltitude); // AirGroupWaypoint.DefaultNormalflyZ, AirGroupWaypoint.DefaultNormalflyZ * 5
                AiAirport target = GetRandomAiAirportBasedOnDistance(random, gamePlay.gpAirports().Where(x => gamePlay.gpFrontArmy(x.Pos().x, x.Pos().y) == army), point);
                point = target.Pos();
                return CreateAirGroup(ref point, airGroupSquadron, airGroupInfo, aircraftInfo, paramInfo);
            }
            return null;
        }

        private static AirGroup CreateAirGroup(ref Point3d pos, string airGroupSquadron, AirGroupInfo airGroupInfo, AircraftInfo aircraftInfo, AircraftParametersInfo paramInfo)
        {
            string id = string.Format("{0}{1}", airGroupSquadron, 0.ToString(Config.NumberFormat));
            AircraftLoadoutInfo loadoutInfo = paramInfo != null ? aircraftInfo.GetAircraftLoadoutInfo(paramInfo.LoadoutId) : null;
            AirGroup airGroup = new AirGroup(id, aircraftInfo, pos, loadoutInfo);
            if (airGroup != null)
            {
                airGroup.SetAirGroupInfo(airGroupInfo);
                return airGroup;
            }
            return null;
        }

        #endregion

#if DEBUG

        [Conditional("DEBUG")]
        public void TraceAssignedAirGroups()
        {
            foreach (var item in AssignedAirGroups/*missionTemplateFile.AirGroups*/.Where(x => x.ArmyIndex == AirGroupPlayer.ArmyIndex).OrderBy(x => x.Position.x))
            {
                AirGroupWaypoint way = item.Waypoints.FirstOrDefault();
                if (way != null)
                {
                    Debug.WriteLine("Name={0} Mission={1} Pos=({2:F2},{3:F2},{4:F2}) V={5:F2}, AirStart={6}, SetOnParked={7}, SpawnFromScript={8}({9})",
                                item.DisplayDetailName, item.MissionType, way.X, way.Y, way.Z, way.V, item.Airstart, item.SetOnParked, item.SpawnFromScript, item.Spawn.Time.Value);
                }
            }
            foreach (var item in AssignedAirGroups/*missionTemplateFile.AirGroups*/.Where(x => x.ArmyIndex == Army.Enemy(AirGroupPlayer.ArmyIndex)).OrderBy(x => x.Position.x))
            {
                AirGroupWaypoint way = item.Waypoints.FirstOrDefault();
                if (way != null)
                {
                    Debug.WriteLine("Name={0} Mission={1} Pos=({2:F2},{3:F2},{4:F2}) V={5:F2}, AirStart={6}, SetOnParked={7}, SpawnFromScript={8}({9})",
                                item.DisplayDetailName, item.MissionType, way.X, way.Y, way.Z, way.V, item.Airstart, item.SetOnParked, item.SpawnFromScript, item.Spawn.Time.Value);
                }
            }
        }
#endif
    }
}