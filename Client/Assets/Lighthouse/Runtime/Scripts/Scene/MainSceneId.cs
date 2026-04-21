using System;

namespace Lighthouse.Scene
{
    /// <summary>
    /// Value object that uniquely identifies a main scene by integer Id and Name.
    /// Implements IEquatable and equality operators for use in collections and comparisons.
    /// </summary>
    public sealed class MainSceneId : IEquatable<MainSceneId>
    {
        public int Id { get; }
        public string Name { get; }

        public MainSceneId(int id, string name)
        {
            Id = id;
            Name = name ?? "";
        }

        public bool Equals(MainSceneId other) => other is not null && Id == other.Id;
        public override bool Equals(object obj) => obj is MainSceneId other && Equals(other);
        public override int GetHashCode() => Id;
        public static bool operator ==(MainSceneId a, MainSceneId b)
            => ReferenceEquals(a, b) || (a is not null && b is not null && a.Id == b.Id);
        public static bool operator !=(MainSceneId a, MainSceneId b) => !(a == b);
        public override string ToString() => string.IsNullOrEmpty(Name) ? Id.ToString() : $"{Name}({Id})";
    }
}