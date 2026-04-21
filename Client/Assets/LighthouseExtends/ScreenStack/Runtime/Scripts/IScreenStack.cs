using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace LighthouseExtends.ScreenStack
{
    public interface IScreenStack : IDisposable
    {
        void SetParent(Transform parentTransform);

        UniTask OnInitialize();
        UniTask OnEnter(bool isResume);
        UniTask OnLeave();

        void ResetInAnimation();
        UniTask PlayInAnimation();
        void EndInAnimation();
        void ResetOutAnimation();
        UniTask PlayOutAnimation();
        void EndOutAnimation();
    }
}