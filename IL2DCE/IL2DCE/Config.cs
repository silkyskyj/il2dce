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

using System.Globalization;
using maddox.game;

namespace IL2DCE
{
    public class Config
    {

        #region Define

        public static readonly char[] SplitSpace = new char[] { ' ' };
        public static readonly char[] SplitComma = new char[] { ',' };

        public const string HomeFolder = "$home/";
        public const string UserFolder = "$user/";
        public const string PartsFolder = "$home/parts";
        public const string CampaignsFolderDefault = "$home/parts/IL2DCE/Campaigns";
        public const string MissionFolderSingle = "$home/missions/Single";
        public const string MissionFolderFormatSingle = "$home/parts/{0}/missions/Single";
        public const string MissionFolderFormatQuick = "$home/parts/{0}/mission/Quick";
        public const string MissionFolderFormatCampaign = "$home/parts/{0}/mission/campaign";
        
        public const string ConfigFilePath = "$home/parts/IL2DCE/conf.ini";
        public const string CampaignInfoFileName = "CampaignInfo.ini";
        public const string AircraftInfoFileName = "AircraftInfo.ini";
        public const string AirGroupInfoFileName = "AirGroupInfo.ini";
        public const string CareerInfoFileName = "Career.ini";
        public const string MissionScriptFileName = "MissionSingle.cs";
        public const string UserMissionFolder = "$user/mission/IL2DCE";
        public const string UserMissionsFolder = "$user/missions/IL2DCE";
        public const string DebugFolderName = "debug";
        public const string DebugMissionTemplateFileName = "IL2DCEDebugTemplate.mis";
        public const string DebugMissionFileName = "IL2DCEDebug.mis";
        public const string DebugBriefingFileName = "IL2DCEDebug.briefing";
        public const string DebugMissionScriptFileName = "IL2DCEDebug.cs";

        public const string SectionMain = "Main";
        public const string SectionCore = "Core";
        public const string SectionMissionFileConverter = "MissionFileConverter";
        public const string SectionQuickMissionPage = "QuickMissionPage";

        public const string KeySorceFolderFileName = "SorceFolderFileName";
        public const string KeySorceFolderFolderName = "SorceFolderFolderName";
        public const string KeyEnableFilterSelectCampaign = "EnableFilterSelectCampaign";
        public const string KeyEnableFilterSelectAirGroup = "EnableFilterSelectAirGroup";
        public const string KeyEnableAutoSelectComboBoxItem = "EnableAutoSelectComboBoxItem";

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
        private int _additionalAirOperations = 0;

        public int AdditionalGroundOperations
        {
            get
            {
                return _additionalGroundOperations;
            }
        }
        private int _additionalGroundOperations = 0;

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
        private string [] sorceFolderFileName;

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

        public bool EnableAutoSelectComboBoxItem
        {
            get;
            private set;
        }

        public static CultureInfo Culture = new CultureInfo("en-US", true);

        public Config(ISectionFile confFile)
        {
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

            if (confFile.exist(SectionMain, "campaignsFolder"))
            {
                _campaignsFolder = confFile.get(SectionMain, "campaignsFolder");
            }
            else
            {
                _campaignsFolder = CampaignsFolderDefault;
            }

            if (confFile.exist(SectionMissionFileConverter, KeySorceFolderFileName))
            {
                string str = confFile.get(SectionMissionFileConverter, KeySorceFolderFileName);
                sorceFolderFileName = string.IsNullOrEmpty(str) ? new string[0] : str.Split(SplitComma, System.StringSplitOptions.RemoveEmptyEntries);
            }
            else
            {
                sorceFolderFileName = new string[0];
            }

            if (confFile.exist(SectionMissionFileConverter, KeySorceFolderFolderName))
            {
                string str = confFile.get(SectionMissionFileConverter, KeySorceFolderFolderName);
                sorceFolderFolderName = string.IsNullOrEmpty(str) ? new string[0] : str.Split(SplitComma, System.StringSplitOptions.RemoveEmptyEntries);
            }
            else
            {
                sorceFolderFolderName = new string[0];
            }

            EnableFilterSelectCampaign = confFile.get(SectionQuickMissionPage, KeyEnableFilterSelectCampaign, 0) == 1;
            EnableFilterSelectAirGroup = confFile.get(SectionQuickMissionPage, KeyEnableFilterSelectAirGroup, 0) == 1;
            EnableAutoSelectComboBoxItem = confFile.get(SectionQuickMissionPage, KeyEnableAutoSelectComboBoxItem, 0) == 1;
        }
    }
}
