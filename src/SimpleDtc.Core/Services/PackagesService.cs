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
using System.IO.Compression;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using NLog;
using SimpleDtc.Core.Data;

namespace SimpleDtc.Core.Services {
    public interface IPackagesService {
        IEnumerable<string> GetFolders ();
        IEnumerable<string> GetPackages (string folder);
        TargetPackage GetPackage (string folder, string name);
        TargetPackage Import (string data);
        string Export (TargetPackage package, bool compress);
    }

    internal class PackagesService : IPackagesService {
        private static readonly string _CompressedStringHeader = "tpkg:";
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

        public TargetPackage Import (string data) {
            try {
                if (String.IsNullOrWhiteSpace (data)) {
                    throw new ArgumentException ("data cannot be null or empty");
                }

                var json = Decompress (data);
                var package = JsonConvert.DeserializeObject<TargetPackage> (json, new JsonSerializerSettings {
                    ContractResolver = new CamelCasePropertyNamesContractResolver ()
                });

                if (package.Campaign == null) {
                    package.Campaign = "KTO";
                }

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

        public string Export (TargetPackage package, bool compress) {
            try {
                var json = JsonConvert.SerializeObject (package, compress ? Formatting.None : Formatting.Indented, new JsonSerializerSettings {
                    ContractResolver = new CamelCasePropertyNamesContractResolver ()
                });

                if (!compress) {
                    return json;
                }

                var buffer = Encoding.UTF8.GetBytes (json);
                var memoryStream = new MemoryStream ();
                using (var compressionStream = new GZipStream (memoryStream, CompressionMode.Compress, true)) {
                    compressionStream.Write (buffer, 0, buffer.Length);
                }

                memoryStream.Position = 0;
                var compressedData = new byte[memoryStream.Length];
                memoryStream.Read (compressedData, 0, compressedData.Length);

                var compressedBuffer = new byte[compressedData.Length + 4];
                Buffer.BlockCopy (compressedData, 0, compressedBuffer, 4, compressedData.Length);
                Buffer.BlockCopy (BitConverter.GetBytes (buffer.Length), 0, compressedBuffer, 0, 4);
                return $"{_CompressedStringHeader}{Convert.ToBase64String (compressedBuffer)}";
            }
            catch (Exception ex) {
                _logger.Error (ex, "Could not export target package");
                return String.Empty;
            }
        }

        private static string Decompress (string data) {
            if (!data.StartsWith (_CompressedStringHeader)) {
                return data;
            }

            data = data.Substring (_CompressedStringHeader.Length);
            var compressedBuffer = Convert.FromBase64String (data);
            using (var memoryStream = new MemoryStream ()) {
                var len = BitConverter.ToInt32 (compressedBuffer, 0);
                memoryStream.Write (compressedBuffer, 4, compressedBuffer.Length - 4);

                var buffer = new byte[len];
                memoryStream.Position = 0;

                using (var compressionStream = new GZipStream (memoryStream, CompressionMode.Decompress)) {
                    compressionStream.Read (buffer, 0, buffer.Length);
                }

                return Encoding.UTF8.GetString (buffer);
            }
        }
    }
}
