using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Flantter.MilkyWay.Models.Apis.Wrapper
{
    public static class Utils
    {
        public static TValue GetValueOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key,
            TValue defaultValue)
        {
            return dictionary.TryGetValue(key, out var value) ? value : defaultValue;
        }

        public static IDictionary<string, object> ConvertToMastodonParameters(IDictionary<string, object> parameters)
        {
            if (parameters.ContainsKey("count"))
            {
                parameters.Add("limit", parameters["count"]);
                parameters.Remove("count");
            }
            if (parameters.ContainsKey("user_id"))
            {
                parameters.Add("id", parameters["user_id"]);
                parameters.Remove("user_id");
            }
            if (parameters.ContainsKey("cursor"))
            {
                parameters.Add("max_id", parameters["cursor"]);
                parameters.Remove("cursor");
            }
            if (parameters.ContainsKey("in_reply_to_status_id"))
            {
                parameters.Add("in_reply_to_id", parameters["in_reply_to_status_id"]);
                parameters.Remove("in_reply_to_status_id");
            }
            if (parameters.ContainsKey("possibly_sensitive"))
            {
                parameters.Add("sensitive", parameters["possibly_sensitive"]);
                parameters.Remove("possibly_sensitive");
            }
            if (parameters.ContainsKey("name"))
            {
                parameters.Add("title", parameters["name"]);
                parameters.Remove("name");
            }
            if (parameters.ContainsKey("list_id"))
            {
                parameters.Add("id", parameters["list_id"]);
                parameters.Remove("list_id");
            }

            return parameters;
        }
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
            return expr.Body is ConstantExpression constExpr ? constExpr.Value : expr.Compile()("");
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
        public Task<Apis.Objects.Status> CreateAsync(params Expression<Func<string, object>>[] parameters)
        {
            return CreateAsyncImpl(ExpressionToDictionary(parameters));
        }

        public Task<Apis.Objects.Status> CreateAsync(IDictionary<string, object> parameters)
        {
            return CreateAsyncImpl(parameters);
        }

        private async Task<Apis.Objects.Status> CreateAsyncImpl(IDictionary<string, object> parameters)
        {
            switch (Tokens.Platform)
            {
                case Tokens.PlatformEnum.Twitter:
                    return new Apis.Objects.Status(await Tokens.TwitterTokens.Favorites.CreateAsync(parameters));
                case Tokens.PlatformEnum.Mastodon:
                    return new Apis.Objects.Status(
                        await Tokens.MastodonTokens.Statuses.FavouriteAsync(
                            Utils.ConvertToMastodonParameters(parameters)));
            }
            throw new NotImplementedException();
        }

        public Task<Apis.Objects.Status> DestroyAsync(params Expression<Func<string, object>>[] parameters)
        {
            return DestroyAsyncImpl(ExpressionToDictionary(parameters));
        }

        public Task<Apis.Objects.Status> DestroyAsync(IDictionary<string, object> parameters)
        {
            return DestroyAsyncImpl(parameters);
        }

        private async Task<Apis.Objects.Status> DestroyAsyncImpl(IDictionary<string, object> parameters)
        {
            switch (Tokens.Platform)
            {
                case Tokens.PlatformEnum.Twitter:
                    return new Apis.Objects.Status(await Tokens.TwitterTokens.Favorites.DestroyAsync(parameters));
                case Tokens.PlatformEnum.Mastodon:
                    return new Apis.Objects.Status(
                        await Tokens.MastodonTokens.Statuses.UnfavouriteAsync(
                            Utils.ConvertToMastodonParameters(parameters)));
            }
            throw new NotImplementedException();
        }

        public Task<List<Apis.Objects.Status>> ListAsync(params Expression<Func<string, object>>[] parameters)
        {
            return ListAsyncImpl(ExpressionToDictionary(parameters));
        }

        public Task<List<Apis.Objects.Status>> ListAsync(IDictionary<string, object> parameters)
        {
            return ListAsyncImpl(parameters);
        }

        private async Task<List<Apis.Objects.Status>> ListAsyncImpl(IDictionary<string, object> parameters)
        {
            switch (Tokens.Platform)
            {
                case Tokens.PlatformEnum.Twitter:
                    return (await Tokens.TwitterTokens.Favorites.ListAsync(parameters))
                        .Select(x => new Apis.Objects.Status(x))
                        .ToList();
                case Tokens.PlatformEnum.Mastodon:
                    if (parameters.ContainsKey("id") || parameters.ContainsKey("screen_name"))
                        throw new NotImplementedException();
                    var result =
                        (await Tokens.MastodonTokens.Favourites.GetAsync(Utils.ConvertToMastodonParameters(parameters)))
                        .Select(x => new Apis.Objects.Status(x))
                        .ToList();
                    return result;
            }
            throw new NotImplementedException();
        }
    }

    public class Users : ApiBase
    {
        public Task<List<Apis.Objects.User>> LookupAsync(params Expression<Func<string, object>>[] parameters)
        {
            return LookupAsyncImpl(ExpressionToDictionary(parameters));
        }

        public Task<List<Apis.Objects.User>> LookupAsync(IDictionary<string, object> parameters)
        {
            return LookupAsyncImpl(parameters);
        }

        private async Task<List<Apis.Objects.User>> LookupAsyncImpl(IDictionary<string, object> parameters)
        {
            switch (Tokens.Platform)
            {
                case Tokens.PlatformEnum.Twitter:
                    return (await Tokens.TwitterTokens.Users.LookupAsync(parameters))
                        .Select(x => new Apis.Objects.User(x))
                        .ToList();
                case Tokens.PlatformEnum.Mastodon:
                    throw new NotImplementedException();
            }
            throw new NotImplementedException();
        }

        public Task<Apis.Objects.User> ReportSpamAsync(params Expression<Func<string, object>>[] parameters)
        {
            return ReportSpamAsyncImpl(ExpressionToDictionary(parameters));
        }

        public Task<Apis.Objects.User> ReportSpamAsync(IDictionary<string, object> parameters)
        {
            return ReportSpamAsyncImpl(parameters);
        }

        private async Task<Apis.Objects.User> ReportSpamAsyncImpl(IDictionary<string, object> parameters)
        {
            switch (Tokens.Platform)
            {
                case Tokens.PlatformEnum.Twitter:
                    return new Apis.Objects.User(await Tokens.TwitterTokens.Users.ReportSpamAsync(parameters));
                case Tokens.PlatformEnum.Mastodon:
                    throw new NotImplementedException();
            }
            throw new NotImplementedException();
        }

        public Task<List<Apis.Objects.User>> SearchAsync(params Expression<Func<string, object>>[] parameters)
        {
            return SearchAsyncImpl(ExpressionToDictionary(parameters));
        }

        public Task<List<Apis.Objects.User>> SearchAsync(IDictionary<string, object> parameters)
        {
            return SearchAsyncImpl(parameters);
        }

        private async Task<List<Apis.Objects.User>> SearchAsyncImpl(IDictionary<string, object> parameters)
        {
            switch (Tokens.Platform)
            {
                case Tokens.PlatformEnum.Twitter:
                    return (await Tokens.TwitterTokens.Users.SearchAsync(parameters))
                        .Select(x => new Apis.Objects.User(x))
                        .ToList();
                case Tokens.PlatformEnum.Mastodon:
                    var result =
                        (await Tokens.MastodonTokens.Accounts.SearchAsync(Utils.ConvertToMastodonParameters(parameters))
                        )
                        .Select(x => new Apis.Objects.User(x))
                        .ToList();
                    return result;
            }
            throw new NotImplementedException();
        }

        public Task<Apis.Objects.User> ShowAsync(params Expression<Func<string, object>>[] parameters)
        {
            return ShowAsyncImpl(ExpressionToDictionary(parameters));
        }

        public Task<Apis.Objects.User> ShowAsync(IDictionary<string, object> parameters)
        {
            return ShowAsyncImpl(parameters);
        }

        private async Task<Apis.Objects.User> ShowAsyncImpl(IDictionary<string, object> parameters)
        {
            switch (Tokens.Platform)
            {
                case Tokens.PlatformEnum.Twitter:
                    return new Apis.Objects.User(await Tokens.TwitterTokens.Users.ShowAsync(parameters));
                case Tokens.PlatformEnum.Mastodon:
                    return new Apis.Objects.User(
                        await Tokens.MastodonTokens.Accounts.IdAsync(Utils.ConvertToMastodonParameters(parameters)));
            }
            throw new NotImplementedException();
        }
    }

    public class Trends : ApiBase
    {
        public Task<List<Apis.Objects.Trend>> PlaceAsync(params Expression<Func<string, object>>[] parameters)
        {
            return PlaceAsyncImpl(ExpressionToDictionary(parameters));
        }

        public Task<List<Apis.Objects.Trend>> PlaceAsync(IDictionary<string, object> parameters)
        {
            return PlaceAsyncImpl(parameters);
        }

        private async Task<List<Apis.Objects.Trend>> PlaceAsyncImpl(IDictionary<string, object> parameters)
        {
            switch (Tokens.Platform)
            {
                case Tokens.PlatformEnum.Twitter:
                    return (await Tokens.TwitterTokens.Trends.PlaceAsync(parameters)).SelectMany(x => x.Trends)
                        .Select(x => new Apis.Objects.Trend(x))
                        .ToList();
                case Tokens.PlatformEnum.Mastodon:
                    throw new NotImplementedException();
            }
            throw new NotImplementedException();
        }
    }

    public class Statuses : ApiBase
    {
        public Task<Apis.Objects.Status> DestroyAsync(params Expression<Func<string, object>>[] parameters)
        {
            return DestroyAsyncImpl(ExpressionToDictionary(parameters));
        }

        public Task<Apis.Objects.Status> DestroyAsync(IDictionary<string, object> parameters)
        {
            return DestroyAsyncImpl(parameters);
        }

        private async Task<Apis.Objects.Status> DestroyAsyncImpl(IDictionary<string, object> parameters)
        {
            switch (Tokens.Platform)
            {
                case Tokens.PlatformEnum.Twitter:
                    return new Apis.Objects.Status(await Tokens.TwitterTokens.Statuses.DestroyAsync(parameters));
                case Tokens.PlatformEnum.Mastodon:
                    await Tokens.MastodonTokens.Statuses.DeleteAsync(Utils.ConvertToMastodonParameters(parameters));
                    return null;
            }
            throw new NotImplementedException();
        }

        public Task<List<Apis.Objects.Status>> HomeTimelineAsync(
            params Expression<Func<string, object>>[] parameters)
        {
            return HomeTimelineAsyncImpl(ExpressionToDictionary(parameters));
        }

        public Task<List<Apis.Objects.Status>> HomeTimelineAsync(IDictionary<string, object> parameters)
        {
            return HomeTimelineAsyncImpl(parameters);
        }

        private async Task<List<Apis.Objects.Status>> HomeTimelineAsyncImpl(IDictionary<string, object> parameters)
        {
            switch (Tokens.Platform)
            {
                case Tokens.PlatformEnum.Twitter:
                    return (await Tokens.TwitterTokens.Statuses.HomeTimelineAsync(parameters))
                        .Select(x => new Apis.Objects.Status(x))
                        .ToList();
                case Tokens.PlatformEnum.Mastodon:
                    var result =
                        (await Tokens.MastodonTokens.Timelines.HomeAsync(Utils.ConvertToMastodonParameters(parameters)))
                        .Select(x => new Apis.Objects.Status(x))
                        .ToList();
                    return result;
            }
            throw new NotImplementedException();
        }

        public Task<List<Apis.Objects.Status>> LookupAsync(params Expression<Func<string, object>>[] parameters)
        {
            return LookupAsyncImpl(ExpressionToDictionary(parameters));
        }

        public Task<List<Apis.Objects.Status>> LookupAsync(IDictionary<string, object> parameters)
        {
            return LookupAsyncImpl(parameters);
        }

        private async Task<List<Apis.Objects.Status>> LookupAsyncImpl(IDictionary<string, object> parameters)
        {
            switch (Tokens.Platform)
            {
                case Tokens.PlatformEnum.Twitter:
                    return (await Tokens.TwitterTokens.Statuses.LookupAsync(parameters))
                        .Select(x => new Apis.Objects.Status(x))
                        .ToList();
                case Tokens.PlatformEnum.Mastodon:
                    throw new NotImplementedException();
            }
            throw new NotImplementedException();
        }

        public Task<List<Apis.Objects.Status>> MentionsTimelineAsync(
            params Expression<Func<string, object>>[] parameters)
        {
            return MentionsTimelineAsyncImpl(ExpressionToDictionary(parameters));
        }

        public Task<List<Apis.Objects.Status>> MentionsTimelineAsync(IDictionary<string, object> parameters)
        {
            return MentionsTimelineAsyncImpl(parameters);
        }

        private async Task<List<Apis.Objects.Status>> MentionsTimelineAsyncImpl(
            IDictionary<string, object> parameters)
        {
            switch (Tokens.Platform)
            {
                case Tokens.PlatformEnum.Twitter:
                    return (await Tokens.TwitterTokens.Statuses.MentionsTimelineAsync(parameters))
                        .Select(x => new Apis.Objects.Status(x))
                        .ToList();
                case Tokens.PlatformEnum.Mastodon:
                    var result =
                        (await Tokens.MastodonTokens.Notifications.GetAsync(
                            Utils.ConvertToMastodonParameters(parameters)))
                        .Where(x => x.Type == "mention")
                        .Select(x => new Apis.Objects.Status(x.Status))
                        .ToList();
                    return result;
            }
            throw new NotImplementedException();
        }

        public Task<Apis.Objects.Status> RetweetAsync(params Expression<Func<string, object>>[] parameters)
        {
            return RetweetAsyncImpl(ExpressionToDictionary(parameters));
        }

        public Task<Apis.Objects.Status> RetweetAsync(IDictionary<string, object> parameters)
        {
            return RetweetAsyncImpl(parameters);
        }

        private async Task<Apis.Objects.Status> RetweetAsyncImpl(IDictionary<string, object> parameters)
        {
            switch (Tokens.Platform)
            {
                case Tokens.PlatformEnum.Twitter:
                    return new Apis.Objects.Status(await Tokens.TwitterTokens.Statuses.RetweetAsync(parameters));
                case Tokens.PlatformEnum.Mastodon:
                    return new Apis.Objects.Status(
                        await Tokens.MastodonTokens.Statuses.ReblogAsync(parameters));
            }
            throw new NotImplementedException();
        }

        public Task<CursoredList<long>> RetweetersIdsAsync(params Expression<Func<string, object>>[] parameters)
        {
            return RetweetersIdsAsyncImpl(ExpressionToDictionary(parameters));
        }

        public Task<CursoredList<long>> RetweetersIdsAsync(IDictionary<string, object> parameters)
        {
            return RetweetersIdsAsyncImpl(parameters);
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
                    var data = await Tokens.MastodonTokens.Statuses.RebloggedByAsync(
                        Utils.ConvertToMastodonParameters(parameters));
                    var result = new CursoredList<long>(data.Select(x => x.Id));
                    result.NextCursor = data.MaxId ?? 0;
                    result.PreviousCursor = data.SinceId ?? 0;
                    return result;
            }
            throw new NotImplementedException();
        }

        public Task<CursoredList<Apis.Objects.User>> RetweetersAsync(
            params Expression<Func<string, object>>[] parameters)
        {
            return RetweetersAsyncImpl(ExpressionToDictionary(parameters));
        }

        public Task<CursoredList<Apis.Objects.User>> RetweetersAsync(IDictionary<string, object> parameters)
        {
            return RetweetersAsyncImpl(parameters);
        }

        private async Task<CursoredList<Apis.Objects.User>> RetweetersAsyncImpl(
            IDictionary<string, object> parameters)
        {
            switch (Tokens.Platform)
            {
                case Tokens.PlatformEnum.Twitter:
                    var response = await Tokens.TwitterTokens.Statuses.RetweetersIdsAsync(parameters);
                    var list =
                        new CursoredList<Apis.Objects.User>(
                            (await Tokens.TwitterTokens.Users.LookupAsync(user_id => response)).Select(
                                x => new Apis.Objects.User(x)));
                    list.NextCursor = response.NextCursor;
                    list.PreviousCursor = response.PreviousCursor;
                    return list;
                case Tokens.PlatformEnum.Mastodon:
                    var data = await Tokens.MastodonTokens.Statuses.RebloggedByAsync(
                        Utils.ConvertToMastodonParameters(parameters));
                    var result = new CursoredList<Apis.Objects.User>(data.Select(x => new Apis.Objects.User(x)));
                    result.NextCursor = data.MaxId ?? 0;
                    result.PreviousCursor = data.SinceId ?? 0;
                    return result;
            }
            throw new NotImplementedException();
        }

        public Task<List<Apis.Objects.Status>> RetweetsOfMeAsync(
            params Expression<Func<string, object>>[] parameters)
        {
            return RetweetsOfMeAsyncImpl(ExpressionToDictionary(parameters));
        }

        public Task<List<Apis.Objects.Status>> RetweetsOfMeAsync(IDictionary<string, object> parameters)
        {
            return RetweetsOfMeAsyncImpl(parameters);
        }

        private async Task<List<Apis.Objects.Status>> RetweetsOfMeAsyncImpl(IDictionary<string, object> parameters)
        {
            switch (Tokens.Platform)
            {
                case Tokens.PlatformEnum.Twitter:
                    return (await Tokens.TwitterTokens.Statuses.RetweetsOfMeAsync(parameters))
                        .Select(x => new Apis.Objects.Status(x))
                        .ToList();
                case Tokens.PlatformEnum.Mastodon:
                    throw new NotImplementedException();
            }
            throw new NotImplementedException();
        }

        public Task<Apis.Objects.Status> ShowAsync(params Expression<Func<string, object>>[] parameters)
        {
            return ShowAsyncImpl(ExpressionToDictionary(parameters));
        }

        public Task<Apis.Objects.Status> ShowAsync(IDictionary<string, object> parameters)
        {
            return ShowAsyncImpl(parameters);
        }

        private async Task<Apis.Objects.Status> ShowAsyncImpl(IDictionary<string, object> parameters)
        {
            switch (Tokens.Platform)
            {
                case Tokens.PlatformEnum.Twitter:
                    return new Apis.Objects.Status(await Tokens.TwitterTokens.Statuses.ShowAsync(parameters));
                case Tokens.PlatformEnum.Mastodon:
                    return new Apis.Objects.Status(
                        await Tokens.MastodonTokens.Statuses.IdAsync(Utils.ConvertToMastodonParameters(parameters)));
            }
            throw new NotImplementedException();
        }

        public Task<Apis.Objects.Status> UnretweetAsync(params Expression<Func<string, object>>[] parameters)
        {
            return UnretweetAsyncImpl(ExpressionToDictionary(parameters));
        }

        public Task<Apis.Objects.Status> UnretweetAsync(IDictionary<string, object> parameters)
        {
            return UnretweetAsyncImpl(parameters);
        }

        private async Task<Apis.Objects.Status> UnretweetAsyncImpl(IDictionary<string, object> parameters)
        {
            switch (Tokens.Platform)
            {
                case Tokens.PlatformEnum.Twitter:
                    return new Apis.Objects.Status(await Tokens.TwitterTokens.Statuses.UnretweetAsync(parameters));
                case Tokens.PlatformEnum.Mastodon:
                    return new Apis.Objects.Status(
                        await Tokens.MastodonTokens.Statuses.UnreblogAsync(
                            Utils.ConvertToMastodonParameters(parameters)));
            }
            throw new NotImplementedException();
        }

        public Task<Apis.Objects.Status> UpdateAsync(params Expression<Func<string, object>>[] parameters)
        {
            return UpdateAsyncImpl(ExpressionToDictionary(parameters));
        }

        public Task<Apis.Objects.Status> UpdateAsync(IDictionary<string, object> parameters)
        {
            return UpdateAsyncImpl(parameters);
        }

        private async Task<Apis.Objects.Status> UpdateAsyncImpl(IDictionary<string, object> parameters)
        {
            switch (Tokens.Platform)
            {
                case Tokens.PlatformEnum.Twitter:
                    return new Apis.Objects.Status(await Tokens.TwitterTokens.Statuses.UpdateAsync(parameters));
                case Tokens.PlatformEnum.Mastodon:
                    return new Apis.Objects.Status(
                        await Tokens.MastodonTokens.Statuses.PostAsync(Utils.ConvertToMastodonParameters(parameters)));
            }
            throw new NotImplementedException();
        }

        public Task<List<Apis.Objects.Status>> UserTimelineAsync(
            params Expression<Func<string, object>>[] parameters)
        {
            return UserTimelineAsyncImpl(ExpressionToDictionary(parameters));
        }

        public Task<List<Apis.Objects.Status>> UserTimelineAsync(IDictionary<string, object> parameters)
        {
            return UserTimelineAsyncImpl(parameters);
        }

        private async Task<List<Apis.Objects.Status>> UserTimelineAsyncImpl(IDictionary<string, object> parameters)
        {
            switch (Tokens.Platform)
            {
                case Tokens.PlatformEnum.Twitter:
                    return (await Tokens.TwitterTokens.Statuses.UserTimelineAsync(parameters))
                        .Select(x => new Apis.Objects.Status(x))
                        .ToList();
                case Tokens.PlatformEnum.Mastodon:
                    var result =
                        (await Tokens.MastodonTokens.Accounts.StatusesAsync(
                            Utils.ConvertToMastodonParameters(parameters)))
                        .Select(x => new Apis.Objects.Status(x))
                        .ToList();
                    return result;
            }
            throw new NotImplementedException();
        }

        public Task<List<Apis.Objects.Status>> PublicTimelineAsync(
            params Expression<Func<string, object>>[] parameters)
        {
            return PublicTimelineAsyncImpl(ExpressionToDictionary(parameters));
        }

        public Task<List<Apis.Objects.Status>> PublicTimelineAsync(IDictionary<string, object> parameters)
        {
            return PublicTimelineAsyncImpl(parameters);
        }

        private async Task<List<Apis.Objects.Status>> PublicTimelineAsyncImpl(IDictionary<string, object> parameters)
        {
            switch (Tokens.Platform)
            {
                case Tokens.PlatformEnum.Twitter:
                    throw new NotImplementedException();
                case Tokens.PlatformEnum.Mastodon:
                    var result =
                        (await Tokens.MastodonTokens.Timelines.PublicAsync(
                            Utils.ConvertToMastodonParameters(parameters)))
                        .Select(x => new Apis.Objects.Status(x))
                        .ToList();
                    return result;
            }
            throw new NotImplementedException();
        }
    }

    public class SavedSearches : ApiBase
    {
        public Task<Apis.Objects.SearchQuery> CreateAsync(params Expression<Func<string, object>>[] parameters)
        {
            return CreateAsyncImpl(ExpressionToDictionary(parameters));
        }

        public Task<Apis.Objects.SearchQuery> CreateAsync(IDictionary<string, object> parameters)
        {
            return CreateAsyncImpl(parameters);
        }

        private async Task<Apis.Objects.SearchQuery> CreateAsyncImpl(IDictionary<string, object> parameters)
        {
            switch (Tokens.Platform)
            {
                case Tokens.PlatformEnum.Twitter:
                    return new Apis.Objects.SearchQuery(
                        await Tokens.TwitterTokens.SavedSearches.CreateAsync(parameters));
                case Tokens.PlatformEnum.Mastodon:
                    throw new NotImplementedException();
            }
            throw new NotImplementedException();
        }

        public Task<Apis.Objects.SearchQuery> DestroyAsync(params Expression<Func<string, object>>[] parameters)
        {
            return DestroyAsyncImpl(ExpressionToDictionary(parameters));
        }

        public Task<Apis.Objects.SearchQuery> DestroyAsync(IDictionary<string, object> parameters)
        {
            return DestroyAsyncImpl(parameters);
        }

        private async Task<Apis.Objects.SearchQuery> DestroyAsyncImpl(IDictionary<string, object> parameters)
        {
            switch (Tokens.Platform)
            {
                case Tokens.PlatformEnum.Twitter:
                    return new Apis.Objects.SearchQuery(
                        await Tokens.TwitterTokens.SavedSearches.DestroyAsync(parameters));
                case Tokens.PlatformEnum.Mastodon:
                    throw new NotImplementedException();
            }
            throw new NotImplementedException();
        }

        public Task<List<Apis.Objects.SearchQuery>> ListAsync(params Expression<Func<string, object>>[] parameters)
        {
            return ListAsyncImpl(ExpressionToDictionary(parameters));
        }

        public Task<List<Apis.Objects.SearchQuery>> ListAsync(IDictionary<string, object> parameters)
        {
            return ListAsyncImpl(parameters);
        }

        private async Task<List<Apis.Objects.SearchQuery>> ListAsyncImpl(IDictionary<string, object> parameters)
        {
            switch (Tokens.Platform)
            {
                case Tokens.PlatformEnum.Twitter:
                    return (await Tokens.TwitterTokens.SavedSearches.ListAsync(parameters))
                        .Select(x => new Apis.Objects.SearchQuery(x))
                        .ToList();
                case Tokens.PlatformEnum.Mastodon:
                    throw new NotImplementedException();
            }
            throw new NotImplementedException();
        }
    }

    public class Search : ApiBase
    {
        public Task<List<Apis.Objects.Status>> TweetsAsync(params Expression<Func<string, object>>[] parameters)
        {
            return TweetsAsyncImpl(ExpressionToDictionary(parameters));
        }

        public Task<List<Apis.Objects.Status>> TweetsAsync(IDictionary<string, object> parameters)
        {
            return TweetsAsyncImpl(parameters);
        }

        private async Task<List<Apis.Objects.Status>> TweetsAsyncImpl(IDictionary<string, object> parameters)
        {
            switch (Tokens.Platform)
            {
                case Tokens.PlatformEnum.Twitter:
                    return (await Tokens.TwitterTokens.Search.TweetsAsync(parameters))
                        .Select(x => new Apis.Objects.Status(x))
                        .ToList();
                case Tokens.PlatformEnum.Mastodon:
                    var query = (string) parameters["q"];
                    if (query.StartsWith("#"))
                        query = query.Replace("#", "");
                    parameters["hashtag"] = query;
                    var result =
                        (await Tokens.MastodonTokens.Timelines.TagAsync(Utils.ConvertToMastodonParameters(parameters)))
                        .Select(x => new Apis.Objects.Status(x))
                        .ToList();
                    return result;
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
        public Task<Apis.Objects.User> CreateAsync(params Expression<Func<string, object>>[] parameters)
        {
            return CreateAsyncImpl(ExpressionToDictionary(parameters));
        }

        public Task<Apis.Objects.User> CreateAsync(IDictionary<string, object> parameters)
        {
            return CreateAsyncImpl(parameters);
        }

        private async Task<Apis.Objects.User> CreateAsyncImpl(IDictionary<string, object> parameters)
        {
            switch (Tokens.Platform)
            {
                case Tokens.PlatformEnum.Twitter:
                    return new Apis.Objects.User(await Tokens.TwitterTokens.Mutes.Users.CreateAsync(parameters));
                case Tokens.PlatformEnum.Mastodon:
                    return new Apis.Objects.User(
                        await Tokens.MastodonTokens.Accounts.MuteAsync(Utils.ConvertToMastodonParameters(parameters)));
            }
            throw new NotImplementedException();
        }

        public Task<Apis.Objects.User> DestroyAsync(params Expression<Func<string, object>>[] parameters)
        {
            return DestroyAsyncImpl(ExpressionToDictionary(parameters));
        }

        public Task<Apis.Objects.User> DestroyAsync(IDictionary<string, object> parameters)
        {
            return DestroyAsyncImpl(parameters);
        }

        private async Task<Apis.Objects.User> DestroyAsyncImpl(IDictionary<string, object> parameters)
        {
            switch (Tokens.Platform)
            {
                case Tokens.PlatformEnum.Twitter:
                    return new Apis.Objects.User(await Tokens.TwitterTokens.Mutes.Users.DestroyAsync(parameters));
                case Tokens.PlatformEnum.Mastodon:
                    return new Apis.Objects.User(
                        await Tokens.MastodonTokens.Accounts.UnmuteAsync(
                            Utils.ConvertToMastodonParameters(parameters)));
            }
            throw new NotImplementedException();
        }

        public Task<CursoredList<long>> IdsAsync(params Expression<Func<string, object>>[] parameters)
        {
            return IdsAsyncImpl(ExpressionToDictionary(parameters));
        }

        public Task<CursoredList<long>> IdsAsync(IDictionary<string, object> parameters)
        {
            return IdsAsyncImpl(parameters);
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
                    var data = await Tokens.MastodonTokens.Mutes.GetAsync(
                        Utils.ConvertToMastodonParameters(parameters));
                    var result = new CursoredList<long>(data.Select(x => x.Id));
                    result.NextCursor = data.MaxId ?? 0;
                    result.PreviousCursor = data.SinceId ?? 0;
                    return result;
            }
            throw new NotImplementedException();
        }

        public Task<CursoredList<Apis.Objects.User>> ListAsync(params Expression<Func<string, object>>[] parameters)
        {
            return ListAsyncImpl(ExpressionToDictionary(parameters));
        }

        public Task<CursoredList<Apis.Objects.User>> ListAsync(IDictionary<string, object> parameters)
        {
            return ListAsyncImpl(parameters);
        }

        private async Task<CursoredList<Apis.Objects.User>> ListAsyncImpl(IDictionary<string, object> parameters)
        {
            switch (Tokens.Platform)
            {
                case Tokens.PlatformEnum.Twitter:
                    var response = await Tokens.TwitterTokens.Mutes.Users.ListAsync(parameters);
                    var list =
                        new CursoredList<Apis.Objects.User>(response.Select(x => new Apis.Objects.User(x)));
                    list.NextCursor = response.NextCursor;
                    list.PreviousCursor = response.PreviousCursor;
                    return list;
                case Tokens.PlatformEnum.Mastodon:
                    var data = await Tokens.MastodonTokens.Mutes.GetAsync(
                        Utils.ConvertToMastodonParameters(parameters));
                    var result = new CursoredList<Apis.Objects.User>(data.Select(x => new Apis.Objects.User(x)));
                    result.NextCursor = data.MaxId ?? 0;
                    result.PreviousCursor = data.SinceId ?? 0;
                    return result;
            }
            throw new NotImplementedException();
        }
    }

    public class Lists : ApiBase
    {
        public ListsSubscribers Subscribers { get; set; }
        public ListsMembers Members { get; set; }

        public Task<Apis.Objects.List> CreateAsync(params Expression<Func<string, object>>[] parameters)
        {
            return CreateAsyncImpl(ExpressionToDictionary(parameters));
        }

        public Task<Apis.Objects.List> CreateAsync(IDictionary<string, object> parameters)
        {
            return CreateAsyncImpl(parameters);
        }

        private async Task<Apis.Objects.List> CreateAsyncImpl(IDictionary<string, object> parameters)
        {
            switch (Tokens.Platform)
            {
                case Tokens.PlatformEnum.Twitter:
                    return new Apis.Objects.List(
                        await Tokens.TwitterTokens.Lists.CreateAsync(parameters));
                case Tokens.PlatformEnum.Mastodon:
                    return new Apis.Objects.List(
                        await Tokens.MastodonTokens.Lists.CreateAsync(Utils.ConvertToMastodonParameters(parameters)));
            }
            throw new NotImplementedException();
        }

        public Task<Apis.Objects.List> DestroyAsync(params Expression<Func<string, object>>[] parameters)
        {
            return DestroyAsyncImpl(ExpressionToDictionary(parameters));
        }

        public Task<Apis.Objects.List> DestroyAsync(IDictionary<string, object> parameters)
        {
            return DestroyAsyncImpl(parameters);
        }

        private async Task<Apis.Objects.List> DestroyAsyncImpl(IDictionary<string, object> parameters)
        {
            switch (Tokens.Platform)
            {
                case Tokens.PlatformEnum.Twitter:
                    return new Apis.Objects.List(
                        await Tokens.TwitterTokens.Lists.DestroyAsync(parameters));
                case Tokens.PlatformEnum.Mastodon:
                    await Tokens.MastodonTokens.Lists.DeleteAsync(Utils.ConvertToMastodonParameters(parameters));
                    return new Apis.Objects.List();
            }
            throw new NotImplementedException();
        }

        public Task<Apis.Objects.List> UpdateAsync(params Expression<Func<string, object>>[] parameters)
        {
            return UpdateAsyncImpl(ExpressionToDictionary(parameters));
        }

        public Task<Apis.Objects.List> UpdateAsync(IDictionary<string, object> parameters)
        {
            return UpdateAsyncImpl(parameters);
        }

        private async Task<Apis.Objects.List> UpdateAsyncImpl(IDictionary<string, object> parameters)
        {
            switch (Tokens.Platform)
            {
                case Tokens.PlatformEnum.Twitter:
                    return new Apis.Objects.List(
                        await Tokens.TwitterTokens.Lists.UpdateAsync(parameters));
                case Tokens.PlatformEnum.Mastodon:
                    return new Apis.Objects.List(
                        await Tokens.MastodonTokens.Lists.UpdateAsync(Utils.ConvertToMastodonParameters(parameters)));
            }
            throw new NotImplementedException();
        }

        public Task<CursoredList<Apis.Objects.List>> MembershipsAsync(
            params Expression<Func<string, object>>[] parameters)
        {
            return MembershipsAsyncImpl(ExpressionToDictionary(parameters));
        }

        public Task<CursoredList<Apis.Objects.List>> MembershipsAsync(IDictionary<string, object> parameters)
        {
            return MembershipsAsyncImpl(parameters);
        }

        private async Task<CursoredList<Apis.Objects.List>> MembershipsAsyncImpl(
            IDictionary<string, object> parameters)
        {
            switch (Tokens.Platform)
            {
                case Tokens.PlatformEnum.Twitter:
                    var response = await Tokens.TwitterTokens.Lists.MembershipsAsync(parameters);
                    var list =
                        new CursoredList<Apis.Objects.List>(response.Select(x => new Apis.Objects.List(x)));
                    list.NextCursor = response.NextCursor;
                    list.PreviousCursor = response.PreviousCursor;
                    return list;
                case Tokens.PlatformEnum.Mastodon:
                    throw new NotImplementedException();
            }
            throw new NotImplementedException();
        }

        public Task<CursoredList<Apis.Objects.List>> OwnershipsAsync(
            params Expression<Func<string, object>>[] parameters)
        {
            return OwnershipsAsyncImpl(ExpressionToDictionary(parameters));
        }

        public Task<CursoredList<Apis.Objects.List>> OwnershipsAsync(IDictionary<string, object> parameters)
        {
            return OwnershipsAsyncImpl(parameters);
        }

        private async Task<CursoredList<Apis.Objects.List>> OwnershipsAsyncImpl(
            IDictionary<string, object> parameters)
        {
            switch (Tokens.Platform)
            {
                case Tokens.PlatformEnum.Twitter:
                    var response = await Tokens.TwitterTokens.Lists.OwnershipsAsync(parameters);
                    var list =
                        new CursoredList<Apis.Objects.List>(response.Select(x => new Apis.Objects.List(x)));
                    list.NextCursor = response.NextCursor;
                    list.PreviousCursor = response.PreviousCursor;
                    return list;
                case Tokens.PlatformEnum.Mastodon:
                    var data = await Tokens.MastodonTokens.Lists.GetAsync(
                        Utils.ConvertToMastodonParameters(parameters));
                    var result = new CursoredList<Apis.Objects.List>(data.Select(x => new Apis.Objects.List(x)));
                    result.NextCursor = data.MaxId ?? 0;
                    result.PreviousCursor = data.SinceId ?? 0;
                    return result;
            }
            throw new NotImplementedException();
        }

        public Task<List<Apis.Objects.Status>> StatusesAsync(params Expression<Func<string, object>>[] parameters)
        {
            return StatusesAsyncImpl(ExpressionToDictionary(parameters));
        }

        public Task<List<Apis.Objects.Status>> StatusesAsync(IDictionary<string, object> parameters)
        {
            return StatusesAsyncImpl(parameters);
        }

        private async Task<List<Apis.Objects.Status>> StatusesAsyncImpl(IDictionary<string, object> parameters)
        {
            switch (Tokens.Platform)
            {
                case Tokens.PlatformEnum.Twitter:
                    return (await Tokens.TwitterTokens.Lists.StatusesAsync(parameters))
                        .Select(x => new Apis.Objects.Status(x))
                        .ToList();
                case Tokens.PlatformEnum.Mastodon:
                    return (await Tokens.MastodonTokens.Timelines.ListAsync(parameters))
                        .Select(x => new Apis.Objects.Status(x))
                        .ToList();
            }
            throw new NotImplementedException();
        }

        public Task<CursoredList<Apis.Objects.List>> SubscriptionsAsync(
            params Expression<Func<string, object>>[] parameters)
        {
            return SubscriptionsAsyncImpl(ExpressionToDictionary(parameters));
        }

        public Task<CursoredList<Apis.Objects.List>> SubscriptionsAsync(IDictionary<string, object> parameters)
        {
            return SubscriptionsAsyncImpl(parameters);
        }

        private async Task<CursoredList<Apis.Objects.List>> SubscriptionsAsyncImpl(
            IDictionary<string, object> parameters)
        {
            switch (Tokens.Platform)
            {
                case Tokens.PlatformEnum.Twitter:
                    var response = await Tokens.TwitterTokens.Lists.SubscriptionsAsync(parameters);
                    var list =
                        new CursoredList<Apis.Objects.List>(response.Select(x => new Apis.Objects.List(x)));
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
        public Task<Apis.Objects.List> CreateAsync(params Expression<Func<string, object>>[] parameters)
        {
            return CreateAsyncImpl(ExpressionToDictionary(parameters));
        }

        public Task<Apis.Objects.List> CreateAsync(IDictionary<string, object> parameters)
        {
            return CreateAsyncImpl(parameters);
        }

        private async Task<Apis.Objects.List> CreateAsyncImpl(IDictionary<string, object> parameters)
        {
            switch (Tokens.Platform)
            {
                case Tokens.PlatformEnum.Twitter:
                    return new Apis.Objects.List(
                        await Tokens.TwitterTokens.Lists.Subscribers.CreateAsync(parameters));
                case Tokens.PlatformEnum.Mastodon:
                    throw new NotImplementedException();
            }
            throw new NotImplementedException();
        }

        public Task<Apis.Objects.List> DestroyAsync(params Expression<Func<string, object>>[] parameters)
        {
            return DestroyAsyncImpl(ExpressionToDictionary(parameters));
        }

        public Task<Apis.Objects.List> DestroyAsync(IDictionary<string, object> parameters)
        {
            return DestroyAsyncImpl(parameters);
        }

        private async Task<Apis.Objects.List> DestroyAsyncImpl(IDictionary<string, object> parameters)
        {
            switch (Tokens.Platform)
            {
                case Tokens.PlatformEnum.Twitter:
                    return new Apis.Objects.List(
                        await Tokens.TwitterTokens.Lists.Subscribers.DestroyAsync(parameters));
                case Tokens.PlatformEnum.Mastodon:
                    throw new NotImplementedException();
            }
            throw new NotImplementedException();
        }

        public Task<CursoredList<Apis.Objects.User>> ListAsync(params Expression<Func<string, object>>[] parameters)
        {
            return ListAsyncImpl(ExpressionToDictionary(parameters));
        }

        public Task<CursoredList<Apis.Objects.User>> ListAsync(IDictionary<string, object> parameters)
        {
            return ListAsyncImpl(parameters);
        }

        private async Task<CursoredList<Apis.Objects.User>> ListAsyncImpl(IDictionary<string, object> parameters)
        {
            switch (Tokens.Platform)
            {
                case Tokens.PlatformEnum.Twitter:
                    var response = await Tokens.TwitterTokens.Lists.Subscribers.ListAsync(parameters);
                    var list =
                        new CursoredList<Apis.Objects.User>(response.Select(x => new Apis.Objects.User(x)));
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
        public Task<Apis.Objects.List> CreateAsync(params Expression<Func<string, object>>[] parameters)
        {
            return CreateAsyncImpl(ExpressionToDictionary(parameters));
        }

        public Task<Apis.Objects.List> CreateAsync(IDictionary<string, object> parameters)
        {
            return CreateAsyncImpl(parameters);
        }

        private async Task<Apis.Objects.List> CreateAsyncImpl(IDictionary<string, object> parameters)
        {
            switch (Tokens.Platform)
            {
                case Tokens.PlatformEnum.Twitter:
                    return new Apis.Objects.List(await Tokens.TwitterTokens.Lists.Members.CreateAsync(parameters));
                case Tokens.PlatformEnum.Mastodon:
                    parameters["account_ids"] = new List<long> {(long) parameters["user_id"]};
                    parameters.Remove("user_id");
                    await Tokens.MastodonTokens.Lists.AddAccountAsync(Utils.ConvertToMastodonParameters(parameters));
                    return new Apis.Objects.List();
            }
            throw new NotImplementedException();
        }

        public Task<Apis.Objects.List> DestroyAsync(params Expression<Func<string, object>>[] parameters)
        {
            return DestroyAsyncImpl(ExpressionToDictionary(parameters));
        }

        public Task<Apis.Objects.List> DestroyAsync(IDictionary<string, object> parameters)
        {
            return DestroyAsyncImpl(parameters);
        }

        private async Task<Apis.Objects.List> DestroyAsyncImpl(IDictionary<string, object> parameters)
        {
            switch (Tokens.Platform)
            {
                case Tokens.PlatformEnum.Twitter:
                    return new Apis.Objects.List(await Tokens.TwitterTokens.Lists.Members.DestroyAsync(parameters));
                case Tokens.PlatformEnum.Mastodon:
                    parameters["account_ids"] = new List<long> { (long)parameters["user_id"] };
                    parameters.Remove("user_id");
                    await Tokens.MastodonTokens.Lists.DeleteAccountAsync(Utils.ConvertToMastodonParameters(parameters));
                    return new Apis.Objects.List();
            }
            throw new NotImplementedException();
        }

        public Task<CursoredList<Apis.Objects.User>> ListAsync(params Expression<Func<string, object>>[] parameters)
        {
            return ListAsyncImpl(ExpressionToDictionary(parameters));
        }

        public Task<CursoredList<Apis.Objects.User>> ListAsync(IDictionary<string, object> parameters)
        {
            return ListAsyncImpl(parameters);
        }

        private async Task<CursoredList<Apis.Objects.User>> ListAsyncImpl(IDictionary<string, object> parameters)
        {
            switch (Tokens.Platform)
            {
                case Tokens.PlatformEnum.Twitter:
                    var response = await Tokens.TwitterTokens.Lists.Members.ListAsync(parameters);
                    var list =
                        new CursoredList<Apis.Objects.User>(response.Select(x => new Apis.Objects.User(x)));
                    list.NextCursor = response.NextCursor;
                    list.PreviousCursor = response.PreviousCursor;
                    return list;
                case Tokens.PlatformEnum.Mastodon:
                    var data = await Tokens.MastodonTokens.Lists.AccountsAsync(
                        Utils.ConvertToMastodonParameters(parameters));
                    var result = new CursoredList<Apis.Objects.User>(data.Select(x => new Apis.Objects.User(x)));
                    result.NextCursor = data.MaxId ?? 0;
                    result.PreviousCursor = data.SinceId ?? 0;
                    return result;
            }
            throw new NotImplementedException();
        }
    }

    public class Friendships : ApiBase
    {
        public Task<Apis.Objects.User> CreateAsync(params Expression<Func<string, object>>[] parameters)
        {
            return CreateAsyncImpl(ExpressionToDictionary(parameters));
        }

        public Task<Apis.Objects.User> CreateAsync(IDictionary<string, object> parameters)
        {
            return CreateAsyncImpl(parameters);
        }

        private async Task<Apis.Objects.User> CreateAsyncImpl(IDictionary<string, object> parameters)
        {
            switch (Tokens.Platform)
            {
                case Tokens.PlatformEnum.Twitter:
                    return new Apis.Objects.User(await Tokens.TwitterTokens.Friendships.CreateAsync(parameters));
                case Tokens.PlatformEnum.Mastodon:
                    if (parameters.ContainsKey("screen_name"))
                        return new Apis.Objects.User(
                            await Tokens.MastodonTokens.Follows.PostAsync(uri => (string) parameters["screen_name"]));
                    else
                        return new Apis.Objects.User(
                            await Tokens.MastodonTokens.Accounts.FollowAsync(
                                Utils.ConvertToMastodonParameters(parameters)));
            }
            throw new NotImplementedException();
        }

        public Task<Apis.Objects.User> DestroyAsync(params Expression<Func<string, object>>[] parameters)
        {
            return DestroyAsyncImpl(ExpressionToDictionary(parameters));
        }

        public Task<Apis.Objects.User> DestroyAsync(IDictionary<string, object> parameters)
        {
            return DestroyAsyncImpl(parameters);
        }

        private async Task<Apis.Objects.User> DestroyAsyncImpl(IDictionary<string, object> parameters)
        {
            switch (Tokens.Platform)
            {
                case Tokens.PlatformEnum.Twitter:
                    return new Apis.Objects.User(await Tokens.TwitterTokens.Friendships.DestroyAsync(parameters));
                case Tokens.PlatformEnum.Mastodon:
                    return new Apis.Objects.User(
                        await Tokens.MastodonTokens.Accounts.UnfollowAsync(
                            Utils.ConvertToMastodonParameters(parameters)));
            }
            throw new NotImplementedException();
        }

        public Task<List<long>> NoRetweetsIdsAsync(params Expression<Func<string, object>>[] parameters)
        {
            return NoRetweetsIdsAsyncImpl(ExpressionToDictionary(parameters));
        }

        public Task<List<long>> NoRetweetsIdsAsync(IDictionary<string, object> parameters)
        {
            return NoRetweetsIdsAsyncImpl(parameters);
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

        public Task<Apis.Objects.Relationship> ShowAsync(params Expression<Func<string, object>>[] parameters)
        {
            return ShowAsyncImpl(ExpressionToDictionary(parameters));
        }

        public Task<Apis.Objects.Relationship> ShowAsync(IDictionary<string, object> parameters)
        {
            return ShowAsyncImpl(parameters);
        }

        private async Task<Apis.Objects.Relationship> ShowAsyncImpl(IDictionary<string, object> parameters)
        {
            switch (Tokens.Platform)
            {
                case Tokens.PlatformEnum.Twitter:
                    return new Apis.Objects.Relationship(
                        await Tokens.TwitterTokens.Friendships.ShowAsync(parameters));
                case Tokens.PlatformEnum.Mastodon:
                    var ids = new List<long>();
                    ids.Add((long) parameters["target_id"]);
                    return new Apis.Objects.Relationship(
                        (await Tokens.MastodonTokens.Accounts.RelationshipsAsync(id => ids))
                        .First());
            }
            throw new NotImplementedException();
        }
    }

    public class Followers : ApiBase
    {
        public Task<CursoredList<long>> IdsAsync(params Expression<Func<string, object>>[] parameters)
        {
            return IdsAsyncImpl(ExpressionToDictionary(parameters));
        }

        public Task<CursoredList<long>> IdsAsync(IDictionary<string, object> parameters)
        {
            return IdsAsyncImpl(parameters);
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
                    var data = await Tokens.MastodonTokens.Accounts.FollowersAsync(
                        Utils.ConvertToMastodonParameters(parameters));
                    var result = new CursoredList<long>(data.Select(x => x.Id));
                    result.NextCursor = data.MaxId ?? 0;
                    result.PreviousCursor = data.SinceId ?? 0;
                    return result;
            }
            throw new NotImplementedException();
        }

        public Task<CursoredList<Apis.Objects.User>> ListAsync(params Expression<Func<string, object>>[] parameters)
        {
            return ListAsyncImpl(ExpressionToDictionary(parameters));
        }

        public Task<CursoredList<Apis.Objects.User>> ListAsync(IDictionary<string, object> parameters)
        {
            return ListAsyncImpl(parameters);
        }

        private async Task<CursoredList<Apis.Objects.User>> ListAsyncImpl(IDictionary<string, object> parameters)
        {
            switch (Tokens.Platform)
            {
                case Tokens.PlatformEnum.Twitter:
                    var response = await Tokens.TwitterTokens.Followers.ListAsync(parameters);
                    var list =
                        new CursoredList<Apis.Objects.User>(response.Select(x => new Apis.Objects.User(x)));
                    list.NextCursor = response.NextCursor;
                    list.PreviousCursor = response.PreviousCursor;
                    return list;
                case Tokens.PlatformEnum.Mastodon:
                    var data = await Tokens.MastodonTokens.Accounts.FollowersAsync(
                        Utils.ConvertToMastodonParameters(parameters));
                    var result = new CursoredList<Apis.Objects.User>(data.Select(x => new Apis.Objects.User(x)));
                    result.NextCursor = data.MaxId ?? 0;
                    result.PreviousCursor = data.SinceId ?? 0;
                    return result;
            }
            throw new NotImplementedException();
        }
    }

    public class Friends : ApiBase
    {
        public Task<CursoredList<long>> IdsAsync(params Expression<Func<string, object>>[] parameters)
        {
            return IdsAsyncImpl(ExpressionToDictionary(parameters));
        }

        public Task<CursoredList<long>> IdsAsync(IDictionary<string, object> parameters)
        {
            return IdsAsyncImpl(parameters);
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
                    var data = await Tokens.MastodonTokens.Accounts.FollowingAsync(
                        Utils.ConvertToMastodonParameters(parameters));
                    var result = new CursoredList<long>(data.Select(x => x.Id));
                    result.NextCursor = data.MaxId ?? 0;
                    result.PreviousCursor = data.SinceId ?? 0;
                    return result;
            }
            throw new NotImplementedException();
        }

        public Task<CursoredList<Apis.Objects.User>> ListAsync(params Expression<Func<string, object>>[] parameters)
        {
            return ListAsyncImpl(ExpressionToDictionary(parameters));
        }

        public Task<CursoredList<Apis.Objects.User>> ListAsync(IDictionary<string, object> parameters)
        {
            return ListAsyncImpl(parameters);
        }

        private async Task<CursoredList<Apis.Objects.User>> ListAsyncImpl(IDictionary<string, object> parameters)
        {
            switch (Tokens.Platform)
            {
                case Tokens.PlatformEnum.Twitter:
                    var response = await Tokens.TwitterTokens.Friends.ListAsync(parameters);
                    var list =
                        new CursoredList<Apis.Objects.User>(response.Select(x => new Apis.Objects.User(x)));
                    list.NextCursor = response.NextCursor;
                    list.PreviousCursor = response.PreviousCursor;
                    return list;
                case Tokens.PlatformEnum.Mastodon:
                    var data = await Tokens.MastodonTokens.Accounts.FollowingAsync(
                        Utils.ConvertToMastodonParameters(parameters));
                    var result = new CursoredList<Apis.Objects.User>(data.Select(x => new Apis.Objects.User(x)));
                    result.NextCursor = data.MaxId ?? 0;
                    result.PreviousCursor = data.SinceId ?? 0;
                    return result;
            }
            throw new NotImplementedException();
        }
    }

    public class DirectMessages : ApiBase
    {
        public Task<Apis.Objects.DirectMessage> DestroyAsync(params Expression<Func<string, object>>[] parameters)
        {
            return DestroyAsyncImpl(ExpressionToDictionary(parameters));
        }

        public Task<Apis.Objects.DirectMessage> DestroyAsync(IDictionary<string, object> parameters)
        {
            return DestroyAsyncImpl(parameters);
        }

        private async Task<Apis.Objects.DirectMessage> DestroyAsyncImpl(IDictionary<string, object> parameters)
        {
            switch (Tokens.Platform)
            {
                case Tokens.PlatformEnum.Twitter:
                    return new Apis.Objects.DirectMessage(
                        await Tokens.TwitterTokens.DirectMessages.DestroyAsync(parameters));
                case Tokens.PlatformEnum.Mastodon:
                    await Tokens.MastodonTokens.Statuses.DeleteAsync(Utils.ConvertToMastodonParameters(parameters));
                    return null;
            }
            throw new NotImplementedException();
        }

        public Task<Apis.Objects.DirectMessage> NewAsync(params Expression<Func<string, object>>[] parameters)
        {
            return NewAsyncImpl(ExpressionToDictionary(parameters));
        }

        public Task<Apis.Objects.DirectMessage> NewAsync(IDictionary<string, object> parameters)
        {
            return NewAsyncImpl(parameters);
        }

        private async Task<Apis.Objects.DirectMessage> NewAsyncImpl(IDictionary<string, object> parameters)
        {
            switch (Tokens.Platform)
            {
                case Tokens.PlatformEnum.Twitter:
                    return new Apis.Objects.DirectMessage(
                        await Tokens.TwitterTokens.DirectMessages.NewAsync(parameters));
                case Tokens.PlatformEnum.Mastodon:
                    if (!parameters.ContainsKey("in_reply_to_status_id"))
                        throw new NotImplementedException();
                    var status =
                        await Tokens.MastodonTokens.Statuses.PostAsync(Utils.ConvertToMastodonParameters(parameters));
                    return new Apis.Objects.DirectMessage(status,
                        (await Tokens.MastodonTokens.Statuses.IdAsync(id => status.InReplyToId.Value)).Account);
            }
            throw new NotImplementedException();
        }

        public Task<List<Apis.Objects.DirectMessage>> ReceivedAsync(
            params Expression<Func<string, object>>[] parameters)
        {
            return ReceivedAsyncImpl(ExpressionToDictionary(parameters));
        }

        public Task<List<Apis.Objects.DirectMessage>> ReceivedAsync(IDictionary<string, object> parameters)
        {
            return ReceivedAsyncImpl(parameters);
        }

        private async Task<List<Apis.Objects.DirectMessage>> ReceivedAsyncImpl(
            IDictionary<string, object> parameters)
        {
            switch (Tokens.Platform)
            {
                case Tokens.PlatformEnum.Twitter:
                    return (await Tokens.TwitterTokens.DirectMessages.ReceivedAsync(parameters))
                        .Select(x => new Apis.Objects.DirectMessage(x))
                        .ToList();
                case Tokens.PlatformEnum.Mastodon:
                    var statuses =
                        (await Tokens.MastodonTokens.Notifications.GetAsync(
                            Utils.ConvertToMastodonParameters(parameters)))
                        .Where(x => x.Type == "mention")
                        .Select(x => x.Status);
                    var tasks = statuses.Select(async x => new Apis.Objects.DirectMessage(x,
                        (await Tokens.MastodonTokens.Statuses.IdAsync(id => x.InReplyToId.Value)).Account));
                    return (await Task.WhenAll(tasks)).ToList();
            }
            throw new NotImplementedException();
        }

        public Task<List<Apis.Objects.DirectMessage>> SentAsync(params Expression<Func<string, object>>[] parameters)
        {
            return SentAsyncImpl(ExpressionToDictionary(parameters));
        }

        public Task<List<Apis.Objects.DirectMessage>> SentAsync(IDictionary<string, object> parameters)
        {
            return SentAsyncImpl(parameters);
        }

        private async Task<List<Apis.Objects.DirectMessage>> SentAsyncImpl(IDictionary<string, object> parameters)
        {
            switch (Tokens.Platform)
            {
                case Tokens.PlatformEnum.Twitter:
                    return (await Tokens.TwitterTokens.DirectMessages.SentAsync(parameters))
                        .Select(x => new Apis.Objects.DirectMessage(x))
                        .ToList();
                case Tokens.PlatformEnum.Mastodon:
                    throw new NotImplementedException();
            }
            throw new NotImplementedException();
        }
    }

    public class Collections : ApiBase
    {
        public Task<Apis.Objects.Collection> CreateAsync(params Expression<Func<string, object>>[] parameters)
        {
            return CreateAsyncImpl(ExpressionToDictionary(parameters));
        }

        public Task<Apis.Objects.Collection> CreateAsync(IDictionary<string, object> parameters)
        {
            return CreateAsyncImpl(parameters);
        }

        private async Task<Apis.Objects.Collection> CreateAsyncImpl(IDictionary<string, object> parameters)
        {
            switch (Tokens.Platform)
            {
                case Tokens.PlatformEnum.Twitter:
                    return new Apis.Objects.Collection(
                        await Tokens.TwitterTokens.Collections.CreateAsync(parameters));
                case Tokens.PlatformEnum.Mastodon:
                    throw new NotImplementedException();
            }
            throw new NotImplementedException();
        }

        public Task<bool> DestroyAsync(params Expression<Func<string, object>>[] parameters)
        {
            return DestroyAsyncImpl(ExpressionToDictionary(parameters));
        }

        public Task<bool> DestroyAsync(IDictionary<string, object> parameters)
        {
            return DestroyAsyncImpl(parameters);
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
            return EntriesAddAsyncImpl(ExpressionToDictionary(parameters));
        }

        public Task<bool> EntriesAddAsync(IDictionary<string, object> parameters)
        {
            return EntriesAddAsyncImpl(parameters);
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

        public Task<List<Apis.Objects.CollectionEntry>> EntriesAsync(
            params Expression<Func<string, object>>[] parameters)
        {
            return EntriesAsyncImpl(ExpressionToDictionary(parameters));
        }

        public Task<List<Apis.Objects.CollectionEntry>> EntriesAsync(IDictionary<string, object> parameters)
        {
            return EntriesAsyncImpl(parameters);
        }

        private async Task<List<Apis.Objects.CollectionEntry>> EntriesAsyncImpl(
            IDictionary<string, object> parameters)
        {
            switch (Tokens.Platform)
            {
                case Tokens.PlatformEnum.Twitter:
                    return (await Tokens.TwitterTokens.Collections.EntriesAsync(parameters)).Entries
                        .Select(x => new Apis.Objects.CollectionEntry(x))
                        .ToList();
                case Tokens.PlatformEnum.Mastodon:
                    throw new NotImplementedException();
            }
            throw new NotImplementedException();
        }

        public Task<bool> EntriesRemoveAsync(params Expression<Func<string, object>>[] parameters)
        {
            return EntriesRemoveAsyncImpl(ExpressionToDictionary(parameters));
        }

        public Task<bool> EntriesRemoveAsync(IDictionary<string, object> parameters)
        {
            return EntriesRemoveAsyncImpl(parameters);
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

        public Task<StringCursoredList<Apis.Objects.Collection>> ListAsync(
            params Expression<Func<string, object>>[] parameters)
        {
            return ListAsyncImpl(ExpressionToDictionary(parameters));
        }

        public Task<StringCursoredList<Apis.Objects.Collection>> ListAsync(IDictionary<string, object> parameters)
        {
            return ListAsyncImpl(parameters);
        }

        private async Task<StringCursoredList<Apis.Objects.Collection>> ListAsyncImpl(
            IDictionary<string, object> parameters)
        {
            switch (Tokens.Platform)
            {
                case Tokens.PlatformEnum.Twitter:
                    var response = await Tokens.TwitterTokens.Collections.ListAsync(parameters);
                    var list =
                        new StringCursoredList<Apis.Objects.Collection>(
                            response.Results.Select(x => new Apis.Objects.Collection(x)));
                    list.NextCursor = response.Cursors.NextCursor;
                    list.PreviousCursor = response.Cursors.PreviousCursor;
                    return list;
                case Tokens.PlatformEnum.Mastodon:
                    throw new NotImplementedException();
            }
            throw new NotImplementedException();
        }

        public Task<Apis.Objects.Collection> ShowAsync(params Expression<Func<string, object>>[] parameters)
        {
            return ShowAsyncImpl(ExpressionToDictionary(parameters));
        }

        public Task<Apis.Objects.Collection> ShowAsync(IDictionary<string, object> parameters)
        {
            return ShowAsyncImpl(parameters);
        }

        private async Task<Apis.Objects.Collection> ShowAsyncImpl(IDictionary<string, object> parameters)
        {
            switch (Tokens.Platform)
            {
                case Tokens.PlatformEnum.Twitter:
                    return new Apis.Objects.Collection(await Tokens.TwitterTokens.Collections.ShowAsync(parameters));
                case Tokens.PlatformEnum.Mastodon:
                    throw new NotImplementedException();
            }
            throw new NotImplementedException();
        }

        public Task<Apis.Objects.Collection> UpdateAsync(params Expression<Func<string, object>>[] parameters)
        {
            return UpdateAsyncImpl(ExpressionToDictionary(parameters));
        }

        public Task<Apis.Objects.Collection> UpdateAsync(IDictionary<string, object> parameters)
        {
            return UpdateAsyncImpl(parameters);
        }

        private async Task<Apis.Objects.Collection> UpdateAsyncImpl(IDictionary<string, object> parameters)
        {
            switch (Tokens.Platform)
            {
                case Tokens.PlatformEnum.Twitter:
                    return new Apis.Objects.Collection(
                        await Tokens.TwitterTokens.Collections.UpdateAsync(parameters));
                case Tokens.PlatformEnum.Mastodon:
                    throw new NotImplementedException();
            }
            throw new NotImplementedException();
        }
    }

    public class Blocks : ApiBase
    {
        public Task<Apis.Objects.User> CreateAsync(params Expression<Func<string, object>>[] parameters)
        {
            return CreateAsyncImpl(ExpressionToDictionary(parameters));
        }

        public Task<Apis.Objects.User> CreateAsync(IDictionary<string, object> parameters)
        {
            return CreateAsyncImpl(parameters);
        }

        private async Task<Apis.Objects.User> CreateAsyncImpl(IDictionary<string, object> parameters)
        {
            switch (Tokens.Platform)
            {
                case Tokens.PlatformEnum.Twitter:
                    return new Apis.Objects.User(await Tokens.TwitterTokens.Blocks.CreateAsync(parameters));
                case Tokens.PlatformEnum.Mastodon:
                    return new Apis.Objects.User(
                        await Tokens.MastodonTokens.Accounts.BlockAsync(Utils.ConvertToMastodonParameters(parameters)));
            }
            throw new NotImplementedException();
        }

        public Task<Apis.Objects.User> DestroyAsync(params Expression<Func<string, object>>[] parameters)
        {
            return DestroyAsyncImpl(ExpressionToDictionary(parameters));
        }

        public Task<Apis.Objects.User> DestroyAsync(IDictionary<string, object> parameters)
        {
            return DestroyAsyncImpl(parameters);
        }

        private async Task<Apis.Objects.User> DestroyAsyncImpl(IDictionary<string, object> parameters)
        {
            switch (Tokens.Platform)
            {
                case Tokens.PlatformEnum.Twitter:
                    return new Apis.Objects.User(await Tokens.TwitterTokens.Blocks.DestroyAsync(parameters));
                case Tokens.PlatformEnum.Mastodon:
                    return new Apis.Objects.User(
                        await Tokens.MastodonTokens.Accounts.UnblockAsync(
                            Utils.ConvertToMastodonParameters(parameters)));
            }
            throw new NotImplementedException();
        }

        public Task<CursoredList<long>> IdsAsync(params Expression<Func<string, object>>[] parameters)
        {
            return IdsAsyncImpl(ExpressionToDictionary(parameters));
        }

        public Task<CursoredList<long>> IdsAsync(IDictionary<string, object> parameters)
        {
            return IdsAsyncImpl(parameters);
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
                    var data = await Tokens.MastodonTokens.Blocks.GetAsync(
                        Utils.ConvertToMastodonParameters(parameters));
                    var result = new CursoredList<long>(data.Select(x => x.Id));
                    result.NextCursor = data.MaxId ?? 0;
                    result.PreviousCursor = data.SinceId ?? 0;
                    return result;
            }
            throw new NotImplementedException();
        }

        public Task<CursoredList<Apis.Objects.User>> ListAsync(params Expression<Func<string, object>>[] parameters)
        {
            return ListAsyncImpl(ExpressionToDictionary(parameters));
        }

        public Task<CursoredList<Apis.Objects.User>> ListAsync(IDictionary<string, object> parameters)
        {
            return ListAsyncImpl(parameters);
        }

        private async Task<CursoredList<Apis.Objects.User>> ListAsyncImpl(IDictionary<string, object> parameters)
        {
            switch (Tokens.Platform)
            {
                case Tokens.PlatformEnum.Twitter:
                    var response = await Tokens.TwitterTokens.Blocks.ListAsync(parameters);
                    var list =
                        new CursoredList<Apis.Objects.User>(response.Select(x => new Apis.Objects.User(x)));
                    list.NextCursor = response.NextCursor;
                    list.PreviousCursor = response.PreviousCursor;
                    return list;
                case Tokens.PlatformEnum.Mastodon:
                    var data = await Tokens.MastodonTokens.Blocks.GetAsync(
                        Utils.ConvertToMastodonParameters(parameters));
                    var result = new CursoredList<Apis.Objects.User>(data.Select(x => new Apis.Objects.User(x)));
                    result.NextCursor = data.MaxId ?? 0;
                    result.PreviousCursor = data.SinceId ?? 0;
                    return result;
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
            return UploadAsyncImpl(ExpressionToDictionary(parameters));
        }

        public Task<long> UploadAsync(IDictionary<string, object> parameters)
        {
            return UploadAsyncImpl(parameters);
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
                    return (await Tokens.MastodonTokens.Media.PostAsync(file => media)).Id;
            }
            throw new NotImplementedException();
        }

        public Task<long> UploadChunkedAsync(params Expression<Func<string, object>>[] parameters)
        {
            return UploadChunkedAsyncImpl(ExpressionToDictionary(parameters));
        }

        public Task<long> UploadChunkedAsync(IDictionary<string, object> parameters)
        {
            return UploadChunkedAsyncImpl(parameters);
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
        public Task<List<Apis.Objects.EventMessage>> AboutMeAsync(
            params Expression<Func<string, object>>[] parameters)
        {
            return AboutMeAsyncImpl(ExpressionToDictionary(parameters));
        }

        public Task<List<Apis.Objects.EventMessage>> AboutMeAsync(IDictionary<string, object> parameters)
        {
            return AboutMeAsyncImpl(parameters);
        }

        private async Task<List<Apis.Objects.EventMessage>> AboutMeAsyncImpl(IDictionary<string, object> parameters)
        {
            switch (Tokens.Platform)
            {
                case Tokens.PlatformEnum.Twitter:
                    throw new NotImplementedException();

                case Tokens.PlatformEnum.Mastodon:
                    var result =
                        (await Tokens.MastodonTokens.Notifications.GetAsync(
                            Utils.ConvertToMastodonParameters(parameters)))
                        .Select(x => new Apis.Objects.EventMessage(x))
                        .ToList();
                    return result;
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
                Public = 2,
                List = 3
            }

            public class StreamingObservable : IObservable<Apis.Objects.StreamingMessage>
            {
                public StreamingObservable(Tokens tokens, StreamingType type,
                    IDictionary<string, object> parameters = null)
                {
                    _tokens = tokens;
                    _type = type;
                    _parameters = parameters;
                }

                private readonly Tokens _tokens;
                private readonly StreamingType _type;
                private readonly IDictionary<string, object> _parameters;

                public IDisposable Subscribe(IObserver<Apis.Objects.StreamingMessage> observer)
                {
                    var streamingUrl = _tokens.Instance;
                    var conn = new StreamingConnection();
                    switch (_type)
                    {
                        case StreamingType.User:
                            conn.Start(observer, _tokens, "https://" + streamingUrl + "/api/v1/streaming/user");
                            break;
                        case StreamingType.Tag:
                            if (!_parameters.ContainsKey("tag") || string.IsNullOrEmpty(_parameters["tag"].ToString()))
                                throw new ArgumentException("You must specify a hashtag");

                            conn.Start(observer, _tokens,
                                "https://" + streamingUrl + "/api/v1/streaming/hashtag" + "?tag=" +
                                _parameters["tag"]);
                            break;
                        case StreamingType.Public:
                            var publicStreamingUrl = "https://" + streamingUrl + "/api/v1/streaming/public";
                            if (_parameters.ContainsKey("local") && (bool)_parameters["local"])
                                publicStreamingUrl += "/local";

                            conn.Start(observer, _tokens, publicStreamingUrl);
                            break;
                        case StreamingType.List:
                            if (!_parameters.ContainsKey("list") || string.IsNullOrEmpty(_parameters["list"].ToString()))
                                throw new ArgumentException("You must specify a list_id");

                            conn.Start(observer, _tokens, "https://" + streamingUrl + "/api/v1/streaming/list" + "?list=" + _parameters["list"]);
                            break;
                    }
                    return conn;
                }
            }

            public class StreamingConnection : IDisposable
            {
                private readonly CancellationTokenSource _cancel = new CancellationTokenSource();

                public async void Start(IObserver<Apis.Objects.StreamingMessage> observer, Tokens tokens, string url)
                {
                    var token = _cancel.Token;
                    try
                    {
                        using (var client = tokens.MastodonTokens.ConnectionOptions.GetHttpClient(tokens.AccessToken, false))
                        using (token.Register(client.Dispose))
                        {
                            using (var stream = await client.GetStreamAsync(url))
                            using (token.Register(stream.Dispose))
                            using (var reader = new StreamReader(stream))
                            using (token.Register(reader.Dispose))
                            {
                                string eventName = null;
                                while (!reader.EndOfStream)
                                {
                                    var line = await reader.ReadLineAsync();
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
                                                    JsonConvert.DeserializeObject<TootNet.Objects.Status>(data);
                                                if (status.Visibility == "direct")
                                                {
                                                    var dm =
                                                        new Apis.Objects.DirectMessage(status,
                                                            (await tokens.MastodonTokens.Statuses.IdAsync(id =>
                                                                status.InReplyToId.Value)).Account);
                                                    observer.OnNext(new Apis.Objects.StreamingMessage(dm));
                                                }
                                                else
                                                {
                                                    var s = new Apis.Objects.Status(status);
                                                    observer.OnNext(new Apis.Objects.StreamingMessage(s));
                                                }
                                                break;
                                            case "notification":
                                                var notification =
                                                    JsonConvert.DeserializeObject<TootNet.Objects.Notification>(data);
                                                observer.OnNext(new Apis.Objects.StreamingMessage(
                                                    new Apis.Objects.EventMessage(notification)));
                                                break;
                                            case "delete":
                                                var statusId = long.Parse(data);
                                                observer.OnNext(new Apis.Objects.StreamingMessage(statusId));
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
                    try
                    {
                        _cancel.Cancel();
                    }
                    catch
                    {
                    }
                }
            }
        }

        public static class TwitterStreaming
        {
            public enum StreamingType
            {
                User = 0,
                Site = 1,
                Filter = 2,
                Sample = 3,
                Firehose = 4
            }

            public class StreamingObservable : IObservable<Apis.Objects.StreamingMessage>
            {
                public StreamingObservable(Tokens tokens, StreamingType type,
                    IDictionary<string, object> parameters = null)
                {
                    _tokens = tokens;
                    _type = type;
                    _parameters = parameters;
                }

                private readonly Tokens _tokens;
                private readonly StreamingType _type;
                private readonly IDictionary<string, object> _parameters;

                public IDisposable Subscribe(IObserver<Apis.Objects.StreamingMessage> observer)
                {
                    var conn = new StreamingConnection();
                    conn.Start(observer, _tokens, _type, _parameters);
                    return conn;
                }
            }

            public class StreamingConnection : IDisposable
            {
                private readonly CancellationTokenSource _cancel = new CancellationTokenSource();

                internal static string GetUrl(CoreTweet.ConnectionOptions options, string baseUrl, bool needsVersion,
                    string rest)
                {
                    var result = new StringBuilder(baseUrl.TrimEnd('/'));
                    if (needsVersion)
                    {
                        result.Append('/');
                        result.Append(options.ApiVersion);
                    }
                    result.Append('/');
                    result.Append(rest);
                    return result.ToString();
                }

                internal string GetUrl(StreamingType type, CoreTweet.Tokens tokens)
                {
                    var options = tokens.ConnectionOptions;
                    string baseUrl;
                    string apiName;
                    switch (type)
                    {
                        case StreamingType.User:
                            baseUrl = options.UserStreamUrl;
                            apiName = "user.json";
                            break;
                        case StreamingType.Site:
                            baseUrl = options.SiteStreamUrl;
                            apiName = "site.json";
                            break;
                        case StreamingType.Filter:
                            baseUrl = options.StreamUrl;
                            apiName = "statuses/filter.json";
                            break;
                        case StreamingType.Sample:
                            baseUrl = options.StreamUrl;
                            apiName = "statuses/sample.json";
                            break;
                        case StreamingType.Firehose:
                            baseUrl = options.StreamUrl;
                            apiName = "statuses/firehose.json";
                            break;
                        default:
                            throw new ArgumentException("Invalid StreamingType.");
                    }
                    return GetUrl(options, baseUrl, true, apiName);
                }


                public async void Start(IObserver<Apis.Objects.StreamingMessage> observer, Tokens tokens,
                    StreamingType type, IDictionary<string, object> parameters)
                {
                    var token = _cancel.Token;
                    var methodType = type == StreamingType.Filter
                        ? CoreTweet.MethodType.Post
                        : CoreTweet.MethodType.Get;

                    try
                    {
                        using (var res =
                            await tokens.TwitterTokens.SendStreamingRequestAsync(methodType,
                                GetUrl(type, tokens.TwitterTokens), parameters, token))
                        using (token.Register(res.Dispose))
                        {
                            using (var stream = await res.GetResponseStreamAsync())
                            using (token.Register(stream.Dispose))
                            {
                                using (var reader = new StreamReader(stream))
                                using (token.Register(reader.Dispose))
                                {
                                    while (!reader.EndOfStream)
                                    {
                                        var s = await reader.ReadLineAsync();
                                        if (string.IsNullOrWhiteSpace(s))
                                            continue;

                                        try
                                        {
                                            observer.OnNext(
                                                new Apis.Objects.StreamingMessage(CoreTweet.Streaming
                                                    .StreamingMessage.Parse(s)));
                                        }
                                        catch
                                        {
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
                    try
                    {
                        _cancel.Cancel();
                    }
                    catch
                    {
                    }
                }
            }
        }

        public IObservable<Apis.Objects.StreamingMessage> FilterAsObservable(
            params Expression<Func<string, object>>[] parameters)
        {
            return FilterAsObservableImpl(ExpressionToDictionary(parameters));
        }

        public IObservable<Apis.Objects.StreamingMessage> FilterAsObservable(IDictionary<string, object> parameters)
        {
            return FilterAsObservableImpl(parameters);
        }

        private IObservable<Apis.Objects.StreamingMessage> FilterAsObservableImpl(
            IDictionary<string, object> parameters)
        {
            switch (Tokens.Platform)
            {
                case Tokens.PlatformEnum.Twitter:
                    return new TwitterStreaming.StreamingObservable(Tokens, TwitterStreaming.StreamingType.Filter,
                        parameters);
                case Tokens.PlatformEnum.Mastodon:
                    parameters["tag"] = parameters["track"];
                    parameters.Remove("track");
                    return new MastodonStreaming.StreamingObservable(Tokens, MastodonStreaming.StreamingType.Tag,
                        parameters);
            }
            throw new NotImplementedException();
        }

        public IObservable<Apis.Objects.StreamingMessage> SampleAsObservable(
            params Expression<Func<string, object>>[] parameters)
        {
            return SampleAsObservableImpl(ExpressionToDictionary(parameters));
        }

        public IObservable<Apis.Objects.StreamingMessage> SampleAsObservable(IDictionary<string, object> parameters)
        {
            return SampleAsObservableImpl(parameters);
        }

        private IObservable<Apis.Objects.StreamingMessage> SampleAsObservableImpl(
            IDictionary<string, object> parameters)
        {
            switch (Tokens.Platform)
            {
                case Tokens.PlatformEnum.Twitter:
                    return new TwitterStreaming.StreamingObservable(Tokens, TwitterStreaming.StreamingType.Sample,
                        parameters);
                case Tokens.PlatformEnum.Mastodon:
                    throw new NotImplementedException();
            }
            throw new NotImplementedException();
        }

        public IObservable<Apis.Objects.StreamingMessage> UserAsObservable(
            params Expression<Func<string, object>>[] parameters)
        {
            return UserAsObservableImpl(ExpressionToDictionary(parameters));
        }

        public IObservable<Apis.Objects.StreamingMessage> UserAsObservable(IDictionary<string, object> parameters)
        {
            return UserAsObservableImpl(parameters);
        }

        private IObservable<Apis.Objects.StreamingMessage> UserAsObservableImpl(
            IDictionary<string, object> parameters)
        {
            switch (Tokens.Platform)
            {
                case Tokens.PlatformEnum.Twitter:
                    return new TwitterStreaming.StreamingObservable(Tokens, TwitterStreaming.StreamingType.User,
                        parameters);
                case Tokens.PlatformEnum.Mastodon:
                    return new MastodonStreaming.StreamingObservable(Tokens, MastodonStreaming.StreamingType.User,
                        parameters);
            }
            throw new NotImplementedException();
        }

        public IObservable<Apis.Objects.StreamingMessage> PublicAsObservable(
            params Expression<Func<string, object>>[] parameters)
        {
            return PublicAsObservableImpl(ExpressionToDictionary(parameters));
        }

        public IObservable<Apis.Objects.StreamingMessage> PublicAsObservable(IDictionary<string, object> parameters)
        {
            return PublicAsObservableImpl(parameters);
        }

        private IObservable<Apis.Objects.StreamingMessage> PublicAsObservableImpl(
            IDictionary<string, object> parameters)
        {
            switch (Tokens.Platform)
            {
                case Tokens.PlatformEnum.Twitter:
                    throw new NotImplementedException();
                case Tokens.PlatformEnum.Mastodon:
                    return new MastodonStreaming.StreamingObservable(Tokens, MastodonStreaming.StreamingType.Public,
                        parameters);
            }
            throw new NotImplementedException();
        }

        public IObservable<Apis.Objects.StreamingMessage> ListAsObservable(
            params Expression<Func<string, object>>[] parameters)
        {
            return ListAsObservableImpl(ExpressionToDictionary(parameters));
        }

        public IObservable<Apis.Objects.StreamingMessage> ListAsObservable(IDictionary<string, object> parameters)
        {
            return ListAsObservableImpl(parameters);
        }

        private IObservable<Apis.Objects.StreamingMessage> ListAsObservableImpl(
            IDictionary<string, object> parameters)
        {
            switch (Tokens.Platform)
            {
                case Tokens.PlatformEnum.Twitter:
                    throw new NotImplementedException();
                case Tokens.PlatformEnum.Mastodon:
                    return new MastodonStreaming.StreamingObservable(Tokens, MastodonStreaming.StreamingType.List,
                        parameters);
            }
            throw new NotImplementedException();
        }
    }

    public class Tokens
    {
        public Tokens()
        {
            Activity = new Activity {Tokens = this};
            Favorites = new Favorites {Tokens = this};
            Users = new Users {Tokens = this};
            Trends = new Trends {Tokens = this};
            Statuses = new Statuses {Tokens = this};
            SavedSearches = new SavedSearches {Tokens = this};
            Search = new Search {Tokens = this};
            Mutes = new Mutes
            {
                Tokens = this,
                Users = new MutesUsers {Tokens = this}
            };
            Lists = new Lists
            {
                Tokens = this,
                Members = new ListsMembers {Tokens = this}
            };
            Lists.Members = new ListsMembers {Tokens = this};
            Friendships = new Friendships {Tokens = this};
            Followers = new Followers {Tokens = this};
            Friends = new Friends {Tokens = this};
            DirectMessages = new DirectMessages {Tokens = this};
            Collections = new Collections {Tokens = this};
            Blocks = new Blocks {Tokens = this};
            Media = new Media {Tokens = this};
            Streaming = new StreamingApi {Tokens = this};
        }

        public enum PlatformEnum
        {
            Twitter = 0,
            Mastodon = 1
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
                                                     CoreTweet.Tokens.Create(ConsumerKey, ConsumerSecret,
                                                         AccessToken, AccessTokenSecret));

        private TootNet.Tokens _mastodonTokens;

        public TootNet.Tokens MastodonTokens => _mastodonTokens ??
                                                (_mastodonTokens =
                                                    new TootNet.Tokens(Instance, AccessToken));

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
            var tokens = new Tokens
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
            return tokens;
        }
    }
}