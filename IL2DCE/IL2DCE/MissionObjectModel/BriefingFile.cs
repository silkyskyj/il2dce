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

using System.Collections.Generic;
using System.IO;

namespace IL2DCE.MissionObjectModel
{
    public class BriefingFile
    {
        public class Text
        {
            public IDictionary<string, string> Sections
            {
                get
                {
                    return this.sections;
                }
            }
            IDictionary<string, string> sections = new Dictionary<string, string>();

            public override string ToString()
            {
                string result = "";
                foreach (string key in Sections.Keys)
                {
                    result += Sections[key] + "\n\n";
                }
                return result;
            }
        }

        public string MissionName
        {
            get
            {
                return _missionName;
            }
            set
            {
                _missionName = value;
            }
        }
        private string _missionName = "";

        public string MissionDescription
        {
            get
            {
                return _missionDescription;
            }
            set
            {
                _missionDescription = value;
            }
        }
        private string _missionDescription = "";

        public IDictionary<string, string> Name
        {
            get
            {
                return _name;
            }
        }
        private Dictionary<string, string> _name = new Dictionary<string, string>();

        public IDictionary<string, Text> Description
        {
            get
            {
                return _description;
            }
        }
        private Dictionary<string, Text> _description = new Dictionary<string, Text>();

        public void SaveTo(string systemFileName)
        {
            TextWriter briefingFileWriter = new StreamWriter(systemFileName, false);

            briefingFileWriter.WriteLine("[Info]");
            briefingFileWriter.WriteLine("<Name>");
            briefingFileWriter.WriteLine("Info");
            briefingFileWriter.WriteLine("<Caption>");
            briefingFileWriter.WriteLine(MissionName);
            briefingFileWriter.WriteLine("<Caption>");
            briefingFileWriter.WriteLine(MissionDescription);

            foreach (string key in Name.Keys)
            {
                briefingFileWriter.WriteLine("[" + key + "]");
                briefingFileWriter.WriteLine("<Name>");
                briefingFileWriter.WriteLine(Name[key]);

                briefingFileWriter.WriteLine("<Description>");
                if (Description.ContainsKey(key))
                {
                    briefingFileWriter.WriteLine(Description[key]);
                }
                else
                {
                    briefingFileWriter.WriteLine("");
                }
            }

            briefingFileWriter.Close();
        }
    }
}