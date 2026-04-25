using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using Lighthouse;
using Lighthouse.Input;
using Lighthouse.Scene;
using UnityEngine;
using VContainer;

namespace LighthouseExtends.ScreenStack
{
    public class ScreenStackManager : IScreenStackManager
    {
        readonly IScreenStackCanvasController screenStackCanvasController;
        readonly IScreenStackEntityFactory screenStackEntityFactory;
        readonly IScreenStackBackgroundInputBlocker screenStackBackgroundInputBlocker;
        readonly IInputBlocker inputBlocker;

        readonly List<(MainSceneId SceneId, List<IScreenStackData> DataList)> screenStackDataSceneList = new();
        readonly List<ScreenStackEntity> screenStackEntityList = new();

        readonly Queue<Func<UniTask>> commandQueue = new();

        List<IScreenStackData> screenStackDataList;

        bool isProcessing;

        [Inject]
        public ScreenStackManager(
            IScreenStackCanvasController screenStackCanvasController,
            IScreenStackEntityFactory screenStackEntityFactory,
            IScreenStackBackgroundInputBlocker screenStackBackgroundInputBlocker,
            IInputBlocker inputBlocker)
        {
            this.screenStackCanvasController = screenStackCanvasController;
            this.screenStackEntityFactory = screenStackEntityFactory;
            this.screenStackBackgroundInputBlocker = screenStackBackgroundInputBlocker;
            this.inputBlocker = inputBlocker;
        }

        void IScreenStackManager.Setup()
        {
            screenStackBackgroundInputBlocker.Setup();
        }

        UniTask IScreenStackManager.Enqueue(IScreenStackData screenStackData)
        {
            return EnqueueCommand(() =>
            {
                EnqueueScreenStackCore(screenStackData);
                return UniTask.CompletedTask;
            });
        }

        UniTask IScreenStackManager.Open()
        {
            return EnqueueCommand(() => OpenScreenStackCore(true));
        }

        UniTask IScreenStackManager.Open(IScreenStackData screenStackData)
        {
            return EnqueueCommand(async () =>
            {
                EnqueueScreenStackCore(screenStackData);
                await OpenScreenStackCore(true);
            });
        }

        UniTask IScreenStackManager.Close(IScreenStackData screenStackData)
        {
            return EnqueueCommand(() => CloseScreenStackCore(screenStackData));
        }

        UniTask IScreenStackManager.Close()
        {
            return EnqueueCommand(() => CloseScreenStackCore());
        }

        UniTask IScreenStackManager.ClearAll()
        {
            return EnqueueCommand(() => ClearAllScreenStackCore());
        }

        UniTask IScreenStackManager.ClearCurrentAll()
        {
            return EnqueueCommand(() => ClearCurrentAllScreenStackCore());
        }

        UniTask IScreenStackManager.ResumeFromSceneId(MainSceneId mainSceneId, bool isPlayInAnimation)
        {
            return EnqueueCommand(async () =>
            {
                ResumeScreenStackFromSceneIdCore(mainSceneId);

                if (screenStackDataList?.Any() ?? false)
                {
                    await ResumeOpenScreenStacksCore(isPlayInAnimation);
                }
            });
        }

        UniTask IScreenStackManager.SuspendFromSceneId(MainSceneId mainSceneId)
        {
            return EnqueueCommand(async () =>
            {
                SuspendScreenStackFromSceneIdCore(mainSceneId);
                await ClearCurrentAllScreenStackCore();
            });
        }

        UniTask EnqueueCommand(Func<UniTask> action)
        {
            var tcs = new UniTaskCompletionSource();

            commandQueue.Enqueue(async () =>
            {
                try
                {
                    await action();
                    tcs.TrySetResult();
                }
                catch (OperationCanceledException ex)
                {
                    tcs.TrySetException(ex);
                }
                catch (Exception ex)
                {
                    // State consistency cannot be guaranteed after an unexpected exception.
                    // Clear the queue and force-dispose all entities so callers can reboot cleanly.
                    LHLogger.LogError($"[ScreenStackManager] Unhandled exception in command. Force disposing all stacks.\n{ex}");
                    commandQueue.Clear();
                    ForceDisposeAll();
                    tcs.TrySetException(ex);
                }
            });

            if (!isProcessing)
            {
                isProcessing = true;
                CommandProcessLoop().Forget();
            }

            return tcs.Task;
        }

        async UniTaskVoid CommandProcessLoop()
        {
            try
            {
                inputBlocker.Block<ScreenStackManager>();

                while (commandQueue.Count > 0)
                {
                    await commandQueue.Dequeue()();
                }
            }
            finally
            {
                inputBlocker.UnBlock<ScreenStackManager>();
                isProcessing = false;
            }
        }

