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
        protected List<(MainSceneId SceneId, List<IScreenStackData> DataList)> ScreenStackDataSceneList { get; } = new();
        protected List<ScreenStackEntity> ScreenStackEntityList { get; } = new();

        protected Queue<Func<UniTask>> CommandQueue { get; } = new();

        protected IScreenStackCanvasController ScreenStackCanvasController { get; private set; }
        protected IScreenStackEntityFactory ScreenStackEntityFactory { get; private set; }
        protected IScreenStackBackgroundInputBlocker ScreenStackBackgroundInputBlocker { get; private set; }
        protected IInputBlocker InputBlocker { get; private set; }

        protected List<IScreenStackData> ScreenStackDataList { get; set; }

        protected bool IsProcessing { get; set; }

        [Inject]
        public void Construct(
            IScreenStackCanvasController screenStackCanvasController,
            IScreenStackEntityFactory screenStackEntityFactory,
            IScreenStackBackgroundInputBlocker screenStackBackgroundInputBlocker,
            IInputBlocker inputBlocker)
        {
            this.ScreenStackCanvasController = screenStackCanvasController;
            this.ScreenStackEntityFactory = screenStackEntityFactory;
            this.ScreenStackBackgroundInputBlocker = screenStackBackgroundInputBlocker;
            this.InputBlocker = inputBlocker;
        }

        void IScreenStackManager.Setup()
        {
            ScreenStackBackgroundInputBlocker.Setup();
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

                if (ScreenStackDataList?.Any() ?? false)
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

            CommandQueue.Enqueue(async () =>
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
                    CommandQueue.Clear();
                    ForceDisposeAll();
                    tcs.TrySetException(ex);
                }
            });

            if (!IsProcessing)
            {
                IsProcessing = true;
                CommandProcessLoop().Forget();
            }

            return tcs.Task;
        }

        async UniTaskVoid CommandProcessLoop()
        {
            try
            {
                InputBlocker.Block<ScreenStackManager>();

                while (CommandQueue.Count > 0)
                {
                    await CommandQueue.Dequeue()();
                }
            }
            finally
            {
                InputBlocker.UnBlock<ScreenStackManager>();
                IsProcessing = false;
            }
        }

        void EnqueueScreenStackCore(IScreenStackData screenStackData)
        {
            if (ScreenStackDataList == null)
            {
                ScreenStackDataList = new List<IScreenStackData>();
            }

            ScreenStackDataList.Add(screenStackData);
        }

        protected virtual async UniTask ResumeOpenScreenStacksCore(bool isPlayInAnimation)
        {
            try
            {
                for (var i = 0; i < ScreenStackDataList.Count; i++)
                {
                    var screenStackData = ScreenStackDataList[i];
                    var shouldPlayAnimation = isPlayInAnimation && i == ScreenStackDataList.Count - 1;

                    var prevScreenStackEntity = ScreenStackEntityList.LastOrDefault();
                    if (prevScreenStackEntity?.ScreenStackData == screenStackData)
                    {
                        throw new InvalidOperationException($"Duplicate open");
                    }

                    var screenStackEntity = await ScreenStackEntityFactory.CreateAsync(screenStackData, CancellationToken.None);
                    if (shouldPlayAnimation)
                    {
                        screenStackEntity.ScreenStack.ResetInAnimation();
                    }
                    else
                    {
                        screenStackEntity.ScreenStack.EndInAnimation();
                    }

                    ScreenStackEntityList.Add(screenStackEntity);
                    ScreenStackCanvasController.AddChild(screenStackEntity.ScreenStack, screenStackData.IsSystem);

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

                    ScreenStackBackgroundInputBlocker.BlockScreenStackBackground(screenStackData.IsSystem);
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
            var screenStackData = ScreenStackDataList?.LastOrDefault();
            if (screenStackData == null)
            {
                throw new InvalidOperationException($"Empty screenStack data");
            }

            var prevScreenStackEntity = ScreenStackEntityList.LastOrDefault();
            if (prevScreenStackEntity?.ScreenStackData == screenStackData)
            {
                throw new InvalidOperationException($"Duplicate open");
            }

            var screenStackEntity = await ScreenStackEntityFactory.CreateAsync(screenStackData, CancellationToken.None);

            if (isPlayInAnimation)
            {
                screenStackEntity.ScreenStack.ResetInAnimation();
            }
            else
            {
                screenStackEntity.ScreenStack.EndInAnimation();
            }

            ScreenStackEntityList.Add(screenStackEntity);
            ScreenStackCanvasController.AddChild(screenStackEntity.ScreenStack, screenStackData.IsSystem);

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

                ScreenStackBackgroundInputBlocker.BlockScreenStackBackground(screenStackData.IsSystem);
            }
            catch
            {
                ScreenStackEntityList.Remove(screenStackEntity);
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
            if (ScreenStackDataList == null || !ScreenStackDataList.Remove(screenStackData))
            {
                foreach (var screenStackDataScene in ScreenStackDataSceneList)
                {
                    if (screenStackDataScene.Item2.Remove(screenStackData))
                    {
                        break;
                    }
                }

                return;
            }

            if (!(ScreenStackEntityList?.Any() ?? false))
            {
                return;
            }

            // If there is a screenStack view, remove it too.
            var target = ScreenStackEntityList.FirstOrDefault(x => ReferenceEquals(x.ScreenStackData, screenStackData));
            if (target == null)
            {
                return;
            }

            var isLast = ReferenceEquals(target, ScreenStackEntityList[^1]);
            ScreenStackEntityList.Remove(target);

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

            var prevScreenStack = ScreenStackEntityList.LastOrDefault();
            if (prevScreenStack == null)
            {
                ScreenStackBackgroundInputBlocker.UnBlock();
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
                ScreenStackBackgroundInputBlocker.BlockScreenStackBackground(prevScreenStack.ScreenStackData.IsSystem);
            }
        }

        protected virtual async UniTask CloseScreenStackCore()
        {
            var lastScreenStackData = ScreenStackDataList?.LastOrDefault();
            if (lastScreenStackData == null || !ScreenStackDataList.Remove(lastScreenStackData))
            {
                return;
            }

            var currentScreenStack = ScreenStackEntityList.LastOrDefault();
            if (currentScreenStack == null)
            {
                return;
            }

            ScreenStackEntityList.Remove(currentScreenStack);

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

            var prevScreenStack = ScreenStackEntityList.LastOrDefault();
            if (prevScreenStack == null)
            {
                ScreenStackBackgroundInputBlocker.UnBlock();
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
                ScreenStackBackgroundInputBlocker.BlockScreenStackBackground(prevScreenStack.ScreenStackData.IsSystem);
            }
        }

        protected virtual UniTask ClearAllScreenStackCore()
        {
            ScreenStackDataSceneList.Clear();
            return ClearCurrentAllScreenStackCore();
        }

        protected virtual async UniTask ClearCurrentAllScreenStackCore()
        {
            ScreenStackDataList?.Clear();

            var lastTarget = ScreenStackEntityList.LastOrDefault();
            while (0 < ScreenStackEntityList.Count)
            {
                var target = ScreenStackEntityList[^1];
                var isLast = ReferenceEquals(target, lastTarget);
                ScreenStackEntityList.RemoveAt(ScreenStackEntityList.Count - 1);

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

            ScreenStackBackgroundInputBlocker.UnBlock();
            ScreenStackEntityList.Clear();
        }

        protected virtual void ResumeScreenStackFromSceneIdCore(MainSceneId mainSceneId)
        {
            if (ScreenStackDataSceneList.All(x => x.SceneId != mainSceneId))
            {
                Debug.LogWarning($"[ScreenStackManager] ResumeFromSceneId: SceneId '{mainSceneId}' not found in suspended stack.");
                return;
            }

            var (lastMainSceneId, lastScreenStackDataList) = ScreenStackDataSceneList[^1];

            while (lastMainSceneId != mainSceneId)
            {
                ScreenStackDataSceneList.RemoveAt(ScreenStackDataSceneList.Count - 1);
                (lastMainSceneId, lastScreenStackDataList) = ScreenStackDataSceneList[^1];
            }

            ScreenStackDataSceneList.RemoveAt(ScreenStackDataSceneList.Count - 1);

            ScreenStackDataList = lastScreenStackDataList;
        }

        protected virtual void ForceDisposeAll()
        {
            ScreenStackDataList?.Clear();
            ScreenStackDataSceneList.Clear();

            foreach (var entity in ScreenStackEntityList)
            {
                entity.ScreenStack.Dispose();
            }

            ScreenStackEntityList.Clear();
            ScreenStackBackgroundInputBlocker.UnBlock();
        }

        protected virtual void SuspendScreenStackFromSceneIdCore(MainSceneId mainSceneId)
        {
            ScreenStackDataSceneList.Add((mainSceneId, ScreenStackDataList?.ToList() ?? new List<IScreenStackData>()));
        }
    }
}
