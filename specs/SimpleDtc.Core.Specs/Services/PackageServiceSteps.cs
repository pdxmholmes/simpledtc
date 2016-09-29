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
using Moq;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using NLog;
using NUnit.Framework;
using SimpleDtc.Core.Data;
using SimpleDtc.Core.Services;
using TechTalk.SpecFlow;

namespace SimpleDtc.Core.Specs.Services {
    [Binding]
    public class PackageServiceSteps {
        private static string _path;
        private IEnumerable<string> _folders;
        private TargetPackage _package;
        private IEnumerable<string> _packages;
        private PackagesService _packagesService;

        [BeforeFeature ("packagesService")]
        public static void BeforeFeature () {
            _path = Path.Combine (Path.GetTempPath (), Guid.NewGuid ().ToString ());
        }

        [AfterFeature ("packagesService")]
        public static void AfterFeature () {
            Directory.Delete (_path, true);
        }

        [Given (@"a package service")]
        public void GiveAPackageService () {
            var logger = new Mock<ILogger> ();
            _packagesService = new PackagesService (_path, new DirectoryService (), logger.Object);
        }

        [Given (@"a folder called '(.*)'")]
        public void GivenAFolderCalled (string p0) {
            Directory.CreateDirectory (Path.Combine (_path, p0));
        }

        [When (@"folders are queried")]
        public void WhenFoldersAreQueried () {
            _folders = _packagesService.GetFolders ();
        }

        [Then (@"folders (.*)")]
        public void ThenFoldersContains (string[] folders) {
            Array.ForEach (folders, n => Assert.IsTrue (_folders.Any (f => String.Equals (n, f, StringComparison.InvariantCultureIgnoreCase))));
        }

        [Given (@"a target package '(.*)' in folder '(.*)'")]
        public void GivenATargetPackageInFolder (string p0, string p1) {
            var package = CreateTestPackage (p0);
            var path = Path.Combine (_path, $"{p1}\\{p0}.tpkg");
            File.WriteAllText (path, JsonConvert.SerializeObject (package, Formatting.Indented, new JsonSerializerSettings {
                ContractResolver = new CamelCasePropertyNamesContractResolver ()
            }));
        }

        [When (@"packages are queried for '(.*)'")]
        public void WhenPackagteNamesAsreQueried (string p0) {
            _packages = _packagesService.GetPackages (p0);
        }

        [Then (@"packages (.*)")]
        public void ThenPackageNamesContains (string[] names) {
            Array.ForEach (names, n => Assert.IsTrue (_packages.Any (f => String.Equals (n, f, StringComparison.InvariantCultureIgnoreCase))));
        }

        [When (@"package '(.*)' is queried from '(.*)'")]
        public void WhenPackageIsQueriedFrom (string p0, string p1) {
            _package = _packagesService.GetPackage (p1, p0);
        }

        [Then (@"a package is returned")]
        public void ThenAPackageIsReturned () {
            Assert.IsNotNull (_package);
        }

        [Then (@"package is null")]
        public void ThenPackageIsNull () {
            Assert.IsNull (_package);
        }


        [StepArgumentTransformation (@"contains '(.*)'")]
        public string[] CommaSeperatedList (string list) {
            return list.Split (',').Select (s => s.Trim ()).ToArray ();
        }

        private TargetPackage CreateTestPackage (string name) {
            return new TargetPackage {
                Name = name,
                Creator = "test_user",
                Campaign = "KTO",
                TargetSteerpoints = new List<Steerpoint> (),
                ThreatSteerpoints = new List<Steerpoint> (),
                WeaponTargetPoints = new List<Steerpoint> ()
            };
        }
    }
}
