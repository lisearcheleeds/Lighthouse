namespace Lighthouse.Editor.ScriptGenerator
{
    public class GenerateBaseClassInfo
    {
        public string TypeName { get; set; }
        public string Namespace { get; set; }
        public bool IsGeneric { get; set; }

        public string DropdownLabel
        {
            get
            {
                return IsGeneric ? $"{TypeName}<T>  ({Namespace})" : $"{TypeName}  ({Namespace})";
            }
        }

        public string GetBaseClassExpression(string sceneFileName, string sceneName)
        {
            return IsGeneric ? $"{TypeName}<{sceneFileName}.{sceneName}TransitionData>" : TypeName;
        }
    }
}
