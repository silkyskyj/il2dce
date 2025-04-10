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
using System.Globalization;
using System.Linq;
using System.Threading;
using IL2DCE.MissionObjectModel;
using maddox.game;
using maddox.game.world;
using maddox.GP;

namespace IL2DCE.Generator
{
    class GeneratorGroundOperation
    {
        private IGamePlay GamePlay
        {
            get;
            set;
        }

        private IRandom Random
        {
            get;
            set;
        }

        public bool HasAvailableGroundGroup
        {
            get
            {
                return AvailableGroundGroups.Count > 0;
            }
        }

        private List<GroundGroup> AvailableGroundGroups = new List<GroundGroup>();

        private List<Stationary> AvailableStationaries = new List<Stationary>();

        private IEnumerable<Point3d> FrontMarkers;

        public GeneratorGroundOperation(IGamePlay gamePlay, Config config, IRandom random, CampaignInfo campaignInfo, IEnumerable<GroundGroup> groundGroups, IEnumerable<Stationary> stationaries, IEnumerable<Point3d> frontMarkers)
        {
            GamePlay = gamePlay;
            Random = random;

            AvailableGroundGroups.Clear();
            AvailableStationaries.Clear();
            AvailableGroundGroups.AddRange(groundGroups);
            AvailableStationaries.AddRange(stationaries);

            FrontMarkers = frontMarkers;
        }

        public GroundGroup getRandomTargetBasedOnRange(List<GroundGroup> availableGroundGroups, AirGroup offensiveAirGroup)
        {
            GroundGroup selectedGroundGroup = null;

            if (availableGroundGroups.Count > 1)
            {
                var copy = new List<GroundGroup>(availableGroundGroups);
                copy.Sort(new DistanceComparer(offensiveAirGroup.Position));

                Point2d position = new Point2d(offensiveAirGroup.Position.x, offensiveAirGroup.Position.y);

                // TODO: Use range of the aircraft instead of the maxDistance.
                // Problem is that range depends on loadout, so depending on loadout different targets would be available.

                Point2d last = copy.Last().Position;
                double maxDistance = last.distance(ref position);

                List<KeyValuePair<GroundGroup, int>> elements = new List<KeyValuePair<GroundGroup, int>>();

                int previousWeight = 0;

                foreach (GroundGroup groundGroup in copy)
                {
                    double distance = groundGroup.Position.distance(ref position);
                    int weight = Convert.ToInt32(Math.Ceiling(maxDistance - distance));
                    int cumulativeWeight = previousWeight + weight;
                    elements.Add(new KeyValuePair<GroundGroup, int>(groundGroup, cumulativeWeight));

                    previousWeight = cumulativeWeight;
                }

                int diceRoll = Random.Next(0, previousWeight);
                int cumulative = 0;
                for (int i = 0; i < elements.Count; i++)
                {
                    cumulative += elements[i].Value;
                    if (diceRoll <= cumulative)
                    {
                        selectedGroundGroup = elements[i].Key;
                        break;
                    }
                }
            }
            else if (availableGroundGroups.Count == 1)
            {
                selectedGroundGroup = availableGroundGroups.First();
            }

            return selectedGroundGroup;
        }

