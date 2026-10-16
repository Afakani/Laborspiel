using UnityEngine;

namespace PlanErfuellt
{
    /// <summary>Zeichnet das unsichtbare Baumodus-Raster (GDD 6.10) als Gizmo in der Szenenansicht.</summary>
    public class BuildGridGizmo : MonoBehaviour
    {
        [SerializeField] GameConfig config;

        void OnDrawGizmos()
        {
            if (config == null) return;
            var g = config.buildMachines.grid;
            for (int gx = 0; gx < g.cols; gx++)
            for (int gz = 0; gz < g.rows; gz++)
            {
                bool blocked = gz == g.blockedRow && gx >= g.blockedFrom && gx <= g.blockedTo;
                Gizmos.color = blocked ? new Color(1f, 0.2f, 0.2f, 0.5f) : new Color(0.25f, 1f, 0.5f, 0.5f);
                var c = LayoutConfig.ToUnity(g.x0 + g.cell * gx, 0.02f, g.z0 + g.cell * gz);
                Gizmos.DrawWireCube(c, new Vector3(g.cell * 0.95f, 0.02f, g.cell * 0.95f));
            }
        }

        public void SetConfig(GameConfig cfg) => config = cfg;
    }
}
