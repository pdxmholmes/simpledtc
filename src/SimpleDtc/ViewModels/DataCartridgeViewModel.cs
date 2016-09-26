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

using System.Collections.Generic;
using System.Linq;
using Microsoft.Practices.Prism.PubSubEvents;
using SimpleDtc.Core;
using SimpleDtc.Core.Data;
using SimpleDtc.Core.Services;

namespace SimpleDtc.ViewModels {
    public interface IDataCartridgeViewModel {
        string SelectedProfile { get; set; }
        IEnumerable<string> AvailableProfiles { get; }
    }

    internal class PointType {
        public string Header { get; set; }
        public SteerpointType Type { get; set; }
    }

    internal class DataCartridgeViewModel : PropertyStateBase, IDataCartridgeViewModel {
        private static readonly PointType[] _PointTypes = {
            new PointType { Header = "Target Steerpoints", Type = SteerpointType.Target },
            new PointType { Header = "Threat Steerpoints", Type = SteerpointType.Threat },
            new PointType { Header = "Weapon Target Points", Type = SteerpointType.Weapon }
        };

        private readonly IFalconService _falconService;
        private readonly IOptionsService _optionsService;

        private List<string> _availableProfiles;
        private DataCartridge _loadedCartridge;
        private IEnumerable<Steerpoint> _selectedPoints;
        private SteerpointType _selectedPointType;
        private string _selectedProfile;

        public DataCartridgeViewModel (IEventAggregator eventAggregator, IOptionsService optionsService, IFalconService falconService) {
            _optionsService = optionsService;
            _falconService = falconService;

            eventAggregator.GetEvent<OptionsUpdated> ().Subscribe (OnOptionsUpdated);
            _availableProfiles = new List<string> (falconService.AvailableProfiles);

            WhenStateUpdated (() => SelectedProfile, OnProfileUpdated);
            WhenStateUpdated (() => SelectedPointType, OnSelectedPointTypeUpdated);

            SelectedProfile = optionsService.Get ().SelectedProfile;
            SelectedPointType = SteerpointType.Target;
        }

        public IEnumerable<PointType> PointTypes => _PointTypes;

        public SteerpointType SelectedPointType {
            get { return _selectedPointType; }
            set { SetProperty (ref _selectedPointType, value); }
        }

        public IEnumerable<Steerpoint> SelectedPoints {
            get { return _selectedPoints; }
            set { SetProperty (ref _selectedPoints, value); }
        }

        public string SelectedProfile {
            get { return _selectedProfile; }
            set { SetProperty (ref _selectedProfile, value); }
        }

        public IEnumerable<string> AvailableProfiles {
            get { return _availableProfiles; }
            set { SetProperty (ref _availableProfiles, value?.ToList ()); }
        }

        private void OnProfileUpdated () {
            var options = _optionsService.Get ();
            options.SelectedProfile = SelectedProfile;
            _optionsService.Update (options);

            _loadedCartridge = _falconService.LoadDtc (SelectedProfile);
            OnSelectedPointTypeUpdated ();
        }

        private void OnOptionsUpdated (OptionsChangedEventArgs args) {
            if (!args.HasFalconPathChanged) {
                return;
            }

            AvailableProfiles = new List<string> (_falconService.AvailableProfiles);
            SelectedProfile = _availableProfiles?.FirstOrDefault ();
        }

        private void OnSelectedPointTypeUpdated () {
            switch (SelectedPointType) {
                case SteerpointType.Target:
                    SelectedPoints = _loadedCartridge?.TargetSteerpoints;
                    break;

                case SteerpointType.Threat:
                    SelectedPoints = _loadedCartridge?.ThreatSteerpoints;
                    break;

                case SteerpointType.Weapon:
                    SelectedPoints = _loadedCartridge?.WeaponTargetPoints;
                    break;
            }
        }
    }
}
