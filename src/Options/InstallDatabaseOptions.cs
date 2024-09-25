using Newtonsoft.Json;

using Xperience.Manager.Commands;

namespace Xperience.Manager.Options
{
    /// <summary>
    /// The options used to install Xperience by Kentico databases, used by <see cref="InstallCommand"/>.
    /// </summary>
    public class InstallDatabaseOptions : IWizardOptions
    {
        /// <summary>
        /// The name of the new database.
        /// </summary>
        public string DatabaseName { get; set; } = "xperience";


        /// <summary>
        /// If <c>true</c>, a new database will not be installed and the <see cref="DatabaseName"/> will be used.
        /// </summary>
        public bool UseExistingDatabase { get; set; } = false;


        /// <summary>
        /// The name of the SQL server to use.
        /// </summary>
        public string? ServerName { get; set; }


        /// <summary>
        /// The name of the global administrator password.
        /// </summary>
        [JsonIgnore]
        public string AdminPassword { get; set; } = GenerateRandomPassword();


        /// <summary>
        /// Generates a random password.
        /// </summary>
        private static string GenerateRandomPassword()
        {
            int requiredLength = 10;
            int requiredUniqueChars = 4;
            bool requireDigit = true;
            bool requireLowercase = true;
            bool requireNonAlphanumeric = true;
            bool requireUppercase = true;
            string[] randomChars = [
                "ABCDEFGHJKLMNOPQRSTUVWXYZ",
                "abcdefghijkmnopqrstuvwxyz",
                "0123456789",
                "!@$?"
            ];

            var rand = new Random(Environment.TickCount);
            var chars = new List<char>();

            if (requireUppercase)
            {
                chars.Insert(rand.Next(0, chars.Count),
                    randomChars[0][rand.Next(0, randomChars[0].Length)]);
            }

            if (requireLowercase)
            {
                chars.Insert(rand.Next(0, chars.Count),
                    randomChars[1][rand.Next(0, randomChars[1].Length)]);
            }

            if (requireDigit)
            {
                chars.Insert(rand.Next(0, chars.Count),
                    randomChars[2][rand.Next(0, randomChars[2].Length)]);
            }

            if (requireNonAlphanumeric)
            {
                chars.Insert(rand.Next(0, chars.Count),
                    randomChars[3][rand.Next(0, randomChars[3].Length)]);
            }

            for (int i = chars.Count; i < requiredLength
                || chars.Distinct().Count() < requiredUniqueChars; i++)
            {
                string rcs = randomChars[rand.Next(0, randomChars.Length)];
                chars.Insert(rand.Next(0, chars.Count),
                    rcs[rand.Next(0, rcs.Length)]);
            }

            return new string(chars.ToArray());
        }
    }
}
