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
using System.IO;
using System.Text;
using Microsoft.Practices.Prism.PubSubEvents;
using Microsoft.Win32;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using NLog;
using SimpleDtc.Core.Data;

namespace SimpleDtc.Core.Services {
    public interface IOptionsService {
        Options Get ();
        void Update (Options options);
    }

    internal class OptionsService : IOptionsService {
        private static readonly string _OptionsRootPath = Path.Combine (Environment.GetFolderPath (Environment.SpecialFolder.ApplicationData), "SimpleDtc");
        private readonly IEventAggregator _eventAggregator;
        private readonly ILogger _logger;
        private readonly string _optionsPath;
        private readonly object _writeLock = new object ();
        private Options _options;

        public OptionsService (IEventAggregator eventAggregator, IDirectoryService directoryService, ILogger logger) {
            _eventAggregator = eventAggregator;
            _logger = logger;

            directoryService.EnsureFolderExists (_OptionsRootPath);
            _optionsPath = Path.Combine (_OptionsRootPath, "options.json");
        }

        public Options Get () {
            if (_options != null) {
                return (Options) _options.Clone ();
            }

            if (!File.Exists (_optionsPath)) {
                _options = MakeDefault ();
                Update (_options);
            }

            try {
                _options = JsonConvert.DeserializeObject<Options> (File.ReadAllText (_optionsPath, Encoding.UTF8), new JsonSerializerSettings {
                    ContractResolver = new CamelCasePropertyNamesContractResolver ()
                });
            }
            catch (Exception ex) {
                _logger.Error (ex, "Could not load options");
                _options = MakeDefault ();
                Update (_options);
            }

            return (Options) _options.Clone ();
        }

        public void Update (Options options) {
            bool falconPathChanged;
            lock (_writeLock) {
                falconPathChanged = !String.Equals (_options?.FalconRoot, options?.FalconRoot, StringComparison.InvariantCultureIgnoreCase);

                _options = options;
                var json = JsonConvert.SerializeObject (_options, Formatting.Indented, new JsonSerializerSettings {
                    ContractResolver = new CamelCasePropertyNamesContractResolver ()
                });
                File.WriteAllText (_optionsPath, json);
            }

            _eventAggregator.GetEvent<OptionsUpdated> ().Publish (new OptionsChangedEventArgs {
                HasFalconPathChanged = falconPathChanged,
                Options = _options
            });
        }

        private static Options MakeDefault () {
            return new Options {
                FalconRoot = FindFalconRoot ()
            };
        }

        private static string FindFalconRoot () {
            var key = Registry.CurrentUser.OpenSubKey (@"SOFTWARE\F4Patch\Settings");
            var path = key?.GetValue ("F4Exe");
            return Path.GetDirectoryName (path?.ToString ());
        }
    }
}
