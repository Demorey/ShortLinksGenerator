namespace ShortLinksGenerator.Service
{
    public static class GenShortLinks
    {
        private static readonly Random Random = new Random();
        public static string GenShortLink() 
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            return new string(Enumerable.Repeat(chars, 4)
                .Select(s => s[Random.Next(s.Length)]).ToArray()); 
        } 
    }
}