        public Stationary getRandomTargetBasedOnRange(List<Stationary> availableStationaries, AirGroup offensiveAirGroup)
        {
            Stationary selectedStationary = null;

            if (availableStationaries.Count > 1)
            {
                var copy = new List<Stationary>(availableStationaries);
                copy.Sort(new DistanceComparer(offensiveAirGroup.Position));

                Point2d position = new Point2d(offensiveAirGroup.Position.x, offensiveAirGroup.Position.y);

                // TODO: Use range of the aircraft instead of the maxDistance.
                // Problem is that range depends on loadout, so depending on loadout different targets would be available.

                Point2d last = copy.Last().Position;
                double maxDistance = last.distance(ref position);

                List<KeyValuePair<Stationary, int>> elements = new List<KeyValuePair<Stationary, int>>();

                int previousWeight = 0;

                foreach (Stationary stationary in copy)
                {
                    double distance = stationary.Position.distance(ref position);
                    int weight = Convert.ToInt32(Math.Ceiling(maxDistance - distance));
                    int cumulativeWeight = previousWeight + weight;
                    elements.Add(new KeyValuePair<Stationary, int>(stationary, cumulativeWeight));

                    previousWeight = cumulativeWeight;
                }

                int diceRoll = Random.Next(0, previousWeight);
                int cumulative = 0;
                for (int i = 0; i < elements.Count; i++)
                {
                    cumulative += elements[i].Value;
                    if (diceRoll <= cumulative)
                    {
                        selectedStationary = elements[i].Key;
                        break;
                    }
                }
            }
            else if (availableStationaries.Count == 1)
            {
                selectedStationary = availableStationaries.First();
            }

            return selectedStationary;
        }

        private IEnumerable<GroundGroupWaypoint> CreateWaypoints(GroundGroup groundGroup, Point2d start, Point2d end)
        {
            IRecalcPathParams pathParams = null;
            if (groundGroup.Type == EGroundGroupType.Armor || groundGroup.Type == EGroundGroupType.Vehicle)
            {
                pathParams = GamePlay.gpFindPath(start, 10.0, end, 20.0, PathType.GROUND, groundGroup.Army);
            }
            else if (groundGroup.Type == EGroundGroupType.Ship)
            {
                pathParams = GamePlay.gpFindPath(start, 10.0, end, 20.0, PathType.WATER, groundGroup.Army);
            }

            if (pathParams != null)
            {
                while (pathParams.State == RecalcPathState.WAIT)
                {
                    //Game.gpLogServer(new Player[] { Game.gpPlayer() }, "Wait for path.", null);
                    Thread.Sleep(100);
                }

                if (pathParams.State == RecalcPathState.SUCCESS)
                {
                    //Game.gpLogServer(new Player[] { Game.gpPlayer() }, "Path found (" + pathParams.Path.Length.ToString(System.Globalization.CultureInfo.InvariantCulture.NumberFormat) + ").", null);

                    List<GroundGroupWaypoint> waypoints = new List<GroundGroupWaypoint>();
                    GroundGroupWaypoint lastGroundGroupWaypoint = null;
                    foreach (AiWayPoint aiWayPoint in pathParams.Path)
                    {
                        if (aiWayPoint is AiGroundWayPoint)
                        {
                            AiGroundWayPoint aiGroundWayPoint = aiWayPoint as AiGroundWayPoint;
                            if (aiGroundWayPoint.P.z == -1)
                            {
                                GroundGroupWaypoint groundGroupWaypoint = new GroundGroupWaypointLine(aiGroundWayPoint.P.x, aiGroundWayPoint.P.y, aiGroundWayPoint.roadWidth, aiGroundWayPoint.Speed);
                                lastGroundGroupWaypoint = groundGroupWaypoint;
                                waypoints.Add(groundGroupWaypoint);
                            }
                            else if (lastGroundGroupWaypoint != null)
                            {
                                // TODO: Fix calculated param

                                //string s = string.Format(CultureInfo.InvariantCulture.NumberFormat, "{0:F2} {1:F2} {2:F2} {3:F2}", 
                                //    aiGroundWayPoint.P.x, aiGroundWayPoint.P.y, aiGroundWayPoint.P.z, aiGroundWayPoint.roadWidth);

                                GroundGroupWaypoint groundGroupSubWaypoint = new GroundGroupWaypointLine(aiGroundWayPoint.P.x, aiGroundWayPoint.P.x, aiGroundWayPoint.P.z, null);
                                lastGroundGroupWaypoint.SubWaypoints.Add(groundGroupSubWaypoint);
                            }
                        }
                    }
                    return waypoints;
                }
                else if (pathParams.State == RecalcPathState.FAILED)
                {
                    Debug.WriteLine("Path not found.");
                    //Game.gpLogServer(new Player[] { Game.gpPlayer() }, "Path not found.", null);
                }
            }
            return null;
        }

