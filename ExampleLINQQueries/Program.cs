namespace ExampleLINQQueries
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var arr = new int[] { 10, 9, 8, 7, 6, 5, 4, 3, 2, 1 };
            var whereResult = arr.Where(x => x % 2 == 0); // Ленивый вызов
            var selectResult = whereResult.Select(x => x * 2); // Ленивый вызов
            var orderResult = selectResult.OrderBy(x => x); // Ленивый вызов
            var result = orderResult.ToArray(); // Материализующий вызов
            foreach (var item in result)
            {
                Console.WriteLine(item);
            }
        }
    }
}
