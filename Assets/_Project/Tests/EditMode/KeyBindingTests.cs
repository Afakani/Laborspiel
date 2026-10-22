using NUnit.Framework;
using UnityEngine.InputSystem;

namespace PlanErfuellt.Tests
{
    /// <summary>Tastenbelegung: jede belegbare Taste aus settings.rebind hat eine Tastatur-Bindung und einen Namen.</summary>
    public class KeyBindingTests
    {
        [Test]
        public void EveryRebindEntryFindsAKeyboardBinding()
        {
            var cfg = TestData.Config();
            var t = TestData.Texts();
            Assert.IsNotNull(InputSystem.actions, "projektweite Input Actions gesetzt");
            Assert.Greater(cfg.settings.rebind.Length, 5);
            foreach (var r in cfg.settings.rebind)
            {
                var a = InputSystem.actions.FindAction(r.action);
                Assert.IsNotNull(a, r.action);
                Assert.GreaterOrEqual(KeyBindings.BindingIndex(a, r.part), 0, r.id);
                Assert.IsFalse(t.Get("ui.settings.keyNames." + r.id).StartsWith("ui."), r.id);
            }
        }

        [Test]
        public void MoveUpIsTheWasdPart()
        {
            var a = InputSystem.actions.FindAction("Player/Move");
            int i = KeyBindings.BindingIndex(a, "up");
            Assert.AreEqual("<Keyboard>/w", a.bindings[i].path);
            Assert.AreEqual("<Keyboard>/f", InputSystem.actions.FindAction("Player/Interact").bindings[KeyBindings.BindingIndex(InputSystem.actions.FindAction("Player/Interact"), null)].path);
        }
    }
}
