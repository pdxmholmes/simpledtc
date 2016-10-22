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
using System.Linq;

namespace SimpleDtc.Core.Data {
    public enum SteerpointType {
        Target,
        Threat,
        Weapon
    }

    public class Steerpoint {
        public const string OutputFormat = "0.000000";
        public const string EmptyNote = "Not set";
        public const double EmptyValue = -1.0;

        public int Index { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }
        public double Unknown { get; set; }
        public string Note { get; set; }

        public bool IsEmpty () {
            return (X == 0.0 && Y == 0.0 && Z == 0.0) || Note == EmptyNote;
        }

        public static Steerpoint Parse (int index, string data) {
            var point = new Steerpoint {
                Index = index
            };

            var parts = data.Split (',').Select (s => s.Trim ()).ToArray ();
            for (var i = 0; i < Math.Min (parts.Length, 5); i++) {
                switch (i) {
                    case 0:
                        point.X = GetCoord (parts[i]);
                        break;
                    case 1:
                        point.Y = GetCoord (parts[i]);
                        break;
                    case 2:
                        point.Z = GetCoord (parts[i]);
                        break;
                    case 3:
                        point.Unknown = GetCoord (parts[i]);
                        break;
                    default:
                        point.Note = parts[i];
                        break;
                }
            }

            return point;
        }

        public override string ToString () {
            return $"{FormatCoordinate (X)}, {FormatCoordinate (Y)}, {FormatCoordinate (Z)}, {FormatCoordinate (Unknown)}, {Note}";
        }

        private static string FormatCoordinate (double c) {
            return c == EmptyValue ? "-1" : c.ToString (OutputFormat);
        }

        private static double GetCoord (string part) {
            double coord;
            return Double.TryParse (part, out coord) ? coord : 0.0;
        }
    }
}
