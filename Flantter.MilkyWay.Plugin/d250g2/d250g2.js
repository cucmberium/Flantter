// ツイートに d250g2.com が含まれるときに燃える
//
// Usage:
// d250g2(Entities)
// Return:
// bool (always false)
//


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
    fl.Debug.Log("Initialize d250g2.js");

    // Filterに関数を追加する関数 (関数名, 引数の数, jsの関数)
    fl.Filter.RegisterFunction("d250g2", 1, d250g2);
}

// Filter用関数定義
function d250g2(entities) {
	fl.Debug.Log("Search d250g2.com");
    // Linqはまだ使えない
	for (var i = 0; i < entities.Urls.Count; i++) {
	    var url = entities.Urls[i];
	    if (url.DisplayUrl == "d250g2.com") {
	        fl.Debug.Log("Detect d250g2.com");
	        var media = new Flantter.MilkyWay.Models.Twitter.Objects.MediaEntity();
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
	
    return false;
}

function istypeof(type, obj) {
    var clas = Object.prototype.toString.call(obj).slice(8, -1);
    return obj !== undefined && obj !== null && clas === type;
}

// すべてのプラグインはregisterPluginでプラグインを登録する必要がある
registerPlugin("d250g2", "d250g2.com", "0.0.1")