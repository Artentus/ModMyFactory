using System.Linq;
using System.Threading.Tasks;
using Octokit;

namespace ModMyFactory.Helpers
{
    static class GitHubClientExtensions
    {
        public static async Task<Release> GetPreReleaseAsync(this GitHubClient client, string owner, string name)
        {
            var releases = await client.Repository.Release.GetAll(owner, name);
            return releases.Where(release => release.Prerelease).MaxBy(release => new ExtendedVersion(release.TagName));
        }

        public static async Task<Release> GetLatestReleaseAsync(this GitHubClient client, string owner, string name, bool includePreReleases)
        {
            var latest = await client.Repository.Release.GetLatest(owner, name);
            if (!includePreReleases) return latest;

            var latestPre = await GetPreReleaseAsync(client, owner, name);
            return new ExtendedVersion(latestPre.TagName) > new ExtendedVersion(latest.TagName) ? latestPre : latest;
        }
    }
}
