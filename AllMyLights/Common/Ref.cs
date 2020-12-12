namespace AllMyLights.Common
{
    public class Ref<T> where T : struct
    {
        public T Value { get; set; }

        public Ref(T value)
        {
            Value = value;
        }


        public override string ToString()
        {
            return Value.ToString();
        }

        public override bool Equals(object obj)
        {
            return obj switch
            {
                Ref<T> r => Value.Equals(r.Value),
                _ => false
            };
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        public static implicit operator T(Ref<T> wrapper)
        {
            return wrapper.Value;
        }

        public static implicit operator Ref<T>(T value)
        {
            return new Ref<T>(value);
        }
    }
}
