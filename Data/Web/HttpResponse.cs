using System;

namespace Pixeval.Data.Web
{
    public class HttpResponse<T> : Tuple<bool, T>
    {
        private HttpResponse(bool status, T response) : base(status, response) { }

        public static HttpResponse<T> Wrap(bool status)
        {
            return new HttpResponse<T>(status, default);
        }

        public static HttpResponse<T> Wrap(bool status, T response)
        {
            return new HttpResponse<T>(status, response);
        }

        public void Deconstruct(out bool status, out T response)
        {
            status = Item1;
            response = Item2;
        }
    }
}