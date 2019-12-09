using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Pixeval.Data.Model.Web.Response;
using Pixeval.Objects;

namespace Pixeval.Caching.Persisting
{
    public class Identity
    {
        public static Identity Global;

        public string Name { get; set; }

        public DateTime ExpireIn { get; set; }

        public string AccessToken { get; set; }

        public string RefreshToken { get; set; }

        public string AvatarUrl { get; set; }

        public string Id { get; set; }

        public string MailAddress { get; set; }

        public string Account { get; set; }

        public string Password { get; set; }

        public static Identity Parse(string password, TokenResponse token)
        {
            
            var response = token.ToResponse;
            return new Identity
            {
                Name = response.User.Name,
                ExpireIn = DateTime.Now + TimeSpan.FromSeconds(response.ExpiresIn),
                AccessToken = response.AccessToken,
                RefreshToken = response.RefreshToken,
                AvatarUrl = response.User.ProfileImageUrls.Px170X170,
                Id = response.User.Id.ToString(),
                MailAddress = response.User.MailAddress,
                Account = response.User.Account,
                Password = password
            };
        }

        public override string ToString()
        {
            return this.ToJson();
        }

        public async Task Store()
        {
            if (!Directory.Exists(PixevalEnvironment.ConfFolder)) Directory.CreateDirectory(PixevalEnvironment.ConfFolder);
            await File.WriteAllTextAsync(Path.Combine(PixevalEnvironment.ConfFolder, "pixeval_conf.json"), ToString());
        }

        public static async Task Restore()
        {
            Global = (await File.ReadAllTextAsync(Path.Combine(PixevalEnvironment.ConfFolder, "pixeval_conf.json"), Encoding.UTF8)).FromJson<Identity>();
        }

        public static bool ConfExists()
        {
            var path = Path.Combine(PixevalEnvironment.ConfFolder, "pixeval_conf.json");
            return File.Exists(path) && new FileInfo(path).Length != 0;
        }

        public static async ValueTask<bool> RefreshRequired()
        {
            return (await File.ReadAllTextAsync(Path.Combine(PixevalEnvironment.ConfFolder, "pixeval_conf.json"), Encoding.UTF8)).FromJson<Identity>().ExpireIn <= DateTime.Now;
        }

        public static async Task RefreshIfRequired()
        {
            if (Global == null)
            {
                await Restore();
            }

            if (await RefreshRequired())
            {
                await Authentication.Authenticate(Global?.MailAddress, Global?.Password);
                if (Global != null) await Global.Store();
            }
        }

        public static void Clear()
        {
            File.Delete(Path.Combine(PixevalEnvironment.ConfFolder, "pixeval_conf.json"));
        }
    }
}