        private IEnumerable<GroundGroupWaypoint> CreateWaypoints(GroundGroup groundGroup, Point2d start, Point2d end, IList<Groundway> roads)
        {
            if (roads != null && roads.Count > 0)
            {
                Groundway closestRoad = null;
                double closestRoadDistance = 0.0;
                foreach (Groundway road in roads)
                {
                    if (road.Start != null && road.End != null)
                    {
                        Point2d roadStart = new Point2d(road.Start.Position.x, road.Start.Position.y);
                        double distanceStart = start.distance(ref roadStart);
                        Point2d roadEnd = new Point2d(road.End.Position.x, road.End.Position.y);
                        double distanceEnd = end.distance(ref roadEnd);

                        Point2d p = new Point2d(end.x, end.y);
                        if (distanceEnd < start.distance(ref p))
                        {
                            if (closestRoad == null)
                            {
                                closestRoad = road;
                                closestRoadDistance = distanceStart + distanceEnd;
                            }
                            else
                            {
                                if (distanceStart + distanceEnd < closestRoadDistance)
                                {
                                    closestRoad = road;
                                    closestRoadDistance = distanceStart + distanceEnd;
                                }
                            }
                        }
                    }
                }

                if (closestRoad != null)
                {
                    // CreateWaypoints(groundGroup, start, new Point2d(closestRoad.Start.X, closestRoad.Start.Y));

                    List<GroundGroupWaypoint> waypoints = new List<GroundGroupWaypoint>();
                    waypoints.AddRange(closestRoad.Waypoints);

                    List<Groundway> availableRoads = new List<Groundway>(roads);
                    availableRoads.Remove(closestRoad);

                    IEnumerable<GroundGroupWaypoint> results = CreateWaypoints(groundGroup, new Point2d(closestRoad.End.Position.x, closestRoad.End.Position.y), end, availableRoads);
                    if (results != null && results.Any())
                    {
                        waypoints.AddRange(results);
                    }

                    return waypoints;
                }
            }

            return null;
        }

        public bool CreateRandomGroundOperation(ISectionFile missionFile, GroundGroup groundGroup)
        {
            bool result = false;
            AvailableGroundGroups.Remove(groundGroup);

            if (groundGroup.Type == EGroundGroupType.Ship)
            {
                // Ships already have the correct waypoint from the mission template. Only remove some waypoints to make the position more random, but leave at least 2 waypoints.
                groundGroup.Waypoints.RemoveRange(0, Random.Next(0, groundGroup.Waypoints.Count - 1));
                groundGroup.WriteTo(missionFile);
                generateColumnFormation(missionFile, groundGroup, 3);
                result = true;
            }
            else if (groundGroup.Type == EGroundGroupType.Train)
            {
                groundGroup.Waypoints.RemoveRange(0, Random.Next(0, groundGroup.Waypoints.Count - 1));
                groundGroup.WriteTo(missionFile);
                result = true;
            }
            else
            {     // Vehicle Armor Unknown
                IEnumerable<Point3d> friendlyMarkers = FrontMarkers.Where(x => x.z == groundGroup.Army);
                if (friendlyMarkers.Any())
                {
                    List<Point3d> availableFriendlyMarkers = new List<Point3d>(friendlyMarkers);

                    // Find closest friendly marker
                    Point3d? closestMarker = null;
                    foreach (Point3d marker in availableFriendlyMarkers)
                    {
                        if (closestMarker == null)
                        {
                            closestMarker = marker;
                        }
                        else if (closestMarker.HasValue)
                        {
                            Point2d p1 = new Point2d(marker.x, marker.y);
                            Point2d p2 = new Point2d(closestMarker.Value.x, closestMarker.Value.y);
                            if (groundGroup.Position.distance(ref p1) < groundGroup.Position.distance(ref p2))
                            {
                                closestMarker = marker;
                            }
                        }
                    }

                    if (closestMarker != null && closestMarker.HasValue)
                    {
                        availableFriendlyMarkers.Remove(closestMarker.Value);

                        if (availableFriendlyMarkers.Count > 0)
                        {
                            int markerIndex = Random.Next(availableFriendlyMarkers.Count);
                            Point2d end = new Point2d(availableFriendlyMarkers[markerIndex].x, availableFriendlyMarkers[markerIndex].y);

                            IEnumerable<GroundGroupWaypoint> wayPoints;
                            if (groundGroup.Type == EGroundGroupType.Armor || groundGroup.Type == EGroundGroupType.Vehicle)
                            {
                                Point2d start = new Point2d(closestMarker.Value.x, closestMarker.Value.y);
                                wayPoints = CreateWaypoints(groundGroup, start, end);
                                groundGroup.WriteTo(missionFile);
                            }
                            else
                            {
                                Point2d start = new Point2d(groundGroup.Position.x, groundGroup.Position.y);
                                wayPoints = CreateWaypoints(groundGroup, start, end);
                                groundGroup.WriteTo(missionFile);
                                generateColumnFormation(missionFile, groundGroup, 3);
                            }

                            if (wayPoints != null && wayPoints.Any())
                            {
                                groundGroup.Waypoints.Clear();
                                groundGroup.Waypoints.AddRange(wayPoints);

                                result = true;
                            }
                        }
                    }
                }
            }
            return result;
        }

