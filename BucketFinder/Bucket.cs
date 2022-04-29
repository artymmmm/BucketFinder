namespace BucketFinder
{   
    //Класс бакета
    public class Bucket
    {
        public Bucket() { }
        //Свойство URI бакета
        public string Uri { get; set; }
        //Свойство Код состояния бакета
        public int StatusCode { get; set; }
        //Свойство Состояние бакета
        public string Status { get; set; }
    }
}
