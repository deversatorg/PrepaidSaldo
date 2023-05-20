using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationAuth.Models.RequestModels.Saldo
{
    public class SaldoPaginationRequestModel<T> : PaginationRequestModel<T> where T : struct
    {
        [JsonProperty("page")]
        public int CurrentPage { get; set; }
        [JsonProperty("period")]
        public string Period { get; set; }

    }
}
