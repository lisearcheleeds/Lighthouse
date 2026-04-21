using UnityEngine;

namespace LighthouseExtends.ScreenStack
{
    public interface IScreenStackBackgroundInputBlocker
    {
        void Setup();
        void SetParent(Transform parent);
        void BlockScreenStackBackground(bool isSystem);
        void UnBlock();
    }
}