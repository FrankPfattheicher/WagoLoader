using WagoLoader.Loader;
using Xunit;

namespace WagoLoader.Test
{
    public class TimezoneTests
    {
        [Fact]
        public void TimezoneUtcShouldBeValid()
        {
            Assert.True(Timezone.Validate("Etc/UTC"));
        }

        [Fact]
        public void TimezoneEuropeBerlinShouldBeValid()
        {
            Assert.True(Timezone.Validate("Europe/Berlin"));
        }

        [Fact]
        public void TimezoneUnknownShouldBeInvalid()
        {
            Assert.False(Timezone.Validate("Etc/Unknown"));
        }

    }
}