        private static void generateColumnFormation(ISectionFile missionFile, GroundGroup groundGroup, int columnSize)
        {
            string groundGroupId = groundGroup.Id;

            List<GroundGroupWaypoint> waypoints = groundGroup.Waypoints;
            for (int i = 1; i < columnSize; i++)
            {
                double xOffset = -1.0;
                double yOffset = -1.0;

                bool subWaypointUsed = false;
                GroundGroupWaypoint wayPointFirst = waypoints.First();
                Point2d p1 = wayPointFirst.Position;
                if (wayPointFirst.SubWaypoints.Count > 0)
                {
                    Point2d p2 = wayPointFirst.SubWaypoints[0].Position;
                    double distance = p1.distance(ref p2);
                    xOffset = 500 * ((p2.x - p1.x) / distance);
                    yOffset = 500 * ((p2.y - p1.y) / distance);
                    subWaypointUsed = true;
                }
                if (subWaypointUsed == false)
                {
                    Point2d p2 = waypoints[1].Position;
                    double distance = p1.distance(ref p2);
                    xOffset = 500 * ((p2.x - p1.x) / distance);
                    yOffset = 500 * ((p2.y - p1.y) / distance);
                }

                wayPointFirst.X += xOffset;
                wayPointFirst.Y += yOffset;

                subWaypointUsed = false;
                GroundGroupWaypoint wayPointLast = waypoints.Last();
                p1 = new Point2d(wayPointLast.X, wayPointLast.Y);
                GroundGroupWaypoint wayPointLast2 = waypoints[waypoints.Count - 2];
                if (wayPointLast2.SubWaypoints.Count > 0)
                {
                    GroundGroupWaypoint subWaypoint = wayPointLast2.SubWaypoints.Last();
                    Point2d p2 = subWaypoint.Position;
                    double distance = p1.distance(ref p2);
                    xOffset = 500 * ((p2.x - p1.x) / distance);
                    yOffset = 500 * ((p2.y - p1.y) / distance);
                    subWaypointUsed = true;
                }
                if (subWaypointUsed == false)
                {
                    Point2d p2 = new Point2d(wayPointLast2.X, wayPointLast2.Y);
                    double distance = p1.distance(ref p2);
                    xOffset = 500 * ((p2.x - p1.x) / distance);
                    yOffset = 500 * ((p2.y - p1.y) / distance);
                }

                wayPointLast.X -= xOffset;
                wayPointLast.Y -= yOffset;

                groundGroup.Id = groundGroupId + "." + i.ToString(CultureInfo.InvariantCulture.NumberFormat);

                groundGroup.WriteTo(missionFile);
            }
        }

