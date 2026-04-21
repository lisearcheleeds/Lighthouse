using UnityEngine.InputSystem;

namespace LighthouseExtends.InputLayer
{
    /// <summary>
    /// Represents a single layer in the input stack. Returning true from a callback marks the input as consumed.
    /// BlocksAllInput prevents propagation to lower layers regardless of the return value.
    /// </summary>
    public interface IInputLayer
    {
        bool BlocksAllInput { get; }

        bool OnActionStarted(InputAction.CallbackContext callbackContext);

        bool OnActionPerformed(InputAction.CallbackContext callbackContext);

        bool OnActionCanceled(InputAction.CallbackContext callbackContext);
    }
}
