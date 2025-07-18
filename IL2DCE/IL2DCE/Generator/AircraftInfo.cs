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
using maddox.game;

namespace IL2DCE.Generator
{
    public enum EAircraftType
    {
        Unknown = 0,
        Fighter = 1,            // = no gunner seat
        FighterSub = 2,         // = gunner seat
        FighterBomber = 3,      // = no gunner seat
        FighterBomberSub = 4,   // = gunner seat
        Bomber = 5,             // = no gunner seat
        BomberSub = 6,          // = gunner seat 
        Other = 7,              // = no gunner seat
        OtherSub = 8,           // = gunner seat 
        Count,
    }

    public class AircraftInfo
    {
        public const string SectionMain = "Main";
        public const string KeyPlayer = "Player";
        public const string KeyType = "Type";
        public const string KeyImage = "Image";

        public bool IsFlyable
        {
            get
            {
                if (aircraftInfoFile.exist(Aircraft, KeyPlayer))
                {
                    string value = aircraftInfoFile.get(Aircraft, KeyPlayer);
                    int player;
                    return int.TryParse(value, out player) && player != 0;
                }
                else
                {
                    string error = string.Format("no Palyer info[{0}] in the file[{1}]", Aircraft, "Aircraftinfo.ini");
                    Debug.WriteLine(error);
                    throw new FormatException(error);
                }
            }
        }

        public IList<EMissionType> MissionTypes
        {
            get
            {
                IList<EMissionType> missionTypes = new List<EMissionType>();
                int lines = aircraftInfoFile.lines(Aircraft);
                for (int i = 0; i < lines; i++)
                {
                    string key;
                    string value;
                    aircraftInfoFile.get(Aircraft, i, out key, out value);

                    EMissionType missionType;
                    if (Enum.TryParse<EMissionType>(key, true, out missionType))
                    {
                        missionTypes.Add(missionType);
                    }
                }
                return missionTypes;
            }
        }

        public string Aircraft
        {
            get;
            set;
        }

        public string DisplayName
        {
            get
            {
                return CreateDisplayName(Aircraft);
            }
        }

        public string ImageFolderName
        {
            get
            {
                return string.Empty;
            }
        }

        public EAircraftType AircraftType
        {
            get
            {
                int value = aircraftInfoFile.get(Aircraft, KeyPlayer, 0);
                if (value > (int)EAircraftType.Unknown && value < (int)EAircraftType.Count)
                {
                    return (EAircraftType)value;
                }
                return EAircraftType.Unknown;
            }
        }

        private ISectionFile aircraftInfoFile;

        public AircraftInfo(ISectionFile aircraftInfoFile, string aircraft)
        {
            this.aircraftInfoFile = aircraftInfoFile;
            Aircraft = aircraft;
        }

        public static string CreateDisplayName(string aircraftInfo)
        {
            // Aircraft.Bf-110C-7 -> Bf-110C-7
            // tobruk:Aircraft.Macchi-C202-SeriesVII -> Macchi-C202-SeriesVII
            const string del = "Aircraft.";
            int idx = aircraftInfo.IndexOf(del, StringComparison.CurrentCultureIgnoreCase);
            return idx != -1 ? aircraftInfo.Substring(idx + del.Length) : aircraftInfo;
        }

        public AircraftParametersInfo GetDefaultAircraftParametersInfo()
        {
            int lines = aircraftInfoFile.lines(Aircraft);
            for (int i = 0; i < lines; i++)
            {
                string key;
                string value;
                aircraftInfoFile.get(Aircraft, i, out key, out value);

                EMissionType missionType;
                if (Enum.TryParse<EMissionType>(key, true, out missionType))
                {
                    IList<AircraftParametersInfo> infos = GetAircraftParametersInfo(missionType);
                    return infos.FirstOrDefault();
                }
            }

            return null;
        }

        public AircraftLoadoutInfo GetDefaultAircraftLoadoutInfo()
        {
            AircraftParametersInfo paramInfo = GetDefaultAircraftParametersInfo();
            return paramInfo != null ? new AircraftLoadoutInfo(aircraftInfoFile, Aircraft, paramInfo.LoadoutId): null;
        }