        #region GroundGroup

        #region GetAvailable

        public GroundGroup GetAvailableRandomGroundGroup()
        {
            if (HasAvailableGroundGroup)
            {
                int randomIndex = Random.Next(AvailableGroundGroups.Count);
                return AvailableGroundGroups[randomIndex];
            }
            return null;
        }

        public List<GroundGroup> getAvailableEnemyGroundGroups(int armyIndex)
        {
            List<GroundGroup> groundGroups = new List<GroundGroup>();
            foreach (GroundGroup groundGroup in AvailableGroundGroups)
            {
                if (groundGroup.Army != armyIndex)
                {
                    groundGroups.Add(groundGroup);
                }
            }
            return groundGroups;
        }

        public List<GroundGroup> getAvailableFriendlyGroundGroups(int armyIndex)
        {
            List<GroundGroup> groundGroups = new List<GroundGroup>();
            foreach (GroundGroup groundGroup in AvailableGroundGroups)
            {
                if (groundGroup.Army == armyIndex)
                {
                    groundGroups.Add(groundGroup);
                }
            }
            return groundGroups;
        }

        public List<GroundGroup> getAvailableEnemyGroundGroups(int armyIndex, List<EGroundGroupType> groundGroupTypes)
        {
            List<GroundGroup> groundGroups = new List<GroundGroup>();
            foreach (GroundGroup groundGroup in getAvailableEnemyGroundGroups(armyIndex))
            {
                if (groundGroupTypes.Contains(groundGroup.Type))
                {
                    groundGroups.Add(groundGroup);
                }
            }
            return groundGroups;
        }

        public List<GroundGroup> getAvailableFriendlyGroundGroups(int armyIndex, List<EGroundGroupType> groundGroupTypes)
        {
            List<GroundGroup> groundGroups = new List<GroundGroup>();
            foreach (GroundGroup groundGroup in getAvailableFriendlyGroundGroups(armyIndex))
            {
                if (groundGroupTypes.Contains(groundGroup.Type))
                {
                    groundGroups.Add(groundGroup);
                }
            }
            return groundGroups;
        }

        public GroundGroup getAvailableRandomEnemyGroundGroup(int armyIndex)
        {
            List<GroundGroup> groundGroups = getAvailableEnemyGroundGroups(armyIndex);
            if (groundGroups.Count > 0)
            {
                int groundGroupIndex = Random.Next(groundGroups.Count);
                GroundGroup targetGroundGroup = groundGroups[groundGroupIndex];

                return targetGroundGroup;
            }
            else
            {
                return null;
            }
        }

        public GroundGroup getAvailableRandomEnemyGroundGroup(AirGroup airGroup, EMissionType missionType)
        {
            if (missionType == EMissionType.ARMED_MARITIME_RECON || missionType == EMissionType.MARITIME_RECON
                || missionType == EMissionType.ATTACK_SHIP)
            {
                return getAvailableRandomEnemyGroundGroup(airGroup, new List<EGroundGroupType> { EGroundGroupType.Ship });
            }
            else if (missionType == EMissionType.ARMED_RECON || missionType == EMissionType.RECON)
            {
                return getAvailableRandomEnemyGroundGroup(airGroup, new List<EGroundGroupType> { EGroundGroupType.Armor, EGroundGroupType.Train, EGroundGroupType.Vehicle });
            }
            else if (missionType == EMissionType.ATTACK_ARMOR)
            {
                return getAvailableRandomEnemyGroundGroup(airGroup, new List<EGroundGroupType> { EGroundGroupType.Armor });
            }
            else if (missionType == EMissionType.ATTACK_VEHICLE)
            {
                return getAvailableRandomEnemyGroundGroup(airGroup, new List<EGroundGroupType> { EGroundGroupType.Vehicle });
            }
            else if (missionType == EMissionType.ATTACK_TRAIN)
            {
                return getAvailableRandomEnemyGroundGroup(airGroup, new List<EGroundGroupType> { EGroundGroupType.Train });
            }
            else
            {
                return null;
            }
        }

