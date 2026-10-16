using UnityEngine;

namespace PlanErfuellt.EditorTools
{
    /// <summary>Blockout-Materialien nach der Farbpalette der Art Direction (04).</summary>
    public static class LabMaterials
    {
        public static Material Concrete, WallUpper, WallLower, Ceiling, SteelDark, Steel, Brass, PropRed, PropRedBright, Cream,
            Glass, Gas, ChamberRing, LampWarm, TvScreen, PlanScreen, TermScreen, WindowSky, Hazard, Wood, Rubber, Crate,
            PultBody, PultTop, Barrel, MotorGreen, ProcessingMetal, CryoMetal, CryoWindow, CentrifugeWhite, SignPlate,
            StopButton, LampGreen, LampBlue, LampAmber, Hole, VatGreen, Cloth, Black, KnobA, KnobB, KnobC, LiquidA, LiquidB, LiquidC, Antidote;

        public static readonly Color SubstanceA = Hex("#46D14F");
        public static readonly Color SubstanceB = Hex("#3AA0FF");
        public static readonly Color SubstanceC = Hex("#FFCC2A");
        public static readonly Color CreamText = Hex("#F2E6C9");
        public static readonly Color Gold = Hex("#FFD24A");

        public static void Init()
        {
            Concrete = BlockoutKit.Mat("Concrete", Hex("#5A5650"), 0f, 0.22f);
            WallUpper = BlockoutKit.Mat("WallUpper", Hex("#6A6458"), 0f, 0.12f);
            WallLower = BlockoutKit.Mat("WallLower", Hex("#3B3A36"), 0f, 0.35f);
            Ceiling = BlockoutKit.Mat("Ceiling", Hex("#2B2523"), 0f, 0.05f);
            SteelDark = BlockoutKit.Mat("SteelDark", Hex("#2F3236"), 0.7f, 0.45f);
            Steel = BlockoutKit.Mat("Steel", Hex("#8A8F94"), 0.8f, 0.6f);
            Brass = BlockoutKit.Mat("Brass", Hex("#B08A3A"), 0.85f, 0.6f);
            PropRed = BlockoutKit.Mat("PropRed", Hex("#8E1B1B"), 0f, 0.25f);
            PropRedBright = BlockoutKit.Mat("PropRedBright", Hex("#B3121B"), 0.3f, 0.4f);
            Cream = BlockoutKit.Mat("Cream", Hex("#F2E6C9"), 0f, 0.2f);
            Glass = BlockoutKit.Transparent("Glass", new Color(0.75f, 0.91f, 1f, 0.12f), 0.95f);
            Gas = BlockoutKit.Transparent("ChamberGas", new Color(0.43f, 1f, 0.29f, 0.07f), 0.2f, new Color(0.25f, 0.6f, 0.17f));
            ChamberRing = BlockoutKit.Emissive("ChamberNozzleRing", Hex("#6DFF4A"), 1.5f, false);
            LampWarm = BlockoutKit.Emissive("LampWarm", Hex("#FFE27A"), 2.5f, true);
            TvScreen = BlockoutKit.Emissive("TvScreen", Hex("#7A1A14"), 1.2f, false);
            PlanScreen = BlockoutKit.Emissive("PlanScreen", Hex("#14301E"), 1f, false);
            TermScreen = BlockoutKit.Emissive("TermScreen", Hex("#1F6B2A"), 1.2f, false);
            WindowSky = BlockoutKit.Emissive("WindowSky", Hex("#5B6478"), 0.9f, true);
            Hazard = BlockoutKit.Mat("Hazard", Hex("#F2B705"), 0.1f, 0.35f);
            Wood = BlockoutKit.Mat("Wood", Hex("#6B4A2B"), 0f, 0.2f);
            Rubber = BlockoutKit.Mat("BeltRubber", Hex("#2C3236"), 0.1f, 0.25f);
            Crate = BlockoutKit.Mat("Crate", Hex("#8A6A3F"), 0f, 0.1f);
            PultBody = BlockoutKit.Mat("PultBody", Hex("#4A4F4A"), 0.55f, 0.45f);
            PultTop = BlockoutKit.Mat("PultTop", Hex("#3A3E3A"), 0.6f, 0.5f);
            Barrel = BlockoutKit.Mat("Barrel", Hex("#B3121B"), 0.4f, 0.45f);
            MotorGreen = BlockoutKit.Mat("MotorGreen", Hex("#2F6B3A"), 0.4f, 0.45f);
            ProcessingMetal = BlockoutKit.Mat("ProcessingMetal", Hex("#9AA0A6"), 0.6f, 0.55f);
            CryoMetal = BlockoutKit.Mat("CryoMetal", Hex("#BFCFD8"), 0.5f, 0.6f);
            CryoWindow = BlockoutKit.Emissive("CryoWindow", Hex("#9FE8FF"), 0.4f, false);
            CentrifugeWhite = BlockoutKit.Mat("CentrifugeWhite", Hex("#D8DCDF"), 0.4f, 0.55f);
            SignPlate = BlockoutKit.Mat("SignPlate", Hex("#1A1612"), 0.2f, 0.3f);
            StopButton = BlockoutKit.Emissive("StopButton", Hex("#D01A1A"), 0.4f, false);
            LampGreen = BlockoutKit.Emissive("LampGreen", Hex("#46D14F"), 1.2f, false);
            LampBlue = BlockoutKit.Emissive("LampBlue", Hex("#3AA0FF"), 0.6f, false);
            LampAmber = BlockoutKit.Emissive("LampAmber", Hex("#FFD24A"), 0.6f, false);
            Hole = BlockoutKit.Mat("Hole", Hex("#0C0C0C"), 0f, 0f);
            VatGreen = BlockoutKit.Emissive("VatGreen", Hex("#9DFF6A"), 0.8f, false);
            Cloth = BlockoutKit.Mat("Cloth", Hex("#8C8270"), 0f, 0.05f);
            Black = BlockoutKit.Mat("Black", Hex("#111111"), 0.2f, 0.6f);
            KnobA = BlockoutKit.Emissive("KnobA", SubstanceA, 0.5f, false);
            KnobB = BlockoutKit.Emissive("KnobB", SubstanceB, 0.5f, false);
            KnobC = BlockoutKit.Emissive("KnobC", SubstanceC, 0.5f, false);
            LiquidA = BlockoutKit.Emissive("LiquidA", SubstanceA, 0.45f, false);
            LiquidB = BlockoutKit.Emissive("LiquidB", SubstanceB, 0.45f, false);
            LiquidC = BlockoutKit.Emissive("LiquidC", SubstanceC, 0.45f, false);
            Antidote = BlockoutKit.Mat("Antidote", Hex("#F4F1EA"), 0f, 0.3f);
        }

        public static Color Hex(string hex) => ColorUtility.TryParseHtmlString(hex, out var c) ? c : Color.magenta;
    }
}
