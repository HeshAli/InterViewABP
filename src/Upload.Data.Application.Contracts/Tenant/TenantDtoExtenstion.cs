using System;
using System.Collections.Generic;
using System.Text;
using Volo.Abp.Identity;
using Volo.Abp.ObjectExtending;
using Volo.Abp.TenantManagement;
using Volo.Abp.Threading;

namespace Upload.Data.Tenant
{
    public class TenantDtoExtenstion
    {
        private static readonly OneTimeRunner OneTimeRunner = new OneTimeRunner();

        public static void Configure()
        {
            OneTimeRunner.Run(() =>
            {
                ObjectExtensionManager.Instance
                  .AddOrUpdateProperty<TenantDto, string>("Title");
                /* You can add extension properties to DTOs
                 * defined in the depended modules.
                 *
                 * Example:
                 *
                  ObjectExtensionManager.Instance
                    .AddOrUpdateProperty<IdentityRoleDto, string>("Title");
                 *
                 * See the documentation for more:
                 * https://docs.abp.io/en/abp/latest/Object-Extensions
                 */
            });
        }
    }
}

