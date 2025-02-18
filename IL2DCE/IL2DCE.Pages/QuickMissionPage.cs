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

using System.Linq;
using System.Windows;
using System.Windows.Controls;
using maddox.game;
using maddox.game.play;

namespace IL2DCE
{
    namespace Pages
    {
        public class QuickMissionPage : PageDefImpl
        {
            private QuickMission FrameworkElement
            {
                get
                {
                    return FE as QuickMission;
                }
            }

            private IGame Game
            {
                get
                {
                    return _game;
                }
            }
            private IGame _game;

            private int SelectedArmyIndex
            {
                get
                {
                    ComboBoxItem selected = FrameworkElement.comboBoxSelectArmy.SelectedItem as ComboBoxItem;
                    if (selected != null)
                    {
                        return (int)selected.Tag;
                    }

                    return -1;
                }
            }

            private int SelectedAirForceIndex
            {
                get
                {
                    ComboBoxItem selected = FrameworkElement.comboBoxSelectAirForce.SelectedItem as ComboBoxItem;
                    if (selected != null)
                    {
                        return (int)selected.Tag;
                    }

                    return -1;
                }
            }

            private CampaignInfo SelectedCampaign
            {
                get
                {
                    return FrameworkElement.comboBoxSelectCampaign.SelectedItem as CampaignInfo;
                }
            }

            private AirGroup SelectedAirGroup
            {
                get
                {
                    ComboBoxItem selected =  FrameworkElement.comboBoxSelectAirGroup.SelectedItem as ComboBoxItem;
                    if (selected != null)
                    {
                        return (AirGroup)selected.Tag;
                    }

                    return null;
                }
            }

            private EMissionType? SelectedMissionType
            {
                get
                {
                    ComboBoxItem selected = FrameworkElement.comboBoxSelectMissionType.SelectedItem as ComboBoxItem;
                    if (selected != null)
                    {
                        if (selected.Tag != null)
                        {
                            return (EMissionType)selected.Tag;
                        }
                    }

                    return null;
                }
            }

            private int SelectedRank
            {
                get
                {
                    ComboBoxItem selected = FrameworkElement.comboBoxSelectRank.SelectedItem as ComboBoxItem;
                    if (selected != null)
                    {
                        return (int)selected.Tag;
                    }

                    return -1;
                }
            }

            private MissionFile CurrentMissionFile = null;

            public QuickMissionPage()
                : base("Quick Mission", new QuickMission())
            {
                FrameworkElement.Start.Click += new System.Windows.RoutedEventHandler(Start_Click);
                FrameworkElement.Back.Click += new System.Windows.RoutedEventHandler(Back_Click);
                FrameworkElement.comboBoxSelectCampaign.SelectionChanged += new SelectionChangedEventHandler(comboBoxSelectCampaign_SelectionChanged);
                FrameworkElement.comboBoxSelectArmy.SelectionChanged += new SelectionChangedEventHandler(comboBoxSelectArmy_SelectionChanged);
                FrameworkElement.comboBoxSelectAirForce.SelectionChanged += new SelectionChangedEventHandler(comboBoxSelectAirForce_SelectionChanged);
                FrameworkElement.comboBoxSelectAirGroup.SelectionChanged += new SelectionChangedEventHandler(comboBoxSelectAirGroup_SelectionChanged);
                FrameworkElement.comboBoxSelectMissionType.SelectionChanged += new SelectionChangedEventHandler(comboBoxSelectMissionType_SelectionChanged);

                FrameworkElement.labelSelectMissionTarget.Visibility = Visibility.Hidden;
                FrameworkElement.comboBoxSelectTarget.Visibility = Visibility.Hidden;
                FrameworkElement.labelSelectAltitude.Visibility = Visibility.Hidden;
                FrameworkElement.comboBoxSelectAltitude.Visibility = Visibility.Hidden;
            }

            public override void _enter(maddox.game.IGame play, object arg)
            {
                base._enter(play, arg);

                _game = play as IGame;

                UpdateCampaignComboBoxInfo();

                if (Game.Core.CurrentCareer != null)
                {
                    SelectLastInfo(Game.Core.CurrentCareer);
                }
            }

            public override void _leave(maddox.game.IGame play, object arg)
            {
                base._leave(play, arg);

                _game = null;
            }

            void comboBoxSelectCampaign_SelectionChanged(object sender, SelectionChangedEventArgs e)
            {
                if (e.AddedItems.Count > 0)
                {

                    UpdateArmyComboBoxInfo();

                    CampaignInfo campaignInfo = SelectedCampaign;
                    if (campaignInfo != null)
                    {

                    }
                }

                UpdateButtonStatus();
            }

