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
using System.Globalization;
using System.Linq;
using System.Reflection;
using IL2DCE.MissionObjectModel;
using maddox.game;

namespace IL2DCE
{
    public class Config
    {

        #region Definition

        public const char Comma = ',';
        public const string CommaStr = ",";
        public const char Or = '|';
        public static readonly char[] SplitSpace = new char[] { ' ' };
        public static readonly char[] SplitComma = new char[] { ',' };
        public static readonly char[] SplitOr = new char[] { '|' };

        public const string AppName = "IL2DCE";
        public const string HomeFolder = "$home/";
        public const string UserFolder = "$user/";
        public const string PartsFolder = "$home/parts";
        public const string CampaignsFolderDefault = "$home/parts/IL2DCE/Campaigns";
        public const string MissionFolderSingle = "$home/missions/Single";
        public const string MissionFolderFormatSingle = "$home/parts/{0}/missions/Single";
        public const string MissionFolderFormatQuick = "$home/parts/{0}/mission/Quick";
        public const string MissionFolderFormatCampaign = "$home/parts/{0}/mission/campaign";
        public const string RecordFolder = "$user/records";

        public const string ConfigFilePath = "$home/parts/IL2DCE/conf.ini";
        public const string CampaignInfoFileName = "CampaignInfo.ini";
        public const string AircraftInfoFileName = "AircraftInfo.ini";
        public const string AirGroupInfoFileName = "AirGroupInfo.ini";
        public const string CareerInfoFileName = "Career.ini";
        public const string StatsInfoFileName = "Stats.ini";
        public const string MissionScriptFileName = "MissionSingle.cs";
        public const string UserMissionFolder = "$user/mission/IL2DCE";
        public const string UserMissionsFolder = "$user/missions/IL2DCE";
        public const string DebugFolderName = "debug";
        public const string DebugMissionTemplateFileName = "IL2DCEDebugTemplate.mis";
        public const string DebugMissionFileName = "IL2DCEDebug.mis";
        public const string DebugBriefingFileName = "IL2DCEDebug.briefing";
        public const string DebugMissionScriptFileName = "IL2DCEDebug.cs";
        public const string RecordExt = ".trk";

        public const string SectionMain = "Main";
        public const string SectionCore = "Core";
        public const string SectionMissionFileConverter = "MissionFileConverter";
        public const string SectionQuickMissionPage = "QuickMissionPage";
        public const string SectionSkill = "Skill";
        public const string SectionMissionType = "MissionType";
        public const string SectionAircraft = "Aircraft";

        public const string KeySourceFolderFileName = "SourceFolderFileName";
        public const string KeySourceFolderFolderName = "SourceFolderFolderName";
        public const string KeyEnableFilterSelectCampaign = "EnableFilterSelectCampaign";
        public const string KeyEnableFilterSelectAirGroup = "EnableFilterSelectAirGroup";
        public const string KeyDisableMissionType = "Disable";
        public const string KeyRandomRed = "RandomRed";
        public const string KeyRandomBlue = "RandomBlue";
        public const string KeyEnableMissionMultiAssign = "EnableMissionMultiAssign";
        public const string KeyProcessTimeReArm = "ProcessTimeReArm";
        public const string KeyProcessTimeReFuel = "ProcessTimeReFuel";
        public const string KeyKillsHistoryMax = "KillsHistoryMax";

        public const string LogFileName = "il2dce.log";
        public const string ConvertLogFileName = "Convert.log";

        public const string DefaultFixedFontName = "Consolas";
        public const string KillsFormat = "F0";

        public const int DefaultAdditionalAirOperations = 3;
        public const int MaxAdditionalAirOperations = 12;
        public const int MinAdditionalAirOperations = 1;
        public const int DefaultAdditionalGroundOperations = 100;
        public const int MaxAdditionalGroundOperations = 300;
        public const int MinAdditionalGroundOperations = 10;
        public const int DefaultProcessTimeReArm = 300;
        public const int DefaultProcessTimeReFuel = 300;
        public const int KillsHistoryMaxDefault = 1000;

        public const int RankupExp = 1000;
        public const int ExpSuccess = 200;
        public const int ExpFail = 0;
        public const int ExpDraw = 100;

        #endregion

        public string CampaignsFolder
        {
            get
            {
                return _campaignsFolder;
            }
        }
        private string _campaignsFolder;

        public int AdditionalAirOperations
        {
            get
            {
                return _additionalAirOperations;
            }
        }
        private int _additionalAirOperations = DefaultAdditionalAirOperations;

