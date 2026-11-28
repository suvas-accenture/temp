namespace Capstone.API.DTOs.User;

public class UserResponseDto
{
    public string UserName { get; set; } = string.Empty;

    public int RoleId { get; set; }

    public string RoleName { get; set; } = string.Empty;

    public DateTime CreatedOn { get; set; }
}