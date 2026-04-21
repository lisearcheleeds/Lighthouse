namespace LighthouseExtends.UIComponent.ExclusiveInput
{
    public interface IExclusiveInputService
    {
        bool TryUsePointerId(int pointerId);
        void ReleasePointerId(int pointerId);
    }
}