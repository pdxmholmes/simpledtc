#region License

// Copyright (c) 2016, Matt Holmes
// All rights reserved.
// 
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions are met:
//     * Redistributions of source code must retain the above copyright
//       notice, this list of conditions and the following disclaimer.
//     * Redistributions in binary form must reproduce the above copyright
//       notice, this list of conditions and the following disclaimer in the
//       documentation and/or other materials provided with the distribution.
//     * Neither the name of the project nor the
//       names of its contributors may be used to endorse or promote products
//       derived from this software without specific prior written permission.
// 
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND 
// ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT  LIMITED TO, THE IMPLIED WARRANTIES 
// OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL 
// THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, 
// SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT  LIMITED TO, PROCUREMENT 
// OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) 
// HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR 
// TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, 
// EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using IniParser;
using IniParser.Model;

namespace SimpleDtc.Core.Data {
    internal class DataCartridgeParser {
        private static readonly Regex SteerpointNumberRegex = new Regex (@".*_(\d+)");

        public DataCartridge Parse (string path) {
            var parser = new FileIniDataParser ();
            var data = parser.ReadFile (path);

            var weaponPointData = data["STPT"]?.Where (k => k.KeyName.StartsWith ("wpntarget_"));
            var targetPointData = data["STPT"]?.Where (k => k.KeyName.StartsWith ("target_"));
            var threatPointData = data["STPT"]?.Where (k => k.KeyName.StartsWith ("ppt_"));

            var cartridge = new DataCartridge {
                TargetSteerpoints = BuildSteerPoints (targetPointData),
                WeaponTargetPoints = BuildSteerPoints (weaponPointData),
                ThreatSteerpoints = BuildSteerPoints (threatPointData)
            };

            return cartridge;
        }

        private List<Steerpoint> BuildSteerPoints (IEnumerable<KeyData> keys) {
            if (keys == null) {
                return Enumerable.Empty<Steerpoint> ().ToList ();
            }

            var pointKeys = keys
                .Select (data => new {
                    Match = SteerpointNumberRegex.Match (data.KeyName),
                    Value = data.Value
                })
                .Where (key => key.Match.Success)
                .Select (
                    m => {
                        int idx;
                        if (Int32.TryParse (m.Match.Groups[1].Value, out idx)) {
                            return new {
                                Index = idx + 1,
                                Value = m.Value
                            };
                        }

                        return null;
                    })
                .Where (p => p != null)
                .OrderBy (p => p.Index);

            return pointKeys.Select (key => Steerpoint.Parse (key.Index, key.Value)).Where (p => !p.IsEmpty ()).ToList ();
        }
    }
}
