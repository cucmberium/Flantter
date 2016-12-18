// ツイートに pixiv が含まれる際に高画質なものに置換し、R-18なものはR-18画像を表示する

// System Namespace以下の機能は以下のようにして使える
// javascript > var log = System.Diagnostics.Debug.WriteLine;
// javascript > log("ふらんちゃん");
//
// 詳細 : https://github.com/sebastienros/jint 


// おまじない
var fl = importNamespace('Flantter.MilkyWay.Plugin');

// プラグインはregisterPluginで登録することでload関数が呼ばれる (load関数は必須)
function load() {
    // ログを吐く関数(デバッグ用)
    fl.Debug.Log("Initialize pixiv.js");

    // Filterに関数を追加する関数 (イベント名, jsの関数)
    fl.Event.RegisterFunction("TweetReceivedAtColumn", pixivchecker);
}

function unload() {
    fl.Event.UnregisterFunction("TweetReceivedAtColumn", pixivchecker);
}

// Filter用関数定義
function pixivchecker(ev) {
    fl.Debug.Log("Search pixiv image");
    if (!ev.Info.MediaVisibility)
        return;

    // Linqはまだ使えない
    for (var i = 0; i < ev.Info.MediaEntities.Count; i++) {
        var media = ev.Info.MediaEntities[i];

        var illust_id = (/pixiv.net.*illust_id=(\d+)/.exec(media.Model.ExpandedUrl) || [])[1];
        if (illust_id === undefined)
            continue;

        fl.Debug.Log("Detect pixiv");
        media.MediaUrl = "http://d250g2.com/d250g2.jpg";
        media.MediaThumbnailUrl = "http://d250g2.com/d250g2.jpg";
        media.DisplayUrl = "d250g2.com";
        media.ExpandedUrl = "http://d250g2.com/";
        media.Type = "Image";
        media.ParentEntities = entities;

        entities.Media.Add(media);
        break;
	}
}

function istypeof(type, obj) {
    var clas = Object.prototype.toString.call(obj).slice(8, -1);
    return obj !== undefined && obj !== null && clas === type;
}

// すべてのプラグインはregisterPluginでプラグインを登録する必要がある (プラグイン名, 説明, バージョン)
registerPlugin("pixiv", "d250g2.com", "0.0.1")