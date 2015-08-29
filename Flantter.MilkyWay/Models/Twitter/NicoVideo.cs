using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Flantter.MilkyWay.Models.Twitter
{
    public class NicoVideo
    {
        private const string thumbWatchUrl = "http://ext.nicovideo.jp/thumb_watch";

        public string VideoUrl { get; set; }
        public string VideoCookieUrl { get; set; }
        public string VideoContentType { get; set; }

        public NicoVideo()
        {
            this.VideoUrl = string.Empty;
            this.VideoCookieUrl = string.Empty;
            this.VideoContentType = string.Empty;
        }

        public async Task GetNicoVideoInfo(string videoId)
        {
            string videoInfoJs = string.Empty;
            string videoInfoData = string.Empty;
            string videoInfoUrl = string.Empty;

            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.AcceptLanguage.Add(new StringWithQualityHeaderValue("en-us", 0.5));
            client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("Mozilla", "5.0"));
            client.DefaultRequestHeaders.AcceptCharset.Add(new StringWithQualityHeaderValue("utf-8", 0.7));
            HttpResponseMessage response = await client.GetAsync(new Uri(thumbWatchUrl + "/" + videoId));
            response.Content.Headers.ContentType.CharSet = "utf-8";
            string contents = await response.Content.ReadAsStringAsync();

            if (string.IsNullOrWhiteSpace(contents))
                return;

            videoInfoJs = contents;

            string thumbPlayKey = string.Empty;
            Match match;
            match = Regex.Match(videoInfoJs, @"'thumbPlayKey':\s'(?<ThumbPlayKey>[a-z0-9\.\-_]+)'", RegexOptions.IgnoreCase);
            if (match.Success)
                thumbPlayKey = match.Groups["ThumbPlayKey"].ToString();
            else
                return;

            match = Regex.Match(videoInfoJs, @"movieType:\s'(?<MovieType>[a-z0-9\.\-_]+)'", RegexOptions.IgnoreCase);
            if (match.Success)
                this.VideoContentType = "video/" + match.Groups["MovieType"].ToString();
            else
                return;

            var cookieClient = new Windows.Web.Http.HttpClient();
            this.VideoCookieUrl = thumbWatchUrl + "?as3=1&v=" + videoId + "&k=" + thumbPlayKey;
            var cookieResponse = await cookieClient.GetAsync(new Uri(this.VideoCookieUrl));
            cookieResponse.EnsureSuccessStatusCode();
            contents = await cookieResponse.Content.ReadAsStringAsync();

            if (string.IsNullOrWhiteSpace(contents))
                return;

            videoInfoData = contents;
            string[] videoInfoArrayData = videoInfoData.Split('&');

            foreach (var videoInfo in videoInfoArrayData)
            {
                if (videoInfo.StartsWith("url="))
                {
                    videoInfoUrl = Uri.UnescapeDataString(videoInfo.Replace("url=", ""));
                    break;
                }
            }

            if (!string.IsNullOrWhiteSpace(videoInfoUrl))
                this.VideoUrl = videoInfoUrl;

            return;
        }
    }
}