        public int AdditionalGroundOperations
        {
            get
            {
                return _additionalGroundOperations;
            }
        }
        private int _additionalGroundOperations = DefaultAdditionalGroundOperations;

        public double FlightSize
        {
            get
            {
                return _flightSize;
            }
        }
        private double _flightSize = 1.0;

        public double FlightCount
        {
            get
            {
                return _flightCount;
            }
        }
        private double _flightCount = 1.0;

        public bool SpawnParked
        {
            get
            {
                return _spawnParked;
            }
            set
            {
                _spawnParked = value;
            }
        }
        public static bool _spawnParked = false;

        public int Debug
        {
            get
            {
                return _debug;
            }
            set
            {
                _debug = value;
            }
        }
        private int _debug = 0;

        public int StatType
        {
            get
            {
                return _statType;
            }
            set
            {
                _statType = value;
            }
        }
        private int _statType = 0;

        public double StatKillsOver
        {
            get
            {
                return _statKillsOver;
            }
            set
            {
                _statKillsOver = value;
            }
        }
        private double _statKillsOver = 0.5;

        public string[] SorceFolderFileName
        {
            get
            {
                return sorceFolderFileName;
            }
        }
        private string[] sorceFolderFileName;

        public string[] SorceFolderFolderName
        {
            get
            {
                return sorceFolderFolderName;
            }
        }
        private string[] sorceFolderFolderName;

        public bool EnableFilterSelectCampaign
        {
            get;
            private set;
        }

        public bool EnableFilterSelectAirGroup
        {
            get;
            private set;
        }

        public Skills Skills
        {
            get;
            private set;
        }

        public EMissionType[] DisableMissionType
        {
            get;
            private set;
        }

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

        public bool EnableMissionMultiAssign
        {
            get;
            private set;
        }

        public int ProcessTimeReArm
        {
            get;
            private set;
        }

        public int ProcessTimeReFuel
        {
            get;
            private set;
        }

        public int KillsHistoryMax
        {
            get;
            private set;
        }

        public static CultureInfo Culture = new CultureInfo("en-US", true);
        public static NumberFormatInfo NumberFormat = CultureInfo.InvariantCulture.NumberFormat;

        public static Version Version
        {
            get;
        }

        static Config()
        {
            Version = Assembly.GetExecutingAssembly().GetName().Version;
        }

        public static string CreateVersionString(Version targetVersion)
        {
            return string.Format("Version {0} [Core{1}]", targetVersion.ToString(), Version.ToString());
        }

        public Config(ISectionFile confFile)
        {
            if (confFile.exist(SectionMain, "campaignsFolder"))
            {
                _campaignsFolder = confFile.get(SectionMain, "campaignsFolder");
            }
            else
            {
                _campaignsFolder = CampaignsFolderDefault;
            }

            SpawnParked = false;
            if (confFile.exist(SectionCore, "forceSetOnPark"))
            {
                string value = confFile.get(SectionCore, "forceSetOnPark");
                if (value == "1")
                {
                    SpawnParked = true;
                }
                else
                {
                    SpawnParked = false;
                }
            }

            if (confFile.exist(SectionCore, "additionalAirOperations"))
            {
                string value = confFile.get(SectionCore, "additionalAirOperations");
                int.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat, out _additionalAirOperations);
            }

            if (confFile.exist(SectionCore, "additionalGroundOperations"))
            {
                string value = confFile.get(SectionCore, "additionalGroundOperations");
                int.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat, out _additionalGroundOperations);
            }

