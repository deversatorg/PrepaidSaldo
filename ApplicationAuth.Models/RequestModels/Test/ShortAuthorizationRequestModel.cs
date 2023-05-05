using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace ApplicationAuth.Models.RequestModels.Test
{
    public class ShortAuthorizationRequestModel
    {
        [Range(1, int.MaxValue, ErrorMessage = "{0} is invalid")]
        public int? Id { get; set; }

        public string UserName { get; set; }
    }
}
