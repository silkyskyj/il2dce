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

using System.Reflection;
using System.Runtime.InteropServices;

#if DEBUG
[assembly: AssemblyConfiguration("Debug")]
#else
[assembly: AssemblyConfiguration("Release")]
#endif
[assembly: AssemblyTitle("IL2DCE.Pages")]
[assembly: AssemblyDescription("A dynamic campaign engine & dynamicmission for IL-2 Sturmovik: Cliffs of Dover Blitz + DLC")]
[assembly: Guid("2c4653c3-9629-4d26-8596-412fc6780829")]
[assembly: AssemblyCompany("https://github.com/silkyskyj/il2dce")]
[assembly: AssemblyProduct("IL2DCE [Forked by silkysky]")]
[assembly: AssemblyCopyright("Copyright © Stefan Rothdach 2011- & silkysky 2025-")]
[assembly: AssemblyTrademark("AGPLv3, or (at your option) any later version")]
[assembly: AssemblyCulture("")]
[assembly: ComVisible(false)]
[assembly: AssemblyVersion("0.7.4.0")]
[assembly: AssemblyFileVersion("0.7.4.0")]
