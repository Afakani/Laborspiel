using System;
using UnityEngine;

namespace PlanErfuellt
{
    /// <summary>
    /// Labor-Layout (GDD 3) in Prototyp-Koordinaten. Umrechnung nach Unity: x bleibt, z_unity = -z_proto.
    /// </summary>
    [Serializable]
    public class LayoutConfig
    {
        public RoomLayout room = new RoomLayout();
        public AreaLayout playerArea = new AreaLayout();
        public PlayerStartLayout playerStart = new PlayerStartLayout();
        public ChamberLayout chamber = new ChamberLayout();
        public BeltLayout belt = new BeltLayout();
        public ZoneLayout pultZone = new ZoneLayout();
        public ZoneLayout clothZone = new ZoneLayout();
        public WindowLayout window = new WindowLayout();
        public StationLayout[] stations = new StationLayout[0];

        /// <summary>Prototyp-Koordinate nach Unity umrechnen.</summary>
        public static Vector3 ToUnity(float x, float y, float zProto) => new Vector3(x, y, -zProto);

        /// <summary>Station nach Id, sonst null.</summary>
        public StationLayout Station(string id)
        {
            foreach (var s in stations) if (s.id == id) return s;
            return null;
        }
    }
}
