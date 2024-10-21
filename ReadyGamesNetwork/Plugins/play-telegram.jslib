mergeInto(LibraryManager.library, {
  PLAYGetAppDataJs: function() {
    let appData;
    try {
      appData = window.Telegram.WebView.initParams.tgWebAppData;
    }
    catch (error) {
      console.error("Error while getting Telegram init params: ", error);
    }
    appData = appData ? appData : "";
    var bufferSize = lengthBytesUTF8(appData) + 1;
    var buffer = _malloc(bufferSize);
    stringToUTF8(appData, buffer, bufferSize);
    return buffer;
  },

  PLAYOpenLinkJs: function (url) {
    const telegram = window.Telegram.WebApp;
    telegram.openLink(UTF8ToString(url));
  },

  PLAYOpenTelegramLinkJs: function (url) {
    const telegram = window.Telegram.WebApp;
    telegram.openTelegramLink(UTF8ToString(url));
  },
});