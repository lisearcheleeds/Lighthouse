using Lighthouse.Scene.SceneCamera;
using UnityEngine;
using UnityEngine.EventSystems;

namespace LighthouseExtends.UIComponent.CanvasSceneObject
{
    public class LHCanvasSceneObject : MonoBehaviour, ICanvasSceneObject
    {
        [SerializeField] SceneCamera uiCamera;
        [SerializeField] EventSystem uiEventSystem;

        public ISceneCamera UICamera => uiCamera;
        public EventSystem UIEventSystem => uiEventSystem;
    }
}