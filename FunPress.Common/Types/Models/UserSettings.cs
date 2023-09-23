using System;
using FunPress.Common.Types.Enums;

namespace FunPress.Common.Types.Models
{
    public class UserSettings : ICloneable
    {
        public string PrinterName { get; set; }
        public PrinterActionType PrinterActionType { get; set; }
        public string FolderPath { get; set; }
        
        public object Clone()
        {
            return MemberwiseClone();
        }
    }
}