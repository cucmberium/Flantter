using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Flantter.MilkyWay.Models.Twitter.Video
{
    public class NicoVideo
    {
        private const string ThumbWatchUrl = "http://ext.nicovideo.jp/thumb_watch";

        public NicoVideo()
        {
            VideoUrl = string.Empty;
            VideoCookieUrl = string.Empty;
            VideoContentType = string.Empty;
        }

        public string VideoUrl { get; set; }
        public string VideoCookieUrl { get; set; }
        public string VideoContentType { get; set; }

        public async Task GetNicoVideoInfo(string videoId)
        {
            var videoInfoUrl = string.Empty;

            var client = new HttpClient();
            client.DefaultRequestHeaders.AcceptLanguage.Add(new StringWithQualityHeaderValue("en-us", 0.5));
            client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("Mozilla", "5.0"));
            client.DefaultRequestHeaders.AcceptCharset.Add(new StringWithQualityHeaderValue("utf-8", 0.7));
            var response = await client.GetAsync(new Uri(ThumbWatchUrl + "/" + videoId));

            if (!response.IsSuccessStatusCode)
                return;

            response.Content.Headers.ContentType.CharSet = "utf-8";
            var contents = await response.Content.ReadAsStringAsync();

            if (string.IsNullOrWhiteSpace(contents))
                return;

            var videoInfoJs = contents;

            string thumbPlayKey;
            var match = Regex.Match(videoInfoJs, @"'thumbPlayKey':\s'(?<ThumbPlayKey>[a-z0-9\.\-_]+)'",
                RegexOptions.IgnoreCase);
            if (match.Success)
                thumbPlayKey = match.Groups["ThumbPlayKey"].ToString();
            else
                return;

            match = Regex.Match(videoInfoJs, @"movieType:\s'(?<MovieType>[a-z0-9\.\-_]+)'", RegexOptions.IgnoreCase);
            if (match.Success)
                VideoContentType = "video/" + match.Groups["MovieType"];
            else
                return;

            var cookieClient = new Windows.Web.Http.HttpClient();
            VideoCookieUrl = ThumbWatchUrl + "?as3=1&v=" + videoId + "&k=" + thumbPlayKey;
            var cookieResponse = await cookieClient.GetAsync(new Uri(VideoCookieUrl));
            if (!cookieResponse.IsSuccessStatusCode)
                return;

            contents = await cookieResponse.Content.ReadAsStringAsync();

            if (string.IsNullOrWhiteSpace(contents))
                return;

            var videoInfoData = contents;
            var videoInfoArrayData = videoInfoData.Split('&');

            foreach (var videoInfo in videoInfoArrayData)
                if (videoInfo.StartsWith("url="))
                {
                    videoInfoUrl = Uri.UnescapeDataString(videoInfo.Replace("url=", ""));
                    break;
                }

            if (!string.IsNullOrWhiteSpace(videoInfoUrl))
                VideoUrl = videoInfoUrl;
        }
    }
}