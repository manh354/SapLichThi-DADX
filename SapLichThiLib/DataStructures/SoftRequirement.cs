using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SapLichThiLib.DataStructures
{
    public enum Shift
    {
        shift1, shift2, shift3, shift4, shift5
    }
    public static class ext
    {
        public static string MakeString(this Shift shift)
        {
            switch (shift)
            {
                case Shift.shift1:
                    return "Kíp 1";
                case Shift.shift2:
                    return "Kíp 2";
                case Shift.shift3:
                    return "Kíp 3";
                case Shift.shift4:
                    return "Kíp 4";
                case Shift.shift5:
                    return "Kíp 5";
                default:
                    return "VÔ LÝ";
            } 
        }
    }
    public struct SoftRequirement
    {
        int StartDay { get; set; }
        int EndDay { get; set; }

        Shift Shift { get; set; }
    }
}
