﻿using System;
using System.Collections.Generic;
using Couchbase.Configuration.Client;
using Microsoft.Extensions.Configuration;
using System.Linq;

namespace Tester
{
    /// <summary>
    /// Provides the configurations defined in app.config.
    /// </summary>
    public static class TestConfiguration
    {
        private static IConfigurationRoot _jsonConfiguration;
        private static TestSettings _settings;

        public static TestSettings Settings
        {
            get
            {
                if (_settings == null)
                {
                    EnsureConfigurationLoaded();

                    _settings = new TestSettings();
                    _jsonConfiguration.GetSection("testSettings").Bind(_settings);
                }

                return _settings;
            }
        }

        public static ClientConfiguration GetDefaultConfiguration()
        {
            return new ClientConfiguration
            {
                Servers = new List<Uri>
                {
                    BuildBootStrapUrl()
                }
            };
        }

        /// <summary>
        /// Gets the configuration for the "current" appSettings setting. The hostname and port will be pulled from the appsettings as well.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="ConfigurationErrorsException">A configuration file could not be loaded.</exception>
        public static ClientConfiguration GetCurrentConfiguration()
        {
            return GetConfiguration(Settings.Current);
        }

        /// <summary>
        /// Gets the configuration for a config section. The hostname and port will be pulled from the appsettings.
        /// </summary>
        /// <param name="sectionName"></param>
        /// <returns></returns>
        /// <exception cref="ConfigurationErrorsException">A configuration file could not be loaded.</exception>
        public static ClientConfiguration GetConfiguration(string sectionName)
        {
            EnsureConfigurationLoaded();

            var definition = new CouchbaseClientDefinition();
            _jsonConfiguration.GetSection("couchbase:" + sectionName).Bind(definition);

            var configuration = new ClientConfiguration(definition)
            {
                Servers = new List<Uri>
                {
                    BuildBootStrapUrl()
                }
            };

            return configuration;
        }

        public static Uri BuildBootStrapUrl()
        {
            EnsureConfigurationLoaded();

            return new Uri(string.Format("http://{0}:{1}/", Settings.Hostname, Settings.BootPort));
        }

        private static void EnsureConfigurationLoaded()
        {
            if (_jsonConfiguration == null)
            {
                var builder = new ConfigurationBuilder();
                builder.AddJsonFile("config.json");
                _jsonConfiguration = builder.Build();
            }
        }
    }
}

#region [ License information          ]

/* ************************************************************
 *
 *    @author Couchbase <info@couchbase.com>
 *    @copyright 2015 Couchbase, Inc.
 *
 *    Licensed under the Apache License, Version 2.0 (the "License");
 *    you may not use this file except in compliance with the License.
 *    You may obtain a copy of the License at
 *
 *        http://www.apache.org/licenses/LICENSE-2.0
 *
 *    Unless required by applicable law or agreed to in writing, software
 *    distributed under the License is distributed on an "AS IS" BASIS,
 *    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 *    See the License for the specific language governing permissions and
 *    limitations under the License.
 *
 * ************************************************************/

#endregion
