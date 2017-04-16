using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Http;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Flantter.MilkyWay.Models.Twitter.Wrapper
{
    public static class Util
    {
        public static TValue GetValueOrDefault<TKey, TValue>
        (this IDictionary<TKey, TValue> dictionary,
            TKey key,
            TValue defaultValue)
        {
            TValue value;
            return dictionary.TryGetValue(key, out value) ? value : defaultValue;
        }

        /*public static TValue GetValueOrDefault<TKey, TValue>
        (this IDictionary<TKey, TValue> dictionary,
            TKey key,
            Func<TValue> defaultValueProvider)
        {
            TValue value;
            return dictionary.TryGetValue(key, out value)
                ? value
                : defaultValueProvider();
        }*/
    }

    public class CursoredList<T> : List<T>
    {
        public CursoredList(IEnumerable<T> collection) : base(collection)
        {
        }

        public long NextCursor { get; set; }

        public long PreviousCursor { get; set; }
    }

    public class StringCursoredList<T> : List<T>
    {
        public StringCursoredList(IEnumerable<T> collection) : base(collection)
        {
        }

        public string NextCursor { get; set; }

        public string PreviousCursor { get; set; }
    }

    public abstract class ApiBase
    {
        protected object GetExpressionValue(Expression<Func<string, object>> expr)
        {
            var constExpr = expr.Body as ConstantExpression;
            return constExpr != null ? constExpr.Value : expr.Compile()("");
        }

        protected IDictionary<string, object> ExpressionToDictionary(Expression<Func<string, object>>[] parameters)
        {
            return parameters.Select(x => new KeyValuePair<string, object>(x.Parameters[0].Name, GetExpressionValue(x)))
                .ToDictionary(x => x.Key, x => x.Value);
        }

        public Tokens Tokens { get; set; }
    }

    public class Favorites : ApiBase
    {
        public Task<Twitter.Objects.Status> CreateAsync(params Expression<Func<string, object>>[] parameters)
        {
            return this.CreateAsyncImpl(ExpressionToDictionary(parameters));
        }

        public Task<Twitter.Objects.Status> CreateAsync(IDictionary<string, object> parameters)
        {
            return this.CreateAsyncImpl(parameters);
        }

        private async Task<Twitter.Objects.Status> CreateAsyncImpl(IDictionary<string, object> parameters)
        {
            switch (Tokens.Platform)
            {
                case Tokens.PlatformEnum.Twitter:
                    return new Twitter.Objects.Status(await Tokens.TwitterTokens.Favorites.CreateAsync(parameters));
                case Tokens.PlatformEnum.Mastodon:
                    return new Twitter.Objects.Status(await Tokens.MastodonTokens.Favourite((long) parameters["id"]));
            }
            throw new NotImplementedException();
        }

        public Task<Twitter.Objects.Status> DestroyAsync(params Expression<Func<string, object>>[] parameters)
        {
            return this.DestroyAsyncImpl(ExpressionToDictionary(parameters));
        }

        public Task<Twitter.Objects.Status> DestroyAsync(IDictionary<string, object> parameters)
        {
            return this.DestroyAsyncImpl(parameters);
        }

        private async Task<Twitter.Objects.Status> DestroyAsyncImpl(IDictionary<string, object> parameters)
        {
            switch (Tokens.Platform)
            {
                case Tokens.PlatformEnum.Twitter:
                    return new Twitter.Objects.Status(await Tokens.TwitterTokens.Favorites.DestroyAsync(parameters));
                case Tokens.PlatformEnum.Mastodon:
                    return new Twitter.Objects.Status(await Tokens.MastodonTokens.Unfavourite((long) parameters["id"]));
            }
            throw new NotImplementedException();
        }

        public Task<List<Twitter.Objects.Status>> ListAsync(params Expression<Func<string, object>>[] parameters)
        {
            return this.ListAsyncImpl(ExpressionToDictionary(parameters));
        }

        public Task<List<Twitter.Objects.Status>> ListAsync(IDictionary<string, object> parameters)
        {
            return this.ListAsyncImpl(parameters);
        }

        private async Task<List<Twitter.Objects.Status>> ListAsyncImpl(IDictionary<string, object> parameters)
        {
            switch (Tokens.Platform)
            {
                case Tokens.PlatformEnum.Twitter:
                    return (await Tokens.TwitterTokens.Favorites.ListAsync(parameters))
                        .Select(x => new Twitter.Objects.Status(x))
                        .ToList();
                case Tokens.PlatformEnum.Mastodon:
                    if (parameters.ContainsKey("id") || parameters.ContainsKey("screen_name"))
                        throw new NotImplementedException();
                    return (await Tokens.MastodonTokens.GetFavourites()).Select(x => new Twitter.Objects.Status(x))
                        .ToList();
            }
            throw new NotImplementedException();
        }
    }

    public class Users : ApiBase
    {
        public Task<List<Twitter.Objects.User>> LookupAsync(params Expression<Func<string, object>>[] parameters)
        {
            return this.LookupAsyncImpl(ExpressionToDictionary(parameters));
        }

        public Task<List<Twitter.Objects.User>> LookupAsync(IDictionary<string, object> parameters)
        {
            return this.LookupAsyncImpl(parameters);
        }

        private async Task<List<Twitter.Objects.User>> LookupAsyncImpl(IDictionary<string, object> parameters)
        {
            switch (Tokens.Platform)
            {
                case Tokens.PlatformEnum.Twitter:
                    return (await Tokens.TwitterTokens.Users.LookupAsync(parameters))
                        .Select(x => new Twitter.Objects.User(x))
                        .ToList();
                case Tokens.PlatformEnum.Mastodon:
                    throw new NotImplementedException();
            }
            throw new NotImplementedException();
        }

        public Task<Twitter.Objects.User> ReportSpamAsync(params Expression<Func<string, object>>[] parameters)
        {
            return this.ReportSpamAsyncImpl(ExpressionToDictionary(parameters));
        }

        public Task<Twitter.Objects.User> ReportSpamAsync(IDictionary<string, object> parameters)
        {
            return this.ReportSpamAsyncImpl(parameters);
        }

        private async Task<Twitter.Objects.User> ReportSpamAsyncImpl(IDictionary<string, object> parameters)
        {
            switch (Tokens.Platform)
            {
                case Tokens.PlatformEnum.Twitter:
                    return new Twitter.Objects.User(await Tokens.TwitterTokens.Users.ReportSpamAsync(parameters));
                case Tokens.PlatformEnum.Mastodon:
                    throw new NotImplementedException();
            }
            throw new NotImplementedException();
        }

        public Task<List<Twitter.Objects.User>> SearchAsync(params Expression<Func<string, object>>[] parameters)
        {
            return this.SearchAsyncImpl(ExpressionToDictionary(parameters));
        }

        public Task<List<Twitter.Objects.User>> SearchAsync(IDictionary<string, object> parameters)
        {
            return this.SearchAsyncImpl(parameters);
        }

        private async Task<List<Twitter.Objects.User>> SearchAsyncImpl(IDictionary<string, object> parameters)
        {
            switch (Tokens.Platform)
            {
                case Tokens.PlatformEnum.Twitter:
                    return (await Tokens.TwitterTokens.Users.SearchAsync(parameters))
                        .Select(x => new Twitter.Objects.User(x))
                        .ToList();
                case Tokens.PlatformEnum.Mastodon:
                    if (parameters.ContainsKey("page"))
                        throw new NotImplementedException();
                    return (await Tokens.MastodonTokens.SearchAccounts((string) parameters["q"],
                            (int) parameters.GetValueOrDefault("count", 10))).Select(x => new Twitter.Objects.User(x))
                        .ToList();
            }
            throw new NotImplementedException();
        }

        public Task<Twitter.Objects.User> ShowAsync(params Expression<Func<string, object>>[] parameters)
        {
            return this.ShowAsyncImpl(ExpressionToDictionary(parameters));
        }

        public Task<Twitter.Objects.User> ShowAsync(IDictionary<string, object> parameters)
        {
            return this.ShowAsyncImpl(parameters);
        }

        private async Task<Twitter.Objects.User> ShowAsyncImpl(IDictionary<string, object> parameters)
        {
            switch (Tokens.Platform)
            {
                case Tokens.PlatformEnum.Twitter:
                    return new Twitter.Objects.User(await Tokens.TwitterTokens.Users.ShowAsync(parameters));
                case Tokens.PlatformEnum.Mastodon:
                    return new Twitter.Objects.User(
                        await Tokens.MastodonTokens.GetAccount((long) parameters["user_id"]));
            }
            throw new NotImplementedException();
        }
    }

    public class Trends : ApiBase
    {
        public Task<List<Twitter.Objects.Trend>> PlaceAsync(params Expression<Func<string, object>>[] parameters)
        {
            return this.PlaceAsyncImpl(ExpressionToDictionary(parameters));
        }

        public Task<List<Twitter.Objects.Trend>> PlaceAsync(IDictionary<string, object> parameters)
        {
            return this.PlaceAsyncImpl(parameters);
        }

        private async Task<List<Twitter.Objects.Trend>> PlaceAsyncImpl(IDictionary<string, object> parameters)
        {
            switch (Tokens.Platform)
            {
                case Tokens.PlatformEnum.Twitter:
                    return (await Tokens.TwitterTokens.Trends.PlaceAsync(parameters)).SelectMany(x => x.Trends)
                        .Select(x => new Twitter.Objects.Trend(x))
                        .ToList();
                case Tokens.PlatformEnum.Mastodon:
                    throw new NotImplementedException();
            }
            throw new NotImplementedException();
        }
    }

    public class Statuses : ApiBase
    {
        public Task<Twitter.Objects.Status> DestroyAsync(params Expression<Func<string, object>>[] parameters)
        {
            return this.DestroyAsyncImpl(ExpressionToDictionary(parameters));
        }

        public Task<Twitter.Objects.Status> DestroyAsync(IDictionary<string, object> parameters)
        {
            return this.DestroyAsyncImpl(parameters);
        }

        private async Task<Twitter.Objects.Status> DestroyAsyncImpl(IDictionary<string, object> parameters)
        {
            switch (Tokens.Platform)
            {
                case Tokens.PlatformEnum.Twitter:
                    return new Twitter.Objects.Status(await Tokens.TwitterTokens.Statuses.DestroyAsync(parameters));
                case Tokens.PlatformEnum.Mastodon:
                    await Tokens.MastodonTokens.DeleteStatus((long) parameters["id"]);
                    return null;
            }
            throw new NotImplementedException();
        }

        public Task<List<Twitter.Objects.Status>> HomeTimelineAsync(
            params Expression<Func<string, object>>[] parameters)
        {
            return this.HomeTimelineAsyncImpl(ExpressionToDictionary(parameters));
        }

        public Task<List<Twitter.Objects.Status>> HomeTimelineAsync(IDictionary<string, object> parameters)
        {
            return this.HomeTimelineAsyncImpl(parameters);
        }

        private async Task<List<Twitter.Objects.Status>> HomeTimelineAsyncImpl(IDictionary<string, object> parameters)
        {
            switch (Tokens.Platform)
            {
                case Tokens.PlatformEnum.Twitter:
                    return (await Tokens.TwitterTokens.Statuses.HomeTimelineAsync(parameters))
                        .Select(x => new Twitter.Objects.Status(x))
                        .ToList();
                case Tokens.PlatformEnum.Mastodon:
                    return (await Tokens.MastodonTokens.GetHomeTimeline(
                            (long) parameters.GetValueOrDefault("max_id", (long) 0),
                            (long) parameters.GetValueOrDefault("since_id", (long) 0),
                            (int) parameters.GetValueOrDefault("count", 10))).Select(x => new Twitter.Objects.Status(x))
                        .ToList();
            }
            throw new NotImplementedException();
        }

        public Task<List<Twitter.Objects.Status>> LookupAsync(params Expression<Func<string, object>>[] parameters)
        {
            return this.LookupAsyncImpl(ExpressionToDictionary(parameters));
        }

        public Task<List<Twitter.Objects.Status>> LookupAsync(IDictionary<string, object> parameters)
        {
            return this.LookupAsyncImpl(parameters);
        }

        private async Task<List<Twitter.Objects.Status>> LookupAsyncImpl(IDictionary<string, object> parameters)
        {
            switch (Tokens.Platform)
            {
                case Tokens.PlatformEnum.Twitter:
                    return (await Tokens.TwitterTokens.Statuses.LookupAsync(parameters))
                        .Select(x => new Twitter.Objects.Status(x))
                        .ToList();
                case Tokens.PlatformEnum.Mastodon:
                    throw new NotImplementedException();
            }
            throw new NotImplementedException();
        }

        public Task<List<Twitter.Objects.Status>> MentionsTimelineAsync(
            params Expression<Func<string, object>>[] parameters)
        {
            return this.MentionsTimelineAsyncImpl(ExpressionToDictionary(parameters));
        }

        public Task<List<Twitter.Objects.Status>> MentionsTimelineAsync(IDictionary<string, object> parameters)
        {
            return this.MentionsTimelineAsyncImpl(parameters);
        }

        private async Task<List<Twitter.Objects.Status>> MentionsTimelineAsyncImpl(
            IDictionary<string, object> parameters)
        {
            switch (Tokens.Platform)
            {
                case Tokens.PlatformEnum.Twitter:
                    return (await Tokens.TwitterTokens.Statuses.MentionsTimelineAsync(parameters))
                        .Select(x => new Twitter.Objects.Status(x))
                        .ToList();
                case Tokens.PlatformEnum.Mastodon:
                    return (await Tokens.MastodonTokens.GetNotifications())
                        .Where(x => x.Type == "mention" && x.Status.Visibility != "direct")
                        .Select(x => new Twitter.Objects.Status(x.Status))
                        .ToList();
            }
            throw new NotImplementedException();
        }

        public Task<Twitter.Objects.Status> RetweetAsync(params Expression<Func<string, object>>[] parameters)
        {
            return this.RetweetAsyncImpl(ExpressionToDictionary(parameters));
        }

        public Task<Twitter.Objects.Status> RetweetAsync(IDictionary<string, object> parameters)
        {
            return this.RetweetAsyncImpl(parameters);
        }

        private async Task<Twitter.Objects.Status> RetweetAsyncImpl(IDictionary<string, object> parameters)
        {
            switch (Tokens.Platform)
            {
                case Tokens.PlatformEnum.Twitter:
                    return new Twitter.Objects.Status(await Tokens.TwitterTokens.Statuses.RetweetAsync(parameters));
                case Tokens.PlatformEnum.Mastodon:
                    return new Twitter.Objects.Status(await Tokens.MastodonTokens.Reblog((long) parameters["id"]));
            }
            throw new NotImplementedException();
        }

        public Task<CursoredList<long>> RetweetersIdsAsync(params Expression<Func<string, object>>[] parameters)
        {
            return this.RetweetersIdsAsyncImpl(ExpressionToDictionary(parameters));
        }

        public Task<CursoredList<long>> RetweetersIdsAsync(IDictionary<string, object> parameters)
        {
            return this.RetweetersIdsAsyncImpl(parameters);
        }

        private async Task<CursoredList<long>> RetweetersIdsAsyncImpl(IDictionary<string, object> parameters)
        {
            switch (Tokens.Platform)
            {
                case Tokens.PlatformEnum.Twitter:
                    var response = await Tokens.TwitterTokens.Statuses.RetweetersIdsAsync(parameters);
                    var list = new CursoredList<long>(response);
                    list.NextCursor = response.NextCursor;
                    list.PreviousCursor = response.PreviousCursor;
                    return list;
                case Tokens.PlatformEnum.Mastodon:
                    return new CursoredList<long>((await Tokens.MastodonTokens.GetRebloggedBy((long) parameters["id"]))
                        .Select(x => (long) x.Id));
            }
            throw new NotImplementedException();
        }

        public Task<CursoredList<Twitter.Objects.User>> RetweetersAsync(
            params Expression<Func<string, object>>[] parameters)
        {
            return this.RetweetersAsyncImpl(ExpressionToDictionary(parameters));
        }

        public Task<CursoredList<Twitter.Objects.User>> RetweetersAsync(IDictionary<string, object> parameters)
        {
            return this.RetweetersAsyncImpl(parameters);
        }

        private async Task<CursoredList<Twitter.Objects.User>> RetweetersAsyncImpl(
            IDictionary<string, object> parameters)
        {
            switch (Tokens.Platform)
            {
                case Tokens.PlatformEnum.Twitter:
                    var response = await Tokens.TwitterTokens.Statuses.RetweetersIdsAsync(parameters);
                    var list =
                        new CursoredList<Twitter.Objects.User>(
                            (await Tokens.TwitterTokens.Users.LookupAsync(user_id => response)).Select(
                                x => new Twitter.Objects.User(x)));
                    list.NextCursor = response.NextCursor;
                    list.PreviousCursor = response.PreviousCursor;
                    return list;
                case Tokens.PlatformEnum.Mastodon:
                    return new CursoredList<Twitter.Objects.User>(
                        (await Tokens.MastodonTokens.GetRebloggedBy((long) parameters["id"])).Select(
                            x => new Twitter.Objects.User(x)));
            }
            throw new NotImplementedException();
        }

        public Task<List<Twitter.Objects.Status>> RetweetsOfMeAsync(
            params Expression<Func<string, object>>[] parameters)
        {
            return this.RetweetsOfMeAsyncImpl(ExpressionToDictionary(parameters));
        }

        public Task<List<Twitter.Objects.Status>> RetweetsOfMeAsync(IDictionary<string, object> parameters)
        {
            return this.RetweetsOfMeAsyncImpl(parameters);
        }

        private async Task<List<Twitter.Objects.Status>> RetweetsOfMeAsyncImpl(IDictionary<string, object> parameters)
        {
            switch (Tokens.Platform)
            {
                case Tokens.PlatformEnum.Twitter:
                    return (await Tokens.TwitterTokens.Statuses.RetweetsOfMeAsync(parameters))
                        .Select(x => new Twitter.Objects.Status(x))
                        .ToList();
                case Tokens.PlatformEnum.Mastodon:
                    throw new NotImplementedException();
            }
            throw new NotImplementedException();
        }

        public Task<Twitter.Objects.Status> ShowAsync(params Expression<Func<string, object>>[] parameters)
        {
            return this.ShowAsyncImpl(ExpressionToDictionary(parameters));
        }

        public Task<Twitter.Objects.Status> ShowAsync(IDictionary<string, object> parameters)
        {
            return this.ShowAsyncImpl(parameters);
        }

        private async Task<Twitter.Objects.Status> ShowAsyncImpl(IDictionary<string, object> parameters)
        {
            switch (Tokens.Platform)
            {
                case Tokens.PlatformEnum.Twitter:
                    return new Twitter.Objects.Status(await Tokens.TwitterTokens.Statuses.ShowAsync(parameters));
                case Tokens.PlatformEnum.Mastodon:
                    return new Twitter.Objects.Status(await Tokens.MastodonTokens.GetStatus((long) parameters["id"]));
            }
            throw new NotImplementedException();
        }

        public Task<Twitter.Objects.Status> UnretweetAsync(params Expression<Func<string, object>>[] parameters)
        {
            return this.UnretweetAsyncImpl(ExpressionToDictionary(parameters));
        }

        public Task<Twitter.Objects.Status> UnretweetAsync(IDictionary<string, object> parameters)
        {
            return this.UnretweetAsyncImpl(parameters);
        }

        private async Task<Twitter.Objects.Status> UnretweetAsyncImpl(IDictionary<string, object> parameters)
        {
            switch (Tokens.Platform)
            {
                case Tokens.PlatformEnum.Twitter:
                    return new Twitter.Objects.Status(await Tokens.TwitterTokens.Statuses.UnretweetAsync(parameters));
                case Tokens.PlatformEnum.Mastodon:
                    return new Twitter.Objects.Status(await Tokens.MastodonTokens.Unreblog((long) parameters["id"]));
            }
            throw new NotImplementedException();
        }

        public Task<Twitter.Objects.Status> UpdateAsync(params Expression<Func<string, object>>[] parameters)
        {
            return this.UpdateAsyncImpl(ExpressionToDictionary(parameters));
        }

        public Task<Twitter.Objects.Status> UpdateAsync(IDictionary<string, object> parameters)
        {
            return this.UpdateAsyncImpl(parameters);
        }

        private async Task<Twitter.Objects.Status> UpdateAsyncImpl(IDictionary<string, object> parameters)
        {
            switch (Tokens.Platform)
            {
                case Tokens.PlatformEnum.Twitter:
                    return new Twitter.Objects.Status(await Tokens.TwitterTokens.Statuses.UpdateAsync(parameters));
                case Tokens.PlatformEnum.Mastodon:
                    return new Twitter.Objects.Status(await Tokens.MastodonTokens.PostStatus(
                        (string) parameters["status"], Mastonet.Visibility.Public,
                        (int?) parameters.GetValueOrDefault("in_reply_to_status_id", null),
                        (IEnumerable<long>) parameters.GetValueOrDefault("media_ids", null),
                        (bool) parameters.GetValueOrDefault("possibly_sensitive", false)));
            }
            throw new NotImplementedException();
        }

        public Task<List<Twitter.Objects.Status>> UserTimelineAsync(
            params Expression<Func<string, object>>[] parameters)
        {
            return this.UserTimelineAsyncImpl(ExpressionToDictionary(parameters));
        }

        public Task<List<Twitter.Objects.Status>> UserTimelineAsync(IDictionary<string, object> parameters)
        {
            return this.UserTimelineAsyncImpl(parameters);
        }

        private async Task<List<Twitter.Objects.Status>> UserTimelineAsyncImpl(IDictionary<string, object> parameters)
        {
            switch (Tokens.Platform)
            {
                case Tokens.PlatformEnum.Twitter:
                    return (await Tokens.TwitterTokens.Statuses.UserTimelineAsync(parameters))
                        .Select(x => new Twitter.Objects.Status(x))
                        .ToList();
                case Tokens.PlatformEnum.Mastodon:
                    return (await Tokens.MastodonTokens.GetAccountStatuses((long) parameters["user_id"],
                            (long) parameters.GetValueOrDefault("max_id", (long) 0),
                            (long) parameters.GetValueOrDefault("since_id", (long) 0),
                            (int) parameters.GetValueOrDefault("count", 10))).Select(x => new Twitter.Objects.Status(x))
                        .ToList();
            }
            throw new NotImplementedException();
        }
    }

    public class SavedSearches : ApiBase
    {
        public Task<Twitter.Objects.SearchQuery> CreateAsync(params Expression<Func<string, object>>[] parameters)
        {
            return this.CreateAsyncImpl(ExpressionToDictionary(parameters));
        }

        public Task<Twitter.Objects.SearchQuery> CreateAsync(IDictionary<string, object> parameters)
        {
            return this.CreateAsyncImpl(parameters);
        }

        private async Task<Twitter.Objects.SearchQuery> CreateAsyncImpl(IDictionary<string, object> parameters)
        {
            switch (Tokens.Platform)
            {
                case Tokens.PlatformEnum.Twitter:
                    return new Twitter.Objects.SearchQuery(
                        await Tokens.TwitterTokens.SavedSearches.CreateAsync(parameters));
                case Tokens.PlatformEnum.Mastodon:
                    throw new NotImplementedException();
            }
            throw new NotImplementedException();
        }

        public Task<Twitter.Objects.SearchQuery> DestroyAsync(params Expression<Func<string, object>>[] parameters)
        {
            return this.DestroyAsyncImpl(ExpressionToDictionary(parameters));
        }

        public Task<Twitter.Objects.SearchQuery> DestroyAsync(IDictionary<string, object> parameters)
        {
            return this.DestroyAsyncImpl(parameters);
        }

        private async Task<Twitter.Objects.SearchQuery> DestroyAsyncImpl(IDictionary<string, object> parameters)
        {
            switch (Tokens.Platform)
            {
                case Tokens.PlatformEnum.Twitter:
                    return new Twitter.Objects.SearchQuery(
                        await Tokens.TwitterTokens.SavedSearches.DestroyAsync(parameters));
                case Tokens.PlatformEnum.Mastodon:
                    throw new NotImplementedException();
            }
            throw new NotImplementedException();
        }

        public Task<List<Twitter.Objects.SearchQuery>> ListAsync(params Expression<Func<string, object>>[] parameters)
        {
            return this.ListAsyncImpl(ExpressionToDictionary(parameters));
        }

        public Task<List<Twitter.Objects.SearchQuery>> ListAsync(IDictionary<string, object> parameters)
        {
            return this.ListAsyncImpl(parameters);
        }

        private async Task<List<Twitter.Objects.SearchQuery>> ListAsyncImpl(IDictionary<string, object> parameters)
        {
            switch (Tokens.Platform)
            {
                case Tokens.PlatformEnum.Twitter:
                    return (await Tokens.TwitterTokens.SavedSearches.ListAsync(parameters))
                        .Select(x => new Twitter.Objects.SearchQuery(x))
                        .ToList();
                case Tokens.PlatformEnum.Mastodon:
                    throw new NotImplementedException();
            }
            throw new NotImplementedException();
        }
    }

    public class Search : ApiBase
    {
        public Task<List<Twitter.Objects.Status>> TweetsAsync(params Expression<Func<string, object>>[] parameters)
        {
            return this.TweetsAsyncImpl(ExpressionToDictionary(parameters));
        }

        public Task<List<Twitter.Objects.Status>> TweetsAsync(IDictionary<string, object> parameters)
        {
            return this.TweetsAsyncImpl(parameters);
        }

        private async Task<List<Twitter.Objects.Status>> TweetsAsyncImpl(IDictionary<string, object> parameters)
        {
            switch (Tokens.Platform)
            {
                case Tokens.PlatformEnum.Twitter:
                    return (await Tokens.TwitterTokens.Search.TweetsAsync(parameters))
                        .Select(x => new Twitter.Objects.Status(x))
                        .ToList();
                case Tokens.PlatformEnum.Mastodon:
                    return (await Tokens.MastodonTokens.Search((string) parameters["q"], true)).Statuses
                        .Select(x => new Twitter.Objects.Status(x))
                        .ToList();
            }
            throw new NotImplementedException();
        }
    }

    public class Mutes : ApiBase
    {
        public MutesUsers Users { get; set; }
    }

    public class MutesUsers : ApiBase
    {
        public Task<Twitter.Objects.User> CreateAsync(params Expression<Func<string, object>>[] parameters)
        {
            return this.CreateAsyncImpl(ExpressionToDictionary(parameters));
        }

        public Task<Twitter.Objects.User> CreateAsync(IDictionary<string, object> parameters)
        {
            return this.CreateAsyncImpl(parameters);
        }

        private async Task<Twitter.Objects.User> CreateAsyncImpl(IDictionary<string, object> parameters)
        {
            switch (Tokens.Platform)
            {
                case Tokens.PlatformEnum.Twitter:
                    return new Twitter.Objects.User(await Tokens.TwitterTokens.Mutes.Users.CreateAsync(parameters));
                case Tokens.PlatformEnum.Mastodon:
                    return new Twitter.Objects.User(await Tokens.MastodonTokens.Mute((long) parameters["user_id"]));
            }
            throw new NotImplementedException();
        }

        public Task<Twitter.Objects.User> DestroyAsync(params Expression<Func<string, object>>[] parameters)
        {
            return this.DestroyAsyncImpl(ExpressionToDictionary(parameters));
        }

        public Task<Twitter.Objects.User> DestroyAsync(IDictionary<string, object> parameters)
        {
            return this.DestroyAsyncImpl(parameters);
        }

        private async Task<Twitter.Objects.User> DestroyAsyncImpl(IDictionary<string, object> parameters)
        {
            switch (Tokens.Platform)
            {
                case Tokens.PlatformEnum.Twitter:
                    return new Twitter.Objects.User(await Tokens.TwitterTokens.Mutes.Users.DestroyAsync(parameters));
                case Tokens.PlatformEnum.Mastodon:
                    return new Twitter.Objects.User(await Tokens.MastodonTokens.Unmute((long) parameters["user_id"]));
            }
            throw new NotImplementedException();
        }

        public Task<CursoredList<long>> IdsAsync(params Expression<Func<string, object>>[] parameters)
        {
            return this.IdsAsyncImpl(ExpressionToDictionary(parameters));
        }

        public Task<CursoredList<long>> IdsAsync(IDictionary<string, object> parameters)
        {
            return this.IdsAsyncImpl(parameters);
        }

        private async Task<CursoredList<long>> IdsAsyncImpl(IDictionary<string, object> parameters)
        {
            switch (Tokens.Platform)
            {
                case Tokens.PlatformEnum.Twitter:
                    var response = await Tokens.TwitterTokens.Mutes.Users.IdsAsync(parameters);
                    var list = new CursoredList<long>(response);
                    list.NextCursor = response.NextCursor;
                    list.PreviousCursor = response.PreviousCursor;
                    return list;
                case Tokens.PlatformEnum.Mastodon:
                    return new CursoredList<long>((await Tokens.MastodonTokens.GetMutes()).Select(x => (long) x.Id));
            }
            throw new NotImplementedException();
        }

        public Task<CursoredList<Twitter.Objects.User>> ListAsync(params Expression<Func<string, object>>[] parameters)
        {
            return this.ListAsyncImpl(ExpressionToDictionary(parameters));
        }

        public Task<CursoredList<Twitter.Objects.User>> ListAsync(IDictionary<string, object> parameters)
        {
            return this.ListAsyncImpl(parameters);
        }

        private async Task<CursoredList<Twitter.Objects.User>> ListAsyncImpl(IDictionary<string, object> parameters)
        {
            switch (Tokens.Platform)
            {
                case Tokens.PlatformEnum.Twitter:
                    var response = await Tokens.TwitterTokens.Mutes.Users.ListAsync(parameters);
                    var list =
                        new CursoredList<Twitter.Objects.User>(response.Select(x => new Twitter.Objects.User(x)));
                    list.NextCursor = response.NextCursor;
                    list.PreviousCursor = response.PreviousCursor;
                    return list;
                case Tokens.PlatformEnum.Mastodon:
                    return new CursoredList<Twitter.Objects.User>(
                        (await Tokens.MastodonTokens.GetMutes()).Select(x => new Twitter.Objects.User(x)));
            }
            throw new NotImplementedException();
        }
    }

    public class Lists : ApiBase
    {
        public ListsSubscribers Subscribers { get; set; }
        public ListsMembers Members { get; set; }

        public Task<CursoredList<Twitter.Objects.List>> MembershipsAsync(
            params Expression<Func<string, object>>[] parameters)
        {
            return this.MembershipsAsyncImpl(ExpressionToDictionary(parameters));
        }

        public Task<CursoredList<Twitter.Objects.List>> MembershipsAsync(IDictionary<string, object> parameters)
        {
            return this.MembershipsAsyncImpl(parameters);
        }

        private async Task<CursoredList<Twitter.Objects.List>> MembershipsAsyncImpl(
            IDictionary<string, object> parameters)
        {
            switch (Tokens.Platform)
            {
                case Tokens.PlatformEnum.Twitter:
                    var response = await Tokens.TwitterTokens.Lists.MembershipsAsync(parameters);
                    var list =
                        new CursoredList<Twitter.Objects.List>(response.Select(x => new Twitter.Objects.List(x)));
                    list.NextCursor = response.NextCursor;
                    list.PreviousCursor = response.PreviousCursor;
                    return list;
                case Tokens.PlatformEnum.Mastodon:
                    throw new NotImplementedException();
            }
            throw new NotImplementedException();
        }

        public Task<CursoredList<Twitter.Objects.List>> OwnershipsAsync(
            params Expression<Func<string, object>>[] parameters)
        {
            return this.OwnershipsAsyncImpl(ExpressionToDictionary(parameters));
        }

        public Task<CursoredList<Twitter.Objects.List>> OwnershipsAsync(IDictionary<string, object> parameters)
        {
            return this.OwnershipsAsyncImpl(parameters);
        }

        private async Task<CursoredList<Twitter.Objects.List>> OwnershipsAsyncImpl(
            IDictionary<string, object> parameters)
        {
            switch (Tokens.Platform)
            {
                case Tokens.PlatformEnum.Twitter:
                    var response = await Tokens.TwitterTokens.Lists.OwnershipsAsync(parameters);
                    var list =
                        new CursoredList<Twitter.Objects.List>(response.Select(x => new Twitter.Objects.List(x)));
                    list.NextCursor = response.NextCursor;
                    list.PreviousCursor = response.PreviousCursor;
                    return list;
                case Tokens.PlatformEnum.Mastodon:
                    throw new NotImplementedException();
            }
            throw new NotImplementedException();
        }

        public Task<List<Twitter.Objects.Status>> StatusesAsync(params Expression<Func<string, object>>[] parameters)
        {
            return this.StatusesAsyncImpl(ExpressionToDictionary(parameters));
        }

        public Task<List<Twitter.Objects.Status>> StatusesAsync(IDictionary<string, object> parameters)
        {
            return this.StatusesAsyncImpl(parameters);
        }

        private async Task<List<Twitter.Objects.Status>> StatusesAsyncImpl(IDictionary<string, object> parameters)
        {
            switch (Tokens.Platform)
            {
                case Tokens.PlatformEnum.Twitter:
                    return (await Tokens.TwitterTokens.Lists.StatusesAsync(parameters))
                        .Select(x => new Twitter.Objects.Status(x))
                        .ToList();
                case Tokens.PlatformEnum.Mastodon:
                    throw new NotImplementedException();
            }
            throw new NotImplementedException();
        }

        public Task<CursoredList<Twitter.Objects.List>> SubscriptionsAsync(
            params Expression<Func<string, object>>[] parameters)
        {
            return this.SubscriptionsAsyncImpl(ExpressionToDictionary(parameters));
        }

        public Task<CursoredList<Twitter.Objects.List>> SubscriptionsAsync(IDictionary<string, object> parameters)
        {
            return this.SubscriptionsAsyncImpl(parameters);
        }

        private async Task<CursoredList<Twitter.Objects.List>> SubscriptionsAsyncImpl(
            IDictionary<string, object> parameters)
        {
            switch (Tokens.Platform)
            {
                case Tokens.PlatformEnum.Twitter:
                    var response = await Tokens.TwitterTokens.Lists.SubscriptionsAsync(parameters);
                    var list =
                        new CursoredList<Twitter.Objects.List>(response.Select(x => new Twitter.Objects.List(x)));
                    list.NextCursor = response.NextCursor;
                    list.PreviousCursor = response.PreviousCursor;
                    return list;
                case Tokens.PlatformEnum.Mastodon:
                    throw new NotImplementedException();
            }
            throw new NotImplementedException();
        }
    }

    public class ListsSubscribers : ApiBase
    {
        public Task<Twitter.Objects.List> CreateAsync(params Expression<Func<string, object>>[] parameters)
        {
            return this.CreateAsyncImpl(ExpressionToDictionary(parameters));
        }

        public Task<Twitter.Objects.List> CreateAsync(IDictionary<string, object> parameters)
        {
            return this.CreateAsyncImpl(parameters);
        }

        private async Task<Twitter.Objects.List> CreateAsyncImpl(IDictionary<string, object> parameters)
        {
            switch (Tokens.Platform)
            {
                case Tokens.PlatformEnum.Twitter:
                    return new Twitter.Objects.List(
                        await Tokens.TwitterTokens.Lists.Subscribers.CreateAsync(parameters));
                case Tokens.PlatformEnum.Mastodon:
                    throw new NotImplementedException();
            }
            throw new NotImplementedException();
        }

        public Task<Twitter.Objects.List> DestroyAsync(params Expression<Func<string, object>>[] parameters)
        {
            return this.DestroyAsyncImpl(ExpressionToDictionary(parameters));
        }

        public Task<Twitter.Objects.List> DestroyAsync(IDictionary<string, object> parameters)
        {
            return this.DestroyAsyncImpl(parameters);
        }

        private async Task<Twitter.Objects.List> DestroyAsyncImpl(IDictionary<string, object> parameters)
        {
            switch (Tokens.Platform)
            {
                case Tokens.PlatformEnum.Twitter:
                    return new Twitter.Objects.List(
                        await Tokens.TwitterTokens.Lists.Subscribers.DestroyAsync(parameters));
                case Tokens.PlatformEnum.Mastodon:
                    throw new NotImplementedException();
            }
            throw new NotImplementedException();
        }

        public Task<CursoredList<Twitter.Objects.User>> ListAsync(params Expression<Func<string, object>>[] parameters)
        {
            return this.ListAsyncImpl(ExpressionToDictionary(parameters));
        }

        public Task<CursoredList<Twitter.Objects.User>> ListAsync(IDictionary<string, object> parameters)
        {
            return this.ListAsyncImpl(parameters);
        }

        private async Task<CursoredList<Twitter.Objects.User>> ListAsyncImpl(IDictionary<string, object> parameters)
        {
            switch (Tokens.Platform)
            {
                case Tokens.PlatformEnum.Twitter:
                    var response = await Tokens.TwitterTokens.Lists.Subscribers.ListAsync(parameters);
                    var list =
                        new CursoredList<Twitter.Objects.User>(response.Select(x => new Twitter.Objects.User(x)));
                    list.NextCursor = response.NextCursor;
                    list.PreviousCursor = response.PreviousCursor;
                    return list;
                case Tokens.PlatformEnum.Mastodon:
                    throw new NotImplementedException();
            }
            throw new NotImplementedException();
        }
    }

    public class ListsMembers : ApiBase
    {
        public Task<Twitter.Objects.List> CreateAsync(params Expression<Func<string, object>>[] parameters)
        {
            return this.CreateAsyncImpl(ExpressionToDictionary(parameters));
        }

        public Task<Twitter.Objects.List> CreateAsync(IDictionary<string, object> parameters)
        {
            return this.CreateAsyncImpl(parameters);
        }

        private async Task<Twitter.Objects.List> CreateAsyncImpl(IDictionary<string, object> parameters)
        {
            switch (Tokens.Platform)
            {
                case Tokens.PlatformEnum.Twitter:
                    return new Twitter.Objects.List(await Tokens.TwitterTokens.Lists.Members.CreateAsync(parameters));
                case Tokens.PlatformEnum.Mastodon:
                    throw new NotImplementedException();
            }
            throw new NotImplementedException();
        }

        public Task<Twitter.Objects.List> DestroyAsync(params Expression<Func<string, object>>[] parameters)
        {
            return this.DestroyAsyncImpl(ExpressionToDictionary(parameters));
        }

        public Task<Twitter.Objects.List> DestroyAsync(IDictionary<string, object> parameters)
        {
            return this.DestroyAsyncImpl(parameters);
        }

        private async Task<Twitter.Objects.List> DestroyAsyncImpl(IDictionary<string, object> parameters)
        {
            switch (Tokens.Platform)
            {
                case Tokens.PlatformEnum.Twitter:
                    return new Twitter.Objects.List(await Tokens.TwitterTokens.Lists.Members.DestroyAsync(parameters));
                case Tokens.PlatformEnum.Mastodon:
                    throw new NotImplementedException();
            }
            throw new NotImplementedException();
        }

        public Task<CursoredList<Twitter.Objects.User>> ListAsync(params Expression<Func<string, object>>[] parameters)
        {
            return this.ListAsyncImpl(ExpressionToDictionary(parameters));
        }

        public Task<CursoredList<Twitter.Objects.User>> ListAsync(IDictionary<string, object> parameters)
        {
            return this.ListAsyncImpl(parameters);
        }

        private async Task<CursoredList<Twitter.Objects.User>> ListAsyncImpl(IDictionary<string, object> parameters)
        {
            switch (Tokens.Platform)
            {
                case Tokens.PlatformEnum.Twitter:
                    var response = await Tokens.TwitterTokens.Lists.Members.ListAsync(parameters);
                    var list =
                        new CursoredList<Twitter.Objects.User>(response.Select(x => new Twitter.Objects.User(x)));
                    list.NextCursor = response.NextCursor;
                    list.PreviousCursor = response.PreviousCursor;
                    return list;
                case Tokens.PlatformEnum.Mastodon:
                    throw new NotImplementedException();
            }
            throw new NotImplementedException();
        }
    }

    public class Friendships : ApiBase
    {
        public Task<Twitter.Objects.User> CreateAsync(params Expression<Func<string, object>>[] parameters)
        {
            return this.CreateAsyncImpl(ExpressionToDictionary(parameters));
        }

        public Task<Twitter.Objects.User> CreateAsync(IDictionary<string, object> parameters)
        {
            return this.CreateAsyncImpl(parameters);
        }

        private async Task<Twitter.Objects.User> CreateAsyncImpl(IDictionary<string, object> parameters)
        {
            switch (Tokens.Platform)
            {
                case Tokens.PlatformEnum.Twitter:
                    return new Twitter.Objects.User(await Tokens.TwitterTokens.Friendships.CreateAsync(parameters));
                case Tokens.PlatformEnum.Mastodon:
                    if (parameters.ContainsKey("screen_name"))
                        return new Twitter.Objects.User(
                            await Tokens.MastodonTokens.Follow((string) parameters["screen_name"]));
                    else
                        return new Twitter.Objects.User(
                            await Tokens.MastodonTokens.Follow((long) parameters["user_id"]));
            }
            throw new NotImplementedException();
        }

        public Task<Twitter.Objects.User> DestroyAsync(params Expression<Func<string, object>>[] parameters)
        {
            return this.DestroyAsyncImpl(ExpressionToDictionary(parameters));
        }

        public Task<Twitter.Objects.User> DestroyAsync(IDictionary<string, object> parameters)
        {
            return this.DestroyAsyncImpl(parameters);
        }

        private async Task<Twitter.Objects.User> DestroyAsyncImpl(IDictionary<string, object> parameters)
        {
            switch (Tokens.Platform)
            {
                case Tokens.PlatformEnum.Twitter:
                    return new Twitter.Objects.User(await Tokens.TwitterTokens.Friendships.DestroyAsync(parameters));
                case Tokens.PlatformEnum.Mastodon:
                    return new Twitter.Objects.User(await Tokens.MastodonTokens.Unfollow((long) parameters["user_id"]));
            }
            throw new NotImplementedException();
        }

        public Task<List<long>> NoRetweetsIdsAsync(params Expression<Func<string, object>>[] parameters)
        {
            return this.NoRetweetsIdsAsyncImpl(ExpressionToDictionary(parameters));
        }

        public Task<List<long>> NoRetweetsIdsAsync(IDictionary<string, object> parameters)
        {
            return this.NoRetweetsIdsAsyncImpl(parameters);
        }

        private async Task<List<long>> NoRetweetsIdsAsyncImpl(IDictionary<string, object> parameters)
        {
            switch (Tokens.Platform)
            {
                case Tokens.PlatformEnum.Twitter:
                    return (await Tokens.TwitterTokens.Friendships.NoRetweetsIdsAsync(parameters)).ToList();
                case Tokens.PlatformEnum.Mastodon:
                    throw new NotImplementedException();
            }
            throw new NotImplementedException();
        }

        public Task<Twitter.Objects.Relationship> ShowAsync(params Expression<Func<string, object>>[] parameters)
        {
            return this.ShowAsyncImpl(ExpressionToDictionary(parameters));
        }

        public Task<Twitter.Objects.Relationship> ShowAsync(IDictionary<string, object> parameters)
        {
            return this.ShowAsyncImpl(parameters);
        }

        private async Task<Twitter.Objects.Relationship> ShowAsyncImpl(IDictionary<string, object> parameters)
        {
            switch (Tokens.Platform)
            {
                case Tokens.PlatformEnum.Twitter:
                    return new Twitter.Objects.Relationship(
                        await Tokens.TwitterTokens.Friendships.ShowAsync(parameters));
                case Tokens.PlatformEnum.Mastodon:
                    return new Twitter.Objects.Relationship(
                        (await Tokens.MastodonTokens.GetAccountRelationships((long) parameters["target_id"])).First());
            }
            throw new NotImplementedException();
        }
    }

    public class Followers : ApiBase
    {
        public Task<CursoredList<long>> IdsAsync(params Expression<Func<string, object>>[] parameters)
        {
            return this.IdsAsyncImpl(ExpressionToDictionary(parameters));
        }

        public Task<CursoredList<long>> IdsAsync(IDictionary<string, object> parameters)
        {
            return this.IdsAsyncImpl(parameters);
        }

        private async Task<CursoredList<long>> IdsAsyncImpl(IDictionary<string, object> parameters)
        {
            switch (Tokens.Platform)
            {
                case Tokens.PlatformEnum.Twitter:
                    var response = await Tokens.TwitterTokens.Followers.IdsAsync(parameters);
                    var list = new CursoredList<long>(response);
                    list.NextCursor = response.NextCursor;
                    list.PreviousCursor = response.PreviousCursor;
                    return list;
                case Tokens.PlatformEnum.Mastodon:
                    return new CursoredList<long>(
                        (await Tokens.MastodonTokens.GetAccountFollowers((long) parameters["user_id"]))
                        .Select(x => (long) x.Id));
            }
            throw new NotImplementedException();
        }

        public Task<CursoredList<Twitter.Objects.User>> ListAsync(params Expression<Func<string, object>>[] parameters)
        {
            return this.ListAsyncImpl(ExpressionToDictionary(parameters));
        }

        public Task<CursoredList<Twitter.Objects.User>> ListAsync(IDictionary<string, object> parameters)
        {
            return this.ListAsyncImpl(parameters);
        }

        private async Task<CursoredList<Twitter.Objects.User>> ListAsyncImpl(IDictionary<string, object> parameters)
        {
            switch (Tokens.Platform)
            {
                case Tokens.PlatformEnum.Twitter:
                    var response = await Tokens.TwitterTokens.Followers.ListAsync(parameters);
                    var list =
                        new CursoredList<Twitter.Objects.User>(response.Select(x => new Twitter.Objects.User(x)));
                    list.NextCursor = response.NextCursor;
                    list.PreviousCursor = response.PreviousCursor;
                    return list;
                case Tokens.PlatformEnum.Mastodon:
                    return new CursoredList<Twitter.Objects.User>(
                        (await Tokens.MastodonTokens.GetAccountFollowers((long) parameters["user_id"])).Select(
                            x => new Twitter.Objects.User(x)));
            }
            throw new NotImplementedException();
        }
    }

    public class Friends : ApiBase
    {
        public Task<CursoredList<long>> IdsAsync(params Expression<Func<string, object>>[] parameters)
        {
            return this.IdsAsyncImpl(ExpressionToDictionary(parameters));
        }

        public Task<CursoredList<long>> IdsAsync(IDictionary<string, object> parameters)
        {
            return this.IdsAsyncImpl(parameters);
        }

        private async Task<CursoredList<long>> IdsAsyncImpl(IDictionary<string, object> parameters)
        {
            switch (Tokens.Platform)
            {
                case Tokens.PlatformEnum.Twitter:
                    var response = await Tokens.TwitterTokens.Friends.IdsAsync(parameters);
                    var list = new CursoredList<long>(response);
                    list.NextCursor = response.NextCursor;
                    list.PreviousCursor = response.PreviousCursor;
                    return list;
                case Tokens.PlatformEnum.Mastodon:
                    return new CursoredList<long>(
                        (await Tokens.MastodonTokens.GetAccountFollowers((long) parameters["user_id"]))
                        .Select(x => (long) x.Id));
            }
            throw new NotImplementedException();
        }

        public Task<CursoredList<Twitter.Objects.User>> ListAsync(params Expression<Func<string, object>>[] parameters)
        {
            return this.ListAsyncImpl(ExpressionToDictionary(parameters));
        }

        public Task<CursoredList<Twitter.Objects.User>> ListAsync(IDictionary<string, object> parameters)
        {
            return this.ListAsyncImpl(parameters);
        }

        private async Task<CursoredList<Twitter.Objects.User>> ListAsyncImpl(IDictionary<string, object> parameters)
        {
            switch (Tokens.Platform)
            {
                case Tokens.PlatformEnum.Twitter:
                    var response = await Tokens.TwitterTokens.Friends.ListAsync(parameters);
                    var list =
                        new CursoredList<Twitter.Objects.User>(response.Select(x => new Twitter.Objects.User(x)));
                    list.NextCursor = response.NextCursor;
                    list.PreviousCursor = response.PreviousCursor;
                    return list;
                case Tokens.PlatformEnum.Mastodon:
                    return new CursoredList<Twitter.Objects.User>(
                        (await Tokens.MastodonTokens.GetAccountFollowers((long) parameters["user_id"])).Select(
                            x => new Twitter.Objects.User(x)));
            }
            throw new NotImplementedException();
        }
    }

    public class DirectMessages : ApiBase
    {
        public Task<Twitter.Objects.DirectMessage> DestroyAsync(params Expression<Func<string, object>>[] parameters)
        {
            return this.DestroyAsyncImpl(ExpressionToDictionary(parameters));
        }

        public Task<Twitter.Objects.DirectMessage> DestroyAsync(IDictionary<string, object> parameters)
        {
            return this.DestroyAsyncImpl(parameters);
        }

        private async Task<Twitter.Objects.DirectMessage> DestroyAsyncImpl(IDictionary<string, object> parameters)
        {
            switch (Tokens.Platform)
            {
                case Tokens.PlatformEnum.Twitter:
                    return new Twitter.Objects.DirectMessage(
                        await Tokens.TwitterTokens.DirectMessages.DestroyAsync(parameters));
                case Tokens.PlatformEnum.Mastodon:
                    await Tokens.MastodonTokens.DeleteStatus((long) parameters["id"]);
                    return null;
            }
            throw new NotImplementedException();
        }

        public Task<Twitter.Objects.DirectMessage> NewAsync(params Expression<Func<string, object>>[] parameters)
        {
            return this.NewAsyncImpl(ExpressionToDictionary(parameters));
        }

        public Task<Twitter.Objects.DirectMessage> NewAsync(IDictionary<string, object> parameters)
        {
            return this.NewAsyncImpl(parameters);
        }

        private async Task<Twitter.Objects.DirectMessage> NewAsyncImpl(IDictionary<string, object> parameters)
        {
            switch (Tokens.Platform)
            {
                case Tokens.PlatformEnum.Twitter:
                    return new Twitter.Objects.DirectMessage(
                        await Tokens.TwitterTokens.DirectMessages.NewAsync(parameters));
                case Tokens.PlatformEnum.Mastodon:
                    if (!parameters.ContainsKey("in_reply_to_status_id"))
                        throw new NotImplementedException();
                    var status = await Tokens.MastodonTokens.PostStatus((string) parameters["text"],
                        Mastonet.Visibility.Direct, (long) parameters["in_reply_to_status_id"],
                        (IEnumerable<long>) parameters.GetValueOrDefault("media_ids", null),
                        (bool) parameters.GetValueOrDefault("possibly_sensitive", false));
                    return new Twitter.Objects.DirectMessage(status,
                        (await Tokens.MastodonTokens.GetStatus(status.InReplyToId.Value)).Account);
            }
            throw new NotImplementedException();
        }

        public Task<List<Twitter.Objects.DirectMessage>> ReceivedAsync(
            params Expression<Func<string, object>>[] parameters)
        {
            return this.ReceivedAsyncImpl(ExpressionToDictionary(parameters));
        }

        public Task<List<Twitter.Objects.DirectMessage>> ReceivedAsync(IDictionary<string, object> parameters)
        {
            return this.ReceivedAsyncImpl(parameters);
        }

        private async Task<List<Twitter.Objects.DirectMessage>> ReceivedAsyncImpl(
            IDictionary<string, object> parameters)
        {
            switch (Tokens.Platform)
            {
                case Tokens.PlatformEnum.Twitter:
                    return (await Tokens.TwitterTokens.DirectMessages.ReceivedAsync(parameters))
                        .Select(x => new Twitter.Objects.DirectMessage(x))
                        .ToList();
                case Tokens.PlatformEnum.Mastodon:
                    var statuses = (await Tokens.MastodonTokens.GetNotifications())
                        .Where(x => x.Type == "mention" && x.Status.Visibility == "direct")
                        .Select(x => x.Status);
                    var tasks = statuses.Select(async x => new Twitter.Objects.DirectMessage(x,
                        (await Tokens.MastodonTokens.GetStatus(x.InReplyToId.Value)).Account));
                    return (await Task.WhenAll(tasks)).ToList();
            }
            throw new NotImplementedException();
        }

        public Task<List<Twitter.Objects.DirectMessage>> SentAsync(params Expression<Func<string, object>>[] parameters)
        {
            return this.SentAsyncImpl(ExpressionToDictionary(parameters));
        }

        public Task<List<Twitter.Objects.DirectMessage>> SentAsync(IDictionary<string, object> parameters)
        {
            return this.SentAsyncImpl(parameters);
        }

        private async Task<List<Twitter.Objects.DirectMessage>> SentAsyncImpl(IDictionary<string, object> parameters)
        {
            switch (Tokens.Platform)
            {
                case Tokens.PlatformEnum.Twitter:
                    return (await Tokens.TwitterTokens.DirectMessages.SentAsync(parameters))
                        .Select(x => new Twitter.Objects.DirectMessage(x))
                        .ToList();
                case Tokens.PlatformEnum.Mastodon:
                    throw new NotImplementedException();
            }
            throw new NotImplementedException();
        }
    }

    public class Collections : ApiBase
    {
        public Task<Twitter.Objects.Collection> CreateAsync(params Expression<Func<string, object>>[] parameters)
        {
            return this.CreateAsyncImpl(ExpressionToDictionary(parameters));
        }

        public Task<Twitter.Objects.Collection> CreateAsync(IDictionary<string, object> parameters)
        {
            return this.CreateAsyncImpl(parameters);
        }

        private async Task<Twitter.Objects.Collection> CreateAsyncImpl(IDictionary<string, object> parameters)
        {
            switch (Tokens.Platform)
            {
                case Tokens.PlatformEnum.Twitter:
                    return new Twitter.Objects.Collection(
                        await Tokens.TwitterTokens.Collections.CreateAsync(parameters));
                case Tokens.PlatformEnum.Mastodon:
                    throw new NotImplementedException();
            }
            throw new NotImplementedException();
        }

        public Task<bool> DestroyAsync(params Expression<Func<string, object>>[] parameters)
        {
            return this.DestroyAsyncImpl(ExpressionToDictionary(parameters));
        }

        public Task<bool> DestroyAsync(IDictionary<string, object> parameters)
        {
            return this.DestroyAsyncImpl(parameters);
        }

        private async Task<bool> DestroyAsyncImpl(IDictionary<string, object> parameters)
        {
            switch (Tokens.Platform)
            {
                case Tokens.PlatformEnum.Twitter:
                    return (await Tokens.TwitterTokens.Collections.DestroyAsync(parameters)).Destroyed;
                case Tokens.PlatformEnum.Mastodon:
                    throw new NotImplementedException();
            }
            throw new NotImplementedException();
        }

        public Task<bool> EntriesAddAsync(params Expression<Func<string, object>>[] parameters)
        {
            return this.EntriesAddAsyncImpl(ExpressionToDictionary(parameters));
        }

        public Task<bool> EntriesAddAsync(IDictionary<string, object> parameters)
        {
            return this.EntriesAddAsyncImpl(parameters);
        }

        private async Task<bool> EntriesAddAsyncImpl(IDictionary<string, object> parameters)
        {
            switch (Tokens.Platform)
            {
                case Tokens.PlatformEnum.Twitter:
                    return (await Tokens.TwitterTokens.Collections.EntriesAddAsync(parameters)).Count == 0;
                case Tokens.PlatformEnum.Mastodon:
                    throw new NotImplementedException();
            }
            throw new NotImplementedException();
        }

        public Task<List<Twitter.Objects.CollectionEntry>> EntriesAsync(
            params Expression<Func<string, object>>[] parameters)
        {
            return this.EntriesAsyncImpl(ExpressionToDictionary(parameters));
        }

        public Task<List<Twitter.Objects.CollectionEntry>> EntriesAsync(IDictionary<string, object> parameters)
        {
            return this.EntriesAsyncImpl(parameters);
        }

        private async Task<List<Twitter.Objects.CollectionEntry>> EntriesAsyncImpl(
            IDictionary<string, object> parameters)
        {
            switch (Tokens.Platform)
            {
                case Tokens.PlatformEnum.Twitter:
                    return (await Tokens.TwitterTokens.Collections.EntriesAsync(parameters)).Entries
                        .Select(x => new Twitter.Objects.CollectionEntry(x))
                        .ToList();
                case Tokens.PlatformEnum.Mastodon:
                    throw new NotImplementedException();
            }
            throw new NotImplementedException();
        }

        public Task<bool> EntriesRemoveAsync(params Expression<Func<string, object>>[] parameters)
        {
            return this.EntriesRemoveAsyncImpl(ExpressionToDictionary(parameters));
        }

        public Task<bool> EntriesRemoveAsync(IDictionary<string, object> parameters)
        {
            return this.EntriesRemoveAsyncImpl(parameters);
        }

        private async Task<bool> EntriesRemoveAsyncImpl(IDictionary<string, object> parameters)
        {
            switch (Tokens.Platform)
            {
                case Tokens.PlatformEnum.Twitter:
                    return (await Tokens.TwitterTokens.Collections.EntriesRemoveAsync(parameters)).Count == 0;
                case Tokens.PlatformEnum.Mastodon:
                    throw new NotImplementedException();
            }
            throw new NotImplementedException();
        }

        public Task<StringCursoredList<Twitter.Objects.Collection>> ListAsync(
            params Expression<Func<string, object>>[] parameters)
        {
            return this.ListAsyncImpl(ExpressionToDictionary(parameters));
        }

        public Task<StringCursoredList<Twitter.Objects.Collection>> ListAsync(IDictionary<string, object> parameters)
        {
            return this.ListAsyncImpl(parameters);
        }

        private async Task<StringCursoredList<Twitter.Objects.Collection>> ListAsyncImpl(
            IDictionary<string, object> parameters)
        {
            switch (Tokens.Platform)
            {
                case Tokens.PlatformEnum.Twitter:
                    var response = await Tokens.TwitterTokens.Collections.ListAsync(parameters);
                    var list =
                        new StringCursoredList<Twitter.Objects.Collection>(
                            response.Results.Select(x => new Twitter.Objects.Collection(x)));
                    list.NextCursor = response.Cursors.NextCursor;
                    list.PreviousCursor = response.Cursors.PreviousCursor;
                    return list;
                case Tokens.PlatformEnum.Mastodon:
                    throw new NotImplementedException();
            }
            throw new NotImplementedException();
        }

        public Task<Twitter.Objects.Collection> ShowAsync(params Expression<Func<string, object>>[] parameters)
        {
            return this.ShowAsyncImpl(ExpressionToDictionary(parameters));
        }

        public Task<Twitter.Objects.Collection> ShowAsync(IDictionary<string, object> parameters)
        {
            return this.ShowAsyncImpl(parameters);
        }

        private async Task<Twitter.Objects.Collection> ShowAsyncImpl(IDictionary<string, object> parameters)
        {
            switch (Tokens.Platform)
            {
                case Tokens.PlatformEnum.Twitter:
                    return new Twitter.Objects.Collection(await Tokens.TwitterTokens.Collections.ShowAsync(parameters));
                case Tokens.PlatformEnum.Mastodon:
                    throw new NotImplementedException();
            }
            throw new NotImplementedException();
        }

        public Task<Twitter.Objects.Collection> UpdateAsync(params Expression<Func<string, object>>[] parameters)
        {
            return this.UpdateAsyncImpl(ExpressionToDictionary(parameters));
        }

        public Task<Twitter.Objects.Collection> UpdateAsync(IDictionary<string, object> parameters)
        {
            return this.UpdateAsyncImpl(parameters);
        }

        private async Task<Twitter.Objects.Collection> UpdateAsyncImpl(IDictionary<string, object> parameters)
        {
            switch (Tokens.Platform)
            {
                case Tokens.PlatformEnum.Twitter:
                    return new Twitter.Objects.Collection(
                        await Tokens.TwitterTokens.Collections.UpdateAsync(parameters));
                case Tokens.PlatformEnum.Mastodon:
                    throw new NotImplementedException();
            }
            throw new NotImplementedException();
        }
    }

    public class Blocks : ApiBase
    {
        public Task<Twitter.Objects.User> CreateAsync(params Expression<Func<string, object>>[] parameters)
        {
            return this.CreateAsyncImpl(ExpressionToDictionary(parameters));
        }

        public Task<Twitter.Objects.User> CreateAsync(IDictionary<string, object> parameters)
        {
            return this.CreateAsyncImpl(parameters);
        }

        private async Task<Twitter.Objects.User> CreateAsyncImpl(IDictionary<string, object> parameters)
        {
            switch (Tokens.Platform)
            {
                case Tokens.PlatformEnum.Twitter:
                    return new Twitter.Objects.User(await Tokens.TwitterTokens.Blocks.CreateAsync(parameters));
                case Tokens.PlatformEnum.Mastodon:
                    return new Twitter.Objects.User(await Tokens.MastodonTokens.Block((long) parameters["user_id"]));
            }
            throw new NotImplementedException();
        }

        public Task<Twitter.Objects.User> DestroyAsync(params Expression<Func<string, object>>[] parameters)
        {
            return this.DestroyAsyncImpl(ExpressionToDictionary(parameters));
        }

        public Task<Twitter.Objects.User> DestroyAsync(IDictionary<string, object> parameters)
        {
            return this.DestroyAsyncImpl(parameters);
        }

        private async Task<Twitter.Objects.User> DestroyAsyncImpl(IDictionary<string, object> parameters)
        {
            switch (Tokens.Platform)
            {
                case Tokens.PlatformEnum.Twitter:
                    return new Twitter.Objects.User(await Tokens.TwitterTokens.Blocks.DestroyAsync(parameters));
                case Tokens.PlatformEnum.Mastodon:
                    return new Twitter.Objects.User(await Tokens.MastodonTokens.Unblock((long) parameters["user_id"]));
            }
            throw new NotImplementedException();
        }

        public Task<CursoredList<long>> IdsAsync(params Expression<Func<string, object>>[] parameters)
        {
            return this.IdsAsyncImpl(ExpressionToDictionary(parameters));
        }

        public Task<CursoredList<long>> IdsAsync(IDictionary<string, object> parameters)
        {
            return this.IdsAsyncImpl(parameters);
        }

        private async Task<CursoredList<long>> IdsAsyncImpl(IDictionary<string, object> parameters)
        {
            switch (Tokens.Platform)
            {
                case Tokens.PlatformEnum.Twitter:
                    var response = await Tokens.TwitterTokens.Blocks.IdsAsync(parameters);
                    var list = new CursoredList<long>(response);
                    list.NextCursor = response.NextCursor;
                    list.PreviousCursor = response.PreviousCursor;
                    return list;
                case Tokens.PlatformEnum.Mastodon:
                    return new CursoredList<long>((await Tokens.MastodonTokens.GetBlocks()).Select(x => (long) x.Id));
            }
            throw new NotImplementedException();
        }

        public Task<CursoredList<Twitter.Objects.User>> ListAsync(params Expression<Func<string, object>>[] parameters)
        {
            return this.ListAsyncImpl(ExpressionToDictionary(parameters));
        }

        public Task<CursoredList<Twitter.Objects.User>> ListAsync(IDictionary<string, object> parameters)
        {
            return this.ListAsyncImpl(parameters);
        }

        private async Task<CursoredList<Twitter.Objects.User>> ListAsyncImpl(IDictionary<string, object> parameters)
        {
            switch (Tokens.Platform)
            {
                case Tokens.PlatformEnum.Twitter:
                    var response = await Tokens.TwitterTokens.Blocks.ListAsync(parameters);
                    var list =
                        new CursoredList<Twitter.Objects.User>(response.Select(x => new Twitter.Objects.User(x)));
                    list.NextCursor = response.NextCursor;
                    list.PreviousCursor = response.PreviousCursor;
                    return list;
                case Tokens.PlatformEnum.Mastodon:
                    return new CursoredList<Twitter.Objects.User>(
                        (await Tokens.MastodonTokens.GetBlocks()).Select(x => new Twitter.Objects.User(x)));
            }
            throw new NotImplementedException();
        }
    }

    public class Media : ApiBase
    {
        public enum UploadMediaTypeEnum
        {
            Image = 0,
            Video = 1
        }

        public Task<long> UploadAsync(params Expression<Func<string, object>>[] parameters)
        {
            return this.UploadAsyncImpl(ExpressionToDictionary(parameters));
        }

        public Task<long> UploadAsync(IDictionary<string, object> parameters)
        {
            return this.UploadAsyncImpl(parameters);
        }

        private async Task<long> UploadAsyncImpl(IDictionary<string, object> parameters)
        {
            switch (Tokens.Platform)
            {
                case Tokens.PlatformEnum.Twitter:
                    return (await Tokens.TwitterTokens.Media.UploadAsync(parameters)).MediaId;
                case Tokens.PlatformEnum.Mastodon:
                    throw new NotImplementedException();
            }
            throw new NotImplementedException();
        }

        public async Task<long> UploadAsync(Stream media, IEnumerable<long> additional_owners = null,
            IProgress<CoreTweet.UploadProgressInfo> progress = null)
        {
            switch (Tokens.Platform)
            {
                case Tokens.PlatformEnum.Twitter:
                    return (await Tokens.TwitterTokens.Media.UploadAsync(media, additional_owners, progress: progress))
                        .MediaId;
                case Tokens.PlatformEnum.Mastodon:
                    throw new NotImplementedException();
            }
            throw new NotImplementedException();
        }

        public Task<long> UploadChunkedAsync(params Expression<Func<string, object>>[] parameters)
        {
            return this.UploadChunkedAsyncImpl(ExpressionToDictionary(parameters));
        }

        public Task<long> UploadChunkedAsync(IDictionary<string, object> parameters)
        {
            return this.UploadChunkedAsyncImpl(parameters);
        }

        private async Task<long> UploadChunkedAsyncImpl(IDictionary<string, object> parameters)
        {
            switch (Tokens.Platform)
            {
                case Tokens.PlatformEnum.Twitter:
                    throw new NotImplementedException();
                case Tokens.PlatformEnum.Mastodon:
                    throw new NotImplementedException();
            }
            throw new NotImplementedException();
        }

        public async Task<long> UploadChunkedAsync(Stream media, UploadMediaTypeEnum mediaType,
            string media_category = null, IEnumerable<long> additional_owners = null,
            IProgress<CoreTweet.UploadChunkedProgressInfo> progress = null)
        {
            switch (Tokens.Platform)
            {
                case Tokens.PlatformEnum.Twitter:
                    return (await Tokens.TwitterTokens.Media.UploadChunkedAsync(media,
                        (CoreTweet.UploadMediaType) Enum.ToObject(typeof(CoreTweet.UploadMediaType), mediaType),
                        media_category, additional_owners, progress: progress)).MediaId;
                case Tokens.PlatformEnum.Mastodon:
                    throw new NotImplementedException();
            }
            throw new NotImplementedException();
        }
    }

    public class Activity : ApiBase
    {
        public Task<List<Twitter.Objects.EventMessage>> AboutMeAsync(
            params Expression<Func<string, object>>[] parameters)
        {
            return this.AboutMeAsyncImpl(ExpressionToDictionary(parameters));
        }

        public Task<List<Twitter.Objects.EventMessage>> AboutMeAsync(IDictionary<string, object> parameters)
        {
            return this.AboutMeAsyncImpl(parameters);
        }

        private async Task<List<Twitter.Objects.EventMessage>> AboutMeAsyncImpl(IDictionary<string, object> parameters)
        {
            switch (Tokens.Platform)
            {
                case Tokens.PlatformEnum.Twitter:
                    throw new NotImplementedException();

                case Tokens.PlatformEnum.Mastodon:
                    return (await Tokens.MastodonTokens.GetNotifications())
                        .Select(x => new Twitter.Objects.EventMessage(x))
                        .ToList();
            }
            throw new NotImplementedException();
        }
    }

    public class StreamingApi : ApiBase
    {
        public static class MastodonStreaming
        {
            public enum StreamingType
            {
                User = 0,
                Tag = 1,
                Public = 2
            }

            public class StreamingObservable : IObservable<Twitter.Objects.StreamingMessage>
            {
                public StreamingObservable(Tokens tokens, StreamingType type,
                    IDictionary<string, object> parameters = null)
                {
                    this.tokens = tokens;
                    this.type = type;
                    this.parameters = parameters;
                }

                private readonly Tokens tokens;
                private readonly StreamingType type;
                private readonly IDictionary<string, object> parameters;

                public IDisposable Subscribe(IObserver<Twitter.Objects.StreamingMessage> observer)
                {
                    var streamingUrl = tokens.Instance == "mstdn.jp" ? "streaming.mstdn.jp" : tokens.Instance;
                    var conn = new StreamingConnection();
                    switch (type)
                    {
                        case StreamingType.User:
                            conn.Start(observer, tokens, "https://" + streamingUrl + "/api/v1/streaming/user");
                            break;
                        case StreamingType.Tag:
                            conn.Start(observer, tokens,
                                "https://" + streamingUrl + "/api/v1/streaming/hashtag" + "?tag=" +
                                parameters["track"]);
                            break;
                        case StreamingType.Public:
                            conn.Start(observer, tokens, "https://" + streamingUrl + "/api/v1/streaming/public");
                            break;
                    }
                    return conn;
                }
            }

            internal class StreamingConnection : IDisposable
            {
                private readonly CancellationTokenSource cancel = new CancellationTokenSource();

                public async void Start(IObserver<Twitter.Objects.StreamingMessage> observer, Tokens tokens, string url)
                {
                    var token = this.cancel.Token;
                    try
                    {
                        using (var client = new HttpClient())
                        using (token.Register(client.Dispose))
                        {
                            client.DefaultRequestHeaders.Add("Authorization", "Bearer " + tokens.AccessToken);
                            using (var stream = await client.GetStreamAsync(url))
                            using (token.Register(stream.Dispose))
                            using (var reader = new StreamReader(stream))
                            using (token.Register(reader.Dispose))
                            {
                                string eventName = null;
                                while (!reader.EndOfStream)
                                {
                                    var line = reader.ReadLine();
                                    if (string.IsNullOrEmpty(line) || line.StartsWith(":"))
                                    {
                                        eventName = null;
                                        continue;
                                    }

                                    if (line.StartsWith("event: "))
                                    {
                                        eventName = line.Substring("event: ".Length).Trim();
                                    }
                                    if (line.StartsWith("data: "))
                                    {
                                        var data = line.Substring("data: ".Length);
                                        switch (eventName)
                                        {
                                            case "update":
                                                var status =
                                                    JsonConvert.DeserializeObject<Mastonet.Entities.Status>(data);
                                                if (status.Visibility == "direct")
                                                {
                                                    var dm =
                                                        new Twitter.Objects.DirectMessage(status,
                                                            (await tokens.MastodonTokens.GetStatus(status.InReplyToId
                                                                .Value)).Account);
                                                    observer.OnNext(new Twitter.Objects.StreamingMessage(dm));
                                                }
                                                else
                                                {
                                                    var s = new Twitter.Objects.Status(status);
                                                    observer.OnNext(new Twitter.Objects.StreamingMessage(s));
                                                }
                                                break;
                                            case "notification":
                                                var notification =
                                                    JsonConvert.DeserializeObject<Mastonet.Entities.Notification>(data);
                                                observer.OnNext(new Twitter.Objects.StreamingMessage(
                                                    new Twitter.Objects.EventMessage(notification)));
                                                break;
                                            case "delete":
                                                var statusId = int.Parse(data);
                                                observer.OnNext(new Twitter.Objects.StreamingMessage(statusId));
                                                break;
                                        }
                                    }
                                }
                                observer.OnCompleted();
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        if (!token.IsCancellationRequested)
                        {
                            observer.OnError(e);
                        }
                    }
                }

                public void Dispose()
                {
                    this.cancel.Cancel();
                }
            }
        }

        public IObservable<Twitter.Objects.StreamingMessage> FilterAsObservable(
            params Expression<Func<string, object>>[] parameters)
        {
            return this.FilterAsObservableImpl(ExpressionToDictionary(parameters));
        }

        public IObservable<Twitter.Objects.StreamingMessage> FilterAsObservable(IDictionary<string, object> parameters)
        {
            return this.FilterAsObservableImpl(parameters);
        }

        private IObservable<Twitter.Objects.StreamingMessage> FilterAsObservableImpl(
            IDictionary<string, object> parameters)
        {
            switch (Tokens.Platform)
            {
                case Tokens.PlatformEnum.Twitter:
                    return Tokens.TwitterTokens.Streaming.FilterAsObservable(parameters)
                        .Select(x => new Twitter.Objects.StreamingMessage(x));
                case Tokens.PlatformEnum.Mastodon:
                    return new MastodonStreaming.StreamingObservable(Tokens, MastodonStreaming.StreamingType.Tag,
                        parameters);
            }
            throw new NotImplementedException();
        }

        public IObservable<Twitter.Objects.StreamingMessage> SampleAsObservable(
            params Expression<Func<string, object>>[] parameters)
        {
            return this.SampleAsObservableImpl(ExpressionToDictionary(parameters));
        }

        public IObservable<Twitter.Objects.StreamingMessage> SampleAsObservable(IDictionary<string, object> parameters)
        {
            return this.SampleAsObservableImpl(parameters);
        }

        private IObservable<Twitter.Objects.StreamingMessage> SampleAsObservableImpl(
            IDictionary<string, object> parameters)
        {
            switch (Tokens.Platform)
            {
                case Tokens.PlatformEnum.Twitter:
                    return Tokens.TwitterTokens.Streaming.SampleAsObservable(parameters)
                        .Select(x => new Twitter.Objects.StreamingMessage(x));
                case Tokens.PlatformEnum.Mastodon:
                    return new MastodonStreaming.StreamingObservable(Tokens, MastodonStreaming.StreamingType.Public,
                        parameters);
            }
            throw new NotImplementedException();
        }

        public IObservable<Twitter.Objects.StreamingMessage> UserAsObservable(
            params Expression<Func<string, object>>[] parameters)
        {
            return this.UserAsObservableImpl(ExpressionToDictionary(parameters));
        }

        public IObservable<Twitter.Objects.StreamingMessage> UserAsObservable(IDictionary<string, object> parameters)
        {
            return this.UserAsObservableImpl(parameters);
        }

        private IObservable<Twitter.Objects.StreamingMessage> UserAsObservableImpl(
            IDictionary<string, object> parameters)
        {
            switch (Tokens.Platform)
            {
                case Tokens.PlatformEnum.Twitter:
                    return Tokens.TwitterTokens.Streaming.UserAsObservable(parameters)
                        .Select(x => new Twitter.Objects.StreamingMessage(x));
                case Tokens.PlatformEnum.Mastodon:
                    return new MastodonStreaming.StreamingObservable(Tokens, MastodonStreaming.StreamingType.User,
                        parameters);
            }
            throw new NotImplementedException();
        }
    }

    public class Tokens
    {
        public Tokens()
        {
            this.Activity = new Activity() {Tokens = this};
            this.Favorites = new Favorites() {Tokens = this};
            this.Users = new Users() {Tokens = this};
            this.Trends = new Trends() {Tokens = this};
            this.Statuses = new Statuses() {Tokens = this};
            this.SavedSearches = new SavedSearches() {Tokens = this};
            this.Search = new Search() {Tokens = this};
            this.Mutes = new Mutes() {Tokens = this};
            this.Mutes.Users = new MutesUsers() {Tokens = this};
            this.Lists = new Lists() {Tokens = this};
            this.Lists.Members = new ListsMembers() {Tokens = this};
            this.Lists.Members = new ListsMembers() {Tokens = this};
            this.Friendships = new Friendships() {Tokens = this};
            this.Followers = new Followers() {Tokens = this};
            this.Friends = new Friends() {Tokens = this};
            this.DirectMessages = new DirectMessages() {Tokens = this};
            this.Collections = new Collections() {Tokens = this};
            this.Blocks = new Blocks() {Tokens = this};
            this.Media = new Media() {Tokens = this};
            this.Streaming = new StreamingApi() {Tokens = this};
        }

        public enum PlatformEnum
        {
            Twitter = 0,
            Mastodon = 1,
        }

        public string ConsumerKey { get; set; }
        public string ConsumerSecret { get; set; }
        public string AccessToken { get; set; }
        public string AccessTokenSecret { get; set; }
        public long UserId { get; set; }
        public string ScreenName { get; set; }
        public PlatformEnum Platform { get; set; }
        public string Instance { get; set; }

        private CoreTweet.Tokens _twitterToken;

        public CoreTweet.Tokens TwitterTokens => _twitterToken ??
                                                 (_twitterToken =
                                                     CoreTweet.Tokens.Create(this.ConsumerKey, this.ConsumerSecret,
                                                         this.AccessToken, this.AccessTokenSecret));

        private Mastonet.MastodonClient _mastodonTokens;

        public Mastonet.MastodonClient MastodonTokens => _mastodonTokens ??
                                                         (_mastodonTokens =
                                                             new Mastonet.MastodonClient(
                                                                 new Mastonet.Entities.AppRegistration()
                                                                 {
                                                                     ClientId = this.ConsumerKey,
                                                                     ClientSecret = this.ConsumerSecret,
                                                                     Instance = this.Instance
                                                                 }, this.AccessToken));

        public Activity Activity { get; }
        public Favorites Favorites { get; }
        public Users Users { get; }
        public Trends Trends { get; }
        public Statuses Statuses { get; }
        public SavedSearches SavedSearches { get; }
        public Search Search { get; }
        public Mutes Mutes { get; }
        public Lists Lists { get; }
        public Friendships Friendships { get; }
        public Followers Followers { get; }
        public Friends Friends { get; }
        public DirectMessages DirectMessages { get; }
        public Collections Collections { get; }
        public Blocks Blocks { get; }
        public Media Media { get; }
        public StreamingApi Streaming { get; }

        public static Tokens Create(string consumerKey, string consumerSecret, string accessToken, string accessSecret,
            long userId, string screenName, string instance = "")
        {
            return new Tokens()
            {
                ConsumerKey = consumerKey,
                ConsumerSecret = consumerSecret,
                AccessToken = accessToken,
                AccessTokenSecret = accessSecret,
                UserId = userId,
                ScreenName = screenName,
                Platform = string.IsNullOrWhiteSpace(instance) ? PlatformEnum.Twitter : PlatformEnum.Mastodon,
                Instance = instance
            };
        }
    }
}