using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Web.Http;

namespace Flantter.MilkyWay.Models.Twitter.Thumbnail
{
    public static class Pixiv
    {
        public static async void GetThumbnail(string id, string fileName)
        {
            var file = await ApplicationData.Current.TemporaryFolder.TryGetItemAsync(fileName);
            if (file != null)
                return;

            var url = "http://www.pixiv.net/member_illust.php?mode=medium&illust_id=" + id;

            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/51.0.2704.84 Safari/537.36");
            client.DefaultRequestHeaders.Add("Referer", url);
            HttpResponseMessage response = await client.GetAsync(new Uri(url));
            if (!response.IsSuccessStatusCode)
                return;

            response.Content.Headers.ContentType.CharSet = "utf-8";

            string contents = await response.Content.ReadAsStringAsync();

            if (string.IsNullOrWhiteSpace(contents))
                return;

            var doc = new HtmlDocument();
            doc.LoadHtml(contents);
            var imgContainer = doc.DocumentNode.Descendants("div").Where(d => d.Attributes.Contains("class") && d.Attributes["class"].Value.Contains("img-container"));
            if (imgContainer.Count() == 0)
                return;

            var imgUrl = imgContainer.First().Descendants("img").First().GetAttributeValue("src", "");

            response = await client.GetAsync(new Uri(imgUrl));
            var imageFile = await ApplicationData.Current.TemporaryFolder.CreateFileAsync(fileName, CreationCollisionOption.OpenIfExists);
            await FileIO.WriteBytesAsync(imageFile, (await response.Content.ReadAsBufferAsync()).ToArray());
        }
    }
}
