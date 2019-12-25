// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;

namespace Microsoft.Docs.Build
{
    internal class OpsMetadataRule
    {
        // Base fields
        public string Type { get; set; }

        public List<string> ContentTypes { get; set; }

        public string Severity { get; set; }

        public string Code { get; set; }

        public string AdditionalErrorMessage { get; set; }

        public bool Disabled { get; set; }

        // DateformatRule
        public string Format { get; set; }

        // DateRangeRule
        public TimeSpan? RelativeMin { get; set; }

        public TimeSpan? RelativeMax { get; set; }

        // DeprecatedRule
        public string ReplacedBy { get; set; }

        // EitherRule or PrecludesRule or RequiresRule
        public string Name { get; set; }

        // KindRule
        public bool? MultipleValues { get; set; }

        // ListRule
        public string List { get; set; }

        // MatchRule
        public string Value { get; set; }

        // MicrosoftAliasRule
        public string AllowedDLs { get; set; }
    }
}
