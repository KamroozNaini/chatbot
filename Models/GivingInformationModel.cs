// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;

namespace Microsoft.BotBuilderSamples
{
    public class GivingInformationModel
    {
        public string Name { get; set; }

        public string EmployeeCode { get; set; }

        public Boolean IsValidated { get; set; }

        public string ActivityName { get; set; }

        public DateTime StartFrom { get; set; }

        public string Duration { get; set; }

        public string Confirmed { get; set; }
    }
}