            void comboBoxSelectArmy_SelectionChanged(object sender, SelectionChangedEventArgs e)
            {
                if (e.AddedItems.Count > 0)
                {
                    UpdateAirForceComboBoxInfo();
                }

                UpdateButtonStatus();
            }

            void comboBoxSelectAirForce_SelectionChanged(object sender, SelectionChangedEventArgs e)
            {
                if (e.AddedItems.Count > 0)
                {
                    UpdateRankComboBoxInfo();
                    UpdateAirGroupComboBoxInfo();
                }

                UpdateButtonStatus();
            }

            void comboBoxSelectAirGroup_SelectionChanged(object sender, SelectionChangedEventArgs e)
            {
                if (e.AddedItems.Count > 0)
                {
                    UpdateMissionTypeComboBoxInfo();
                }

                UpdateButtonStatus();
            }

            void comboBoxSelectMissionType_SelectionChanged(object sender, SelectionChangedEventArgs e)
            {
                if (e.AddedItems.Count > 0)
                {
                    UpdateSelectTargetComboBoxInfo();
                }

                UpdateButtonStatus();
            }

            private void Back_Click(object sender, RoutedEventArgs e)
            {
                if (Game.gameInterface.BattleIsRun())
                {
                    Game.gameInterface.BattleStop();
                }

                Game.Core.CurrentCareer.CampaignInfo = null;
                Game.Core.CurrentCareer = null;

                Game.gameInterface.PagePop(null);
            }

            private void Start_Click(object sender, RoutedEventArgs e)
            {
                GameIterface gameInterface = Game.gameInterface;
                string pilotName = gameInterface.Player().Name();

                int armyIndex = SelectedArmyIndex;
                int airForceIndex = SelectedAirForceIndex;
                int rankIndex = SelectedRank;
                AirGroup airGroup = SelectedAirGroup;
                CampaignInfo campaignInfo = SelectedCampaign;

                Career career = new Career(pilotName, armyIndex, airForceIndex, rankIndex);
                career.BattleType = EBattleType.QuickMission;
                career.CampaignInfo = campaignInfo;
                career.AirGroup = airGroup.AirGroupKey + "." + airGroup.SquadronIndex;
                career.MissionType = SelectedMissionType;
                career.PlayerAirGroup = airGroup;

                // string aircraft = Regex.Match((item.Content as string), "\\(.+\\)").Value.Trim("()".ToCharArray());
                // career.Aircraft = aircraft;
                Game.Core.CurrentCareer = career;

                campaignInfo.EndDate = campaignInfo.StartDate;
                Game.Core.CreateQuickMission(Game, career);

                Game.gameInterface.PageChange(new BattleIntroPage(), null);
            }

            private void SelectLastInfo(Career career)
            {
                FrameworkElement.comboBoxSelectCampaign.SelectedItem = career.CampaignInfo;
                EnableSelectItem(FrameworkElement.comboBoxSelectArmy, Career.Army[career.ArmyIndex - 1]);
                EnableSelectItem(FrameworkElement.comboBoxSelectAirForce, Career.AirForce[(career.ArmyIndex - 1) * 3 + career.AirForceIndex - 1]);
                FrameworkElement.comboBoxSelectRank.SelectedIndex = career.RankIndex;
                EnableSelectItem(FrameworkElement.comboBoxSelectAirGroup, CreateAirGroupContent(career.PlayerAirGroup, career.CampaignInfo));
                EnableSelectItem(FrameworkElement.comboBoxSelectMissionType, career.MissionType != null ? career.MissionType.ToDescription(): string.Empty);
            }

            private void UpdateCampaignComboBoxInfo()
            {
                ComboBox comboBox = FrameworkElement.comboBoxSelectCampaign;

                comboBox.Items.Clear();

                foreach (CampaignInfo campaignInfo in Game.Core.CampaignInfos)
                {
                    comboBox.Items.Add(campaignInfo);
                }

                comboBox.SelectedIndex = comboBox.Items.Count > 0 ? 0 : -1;
            }

