using System;

namespace Lighthouse.Scene
{
    /// <summary>
    /// Value object that uniquely identifies a module scene by integer Id and Name.
    /// Mirrors MainSceneId in structure, implementing IEquatable and equality operators.
    /// </summary>
    public class ModuleSceneId : IEquatable<ModuleSceneId>
    {
        public int Id { get; }
        public string Name { get; }

        public ModuleSceneId(int id, string name)
        {
            Id = id;
            Name = name ?? "";
        }

        public bool Equals(ModuleSceneId other) => other is not null && Id == other.Id;
        public override bool Equals(object obj) => obj is ModuleSceneId other && Equals(other);
        public override int GetHashCode() => Id;
        public static bool operator ==(ModuleSceneId a, ModuleSceneId b)
            => ReferenceEquals(a, b) || (a is not null && b is not null && a.Id == b.Id);
        public static bool operator !=(ModuleSceneId a, ModuleSceneId b) => !(a == b);
        public override string ToString() => string.IsNullOrEmpty(Name) ? Id.ToString() : $"{Name}({Id})";
    }
}