using ApplicationAuth.Common.Exceptions;
using ApplicationAuth.Common.Extensions;
using ApplicationAuth.DAL.Abstract;
using ApplicationAuth.Domain.Entities;
using ApplicationAuth.Domain.Entities.Identity;
using ApplicationAuth.Models.Enums;
using ApplicationAuth.Models.RequestModels;
using ApplicationAuth.Models.RequestModels.Base.CursorPagination;
using ApplicationAuth.Models.ResponseModels;
using ApplicationAuth.Models.ResponseModels.Base.CursorPagination;
using ApplicationAuth.Services.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace ApplicationAuth.Services.Services
{
    public class UserService : IUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor = null;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper = null;
        private readonly IJWTService _jwtService;

        private bool _isUserSuperAdmin = false;
        private bool _isUserAdmin = false;
        private int? _userId = null;

        public UserService(IUnitOfWork unitOfWork, 
            IMapper mapper, 
            IHttpContextAccessor httpContextAccessor, 
            IServiceProvider serviceProvider, 
            IJWTService jwtService)
        {
            _unitOfWork = unitOfWork;
            _httpContextAccessor = httpContextAccessor;
            _mapper = mapper;
            _jwtService = jwtService;

            var context = httpContextAccessor.HttpContext;

            if (context?.User != null)
            {
                _isUserSuperAdmin = context.User.IsInRole(Role.SuperAdmin);
                _isUserAdmin = context.User.IsInRole(Role.Admin);

                try
                {
                    _userId = context.User.GetUserId();
                }
                catch
                {
                    _userId = null;
                }
            }

        }

        public PaginationResponseModel<UserTableRowResponseModel> GetAll(PaginationRequestModel<UserTableColumn> model, bool getAdmins = false)
        {
            if (!_isUserSuperAdmin && !_isUserAdmin)
                throw new CustomException(HttpStatusCode.Forbidden, "role", "You don't have permissions");

            List<UserTableRowResponseModel> response = new List<UserTableRowResponseModel>();

            var search = !string.IsNullOrEmpty(model.Search) && model.Search.Length > 1;

            //!x.UserRoles.Any(w => (_userIsSuperAdmin && w.Role.Name != Role.Admin) && w.Role.Name != Role.SuperAdmin)

            var users = _unitOfWork.Repository<ApplicationUser>().Get(x => !x.IsDeleted
                                            && !x.UserRoles.Any(w => w.Role.Name == Role.SuperAdmin)
                                            && (!search || (x.Email.Contains(model.Search) || x.Profile.FirstName.Contains(model.Search) || x.Profile.LastName.Contains(model.Search)))
                                            && (getAdmins ? x.UserRoles.Any(w => w.Role.Name == Role.Admin) : x.UserRoles.Any(w => w.Role.Name == Role.User))
                                            && (_isUserSuperAdmin || !x.UserRoles.Any(w => (w.Role.Name == Role.Admin))))
                                        .TagWith(nameof(GetAll) + "_GetUsers")
                                        .Include(w => w.UserRoles)
                                            .ThenInclude(w => w.Role)
                                        .Select(x => new
                                        {
                                            Email = x.Email,
                                            FirstName = x.Profile.FirstName,
                                            LastName = x.Profile.LastName,
                                            IsBlocked = !x.IsActive,
                                            RegisteredAt = x.RegistratedAt,
                                            Id = x.Id
                                        });


            if (search)
                users = users.Where(x => x.Email.Contains(model.Search) || x.FirstName.Contains(model.Search) || x.LastName.Contains(model.Search));

            int count = users.Count();

            if (model.Order != null)
                users = users.OrderBy(model.Order.Key.ToString(), model.Order.Direction == SortingDirection.Asc);

            users = users.Skip(model.Offset).Take(model.Limit);

            response = users.Select(x => new UserTableRowResponseModel
            {
                Email = x.Email,
                FirstName = x.FirstName,
                LastName = x.LastName,
                IsBlocked = x.IsBlocked,
                RegisteredAt = x.RegisteredAt.ToISO(),
                Id = x.Id
            }).ToList();

            return new(response, count);
        }

        public CursorPaginationBaseResponseModel<UserTableRowResponseModel> GetAll(CursorPaginationRequestModel<UserTableColumn> model, bool getAdmins = false)
        {
            if (!_isUserSuperAdmin && !_isUserAdmin)
                throw new CustomException(HttpStatusCode.Forbidden, "role", "You don't have permissions");

            var search = !string.IsNullOrEmpty(model.Search) && model.Search.Length > 1;

            var users = _unitOfWork.Repository<ApplicationUser>().Get(x => !x.IsDeleted
                                            && !x.UserRoles.Any(w => w.Role.Name == Role.SuperAdmin)
                                            && (!search || (x.Email.Contains(model.Search) || x.Profile.FirstName.Contains(model.Search) || x.Profile.LastName.Contains(model.Search)))
                                            && (getAdmins ? x.UserRoles.Any(w => w.Role.Name == Role.Admin) : x.UserRoles.Any(w => w.Role.Name == Role.User))
                                            && (_isUserSuperAdmin || !x.UserRoles.Any(w => (w.Role.Name == Role.Admin))))
                                        .TagWith(nameof(GetAll) + "_GetUsers")
                                        .Select(x => new
                                        {
                                            Email = x.Email,
                                            FirstName = x.Profile.FirstName,
                                            LastName = x.Profile.LastName,
                                            IsBlocked = !x.IsActive,
                                            RegisteredAt = x.RegistratedAt,
                                            Id = x.Id
                                        });

            if (model.Order != null)
                users = users.OrderBy(model.Order.Key.ToString(), model.Order.Direction == SortingDirection.Asc);

            var userList = users.ToList();

            var offset = 0;

            if (model.LastId.HasValue)
            {
                var item = userList.FirstOrDefault(u => u.Id == model.LastId);

                if (item is null)
                    throw new CustomException(HttpStatusCode.BadRequest, "lastId", "There is no user with specific id in selection");

                offset = userList.IndexOf(item) + 1;
            }

            users = users.Skip(offset).Take(model.Limit + 1);

            var response = users.Select(x => new UserTableRowResponseModel
            {
                Email = x.Email,
                FirstName = x.FirstName,
                LastName = x.LastName,
                IsBlocked = x.IsBlocked,
                RegisteredAt = x.RegisteredAt.ToISO(),
                Id = x.Id
            });

            int? nextCursorId = null;

            if (users.Count() > model.Limit)
            {
                response = response.Take(model.Limit);
                nextCursorId = response.AsEnumerable().LastOrDefault()?.Id;
            }

            return new(response.ToList(), nextCursorId);
        }

        public UserResponseModel SoftDeleteUser(int id)
        {
            var user = _unitOfWork.Repository<ApplicationUser>().Get(w => w.Id == id && !w.UserRoles.Any(x => x.Role.Name == Role.SuperAdmin) && (!w.UserRoles.Any(x => x.Role.Name == Role.Admin) || _isUserSuperAdmin))
                                      .TagWith(nameof(SoftDeleteUser) + "_GetUser")
                                      .Include(w => w.Profile)
                                      .FirstOrDefault();

            if (user == null)
                throw new CustomException(HttpStatusCode.BadRequest, "userId", "User is not found");

            user.IsDeleted = true;
            user.DeletedAt = DateTime.UtcNow;

            _unitOfWork.Repository<ApplicationUser>().Update(user);
            _unitOfWork.SaveChanges();

            return _mapper.Map<UserResponseModel>(user.Profile);
        }

        public async Task HardDeleteUser(int id)
        {
            var user = _unitOfWork.Repository<ApplicationUser>().Get(w => w.Id == id && !w.UserRoles.Any(x => x.Role.Name == Role.SuperAdmin) && ((!w.UserRoles.Any(x => x.Role.Name == Role.Admin) || _isUserSuperAdmin)))
                                      .TagWith(nameof(SoftDeleteUser) + "_GetUser")
                                      .Include(w => w.Profile)
                                      .FirstOrDefault();

            if (user == null)
                throw new CustomException(HttpStatusCode.BadRequest, "userId", "User is not found");

            _unitOfWork.Repository<ApplicationUser>().Delete(user);
            _unitOfWork.SaveChanges();
        }

    }
}
