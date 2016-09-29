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

using NLog;
using NLog.Config;
using NLog.Targets;

namespace SimpleDtc.Core {
    public class Log {
        private const string LogLayout = "[${date:HH\\:MM\\:ss}] ${level:uppercase=true}: ${message}";

        public Log () {
            Initialize ();
        }

        private void Initialize () {
            var config = new LoggingConfiguration ();

            var consoleTarget = new ColoredConsoleTarget ();
            config.AddTarget ("console", consoleTarget);

            var fileTarget = new FileTarget ();
            config.AddTarget ("file", fileTarget);

            consoleTarget.Layout = LogLayout;
            fileTarget.FileName = @"${basedir}\${logger}.log";
            fileTarget.Layout = LogLayout;
            fileTarget.ArchiveAboveSize = 1024 * 1024;

#if DEBUG
            fileTarget.MaxArchiveFiles = 5;
#else
            fileTarget.MaxArchiveFiles = 1;
#endif

            var rule = new LoggingRule ("*", LogLevel.Debug, consoleTarget);
            config.LoggingRules.Add (rule);

            rule = new LoggingRule ("*", LogLevel.Debug, fileTarget);
            config.LoggingRules.Add (rule);

            LogManager.Configuration = config;
        }
    }
}
