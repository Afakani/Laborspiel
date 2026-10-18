using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using P = PlanErfuellt.EditorTools.PanelUiBuilder;

namespace PlanErfuellt.EditorTools
{
    /// <summary>
    /// HUD-Teile für Phase 6: Quoten-Panel links, Spielwerte rechts, Direktor-Untertitel, roter Wutrand,
    /// Bestellterminal, Startbildschirm und Endbildschirm. Stil wie die Panels (dunkles Industrieschild).
    /// </summary>
    public static class HudDayBuilder
    {
        static readonly Color Gold = new Color(1f, 0.82f, 0.29f);
        static readonly Color Green = new Color(0.55f, 1f, 0.45f);
        static readonly Color BarBg = new Color(0.05f, 0.05f, 0.04f, 0.9f);
        static readonly Color Dim = new Color(0.06f, 0.03f, 0.02f, 0.82f);
        static readonly Color ConfirmBg = new Color(0.16f, 0.32f, 0.12f);
        static readonly Color ConfirmBorder = new Color(0.43f, 1f, 0.29f);

        /// <summary>Elemente unter dem Pause-Overlay (Reihenfolge = Zeichenreihenfolge).</summary>
        public static void BuildHud(Transform canvas)
        {
            P.Init();
            BuildRageBorder(canvas);
            BuildQuota(canvas);
            BuildWorld(canvas);
            BuildSubtitle(canvas);
            BuildBuildBar(canvas);
        }

        /// <summary>Vollbild-Overlays über allem außer der Pause.</summary>
        public static void BuildOverlays(Transform canvas)
        {
            P.Init();
            BuildOrder(canvas);
            BuildIntro(canvas);
            BuildEnd(canvas);
            BuildSpecialNews(canvas);
        }

        // ---------- HUD ----------

        static void BuildRageBorder(Transform canvas)
        {
            var go = new GameObject("RageBorder", typeof(RectTransform));
            var rt = (RectTransform)go.transform;
            rt.SetParent(canvas, false);
            P.Stretch(rt);
            var img = go.AddComponent<RawImage>();
            img.texture = AssetDatabase.LoadAssetAtPath<Texture2D>(LookDevKit.OwnTexDir + "/Vignette.png");
            img.raycastTarget = false;
            img.color = new Color(1f, 0f, 0f, 0f);
            Set(go.AddComponent<RageBorder>(), "border", img);
        }