        public GroundGroup getAvailableRandomEnemyGroundGroup(AirGroup airGroup, List<EGroundGroupType> groundGroupTypes)
        {
            List<GroundGroup> groundGroups = getAvailableEnemyGroundGroups(airGroup.ArmyIndex, groundGroupTypes);
            if (groundGroups.Count > 0)
            {
                //int groundGroupIndex = Random.Next(groundGroups.Count);
                //GroundGroup targetGroundGroup = groundGroups[groundGroupIndex];

                GroundGroup targetGroundGroup = getRandomTargetBasedOnRange(groundGroups, airGroup);

                return targetGroundGroup;
            }
            else
            {
                return null;
            }
        }

        public GroundGroup getAvailableRandomFriendlyGroundGroup(AirGroup airGroup)
        {
            List<GroundGroup> groundGroups = getAvailableFriendlyGroundGroups(airGroup.ArmyIndex);
            if (groundGroups.Count > 0)
            {
                //int groundGroupIndex = Random.Next(groundGroups.Count);
                //GroundGroup targetGroundGroup = groundGroups[groundGroupIndex];

                GroundGroup targetGroundGroup = getRandomTargetBasedOnRange(groundGroups, airGroup);

                return targetGroundGroup;
            }
            else
            {
                return null;
            }
        }

        public GroundGroup getAvailableRandomFriendlyGroundGroup(AirGroup airGroup, List<EGroundGroupType> groundGroupTypes)
        {
            List<GroundGroup> groundGroups = getAvailableFriendlyGroundGroups(airGroup.ArmyIndex, groundGroupTypes);
            if (groundGroups.Count > 0)
            {
                //int groundGroupIndex = Random.Next(groundGroups.Count);
                //GroundGroup targetGroundGroup = groundGroups[groundGroupIndex];

                GroundGroup targetGroundGroup = getRandomTargetBasedOnRange(groundGroups, airGroup);

                return targetGroundGroup;
            }
            else
            {
                return null;
            }
        }

        #endregion

        #endregion

        #region Stationary

        #region GetAvailable

        public List<Stationary> getAvailableEnemyStationaries(int armyIndex)
        {
            List<Stationary> stationaries = new List<Stationary>();
            foreach (Stationary stationary in AvailableStationaries)
            {
                if (stationary.Army != armyIndex)
                {
                    stationaries.Add(stationary);
                }
            }
            return stationaries;
        }

        public List<Stationary> getAvailableFriendlyStationaries(int armyIndex)
        {
            List<Stationary> stationaries = new List<Stationary>();
            foreach (Stationary stationary in AvailableStationaries)
            {
                if (stationary.Army == armyIndex)
                {
                    stationaries.Add(stationary);
                }
            }
            return stationaries;
        }

        public List<Stationary> getAvailableEnemyStationaries(int armyIndex, List<EStationaryType> stationaryTypes)
        {
            List<Stationary> stationaries = new List<Stationary>();
            foreach (Stationary stationary in getAvailableEnemyStationaries(armyIndex))
            {
                if (stationaryTypes.Contains(stationary.Type))
                {
                    stationaries.Add(stationary);
                }
            }
            return stationaries;
        }

        public List<Stationary> getAvailableFriendlyStationaries(int armyIndex, List<EStationaryType> stationaryTypes)
        {
            List<Stationary> stationaries = new List<Stationary>();
            foreach (Stationary stationary in getAvailableFriendlyStationaries(armyIndex))
            {
                if (stationaryTypes.Contains(stationary.Type))
                {
                    stationaries.Add(stationary);
                }
            }
            return stationaries;
        }

        public Stationary getAvailableRandomEnemyStationary(int armyIndex)
        {
            List<Stationary> stationaries = getAvailableEnemyStationaries(armyIndex);
            if (stationaries.Count > 0)
            {
                int stationaryIndex = Random.Next(stationaries.Count);
                Stationary targetStationary = stationaries[stationaryIndex];

                return targetStationary;
            }
            else
            {
                return null;
            }
        }

