using Newtonsoft.Json;
using ApplicationAuth.Models.Enums;

namespace ApplicationAuth.Models.RequestModels.Base.CursorPagination
{
    public class CursorPaginationRequestModel<T> : CursorPaginationBaseRequestModel where T : struct
    {
        [JsonProperty("search")]
        public string Search { get; set; }

        [JsonProperty("order")]
        public OrderingRequestModel<T, SortingDirection> Order { get; set; }
    }
}