        void EnqueueScreenStackCore(IScreenStackData screenStackData)
        {
            if (screenStackDataList == null)
            {
                screenStackDataList = new List<IScreenStackData>();
            }

            screenStackDataList.Add(screenStackData);
        }

        protected virtual async UniTask ResumeOpenScreenStacksCore(bool isPlayInAnimation)
        {
            try
            {
                for (var i = 0; i < screenStackDataList.Count; i++)
                {
                    var screenStackData = screenStackDataList[i];
                    var shouldPlayAnimation = isPlayInAnimation && i == screenStackDataList.Count - 1;

                    var prevScreenStackEntity = screenStackEntityList.LastOrDefault();
                    if (prevScreenStackEntity?.ScreenStackData == screenStackData)
                    {
                        throw new InvalidOperationException($"Duplicate open");
                    }

                    var screenStackEntity = await screenStackEntityFactory.CreateAsync(screenStackData, CancellationToken.None);
                    if (shouldPlayAnimation)
                    {
                        screenStackEntity.ScreenStack.ResetInAnimation();
                    }
                    else
                    {
                        screenStackEntity.ScreenStack.EndInAnimation();
                    }

                    screenStackEntityList.Add(screenStackEntity);
                    screenStackCanvasController.AddChild(screenStackEntity.ScreenStack, screenStackData.IsSystem);

                    await screenStackEntity.ScreenStack.OnInitialize();

                    if (prevScreenStackEntity != null)
                    {
                        if (!screenStackData.IsOverlayOpen)
                        {
                            prevScreenStackEntity.ScreenStack.EndOutAnimation();
                        }

                        await prevScreenStackEntity.ScreenStack.OnLeave();
                    }

                    await screenStackEntity.ScreenStack.OnEnter(false);

                    if (shouldPlayAnimation)
                    {
                        await screenStackEntity.ScreenStack.PlayInAnimation();
                    }

                    screenStackBackgroundInputBlocker.BlockScreenStackBackground(screenStackData.IsSystem);
                }
            }
            catch (Exception)
            {
                await ClearCurrentAllScreenStackCore();
                throw;
            }
        }

        protected virtual async UniTask OpenScreenStackCore(bool isPlayInAnimation)
        {
            var screenStackData = screenStackDataList?.LastOrDefault();
            if (screenStackData == null)
            {
                throw new InvalidOperationException($"Empty screenStack data");
            }

            var prevScreenStackEntity = screenStackEntityList.LastOrDefault();
            if (prevScreenStackEntity?.ScreenStackData == screenStackData)
            {
                throw new InvalidOperationException($"Duplicate open");
            }

            var screenStackEntity = await screenStackEntityFactory.CreateAsync(screenStackData, CancellationToken.None);

            if (isPlayInAnimation)
            {
                screenStackEntity.ScreenStack.ResetInAnimation();
            }
            else
            {
                screenStackEntity.ScreenStack.EndInAnimation();
            }

            screenStackEntityList.Add(screenStackEntity);
            screenStackCanvasController.AddChild(screenStackEntity.ScreenStack, screenStackData.IsSystem);

            try
            {
                await screenStackEntity.ScreenStack.OnInitialize();

                if (prevScreenStackEntity != null)
                {
                    if (!screenStackData.IsOverlayOpen)
                    {
                        await prevScreenStackEntity.ScreenStack.PlayOutAnimation();
                    }

                    await prevScreenStackEntity.ScreenStack.OnLeave();
                }

                await screenStackEntity.ScreenStack.OnEnter(false);

                if (isPlayInAnimation)
                {
                    await screenStackEntity.ScreenStack.PlayInAnimation();
                }

                screenStackBackgroundInputBlocker.BlockScreenStackBackground(screenStackData.IsSystem);
            }
            catch
            {
                screenStackEntityList.Remove(screenStackEntity);
                screenStackEntity.ScreenStack.Dispose();
                if (prevScreenStackEntity != null && !screenStackData.IsOverlayOpen)
                {
                    prevScreenStackEntity.ScreenStack.EndInAnimation();
                }

                throw;
            }
        }