        public Stationary getAvailableRandomEnemyStationary(AirGroup airGroup, EMissionType missionType)
        {
            if (missionType == EMissionType.ARMED_MARITIME_RECON || missionType == EMissionType.MARITIME_RECON || missionType == EMissionType.ATTACK_SHIP)
            {
                return getAvailableRandomEnemyStationary(airGroup, new List<EStationaryType> { EStationaryType.Ship });
            }
            else if (missionType == EMissionType.ARMED_RECON || missionType == EMissionType.RECON)
            {
                return getAvailableRandomEnemyStationary(airGroup, new List<EStationaryType> { EStationaryType.Artillery, EStationaryType.Ammo, EStationaryType.Weapons, 
                                                    EStationaryType.Aircraft, EStationaryType.Radar, EStationaryType.Depot, EStationaryType.Car, EStationaryType.ConstCar, });
            }
            else if (missionType == EMissionType.ATTACK_AIRCRAFT)
            {
                return getAvailableRandomEnemyStationary(airGroup, new List<EStationaryType> { EStationaryType.Aircraft });
            }
            else if (missionType == EMissionType.ATTACK_ARTILLERY)
            {
                return getAvailableRandomEnemyStationary(airGroup, new List<EStationaryType> { EStationaryType.Artillery, EStationaryType.Ammo, EStationaryType.Weapons, });
            }
            else if (missionType == EMissionType.ATTACK_RADAR)
            {
                return getAvailableRandomEnemyStationary(airGroup, new List<EStationaryType> { EStationaryType.Radar });
            }
            else if (missionType == EMissionType.ATTACK_DEPOT)
            {
                return getAvailableRandomEnemyStationary(airGroup, new List<EStationaryType> { EStationaryType.Depot });
            }
            else
            {
                return null;
            }
        }

        public Stationary getAvailableRandomEnemyStationary(AirGroup airGroup, List<EStationaryType> stationaryTypes)
        {
            List<Stationary> stationaries = getAvailableEnemyStationaries(airGroup.ArmyIndex, stationaryTypes);
            if (stationaries.Count > 0)
            {
                //int groundGroupIndex = Random.Next(groundGroups.Count);
                //GroundGroup targetGroundGroup = groundGroups[groundGroupIndex];

                Stationary targetStationary = getRandomTargetBasedOnRange(stationaries, airGroup);

                return targetStationary;
            }
            else
            {
                return null;
            }
        }

        public Stationary getAvailableRandomFriendlyStationary(AirGroup airGroup)
        {
            List<Stationary> stationaries = getAvailableFriendlyStationaries(airGroup.ArmyIndex);
            if (stationaries.Count > 0)
            {
                //int groundGroupIndex = Random.Next(groundGroups.Count);
                //GroundGroup targetGroundGroup = groundGroups[groundGroupIndex];

                Stationary targetStationary = getRandomTargetBasedOnRange(stationaries, airGroup);

                return targetStationary;
            }
            else
            {
                return null;
            }
        }

        public Stationary getAvailableRandomFriendlyStationary(AirGroup airGroup, List<EStationaryType> stationaryTypes)
        {
            List<Stationary> stationaries = getAvailableFriendlyStationaries(airGroup.ArmyIndex, stationaryTypes);
            if (stationaries.Count > 0)
            {
                //int groundGroupIndex = Random.Next(groundGroups.Count);
                //GroundGroup targetGroundGroup = groundGroups[groundGroupIndex];

                Stationary targetStationary = getRandomTargetBasedOnRange(stationaries, airGroup);

                return targetStationary;
            }
            else
            {
                return null;
            }
        }

        #endregion

        public void StationaryWriteTo(ISectionFile file)
        {
            foreach (Stationary stationary in AvailableStationaries)
            {
                stationary.WriteTo(file);
            }
        }

        #endregion
    }
}