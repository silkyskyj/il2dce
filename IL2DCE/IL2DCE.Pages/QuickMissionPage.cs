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
using maddox.game.play;

namespace IL2DCE
{
    namespace Pages
    {
        public class QuickMissionPage : PageDefImpl
        {
            public QuickMissionPage()
                : base("Quick Mission", new QuickMission())
            {
                FrameworkElement.Start.Click += new System.Windows.RoutedEventHandler(Start_Click);
                FrameworkElement.Back.Click += new System.Windows.RoutedEventHandler(Back_Click);
                FrameworkElement.comboBoxSelectArmy.SelectionChanged += new System.Windows.Controls.SelectionChangedEventHandler(comboBoxSelectArmy_SelectionChanged);
                FrameworkElement.comboBoxSelectAirForce.SelectionChanged += new System.Windows.Controls.SelectionChangedEventHandler(comboBoxSelectAirForce_SelectionChanged);
                FrameworkElement.textBoxPilotName.TextChanged += new System.Windows.Controls.TextChangedEventHandler(textBoxPilotName_TextChanged);
            }

            public override void _enter(maddox.game.IGame play, object arg)
            {
                base._enter(play, arg);

                FrameworkElement.comboBoxSelectArmy.Items.Clear();

                _game = play as IGame;

                System.Windows.Controls.ComboBoxItem itemArmyRed = new System.Windows.Controls.ComboBoxItem();
                itemArmyRed.Content = "Red";
                itemArmyRed.Tag = 1;
                FrameworkElement.comboBoxSelectArmy.Items.Add(itemArmyRed);
                System.Windows.Controls.ComboBoxItem itemArmyBlue = new System.Windows.Controls.ComboBoxItem();
                itemArmyBlue.Content = "Blue";
                itemArmyBlue.Tag = 2;
                FrameworkElement.comboBoxSelectArmy.Items.Add(itemArmyBlue);
                FrameworkElement.comboBoxSelectArmy.SelectedIndex = 0;
            }

            public override void _leave(maddox.game.IGame play, object arg)
            {
                base._leave(play, arg);

                _game = null;
            }

            private CareerIntro FrameworkElement
            {
                get
                {
                    return FE as CareerIntro;
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

            void textBoxPilotName_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
            {
                if (Game != null)
                {
                    string pilotName = FrameworkElement.textBoxPilotName.Text;
                    if (pilotName != null && pilotName != String.Empty)
                    {
                        foreach (Career career in Game.Core.AvailableCareers)
                        {
                            if (career.PilotName == pilotName)
                            {
                                FrameworkElement.Start.IsEnabled = false;
                                return;
                            }
                        }
                    }

                    FrameworkElement.Start.IsEnabled = true;
                }
            }

            void comboBoxSelectArmy_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
            {
                if (e.AddedItems.Count == 1)
                {
                    System.Windows.Controls.ComboBoxItem armySelected = e.AddedItems[0] as System.Windows.Controls.ComboBoxItem;
                    int armyIndex = (int)armySelected.Tag;

                    FrameworkElement.comboBoxSelectAirForce.Items.Clear();

                    if (armyIndex == 1)
                    {
                        System.Windows.Controls.ComboBoxItem itemRaf = new System.Windows.Controls.ComboBoxItem();
                        itemRaf.Tag = 1;
                        itemRaf.Content = "Royal Air Force";
                        FrameworkElement.comboBoxSelectAirForce.Items.Add(itemRaf);

                        FrameworkElement.comboBoxSelectAirForce.SelectedIndex = 0;
                    }
                    else if (armyIndex == 2)
                    {
                        System.Windows.Controls.ComboBoxItem itemLw = new System.Windows.Controls.ComboBoxItem();
                        itemLw.Tag = 1;
                        itemLw.Content = "Luftwaffe";
                        FrameworkElement.comboBoxSelectAirForce.Items.Add(itemLw);

                        System.Windows.Controls.ComboBoxItem itemRa = new System.Windows.Controls.ComboBoxItem();
                        itemRa.Tag = 2;
                        itemRa.Content = "Regia Aeronautica";
                        FrameworkElement.comboBoxSelectAirForce.Items.Add(itemRa);

                        FrameworkElement.comboBoxSelectAirForce.SelectedIndex = 0;
                    }
                }
            }

            void comboBoxSelectAirForce_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
            {
                if (e.AddedItems.Count == 1)
                {
                    System.Windows.Controls.ComboBoxItem armySelected = FrameworkElement.comboBoxSelectArmy.SelectedItem as System.Windows.Controls.ComboBoxItem;
                    int armyIndex = (int)armySelected.Tag;

                    System.Windows.Controls.ComboBoxItem airForceSelected = e.AddedItems[0] as System.Windows.Controls.ComboBoxItem;
                    int airForceIndex = (int)airForceSelected.Tag;



                    if (armyIndex == 1 && airForceIndex == 1)
                    {
                        FrameworkElement.textBoxPilotName.Text = "Joe Bloggs";
                    }
                    else if (armyIndex == 2 && airForceIndex == 1)
                    {
                        FrameworkElement.textBoxPilotName.Text = "Max Mustermann";
                    }
                    else if (armyIndex == 2 && airForceIndex == 2)
                    {
                        FrameworkElement.textBoxPilotName.Text = "Mario Rossi";
                    }


                    FrameworkElement.comboBoxSelectRank.Items.Clear();
                    for (int i = 0; i < 6; i++)
                    {
                        System.Windows.Controls.ComboBoxItem itemRank = new System.Windows.Controls.ComboBoxItem();
                        if (armyIndex == 1 && airForceIndex == 1)
                        {
                            itemRank.Content = Career.RafRanks[i];
                        }
                        else if (armyIndex == 2 && airForceIndex == 1)
                        {
                            itemRank.Content = Career.LwRanks[i];
                        }
                        else if (armyIndex == 2 && airForceIndex == 2)
                        {
                            itemRank.Content = Career.RaRanks[i];
                        }
                        itemRank.Tag = i;
                        FrameworkElement.comboBoxSelectRank.Items.Add(itemRank);
                    }
                    FrameworkElement.comboBoxSelectRank.SelectedIndex = 0;
                }
            }

            private void Back_Click(object sender, System.Windows.RoutedEventArgs e)
            {
                if (Game.gameInterface.BattleIsRun())
                {
                    Game.gameInterface.BattleStop();
                }

                Game.gameInterface.PagePop(null);
            }

            private void Start_Click(object sender, System.Windows.RoutedEventArgs e)
            {
                string pilotName = FrameworkElement.textBoxPilotName.Text;

                System.Windows.Controls.ComboBoxItem armySelected = FrameworkElement.comboBoxSelectArmy.SelectedItem as System.Windows.Controls.ComboBoxItem;
                int armyIndex = (int)armySelected.Tag;

                System.Windows.Controls.ComboBoxItem airForceSelected = FrameworkElement.comboBoxSelectAirForce.SelectedItem as System.Windows.Controls.ComboBoxItem;
                int airForceIndex = (int)airForceSelected.Tag;

                System.Windows.Controls.ComboBoxItem rankSelected = FrameworkElement.comboBoxSelectRank.SelectedItem as System.Windows.Controls.ComboBoxItem;
                int rankIndex = (int)rankSelected.Tag;

                Career career = new Career(pilotName, armyIndex, airForceIndex, rankIndex);
                Game.Core.CurrentCareer = career;
                Game.Core.AvailableCareers.Add(career);

                Game.gameInterface.PageChange(new SelectCampaignPage(), null);
            }
        }
    }
}