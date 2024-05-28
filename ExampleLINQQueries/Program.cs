using ExampleLINQQueries.CustomMetods;

namespace ExampleLINQQueries
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var arr = new int[] { 10, 9, 8, 7, 6, 5, 4, 3, 2, 1 };
            var whereResult = arr.LazyFilter(x => x % 2 == 0); // Ленивый вызов
            var selectResult = whereResult.LazyMap(x => x * 2); // Ленивый вызов
            var orderResult = selectResult.LazyOrderBy(x => x); // Ленивый вызов
            var result = orderResult.CustomToList(); // Материализующий вызов
            foreach (var item in result)
            {
                Console.WriteLine(item);
            }
        }
    }
}
