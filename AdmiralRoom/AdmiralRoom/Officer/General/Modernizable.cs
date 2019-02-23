﻿namespace Huoyaoyuan.AdmiralRoom.Officer
{
    public struct Modernizable
    {
        public int Default { get; }
        public int Max { get; }
        public int Current => Default + Upgrated;
        public int Upgrated { get; }
        public int ShowValue { get; }
        public int Upward => Max - Current;
        public bool IsMax => Current >= Max;
        public Modernizable(LimitedValue masterdata, int upgradedata, int showdata)
        {
            Default = masterdata.Current;
            Max = masterdata.Max;
            Upgrated = upgradedata;
            ShowValue = showdata;
        }
        public override string ToString() => IsMax ? $"{Current} (Max)" : $"{Current} (+{Upward})";
    }
}