        static void BuildQuota(Transform canvas)
        {
            var holder = Holder(canvas, "Quota");
            var root = P.Rect(holder, "QuotaPanel", new Vector2(20f, -136f), new Vector2(460f, 286f));
            var frame = root.gameObject.AddComponent<Image>();
            frame.color = P.Border;
            var inner = Bg(root);
            var title = P.Text(inner, "Title", 22, Gold, new Vector2(14f, -8f), new Vector2(430f, 32f), TextAlignmentOptions.Left);
            var panel = holder.gameObject.AddComponent<QuotaPanel>();
            var so = new SerializedObject(panel);
            so.FindProperty("root").objectReferenceValue = root.gameObject;
            so.FindProperty("frame").objectReferenceValue = frame;
            so.FindProperty("title").objectReferenceValue = title;
            var goods = so.FindProperty("goods");
            goods.arraySize = 3;
            for (int i = 0; i < 3; i++)
            {
                float y = -46f - i * 44f;
                var label = P.Text(inner, "Label_" + i, 19, P.Cream, new Vector2(14f, y), new Vector2(160f, 26f), TextAlignmentOptions.Left);
                var value = P.Text(inner, "Value_" + i, 19, P.Cream, new Vector2(220f, y), new Vector2(222f, 26f), TextAlignmentOptions.Right, P.mono);
                var bg = P.Img(inner, "BarBg_" + i, new Vector2(14f, y - 28f), new Vector2(428f, 9f), BarBg, null, false);
                var fill = P.Img(bg.rectTransform, "Fill", Vector2.zero, new Vector2(428f, 9f), Gold, AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd"), false);
                fill.type = Image.Type.Filled;
                fill.fillMethod = Image.FillMethod.Horizontal;
                fill.fillAmount = 0f;
                var row = goods.GetArrayElementAtIndex(i);
                row.FindPropertyRelative("label").objectReferenceValue = label;
                row.FindPropertyRelative("value").objectReferenceValue = value;
                row.FindPropertyRelative("fill").objectReferenceValue = fill;
            }
            var wish = P.Text(inner, "Wish", 17, Gold, new Vector2(14f, -180f), new Vector2(430f, 24f), TextAlignmentOptions.Left);
            var contract = P.Text(inner, "Contract", 17, Green, new Vector2(14f, -204f), new Vector2(430f, 24f), TextAlignmentOptions.Left);
            var hint = P.Text(inner, "Hint", 16, P.Muted, new Vector2(14f, -230f), new Vector2(430f, 50f), TextAlignmentOptions.TopLeft);
            hint.textWrappingMode = TMPro.TextWrappingModes.Normal;
            foreach (var t in new[] { wish, contract })
            {
                t.enableAutoSizing = true;
                t.fontSizeMin = 12f;
                t.fontSizeMax = 17f;
            }
            so.FindProperty("wish").objectReferenceValue = wish;
            so.FindProperty("contract").objectReferenceValue = contract;
            so.FindProperty("hint").objectReferenceValue = hint;
            so.ApplyModifiedPropertiesWithoutUndo();
            root.gameObject.SetActive(false);
        }

        static void BuildWorld(Transform canvas)
        {
            var root = P.Rect(canvas, "WorldPanel", new Vector2(-20f, -70f), new Vector2(300f, 236f));
            root.gameObject.AddComponent<Image>().color = P.Border;
            var inner = Bg(root);
            var title = P.Text(inner, "Title", 18, P.Head, new Vector2(12f, -6f), new Vector2(276f, 26f), TextAlignmentOptions.Left);
            title.characterSpacing = 4f;
            var panel = root.gameObject.AddComponent<WorldPanel>();
            var so = new SerializedObject(panel);
            so.FindProperty("title").objectReferenceValue = title;
            string[] names = { "food", "pop", "env", "anger", "authority" };
            for (int i = 0; i < names.Length; i++)
            {
                float y = -36f - i * 39f;
                var label = P.Text(inner, "Label_" + names[i], 16, P.Cream, new Vector2(12f, y), new Vector2(200f, 22f), TextAlignmentOptions.Left);
                var value = P.Text(inner, "Value_" + names[i], 16, P.Cream, new Vector2(200f, y), new Vector2(86f, 22f), TextAlignmentOptions.Right, P.mono);
                var bg = P.Img(inner, "BarBg_" + names[i], new Vector2(12f, y - 24f), new Vector2(274f, 8f), BarBg, null, false);
                var fill = P.Img(bg.rectTransform, "Fill", Vector2.zero, new Vector2(274f, 8f), Gold, AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd"), false);
                fill.type = Image.Type.Filled;
                fill.fillMethod = Image.FillMethod.Horizontal;
                var bar = so.FindProperty(names[i]);
                bar.FindPropertyRelative("label").objectReferenceValue = label;
                bar.FindPropertyRelative("value").objectReferenceValue = value;
                bar.FindPropertyRelative("fill").objectReferenceValue = fill;
            }
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        static void BuildSubtitle(Transform canvas)
        {
            var holder = Holder(canvas, "Subtitle");
            var go = new GameObject("Box", typeof(RectTransform));
            var rt = (RectTransform)go.transform;
            rt.SetParent(holder, false);
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0f);
            rt.pivot = new Vector2(0.5f, 0f);
            rt.anchoredPosition = new Vector2(0f, 186f);
            rt.sizeDelta = new Vector2(1240f, 64f);
            go.AddComponent<Image>().color = new Color(0.06f, 0.03f, 0.02f, 0.85f);
            var text = P.Text(rt, "Text", 26, P.Cream, new Vector2(16f, -6f), new Vector2(1208f, 52f), TextAlignmentOptions.Center);
            text.enableAutoSizing = true;
            text.fontSizeMin = 16f;
            text.fontSizeMax = 26f;
            var ui = holder.gameObject.AddComponent<SubtitleUI>();
            Set(ui, "root", go);
            Set(ui, "text", text);
        }

        // ---------- Bestellterminal ----------

        static void BuildOrder(Transform canvas)
        {
            var holder = Holder(canvas, "OrderOverlay");
            var root = FullScreen(holder, "Screen", Dim);
            var frame = CenterFrame(root, "Frame", new Vector2(1060f, 760f));
            var title = P.Text(frame, "Title", 34, Gold, new Vector2(30f, -20f), new Vector2(1000f, 46f), TextAlignmentOptions.Left);
            var info = P.Text(frame, "Info", 18, P.Cream, new Vector2(30f, -72f), new Vector2(1000f, 84f), TextAlignmentOptions.TopLeft);
            info.textWrappingMode = TMPro.TextWrappingModes.Normal;
            var ui = holder.gameObject.AddComponent<OrderTerminalUI>();
            var so = new SerializedObject(ui);
            so.FindProperty("root").objectReferenceValue = root;
            so.FindProperty("title").objectReferenceValue = title;
            so.FindProperty("info").objectReferenceValue = info;
            var rows = so.FindProperty("rows");
            rows.arraySize = 6;
            for (int i = 0; i < 6; i++)
            {
                float y = -168f - i * 82f;
                var rowBg = P.Img(frame, "Row_" + i, new Vector2(24f, y), new Vector2(1012f, 74f), new Color(0.12f, 0.1f, 0.07f, 0.9f), null, false);
                var name = P.Text(rowBg.rectTransform, "Name", 24, P.Cream, new Vector2(16f, -6f), new Vector2(420f, 32f), TextAlignmentOptions.Left);
                var desc = P.Text(rowBg.rectTransform, "Desc", 15, P.Muted, new Vector2(16f, -40f), new Vector2(560f, 26f), TextAlignmentOptions.Left);
                desc.enableAutoSizing = true;
                desc.fontSizeMin = 11f;
                desc.fontSizeMax = 15f;
                var stock = P.Text(rowBg.rectTransform, "Stock", 16, P.Muted, new Vector2(590f, -24f), new Vector2(130f, 26f), TextAlignmentOptions.Left);
                var minus = P.MakeButton(rowBg.rectTransform, "Minus", new Vector2(730f, -14f), new Vector2(46f, 46f), P.ButtonBg, P.Border, 28, true);
                var count = P.Text(rowBg.rectTransform, "Count", 26, Color.white, new Vector2(780f, -18f), new Vector2(60f, 38f), TextAlignmentOptions.Center, P.mono);
                var plus = P.MakeButton(rowBg.rectTransform, "Plus", new Vector2(844f, -14f), new Vector2(46f, 46f), P.ButtonBg, P.Border, 28, true);
                minus.GetComponentInChildren<TMP_Text>().text = "−";
                plus.GetComponentInChildren<TMP_Text>().text = "+";
                var price = P.Text(rowBg.rectTransform, "Price", 18, Gold, new Vector2(900f, -24f), new Vector2(100f, 26f), TextAlignmentOptions.Right, P.mono);
                var r = rows.GetArrayElementAtIndex(i);
                r.FindPropertyRelative("name").objectReferenceValue = name;
                r.FindPropertyRelative("desc").objectReferenceValue = desc;
                r.FindPropertyRelative("stock").objectReferenceValue = stock;
                r.FindPropertyRelative("count").objectReferenceValue = count;
                r.FindPropertyRelative("price").objectReferenceValue = price;
                r.FindPropertyRelative("minus").objectReferenceValue = minus;
                r.FindPropertyRelative("plus").objectReferenceValue = plus;
            }
            var sum = P.Text(frame, "Sum", 22, P.Cream, new Vector2(30f, -680f), new Vector2(620f, 40f), TextAlignmentOptions.Left, P.mono);
            var confirm = P.MakeButton(frame, "Confirm", new Vector2(-30f, -670f), new Vector2(380f, 62f), ConfirmBg, ConfirmBorder, 24, false);
            so.FindProperty("sum").objectReferenceValue = sum;
            so.FindProperty("confirm").objectReferenceValue = confirm;
            so.FindProperty("confirmLabel").objectReferenceValue = confirm.GetComponentInChildren<TMP_Text>();
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        /// <summary>Leiste des Baumodus unten (Prototyp #build): Titel, vier Maschinen, Abriss.</summary>
        static void BuildBuildBar(Transform canvas)
        {
            var holder = Holder(canvas, "BuildBar");
            var bar = new GameObject("Bar", typeof(RectTransform), typeof(Image));
            var rt = (RectTransform)bar.transform;
            rt.SetParent(holder, false);
            rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0f);
            rt.anchoredPosition = new Vector2(0f, 20f);
            rt.sizeDelta = new Vector2(1560f, 176f);
            bar.GetComponent<Image>().color = P.Bg;
            var title = P.Text(rt, "Title", 22, Gold, new Vector2(20f, -8f), new Vector2(800f, 30f), TextAlignmentOptions.Left);

            var ui = holder.gameObject.AddComponent<BuildUI>();
            var so = new SerializedObject(ui);
            var tools = so.FindProperty("tools");
            tools.arraySize = 4;
            for (int i = 0; i < 5; i++)
            {
                var b = P.MakeButton(rt, i < 4 ? "Tool" + i : "Sell", new Vector2(20f + i * 306f, -44f), new Vector2(294f, 120f), P.ButtonBg, P.Border, 18, true);
                b.GetComponentInChildren<TMP_Text>().text = "";
                var name = P.Text(b.transform, "Name", 20, P.Cream, new Vector2(10f, -8f), new Vector2(274f, 28f), TextAlignmentOptions.Left);
                var price = P.Text(b.transform, "Price", 18, Gold, new Vector2(10f, -38f), new Vector2(274f, 26f), TextAlignmentOptions.Left, P.mono);
                var desc = P.Text(b.transform, "Desc", 15, P.Muted, new Vector2(10f, -66f), new Vector2(274f, 48f), TextAlignmentOptions.TopLeft);
                desc.textWrappingMode = TMPro.TextWrappingModes.Normal;
                var e = i < 4 ? tools.GetArrayElementAtIndex(i) : so.FindProperty("sell");
                e.FindPropertyRelative("button").objectReferenceValue = b;
                e.FindPropertyRelative("frame").objectReferenceValue = b.GetComponent<Image>();
                e.FindPropertyRelative("name").objectReferenceValue = name;
                e.FindPropertyRelative("price").objectReferenceValue = price;
                e.FindPropertyRelative("desc").objectReferenceValue = desc;
            }
            so.FindProperty("root").objectReferenceValue = bar;
            so.FindProperty("title").objectReferenceValue = title;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        // ---------- Start- und Endbildschirm ----------

        static void BuildIntro(Transform canvas)
        {
            var holder = Holder(canvas, "IntroOverlay");
            var root = FullScreen(holder, "Screen", new Color(0.07f, 0.035f, 0.025f, 0.97f));
            var frame = CenterFrame(root, "Frame", new Vector2(1400f, 900f));
            var title = P.Text(frame, "Title", 72, Gold, new Vector2(40f, -24f), new Vector2(1320f, 90f), TextAlignmentOptions.Center);
            var sub = P.Text(frame, "Subtitle", 26, P.Head, new Vector2(40f, -112f), new Vector2(1320f, 36f), TextAlignmentOptions.Center);
            sub.characterSpacing = 8f;
            var body = P.Text(frame, "Body", 20, P.Cream, new Vector2(60f, -170f), new Vector2(1280f, 600f), TextAlignmentOptions.TopLeft);
            body.textWrappingMode = TMPro.TextWrappingModes.Normal;
            body.enableAutoSizing = true;
            body.fontSizeMin = 14f;
            body.fontSizeMax = 21f;
            var start = P.MakeButton(frame, "Start", new Vector2(510f, -800f), new Vector2(380f, 70f), ConfirmBg, ConfirmBorder, 30, false);
            var ui = holder.gameObject.AddComponent<IntroUI>();
            var so = new SerializedObject(ui);
            so.FindProperty("root").objectReferenceValue = root;
            so.FindProperty("title").objectReferenceValue = title;
            so.FindProperty("subtitle").objectReferenceValue = sub;
            so.FindProperty("body").objectReferenceValue = body;
            so.FindProperty("start").objectReferenceValue = start;
            so.FindProperty("startLabel").objectReferenceValue = start.GetComponentInChildren<TMP_Text>();
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        static void BuildEnd(Transform canvas)
        {
            var holder = Holder(canvas, "EndOverlay");
            var root = FullScreen(holder, "Screen", new Color(0.07f, 0.035f, 0.025f, 0.95f));
            var frame = CenterFrame(root, "Frame", new Vector2(1200f, 860f));
            var title = P.Text(frame, "Title", 58, Gold, new Vector2(40f, -30f), new Vector2(1120f, 80f), TextAlignmentOptions.Center);
            var text = P.Text(frame, "Text", 22, P.Cream, new Vector2(80f, -120f), new Vector2(1040f, 110f), TextAlignmentOptions.Top);
            text.textWrappingMode = TMPro.TextWrappingModes.Normal;
            var labels = P.Text(frame, "StatLabels", 22, P.Muted, new Vector2(120f, -260f), new Vector2(460f, 420f), TextAlignmentOptions.TopRight);
            var values = P.Text(frame, "StatValues", 22, P.Cream, new Vector2(620f, -260f), new Vector2(460f, 420f), TextAlignmentOptions.TopLeft, P.mono);
            labels.lineSpacing = values.lineSpacing = 12f;
            var again = P.MakeButton(frame, "Again", new Vector2(410f, -760f), new Vector2(380f, 68f), ConfirmBg, ConfirmBorder, 26, false);
            var ui = holder.gameObject.AddComponent<EndScreenUI>();
            var so = new SerializedObject(ui);
            so.FindProperty("root").objectReferenceValue = root;
            so.FindProperty("title").objectReferenceValue = title;
            so.FindProperty("text").objectReferenceValue = text;
            so.FindProperty("statLabels").objectReferenceValue = labels;
            so.FindProperty("statValues").objectReferenceValue = values;
            so.FindProperty("again").objectReferenceValue = again;
            so.FindProperty("againLabel").objectReferenceValue = again.GetComponentInChildren<TMP_Text>();
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        /// <summary>Sondersendung (GDD 10): Staats-TV-Einblendung mit roter Kennung, Schlagzeile und Laufband.</summary>
        static void BuildSpecialNews(Transform canvas)
        {
            var holder = Holder(canvas, "SpecialNewsOverlay");
            var root = FullScreen(holder, "Screen", new Color(0.03f, 0.05f, 0.12f, 0.93f));
            var frame = CenterFrame(root, "Frame", new Vector2(1500f, 560f));
            var box = P.Img(frame, "TagBox", new Vector2(60f, -50f), new Vector2(560f, 92f), new Color(0.82f, 0.1f, 0.1f), null, false);
            box.rectTransform.pivot = new Vector2(0.5f, 0.5f);
            box.rectTransform.anchoredPosition = new Vector2(60f + 280f, -50f - 46f);
            var tag = P.Text(box.rectTransform, "Tag", 58, Color.white, new Vector2(0f, 0f), new Vector2(560f, 92f), TextAlignmentOptions.Center);
            tag.characterSpacing = 6f;
            var head = P.Text(frame, "Head", 54, Gold, new Vector2(60f, -180f), new Vector2(1380f, 230f), TextAlignmentOptions.TopLeft);
            head.textWrappingMode = TMPro.TextWrappingModes.Normal;
            var bar = P.Img(frame, "TickerBar", new Vector2(2f, -470f), new Vector2(1496f, 70f), new Color(0.6f, 0.05f, 0.06f), null, false);
            var ticker = P.Text(bar.rectTransform, "Ticker", 26, P.Cream, new Vector2(30f, -14f), new Vector2(1440f, 44f), TextAlignmentOptions.Left);
            ticker.textWrappingMode = TMPro.TextWrappingModes.NoWrap;
            ticker.overflowMode = TextOverflowModes.Ellipsis;
            holder.gameObject.AddComponent<SpecialNewsUI>().Setup(root, tag, head, box.rectTransform, ticker);
        }

        // ---------- Hilfen ----------

        /// <summary>Immer aktives, bildschirmfüllendes Halteobjekt für ein UI-Skript, dessen Inhalt ein- und ausgeblendet wird.</summary>
        static RectTransform Holder(Transform canvas, string name)
        {
            var go = new GameObject(name, typeof(RectTransform));
            var rt = (RectTransform)go.transform;
            rt.SetParent(canvas, false);
            P.Stretch(rt);
            return rt;
        }

        static RectTransform Bg(RectTransform frame)
        {
            var inner = new GameObject("Bg", typeof(RectTransform), typeof(Image));
            var irt = (RectTransform)inner.transform;
            irt.SetParent(frame, false);
            P.Stretch(irt);
            irt.offsetMin = new Vector2(2f, 2f);
            irt.offsetMax = new Vector2(-2f, -2f);
            inner.GetComponent<Image>().color = P.Bg;
            return irt;
        }

        static GameObject FullScreen(Transform canvas, string name, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            var rt = (RectTransform)go.transform;
            rt.SetParent(canvas, false);
            P.Stretch(rt);
            var img = go.GetComponent<Image>();
            img.color = color;
            img.raycastTarget = true;
            return go;
        }

        /// <summary>Rahmen mittig im Bildschirm; Kinder werden oben links verankert (P.Rect).</summary>
        static RectTransform CenterFrame(GameObject parent, string name, Vector2 size)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            var rt = (RectTransform)go.transform;
            rt.SetParent(parent.transform, false);
            rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = size;
            go.GetComponent<Image>().color = P.Border;
            var inner = Bg(rt);
            inner.GetComponent<Image>().raycastTarget = false;
            return rt;
        }

        static void Set(Object target, string field, Object value) => PlayerBuilder.Set(target, field, value);
    }
}
