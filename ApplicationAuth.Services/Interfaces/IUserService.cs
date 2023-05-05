using ApplicationAuth.Models.Enums;
using ApplicationAuth.Models.RequestModels;
using ApplicationAuth.Models.RequestModels.Base.CursorPagination;
using ApplicationAuth.Models.ResponseModels;
using ApplicationAuth.Models.ResponseModels.Base.CursorPagination;
using System.Threading.Tasks;

namespace ApplicationAuth.Services.Interfaces
{
    public interface IUserService
    {
        /// <summary>
        /// Get all users
        /// </summary>
        /// <param name="model"></param>
        /// <param name="getAdmins"></param>
        /// <returns></returns>
        PaginationResponseModel<UserTableRowResponseModel> GetAll(PaginationRequestModel<UserTableColumn> model, bool getAdmins = false);

        /// <summary>
        /// Get all users
        /// </summary>
        /// <param name="model"></param>
        /// <param name="getAdmins"></param>
        /// <returns></returns>
        CursorPaginationBaseResponseModel<UserTableRowResponseModel> GetAll(CursorPaginationRequestModel<UserTableColumn> model, bool getAdmins = false);

        /// <summary>
        /// Soft delete user (leave in db)
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        UserResponseModel SoftDeleteUser(int id);

        /// <summary>
        /// Hard delete user (delete from db)
        /// </summary>
        /// <param name="id"></param>
        Task HardDeleteUser(int id);

    }
}
