mergeInto(LibraryManager.library, {
  PLAY_GetInitParamsJs: function() {
    let initParams;
    try {
      const tgInitParams = window.Telegram.WebView.initParams;
      
      let tgWebAppData = {};
      if (tgInitParams.tgWebAppData) {
        try {
          tgWebAppData = Object.fromEntries(new URLSearchParams(tgInitParams.tgWebAppData));
          if (tgWebAppData.user) {
              tgWebAppData.user = JSON.parse(decodeURIComponent(tgWebAppData.user));
          }
        } catch (error) {
          console.error("Error decoding or parsing tgWebAppData:", error);
        }
      }
      
      let tgWebAppThemeParams = {};
      if (tgInitParams.tgWebAppThemeParams) {
        try {
          tgWebAppThemeParams = JSON.parse(tgInitParams.tgWebAppThemeParams);
        } catch (error) {
          console.error("Error parsing tgWebAppThemeParams:", error);
        }
      }
      
      const parsedInitParams = {
        tgWebAppData: tgWebAppData,
        tgWebAppThemeParams: tgWebAppThemeParams,
      };
      
      Object.keys(tgInitParams).forEach(key => {
        if (key !== "tgWebAppData" && key !== "tgWebAppThemeParams") {
          parsedInitParams[key] = tgInitParams[key];
        }
      });
      
      initParams = JSON.stringify(parsedInitParams);
    } catch (error) {
      console.error("Error while getting Telegram init params: ", error);
    }
    initParams = initParams ? initParams : "";
    const bufferSize = lengthBytesUTF8(initParams) + 1;
    const buffer = _malloc(bufferSize);
    stringToUTF8(initParams, buffer, bufferSize);
    return buffer;
  },

  PLAY_OpenLinkJs: function (urlPtr) {
    try {
      const url = UTF8ToString(urlPtr);
      const telegram = window.Telegram.WebApp;
      telegram.openLink(url);
    } catch (error) {
      console.error("Error while opening link: ", error);
      window.open(url);
    }
  },

  PLAY_OpenTelegramLinkJs: function (urlPtr) {
    try {
      const url = UTF8ToString(urlPtr);
      const telegram = window.Telegram.WebApp;
      telegram.openTelegramLink(url);
    } catch (error) {
      console.error("Error while opening Telegram link: ", error);
      window.open(url);
    }
  },
});