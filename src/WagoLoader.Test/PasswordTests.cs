using WagoLoader.Loader;
using Xunit;

namespace WagoLoader.Test
{
    public class PasswordTests
    {
        [Fact]
        public void KnownPasswordsShouldBeValid()
        {
            var pwd = new PasswordFile("htpasswd.txt");

            Assert.True(pwd.IsValid("admin", "test"));
            Assert.True(pwd.IsValid("user", "test"));
        }

        [Fact]
        public void OriginalWagoPasswordsShouldBeValid()
        {
            var pwd = new PasswordFile("lighttpd-htpasswd.user");

            Assert.True(pwd.IsValid("admin", "wago"));
            Assert.True(pwd.IsValid("user", "user"));
        }

    }
}
