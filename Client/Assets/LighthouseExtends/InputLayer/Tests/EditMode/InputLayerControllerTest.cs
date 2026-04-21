using System;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;

namespace LighthouseExtends.InputLayer.Tests.EditMode
{
    public class InputLayerControllerTest
    {
        InputActionAsset asset;
        IInputLayerController controller;

        [SetUp]
        public void SetUp()
        {
            asset = ScriptableObject.CreateInstance<InputActionAsset>();
            controller = new InputLayerController(asset);
        }

        [TearDown]
        public void TearDown()
        {
            controller.Dispose();
            UnityEngine.Object.DestroyImmediate(asset);
        }

        [Test]
        public void PushLayer_EnablesActionMap()
        {
            var map = CreateMap("Test");

            controller.PushLayer(new StubInputLayer(), map);

            Assert.IsTrue(map.enabled);
        }

        [Test]
        public void PopLayer_EmptyStack_DoesNotThrow()
        {
            Assert.DoesNotThrow(() => controller.PopLayer());
        }

        [Test]
        public void PopLayer_DisablesMap_WhenNoRemainingLayerReferencesIt()
        {
            var map = CreateMap("Test");
            controller.PushLayer(new StubInputLayer(), map);

            controller.PopLayer();

            Assert.IsFalse(map.enabled);
        }

        [Test]
        public void PopLayer_DoesNotDisableMap_WhenAnotherLayerStillUsesIt()
        {
            var map = CreateMap("Shared");
            controller.PushLayer(new StubInputLayer(), map);
            controller.PushLayer(new StubInputLayer(), map);

            controller.PopLayer();

            Assert.IsTrue(map.enabled);
        }

        [Test]
        public void PopLayer_Target_RemovesSpecificLayer()
        {
            var mapA = CreateMap("A");
            var mapB = CreateMap("B");
            var layerA = new StubInputLayer();

            controller.PushLayer(layerA, mapA);
            controller.PushLayer(new StubInputLayer(), mapB);

            controller.PopLayer(layerA);

            Assert.IsFalse(mapA.enabled);
            Assert.IsTrue(mapB.enabled);
        }

        [Test]
        public void PopLayer_Target_NotInStack_DoesNothing()
        {
            var map = CreateMap("Test");
            var layer = new StubInputLayer();
            controller.PushLayer(layer, map);

            Assert.DoesNotThrow(() => controller.PopLayer(new StubInputLayer()));
            Assert.IsTrue(map.enabled);
        }

        [Test]
        public void PushLayer_NullLayer_Throws()
        {
            var map = CreateMap("Test");

            Assert.Throws<ArgumentException>(() => controller.PushLayer(null, map));
        }

        // InputActionMap.enabled checks the count of enabled actions internally,
        // so a map with no actions stays false even after Enable(). Add one action.
        static InputActionMap CreateMap(string name)
        {
            var map = new InputActionMap(name);
            map.AddAction("action");
            return map;
        }

        class StubInputLayer : IInputLayer
        {
            public bool BlocksAllInput => false;
            public bool OnActionStarted(InputAction.CallbackContext ctx) => false;
            public bool OnActionPerformed(InputAction.CallbackContext ctx) => false;
            public bool OnActionCanceled(InputAction.CallbackContext ctx) => false;
        }
    }
}
