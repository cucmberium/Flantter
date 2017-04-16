using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Flantter.MilkyWay.Common;
using UrlEntity = Flantter.MilkyWay.Models.Twitter.Objects.UrlEntity;

namespace Flantter.MilkyWay.Models.Twitter
{
    public static class MediaParser
    {
        private static readonly Regex RegexTwitpic = new Regex(@"^https?://(www\.)?twitpic\.com/(?<Id>\w+)(/full/?)?$",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex RegexYfrog = new Regex(@"^https?://yfrog\.com/(\w+)$",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex RegexImgur = new Regex(@"^https?://(?:i\.)?imgur\.com/(\w+)(?:\..{3})?$",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex RegexTwipplePhoto = new Regex(@"^https?://p\.twipple\.jp/(?<ContentId>[0-9a-z]+)",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex RegexImgly = new Regex(@"^https?://img\.ly/(\w+)$",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex RegexNicoImage =
            new Regex(@"^https?://(?:seiga\.nicovideo\.jp/seiga/|nico\.ms/)im(?<Id>\d+)",
                RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex RegexPixiv =
            new Regex(
                @"^https?://www\.pixiv\.net/(member_illust|index)\.php\?(?=.*mode=(medium|big))(?=.*illust_id=(?<Id>[0-9]+)).*$",
                RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex RegexInstagram =
            new Regex(@"^https?://(?:www\.)?instagr(?:\.am|am\.com)/p/([\w\-]+)(?:/(?:media/?)?)?(?:\?.*)?$",
                RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex RegexGyazo =
            new Regex(@"^https?://(?:www\.)?gyazo\.com/(\w+)(?:\.png)?(?:\?.*)?$",
                RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex RegexPckles = new Regex(@"^https?://pckles\.com/\w+/\w+$",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex RegexMobyPicture = new Regex(@"^https?://moby\.to/(\w+)$",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex RegexHatenaPhotoLife =
            new Regex(@"^https?://f\.hatena\.ne\.jp/(([a-z])[a-z0-9_-]{1,30}[a-z0-9])/((\d{8})\d+)$",
                RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex RegexKeitaiHyakkei = new Regex(@"^https?://movapic\.com/pic/(\w+)$",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex RegexTwitgoo = new Regex(@"^https?://twitgoo\.com/(\w+)$",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex RegexPhotoZou =
            new Regex(@"^https?://photozou\.jp/photo/show/(?<UserId>[0-9]+)/(?<Id>[0-9]+)",
                RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex RegexFlickr =
            new Regex(
                @"^https?://(?:www\.)?(?:flickr\.com/photos/(?:[\w\-_@]+)/(\d+)(?:/in/[\w\-]*)?|flic\.kr/p/(\w+))/?(?:\?.*)?$",
                RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex RegexDropbox =
            new Regex(
                @"^https?://(?:(?:www\.|dl\.)?dropbox\.com/s/(\w+)/([\w\-\.%]+\.(?:jpeg?|jpg|png|gif|bmp|dib|tiff?))|(?:www\.)?db\.tt/(\w+))/?(?:\?.*)?$",
                RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex RegexNicoVideo =
            new Regex(
                @"^https?://(?:(?:www\.)?nicovideo\.jp/watch|nico\.(?:ms|sc))/(?<VideoId>(?:sm|nm)?\d+)?(?:\?.*)?$",
                RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex RegexYoutube =
            new Regex(@"^https?://(?:(www\.youtube\.com)|(youtu\.be))/(watch\?v=)?(?<VideoId>([\w\-]+))",
                RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex RegexVine = new Regex(@"^https?://(?:www\.)?vine\.co/v/(?<VideoId>\w+)(?:\?.*)?$",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex RegexDirectLink = new Regex(@"^https?://.*(\.jpg|\.jpeg|\.gif|\.png|\.bmp)$",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public static IEnumerable<Media> Parse(CoreTweet.Entities cEntities, CoreTweet.Entities cExtendedEntities = null)
        {
            if (cEntities?.Urls != null)
                foreach (var url in cEntities.Urls)
                {
                    if (url.ExpandedUrl == null)
                        continue;

                    // 画像サービス

                    #region DirectLink

                    var match = RegexDirectLink.Match(url.ExpandedUrl);
                    if (match.Success)
                    {
                        yield return new Media
                        {
                            MediaThumbnailUrl = match.Value,
                            MediaUrl = match.Value,
                            ExpandedUrl = match.Value,
                            DisplayUrl = url.DisplayUrl,
                            Type = "Image"
                        };
                        continue;
                    }

                    #endregion

                    #region yfrog

                    match = RegexYfrog.Match(url.ExpandedUrl);
                    if (match.Success)
                    {
                        yield return new Media
                        {
                            MediaThumbnailUrl = match.Value + ":small",
                            MediaUrl = match.Value + ":medium",
                            ExpandedUrl = match.Value,
                            DisplayUrl = url.DisplayUrl,
                            Type = "Image"
                        };
                        continue;
                    }

                    #endregion

                    #region imgur

                    match = RegexImgur.Match(url.ExpandedUrl);
                    if (match.Success)
                    {
                        yield return new Media
                        {
                            MediaThumbnailUrl = "http://img.imgur.com/" + match.Groups[1] + "l.jpg",
                            MediaUrl = "http://img.imgur.com/" + match.Groups[1] + ".jpg",
                            ExpandedUrl = match.Value,
                            DisplayUrl = url.DisplayUrl,
                            Type = "Image"
                        };
                        continue;
                    }

                    #endregion

                    #region ついっぷるフォト

                    match = RegexTwipplePhoto.Match(url.ExpandedUrl);
                    if (match.Success)
                    {
                        yield return new Media
                        {
                            MediaThumbnailUrl = "http://p.twipple.jp/show/thumb/" + match.Groups["ContentId"],
                            MediaUrl = "http://p.twpl.jp/show/orig/" + match.Groups["ContentId"],
                            ExpandedUrl = match.Value,
                            DisplayUrl = url.DisplayUrl,
                            Type = "Image"
                        };
                        continue;
                    }

                    #endregion

                    #region img.ly

                    match = RegexImgly.Match(url.ExpandedUrl);
                    if (match.Success)
                    {
                        yield return new Media
                        {
                            MediaThumbnailUrl = "http://img.ly/show/thumb/" + match.Groups[1],
                            MediaUrl = "http://img.ly/show/full/" + match.Groups[1],
                            ExpandedUrl = match.Value,
                            DisplayUrl = url.DisplayUrl,
                            Type = "Image"
                        };
                        continue;
                    }

                    #endregion

                    #region ニコニコ静画

                    match = RegexNicoImage.Match(url.ExpandedUrl);
                    if (match.Success)
                    {
                        yield return new Media
                        {
                            MediaThumbnailUrl = "http://lohas.nicoseiga.jp/thumb/" + match.Groups["Id"] + "q?",
                            MediaUrl = "http://lohas.nicoseiga.jp/thumb/" + match.Groups["Id"] + "l?",
                            ExpandedUrl = match.Value,
                            DisplayUrl = url.DisplayUrl,
                            Type = "Image"
                        };
                        continue;
                    }

                    #endregion

                    #region Pixiv

                    match = RegexPixiv.Match(url.ExpandedUrl);
                    if (match.Success)
                    {
                        /*var fileName = "Pixiv_" + match.Groups["Id"];
                        try
                        {
                            Thumbnail.Pixiv.GetThumbnail(match.Groups["Id"].Value, fileName);
                        }
                        catch
                        {
                        }*/

                        yield return new Media
                        {
                            //MediaThumbnailUrl = "http://img.azyobuzi.net/api/redirect?size=large&uri=" + "http://www.pixiv.net/member_illust.php?illust_id=" + match.Groups["Id"],
                            //MediaUrl = "http://img.azyobuzi.net/api/redirect?size=large&uri=" + "http://www.pixiv.net/member_illust.php?illust_id=" + match.Groups["Id"],MediaThumbnailUrl = "http://img.azyobuzi.net/api/redirect?size=large&uri=" + "http://www.pixiv.net/member_illust.php?illust_id=" + match.Groups["Id"],
                            MediaThumbnailUrl = "http://embed.pixiv.net/decorate.php?illust_id=" + match.Groups["Id"],
                            MediaUrl = "http://embed.pixiv.net/decorate.php?illust_id=" + match.Groups["Id"],
                            ExpandedUrl = match.Value,
                            DisplayUrl = url.DisplayUrl,
                            Type = "Image"
                        };
                        continue;
                    }

                    #endregion

                    #region Instagram

                    match = RegexInstagram.Match(url.ExpandedUrl);
                    if (match.Success)
                    {
                        yield return new Media
                        {
                            MediaThumbnailUrl = "http://instagr.am/p/" + match.Groups[1] + "/media/?size=t",
                            MediaUrl = "http://instagr.am/p/" + match.Groups[1] + "/media/?size=l",
                            ExpandedUrl = match.Value,
                            DisplayUrl = url.DisplayUrl,
                            Type = "Image"
                        };
                        continue;
                    }

                    #endregion

                    #region Gyazo

                    match = RegexGyazo.Match(url.ExpandedUrl);
                    if (match.Success)
                    {
                        yield return new Media
                        {
                            MediaThumbnailUrl = "http://gyazo.com/" + match.Groups[1] + ".png",
                            MediaUrl = "http://gyazo.com/" + match.Groups[1] + ".png",
                            ExpandedUrl = match.Value,
                            DisplayUrl = url.DisplayUrl,
                            Type = "Image"
                        };
                        continue;
                    }

                    #endregion

                    #region Pckles

                    match = RegexPckles.Match(url.ExpandedUrl);
                    if (match.Success)
                    {
                        yield return new Media
                        {
                            MediaThumbnailUrl = match.Value + ".resize.jpg",
                            MediaUrl = match.Value + ".jpg",
                            ExpandedUrl = match.Value,
                            DisplayUrl = url.DisplayUrl,
                            Type = "Image"
                        };
                        continue;
                    }

                    #endregion

                    #region フォト蔵

                    match = RegexPhotoZou.Match(url.ExpandedUrl);
                    if (match.Success)
                    {
                        yield return new Media
                        {
                            MediaThumbnailUrl = "http://photozou.jp/p/thumb/" + match.Groups["Id"],
                            MediaUrl = "http://photozou.jp/p/img/" + match.Groups["Id"],
                            ExpandedUrl = match.Value,
                            DisplayUrl = url.DisplayUrl,
                            Type = "Image"
                        };
                        continue;
                    }

                    #endregion

                    #region flickr (外部サービス使用)

                    match = RegexFlickr.Match(url.ExpandedUrl);
                    if (match.Success)
                    {
                        yield return new Media
                        {
                            MediaThumbnailUrl = "http://img.azyobuzi.net/api/redirect?size=thumb&uri=" + match.Value,
                            MediaUrl = "http://img.azyobuzi.net/api/redirect?size=full&uri=" + match.Value,
                            ExpandedUrl = match.Value,
                            DisplayUrl = url.DisplayUrl,
                            Type = "Image"
                        };
                        continue;
                    }

                    #endregion

                    #region MobyPicture

                    match = RegexMobyPicture.Match(url.ExpandedUrl);
                    if (match.Success)
                    {
                        yield return new Media
                        {
                            MediaThumbnailUrl = "http://moby.to/" + match.Groups[1] + ":thumb",
                            MediaUrl = "http://moby.to/" + match.Groups[1] + ":full",
                            ExpandedUrl = match.Value,
                            DisplayUrl = url.DisplayUrl,
                            Type = "Image"
                        };
                        continue;
                    }

                    #endregion

                    #region はてなフォトライフ

                    match = RegexHatenaPhotoLife.Match(url.ExpandedUrl);
                    if (match.Success)
                    {
                        yield return new Media
                        {
                            MediaThumbnailUrl = "http://img.f.hatena.ne.jp/images/fotolife/" + match.Groups[2] + "/" +
                                                match.Groups[1] + "/" + match.Groups[4] + "/" + match.Groups[3] +
                                                "_120.jpg",
                            MediaUrl = "http://img.f.hatena.ne.jp/images/fotolife/" + match.Groups[2] + "/" +
                                       match.Groups[1] + "/" + match.Groups[4] + "/" + match.Groups[3] +
                                       "_original.jpg",
                            ExpandedUrl = match.Value,
                            DisplayUrl = url.DisplayUrl,
                            Type = "Image"
                        };
                        continue;
                    }

                    #endregion

                    #region 携帯百景

                    match = RegexKeitaiHyakkei.Match(url.ExpandedUrl);
                    if (match.Success)
                    {
                        yield return new Media
                        {
                            MediaThumbnailUrl = "http://image.movapic.com/pic/s_" + match.Groups[1] + ".jpeg",
                            MediaUrl = "http://image.movapic.com/pic/m_" + match.Groups[1] + ".jpeg",
                            ExpandedUrl = match.Value,
                            DisplayUrl = url.DisplayUrl,
                            Type = "Image"
                        };
                        continue;
                    }

                    #endregion

                    #region Twitgoo

                    match = RegexTwitgoo.Match(url.ExpandedUrl);
                    if (match.Success)
                    {
                        yield return new Media
                        {
                            MediaThumbnailUrl = "http://twitgoo.com/" + match.Groups[1] + "/mini",
                            MediaUrl = "http://twitgoo.com/" + match.Groups[1] + "/img",
                            ExpandedUrl = match.Value,
                            DisplayUrl = url.DisplayUrl,
                            Type = "Image"
                        };
                        continue;
                    }

                    #endregion

                    #region Dropbox (外部サービス使用)

                    match = RegexDropbox.Match(url.ExpandedUrl);
                    if (match.Success)
                    {
                        yield return new Media
                        {
                            MediaThumbnailUrl = "http://img.azyobuzi.net/api/redirect?size=thumb&uri=" + match.Value,
                            MediaUrl = "http://img.azyobuzi.net/api/redirect?size=full&uri=" + match.Value,
                            ExpandedUrl = match.Value,
                            DisplayUrl = url.DisplayUrl,
                            Type = "Image"
                        };
                        continue;
                    }

                    #endregion

                    #region Twitpic

                    match = RegexTwitpic.Match(url.ExpandedUrl);
                    if (match.Success)
                    {
                        yield return new Media
                        {
                            MediaThumbnailUrl = "http://twitpic.com/show/thumb/" + match.Groups["Id"],
                            MediaUrl = "http://twitpic.com/show/full/" + match.Groups["Id"],
                            ExpandedUrl = match.Value,
                            DisplayUrl = url.DisplayUrl,
                            Type = "Image"
                        };
                        continue;
                    }

                    #endregion

                    // 動画サービス

                    #region ニコニコ動画

                    match = RegexNicoVideo.Match(url.ExpandedUrl);
                    if (match.Success)
                        if (match.Groups["VideoId"].Value.StartsWith("sm") ||
                            match.Groups["VideoId"].Value.StartsWith("nm"))
                        {
                            yield return new Media
                            {
                                MediaThumbnailUrl = "http://tn-skr2.smilevideo.jp/smile?i=" +
                                                    match.Groups["VideoId"].Value.Replace("sm", "").Replace("nm", ""),
                                MediaUrl = string.Empty,
                                ExpandedUrl = match.Value,
                                DisplayUrl = url.DisplayUrl,
                                Type = "Video",
                                VideoInfo = new VideoInfo
                                {
                                    VideoId = match.Groups["VideoId"].Value,
                                    VideoType = "NicoVideo"
                                }
                            };
                            continue;
                        }
                        else
                        {
                            /*var fileName = "NicoVideo_" + match.Groups["VideoId"].Value;
                            try
                            {
                                Thumbnail.NicoVideo.GetThumbnail(match.Groups["VideoId"].Value, fileName);
                            }
                            catch
                            {
                            }*/

                            yield return new Media
                            {
                                // MediaThumbnailUrl = "ms-appdata:///temp/" + fileName,
                                MediaThumbnailUrl = "http://localhost/",
                                MediaUrl = string.Empty,
                                ExpandedUrl = match.Value,
                                DisplayUrl = url.DisplayUrl,
                                Type = "Video",
                                VideoInfo = new VideoInfo
                                {
                                    VideoId = match.Groups["VideoId"].Value,
                                    VideoType = "NicoVideo"
                                }
                            };
                            continue;
                        }

                    #endregion

                    #region Youtube

                    match = RegexYoutube.Match(url.ExpandedUrl);
                    if (match.Success)
                    {
                        yield return new Media
                        {
                            MediaThumbnailUrl = "http://img.youtube.com/vi/" + match.Groups["VideoId"] + "/default.jpg",
                            MediaUrl = string.Empty,
                            ExpandedUrl = match.Value,
                            DisplayUrl = url.DisplayUrl,
                            Type = "Video",
                            VideoInfo = new VideoInfo {VideoId = match.Groups["VideoId"].Value, VideoType = "Youtube"}
                        };
                        continue;
                    }

                    #endregion

                    #region Vine

                    match = RegexVine.Match(url.ExpandedUrl);
                    if (match.Success)
                        yield return new Media
                        {
                            MediaThumbnailUrl = "http://img.azyobuzi.net/api/redirect?size=thumb&uri=" + match.Value,
                            MediaUrl = string.Empty,
                            ExpandedUrl = match.Value,
                            DisplayUrl = url.DisplayUrl,
                            Type = "Video",
                            VideoInfo = new VideoInfo {VideoId = match.Groups["VideoId"].Value, VideoType = "Vine"}
                        };

                    #endregion
                }

            #region Twitter公式

            if (cEntities?.Media != null && cExtendedEntities == null)
                foreach (var media in cEntities.Media)
                    yield return new Media
                    {
                        MediaThumbnailUrl = media.MediaUrl + ":thumb",
                        MediaUrl = media.MediaUrl + ":orig",
                        ExpandedUrl = media.Url,
                        DisplayUrl = media.DisplayUrl,
                        Type = "Image"
                    };

            if (cExtendedEntities != null && cExtendedEntities.Media != null)
                foreach (var media in cExtendedEntities.Media)
                    if (media.Type == "animated_gif" || media.Type == "video")
                    {
                        CoreTweet.VideoVariant variant;

                        var variants = media.VideoInfo.Variants.Where(x => x.ContentType == "video/mp4");
                        if (!variants.Any())
                            variant = media.VideoInfo.Variants.First();
                        else
                            variant = variants.FindMax(x => x.Bitrate ?? 0);

                        yield return new Media
                        {
                            MediaThumbnailUrl = media.MediaUrl + ":thumb",
                            MediaUrl = media.MediaUrl + ":orig",
                            ExpandedUrl = media.Url,
                            DisplayUrl = media.DisplayUrl,
                            Type = "Video",
                            VideoInfo = new VideoInfo
                            {
                                VideoId = variant.Url,
                                VideoType = "Twitter",
                                Size =
                                    new VideoInfo.MediaSize
                                    {
                                        Width = media.Sizes.Large.Width,
                                        Height = media.Sizes.Large.Height
                                    },
                                VideoContentType = variant.ContentType
                            }
                        };
                    }
                    else
                    {
                        yield return new Media
                        {
                            MediaThumbnailUrl = media.MediaUrl + ":thumb",
                            MediaUrl = media.MediaUrl + ":orig",
                            ExpandedUrl = media.Url,
                            DisplayUrl = media.DisplayUrl,
                            Type = "Image"
                        };
                    }

            #endregion
        }

        public static IEnumerable<Media> Parse(IEnumerable<Mastonet.Entities.Attachment> cAttachments, string cContent)
        {
            foreach (Match urlMatch in TweetRegexPatterns.ValidUrl.Matches(cContent))
            {
                if (!urlMatch.Groups[2].Value.StartsWith("http"))
                    continue;

                var url = new UrlEntity {DisplayUrl = urlMatch.Groups[2].Value, ExpandedUrl = urlMatch.Groups[2].Value};

                #region DirectLink

                var match = RegexDirectLink.Match(url.ExpandedUrl);
                if (match.Success)
                {
                    yield return new Media
                    {
                        MediaThumbnailUrl = match.Value,
                        MediaUrl = match.Value,
                        ExpandedUrl = match.Value,
                        DisplayUrl = url.DisplayUrl,
                        Type = "Image"
                    };
                    continue;
                }

                #endregion

                #region yfrog

                match = RegexYfrog.Match(url.ExpandedUrl);
                if (match.Success)
                {
                    yield return new Media
                    {
                        MediaThumbnailUrl = match.Value + ":small",
                        MediaUrl = match.Value + ":medium",
                        ExpandedUrl = match.Value,
                        DisplayUrl = url.DisplayUrl,
                        Type = "Image"
                    };
                    continue;
                }

                #endregion

                #region imgur

                match = RegexImgur.Match(url.ExpandedUrl);
                if (match.Success)
                {
                    yield return new Media
                    {
                        MediaThumbnailUrl = "http://img.imgur.com/" + match.Groups[1] + "l.jpg",
                        MediaUrl = "http://img.imgur.com/" + match.Groups[1] + ".jpg",
                        ExpandedUrl = match.Value,
                        DisplayUrl = url.DisplayUrl,
                        Type = "Image"
                    };
                    continue;
                }

                #endregion

                #region ついっぷるフォト

                match = RegexTwipplePhoto.Match(url.ExpandedUrl);
                if (match.Success)
                {
                    yield return new Media
                    {
                        MediaThumbnailUrl = "http://p.twipple.jp/show/thumb/" + match.Groups["ContentId"],
                        MediaUrl = "http://p.twpl.jp/show/orig/" + match.Groups["ContentId"],
                        ExpandedUrl = match.Value,
                        DisplayUrl = url.DisplayUrl,
                        Type = "Image"
                    };
                    continue;
                }

                #endregion

                #region img.ly

                match = RegexImgly.Match(url.ExpandedUrl);
                if (match.Success)
                {
                    yield return new Media
                    {
                        MediaThumbnailUrl = "http://img.ly/show/thumb/" + match.Groups[1],
                        MediaUrl = "http://img.ly/show/full/" + match.Groups[1],
                        ExpandedUrl = match.Value,
                        DisplayUrl = url.DisplayUrl,
                        Type = "Image"
                    };
                    continue;
                }

                #endregion

                #region ニコニコ静画

                match = RegexNicoImage.Match(url.ExpandedUrl);
                if (match.Success)
                {
                    yield return new Media
                    {
                        MediaThumbnailUrl = "http://lohas.nicoseiga.jp/thumb/" + match.Groups["Id"] + "q?",
                        MediaUrl = "http://lohas.nicoseiga.jp/thumb/" + match.Groups["Id"] + "l?",
                        ExpandedUrl = match.Value,
                        DisplayUrl = url.DisplayUrl,
                        Type = "Image"
                    };
                    continue;
                }

                #endregion

                #region Pixiv

                match = RegexPixiv.Match(url.ExpandedUrl);
                if (match.Success)
                {
                    /*var fileName = "Pixiv_" + match.Groups["Id"];
                    try
                    {
                        Thumbnail.Pixiv.GetThumbnail(match.Groups["Id"].Value, fileName);
                    }
                    catch
                    {
                    }*/

                    yield return new Media
                    {
                        //MediaThumbnailUrl = "http://img.azyobuzi.net/api/redirect?size=large&uri=" + "http://www.pixiv.net/member_illust.php?illust_id=" + match.Groups["Id"],
                        //MediaUrl = "http://img.azyobuzi.net/api/redirect?size=large&uri=" + "http://www.pixiv.net/member_illust.php?illust_id=" + match.Groups["Id"],MediaThumbnailUrl = "http://img.azyobuzi.net/api/redirect?size=large&uri=" + "http://www.pixiv.net/member_illust.php?illust_id=" + match.Groups["Id"],
                        MediaThumbnailUrl = "http://embed.pixiv.net/decorate.php?illust_id=" + match.Groups["Id"],
                        MediaUrl = "http://embed.pixiv.net/decorate.php?illust_id=" + match.Groups["Id"],
                        ExpandedUrl = match.Value,
                        DisplayUrl = url.DisplayUrl,
                        Type = "Image"
                    };
                    continue;
                }

                #endregion

                #region Instagram

                match = RegexInstagram.Match(url.ExpandedUrl);
                if (match.Success)
                {
                    yield return new Media
                    {
                        MediaThumbnailUrl = "http://instagr.am/p/" + match.Groups[1] + "/media/?size=t",
                        MediaUrl = "http://instagr.am/p/" + match.Groups[1] + "/media/?size=l",
                        ExpandedUrl = match.Value,
                        DisplayUrl = url.DisplayUrl,
                        Type = "Image"
                    };
                    continue;
                }

                #endregion

                #region Gyazo

                match = RegexGyazo.Match(url.ExpandedUrl);
                if (match.Success)
                {
                    yield return new Media
                    {
                        MediaThumbnailUrl = "http://gyazo.com/" + match.Groups[1] + ".png",
                        MediaUrl = "http://gyazo.com/" + match.Groups[1] + ".png",
                        ExpandedUrl = match.Value,
                        DisplayUrl = url.DisplayUrl,
                        Type = "Image"
                    };
                    continue;
                }

                #endregion

                #region Pckles

                match = RegexPckles.Match(url.ExpandedUrl);
                if (match.Success)
                {
                    yield return new Media
                    {
                        MediaThumbnailUrl = match.Value + ".resize.jpg",
                        MediaUrl = match.Value + ".jpg",
                        ExpandedUrl = match.Value,
                        DisplayUrl = url.DisplayUrl,
                        Type = "Image"
                    };
                    continue;
                }

                #endregion

                #region フォト蔵

                match = RegexPhotoZou.Match(url.ExpandedUrl);
                if (match.Success)
                {
                    yield return new Media
                    {
                        MediaThumbnailUrl = "http://photozou.jp/p/thumb/" + match.Groups["Id"],
                        MediaUrl = "http://photozou.jp/p/img/" + match.Groups["Id"],
                        ExpandedUrl = match.Value,
                        DisplayUrl = url.DisplayUrl,
                        Type = "Image"
                    };
                    continue;
                }

                #endregion

                #region flickr (外部サービス使用)

                match = RegexFlickr.Match(url.ExpandedUrl);
                if (match.Success)
                {
                    yield return new Media
                    {
                        MediaThumbnailUrl = "http://img.azyobuzi.net/api/redirect?size=thumb&uri=" + match.Value,
                        MediaUrl = "http://img.azyobuzi.net/api/redirect?size=full&uri=" + match.Value,
                        ExpandedUrl = match.Value,
                        DisplayUrl = url.DisplayUrl,
                        Type = "Image"
                    };
                    continue;
                }

                #endregion

                #region MobyPicture

                match = RegexMobyPicture.Match(url.ExpandedUrl);
                if (match.Success)
                {
                    yield return new Media
                    {
                        MediaThumbnailUrl = "http://moby.to/" + match.Groups[1] + ":thumb",
                        MediaUrl = "http://moby.to/" + match.Groups[1] + ":full",
                        ExpandedUrl = match.Value,
                        DisplayUrl = url.DisplayUrl,
                        Type = "Image"
                    };
                    continue;
                }

                #endregion

                #region はてなフォトライフ

                match = RegexHatenaPhotoLife.Match(url.ExpandedUrl);
                if (match.Success)
                {
                    yield return new Media
                    {
                        MediaThumbnailUrl = "http://img.f.hatena.ne.jp/images/fotolife/" + match.Groups[2] + "/" +
                                            match.Groups[1] + "/" + match.Groups[4] + "/" + match.Groups[3] +
                                            "_120.jpg",
                        MediaUrl = "http://img.f.hatena.ne.jp/images/fotolife/" + match.Groups[2] + "/" +
                                   match.Groups[1] + "/" + match.Groups[4] + "/" + match.Groups[3] + "_original.jpg",
                        ExpandedUrl = match.Value,
                        DisplayUrl = url.DisplayUrl,
                        Type = "Image"
                    };
                    continue;
                }

                #endregion

                #region 携帯百景

                match = RegexKeitaiHyakkei.Match(url.ExpandedUrl);
                if (match.Success)
                {
                    yield return new Media
                    {
                        MediaThumbnailUrl = "http://image.movapic.com/pic/s_" + match.Groups[1] + ".jpeg",
                        MediaUrl = "http://image.movapic.com/pic/m_" + match.Groups[1] + ".jpeg",
                        ExpandedUrl = match.Value,
                        DisplayUrl = url.DisplayUrl,
                        Type = "Image"
                    };
                    continue;
                }

                #endregion

                #region Twitgoo

                match = RegexTwitgoo.Match(url.ExpandedUrl);
                if (match.Success)
                {
                    yield return new Media
                    {
                        MediaThumbnailUrl = "http://twitgoo.com/" + match.Groups[1] + "/mini",
                        MediaUrl = "http://twitgoo.com/" + match.Groups[1] + "/img",
                        ExpandedUrl = match.Value,
                        DisplayUrl = url.DisplayUrl,
                        Type = "Image"
                    };
                    continue;
                }

                #endregion

                #region Dropbox (外部サービス使用)

                match = RegexDropbox.Match(url.ExpandedUrl);
                if (match.Success)
                {
                    yield return new Media
                    {
                        MediaThumbnailUrl = "http://img.azyobuzi.net/api/redirect?size=thumb&uri=" + match.Value,
                        MediaUrl = "http://img.azyobuzi.net/api/redirect?size=full&uri=" + match.Value,
                        ExpandedUrl = match.Value,
                        DisplayUrl = url.DisplayUrl,
                        Type = "Image"
                    };
                    continue;
                }

                #endregion

                #region Twitpic

                match = RegexTwitpic.Match(url.ExpandedUrl);
                if (match.Success)
                {
                    yield return new Media
                    {
                        MediaThumbnailUrl = "http://twitpic.com/show/thumb/" + match.Groups["Id"],
                        MediaUrl = "http://twitpic.com/show/full/" + match.Groups["Id"],
                        ExpandedUrl = match.Value,
                        DisplayUrl = url.DisplayUrl,
                        Type = "Image"
                    };
                    continue;
                }

                #endregion

                // 動画サービス

                #region ニコニコ動画

                match = RegexNicoVideo.Match(url.ExpandedUrl);
                if (match.Success)
                    if (match.Groups["VideoId"].Value.StartsWith("sm") ||
                        match.Groups["VideoId"].Value.StartsWith("nm"))
                    {
                        yield return new Media
                        {
                            MediaThumbnailUrl = "http://tn-skr2.smilevideo.jp/smile?i=" +
                                                match.Groups["VideoId"].Value.Replace("sm", "").Replace("nm", ""),
                            MediaUrl = string.Empty,
                            ExpandedUrl = match.Value,
                            DisplayUrl = url.DisplayUrl,
                            Type = "Video",
                            VideoInfo = new VideoInfo {VideoId = match.Groups["VideoId"].Value, VideoType = "NicoVideo"}
                        };
                        continue;
                    }
                    else
                    {
                        /*var fileName = "NicoVideo_" + match.Groups["VideoId"].Value;
                        try
                        {
                            Thumbnail.NicoVideo.GetThumbnail(match.Groups["VideoId"].Value, fileName);
                        }
                        catch
                        {
                        }*/

                        yield return new Media
                        {
                            // MediaThumbnailUrl = "ms-appdata:///temp/" + fileName,
                            MediaThumbnailUrl = "http://localhost/",
                            MediaUrl = string.Empty,
                            ExpandedUrl = match.Value,
                            DisplayUrl = url.DisplayUrl,
                            Type = "Video",
                            VideoInfo = new VideoInfo {VideoId = match.Groups["VideoId"].Value, VideoType = "NicoVideo"}
                        };
                        continue;
                    }

                #endregion

                #region Youtube

                match = RegexYoutube.Match(url.ExpandedUrl);
                if (match.Success)
                {
                    yield return new Media
                    {
                        MediaThumbnailUrl = "http://img.youtube.com/vi/" + match.Groups["VideoId"] + "/default.jpg",
                        MediaUrl = string.Empty,
                        ExpandedUrl = match.Value,
                        DisplayUrl = url.DisplayUrl,
                        Type = "Video",
                        VideoInfo = new VideoInfo {VideoId = match.Groups["VideoId"].Value, VideoType = "Youtube"}
                    };
                    continue;
                }

                #endregion

                #region Vine

                match = RegexVine.Match(url.ExpandedUrl);
                if (match.Success)
                {
                    yield return new Media
                    {
                        MediaThumbnailUrl = "http://img.azyobuzi.net/api/redirect?size=thumb&uri=" + match.Value,
                        MediaUrl = string.Empty,
                        ExpandedUrl = match.Value,
                        DisplayUrl = url.DisplayUrl,
                        Type = "Video",
                        VideoInfo = new VideoInfo {VideoId = match.Groups["VideoId"].Value, VideoType = "Vine"}
                    };
                }

                #endregion
            }

            foreach (var attachment in cAttachments)
                if (attachment.Type == "video" || attachment.Type == "gifv")
                    yield return new Media
                    {
                        MediaThumbnailUrl = attachment.PreviewUrl,
                        MediaUrl = attachment.Url,
                        ExpandedUrl = attachment.Url,
                        DisplayUrl = attachment.Url,
                        Type = "Video",
                        VideoInfo = new VideoInfo
                        {
                            VideoId = attachment.Url,
                            VideoType = "Twitter",
                            Size = new VideoInfo.MediaSize {Width = 640, Height = 360},
                            VideoContentType = "video/mp4"
                        }
                    };
                else
                    yield return new Media
                    {
                        MediaThumbnailUrl = attachment.PreviewUrl,
                        MediaUrl = attachment.Url,
                        ExpandedUrl = attachment.Url,
                        DisplayUrl = attachment.Url,
                        Type = "Image"
                    };
        }
    }

    public class Media
    {
        public string MediaThumbnailUrl { get; set; }
        public string MediaUrl { get; set; }
        public string DisplayUrl { get; set; }
        public string ExpandedUrl { get; set; }
        public string Type { get; set; }

        public VideoInfo VideoInfo { get; set; }
    }

    public class VideoInfo
    {
        public MediaSize Size { get; set; }
        public string VideoId { get; set; }
        public string VideoType { get; set; }
        public string VideoContentType { get; set; }

        public class MediaSize
        {
            public double Width { get; set; }
            public double Height { get; set; }
        }
    }
}