using System;
using BLL;
using Xunit;

namespace CRMPeyvand.Tests
{
    public class PasswordHasherTests
    {
        private const string Sample = "s3cret-Pass";

        [Fact]
        public void Hash_produces_versioned_format()
        {
            string stored = PasswordHasher.Hash(Sample);

            Assert.StartsWith("pbkdf2-sha256:", stored);
            Assert.Equal(4, stored.Split(':').Length);
        }

        [Fact]
        public void Hash_embeds_configured_iteration_count()
        {
            string stored = PasswordHasher.Hash(Sample);

            Assert.Equal(PasswordHasher.Iterations.ToString(),
                         stored.Split(':')[1]);
        }

        [Fact]
        public void Verify_accepts_correct_password()
        {
            Assert.True(PasswordHasher.Verify(Sample, PasswordHasher.Hash(Sample)));
        }

        [Fact]
        public void Verify_rejects_wrong_password()
        {
            Assert.False(PasswordHasher.Verify("wrong", PasswordHasher.Hash(Sample)));
        }

        [Fact]
        public void Same_password_hashes_twice_differ_salts()
        {
            Assert.NotEqual(PasswordHasher.Hash(Sample), PasswordHasher.Hash(Sample));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("cGFzcw==")]
        [InlineData("md5:1024:abc:def")]
        public void Verify_rejects_garbage_or_legacy_stored_values(string stored)
        {
            Assert.False(PasswordHasher.Verify(Sample, stored));
        }

        [Fact]
        public void Verify_roundtrips_manual_stored_string()
        {
            string salt = Convert.ToBase64String(new byte[16]);
            string manual = "pbkdf2-sha256:" + PasswordHasher.Iterations + ":" + salt + ":not-a-real-hash";

            Assert.False(PasswordHasher.Verify(Sample, manual),
                "a malformed-but-parsable stored value must not authenticate");
        }
    }
}
