using System;

namespace Automapper
{
    public abstract class BaseMapAttribute : Attribute
    {
        public Type MapType { get; }
        public BaseMapAttribute(Type mapType) => MapType = mapType ?? throw new ArgumentNullException(nameof(mapType));
    }

    public class MapFromAttribute : BaseMapAttribute
    {
        public MapFromAttribute(Type mapType) : base(mapType) { }
    }

    public class MapToAttribute : BaseMapAttribute
    {
        public MapToAttribute(Type mapType) : base(mapType) { }
    }

    public class ReverseMapAttribute : BaseMapAttribute
    {
        public ReverseMapAttribute(Type mapType) : base(mapType) { }
    }

   public class TwoWayMapAttribute : BaseMapAttribute
    {
        public TwoWayMapAttribute(Type mapType) : base(mapType) { }
    }
}