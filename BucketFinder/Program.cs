using System;
using System.Collections.Generic;

namespace BucketFinder
{
    class Program
    {
        static void Main()
        {
            //Основа для URI бакетов
            string baseUri = "https://storage.yandexcloud.net/";
            //Создание спика URI бакетов
            List<string> uris = HttpResponseParcer.CreateUri(baseUri, 3);
            //Создание списка баектов
            List<Bucket> buckets = HttpResponseParcer.GetHttpResponse(uris);
            //Получение статистики по бакетам
            Dictionary<string, int> bucketsStats = HttpResponseParcer.GetStats(buckets);

            //Вывод результатов в консоль
            foreach(Bucket bucket in buckets)
            {
                Console.WriteLine("Ссылка на бакет: {0}, HTTP ответ бакета: {1}, {2}", bucket.Uri, bucket.StatusCode, bucket.Status);
            }
            //Вывод статистики в консоль
            Console.WriteLine("Количество проверенных бакетов: {0}\n"
                + "Количество публичных бакетов: {1}\n"
                + "Количество частных бакетов: {2}",
                buckets.Count, bucketsStats["OK"], bucketsStats["Forbidden"]);
        }
    }
}
