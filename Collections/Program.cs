using Collections.CustomDictionary;
using Collections.CustomList;

namespace Collections
{
    internal class Program
    {
        static void Main(string[] args)
        {
            // Пример использования CustomDictionary
            var customDictionary = new CustomDictionary<string, double>();
            customDictionary.Add("Яблоко", 5.98);
            customDictionary.Add("Груша", 7.47);
            customDictionary.Add("Ананас", 10.55);
            foreach (var kvp in customDictionary)
            {
                Console.WriteLine($"Фрукт: {kvp.Key}, Цена: {kvp.Value}");
            }

            // Пример использования CustomList
            var customList = new CustomList<string>();
            customList.Add("Один");
            customList.Add("Два");
            customList.Add("Три");

            foreach (var item in customList)
            {
                Console.WriteLine(item);
            }
        }
    }
}
