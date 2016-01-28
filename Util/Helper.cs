using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Table;

namespace Nipster.Util
{
    public static class Helper
    {
        public static IEnumerable<IEnumerable<T>> Chunk<T>(this IEnumerable<T> source, int chunksize)
        {
            return source
                .Select((x, i) => new {Index = i, Value = x})
                .GroupBy(x => x.Index/chunksize)
                .Select(x => x.Select(v => v.Value).ToList())
                .ToList();
        }

        public static string ToRowKey(this string input)
        {
            using (var md5 = MD5.Create())
            {
                var data = md5.ComputeHash(Encoding.UTF8.GetBytes(input));
                var sBuilder = new StringBuilder();
                foreach (var b in data)
                {
                    sBuilder.Append(b.ToString("x2"));
                }
                return sBuilder.ToString();
            }
        }

        public static async Task QueryAsync<T>(this CloudTable table, Action<List<T>> action)
            where T : TableEntity, new()
        {
            await table.QueryAsync(new TableQuery<T>(), action);
        }

        public static async Task QueryAsync<T>(this CloudTable table, TableQuery<T> query, Action<List<T>> action)
            where T : TableEntity, new()
        {
            if (query == null) query = new TableQuery<T>();
            TableContinuationToken token = null;

            do
            {
                var queryResult = await table.ExecuteQuerySegmentedAsync(query, token);
                action(queryResult.Results);
                token = queryResult.ContinuationToken;
            } while (token != null);
        }
    }
}