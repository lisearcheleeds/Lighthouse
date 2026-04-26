using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace LighthouseExtends.ScreenStack
{
    public abstract class ScreenStackLifetimeScopeBase : LifetimeScope
    {
        [SerializeField] ScreenStackModuleSceneBase screenStackModuleSceneBase;
        [SerializeField] ScreenStackCanvasController screenStackCanvasController;

        protected ScreenStackModuleSceneBase ScreenStackModuleSceneBase => screenStackModuleSceneBase;
        protected ScreenStackCanvasController ScreenStackCanvasController => screenStackCanvasController;

        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterEntryPoint<ScreenStackEntryPoint>();
            builder.RegisterComponent(ScreenStackModuleSceneBase);
            builder.RegisterComponent(ScreenStackCanvasController).AsImplementedInterfaces();

            builder.Register<ScreenStackModule>(Lifetime.Singleton).AsImplementedInterfaces();
            builder.Register<ScreenStackManager>(Lifetime.Singleton).AsImplementedInterfaces();
        }
    }
}
