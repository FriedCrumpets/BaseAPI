using Newtonsoft.Json;

namespace RESTful_web_API_Course.Core.Header; 

public static class HTTPResponseUtils { 
    // this feels stupid and will require a check for existence on the other end. Just add it in anyway. It's raw data
    public static void AddPagination(this HttpResponse response, Pagination pagination)
        => response.Headers.Add("X-Pagination", JsonConvert.SerializeObject(pagination));
}