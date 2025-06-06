using EventTracking.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace EventTracking.Components.Account.Pages.Admin;

[Authorize(Roles = "Admin")]
public partial class AccountManagementBase : ComponentBase
{
    [Inject] protected UserManager<ApplicationUser> UserManager { get; set; } = default!;
    [Inject] protected RoleManager<IdentityRole> RoleManager { get; set; } = default!;

    protected string statusMessage = "";
    protected List<string> allRoles = new();
    protected List<UserWithRole> usersWithRoles = new();
    protected ApplicationUser? selectedUser;
    protected string searchTerm = string.Empty;

    protected IEnumerable<UserWithRole> FilteredUsers =>
        string.IsNullOrWhiteSpace(searchTerm)
            ? usersWithRoles
            : usersWithRoles.Where(u =>
                !string.IsNullOrEmpty(u.User.UserName) &&
                u.User.UserName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase));

    protected class DummyModel { }

    public class UserWithRole
    {
        public ApplicationUser User { get; set; } = default!;
        public string SelectedRole { get; set; } = "";
    }

    protected override async Task OnInitializedAsync()
    {
        var users = await UserManager.Users.ToListAsync();
        allRoles = await RoleManager.Roles.Select(r => r.Name!).ToListAsync();

        foreach (var user in users)
        {
            var roles = await UserManager.GetRolesAsync(user);
            usersWithRoles.Add(new UserWithRole
            {
                User = user,
                SelectedRole = roles.FirstOrDefault() ?? ""
            });
        }
    }

    protected async Task OnRoleChangedAsync(ChangeEventArgs e, UserWithRole user)
    {
        user.SelectedRole = e.Value?.ToString() ?? "";
        await UpdateUserRoleAsync(user);
        await InvokeAsync(StateHasChanged);
    }

    protected async Task UpdateUserRoleAsync(UserWithRole userWithRole)
    {
        var currentRoles = await UserManager.GetRolesAsync(userWithRole.User);

        if (currentRoles.Any())
        {
            var removeResult = await UserManager.RemoveFromRolesAsync(userWithRole.User, currentRoles);
            if (!removeResult.Succeeded)
            {
                statusMessage = $"Failed to remove old role: {string.Join(", ", removeResult.Errors.Select(e => e.Description))}";
                return;
            }
        }

        if (!string.IsNullOrWhiteSpace(userWithRole.SelectedRole))
        {
            var addResult = await UserManager.AddToRoleAsync(userWithRole.User, userWithRole.SelectedRole);
            if (!addResult.Succeeded)
            {
                statusMessage = $"Failed to add new role: {string.Join(", ", addResult.Errors.Select(e => e.Description))}";
                return;
            }
        }

        statusMessage = $"Updated role for {userWithRole.User.UserName} to: {userWithRole.SelectedRole}";
    }

    protected async Task OnSaveUserEdit()
    {
        if (selectedUser != null)
        {
            var userInDb = await UserManager.FindByIdAsync(selectedUser.Id);
            if (userInDb != null)
            {
                userInDb.UserName = selectedUser.UserName;
                userInDb.Email = selectedUser.Email;
                userInDb.PhoneNumber = selectedUser.PhoneNumber;
                userInDb.Sex = selectedUser.Sex;
                userInDb.Address = selectedUser.Address;

                var result = await UserManager.UpdateAsync(userInDb);
                statusMessage = result.Succeeded
                    ? "User information updated successfully."
                    : $"Error: {string.Join("; ", result.Errors.Select(e => e.Description))}";
            }

            selectedUser = null;
        }
    }

    protected async Task DeleteUser(string userId)
    {
        var user = await UserManager.FindByIdAsync(userId);
        if (user != null)
        {
            var result = await UserManager.DeleteAsync(user);
            if (result.Succeeded)
            {
                usersWithRoles.RemoveAll(u => u.User.Id == userId);
                statusMessage = $"Deleted user account: {user.UserName}";
            }
            else
            {
                statusMessage = $"Failed to delete: {string.Join(", ", result.Errors.Select(e => e.Description))}";
            }
        }
    }

    protected void OnSubmitShowUser(ApplicationUser user)
    {
        selectedUser = user;
    }

    protected void HideUserModal()
    {
        selectedUser = null;
    }
}
