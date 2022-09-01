using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using System;
using UnityEngine.EventSystems;

public class SearchStock : MonoBehaviour
{
    public InputField searchField;
    public GameObject[] results;
    public int index = 0;

    // Start is called before the first frame update
    void Start()
    {
        index = 0;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void TextChanged()
    {
        for (int i = 0; i < results.Length; i++)
        {
            results[i].GetComponentsInChildren<Text>()[0].text = "";
            results[i].GetComponentsInChildren<Text>()[1].text = "";
            results[i].GetComponentsInChildren<Text>()[2].text = "";
        }

        index = 0;

        for (int i = 0; i < StockEngine.instance.totalStocksName.Length;i++)
        {
            if (StockEngine.instance.totalStocksName[i].ContainsInsensitive(searchField.text))
            {
                if (index < 8)
                {
                    results[index].GetComponentsInChildren<Text>()[0].text = "Stocks";
                    results[index].GetComponentsInChildren<Text>()[1].text = StockEngine.instance.totalStocksID[i];
                    results[index].GetComponentsInChildren<Text>()[2].text = StockEngine.instance.totalStocksName[i];
                    index++;
                }                
            }
        }        
    }

    public void ElementClick()
    {
        StartCoroutine(GetResult());
    }

    IEnumerator GetResult()
    {
        print(EventSystem.current.currentSelectedGameObject.GetComponentsInChildren<Text>()[1].text);
        UnityWebRequest wwwResult = UnityWebRequest.Get("https://api.marketstack.com/v1/tickers/" + EventSystem.current.currentSelectedGameObject.GetComponentsInChildren<Text>()[1].text + "/eod?access_key=3c9529a8a320b0c5e8d8a0cf5a4d69e2" + "&offset = 0 & limit = 10");
        yield return wwwResult.SendWebRequest();
        print(wwwResult.downloadHandler.text);
        var mainStockData = (JObject)JsonConvert.DeserializeObject(wwwResult.downloadHandler.text);
        StockClick.instance.dashboardComName.text = mainStockData.SelectToken("data.name").Value<string>(); ;
        StockClick.instance.rightComName.text = mainStockData.SelectToken("data.name").Value<string>(); ;
        StockClick.instance.rightSubName.text = mainStockData.SelectToken("data.name").Value<string>(); ;
        StockClick.instance.rightLPrice.text = "" + mainStockData.SelectToken("data.eod[1].close").Value<float>();
        StockClick.instance.rightLCurrency.text = "USD";
        StockClick.instance.rightOpen.text = "" + mainStockData.SelectToken("data.eod[0].open").Value<float>();
        StockClick.instance.rightHigh.text = "" + mainStockData.SelectToken("data.eod[0].high").Value<float>();
        StockClick.instance.rightLow.text = "" + mainStockData.SelectToken("data.eod[0].low").Value<float>();
        StockClick.instance.rightClose.text = "" + mainStockData.SelectToken("data.eod[0].close").Value<float>();
        StockClick.instance.rightMarketCap.text = "0";
        StockClick.instance.rightStockVolume.text = "" + Math.Round((mainStockData.SelectToken("data.eod[0].volume").Value<float>() / 1000000f), 1) + "M";

        if ((mainStockData.SelectToken("data.eod[0].close").Value<float>() - mainStockData.SelectToken("data.eod[1].close").Value<float>()) / mainStockData.SelectToken("data.eod[1].close").Value<float>() >= 0)
        {
            StockClick.instance.rightPChangeBox.color = new Color32(17, 58, 36, 255);
            StockClick.instance.rightPChange.color = Color.green;
            StockClick.instance.rightPChange.text = "(+" + Math.Round((mainStockData.SelectToken("data.eod[0].close").Value<float>() - mainStockData.SelectToken("data.eod[1].close").Value<float>()) / mainStockData.SelectToken("data.eod[1].close").Value<float>(), 2) + "%)";
        }
        else
        {
            StockClick.instance.rightPChangeBox.color = new Color32(106, 28, 11, 255);
            StockClick.instance.rightPChange.color = Color.red;
            StockClick.instance.rightPChange.text = "(" + Math.Round((mainStockData.SelectToken("data.eod[0].close").Value<float>() - mainStockData.SelectToken("data.eod[1].close").Value<float>()) / mainStockData.SelectToken("data.eod[1].close").Value<float>(), 2) + "%)";
        }

        TradingView.instance.selectedStockID = EventSystem.current.currentSelectedGameObject.GetComponentsInChildren<Text>()[1].text;
        TradingView.instance.DrawTradingView(TradingView.instance.selectedStockID);
        TradingView.instance.StockRange(TradingView.instance.selectedStockID);
    }
}