        public IList<AircraftParametersInfo> GetAircraftParametersInfo(EMissionType missionType)
        {
            IList<AircraftParametersInfo> missionParameters = new List<AircraftParametersInfo>();
            string value = aircraftInfoFile.get(Aircraft, missionType.ToString());
            string[] valueParts = value.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            if (valueParts.Length > 0)
            {
                foreach (string valuePart in valueParts)
                {
                    AircraftParametersInfo missionParameter = new AircraftParametersInfo(valuePart);
                    missionParameters.Add(missionParameter);
                }
            }
            else
            {
                throw new FormatException(string.Format("Invalid Aircraft Parameter Info[{0} {1}]", Aircraft + "." + missionType.ToString(), value));
            }

            return missionParameters;
        }

        public AircraftLoadoutInfo GetAircraftLoadoutInfo(string loadoutId)
        {
            return new AircraftLoadoutInfo(aircraftInfoFile, Aircraft, loadoutId);
        }

        public void Write(ISectionFile file, bool addMissionTransfer)
        {
            SilkySkyCloDFile.Write(file, SectionMain, Aircraft, string.Empty, true);
            SilkySkyCloDFile.Write(file, Aircraft, KeyPlayer, (IsFlyable ? 1 : 0).ToString(Config.NumberFormat), true);
            SilkySkyCloDFile.Write(file, Aircraft, KeyType, ((int)AircraftType).ToString(Config.NumberFormat), true);
            string key;
            string value;
            int lines = aircraftInfoFile.lines(Aircraft);
            for (int i = 0; i < lines; i++)
            {
                aircraftInfoFile.get(Aircraft, i, out key, out value);
                EMissionType missionType;
                if (Enum.TryParse<EMissionType>(key, false, out missionType))
                {
                    SilkySkyCloDFile.Write(file, Aircraft, key, value, true);
                    IList<AircraftParametersInfo> aircraftParametersInfos = GetAircraftParametersInfo(missionType);
                    foreach (var item in aircraftParametersInfos)
                    {
                        string keyLoadOut = string.Format("{0}_{1}", Aircraft, item.LoadoutId);
                        if (aircraftInfoFile.exist(keyLoadOut))
                        {
                            SilkySkyCloDFile.CopySection(aircraftInfoFile, file, keyLoadOut, false);
                        }
                    }
                }
            }

            if (addMissionTransfer)
            {
                value = string.Format("/{0} {1} {2}", AircraftParametersInfo.DefaulLoadoutId, AircraftParametersInfo.DefaultMinAltitude, AircraftParametersInfo.DefaultMaxAltitude);
                SilkySkyCloDFile.Write(file, Aircraft, EMissionType.TRANSFER.ToString(), value, true);
            }
        }

        #region Debug methods

        [Conditional("DEBUG")]
        public static void TraceAircraftInfo(ISectionFile globalAircraftInfoFile, string path)
        {
            int lines = globalAircraftInfoFile.lines(AircraftInfo.SectionMain);
            Debug.WriteLine("AircraftInfo.ini[Lines={0}] Path={1}", lines, path);
            for (int i = 0; i < lines; i++)
            {
                string key;
                string value;
                globalAircraftInfoFile.get(AircraftInfo.SectionMain, i, out key, out value);
                try
                {
                    AircraftInfo aircraftInfo = new AircraftInfo(globalAircraftInfoFile, key);
                    Debug.WriteLine("  Name={0}, IsFlyable={1}, Type={2}, MissionTypes.Count={3}", aircraftInfo.Aircraft, aircraftInfo.IsFlyable, aircraftInfo.AircraftType, aircraftInfo.MissionTypes.Count);
                    foreach (var missionType in aircraftInfo.MissionTypes)
                    {
                        IList<AircraftParametersInfo> paramList = aircraftInfo.GetAircraftParametersInfo(missionType);
                        foreach (var param in paramList)
                        {
                            Debug.WriteLine("    LoadoutId={0}, MinAltitude={1:F0}, MaxAltitude={2:F0}", param.LoadoutId, param.MinAltitude ?? -1, param.MaxAltitude ?? -1);
                            AircraftLoadoutInfo loadout = aircraftInfo.GetAircraftLoadoutInfo(param.LoadoutId);
                            Debug.WriteLine("    Weapons={0}, Detonator={1}", string.Join(" ", loadout.Weapons.Select(x => x.ToString())), string.Join(" ", loadout.Detonator));
                        }
                    }
                }
                catch (Exception ex)
                {
                    string message = string.Format("Aircraft={0}, Error[{1}]", key, ex.Message);
                    Debug.WriteLine(message);
                    Core.WriteLog(message);
                }
            }
        }

        #endregion
    }
}