        protected virtual async UniTask CloseScreenStackCore(IScreenStackData screenStackData)
        {
            if (screenStackDataList == null || !screenStackDataList.Remove(screenStackData))
            {
                foreach (var screenStackDataScene in screenStackDataSceneList)
                {
                    if (screenStackDataScene.Item2.Remove(screenStackData))
                    {
                        break;
                    }
                }

                return;
            }

            if (!(screenStackEntityList?.Any() ?? false))
            {
                return;
            }

            // If there is a screenStack view, remove it too.
            var target = screenStackEntityList.FirstOrDefault(x => ReferenceEquals(x.ScreenStackData, screenStackData));
            if (target == null)
            {
                return;
            }

            var isLast = ReferenceEquals(target, screenStackEntityList[^1]);
            screenStackEntityList.Remove(target);

            try
            {
                await target.ScreenStack.PlayOutAnimation();
                await target.ScreenStack.OnLeave();
            }
            finally
            {
                target.ScreenStack.Dispose();
            }

            await UniTask.DelayFrame(1);

            if (!isLast)
            {
                return;
            }

            var prevScreenStack = screenStackEntityList.LastOrDefault();
            if (prevScreenStack == null)
            {
                screenStackBackgroundInputBlocker.UnBlock();
                return;
            }

            try
            {
                await prevScreenStack.ScreenStack.OnEnter(true);

                if (!screenStackData.IsOverlayOpen)
                {
                    await prevScreenStack.ScreenStack.PlayInAnimation();
                }
            }
            catch
            {
                prevScreenStack.ScreenStack.EndInAnimation();
                throw;
            }
            finally
            {
                screenStackBackgroundInputBlocker.BlockScreenStackBackground(prevScreenStack.ScreenStackData.IsSystem);
            }
        }

        protected virtual async UniTask CloseScreenStackCore()
        {
            var lastScreenStackData = screenStackDataList?.LastOrDefault();
            if (lastScreenStackData == null || !screenStackDataList.Remove(lastScreenStackData))
            {
                return;
            }

            var currentScreenStack = screenStackEntityList.LastOrDefault();
            if (currentScreenStack == null)
            {
                return;
            }

            screenStackEntityList.Remove(currentScreenStack);

            try
            {
                await currentScreenStack.ScreenStack.PlayOutAnimation();
                await currentScreenStack.ScreenStack.OnLeave();
            }
            finally
            {
                currentScreenStack.ScreenStack.Dispose();
            }

            await UniTask.DelayFrame(1);

            var prevScreenStack = screenStackEntityList.LastOrDefault();
            if (prevScreenStack == null)
            {
                screenStackBackgroundInputBlocker.UnBlock();
                return;
            }

            try
            {
                await prevScreenStack.ScreenStack.OnEnter(true);

                if (!lastScreenStackData.IsOverlayOpen)
                {
                    await prevScreenStack.ScreenStack.PlayInAnimation();
                }
            }
            catch
            {
                prevScreenStack.ScreenStack.EndInAnimation();
                throw;
            }
            finally
            {
                screenStackBackgroundInputBlocker.BlockScreenStackBackground(prevScreenStack.ScreenStackData.IsSystem);
            }
        }

        protected virtual UniTask ClearAllScreenStackCore()
        {
            screenStackDataSceneList.Clear();
            return ClearCurrentAllScreenStackCore();
        }

        protected virtual async UniTask ClearCurrentAllScreenStackCore()
        {
            screenStackDataList?.Clear();

            var lastTarget = screenStackEntityList.LastOrDefault();
            while (0 < screenStackEntityList.Count)
            {
                var target = screenStackEntityList[^1];
                var isLast = ReferenceEquals(target, lastTarget);
                screenStackEntityList.RemoveAt(screenStackEntityList.Count - 1);

                try
                {
                    if (isLast || target.ScreenStackData.IsOverlayOpen)
                    {
                        await target.ScreenStack.PlayOutAnimation();
                    }

                    await target.ScreenStack.OnLeave();
                }
                finally
                {
                    target.ScreenStack.Dispose();
                }
            }

            screenStackBackgroundInputBlocker.UnBlock();
            screenStackEntityList.Clear();
        }

        protected virtual void ResumeScreenStackFromSceneIdCore(MainSceneId mainSceneId)
        {
            if (screenStackDataSceneList.All(x => x.SceneId != mainSceneId))
            {
                Debug.LogWarning($"[ScreenStackManager] ResumeFromSceneId: SceneId '{mainSceneId}' not found in suspended stack.");
                return;
            }

            var (lastMainSceneId, lastScreenStackDataList) = screenStackDataSceneList[^1];

            while (lastMainSceneId != mainSceneId)
            {
                screenStackDataSceneList.RemoveAt(screenStackDataSceneList.Count - 1);
                (lastMainSceneId, lastScreenStackDataList) = screenStackDataSceneList[^1];
            }

            screenStackDataSceneList.RemoveAt(screenStackDataSceneList.Count - 1);

            screenStackDataList = lastScreenStackDataList;
        }

        protected virtual void ForceDisposeAll()
        {
            screenStackDataList?.Clear();
            screenStackDataSceneList.Clear();

            foreach (var entity in screenStackEntityList)
            {
                entity.ScreenStack.Dispose();
            }

            screenStackEntityList.Clear();
            screenStackBackgroundInputBlocker.UnBlock();
        }

        protected virtual void SuspendScreenStackFromSceneIdCore(MainSceneId mainSceneId)
        {
            screenStackDataSceneList.Add((mainSceneId, screenStackDataList?.ToList() ?? new List<IScreenStackData>()));
        }
    }
}
