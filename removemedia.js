// ツイートに付いている画像ツイートからサムネイルを削除するフィルタ関数を追加するプラグイン
//
// Usage:
// RemoveMedia(Entities)
// Return:
// bool (always true)
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
    fl.Debug.Log("Initialize removemedia.js");

    // Filterに関数を追加する関数
    fl.Filter.RegisterFunction("RemoveMedia", 1, removemedia);
}

// Filter用関数定義
function removemedia(entities) {
    entities.Media.Clear();
    return true;
}

function istypeof(type, obj) {
    var clas = Object.prototype.toString.call(obj).slice(8, -1);
    return obj !== undefined && obj !== null && clas === type;
}

// すべてのプラグインはregisterPluginでプラグインを登録する必要がある
registerPlugin("removemedia", "ツイート内の画像を削除するプラグイン", "0.0.1")