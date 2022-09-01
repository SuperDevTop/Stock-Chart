using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

public class StockEngine : MonoBehaviour
{
    public static StockEngine instance;
    public GameObject mainCanvas;
    public GameObject[] stocks;
    public Text[] leftTabs;    
    public string[] totalStocksID;
    public string[] totalStocksName;
    public string[] stocksID;
    public string[] stocksName;
    public float[] stocksVolume;
    public float[] stocksLPrice;
    public float[] stocksChage;
    public float[] stocksPChange;
    public float[] stocksOpen;
    public float[] stocksHigh;
    public float[] stocksLow;
    public float[] stocksClose;
    public float[] stocksMarketCap;
    public string[] stocksCurrency;
    public float[] caches;

    private void Awake()
    {
        instance = this;
    }
    // Start is called before the first frame update
    void Start()
    {
        mainCanvas.transform.localScale = new Vector3(Screen.width / 2532f, Screen.height / 1170f, 1f);

        for (int i = 0; i < 30; i++)
        {
            stocksID[i] = totalStocksID[i + 2 * 30];
        }

        for (int i = 0; i < stocksCurrency.Length; i++)
        {
            stocksCurrency[i] = "USD";
        }

        GetMainStockInfo();
        //StartCoroutine(GetStockName());
    }

    // Update is called once per frame
    void Update()
    {
        mainCanvas.transform.localScale = new Vector3(Screen.width / 2532f, Screen.height / 1170f, 1f);
    }

    public void TopVolumeClick()
    {
        for (int i = 0; i < stocksID.Length; i++)
        {
            caches[i] = stocksVolume[i];
        }

        Array.Sort(caches, (a, b) => a.CompareTo(b));
        ReverseStockVolumeData();

        for (int i = 0; i < leftTabs.Length; i++)
        {
            leftTabs[i].color = Color.white;
        }

        leftTabs[0].color = Color.blue;
    }

    public void TopGainerClick()
    {
        for (int i = 0; i < stocksChage.Length; i++)
        {
            caches[i] = stocksChage[i];
        }

        Array.Sort(caches, (a, b) => a.CompareTo(b));
        ReverseStockChangeData();

        for (int i = 0; i < leftTabs.Length; i++)
        {
            leftTabs[i].color = Color.white;
        }

        leftTabs[1].color = Color.blue;
    }

    public void TopLoserClick()
    {
        for (int i = 0; i < stocksChage.Length; i++)
        {
            caches[i] = stocksChage[i];
        }

        Array.Sort(caches, (a, b) => a.CompareTo(b));
        SortStockChangeData();

        for (int i = 0; i < leftTabs.Length; i++)
        {
            leftTabs[i].color = Color.white;
        }

        leftTabs[2].color = Color.blue;
    }

    public void TopPGainerClick()
    {
        for (int i = 0; i < stocksChage.Length; i++)
        {
            caches[i] = stocksPChange[i];
        }

        Array.Sort(caches, (a, b) => a.CompareTo(b));
        ReverseStockPChangeData();

        for (int i = 0; i < leftTabs.Length; i++)
        {
            leftTabs[i].color = Color.white;
        }

        leftTabs[3].color = Color.blue;
    }

    public void TopPLoserClick()
    {
        for (int i = 0; i < stocksChage.Length; i++)
        {
            caches[i] = stocksPChange[i];
        }

        Array.Sort(caches, (a, b) => a.CompareTo(b));
        SortStockPChangeData();

        for (int i = 0; i < leftTabs.Length; i++)
        {
            leftTabs[i].color = Color.white;
        }

        leftTabs[4].color = Color.blue;
    }

    public void GetMainStockInfo()
    {
        StartCoroutine(MainStockInfo());
        StartCoroutine(MarketCapInfo());
    }

