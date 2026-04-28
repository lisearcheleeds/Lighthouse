using System;
using System.Collections.Generic;
using System.Linq;
using Lighthouse.Input;
using Lighthouse.Scene.SceneCamera;
using R3;
using UnityEngine;
using VContainer;

namespace LighthouseExtends.UIComponent.InputBlocker
{
    public class LHInputBlocker : MonoBehaviour, IInputBlocker
    {
        [SerializeField] Canvas blockerCanvas;
        [SerializeField] int systemBlockSortingOrder;
        [SerializeField] int blockSortingOrder;
        [SerializeField] GameObject blockerObject;

        Dictionary<Type, int> blockCountDic = new();
        Dictionary<Type, int> systemBlockCount = new();

        [Inject]
        public void Construct(ISceneCameraManager sceneCameraManager)
        {
            blockerCanvas.worldCamera = sceneCameraManager.UICamera.GetCamera();
        }

        IDisposable IInputBlocker.Block<T>(bool isSystemLayer)
        {
            var ownerType = typeof(T);

            if (isSystemLayer)
            {
                if (systemBlockCount.TryGetValue(ownerType, out var value))
                {
                    systemBlockCount[ownerType] = value + 1;
                }
                else
                {
                    systemBlockCount.Add(ownerType, 1);
                }
            }
            else
            {
                if (blockCountDic.TryGetValue(ownerType, out var value))
                {
                    blockCountDic[ownerType] = value + 1;
                }
                else
                {
                    blockCountDic.Add(ownerType, 1);
                }
            }

            UpdateBlock();

            return Disposable.Create(() => ((IInputBlocker)this).UnBlock<T>(isSystemLayer));
        }

        void IInputBlocker.UnBlock<T>(bool isSystemLayer)
        {
            var ownerType = typeof(T);

            if (isSystemLayer)
            {
                if (systemBlockCount.TryGetValue(ownerType, out var value))
                {
                    if (value == 1)
                    {
                        systemBlockCount.Remove(ownerType);
                    }
                    else
                    {
                        systemBlockCount[ownerType]--;
                    }
                }
            }
            else
            {
                if (blockCountDic.TryGetValue(ownerType, out var value))
                {
                    if (value == 1)
                    {
                        blockCountDic.Remove(ownerType);
                    }
                    else
                    {
                        blockCountDic[ownerType]--;
                    }
                }
            }

            UpdateBlock();
        }

        void UpdateBlock()
        {
            if (systemBlockCount.Any())
            {
                blockerCanvas.sortingOrder = systemBlockSortingOrder;
                blockerObject.gameObject.SetActive(true);
            }
            else if (blockCountDic.Any())
            {
                blockerCanvas.sortingOrder = blockSortingOrder;
                blockerObject.gameObject.SetActive(true);
            }
            else
            {
                blockerObject.gameObject.SetActive(false);
            }
        }
    }
}