            private void UpdateArmyComboBoxInfo(bool checkArmy = false)
            {
                CampaignInfo campaignInfo = SelectedCampaign;

                ComboBox comboBox = FrameworkElement.comboBoxSelectArmy;
                string selected = comboBox.SelectedItem != null ? (comboBox.SelectedItem as ComboBoxItem).Content as string: string.Empty;
                comboBox.Items.Clear();

                if (campaignInfo != null && (!checkArmy || CurrentMissionFile != null))
                {
                    var armys = checkArmy ? CurrentMissionFile.AirGroups.Select(x => x.ArmyIndex).Distinct(): new int [0];
                    for (int i = 0; i < (int)ArmyType.Count; i++)
                    {
                        if (!checkArmy || armys.Contains(i + 1))
                        {
                            comboBox.Items.Add(new ComboBoxItem() { Tag = i + 1, Content = Career.Army[i] });
                        }
                    }
                }

                EnableSelectItem(comboBox, selected);
            }

            private void UpdateAirForceComboBoxInfo(bool checkAirForce = false)
            {
                ComboBox comboBox = FrameworkElement.comboBoxSelectAirForce;
                string selected = comboBox.SelectedItem != null ? (comboBox.SelectedItem as ComboBoxItem).Content as string : string.Empty;
                comboBox.Items.Clear();

                int armyIndex = SelectedArmyIndex;
                CampaignInfo campaignInfo = SelectedCampaign;
                if (campaignInfo != null && armyIndex != -1 && (!checkAirForce || CurrentMissionFile != null))
                {
                    var airForces = checkAirForce ? CurrentMissionFile.AirGroups.Where(x => x.ArmyIndex == armyIndex).Select(x => x.AirGroupInfo.AirForceIndex).Distinct(): new int [0];
                    if (armyIndex == (int)ArmyType.Red)
                    {
                        for (int i = 0; i < (int)AirForceRed.Count; i++)
                        {
                            if (!checkAirForce || airForces.Contains(i + 1))
                            {
                                comboBox.Items.Add(new ComboBoxItem() { Tag = i + 1, Content = Career.AirForce[i] });
                            }
                        }
                    }
                    else if (armyIndex == (int)ArmyType.Blue)
                    {
                        int diff = (int)AirForceRed.Count;
                        for (int i = 0; i < (int)AirForceBlue.Count; i++)
                        {
                            if (!checkAirForce || airForces.Contains(i + 1))
                            {
                                comboBox.Items.Add(new ComboBoxItem() { Tag = i + 1, Content = Career.AirForce[i + diff] });
                            }
                        }
                    }
                }

                EnableSelectItem(comboBox, selected);
            }

            private void UpdateRankComboBoxInfo()
            {
                ComboBox comboBox = FrameworkElement.comboBoxSelectRank;
                int selected = comboBox.SelectedIndex;
                comboBox.Items.Clear();

                int armyIndex = SelectedArmyIndex;
                int airForceIndex = SelectedAirForceIndex;

                if (armyIndex != -1 && airForceIndex != -1)
                {
                    int airforce = (armyIndex - 1) * 3 + airForceIndex - 1;

                    for (int i = 0; i <= Career.RankMax; i++)
                    {
                        comboBox.Items.Add(
                            new ComboBoxItem()
                            {
                                Content = Career.Rank[airforce][i],
                                Tag = i,
                            });
                    }
                }

                if (comboBox.Items.Count > 0)
                {
                    comboBox.IsEnabled = true;
                    comboBox.SelectedIndex = selected != -1 ? selected: 0;
                }
                else
                {
                    comboBox.IsEnabled = false;
                    comboBox.SelectedIndex = -1;
                }
            }

            private void UpdateAirGroupComboBoxInfo()
            {
                ComboBox comboBox = FrameworkElement.comboBoxSelectAirGroup;
                string selected = comboBox.SelectedItem != null ? (comboBox.SelectedItem as ComboBoxItem).Content as string : string.Empty;
                comboBox.Items.Clear();

                CampaignInfo campaignInfo = SelectedCampaign;
                if (campaignInfo != null)
                {
                    int armyIndex = SelectedArmyIndex;
                    int airForceIndex = SelectedAirForceIndex;

                    if (armyIndex != -1 && airForceIndex != -1)
                    {
                        CurrentMissionFile = new MissionFile(Game, campaignInfo.InitialMissionTemplateFiles, campaignInfo.AirGroupInfos);
                        foreach (AirGroup airGroup in CurrentMissionFile.AirGroups)
                        {
                            AirGroupInfo airGroupInfo = airGroup.AirGroupInfo;
                            AircraftInfo aircraftInfo = campaignInfo.GetAircraftInfo(airGroup.Class);
                            if (airGroupInfo.ArmyIndex == armyIndex && airGroupInfo.AirForceIndex == airForceIndex && aircraftInfo.IsFlyable)
                            {
                                comboBox.Items.Add(new ComboBoxItem() { Tag = airGroup, Content = CreateAirGroupContent(airGroup, campaignInfo) });
                            }
                        }
                    }
                    else
                    {
                        CurrentMissionFile = null;
                    }
                }
                else
                {
                    CurrentMissionFile = null;
                }

                EnableSelectItem(comboBox, selected);
            }

