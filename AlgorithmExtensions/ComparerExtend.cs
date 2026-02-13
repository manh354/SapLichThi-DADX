namespace AlgorithmExtensions
{
    public class ComparerExtend<T, U> : IComparer<T>
    {
        U ConditionalObject;
        Func<T, T, U, int> CompareFunc;
        public ComparerExtend(U conditionalObject, Func<T, T, U, int> compareFunc)
        {
            ConditionalObject = conditionalObject;
            CompareFunc = compareFunc ?? throw new ArgumentNullException(nameof(compareFunc));
        }

        public void SetConditionalObject(U conditionalObject)
        {
            ConditionalObject = conditionalObject;
        }
        public int Compare(T? x, T? y)
        {
            return CompareFunc(x, y, ConditionalObject);
        }
    }

    public static class ComparerExtendExtension
    {
        public static ComparerExtend<T, U> ThenBy<T, U>(this ComparerExtend<T, U> primary, ComparerExtend<T, U> secondary, U additionalObject)
        {
            return new ComparerExtend<T, U>(
                additionalObject, // Assume same condition object used by both
            (x, y, cond) =>
            {
                int result = primary.Compare(x, y);
                return result != 0 ? result : secondary.Compare(x, y);
            });
        }

        public static ComparerExtend<T, U> PreceededBy<T, U>(this ComparerExtend<T, U> secondary, ComparerExtend<T, U> primary, U additionalObject)
        {
            return new ComparerExtend<T, U>(
                additionalObject, // Assume same condition object used by both
            (x, y, cond) =>
            {
                int result = primary.Compare(x, y);
                return result != 0 ? result : secondary.Compare(x, y);
            });

        }
    }

}
