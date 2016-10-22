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
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using SimpleDtc.Core;
using SimpleDtc.Core.Services;

namespace SimpleDtc.ViewModels {
    public interface ITargetPackagesView {
        void ShowExportMenu ();
    }

    public interface ITargetPackagesViewModel {
        ITargetPackagesView View { get; set; }
        object SelectedItem { get; set; }
        bool IsExportMenuOpen { get; set; }
        ICommand ExportTargetPackage { get; }
    }

    public interface IPackageNode {
        string Name { get; }
    }

    public class PackageFolderNode : IPackageNode {
        public IEnumerable<PackageNode> Packages { get; internal set; }
        public string Name { get; internal set; }
    }

    public class PackageNode : IPackageNode {
        public string Name { get; internal set; }
    }

    internal class TargetPackagesViewModel : PropertyStateBase, ITargetPackagesViewModel {
        private readonly ObservableCollection<PackageFolderNode> _packageFolders;
        private IFalconService _falconService;
        private bool _isExportMenuOpen;
        private object _selectedItem;

        public TargetPackagesViewModel (IFalconService falconService, IPackagesService packagesService) {
            _falconService = falconService;

            _packageFolders = new ObservableCollection<PackageFolderNode> (
                packagesService.GetFolders ().Select (f => new PackageFolderNode {
                    Name = f,
                    Packages = new ObservableCollection<PackageNode> (packagesService.GetPackages (f).Select (p => new PackageNode {
                        Name = p
                    }))
                }));
        }

        public object SelectedItem {
            get { return _selectedItem; }
            set { SetProperty (ref _selectedItem, value); }
        }

        public IEnumerable<PackageFolderNode> PackageFolders => _packageFolders;
        public ITargetPackagesView View { get; set; }
        public ICommand ExportTargetPackage => GetCommand ("ExportTargetPackage", ExecuteExportTargetPackage);

        public bool IsExportMenuOpen {
            get { return _isExportMenuOpen; }
            set { SetProperty (ref _isExportMenuOpen, value); }
        }

        private void ExecuteExportTargetPackage () {
            IsExportMenuOpen = !IsExportMenuOpen;
        }
    }
}
