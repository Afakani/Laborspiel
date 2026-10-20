using System;

namespace PlanErfuellt
{
    /// <summary>Funktionen der Nebenzimmer (Kantine, Umkleide, Schlafraum, Direktorbüro).</summary>
    [Serializable]
    public class SideRoomsConfig
    {
        public CanteenConfig canteen = new CanteenConfig();
        public LockerRoomConfig locker = new LockerRoomConfig();
        public DirectorDeskConfig desk = new DirectorDeskConfig();
    }
}
