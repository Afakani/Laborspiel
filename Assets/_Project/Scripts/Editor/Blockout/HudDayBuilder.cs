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
            BuildTutorial(canvas);
        }

        /// <summary>Vollbild-Overlays über allem außer der Pause.</summary>
        public static void BuildOverlays(Transform canvas)
        {
            P.Init();
            BuildOrder(canvas);
            BuildCanteen(canvas);
            BuildDayStart(canvas);
            BuildIntro(canvas);
            BuildEnd(canvas);
            BuildNews(canvas);
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

        const float QuotaScale = 0.8f;

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
            // kompakter: ganzes Planfenster auf 80 % (Drehpunkt oben links bleibt)
            root.localScale = new Vector3(QuotaScale, QuotaScale, 1f);
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
            var frame = CenterFrame(root, "Frame", new Vector2(1060f, 830f));
            var title = P.Text(frame, "Title", 34, Gold, new Vector2(30f, -20f), new Vector2(1000f, 46f), TextAlignmentOptions.Left);
            var info = P.Text(frame, "Info", 18, P.Cream, new Vector2(30f, -70f), new Vector2(1000f, 60f), TextAlignmentOptions.TopLeft);
            info.textWrappingMode = TMPro.TextWrappingModes.Normal;
            var pending = P.Text(frame, "Pending", 18, Green, new Vector2(30f, -134f), new Vector2(760f, 30f), TextAlignmentOptions.Left);
            var repeat = P.MakeButton(frame, "Repeat", new Vector2(-30f, -124f), new Vector2(220f, 42f), P.ButtonBg, Gold, 18, true);
            pending.enableAutoSizing = true;
            pending.fontSizeMin = 12f;
            pending.fontSizeMax = 18f;
            var ui = holder.gameObject.AddComponent<OrderTerminalUI>();
            var so = new SerializedObject(ui);
            so.FindProperty("panelId").stringValue = "order";
            so.FindProperty("root").objectReferenceValue = root;
            so.FindProperty("title").objectReferenceValue = title;
            so.FindProperty("info").objectReferenceValue = info;
            so.FindProperty("pending").objectReferenceValue = pending;
            var rows = so.FindProperty("rows");
            rows.arraySize = OrderLogic.Items;
            for (int i = 0; i < OrderLogic.Items; i++)
            {
                float y = -174f - i * 80f;
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
            var sum = P.Text(frame, "Sum", 22, P.Cream, new Vector2(30f, -750f), new Vector2(520f, 40f), TextAlignmentOptions.Left, P.mono);
            var close = P.MakeButton(frame, "Close", new Vector2(-420f, -740f), new Vector2(170f, 62f), P.ButtonBg, Gold, 22, false);
            var confirm = P.MakeButton(frame, "Confirm", new Vector2(-30f, -740f), new Vector2(370f, 62f), ConfirmBg, ConfirmBorder, 24, false);
            so.FindProperty("closeButton").objectReferenceValue = close;
            so.FindProperty("closeLabel").objectReferenceValue = close.GetComponentInChildren<TMP_Text>();
            so.FindProperty("repeat").objectReferenceValue = repeat;
            so.FindProperty("repeatLabel").objectReferenceValue = repeat.GetComponentInChildren<TMP_Text>();
            so.FindProperty("sum").objectReferenceValue = sum;
            so.FindProperty("confirm").objectReferenceValue = confirm;
            so.FindProperty("confirmLabel").objectReferenceValue = confirm.GetComponentInChildren<TMP_Text>();
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        /// <summary>Auftragskarte zu Schichtbeginn (DayStartUI): Tagessoll groß, Ablauf, Hinweise, Start-Knopf.</summary>
        static void BuildDayStart(Transform canvas)
        {
            var holder = Holder(canvas, "DayStartOverlay");
            var root = FullScreen(holder, "Screen", Dim);
            var frame = CenterFrame(root, "Frame", new Vector2(1040f, 800f));
            var title = P.Text(frame, "Title", 40, Gold, new Vector2(40f, -26f), new Vector2(960f, 56f), TextAlignmentOptions.Center);
            var sub = P.Text(frame, "GoalHead", 20, P.Head, new Vector2(40f, -88f), new Vector2(960f, 30f), TextAlignmentOptions.Center);
            sub.characterSpacing = 6f;
            var ui = holder.gameObject.AddComponent<DayStartUI>();
            var so = new SerializedObject(ui);
            so.FindProperty("panelId").stringValue = "dayStart";
            so.FindProperty("root").objectReferenceValue = root;
            so.FindProperty("title").objectReferenceValue = title;
            var goods = so.FindProperty("goods");
            goods.arraySize = 3;
            var cols = new[] { new Color(0.93f, 0.95f, 1f), new Color(1f, 0.86f, 0.45f), new Color(1f, 0.55f, 0.5f) };
            for (int i = 0; i < 3; i++)
            {
                var box = P.Img(frame, "Goal_" + i, new Vector2(60f + i * 310f, -128f), new Vector2(290f, 110f), new Color(0.12f, 0.1f, 0.07f, 0.95f), null, false);
                var g = P.Text(box.rectTransform, "Text", 40, cols[i], new Vector2(0f, -24f), new Vector2(290f, 60f), TextAlignmentOptions.Center, P.mono);
                g.enableAutoSizing = true;
                g.fontSizeMin = 22f;
                g.fontSizeMax = 40f;
                goods.GetArrayElementAtIndex(i).objectReferenceValue = g;
            }
            var time = P.Text(frame, "Time", 22, P.Cream, new Vector2(40f, -250f), new Vector2(960f, 32f), TextAlignmentOptions.Center);
            var steps = P.Text(frame, "Steps", 22, P.Cream, new Vector2(70f, -300f), new Vector2(900f, 200f), TextAlignmentOptions.TopLeft);
            steps.textWrappingMode = TMPro.TextWrappingModes.Normal;
            steps.enableAutoSizing = true;
            steps.fontSizeMin = 15f;
            steps.fontSizeMax = 22f;
            var notes = P.Text(frame, "Notes", 20, Gold, new Vector2(70f, -510f), new Vector2(900f, 170f), TextAlignmentOptions.TopLeft);
            notes.textWrappingMode = TMPro.TextWrappingModes.Normal;
            notes.enableAutoSizing = true;
            notes.fontSizeMin = 14f;
            notes.fontSizeMax = 20f;
            var start = P.MakeButton(frame, "Start", new Vector2(-310f, -704f), new Vector2(420f, 72f), ConfirmBg, ConfirmBorder, 28, false);
            so.FindProperty("time").objectReferenceValue = time;
            so.FindProperty("steps").objectReferenceValue = steps;
            so.FindProperty("notes").objectReferenceValue = notes;
            so.FindProperty("startButton").objectReferenceValue = start;
            so.FindProperty("startLabel").objectReferenceValue = start.GetComponentInChildren<TMP_Text>();
            so.FindProperty("goalHead").objectReferenceValue = sub;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        /// <summary>Speisekarte der Kantine (CanteenUI): drei Gerichte als große Knöpfe, aktueller Schub, Schließen.</summary>
        static void BuildCanteen(Transform canvas)
        {
            var holder = Holder(canvas, "CanteenOverlay");
            var root = FullScreen(holder, "Screen", Dim);
            var frame = CenterFrame(root, "Frame", new Vector2(820f, 640f));
            var title = P.Text(frame, "Title", 34, Gold, new Vector2(30f, -20f), new Vector2(760f, 46f), TextAlignmentOptions.Left);
            var info = P.Text(frame, "Info", 18, P.Cream, new Vector2(30f, -72f), new Vector2(760f, 56f), TextAlignmentOptions.TopLeft);
            info.textWrappingMode = TMPro.TextWrappingModes.Normal;
            var current = P.Text(frame, "Current", 20, Green, new Vector2(30f, -132f), new Vector2(760f, 30f), TextAlignmentOptions.Left);
            var ui = holder.gameObject.AddComponent<CanteenUI>();
            var so = new SerializedObject(ui);
            so.FindProperty("panelId").stringValue = "canteen";
            so.FindProperty("root").objectReferenceValue = root;
            so.FindProperty("title").objectReferenceValue = title;
            so.FindProperty("info").objectReferenceValue = info;
            so.FindProperty("current").objectReferenceValue = current;
            const int count = 3;
            var meals = so.FindProperty("meals");
            var names = so.FindProperty("mealNames");
            var descs = so.FindProperty("mealDescs");
            meals.arraySize = names.arraySize = descs.arraySize = count;
            for (int i = 0; i < count; i++)
            {
                var b = P.MakeButton(frame, "Meal_" + i, new Vector2(30f, -178f - i * 118f), new Vector2(760f, 106f), P.ButtonBg, Gold, 18, false);
                b.GetComponentInChildren<TMP_Text>().text = "";
                var name = P.Text(b.transform, "Name", 30, P.Cream, new Vector2(22f, -12f), new Vector2(716f, 40f), TextAlignmentOptions.Left);
                var desc = P.Text(b.transform, "Desc", 20, Gold, new Vector2(22f, -58f), new Vector2(716f, 32f), TextAlignmentOptions.Left, P.mono);
                meals.GetArrayElementAtIndex(i).objectReferenceValue = b;
                names.GetArrayElementAtIndex(i).objectReferenceValue = name;
                descs.GetArrayElementAtIndex(i).objectReferenceValue = desc;
            }
            var close = P.MakeButton(frame, "Close", new Vector2(-30f, -560f), new Vector2(220f, 58f), P.ButtonBg, Gold, 22, false);
            so.FindProperty("closeButton").objectReferenceValue = close;
            so.FindProperty("closeLabel").objectReferenceValue = close.GetComponentInChildren<TMP_Text>();
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

            // ---------- Hauptmenü (eigener Vollbild-Bildschirm) ----------
            var menu = FullScreen(holder, "Screen", new Color(0.07f, 0.03f, 0.025f, 1f));
            var mrt = (RectTransform)menu.transform;
            // rotes Band links mit Emblem, Titel und Knöpfen
            var band = new GameObject("Band", typeof(RectTransform), typeof(Image));
            var brt = (RectTransform)band.transform;
            brt.SetParent(mrt, false);
            brt.anchorMin = new Vector2(0f, 0f);
            brt.anchorMax = new Vector2(0f, 1f);
            brt.pivot = new Vector2(0f, 0.5f);
            brt.anchoredPosition = new Vector2(90f, 0f);
            brt.sizeDelta = new Vector2(640f, 0f);
            band.GetComponent<Image>().color = new Color(0.56f, 0.11f, 0.11f);
            var edge = Raw(brt, "Hazard", "Hazard.png", new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0f, 34f), Color.white);
            edge.uvRect = new Rect(0f, 0f, 10f, 0.55f);
            Raw(brt, "Emblem", "EmblemGold.png", new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(230f, 230f), new Color(1f, 1f, 1f, 0.95f))
                .rectTransform.anchoredPosition = new Vector2(0f, -50f);
            var mTitle = P.Text(brt, "Title", 92, P.Cream, new Vector2(20f, -300f), new Vector2(600f, 120f), TextAlignmentOptions.Center);
            mTitle.fontStyle = FontStyles.UpperCase;
            var mSub = P.Text(brt, "Subtitle", 28, Gold, new Vector2(20f, -410f), new Vector2(600f, 40f), TextAlignmentOptions.Center);
            mSub.characterSpacing = 10f;
            var col = P.Rect(brt, "Buttons", new Vector2(80f, -490f), new Vector2(480f, 420f));
            var vl = col.gameObject.AddComponent<VerticalLayoutGroup>();
            vl.spacing = 18f;
            vl.childAlignment = TextAnchor.UpperCenter;
            vl.childControlWidth = vl.childControlHeight = false;
            vl.childForceExpandWidth = vl.childForceExpandHeight = false;
            var dark = new Color(0.2f, 0.06f, 0.05f);
            var cont = P.MakeButton(col, "Continue", Vector2.zero, new Vector2(480f, 76f), ConfirmBg, ConfirmBorder, 26, false);
            var newGame = P.MakeButton(col, "NewGame", Vector2.zero, new Vector2(480f, 76f), dark, Gold, 30, false);
            var settings = P.MakeButton(col, "Settings", Vector2.zero, new Vector2(480f, 76f), dark, Gold, 30, false);
            var quit = P.MakeButton(col, "Quit", Vector2.zero, new Vector2(480f, 76f), dark, Gold, 30, false);

            // rechts: Parole und Direktor-Plakat
            var slogan = P.Text(mrt, "Slogan", 50, P.Cream, new Vector2(-90f, -90f), new Vector2(1000f, 80f), TextAlignmentOptions.Center);
            slogan.characterSpacing = 4f;
            var poster = new GameObject("Poster", typeof(RectTransform), typeof(Image));
            var prt = (RectTransform)poster.transform;
            prt.SetParent(mrt, false);
            prt.anchorMin = prt.anchorMax = prt.pivot = new Vector2(1f, 0.5f);
            prt.anchoredPosition = new Vector2(-180f, 10f);
            prt.sizeDelta = new Vector2(840f, 560f);
            poster.GetComponent<Image>().color = new Color(0.88f, 0.82f, 0.66f);
            Raw(prt, "Director", "Director.png", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(800f, 500f), Color.white)
                .rectTransform.anchoredPosition = new Vector2(0f, 10f);
            var plate = new GameObject("CaptionPlate", typeof(RectTransform), typeof(Image));
            var crt = (RectTransform)plate.transform;
            crt.SetParent(prt, false);
            crt.anchorMin = crt.anchorMax = crt.pivot = new Vector2(0.5f, 0f);
            crt.anchoredPosition = new Vector2(0f, -36f);
            crt.sizeDelta = new Vector2(620f, 64f);
            plate.GetComponent<Image>().color = new Color(0.56f, 0.11f, 0.11f);
            var caption = P.Text(crt, "Caption", 30, P.Cream, new Vector2(0f, -10f), new Vector2(620f, 44f), TextAlignmentOptions.Center);
            caption.characterSpacing = 6f;
            var footer = P.Text(mrt, "Footer", 18, P.Muted, new Vector2(-90f, -1030f), new Vector2(1000f, 30f), TextAlignmentOptions.Right);

            // ---------- Einsatzbesprechung nach „Neues Spiel“ ----------
            var root = FullScreen(holder, "Screen", new Color(0.07f, 0.035f, 0.025f, 1f));
            var frame = CenterFrame(root, "Frame", new Vector2(1400f, 900f));
            var title = P.Text(frame, "Title", 72, Gold, new Vector2(40f, -24f), new Vector2(1320f, 90f), TextAlignmentOptions.Center);
            var sub = P.Text(frame, "Subtitle", 26, P.Head, new Vector2(40f, -112f), new Vector2(1320f, 36f), TextAlignmentOptions.Center);
            sub.characterSpacing = 8f;
            // eine Seite der Einsatzbesprechung: wenig Text, groß
            var body = P.Text(frame, "Body", 28, P.Cream, new Vector2(100f, -190f), new Vector2(1200f, 580f), TextAlignmentOptions.TopLeft);
            body.textWrappingMode = TMPro.TextWrappingModes.Normal;
            body.enableAutoSizing = true;
            body.fontSizeMin = 18f;
            body.fontSizeMax = 30f;
            var row = P.Rect(frame, "Buttons", new Vector2(10f, -800f), new Vector2(1380f, 70f));
            var hl = row.gameObject.AddComponent<HorizontalLayoutGroup>();
            hl.spacing = 24f;
            hl.childAlignment = TextAnchor.MiddleCenter;
            hl.childControlWidth = hl.childControlHeight = false;
            hl.childForceExpandWidth = hl.childForceExpandHeight = false;
            var back = P.MakeButton(row, "Back", Vector2.zero, new Vector2(240f, 70f), P.ButtonBg, Gold, 26, false);
            var noTut = P.MakeButton(row, "StartNoTutorial", Vector2.zero, new Vector2(300f, 70f), P.ButtonBg, Gold, 24, false);
            var start = P.MakeButton(row, "Start", Vector2.zero, new Vector2(380f, 70f), ConfirmBg, ConfirmBorder, 30, false);

            var ui = holder.gameObject.AddComponent<IntroUI>();
            var so = new SerializedObject(ui);
            so.FindProperty("menuRoot").objectReferenceValue = menu;
            so.FindProperty("menuTitle").objectReferenceValue = mTitle;
            so.FindProperty("menuSubtitle").objectReferenceValue = mSub;
            so.FindProperty("slogan").objectReferenceValue = slogan;
            so.FindProperty("caption").objectReferenceValue = caption;
            so.FindProperty("footer").objectReferenceValue = footer;
            so.FindProperty("cont").objectReferenceValue = cont;
            so.FindProperty("newGame").objectReferenceValue = newGame;
            so.FindProperty("settings").objectReferenceValue = settings;
            so.FindProperty("quit").objectReferenceValue = quit;
            so.FindProperty("root").objectReferenceValue = root;
            so.FindProperty("title").objectReferenceValue = title;
            so.FindProperty("subtitle").objectReferenceValue = sub;
            so.FindProperty("body").objectReferenceValue = body;
            so.FindProperty("start").objectReferenceValue = start;
            so.FindProperty("startLabel").objectReferenceValue = start.GetComponentInChildren<TMP_Text>();
            so.FindProperty("back").objectReferenceValue = back;
            so.FindProperty("startNoTutorial").objectReferenceValue = noTut;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        /// <summary>Einführung: Zielkasten oben in der Mitte und Pfeil zur Station (TutorialUI).</summary>
        static void BuildTutorial(Transform canvas)
        {
            var holder = Holder(canvas, "Tutorial");
            var box = new GameObject("Box", typeof(RectTransform), typeof(Image));
            var brt = (RectTransform)box.transform;
            brt.SetParent(holder, false);
            brt.anchorMin = brt.anchorMax = brt.pivot = new Vector2(0.5f, 1f);
            brt.anchoredPosition = new Vector2(0f, -138f);
            brt.sizeDelta = new Vector2(1040f, 134f);
            // grüner Rahmen, dunkle Innenfläche (bekommt das Metallschild aus HudStyle)
            var boxFrame = box.GetComponent<Image>();
            boxFrame.color = Green;
            var inner = new GameObject("Inner", typeof(RectTransform), typeof(Image));
            var irt = (RectTransform)inner.transform;
            irt.SetParent(brt, false);
            irt.anchorMin = Vector2.zero;
            irt.anchorMax = Vector2.one;
            irt.offsetMin = new Vector2(3f, 3f);
            irt.offsetMax = new Vector2(-3f, -3f);
            inner.GetComponent<Image>().color = new Color(0.05f, 0.04f, 0.03f, 0.97f);
            var stripe = new GameObject("Stripe", typeof(RectTransform), typeof(Image));
            var srt = (RectTransform)stripe.transform;
            srt.SetParent(brt, false);
            srt.anchorMin = new Vector2(0f, 0f);
            srt.anchorMax = new Vector2(0f, 1f);
            srt.pivot = new Vector2(0f, 0.5f);
            srt.sizeDelta = new Vector2(12f, 0f);
            stripe.GetComponent<Image>().color = Green;
            var title = P.Text(brt, "Title", 22, Green, new Vector2(30f, -10f), new Vector2(980f, 30f), TextAlignmentOptions.Left);
            title.characterSpacing = 6f;
            var text = P.Text(brt, "Text", 30, Color.white, new Vector2(30f, -44f), new Vector2(990f, 82f), TextAlignmentOptions.TopLeft);
            text.textWrappingMode = TMPro.TextWrappingModes.Normal;
            text.enableAutoSizing = true;
            text.fontSizeMin = 20f;
            text.fontSizeMax = 30f;

            var marker = new GameObject("Marker", typeof(RectTransform));
            var mrt = (RectTransform)marker.transform;
            mrt.SetParent(holder, false);
            mrt.anchorMin = mrt.anchorMax = mrt.pivot = new Vector2(0.5f, 0.5f);
            mrt.sizeDelta = new Vector2(84f, 84f);
            var arrow = Raw(mrt, "Arrow", "Arrow.png", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(76f, 76f), Green);
            arrow.rectTransform.pivot = new Vector2(0.5f, 0.5f);
            var dist = P.Text(mrt, "Distance", 24, Green, new Vector2(-40f, -62f), new Vector2(144f, 28f), TextAlignmentOptions.Center, P.mono);
            dist.rectTransform.anchorMin = dist.rectTransform.anchorMax = dist.rectTransform.pivot = new Vector2(0.5f, 0.5f);
            dist.rectTransform.anchoredPosition = new Vector2(0f, -58f);

            var ui = holder.gameObject.AddComponent<TutorialUI>();
            var so = new SerializedObject(ui);
            so.FindProperty("box").objectReferenceValue = box;
            so.FindProperty("title").objectReferenceValue = title;
            so.FindProperty("text").objectReferenceValue = text;
            so.FindProperty("marker").objectReferenceValue = mrt;
            so.FindProperty("arrow").objectReferenceValue = arrow.rectTransform;
            so.FindProperty("distance").objectReferenceValue = dist;
            so.FindProperty("frame").objectReferenceValue = boxFrame;
            so.ApplyModifiedPropertiesWithoutUndo();

            // Leuchtpfeile am Boden zum Ziel (TutorialPath)
            var path = holder.gameObject.AddComponent<TutorialPath>();
            var pso = new SerializedObject(path);
            pso.FindProperty("material").objectReferenceValue = LookDevKit.Additive("LD_TutorialArrow", "Arrow.png", new Color(0.35f, 1f, 0.4f, 0.9f), true);
            pso.ApplyModifiedPropertiesWithoutUndo();
        }

        /// <summary>Bild aus einer eigenen Textur (RawImage, ändert die Import-Einstellung der 3D-Textur nicht).</summary>
        static RawImage Raw(RectTransform parent, string name, string file, Vector2 anchorMin, Vector2 anchorMax, Vector2 size, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(RawImage));
            var rt = (RectTransform)go.transform;
            rt.SetParent(parent, false);
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.pivot = new Vector2(0.5f, anchorMin.y == anchorMax.y ? anchorMin.y : 0.5f);
            rt.sizeDelta = size;
            var img = go.GetComponent<RawImage>();
            img.texture = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/_Project/Art/Textures/LookDev/" + file);
            img.color = color;
            img.raycastTarget = false;
            return img;
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

        /// <summary>Bauchbinde des Staats-TV (GDD 11): Sender oben links, unten Kennung, Zeitzeile, Schlagzeile, Laufband, „Weiter“.</summary>
        static void BuildNews(Transform canvas)
        {
            var holder = Holder(canvas, "NewsOverlay");
            var root = new GameObject("Root", typeof(RectTransform));
            var rootRt = (RectTransform)root.transform;
            rootRt.SetParent(holder, false);
            P.Stretch(rootRt);

            var channel = P.Text(rootRt, "Channel", 26, Color.white, new Vector2(40f, -30f), new Vector2(900f, 40f), TextAlignmentOptions.Left);
            channel.characterSpacing = 4f;

            // Band unten
            var band = Anchored(rootRt, "Band", new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0f, 0f), new Vector2(0f, 250f), new Color(0.04f, 0.05f, 0.12f, 0.92f));
            band.pivot = new Vector2(0.5f, 0f);
            var tagBox = Anchored(band, "TagBox", new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(40f, -20f), new Vector2(420f, 64f), new Color(0.82f, 0.1f, 0.1f));
            tagBox.pivot = new Vector2(0f, 1f);
            var tag = P.Text(tagBox, "Tag", 38, Color.white, new Vector2(16f, -6f), new Vector2(390f, 52f), TextAlignmentOptions.Left);
            tag.enableAutoSizing = true;
            tag.fontSizeMin = 20f;
            tag.fontSizeMax = 38f;
            var year = P.Text(band, "Year", 22, P.Muted, new Vector2(480f, -38f), new Vector2(1000f, 30f), TextAlignmentOptions.Left, P.mono);
            var head = P.Text(band, "Head", 40, Gold, new Vector2(40f, -96f), new Vector2(1840f, 96f), TextAlignmentOptions.TopLeft);
            head.textWrappingMode = TMPro.TextWrappingModes.Normal;
            head.enableAutoSizing = true;
            head.fontSizeMin = 26f;
            head.fontSizeMax = 40f;

            // Laufband
            var strip = Anchored(band, "TickerStrip", new Vector2(0f, 0f), new Vector2(1f, 0f), Vector2.zero, new Vector2(0f, 48f), new Color(0.6f, 0.05f, 0.06f));
            strip.pivot = new Vector2(0.5f, 0f);
            strip.gameObject.AddComponent<RectMask2D>();
            var track = new GameObject("Track", typeof(RectTransform));
            var trackRt = (RectTransform)track.transform;
            trackRt.SetParent(strip, false);
            trackRt.anchorMin = trackRt.anchorMax = new Vector2(0f, 0.5f);
            trackRt.pivot = new Vector2(0f, 0.5f);
            trackRt.sizeDelta = new Vector2(4000f, 48f);
            var ticker = new GameObject("Ticker", typeof(RectTransform)).AddComponent<TextMeshProUGUI>();
            ticker.transform.SetParent(trackRt, false);
            P.Stretch(ticker.rectTransform);
            ticker.font = P.font;
            ticker.fontSize = 24;
            ticker.color = P.Cream;
            ticker.alignment = TextAlignmentOptions.MidlineLeft;
            ticker.textWrappingMode = TMPro.TextWrappingModes.NoWrap;
            ticker.raycastTarget = false;

            var skip = P.MakeButton(rootRt, "Skip", new Vector2(-40f, -30f), new Vector2(220f, 56f), ConfirmBg, ConfirmBorder, 24, false);
            holder.gameObject.AddComponent<NewsUI>().Setup(root, tag, head, year, channel, trackRt, ticker, skip, tagBox.GetComponent<Image>());
        }

        /// <summary>Bild mit frei gesetzten Ankern.</summary>
        static RectTransform Anchored(Transform parent, string name, Vector2 min, Vector2 max, Vector2 pos, Vector2 size, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            var rt = (RectTransform)go.transform;
            rt.SetParent(parent, false);
            rt.anchorMin = min;
            rt.anchorMax = max;
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;
            var img = go.GetComponent<Image>();
            img.color = color;
            img.raycastTarget = false;
            return rt;
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
