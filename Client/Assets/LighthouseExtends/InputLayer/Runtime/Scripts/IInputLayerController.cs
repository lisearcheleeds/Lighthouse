using System;
using UnityEngine.InputSystem;

namespace LighthouseExtends.InputLayer
{
    /// <summary>
    /// Manages the input layer stack. PopLayer(target) allows removal of a specific layer
    /// without requiring it to be at the top of the stack.
    /// </summary>
    public interface IInputLayerController : IDisposable
    {
        void PushLayer(IInputLayer layer, InputActionMap actionMap);
        void PopLayer();
        void PopLayer(IInputLayer target);
    }
}
