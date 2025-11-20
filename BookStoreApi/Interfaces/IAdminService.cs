using BookStoreApi.Dtos.User;

namespace BookStoreApi.Interfaces;

public interface IAdminService
{
    Task<IEnumerable<UserWithRolesDto>> GetAllUsersAsync();
    Task<UserWithRolesDto?> GetUserByIdAsync(Guid userId);
    Task<BasicResponse> MakeUserAdminAsync(Guid userId, string currentAdminEmail);
    Task<BasicResponse> RemoveAdminRoleAsync(Guid userId);
    Task<BasicResponse> DeleteUserAsync(Guid userId, string currentAdminEmail);
}