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
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using NLog;
using SimpleDtc.Core.Data;

namespace SimpleDtc.Core.Services {
    public interface IPackagesService {
        IEnumerable<string> GetFolders ();
        IEnumerable<string> GetPackages (string folder);
        TargetPackage GetPackage (string folder, string name);
        TargetPackage Import (string json);
    }

    internal class PackagesService : IPackagesService {
        private static readonly string _PackagesRootPath = Path.Combine (Environment.GetFolderPath (Environment.SpecialFolder.MyDocuments), @"SimpleDtc\Target Packages");
        private readonly IDirectoryService _directoryService;
        private readonly ILogger _logger;
        private readonly string _rootPath;
        private readonly object _writeLock = new object ();

        public PackagesService (IDirectoryService directoryService, ILogger logger)
            : this (null, directoryService, logger) {
        }

        internal PackagesService (string rootPath, IDirectoryService directoryService, ILogger logger) {
            _rootPath = rootPath ?? _PackagesRootPath;
            _directoryService = directoryService;
            _logger = logger;

            directoryService.EnsureFolderExists (_rootPath);
        }

        public IEnumerable<string> GetFolders () {
            return Directory.GetDirectories (_rootPath)
                            .Where (d => !d.StartsWith (".") && !d.StartsWith ("_"))
                            .Select (Path.GetFileName)
                            .ToArray ();
        }

        public IEnumerable<string> GetPackages (string folder) {
            var path = Path.Combine (_rootPath, folder);
            return !Directory.Exists (path)
                ? Enumerable.Empty<string> ()
                : Directory.GetFiles (path, "*.tpkg").Select (Path.GetFileNameWithoutExtension).ToArray ();
        }

        public TargetPackage GetPackage (string folder, string name) {
            try {
                var path = Path.Combine (_rootPath, $"{folder}\\{name}.tpkg");
                return JsonConvert.DeserializeObject<TargetPackage> (File.ReadAllText (path), new JsonSerializerSettings {
                    ContractResolver = new CamelCasePropertyNamesContractResolver ()
                });
            }
            catch (Exception ex) {
                _logger.Error (ex, "Could not load target package");
                return null;
            }
        }

        public TargetPackage Import (string json) {
            try {
                var package = JsonConvert.DeserializeObject<TargetPackage> (json, new JsonSerializerSettings {
                    ContractResolver = new CamelCasePropertyNamesContractResolver ()
                });

                var path = Path.Combine (_rootPath, package.Campaign.StripInvalidPathCharacters ());
                _directoryService.EnsureFolderExists (path);

                var file = $"{package.Name.StripInvalidPathCharacters ()}.tpkg";
                lock (_writeLock) {
                    File.WriteAllText (Path.Combine (path, file), JsonConvert.SerializeObject (package, Formatting.Indented, new JsonSerializerSettings {
                        ContractResolver = new CamelCasePropertyNamesContractResolver ()
                    }));
                }

                return package;
            }
            catch (Exception ex) {
                _logger.Error (ex, "Could not import target package");
                return null;
            }
        }
    }
}
