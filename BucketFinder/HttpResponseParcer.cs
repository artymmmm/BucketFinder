using System.Collections.Generic;
using System.Text;
using System.Net;

namespace BucketFinder
{
    //Класс, отвечающий за получение HTTP ответов
    public class HttpResponseParcer
    {
        //Список со всеми URI бакетов, которые будут проверены
        public static List<string> listOfAllUris = new List<string>();
        //Переменная для хранения длины основы для URI
        public static int baseUriLength;

        //Wrapper для createUriLoop
        //Параметры: baseURI - основа URI, maxNameLength - максимальная длина названия бакета
        //Возвращает список URI
        public static List<string> CreateUri (string baseUri, int maxNameLength)
        {
            //Переменная для хранения длины основы URI
            int baseUriLength = baseUri.Length;
            //Добавление основы URI в список всех URI
            listOfAllUris.Add(baseUri);
            //Вызов метода CreateUriLoop и передача методу списка всех URI, максимальной длины названия бакета, длины основы URI
            return CreateUriLoop(listOfAllUris, maxNameLength, baseUriLength);
        }

        //Создание списка URI бакетов
        //Параметры: список URI, максимальная длина названия бакета, длина основы URI
        //Возращает список URI
        public static List<string> CreateUriLoop (List<string> listOfUris, int maxNameLength, int baseUriLength)
        {
            //Список, содержащий URI, созданные в данном вызове createUriLoop
            List<string> newUris = new List<string>();

            //Цикл по URI из списка URI
            foreach (string uri in listOfUris)
            {
                //Цикл по символам ASCII 
                for (char c = 'a'; c <= 'd'; c++)
                {
                    //Создание экземпляра класса StringBuilder для редактирования URI
                    StringBuilder sb = new StringBuilder(uri);

                    //Пропуск запрещённых символов
                    if (c == '/' || (c > '9' & c < 'a'))
                    {
                        continue;
                    }

                    //Добавление символа в URI
                    sb.Append(c);
                    //Добавление изменённого URI в список с новыми URI
                    newUris.Add(sb.ToString());
                }
            }
            //Добавление новых URI в список со всеми URI
            listOfAllUris.AddRange(newUris);
            
            //Проверка на наличие всех URI максимальной длины названия бакета и меньше
            //Если URI максимальной длины получены, то возвращается список всех URI
            if (listOfAllUris[listOfAllUris.Count - 1].Length - baseUriLength == maxNameLength)
            {
                return listOfAllUris;
            }

            //Цикл по URI из копии списка всех URI
            //Используется копия, так как в оригинальный список вносятся изменения
            foreach (string uri in listOfAllUris.ToArray())
            {
                //Так как длина названия бакета не может быть меньше 3, не подходящие URI удаляются
                if (uri.Length - baseUriLength < 3)
                    listOfAllUris.Remove(uri);
            }

            //Если все URI ещё не получены, вызывается CreateUriLoop
            //На вход поступает список новых URI, полученных в нынешнем вызове CreateUriLoop
            return CreateUriLoop(newUris, maxNameLength, baseUriLength);
        }
        
        //Получение HTTP ответа
        //Параметры: список URI, от которых необходимо получить ответ
        //Возвращает список бакетов
        public static List<Bucket> GetHttpResponse(List<string> listOfUris)
        {
            //Создание спика бакетов
            List<Bucket> buckets = new List<Bucket>();

            //Цикл по URI из списка URI
            foreach (string uri in listOfUris)
            {
                //Создание экземпляра бакета
                Bucket bucketInUri = new Bucket();
                //Переменная, которая будет содержать код состояния
                int statusCode;
                //Переменная, которая будет содержать состояние в виде текста
                string status;

                //Создание HTTP запроса на URI, переданный в виде параметра
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);

                //Установка ограничений на использование ресурсов запросом
                //Макисмальное количество переадресаций
                request.MaximumAutomaticRedirections = 4;
                //Максимальная длина заголовка ответов
                request.MaximumResponseHeadersLength = 4;
                //Установка учётных данных для запроса
                request.Credentials = CredentialCache.DefaultCredentials;

                try
                {
                    //Получение HTTP ответа
                    HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                    //Запись код статуса в переменную
                    statusCode = (int)response.StatusCode;
                    //Заркытие ответа и освобождение подключения
                    response.Close();
                }
                //Поймана ошибка WebException (не удалось установить соединение или доступ запрещён)
                catch (WebException e)
                {
                    //Ошибка содержит код статуса 403
                    if (e.Message.Contains("403"))
                    {
                        statusCode = 403;
                    }
                    //Ошибка содержит код статуса 403
                    else if (e.Message.Contains("404"))
                    {
                        statusCode = 404;
                    }
                    //Ошибка содержит код статуса 403
                    else if (e.Message.Contains("400"))
                    {
                        statusCode = 400;
                    }
                    //Ошибка содержит другой код статуса
                    else
                    {
                        statusCode = -1;
                    }
                }

                //Установка состояния в виде текста в зависимости от кода
                if (statusCode == 200)
                {
                    status = "Бакет публичный";
                }
                else if (statusCode == 403)
                {
                    status = "Бакет частный";
                }
                else if (statusCode == 404)
                {
                    status = "Бакет не существует";
                }
                else if (statusCode == 400)
                {
                    status = "Неправильное название бакета";
                }
                else
                {
                    status = "Необработанный статус бакета";
                }

                //Заполнение свойств экземпляра бакета
                bucketInUri.Uri = uri;
                bucketInUri.StatusCode = statusCode;
                bucketInUri.Status = status;
                //Добавление экземпляра бакета в список бакетов
                buckets.Add(bucketInUri);
            }
            //Возвращает список бакетов
            return buckets;
        }
        //Производит подсчёт публичных и частных бакетов
        //Параметры: список бакетов
        //Вовзращает словарь, где ключами являются "Публичный" и "Частный", значениями - количество таких бакетов
        public static Dictionary<string, int> GetStats(List<Bucket> buckets)
        {
            //Переменная для хранения количества публичных бакетов
            int okBuckets = 0;
            //Переменная для хранения количества частных бакетов
            int forbiddenBuckets = 0;

            //Цикл по бакетам
            foreach (Bucket bucket in buckets)
            {
                if (bucket.StatusCode == 200)
                {
                    okBuckets++;
                }
                else if (bucket.StatusCode == 403)
                {
                    forbiddenBuckets++;
                }
            }
            //Объявление словаря
            Dictionary<string, int> bucketsStats = new Dictionary<string, int>();
            //Добавление пар в словарь
            bucketsStats.Add("OK", okBuckets);
            bucketsStats.Add("Forbidden", forbiddenBuckets);
            //Возвращение словаря
            return bucketsStats;
        }
    }
}