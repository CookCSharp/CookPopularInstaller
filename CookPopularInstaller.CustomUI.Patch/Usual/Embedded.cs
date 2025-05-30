/*
 * Description: Embedded 
 * Author: Chance.Zheng
 * Create Time: 2024/7/17 11:38:01
 * .Net Version: 4.8
 * CLR Version: 4.0.30319.42000
 * Copyright (c) NCATest 2020-2024 All Rights Reserved.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CookPopularInstaller.CustomUI.Patch
{
    public class Embedded
    {
        public IList<string> DeleteFiles { get; set; }

        public string UpgradeCode { get; set; }

        public string PatchVersion { get; set; }
    }
}
