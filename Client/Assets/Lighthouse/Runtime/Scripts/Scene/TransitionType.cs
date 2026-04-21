namespace Lighthouse.Scene
{
    /// <summary>
    /// Specifies how scenes are swapped during a transition.
    /// Auto resolves to Cross when the destination is within the same group, otherwise Exclusive.
    /// </summary>
    public enum TransitionType
    {
        Auto,
        Exclusive,
        Cross,
    }
}