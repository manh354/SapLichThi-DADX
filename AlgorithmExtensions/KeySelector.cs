namespace AlgorithmExtensions
{
    public class KeySelector<T, U>
    {
        U ConditionalObject;
        Func<T, U, int> KeySelectorFunc;

        public KeySelector(U conditionalObject, Func<T, U, int> compareFunc)
        {
            ConditionalObject = conditionalObject;
            KeySelectorFunc = compareFunc ?? throw new ArgumentNullException(nameof(compareFunc));
        }

        public void SetConditionalObject(U conditionalObject)
        {
            ConditionalObject = conditionalObject;
        }
        public int GetKey(T selectiod)
        {
            return KeySelectorFunc(selectiod, ConditionalObject);
        }

        public U GetU() => ConditionalObject;
        public Func<T, U, int> GetFunc() => KeySelectorFunc;
    }

    public static class KeySelectorExtension
    {
        public static List<KeySelector<T,U>> ThenBy<T,U>(this KeySelector<T, U> primarySelector, KeySelector<T, U> secondarySelector)
        {
            return [primarySelector, secondarySelector];
        }

        public static List<KeySelector<T, U>> ThenBy<T, U>(this List<KeySelector<T, U>> primarySelectors, KeySelector<T, U> secondarySelector)
        {
            primarySelectors.Add(secondarySelector);
            return primarySelectors;
        }

        public static List<KeySelector<T,U>> PreceededBy<T,U>(this KeySelector<T,U> secondarySelector, KeySelector<T, U> primarySelector)
        {
            return [primarySelector, secondarySelector];
        }

        public static List<KeySelector<T, U>> PreceededBy<T, U>(this List<KeySelector<T, U>> secondarySelectors, KeySelector<T, U> primarySelector)
        {
            secondarySelectors.Insert(0,primarySelector);
            return secondarySelectors;
        }
    }
}
