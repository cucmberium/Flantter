using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Windows.Storage;
using Windows.Web.Http;

namespace Flantter.MilkyWay.Models.Twitter.Thumbnail
{
    public static class NicoVideo
    {
        public static async void GetThumbnail(string videoId, string fileName)
        {
            var file = await ApplicationData.Current.TemporaryFolder.TryGetItemAsync(videoId);
            if (file != null)
                return;

            var apiUrl = "http://ext.nicovideo.jp/api/getthumbinfo/" + videoId;

            HttpClient client = new HttpClient();
            HttpResponseMessage response = await client.GetAsync(new Uri(apiUrl));
            if (!response.IsSuccessStatusCode)
                return;

            response.Content.Headers.ContentType.CharSet = "utf-8";

            string contents = await response.Content.ReadAsStringAsync();

            if (string.IsNullOrWhiteSpace(contents))
                return;

            var xml = XDocument.Parse(contents);
            var responseXml = xml.Element("nicovideo_thumb_response");
            if (responseXml.Attribute("status").Value != "ok")
                return;

            var thumbnailUrl = responseXml.Element("thumb").Element("thumbnail_url").Value;

            response = await client.GetAsync(new Uri(thumbnailUrl));

            var imageFile = await ApplicationData.Current.TemporaryFolder.CreateFileAsync(fileName, CreationCollisionOption.OpenIfExists);
            await FileIO.WriteBytesAsync(imageFile, (await response.Content.ReadAsBufferAsync()).ToArray());
        }
    }
}
