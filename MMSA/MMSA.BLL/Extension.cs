namespace MMSA.BLL
{
    public static class Extension
    {
        public static bool CheckNullOrEmpty<T>(T value)
        {
            if (typeof(T) == typeof(string))
                return string.IsNullOrEmpty(value as string);

            return value == null || value.Equals(default(T));
        }
        public static bool Operator(this string logic, double x, double y)
        {
            switch (logic)
            {
                case ">": return x > y;
                case "<": return x < y;
                case "==": return x == y;
                case "<=": return x <= y;
                case ">=": return x >= y;
                default: throw new Exception("invalid logic");
            }
        }
    }
}
