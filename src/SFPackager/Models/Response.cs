using System.Net;

namespace SFPackager.Models
{
    public class Response<T>
    {
        public BlobOperation Operation { get; set; }
        public HttpStatusCode StatusCode { get; set; }
        public T ResponseContent { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
        public bool IsSuccessful => (int)StatusCode < 300; 
    }
}