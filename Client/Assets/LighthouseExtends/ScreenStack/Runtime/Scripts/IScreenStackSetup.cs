namespace LighthouseExtends.ScreenStack
{
    public interface IScreenStackSetup<in TScreenStackData>
    {
        void Setup(TScreenStackData screenStackData);
    }
}
