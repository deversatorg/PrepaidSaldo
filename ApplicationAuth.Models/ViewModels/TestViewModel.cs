using ApplicationAuth.Models.ResponseModels.Session;
using System.Collections.Generic;

namespace ApplicationAuth.Models.ViewModels
{
    public class TestViewModel
    {
        public string Title { get; set; }

        public string Description { get; set; }

        public List<UserBaseResponseModel> Users { get; set; }
    }
}
