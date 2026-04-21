using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using VContainer;

namespace LighthouseExtends.InputLayer
{
    /// <summary>
    /// Implements IInputLayerController. Subscribes to all actions in the InputActionAsset and dispatches
    /// from the top of the stack downward, stopping on consumption or BlocksAllInput.
    /// An ActionMap is disabled on Pop only when no remaining layer references it.
    /// </summary>
    public class InputLayerController : IInputLayerController, IDisposable
    {
        readonly InputActionAsset inputActionAsset;

        readonly List<LayerEntry> layerStack = new();

        class LayerEntry
        {
            public IInputLayer Layer { get; }
            public InputActionMap ActionMap { get; }

            public LayerEntry(IInputLayer layer, InputActionMap actionMap)
            {
                Layer = layer;
                ActionMap = actionMap;
            }
        }

        [Inject]
        public InputLayerController(InputActionAsset inputActionAsset)
        {
            this.inputActionAsset = inputActionAsset;

            foreach (var map in inputActionAsset.actionMaps)
            {
                foreach (var action in map.actions)
                {
                    action.started += OnActionStarted;
                    action.performed += OnActionPerformed;
                    action.canceled += OnActionCanceled;
                }
            }
        }

        void IInputLayerController.PushLayer(IInputLayer layer, InputActionMap actionMap)
        {
            if (layer == null || actionMap == null)
            {
                throw new ArgumentException($"[InputLayer] layer or actionMap is null");
            }

            layerStack.Insert(0, new LayerEntry(layer, actionMap));
            actionMap.Enable();
        }

        void IInputLayerController.PopLayer()
        {
            if (layerStack.Count == 0)
            {
                return;
            }

            var removedMap = layerStack[0].ActionMap;
            layerStack.RemoveAt(0);

            if (layerStack.All(e => e.ActionMap != removedMap))
            {
                removedMap.Disable();
            }
        }

        void IInputLayerController.PopLayer(IInputLayer target)
        {
            var index = layerStack.FindIndex(e => e.Layer == target);
            if (index < 0)
            {
                return;
            }

            var removedMap = layerStack[index].ActionMap;
            layerStack.RemoveAt(index);

            if (layerStack.All(e => e.ActionMap != removedMap))
            {
                removedMap.Disable();
            }
        }

        void IDisposable.Dispose()
        {
            foreach (var map in inputActionAsset.actionMaps)
            {
                foreach (var action in map.actions)
                {
                    action.started -= OnActionStarted;
                    action.performed -= OnActionPerformed;
                    action.canceled -= OnActionCanceled;
                }
            }
        }

        void OnActionStarted(InputAction.CallbackContext ctx)
        {
            DispatchAction(ctx, (layer, c) => layer.OnActionStarted(c));
        }

        void OnActionPerformed(InputAction.CallbackContext ctx)
        {
            DispatchAction(ctx, (layer, c) => layer.OnActionPerformed(c));
        }

        void OnActionCanceled(InputAction.CallbackContext ctx)
        {
            DispatchAction(ctx, (layer, c) => layer.OnActionCanceled(c));
        }

        void DispatchAction(InputAction.CallbackContext ctx, Func<IInputLayer, InputAction.CallbackContext, bool> handler)
        {
            foreach (var entry in layerStack)
            {
                bool consumed;
                try
                {
                    consumed = handler(entry.Layer, ctx);
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                    consumed = false;
                }

                if (consumed || entry.Layer.BlocksAllInput)
                {
                    break;
                }
            }
        }
    }
}
