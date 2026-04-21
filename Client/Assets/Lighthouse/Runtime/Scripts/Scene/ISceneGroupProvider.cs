namespace Lighthouse.Scene
{
    /// <summary>
    /// Provides the SceneGroup associated with a given MainSceneId.
    /// Implement this to define the scene group configuration for the application.
    /// </summary>
    public interface ISceneGroupProvider
    {
        SceneGroup GetSceneGroup(MainSceneId mainSceneId);
    }
}