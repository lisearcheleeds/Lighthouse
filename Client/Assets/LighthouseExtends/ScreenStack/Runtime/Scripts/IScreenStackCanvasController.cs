namespace LighthouseExtends.ScreenStack
{
    public interface IScreenStackCanvasController
    {
        public void AddChild(IScreenStack screenStack, bool isSystemLayer);
        public void AddChild(IScreenStackBackgroundInputBlocker screenStackBackgroundInputBlocker, bool isSystemLayer);
    }
}