            private string CreateAirGroupContent(AirGroup airGroup, CampaignInfo campaignInfo)
            {
                AircraftInfo aircraftInfo = campaignInfo.GetAircraftInfo(airGroup.Class);
                return string.Format("{0} ({1})", airGroup.DisplayName, aircraftInfo.DisplayName);
            }

            private void UpdateMissionTypeComboBoxInfo()
            {
                ComboBox comboBox = FrameworkElement.comboBoxSelectMissionType;
                string selected = comboBox.SelectedItem != null ? (comboBox.SelectedItem as ComboBoxItem).Content as string : string.Empty;
                comboBox.Items.Clear();

                CampaignInfo campaignInfo = SelectedCampaign;
                AirGroup airGroup = SelectedAirGroup;
                if (campaignInfo != null && CurrentMissionFile != null && airGroup != null)
                {
                    comboBox.Items.Add(new ComboBoxItem() { Tag = null, Content = " - Random - " });
                    // AirGroupInfo airGroupInfo = airGroup.AirGroupInfo;
                    AircraftInfo aircraftInfo = campaignInfo.GetAircraftInfo(airGroup.Class);
                    foreach (var item in aircraftInfo.MissionTypes)
                    {
                        comboBox.Items.Add(new ComboBoxItem() { Tag = item, Content = item.ToDescription() });
                    }
                }

                EnableSelectItem(comboBox, selected);
            }

            private void UpdateSelectTargetComboBoxInfo()
            {
                ComboBox comboBox = FrameworkElement.comboBoxSelectTarget;
                string selected = comboBox.SelectedItem != null ? (comboBox.SelectedItem as ComboBoxItem).Content as string : string.Empty;
                comboBox.Items.Clear();

                CampaignInfo campaignInfo = SelectedCampaign;
                AirGroup airGroup = SelectedAirGroup;
                EMissionType? missionType = SelectedMissionType;
                if (campaignInfo != null && CurrentMissionFile != null && airGroup != null && missionType != null)
                {
                    //AirGroupInfo airGroupInfo = airGroup.AirGroupInfo;
                    //AircraftInfo aircraftInfo = campaignInfo.GetAircraftInfo(airGroup.Class);
                    //foreach (var item in aircraftInfo.MissionTypes)
                    //{
                    //    comboBox.Items.Add(item.ToDescription());
                    //}

                    switch (missionType.Value)
                    {
                        case EMissionType.RECON:
                            break;

                        case EMissionType.MARITIME_RECON:
                            break;

                        case EMissionType.ARMED_RECON:
                            break;

                        case EMissionType.ARMED_MARITIME_RECON:
                            break;

                        case EMissionType.ATTACK_ARMOR:
                            break;

                        case EMissionType.ATTACK_VEHICLE:
                            break;

                        case EMissionType.ATTACK_TRAIN:
                            break;

                        case EMissionType.ATTACK_SHIP:
                            break;

                        case EMissionType.ATTACK_ARTILLERY:
                            break;

                        case EMissionType.ATTACK_RADAR:
                            break;

                        case EMissionType.ATTACK_AIRCRAFT:
                            break;

                        case EMissionType.ATTACK_DEPOT:
                            break;

                        case EMissionType.INTERCEPT:
                            break;

                        case EMissionType.ESCORT:
                            break;

                        case EMissionType.COVER:
                            break;

                        default:
                            break;
                    }
                }

                EnableSelectItem(comboBox, selected);
            }

            private void EnableSelectItem(ComboBox comboBox, string selected)
            {
                if (comboBox.Items.Count > 0)
                {
                    comboBox.IsEnabled = true;
                    comboBox.Text = selected;
                    if (comboBox.SelectedIndex == -1)
                    {
                        comboBox.SelectedIndex = 0;
                    }
                }
                else
                {
                    comboBox.IsEnabled = false;
                    comboBox.SelectedIndex = -1;
                }
            }

            private void UpdateAltitudeComboBoxInfo()
            {

            }

            private void UpdateButtonStatus()
            {
                FrameworkElement.Start.IsEnabled = SelectedArmyIndex != -1 && SelectedAirForceIndex != -1 && SelectedCampaign != null &&
                                                    SelectedAirGroup != null && SelectedRank != -1;


            }

        }
    }
}