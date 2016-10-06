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
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using IniParser;
using IniParser.Model;
using Microsoft.Practices.Prism.PubSubEvents;
using SimpleDtc.Core.Data;

namespace SimpleDtc.Core.Services {
    public interface IFalconService {
        IEnumerable<string> AvailableProfiles { get; }
        DataCartridge LoadDtc (string profileName);
        void MergeTargetPackage (string profileName, TargetPackage package);
        void WatchDtc (string profileName);
        void StopWatching ();
    }

    internal class FalconService : IFalconService {
        private readonly IEventAggregator _eventAggregator;
        private readonly IOptionsService _optionsService;
        private IObservable<EventPattern<FileSystemEventArgs>> _observable;
        private FileSystemWatcher _watcher;

        public FalconService (IEventAggregator eventAggregator, IOptionsService optionsService) {
            _eventAggregator = eventAggregator;
            _optionsService = optionsService;
        }

        public IEnumerable<string> AvailableProfiles {
            get {
                var options = _optionsService.Get ();
                if (options.FalconRoot == null) {
                    return Enumerable.Empty<string> ();
                }

                var files = Directory.GetFiles (Path.Combine (options.FalconRoot, @"User\Config"), "*.plc");
                return files.Select (Path.GetFileNameWithoutExtension);
            }
        }

        public DataCartridge LoadDtc (string profileName) {
            var options = _optionsService.Get ();
            if (options.FalconRoot == null) {
                return null;
            }

            var path = Path.Combine (options.FalconRoot, $"User\\Config\\{profileName}.ini");
            return !File.Exists (path) ? null : new DataCartridgeParser ().Parse (path);
        }

        public void MergeTargetPackage (string profileName, TargetPackage package) {
            var options = _optionsService.Get ();
            if (options.FalconRoot == null) {
                return;
            }

            var path = Path.Combine (options.FalconRoot, $"User\\Config\\{profileName}.ini");
            var parser = new FileIniDataParser ();
            var data = parser.ReadFile (path);

            var newData = new KeyDataCollection ();
            foreach (var point in package.TargetSteerpoints) {
                newData[$"target_{point.Index}"] = point.ToString ();
            }

            foreach (var point in package.ThreatSteerpoints) {
                newData[$"ppt_{point.Index}"] = point.ToString ();
            }

            foreach (var point in package.WeaponTargetPoints) {
                newData[$"wpntarget_{point.Index - 1}"] = point.ToString ();
            }

            data["STPT"].Merge (newData);
            parser.WriteFile (path, data);
        }

        public void WatchDtc (string profileName) {
            if (String.IsNullOrWhiteSpace (profileName)) {
                return;
            }

            var options = _optionsService.Get ();
            if (options.FalconRoot == null) {
                return;
            }

            StopWatching ();
            _watcher = new FileSystemWatcher (Path.Combine (options.FalconRoot, @"User\Config"), $"{profileName}.ini") { NotifyFilter = NotifyFilters.LastWrite };

            _observable = Observable
                .FromEventPattern<FileSystemEventHandler, FileSystemEventArgs> (h => _watcher.Changed += h, h => _watcher.Changed -= h)
                .Throttle (TimeSpan.FromMilliseconds (500));

            _observable.Subscribe (ep => { _eventAggregator.GetEvent<ProfileUpdated> ().Publish (profileName); });
            _watcher.EnableRaisingEvents = true;
        }

        public void StopWatching () {
            if (_watcher != null) {
                _watcher.EnableRaisingEvents = false;
                _watcher.Dispose ();
            }

            _observable = null;
            _watcher = null;
        }
    }
}
