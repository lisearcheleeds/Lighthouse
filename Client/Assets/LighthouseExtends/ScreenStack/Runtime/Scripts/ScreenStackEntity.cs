namespace LighthouseExtends.ScreenStack
{
    public sealed class ScreenStackEntity
    {
        public IScreenStack ScreenStack { get; }
        public IScreenStackData ScreenStackData { get; }

        public ScreenStackEntity(IScreenStack screenStack, IScreenStackData screenStackData)
        {
            ScreenStack = screenStack;
            ScreenStackData = screenStackData;
        }
    }
}
