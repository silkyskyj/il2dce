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

using System.Reflection;
using System.Runtime.InteropServices;

#if DEBUG
[assembly: AssemblyConfiguration("Debug")]
#else
[assembly: AssemblyConfiguration("Release")]
#endif
[assembly: AssemblyTitle("IL2DCE.Core")]
[assembly: AssemblyDescription("A dynamic campaign engine & dynamicmission for IL-2 Sturmovik: Cliffs of Dover Blitz + Desert Wings")]
[assembly: Guid("df52f1e2-6b2a-4726-9f24-39b7c3adc6da")]
[assembly: AssemblyCompany("https://github.com/il2dce/il2dce")]
[assembly: AssemblyProduct("IL2DCE [Forked by silkyskyj]")]
[assembly: AssemblyCopyright("Copyright © Stefan Rothdach 2011- & silkyskyj 2025-")]
[assembly: AssemblyTrademark("AGPLv3, or (at your option) any later version")]
[assembly: AssemblyCulture("")]
[assembly: ComVisible(false)]
[assembly: AssemblyVersion("0.7.3.0")]
[assembly: AssemblyFileVersion("0.7.3.0")]
