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
using System.Windows;
using Microsoft.Practices.Prism.PubSubEvents;
using Microsoft.WindowsAPICodePack.Dialogs;
using SimpleDtc.Core;
using SimpleDtc.Core.Services;

namespace SimpleDtc.ViewModels {
    public interface IMainWindowViewModel {
        string Status { get; }
        void ValidateConfiguration ();
    }

    internal class MainWindowViewModel : PropertyStateBase, IMainWindowViewModel {
        private readonly IOptionsService _optionsService;
        private string _status;

        public MainWindowViewModel (IEventAggregator eventAggregator, IOptionsService optionsService) {            
            _optionsService = optionsService;
            Status = String.Empty;

            eventAggregator.GetEvent<StatusUpdate> ().Subscribe (m => Status = $"{DateTime.Now.ToShortTimeString()}: {m}");
        }

        public string Status {
            get { return _status; }
            set { SetProperty (ref _status, value); }
        }

        public void ValidateConfiguration () {
            var options = _optionsService.Get ();
            if (!String.IsNullOrEmpty (options.FalconRoot) && Directory.Exists (options.FalconRoot)) {
                return;
            }

            var result = MessageBox.Show ("SimpleDtc could not automatically detect where you have Falcon BMS installed. Please select your Falcon BMS install location in the following dialog.", "Auto-Detect Failed", MessageBoxButton.OKCancel);
            if (result != MessageBoxResult.OK) {
                Application.Current.Shutdown ();
                return;
            }

            var dialog = new CommonOpenFileDialog {
                IsFolderPicker = true
            };

            var folderResult = dialog.ShowDialog ();
            if (folderResult != CommonFileDialogResult.Ok) {
                Application.Current.Shutdown();
                return;
            }

            options.FalconRoot = dialog.FileName;
            _optionsService.Update (options);
        }
    }
}
