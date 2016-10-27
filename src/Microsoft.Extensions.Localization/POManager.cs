﻿// Copyright (c) .NET Foundation. All rights reserved. 
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information. 

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;

namespace Microsoft.Extensions.Localization
{
    public class POManager
    {
        private readonly string _baseName;
        private readonly Assembly _assembly;
        private readonly string _resourcesRelativePath;

        private static readonly POParser POParser = new POParser();

        public POManager(string baseName, string location, string resourcesRelativePath)
            : this(baseName, Assembly.Load(new AssemblyName(location)), resourcesRelativePath)
        {
        }

        public POManager(Type resourceSource, string resourcesRelativePath)
            : this(resourceSource.Name, resourceSource.GetTypeInfo().Assembly, resourcesRelativePath)
        {
        }

        private POManager(string baseName, Assembly assembly, string resourcesRelativePath)
        {
            _baseName = baseName;
            _assembly = assembly;
            _resourcesRelativePath = resourcesRelativePath;
        }

        public string GetString(string name)
        {
            return GetString(name, CultureInfo.CurrentUICulture);
        }

        public string GetString(string name, CultureInfo culture)
        {
            var poResult = GetPOResults(culture)[name];
            return string.IsNullOrEmpty(poResult.Translation) ? poResult.Origional : poResult.Translation;
        }

        private IDictionary<string, POEntry> GetPOResults(CultureInfo culture)
        {
            var text = GetPOText(culture);
            var translations = ParsePOFile(text);

            return translations;
        }

        private IDictionary<string, POEntry> ParsePOFile(string poText)
        {
            var translations = new Dictionary<string, POEntry>(StringComparer.OrdinalIgnoreCase);

            POParser.ParseLocalizationStream(poText, translations, true);

            return translations;
        }

        private string GetPOText(CultureInfo culture)
        {
            using (var stream = _assembly.GetManifestResourceStream(GetResourceName(culture)))
            {
                if (stream == null)
                {
                    throw new NotImplementedException("Do something smart!");
                }
                using (var reader = new StreamReader(stream))
                {
                    return reader.ReadToEnd();
                }
            }
        }

        private string GetResourceName(CultureInfo culture)
        {
            var baseNamespace = _assembly.GetName().Name;
            return $"{GetResourcePrefix(_baseName, baseNamespace, _resourcesRelativePath)}.{culture.Name}.po";
        }

        private string GetResourcePrefix(string resourceName, string baseNamespace, string resourcesRelativePath)
        {
            return string.IsNullOrEmpty(resourcesRelativePath)
                ? _baseName
                : baseNamespace + "." + resourcesRelativePath + "." + TrimPrefix(resourceName, baseNamespace + ".");
        }

        private static string TrimPrefix(string name, string prefix)
        {
            if (name.StartsWith(prefix, StringComparison.Ordinal))
            {
                return name.Substring(prefix.Length);
            }

            return name;
        }
    }
}
