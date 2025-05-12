/*
 * Description: ProcessComparer 
 * Author: Chance.Zheng
 * Create Time: 2024/7/2 18:11:57
 * .Net Version: 4.8
 * CLR Version: 4.0.30319.42000
 * Copyright (c) NCATest 2020-2024 All Rights Reserved.
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NCATestInstaller.CustomUI
{
    public class ProcessComparer : IEqualityComparer<Process>
    {
        public bool Equals(Process x, Process y)
        {
            if (ReferenceEquals(x, y)) return true;

            if (x == null || y == null) return false;

            return x.Id == y.Id;
        }

        public int GetHashCode(Process obj)
        {
            return obj.Id.GetHashCode();
        }
    }
}