    IEnumerator MainStockInfo()
    {        
        for (int i = 0; i < stocksID.Length; i++)
        {            
            UnityWebRequest www = UnityWebRequest.Get("https://api.marketstack.com/v1/tickers/" + stocksID[i] + "/eod?access_key=3c9529a8a320b0c5e8d8a0cf5a4d69e2" + "&offset = 0 & limit = 10");
            //UnityWebRequest www = UnityWebRequest.Get("http://api.marketstack.com/v1/eod?access_key=3c9529a8a320b0c5e8d8a0cf5a4d69e2&symbols=" + totalStocksID[i] + "&date_from=" + TradingView.instance.fromDate + "&date_to=" + TradingView.instance.toDate + "&interval=24hour&offset=0&limit=1000");
            yield return www.SendWebRequest();
            
            var mainStockData = (JObject)JsonConvert.DeserializeObject(www.downloadHandler.text);
            //print(mainStockData.SelectToken("data.eod[2000].close").Value<float>());

            stocksName[i] = mainStockData.SelectToken("data.name").Value<string>();
            stocksLPrice[i] = mainStockData.SelectToken("data.eod[1].close").Value<float>();
            stocksChage[i] = mainStockData.SelectToken("data.eod[0].close").Value<float>() - mainStockData.SelectToken("data.eod[1].close").Value<float>();
            stocksPChange[i] = stocksChage[i] / stocksLPrice[i];
            stocksOpen[i] = mainStockData.SelectToken("data.eod[0].open").Value<float>();
            stocksHigh[i] = mainStockData.SelectToken("data.eod[0].high").Value<float>();
            stocksLow[i] = mainStockData.SelectToken("data.eod[0].low").Value<float>();
            stocksClose[i] = mainStockData.SelectToken("data.eod[0].close").Value<float>();

            if (mainStockData.SelectToken("data.eod[0].volume").Value<string>() != null)
            {
                stocksVolume[i] = mainStockData.SelectToken("data.eod[0].volume").Value<float>();
            }
            else
            {
                stocksVolume[i] = 0;
            }
        } 

        TopVolumeClick();                       
    }

    //IEnumerator GetStockName()
    //{
    //    for (int i = 0; i < totalStocksID.Length; i++)
    //    {
    //        UnityWebRequest wwwName = UnityWebRequest.Get("https://api.marketstack.com/v1/tickers/" + totalStocksID[i] + "/eod?access_key=3c9529a8a320b0c5e8d8a0cf5a4d69e2" + "&offset = 0 & limit = 10");
    //        //UnityWebRequest www = UnityWebRequest.Get("http://api.marketstack.com/v1/eod?access_key=3c9529a8a320b0c5e8d8a0cf5a4d69e2&symbols=" + totalStocksID[i] + "&date_from=" + TradingView.instance.fromDate + "&date_to=" + TradingView.instance.toDate + "&interval=24hour&offset=0&limit=1000");
    //        yield return wwwName.SendWebRequest();

    //        var mainStockData = (JObject)JsonConvert.DeserializeObject(wwwName.downloadHandler.text);
    //        //print(mainStockData.SelectToken("data.eod[2000].close").Value<float>());

    //        totalStocksName[i] = mainStockData.SelectToken("data.name").Value<string>();            
    //    }     
    //}

    IEnumerator MarketCapInfo()
    {
        for (int i = 0; i < stocksID.Length; i++)
        {
            UnityWebRequest www1 = UnityWebRequest.Get("https://finnhub.io/api/v1/stock/profile2?symbol=" + stocksID[i] + "&token=cbtn8kqad3i65oqck6f0");
            yield return www1.SendWebRequest();

            var marketCapData = (JObject)JsonConvert.DeserializeObject(www1.downloadHandler.text);
            //print(www1.downloadHandler.text);

            if (www1.downloadHandler.text == "{}")
            {
                stocksMarketCap[i] = 0f;
            }
            else
            {
                stocksMarketCap[i] = marketCapData.SelectToken("marketCapitalization").Value<float>();
            }            
        }
    }   

    public void SortStockChangeData()
    {
        for (int i = 0; i < caches.Length; i++)
        {
            for (int j = 0; j < caches.Length; j++)
            {
                if (caches[i] == stocksChage[j])
                {
                    stocks[i].GetComponentsInChildren<Text>()[0].text = stocksName[j];
                    stocks[i].GetComponentsInChildren<Text>()[1].text = "" + stocksLPrice[j];
                    stocks[i].GetComponentsInChildren<Text>()[2].text = "" + stocksChage[j];
                    stocks[i].GetComponentsInChildren<Text>()[3].text = "" + stocksPChange[j];

                    if (stocksChage[j] >= 0)
                    {
                        stocks[i].GetComponentsInChildren<Text>()[2].color = Color.green;
                    }
                    else
                    {
                        stocks[i].GetComponentsInChildren<Text>()[2].color = Color.red;
                    }

                    if (stocksChage[j] >= 0)
                    {
                        stocks[i].GetComponentsInChildren<Text>()[3].color = Color.green;
                    }
                    else
                    {
                        stocks[i].GetComponentsInChildren<Text>()[3].color = Color.red;
                    }
                }
            }
        }
    }

