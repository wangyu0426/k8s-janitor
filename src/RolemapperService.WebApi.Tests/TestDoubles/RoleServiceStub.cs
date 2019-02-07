using System.Threading.Tasks;
using RolemapperService.WebApi.Services;

namespace RolemapperService.WebApi.Tests.TestDoubles
{
    public class RoleServiceStub :IRoleService
    {
        public Task CreateRole(string roleName, string roleArn)
        {
            return Task.CompletedTask;
        }
    }
}