using System.Net.Http;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Toggl.Multivac.Extensions;
using Toggl.Ultrawave.Network;
using Toggl.Multivac.Models;
using Toggl.Multivac;
using Toggl.Ultrawave.Tests.Integration.Helper;

namespace Toggl.Ultrawave.Tests.Integration
{
    internal static class User
    {
        public static async Task<IUser> Create()
        {
            var (email, password) = generateEmailPassword();
            return await createUser(email, password);
        }

        public static async Task<(Email email, Password password)> CreateEmailPassword()
        {
            var (email, password) = generateEmailPassword();
            await createUser(email, password);
            return (email, password);
        }

        public static async Task ResetApiToken(IUser user)
        {
            var message = AuthorizedRequestBuilder.CreateRequest(Credentials.WithApiToken(user.ApiToken),
                "https://toggl.space/api/v8/reset_token", HttpMethod.Post);

            using (var client = new HttpClient())
            {
                await client.SendAsync(message);
            }
        }

        private static (Email email, Password password) generateEmailPassword()
            => (RandomEmail.GenerateValid(), "123456".ToPassword());

        private static async Task<IUser> createUser(Email email, Password password)
        {
            var api = Helper.TogglApiFactory.TogglApiWith(Credentials.None);
            return await api.User.SignUp(email, password, true, 237);
        }
    }
}
