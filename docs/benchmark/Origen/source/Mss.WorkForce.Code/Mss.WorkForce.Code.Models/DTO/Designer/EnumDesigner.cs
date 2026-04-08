namespace Mss.WorkForce.Code.Models.DTO.Designer
{
    public enum ZoneType
    {
        Unknown = 0,
        Dock = 1,
        Aisle = 2,
        Buffer = 3,
        Shelf = 4,
        Rack = 5,
        DriveIn = 6,
        ChaoticStorage = 7,
        AutomaticStorage = 8,
        Stage = 9,
    }

    public class ZoneTypeGetMethods
    {
        public static ZoneType GetStringZoneType(string zoneType)
        {
            switch (zoneType)
            {
                case "Dock":
                    return ZoneType.Dock;
                case "Aisle":
                    return ZoneType.Aisle;
                case "Buffer":
                    return ZoneType.Buffer;
                case "Shelf":
                    return ZoneType.Shelf;
                case "Rack":
                    return ZoneType.Rack;
                case "DriveIn":
                    return ZoneType.DriveIn;
                case "ChaoticStorage":
                    return ZoneType.ChaoticStorage;
                case "AutomaticStorage":
                    return ZoneType.AutomaticStorage;
                case "Stage":
                    return ZoneType.Stage;
                default:
                    throw new ArgumentException(string.Format("GetStringZoneType('{0}')", zoneType));
            }
        }
        public static string GetZoneTypeString(ZoneType zoneType)
        {
            switch (zoneType)
            {
                case ZoneType.Dock:
                    return "Dock";
                case ZoneType.Aisle:
                    return "Aisle";
                case ZoneType.Buffer:
                    return "Buffer";
                case ZoneType.Shelf:
                    return "Shelf";
                case ZoneType.Rack:
                    return "Rack";
                case ZoneType.DriveIn:
                    return "DriveIn";
                case ZoneType.ChaoticStorage:
                    return "Chaotic Zone";
                case ZoneType.AutomaticStorage:
                    return "Automatic";
                case ZoneType.Stage:
                    return "Stage";
                default:
                    throw new ArgumentException(string.Format("GetZoneTypeString('{0}')", zoneType));
            }
        }
    }
}