    public void ReverseStockChangeData()
    {
        for (int i = 0; i < caches.Length; i++)
        {
            for (int j = 0; j < caches.Length; j++)
            {
                if (caches[i] == stocksChage[j])
                {
                    stocks[caches.Length - i - 1].GetComponentsInChildren<Text>()[0].text = stocksName[j];
                    stocks[caches.Length - i - 1].GetComponentsInChildren<Text>()[1].text = "" + stocksLPrice[j];
                    stocks[caches.Length - i - 1].GetComponentsInChildren<Text>()[2].text = "" + stocksChage[j];
                    stocks[caches.Length - i - 1].GetComponentsInChildren<Text>()[3].text = "" + stocksPChange[j];

                    if (stocksChage[j] >= 0)
                    {
                        stocks[caches.Length - i - 1].GetComponentsInChildren<Text>()[2].color = Color.green;
                    }
                    else
                    {
                        stocks[caches.Length - i - 1].GetComponentsInChildren<Text>()[2].color = Color.red;
                    }

                    if (stocksChage[j] >= 0)
                    {
                        stocks[caches.Length - i - 1].GetComponentsInChildren<Text>()[3].color = Color.green;
                    }
                    else
                    {
                        stocks[caches.Length - i - 1].GetComponentsInChildren<Text>()[3].color = Color.red;
                    }
                }
            }
        }
    }

    public void SortStockPChangeData()
    {
        for (int i = 0; i < caches.Length; i++)
        {
            for (int j = 0; j < caches.Length; j++)
            {
                if (caches[i] == stocksPChange[j])
                {
                    stocks[i].GetComponentsInChildren<Text>()[0].text = stocksName[j];
                    stocks[i].GetComponentsInChildren<Text>()[1].text = "" + stocksLPrice[j];
                    stocks[i].GetComponentsInChildren<Text>()[2].text = "" + stocksChage[j];
                    stocks[i].GetComponentsInChildren<Text>()[3].text = "" + stocksPChange[j];

                    if (stocksPChange[j] >= 0)
                    {
                        stocks[i].GetComponentsInChildren<Text>()[2].color = Color.green;
                    }
                    else
                    {
                        stocks[i].GetComponentsInChildren<Text>()[2].color = Color.red;
                    }

                    if (stocksPChange[j] >= 0)
                    {
                        stocks[i].GetComponentsInChildren<Text>()[3].color = Color.green;
                    }
                    else
                    {
                        stocks[i].GetComponentsInChildren<Text>()[3].color = Color.red;
                    }
                }
            }
        }
    }

    public void ReverseStockPChangeData()
    {
        for (int i = 0; i < caches.Length; i++)
        {
            for (int j = 0; j < caches.Length; j++)
            {
                if (caches[i] == stocksPChange[j])
                {
                    stocks[caches.Length - i - 1].GetComponentsInChildren<Text>()[0].text = stocksName[j];
                    stocks[caches.Length - i - 1].GetComponentsInChildren<Text>()[1].text = "" + stocksLPrice[j];
                    stocks[caches.Length - i - 1].GetComponentsInChildren<Text>()[2].text = "" + stocksChage[j];
                    stocks[caches.Length - i - 1].GetComponentsInChildren<Text>()[3].text = "" + stocksPChange[j];

                    if (stocksPChange[j] >= 0)
                    {
                        stocks[caches.Length - i - 1].GetComponentsInChildren<Text>()[2].color = Color.green;
                    }
                    else
                    {
                        stocks[caches.Length - i - 1].GetComponentsInChildren<Text>()[2].color = Color.red;
                    }

                    if (stocksPChange[j] >= 0)
                    {
                        stocks[caches.Length - i - 1].GetComponentsInChildren<Text>()[3].color = Color.green;
                    }
                    else
                    {
                        stocks[caches.Length - i - 1].GetComponentsInChildren<Text>()[3].color = Color.red;
                    }
                }
            }
        }
    } 

    public void ReverseStockVolumeData()
    {
        for (int i = 0; i < caches.Length; i++)
        {
            for (int j = 0; j < caches.Length; j++)
            {
                if (caches[i] == stocksVolume[j])
                {
                    stocks[caches.Length - i - 1].GetComponentsInChildren<Text>()[0].text = stocksName[j];
                    stocks[caches.Length - i - 1].GetComponentsInChildren<Text>()[1].text = "" + stocksLPrice[j];
                    stocks[caches.Length - i - 1].GetComponentsInChildren<Text>()[2].text = "" + stocksChage[j];
                    stocks[caches.Length - i - 1].GetComponentsInChildren<Text>()[3].text = "" + stocksPChange[j];  

                    if (stocksPChange[j] >= 0)
                    {
                        stocks[caches.Length - i - 1].GetComponentsInChildren<Text>()[2].color = Color.green;
                    }
                    else
                    {
                        stocks[caches.Length - i - 1].GetComponentsInChildren<Text>()[2].color = Color.red;
                    }

                    if (stocksPChange[j] >= 0)
                    {
                        stocks[caches.Length - i - 1].GetComponentsInChildren<Text>()[3].color = Color.green;
                    }
                    else
                    {
                        stocks[caches.Length - i - 1].GetComponentsInChildren<Text>()[3].color = Color.red;
                    }                  
                }
            }
        }
    }
}