            if (confFile.exist(SectionCore, "flightSize"))
            {
                string value = confFile.get(SectionCore, "flightSize");
                double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat, out _flightSize);
            }

            if (confFile.exist(SectionCore, "flightCount"))
            {
                string value = confFile.get(SectionCore, "flightCount");
                double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat, out _flightCount);
            }

            if (confFile.exist(SectionCore, "debug"))
            {
                string value = confFile.get(SectionCore, "debug");
                int.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat, out _debug);
            }

            if (confFile.exist(SectionCore, "statType"))
            {
                string value = confFile.get(SectionCore, "statType");
                int.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat, out _statType);
            }

            if (confFile.exist(SectionCore, "statKillsOver"))
            {
                string value = confFile.get(SectionCore, "statKillsOver");
                double.TryParse(value, NumberStyles.Any, Culture, out _statKillsOver);
            }

            ProcessTimeReArm = confFile.get(SectionCore, KeyProcessTimeReArm, DefaultProcessTimeReArm);
            ProcessTimeReFuel = confFile.get(SectionCore, KeyProcessTimeReFuel, DefaultProcessTimeReFuel);

            if (confFile.exist(SectionCore, "statType"))
            {
                string value = confFile.get(SectionCore, "statType");
                int.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat, out _statType);
            }

            if (confFile.exist(SectionMissionFileConverter, KeySourceFolderFileName))
            {
                string str = confFile.get(SectionMissionFileConverter, KeySourceFolderFileName);
                sorceFolderFileName = string.IsNullOrEmpty(str) ? new string[0] : str.Split(SplitComma, System.StringSplitOptions.RemoveEmptyEntries);
            }
            else
            {
                sorceFolderFileName = new string[0];
            }

            if (confFile.exist(SectionMissionFileConverter, KeySourceFolderFolderName))
            {
                string str = confFile.get(SectionMissionFileConverter, KeySourceFolderFolderName);
                sorceFolderFolderName = string.IsNullOrEmpty(str) ? new string[0] : str.Split(SplitComma, System.StringSplitOptions.RemoveEmptyEntries);
            }
            else
            {
                sorceFolderFolderName = new string[0];
            }

            EnableFilterSelectCampaign = confFile.get(SectionQuickMissionPage, KeyEnableFilterSelectCampaign, 0) == 1;
            EnableFilterSelectAirGroup = confFile.get(SectionQuickMissionPage, KeyEnableFilterSelectAirGroup, 0) == 1;

            Skills = Skills.CreateDefault();
            if (confFile.exist(SectionSkill))
            {
                string key;
                string value;
                int lines = confFile.lines(SectionSkill);
                for (int i = 0; i < lines; i++)
                {
                    confFile.get(SectionSkill, i, out key, out value);
                    System.Diagnostics.Debug.WriteLine("Skill[{0}] name={1} Value={2}", i, key, value ?? string.Empty);
                    // if you need delete default defined skill, please write no value key in ini file.
                    var targetSkills = Skills.Where(x => string.Compare(x.Name, key, true) == 0).ToArray();
                    if (targetSkills.Any())
                    {
                        if (string.IsNullOrEmpty(value))
                        {
                            foreach (var item in targetSkills)
                            {
                                Skills.Remove(item);
                            }
                        }
                        else
                        {
                            Skill skill;
                            if (Skill.TryParse(value, out skill))
                            {
                                foreach (var item in targetSkills)
                                {
                                    item.Skills = skill.Skills;
                                }
                            }
                        }
                    }
                    else
                    {
                        Skill skill;
                        if (Skill.TryParse(value, out skill))
                        {
                            skill.Name = key;
                            Skills.Add(skill);
                        }
                    }
                }
            }

            if (confFile.exist(SectionMissionType, KeyDisableMissionType))
            {
                string value = confFile.get(SectionMissionType, KeyDisableMissionType);
                string[] values = value.Split(SplitSpace, StringSplitOptions.RemoveEmptyEntries);
                List<EMissionType> missionTypes = new List<EMissionType>();
                foreach (var item in values)
                {
                    EMissionType missionType;
                    if (Enum.TryParse<EMissionType>(item, true, out missionType))
                    {
                        missionTypes.Add(missionType);
                    }
                }
                DisableMissionType = missionTypes.ToArray();
            }
            else
            {
                DisableMissionType = new EMissionType[0];
            }

            if (confFile.exist(SectionAircraft, KeyRandomRed))
            {
                string value = confFile.get(SectionAircraft, KeyRandomRed);
                AircraftRandomRed = value.Split(SplitSpace, StringSplitOptions.RemoveEmptyEntries);
            }
            else
            {
                AircraftRandomRed = new string[0];
            }

            if (confFile.exist(SectionAircraft, KeyRandomBlue))
            {
                string value = confFile.get(SectionAircraft, KeyRandomBlue);
                AircraftRandomBlue = value.Split(SplitSpace, StringSplitOptions.RemoveEmptyEntries);
            }
            else
            {
                AircraftRandomBlue = new string[0];
            }

            EnableMissionMultiAssign = confFile.get(SectionCore, KeyEnableMissionMultiAssign, 0) == 1;

            KillsHistoryMax = confFile.get(SectionCore, KeyKillsHistoryMax, KillsHistoryMaxDefault);
        }
    }
}
