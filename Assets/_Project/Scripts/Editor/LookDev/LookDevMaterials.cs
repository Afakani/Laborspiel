using UnityEngine;

namespace PlanErfuellt.EditorTools
{
    /// <summary>Materialien des Look-Passes (Bild 07/08): abgenutzter Stahl, Messing, Lack, Beton, Glas, Leuchtteile.</summary>
    public static class LookDevMaterials
    {
        public static Material SteelPanel, SteelDark, SteelFrame, Brass, GreenPaint, RedPaint, WhitePaint, FloorConcrete, FloorPlate,
            Grate, WallConcrete, WallPanel, Hazard, Paper, Checklist, Director, PlanBoard, EmblemCream, EmblemGold, IconLeaf, IconDrop,
            IconMolecule, GlassClear, GlassTank, CoverRed, HoseGreen, HoseBlue, HoseYellow, BarGreen, BarBlue, BarYellow, LightConeGreen,
            FogGreen, HazeWarm, RailYellow, Rubber, LampBulb, ScreenBlue, CreamPlate, BannerRed;

        public static void Init()
        {
            SteelPanel = LookDevKit.Pbr("LD_SteelPanel", "rusty_metal_sheet", new Color(0.72f, 0.70f, 0.66f), 1.4f, 0.9f);
            SteelDark = LookDevKit.Pbr("LD_SteelDark", "rusty_metal_sheet", new Color(0.30f, 0.29f, 0.28f), 1.0f, 1.0f);
            SteelFrame = LookDevKit.Pbr("LD_SteelFrame", "rusty_metal_02", new Color(0.42f, 0.41f, 0.40f), 1.6f, 1.1f);
            Brass = LookDevKit.Pbr("LD_Brass", "rusty_metal_03", new Color(1.0f, 0.82f, 0.50f), 0.8f, 1.3f, 0.6f);
            GreenPaint = LookDevKit.Pbr("LD_GreenPaint", "green_metal_rust", new Color(0.75f, 0.80f, 0.72f), 1.5f, 0.9f);
            RedPaint = LookDevKit.Pbr("LD_RedPaint", "rusty_painted_metal", new Color(0.95f, 0.75f, 0.72f), 1.5f, 0.8f);
            WhitePaint = LookDevKit.Pbr("LD_WhitePaint", "rusty_metal_02", new Color(1f, 0.97f, 0.9f), 1.0f, 0.8f);
            FloorConcrete = LookDevKit.Pbr("LD_FloorConcrete", "concrete_floor_worn_001", new Color(0.62f, 0.58f, 0.52f), 3.0f, 1.2f);
            FloorPlate = LookDevKit.Pbr("LD_FloorPlate", "metal_plate", new Color(0.8f, 0.78f, 0.75f), 1.2f, 1.1f);
            Grate = LookDevKit.Pbr("LD_Grate", "metal_grate_rusty", new Color(0.55f, 0.5f, 0.45f), 1.0f, 1.0f);
            WallConcrete = LookDevKit.Pbr("LD_WallConcrete", "concrete_wall_006", new Color(0.62f, 0.56f, 0.50f), 3.5f, 0.8f);
            WallPanel = LookDevKit.Pbr("LD_WallPanel", "painted_metal_shutter", new Color(0.42f, 0.40f, 0.36f), 2.0f, 0.8f);
            Hazard = LookDevKit.Textured("LD_Hazard", "Hazard.png", Color.white, 0.6f, 0.35f);
            Paper = LookDevKit.Textured("LD_Paper", "Paper.png", Color.white, 0f, 0.1f);
            Checklist = LookDevKit.Textured("LD_Checklist", "Checklist.png", Color.white, 0f, 0.15f, 0.15f);
            Director = LookDevKit.Textured("LD_Director", "Director.png", Color.white, 0f, 0.6f, 1.6f);
            PlanBoard = LookDevKit.Textured("LD_PlanBoard", "PlanBoard.png", Color.white, 0f, 0.3f, 0.35f);
            EmblemCream = LookDevKit.Textured("LD_EmblemCream", "EmblemCream.png", Color.white, 0f, 0.2f, 0f, true);
            EmblemGold = LookDevKit.Textured("LD_EmblemGold", "EmblemGold.png", Color.white, 0f, 0.6f, 0f, true);
            IconLeaf = LookDevKit.Textured("LD_IconLeaf", "IconLeaf.png", Color.white, 0f, 0.2f, 0f, true);
            IconDrop = LookDevKit.Textured("LD_IconDrop", "IconDrop.png", Color.white, 0f, 0.2f, 0f, true);
            IconMolecule = LookDevKit.Textured("LD_IconMolecule", "IconMolecule.png", Color.white, 0f, 0.2f, 0f, true);
            GlassClear = LookDevKit.Glass("LD_GlassChamber", new Color(0.78f, 0.95f, 0.88f, 0.10f), 0.97f);
            GlassTank = LookDevKit.Glass("LD_GlassTank", new Color(0.9f, 0.97f, 1f, 0.16f), 0.95f);
            CoverRed = LookDevKit.Glass("LD_CoverRed", new Color(1f, 0.15f, 0.1f, 0.35f), 0.9f);
            HoseGreen = BlockoutKit.Mat("LD_HoseGreen", LabMaterials.Hex("#2F8A3A"), 0f, 0.55f);
            HoseBlue = BlockoutKit.Mat("LD_HoseBlue", LabMaterials.Hex("#1F6FC2"), 0f, 0.55f);
            HoseYellow = BlockoutKit.Mat("LD_HoseYellow", LabMaterials.Hex("#D9A514"), 0f, 0.55f);
            BarGreen = BlockoutKit.Emissive("LD_BarGreen", LabMaterials.Hex("#46D14F"), 3.0f, false);
            BarBlue = BlockoutKit.Emissive("LD_BarBlue", LabMaterials.Hex("#3AA0FF"), 3.0f, false);
            BarYellow = BlockoutKit.Emissive("LD_BarYellow", LabMaterials.Hex("#FFCC2A"), 3.0f, false);
            LightConeGreen = LookDevKit.Shaft("LD_LightShaftGreen", new Color(0.35f, 1f, 0.3f), 0.35f, 1.4f);
            FogGreen = LookDevKit.Additive("LD_FogGreen", "SoftSmoke.png", new Color(0.4f, 1f, 0.35f, 0.35f), false);
            HazeWarm = LookDevKit.Additive("LD_HazeWarm", "SoftSmoke.png", new Color(1f, 0.75f, 0.45f, 0.12f), false);
            RailYellow = BlockoutKit.Mat("LD_RailYellow", LabMaterials.Hex("#C9961A"), 0.3f, 0.45f);
            Rubber = BlockoutKit.Mat("LD_Rubber", LabMaterials.Hex("#1E1C1A"), 0f, 0.35f);
            LampBulb = BlockoutKit.Emissive("LD_LampBulb", LabMaterials.Hex("#FFC870"), 6f, true);
            ScreenBlue = BlockoutKit.Emissive("LD_ScreenBlue", LabMaterials.Hex("#1F3E8A"), 1.2f, false);
            CreamPlate = BlockoutKit.Mat("LD_CreamPlate", LabMaterials.Hex("#E9DCBC"), 0f, 0.2f);
            BannerRed = LookDevKit.Pbr("LD_BannerRed", "rusty_painted_metal", new Color(0.85f, 0.25f, 0.22f), 3f, 0.3f, 0.3f);
        }
    }
}
