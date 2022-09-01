using System.Collections;
using System.Drawing;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using UnityEngine.Networking;
using System.Linq;

public class TradingView : MonoBehaviour
{
    public static TradingView instance;
    public Image stockRed;
    public Image stockGreen;
    public Image chartRed;
    public Image chartGreen;    
    public GameObject[] stocks;
    public GameObject[] charts;
    public Text[] yPrice;
    public Text[] yVolume;
    public Text[] rangeMinText;
    public Text[] rangeMaxText;
    public Text[] rangeCMinText;
    public Text[] rangeCMaxText;
    public Text[] averageVolumeText;
    public Text timeInterval;
    public Text[] stockIDSignal;
    public Text[] timeframeSignal;
    public Image[] redImages;
    public Image[] greenImages;
    public Image[] greyImages;
    public Image[] buttonSignals;
    public Image arrowImage;

    public float[] stockOpenValues;
    public float[] stockCloseValues;
    public float[] stockHighValues;
    public float[] stockLowValues;
    public float[] stockVolumeValues;
    public int shortValue;
    public int longValue;

    public string multiplier;
    public string timeSpan;
    public string fromDate;
    public string toDate;
    public string selectedStockID;

    public AudioSource alertSource;
    public GameObject alertImage;

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        shortValue = 20;
        longValue = 100;
        multiplier = "1";
        timeSpan = "day";
        selectedStockID = "AAPL";
        DrawTradingView(selectedStockID);
        print(yVolume[0].rectTransform.position.x);        
        //stockRed.color = new Color32(252, 84, 84, 255);
        //stockGreen.color = new Color32(0, 192, 163, 255);
        //chartRed.color = new Color32(86, 48, 48, 255);
        //chartGreen.color = new Color32(26, 66, 54, 255);
    }

    void Update()
    {
        
    }

    public void DrawTradingView(string stockID)
    {
        StartCoroutine(TradingInfo(stockID));
    }

    public void BBMAClick()
    {
        StartCoroutine(ShowBBMA());
    }

    public void StockRange(string stockID)
    {
        StartCoroutine(GetRange(stockID));
    }

    IEnumerator GetRange(string stockID)
    {       
        UnityWebRequest wwwRange = UnityWebRequest.Get("https://api.marketstack.com/v1/tickers/" + stockID + "/eod?access_key=3c9529a8a320b0c5e8d8a0cf5a4d69e2" + "&offset = 0 & limit = 300");        
        yield return wwwRange.SendWebRequest();
      
        var RangeStockData = (JObject)JsonConvert.DeserializeObject(wwwRange.downloadHandler.text);       

        var lProperties = RangeStockData.DescendantsAndSelf()
                        .OfType<JProperty>()
                        .Where(p => p.Name == "low");
        var min = lProperties
                    .Aggregate((p1, p2) => p1.Value.Value<float>() < p2.Value.Value<float>() ? p1 : p2);

        //print(min.Value);

        var hProperties = RangeStockData.DescendantsAndSelf()
                        .OfType<JProperty>()
                        .Where(p => p.Name == "high");
        var max = hProperties
                    .Aggregate((p1, p2) => p1.Value.Value<float>() > p2.Value.Value<float>() ? p1 : p2);

        //print(max.Value);
        float sum = 0f;
        float average = 0f;

        for (int i = 0; i < RangeStockData.SelectToken("pagination.count").Value<float>(); i++)
        {
            if (RangeStockData.SelectToken("data.eod[" + i + "].volume").Value<string>() != null)
            {
                sum += RangeStockData.SelectToken("data.eod[" + i + "].volume").Value<float>();
            }                      
        }

        average = sum / RangeStockData.SelectToken("pagination.count").Value<float>();
        averageVolumeText[4].text = "" + Math.Round((average / 1000000f), 1) + "M";
        averageVolumeText[5].text = "" + Math.Round((average / 1000000f), 1) + "M";

        for (int i = 0;i < rangeMinText.Length;i++)
        {
            rangeMinText[i].text = "" + min.Value;
            rangeMaxText[i].text = "" + max.Value;
        }

        StartCoroutine(GetWeekRange(stockID));
        StartCoroutine(GetMonthRange(stockID));
        StartCoroutine(GetTMonthRange(stockID));
        StartCoroutine(GetSMonthRange(stockID));
    }

    IEnumerator GetWeekRange(string stockID)
    {       
        UnityWebRequest wwwWeekRange = UnityWebRequest.Get("https://api.marketstack.com/v1/tickers/" + stockID + "/eod?access_key=3c9529a8a320b0c5e8d8a0cf5a4d69e2" + "&offset = 0 & limit = 5");
        yield return wwwWeekRange.SendWebRequest();

        var RangeStockData = (JObject)JsonConvert.DeserializeObject(wwwWeekRange.downloadHandler.text);      

        var lProperties = RangeStockData.DescendantsAndSelf()
                        .OfType<JProperty>()
                        .Where(p => p.Name == "low");
        var min = lProperties
                    .Aggregate((p1, p2) => p1.Value.Value<float>() < p2.Value.Value<float>() ? p1 : p2);

        //print(min.Value);

        var hProperties = RangeStockData.DescendantsAndSelf()
                        .OfType<JProperty>()
                        .Where(p => p.Name == "high");
        var max = hProperties
                    .Aggregate((p1, p2) => p1.Value.Value<float>() > p2.Value.Value<float>() ? p1 : p2);

        //print(max.Value);

        rangeCMinText[0].text = "" + min.Value;
        rangeCMaxText[0].text = "" + max.Value;

        greyImages[0].fillAmount = (float.Parse(rangeCMinText[0].text) - float.Parse(rangeMinText[0].text)) / (float.Parse(rangeMaxText[0].text) - float.Parse(rangeMinText[0].text));

        if (float.Parse(rangeCMinText[0].text) >= (float.Parse(rangeMinText[0].text) + float.Parse(rangeMaxText[0].text)) / 2)
        {
            redImages[0].fillAmount = 0;
            greenImages[0].fillAmount = (float.Parse(rangeCMaxText[0].text) - ((float.Parse(rangeMinText[0].text) + float.Parse(rangeMaxText[0].text)) / 2)) / (((float.Parse(rangeMinText[0].text) + float.Parse(rangeMaxText[0].text)) / 2) - float.Parse(rangeMinText[0].text));
        }
        else
        {
            if (float.Parse(rangeCMaxText[0].text) < (float.Parse(rangeMinText[0].text) + float.Parse(rangeMaxText[0].text)) / 2)
            {
                redImages[0].fillAmount = (float.Parse(rangeCMaxText[0].text) - float.Parse(rangeMinText[0].text)) / (((float.Parse(rangeMinText[0].text) + float.Parse(rangeMaxText[0].text)) / 2) - float.Parse(rangeMinText[0].text));
                greenImages[0].fillAmount = 0;
            }
            else
            {
                redImages[0].fillAmount = 1;
                greenImages[0].fillAmount = (float.Parse(rangeCMaxText[0].text) - ((float.Parse(rangeMinText[0].text) + float.Parse(rangeMaxText[0].text)) / 2)) / (((float.Parse(rangeMinText[0].text) + float.Parse(rangeMaxText[0].text)) / 2) - float.Parse(rangeMinText[0].text)) ;
            }
        }

        float sum = 0f;
        float average = 0f;


        for (int i = 0; i < RangeStockData.SelectToken("pagination.count").Value<float>(); i++)
        {
            if (RangeStockData.SelectToken("data.eod[" + i + "].volume").Value<string>() != null)
            {
                sum += RangeStockData.SelectToken("data.eod[" + i + "].volume").Value<float>();
            }
        }

        average = sum / RangeStockData.SelectToken("pagination.count").Value<float>();
        averageVolumeText[0].text = "" + Math.Round((average / 1000000f), 1) + "M";       
    }

    IEnumerator GetMonthRange(string stockID)
    {       
        UnityWebRequest wwwMonthRange = UnityWebRequest.Get("https://api.marketstack.com/v1/tickers/" + stockID + "/eod?access_key=3c9529a8a320b0c5e8d8a0cf5a4d69e2" + "&offset = 0 & limit = 25");
        yield return wwwMonthRange.SendWebRequest();

        var RangeStockData = (JObject)JsonConvert.DeserializeObject(wwwMonthRange.downloadHandler.text);

        var lProperties = RangeStockData.DescendantsAndSelf()
                        .OfType<JProperty>()
                        .Where(p => p.Name == "low");
        var min = lProperties
                    .Aggregate((p1, p2) => p1.Value.Value<float>() < p2.Value.Value<float>() ? p1 : p2);

        //print(min.Value);

        var hProperties = RangeStockData.DescendantsAndSelf()
                        .OfType<JProperty>()
                        .Where(p => p.Name == "high");
        var max = hProperties
                    .Aggregate((p1, p2) => p1.Value.Value<float>() > p2.Value.Value<float>() ? p1 : p2);

        //print(max.Value);

        rangeCMinText[1].text = "" + min.Value;
        rangeCMaxText[1].text = "" + max.Value;

        greyImages[1].fillAmount = (float.Parse(rangeCMinText[1].text) - float.Parse(rangeMinText[1].text)) / (float.Parse(rangeMaxText[1].text) - float.Parse(rangeMinText[1].text));

        if (float.Parse(rangeCMinText[1].text) >= (float.Parse(rangeMinText[1].text) + float.Parse(rangeMaxText[1].text)) / 2)
        {
            redImages[1].fillAmount = 0;
            greenImages[1].fillAmount = (float.Parse(rangeCMaxText[1].text) - ((float.Parse(rangeMinText[1].text) + float.Parse(rangeMaxText[1].text)) / 2)) / (((float.Parse(rangeMinText[1].text) + float.Parse(rangeMaxText[1].text)) / 2) - float.Parse(rangeMinText[1].text));
        }
        else
        {
            if (float.Parse(rangeCMaxText[1].text) < (float.Parse(rangeMinText[1].text) + float.Parse(rangeMaxText[1].text)) / 2)
            {
                redImages[1].fillAmount = (float.Parse(rangeCMaxText[1].text) - float.Parse(rangeMinText[1].text)) / (((float.Parse(rangeMinText[1].text) + float.Parse(rangeMaxText[1].text)) / 2) - float.Parse(rangeMinText[1].text));
                greenImages[1].fillAmount = 0;
            }
            else
            {
                redImages[1].fillAmount = 1;
                greenImages[1].fillAmount = (float.Parse(rangeCMaxText[1].text) - ((float.Parse(rangeMinText[1].text) + float.Parse(rangeMaxText[1].text)) / 2)) / (((float.Parse(rangeMinText[1].text) + float.Parse(rangeMaxText[1].text)) / 2) - float.Parse(rangeMinText[1].text)) ;
            }
        }

        float sum = 0f;
        float average = 0f;


        for (int i = 0; i < RangeStockData.SelectToken("pagination.count").Value<float>(); i++)
        {
            if (RangeStockData.SelectToken("data.eod[" + i + "].volume").Value<string>() != null)
            {
                sum += RangeStockData.SelectToken("data.eod[" + i + "].volume").Value<float>();
            }
        }

        average = sum / RangeStockData.SelectToken("pagination.count").Value<float>();
        averageVolumeText[1].text = "" + Math.Round((average / 1000000f), 1) + "M";
    }

    IEnumerator GetTMonthRange(string stockID)
    {
        GetDate(90);
        UnityWebRequest wwwTMonthRange = UnityWebRequest.Get("https://api.marketstack.com/v1/tickers/" + stockID + "/eod?access_key=3c9529a8a320b0c5e8d8a0cf5a4d69e2" + "&offset = 0 & limit = 75");
        yield return wwwTMonthRange.SendWebRequest();

        var RangeStockData = (JObject)JsonConvert.DeserializeObject(wwwTMonthRange.downloadHandler.text);

        var lProperties = RangeStockData.DescendantsAndSelf()
                        .OfType<JProperty>()
                        .Where(p => p.Name == "low");
        var min = lProperties
                    .Aggregate((p1, p2) => p1.Value.Value<float>() < p2.Value.Value<float>() ? p1 : p2);

        //print(min.Value);

        var hProperties = RangeStockData.DescendantsAndSelf()
                        .OfType<JProperty>()
                        .Where(p => p.Name == "high");
        var max = hProperties
                    .Aggregate((p1, p2) => p1.Value.Value<float>() > p2.Value.Value<float>() ? p1 : p2);

        //print(max.Value);

        rangeCMinText[2].text = "" + min.Value;
        rangeCMaxText[2].text = "" + max.Value;

        greyImages[2].fillAmount = (float.Parse(rangeCMinText[2].text) - float.Parse(rangeMinText[2].text)) / (float.Parse(rangeMaxText[2].text) - float.Parse(rangeMinText[2].text));

        if (float.Parse(rangeCMinText[2].text) >= (float.Parse(rangeMinText[2].text) + float.Parse(rangeMaxText[2].text)) / 2)
        {
            redImages[2].fillAmount = 0;
            greenImages[2].fillAmount = (float.Parse(rangeCMaxText[2].text) - ((float.Parse(rangeMinText[2].text) + float.Parse(rangeMaxText[2].text)) / 2)) / (((float.Parse(rangeMinText[2].text) + float.Parse(rangeMaxText[2].text)) / 2) - float.Parse(rangeMinText[2].text));
        }
        else
        {
            if (float.Parse(rangeCMaxText[2].text) < (float.Parse(rangeMinText[2].text) + float.Parse(rangeMaxText[2].text)) / 2)
            {
                redImages[2].fillAmount = (float.Parse(rangeCMaxText[2].text) - float.Parse(rangeMinText[2].text)) / (((float.Parse(rangeMinText[2].text) + float.Parse(rangeMaxText[2].text)) / 2) - float.Parse(rangeMinText[2].text));
                greenImages[2].fillAmount = 0;
            }
            else
            {
                redImages[2].fillAmount = 1;
                greenImages[2].fillAmount = (float.Parse(rangeCMaxText[2].text) - ((float.Parse(rangeMinText[2].text) + float.Parse(rangeMaxText[2].text)) / 2)) / (((float.Parse(rangeMinText[2].text) + float.Parse(rangeMaxText[2].text)) / 2) - float.Parse(rangeMinText[2].text));
            }
        }

        float sum = 0f;
        float average = 0f;


        for (int i = 0; i < RangeStockData.SelectToken("pagination.count").Value<float>(); i++)
        {
            if (RangeStockData.SelectToken("data.eod[" + i + "].volume").Value<string>() != null)
            {
                sum += RangeStockData.SelectToken("data.eod[" + i + "].volume").Value<float>();
            }
        }

        average = sum / RangeStockData.SelectToken("pagination.count").Value<float>();
        averageVolumeText[2].text = "" + Math.Round((average / 1000000f), 1) + "M";
    }

    IEnumerator GetSMonthRange(string stockID)
    {
        GetDate(180);
        UnityWebRequest wwwSMonthRange = UnityWebRequest.Get("https://api.marketstack.com/v1/tickers/" + stockID + "/eod?access_key=3c9529a8a320b0c5e8d8a0cf5a4d69e2" + "&offset = 0 & limit = 150");
        yield return wwwSMonthRange.SendWebRequest();

        var RangeStockData = (JObject)JsonConvert.DeserializeObject(wwwSMonthRange.downloadHandler.text);

        var lProperties = RangeStockData.DescendantsAndSelf()
                        .OfType<JProperty>()
                        .Where(p => p.Name == "low");
        var min = lProperties
                    .Aggregate((p1, p2) => p1.Value.Value<float>() < p2.Value.Value<float>() ? p1 : p2);

        //print(min.Value);

        var hProperties = RangeStockData.DescendantsAndSelf()
                        .OfType<JProperty>()
                        .Where(p => p.Name == "high");
        var max = hProperties
                    .Aggregate((p1, p2) => p1.Value.Value<float>() > p2.Value.Value<float>() ? p1 : p2);

        //print(max.Value);

        rangeCMinText[3].text = "" + min.Value;
        rangeCMaxText[3].text = "" + max.Value;

        greyImages[3].fillAmount = (float.Parse(rangeCMinText[3].text) - float.Parse(rangeMinText[3].text)) / (float.Parse(rangeMaxText[3].text) - float.Parse(rangeMinText[3].text));

        if (float.Parse(rangeCMinText[3].text) >= (float.Parse(rangeMinText[3].text) + float.Parse(rangeMaxText[3].text)) / 2)
        {
            redImages[3].fillAmount = 0;
            greenImages[3].fillAmount = (float.Parse(rangeCMaxText[3].text) - ((float.Parse(rangeMinText[3].text) + float.Parse(rangeMaxText[3].text)) / 2)) / (((float.Parse(rangeMinText[3].text) + float.Parse(rangeMaxText[3].text)) / 2) - float.Parse(rangeMinText[3].text));
        }
        else
        {
            if (float.Parse(rangeCMaxText[3].text) < (float.Parse(rangeMinText[3].text) + float.Parse(rangeMaxText[3].text)) / 2)
            {
                redImages[3].fillAmount = (float.Parse(rangeCMaxText[3].text) - float.Parse(rangeMinText[3].text)) / (((float.Parse(rangeMinText[3].text) + float.Parse(rangeMaxText[3].text)) / 2) - float.Parse(rangeMinText[3].text));
                greenImages[3].fillAmount = 0;
            }
            else
            {
                redImages[3].fillAmount = 1;
                greenImages[3].fillAmount = (float.Parse(rangeCMaxText[3].text) - ((float.Parse(rangeMinText[3].text) + float.Parse(rangeMaxText[3].text)) / 2)) / (((float.Parse(rangeMinText[3].text) + float.Parse(rangeMaxText[3].text)) / 2) - float.Parse(rangeMinText[3].text));
            }
        }

        float sum = 0f;
        float average = 0f;


        for (int i = 0; i < RangeStockData.SelectToken("pagination.count").Value<float>(); i++)
        {
            if (RangeStockData.SelectToken("data.eod[" + i + "].volume").Value<string>() != null)
            {
                sum += RangeStockData.SelectToken("data.eod[" + i + "].volume").Value<float>();
            }
        }

        average = sum / RangeStockData.SelectToken("pagination.count").Value<float>();
        averageVolumeText[3].text = "" + Math.Round((average / 1000000f), 1) + "M";
    }

    IEnumerator TradingInfo(string stockID)
    {
        for (int i = 0; i < stocks.Length; i++)
        {
            stocks[i].SetActive(true);
            charts[i].SetActive(true);
        }

        switch (timeInterval.text)
        {
            case "1M":
                {
                    UnityWebRequest wwwTrading = UnityWebRequest.Get("https://api.marketstack.com/v1/tickers/" + stockID + "/intraday?access_key=3c9529a8a320b0c5e8d8a0cf5a4d69e2" + "&interval=1min&offset=0&limit=1000");
                    yield return wwwTrading.SendWebRequest();

                    var tradingStockData = (JObject)JsonConvert.DeserializeObject(wwwTrading.downloadHandler.text);
                    print(wwwTrading.downloadHandler.text);

                    if (tradingStockData.SelectToken("pagination.total").Value<float>() != 0)
                    {
                        for (int i = 0; i < stockLowValues.Length; i++)
                        {
                            stockLowValues[i] = tradingStockData.SelectToken("data.intraday[" + i + "].low").Value<float>();
                            stockOpenValues[i] = tradingStockData.SelectToken("data.intraday[" + i + "].open").Value<float>();
                            stockHighValues[i] = tradingStockData.SelectToken("data.intraday[" + i + "].high").Value<float>();

                            if (tradingStockData.SelectToken("data.intraday[" + i + "].close").Value<string>() != null && tradingStockData.SelectToken("data.intraday[" + i + "].volume").Value<string>() != null)
                            {
                                stockCloseValues[i] = tradingStockData.SelectToken("data.intraday[" + i + "].close").Value<float>();
                                stockVolumeValues[i] = tradingStockData.SelectToken("data.intraday[" + i + "].volume").Value<float>();
                            }
                            else if (tradingStockData.SelectToken("data.intraday[" + i + "].close").Value<string>() == null)
                            {
                                stockCloseValues[i] = stockOpenValues[i];
                            }
                            else if (tradingStockData.SelectToken("data.intraday[" + i + "].volume").Value<string>() == null)
                            {
                                stockVolumeValues[i] = 0;
                            }

                        }

                        GameObject[] lines = GameObject.FindGameObjectsWithTag("Clone");

                        for (int i = 0; i < lines.Length; i++)
                        {
                            Destroy(lines[i]);
                        }

                        ShowYPrice(stockLowValues.Min(), stockHighValues.Max());
                        ShowYVolume();
                    }
                    else
                    {
                        for (int i = 0; i < stocks.Length; i++)
                        {
                            stocks[i].SetActive(false);
                            charts[i].SetActive(false);
                        }
                    }
                    
                    break;
                }

            case "5M":
                {
                    UnityWebRequest wwwTrading = UnityWebRequest.Get("https://api.marketstack.com/v1/tickers/" + stockID + "/intraday?access_key=3c9529a8a320b0c5e8d8a0cf5a4d69e2" + "&interval=5min&offset=0&limit=1000");
                    yield return wwwTrading.SendWebRequest();

                    var tradingStockData = (JObject)JsonConvert.DeserializeObject(wwwTrading.downloadHandler.text);

                    if (tradingStockData.SelectToken("pagination.total").Value<float>() != 0)
                    {
                        for (int i = 0; i < stockLowValues.Length; i++)
                        {
                            stockLowValues[i] = tradingStockData.SelectToken("data.intraday[" + i + "].low").Value<float>();
                            stockOpenValues[i] = tradingStockData.SelectToken("data.intraday[" + i + "].open").Value<float>();
                            stockHighValues[i] = tradingStockData.SelectToken("data.intraday[" + i + "].high").Value<float>();

                            if (tradingStockData.SelectToken("data.intraday[" + i + "].close").Value<string>() != null && tradingStockData.SelectToken("data.intraday[" + i + "].volume").Value<string>() != null)
                            {
                                stockCloseValues[i] = tradingStockData.SelectToken("data.intraday[" + i + "].close").Value<float>();
                                stockVolumeValues[i] = tradingStockData.SelectToken("data.intraday[" + i + "].volume").Value<float>();
                            }
                            else if (tradingStockData.SelectToken("data.intraday[" + i + "].close").Value<string>() == null)
                            {
                                stockCloseValues[i] = stockOpenValues[i];
                            }
                            else if (tradingStockData.SelectToken("data.intraday[" + i + "].volume").Value<string>() == null)
                            {
                                stockVolumeValues[i] = 0;
                            }

                        }

                        GameObject[] lines = GameObject.FindGameObjectsWithTag("Clone");

                        for (int i = 0; i < lines.Length; i++)
                        {
                            Destroy(lines[i]);
                        }

                        ShowYPrice(stockLowValues.Min(), stockHighValues.Max());
                        ShowYVolume();
                    }
                    else
                    {
                        for (int i = 0; i < stocks.Length; i++)
                        {
                            stocks[i].SetActive(false);
                            charts[i].SetActive(false);
                        }
                    }

                    break;
                }

            case "15M":
                {
                    UnityWebRequest wwwTrading = UnityWebRequest.Get("https://api.marketstack.com/v1/tickers/" + stockID + "/intraday?access_key=3c9529a8a320b0c5e8d8a0cf5a4d69e2" + "&interval=15min&offset=0&limit=1000");
                    yield return wwwTrading.SendWebRequest();

                    var tradingStockData = (JObject)JsonConvert.DeserializeObject(wwwTrading.downloadHandler.text);

                    if (tradingStockData.SelectToken("pagination.total").Value<float>() != 0)
                    {
                        for (int i = 0; i < stockLowValues.Length; i++)
                        {
                            stockLowValues[i] = tradingStockData.SelectToken("data.intraday[" + i + "].low").Value<float>();
                            stockOpenValues[i] = tradingStockData.SelectToken("data.intraday[" + i + "].open").Value<float>();
                            stockHighValues[i] = tradingStockData.SelectToken("data.intraday[" + i + "].high").Value<float>();

                            if (tradingStockData.SelectToken("data.intraday[" + i + "].close").Value<string>() != null && tradingStockData.SelectToken("data.intraday[" + i + "].volume").Value<string>() != null)
                            {
                                stockCloseValues[i] = tradingStockData.SelectToken("data.intraday[" + i + "].close").Value<float>();
                                stockVolumeValues[i] = tradingStockData.SelectToken("data.intraday[" + i + "].volume").Value<float>();
                            }
                            else if (tradingStockData.SelectToken("data.intraday[" + i + "].close").Value<string>() == null)
                            {
                                stockCloseValues[i] = stockOpenValues[i];
                            }
                            else if (tradingStockData.SelectToken("data.intraday[" + i + "].volume").Value<string>() == null)
                            {
                                stockVolumeValues[i] = 0;
                            }

                        }

                        GameObject[] lines = GameObject.FindGameObjectsWithTag("Clone");

                        for (int i = 0; i < lines.Length; i++)
                        {
                            Destroy(lines[i]);
                        }

                        ShowYPrice(stockLowValues.Min(), stockHighValues.Max());
                        ShowYVolume();
                    }
                    else
                    {
                        for (int i = 0; i < stocks.Length; i++)
                        {
                            stocks[i].SetActive(false);
                            charts[i].SetActive(false);
                        }
                    }

                    break;
                }

            case "30M":
                {
                    UnityWebRequest wwwTrading = UnityWebRequest.Get("https://api.marketstack.com/v1/tickers/" + stockID + "/intraday?access_key=3c9529a8a320b0c5e8d8a0cf5a4d69e2" + "&interval=30min&offset=0&limit=1000");
                    yield return wwwTrading.SendWebRequest();

                    var tradingStockData = (JObject)JsonConvert.DeserializeObject(wwwTrading.downloadHandler.text);

                    if (tradingStockData.SelectToken("pagination.total").Value<float>() != 0)
                    {
                        for (int i = 0; i < stockLowValues.Length; i++)
                        {
                            stockLowValues[i] = tradingStockData.SelectToken("data.intraday[" + i + "].low").Value<float>();
                            stockOpenValues[i] = tradingStockData.SelectToken("data.intraday[" + i + "].open").Value<float>();
                            stockHighValues[i] = tradingStockData.SelectToken("data.intraday[" + i + "].high").Value<float>();

                            if (tradingStockData.SelectToken("data.intraday[" + i + "].close").Value<string>() != null && tradingStockData.SelectToken("data.intraday[" + i + "].volume").Value<string>() != null)
                            {
                                stockCloseValues[i] = tradingStockData.SelectToken("data.intraday[" + i + "].close").Value<float>();
                                stockVolumeValues[i] = tradingStockData.SelectToken("data.intraday[" + i + "].volume").Value<float>();
                            }
                            else if (tradingStockData.SelectToken("data.intraday[" + i + "].close").Value<string>() == null)
                            {
                                stockCloseValues[i] = stockOpenValues[i];
                            }
                            else if (tradingStockData.SelectToken("data.intraday[" + i + "].volume").Value<string>() == null)
                            {
                                stockVolumeValues[i] = 0;
                            }

                        }

                        GameObject[] lines = GameObject.FindGameObjectsWithTag("Clone");

                        for (int i = 0; i < lines.Length; i++)
                        {
                            Destroy(lines[i]);
                        }

                        ShowYPrice(stockLowValues.Min(), stockHighValues.Max());
                        ShowYVolume();
                    }
                    else
                    {
                        for (int i = 0; i < stocks.Length; i++)
                        {
                            stocks[i].SetActive(false);
                            charts[i].SetActive(false);
                        }
                    }

                    break;
                }

            case "Hr":
                {
                    UnityWebRequest wwwTrading = UnityWebRequest.Get("https://api.marketstack.com/v1/tickers/" + stockID + "/intraday?access_key=3c9529a8a320b0c5e8d8a0cf5a4d69e2" + "&interval=1hour&offset=0&limit=1000");
                    yield return wwwTrading.SendWebRequest();

                    var tradingStockData = (JObject)JsonConvert.DeserializeObject(wwwTrading.downloadHandler.text);

                    if (tradingStockData.SelectToken("pagination.total").Value<float>() != 0)
                    {
                        for (int i = 0; i < stockLowValues.Length; i++)
                        {
                            stockLowValues[i] = tradingStockData.SelectToken("data.intraday[" + i + "].low").Value<float>();
                            stockOpenValues[i] = tradingStockData.SelectToken("data.intraday[" + i + "].open").Value<float>();
                            stockHighValues[i] = tradingStockData.SelectToken("data.intraday[" + i + "].high").Value<float>();

                            if (tradingStockData.SelectToken("data.intraday[" + i + "].close").Value<string>() != null && tradingStockData.SelectToken("data.intraday[" + i + "].volume").Value<string>() != null)
                            {
                                stockCloseValues[i] = tradingStockData.SelectToken("data.intraday[" + i + "].close").Value<float>();
                                stockVolumeValues[i] = tradingStockData.SelectToken("data.intraday[" + i + "].volume").Value<float>();
                            }
                            else if (tradingStockData.SelectToken("data.intraday[" + i + "].close").Value<string>() == null)
                            {
                                stockCloseValues[i] = stockOpenValues[i];
                            }
                            else if (tradingStockData.SelectToken("data.intraday[" + i + "].volume").Value<string>() == null)
                            {
                                stockVolumeValues[i] = 0;
                            }

                        }

                        GameObject[] lines = GameObject.FindGameObjectsWithTag("Clone");

                        for (int i = 0; i < lines.Length; i++)
                        {
                            Destroy(lines[i]);
                        }

                        ShowYPrice(stockLowValues.Min(), stockHighValues.Max());
                        ShowYVolume();
                    }
                    else
                    {
                        for (int i = 0; i < stocks.Length; i++)
                        {
                            stocks[i].SetActive(false);
                            charts[i].SetActive(false);
                        }
                    }

                    break;
                }
            case "H4":
                {
                    UnityWebRequest wwwTrading = UnityWebRequest.Get("https://api.marketstack.com/v1/tickers/" + stockID + "/intraday?access_key=3c9529a8a320b0c5e8d8a0cf5a4d69e2" + "&interval=3hour&offset=0&limit=1000");
                    yield return wwwTrading.SendWebRequest();

                    var tradingStockData = (JObject)JsonConvert.DeserializeObject(wwwTrading.downloadHandler.text);

                    if (tradingStockData.SelectToken("pagination.total").Value<float>() != 0)
                    {
                        for (int i = 0; i < stockLowValues.Length; i++)
                        {
                            stockLowValues[i] = tradingStockData.SelectToken("data.intraday[" + i + "].low").Value<float>();
                            stockOpenValues[i] = tradingStockData.SelectToken("data.intraday[" + i + "].open").Value<float>();
                            stockHighValues[i] = tradingStockData.SelectToken("data.intraday[" + i + "].high").Value<float>();

                            if (tradingStockData.SelectToken("data.intraday[" + i + "].close").Value<string>() != null && tradingStockData.SelectToken("data.intraday[" + i + "].volume").Value<string>() != null)
                            {
                                stockCloseValues[i] = tradingStockData.SelectToken("data.intraday[" + i + "].close").Value<float>();
                                stockVolumeValues[i] = tradingStockData.SelectToken("data.intraday[" + i + "].volume").Value<float>();
                            }
                            else if (tradingStockData.SelectToken("data.intraday[" + i + "].close").Value<string>() == null)
                            {
                                stockCloseValues[i] = stockOpenValues[i];
                            }
                            else if (tradingStockData.SelectToken("data.intraday[" + i + "].volume").Value<string>() == null)
                            {
                                stockVolumeValues[i] = 0;
                            }

                        }

                        GameObject[] lines = GameObject.FindGameObjectsWithTag("Clone");

                        for (int i = 0; i < lines.Length; i++)
                        {
                            Destroy(lines[i]);
                        }

                        ShowYPrice(stockLowValues.Min(), stockHighValues.Max());
                        ShowYVolume();
                    }
                    else
                    {
                        for (int i = 0; i < stocks.Length; i++)
                        {
                            stocks[i].SetActive(false);
                            charts[i].SetActive(false);
                        }
                    }

                    break;
                }

            case "D+":
                {
                    UnityWebRequest wwwTrading = UnityWebRequest.Get("https://api.marketstack.com/v1/tickers/" + stockID + "/eod?access_key=3c9529a8a320b0c5e8d8a0cf5a4d69e2" + "&offset=0&limit=1000");
                    yield return wwwTrading.SendWebRequest();

                    var tradingStockData = (JObject)JsonConvert.DeserializeObject(wwwTrading.downloadHandler.text);

                    for (int i = 0; i < stockLowValues.Length; i++)
                    {
                        stockLowValues[i] = tradingStockData.SelectToken("data.eod[" + i + "].low").Value<float>();
                        stockOpenValues[i] = tradingStockData.SelectToken("data.eod[" + i + "].open").Value<float>();
                        stockHighValues[i] = tradingStockData.SelectToken("data.eod[" + i + "].high").Value<float>();
                        stockCloseValues[i] = tradingStockData.SelectToken("data.eod[" + i + "].close").Value<float>();
                        stockVolumeValues[i] = tradingStockData.SelectToken("data.eod[" + i + "].volume").Value<float>();
                    }

                    //var lProperties = tradingStockData.DescendantsAndSelf()
                    //               .OfType<JProperty>()
                    //               .Where(p => p.Name == "low");
                    //var min = lProperties
                    //            .Aggregate((p1, p2) => p1.Value.Value<float>() < p2.Value.Value<float>() ? p1 : p2);

                    ////print(min.Value);

                    //var hProperties = tradingStockData.DescendantsAndSelf()
                    //                .OfType<JProperty>()
                    //                .Where(p => p.Name == "high");
                    //var max = hProperties
                    //            .Aggregate((p1, p2) => p1.Value.Value<float>() > p2.Value.Value<float>() ? p1 : p2);

                    GameObject[] lines = GameObject.FindGameObjectsWithTag("Clone");

                    for (int i = 0; i < lines.Length; i++)
                    {
                        Destroy(lines[i]);
                    }

                    ShowYPrice(stockLowValues.Min(), stockHighValues.Max());
                    ShowYVolume();
                    break;
                }

            case "D":
                {
                    UnityWebRequest wwwTrading = UnityWebRequest.Get("https://api.marketstack.com/v1/tickers/" + stockID + "/eod?access_key=3c9529a8a320b0c5e8d8a0cf5a4d69e2" + "&offset=0&limit=1000");
                    yield return wwwTrading.SendWebRequest();

                    var tradingStockData = (JObject)JsonConvert.DeserializeObject(wwwTrading.downloadHandler.text);

                    for (int i = 0; i < stockLowValues.Length; i++)
                    {
                        stockLowValues[i] = tradingStockData.SelectToken("data.eod[" + i + "].low").Value<float>();
                        stockOpenValues[i] = tradingStockData.SelectToken("data.eod[" + i + "].open").Value<float>();
                        stockHighValues[i] = tradingStockData.SelectToken("data.eod[" + i + "].high").Value<float>();
                        stockCloseValues[i] = tradingStockData.SelectToken("data.eod[" + i + "].close").Value<float>();
                        stockVolumeValues[i] = tradingStockData.SelectToken("data.eod[" + i + "].volume").Value<float>();
                    }                    

                    GameObject[] lines = GameObject.FindGameObjectsWithTag("Clone");

                    for (int i = 0; i < lines.Length; i++)
                    {
                        Destroy(lines[i]);
                    }

                    ShowYPrice(stockLowValues.Min(), stockHighValues.Max());
                    ShowYVolume();
                    break;
                }

            case "W+":
                {
                    UnityWebRequest wwwTrading = UnityWebRequest.Get("https://api.marketstack.com/v1/tickers/" + stockID + "/eod?access_key=3c9529a8a320b0c5e8d8a0cf5a4d69e2" + "&offset=0&limit=1000");
                    yield return wwwTrading.SendWebRequest();

                    var tradingStockData = (JObject)JsonConvert.DeserializeObject(wwwTrading.downloadHandler.text);

                    for (int i = 0; i < stockLowValues.Length; i++)
                    {
                        stockLowValues[i] = tradingStockData.SelectToken("data.eod[" + i * 5 + "].low").Value<float>();
                        stockOpenValues[i] = tradingStockData.SelectToken("data.eod[" + i * 5 + "].open").Value<float>();
                        stockHighValues[i] = tradingStockData.SelectToken("data.eod[" + i * 5 + "].high").Value<float>();
                        stockCloseValues[i] = tradingStockData.SelectToken("data.eod[" + i * 5 + "].close").Value<float>();
                        stockVolumeValues[i] = tradingStockData.SelectToken("data.eod[" + i * 5 + "].volume").Value<float>();
                    }                   

                    //print(max.Value);

                    GameObject[] lines = GameObject.FindGameObjectsWithTag("Clone");

                    for (int i = 0; i < lines.Length; i++)
                    {
                        Destroy(lines[i]);
                    }

                    ShowYPrice(stockLowValues.Min(), stockHighValues.Max());
                    ShowYVolume();
                    break;                    
                }

            case "W":
                {
                    UnityWebRequest wwwTrading = UnityWebRequest.Get("https://api.marketstack.com/v1/tickers/" + stockID + "/eod?access_key=3c9529a8a320b0c5e8d8a0cf5a4d69e2&offset=0&limit=1000");
                    yield return wwwTrading.SendWebRequest();

                    var tradingStockData = (JObject)JsonConvert.DeserializeObject(wwwTrading.downloadHandler.text);
                    print(wwwTrading.downloadHandler.text);
                    for (int i = 0; i < stockLowValues.Length; i++)
                    {
                        stockLowValues[i] = tradingStockData.SelectToken("data.eod[" + i * 5 + "].low").Value<float>();
                        stockOpenValues[i] = tradingStockData.SelectToken("data.eod[" + i * 5 + "].open").Value<float>();
                        stockHighValues[i] = tradingStockData.SelectToken("data.eod[" + i * 5 + "].high").Value<float>();
                        stockCloseValues[i] = tradingStockData.SelectToken("data.eod[" + i * 5 + "].close").Value<float>();
                        stockVolumeValues[i] = tradingStockData.SelectToken("data.eod[" + i * 5 + "].volume").Value<float>();
                    }                    

                    GameObject[] lines = GameObject.FindGameObjectsWithTag("Clone");

                    for (int i = 0; i < lines.Length; i++)
                    {
                        Destroy(lines[i]);
                    }

                    ShowYPrice(stockLowValues.Min(), stockHighValues.Max());
                    ShowYVolume();
                    break;
                }
            case "M+":
                {
                    UnityWebRequest wwwTrading = UnityWebRequest.Get("https://api.marketstack.com/v1/tickers/" + stockID + "/eod?access_key=3c9529a8a320b0c5e8d8a0cf5a4d69e2" + "&offset=0&limit=1000");
                    yield return wwwTrading.SendWebRequest();

                    var tradingStockData = (JObject)JsonConvert.DeserializeObject(wwwTrading.downloadHandler.text);
                    print(wwwTrading.downloadHandler.text);

                    for (int i = 0; i < 40; i++)
                    {
                        stockLowValues[i] = tradingStockData.SelectToken("data.eod[" + i * 25 + "].low").Value<float>();
                        stockOpenValues[i] = tradingStockData.SelectToken("data.eod[" + i * 25 + "].open").Value<float>();
                        stockHighValues[i] = tradingStockData.SelectToken("data.eod[" + i * 25 + "].high").Value<float>();
                        stockCloseValues[i] = tradingStockData.SelectToken("data.eod[" + i * 25 + "].close").Value<float>();
                        stockVolumeValues[i] = tradingStockData.SelectToken("data.eod[" + i * 25 + "].volume").Value<float>();
                    }

                    UnityWebRequest wwwTrading1 = UnityWebRequest.Get("https://api.marketstack.com/v1/tickers/" + stockID + "/eod?access_key=3c9529a8a320b0c5e8d8a0cf5a4d69e2" + "&offset=1000&limit=1000");
                    yield return wwwTrading1.SendWebRequest();

                    for (int i = 40; i < stockHighValues.Length; i++)
                    {
                        stockLowValues[i] = tradingStockData.SelectToken("data.eod[" + (i - 40) * 25 + "].low").Value<float>();
                        stockOpenValues[i] = tradingStockData.SelectToken("data.eod[" + (i - 40) * 25 + "].open").Value<float>();
                        stockHighValues[i] = tradingStockData.SelectToken("data.eod[" + (i - 40) * 25 + "].high").Value<float>();
                        stockCloseValues[i] = tradingStockData.SelectToken("data.eod[" + (i - 40) * 25 + "].close").Value<float>();
                        stockVolumeValues[i] = tradingStockData.SelectToken("data.eod[" + (i - 40) * 25 + "].volume").Value<float>();
                    }

                    var tradingStockData1 = (JObject)JsonConvert.DeserializeObject(wwwTrading1.downloadHandler.text);
                    //print(max.Value);

                    GameObject[] lines = GameObject.FindGameObjectsWithTag("Clone");

                    for (int i = 0; i < lines.Length; i++)
                    {
                        Destroy(lines[i]);
                    }

                    ShowYPrice(stockLowValues.Min(), stockHighValues.Max());
                    ShowYVolume();
                    break;
                }

            case "M":
                {
                    UnityWebRequest wwwTrading = UnityWebRequest.Get("https://api.marketstack.com/v1/tickers/" + stockID + "/eod?access_key=3c9529a8a320b0c5e8d8a0cf5a4d69e2" + "&offset=0&limit=1000");
                    yield return wwwTrading.SendWebRequest();

                    var tradingStockData = (JObject)JsonConvert.DeserializeObject(wwwTrading.downloadHandler.text);
                    print(wwwTrading.downloadHandler.text);

                    for (int i = 0; i < 40; i++)
                    {
                        stockLowValues[i] = tradingStockData.SelectToken("data.eod[" + i * 25 + "].low").Value<float>();
                        stockOpenValues[i] = tradingStockData.SelectToken("data.eod[" + i * 25 + "].open").Value<float>();
                        stockHighValues[i] = tradingStockData.SelectToken("data.eod[" + i * 25 + "].high").Value<float>();
                        stockCloseValues[i] = tradingStockData.SelectToken("data.eod[" + i * 25 + "].close").Value<float>();
                        stockVolumeValues[i] = tradingStockData.SelectToken("data.eod[" + i * 25 + "].volume").Value<float>();
                    }

                    UnityWebRequest wwwTrading1 = UnityWebRequest.Get("https://api.marketstack.com/v1/tickers/" + stockID + "/eod?access_key=3c9529a8a320b0c5e8d8a0cf5a4d69e2" + "&offset=1000&limit=1000");
                    yield return wwwTrading1.SendWebRequest();

                    for (int i = 40; i < stockHighValues.Length; i++)
                    {
                        stockLowValues[i] = tradingStockData.SelectToken("data.eod[" + (i - 40) * 25 + "].low").Value<float>();
                        stockOpenValues[i] = tradingStockData.SelectToken("data.eod[" + (i - 40) * 25 + "].open").Value<float>();
                        stockHighValues[i] = tradingStockData.SelectToken("data.eod[" + (i - 40) * 25 + "].high").Value<float>();
                        stockCloseValues[i] = tradingStockData.SelectToken("data.eod[" + (i - 40) * 25 + "].close").Value<float>();
                        stockVolumeValues[i] = tradingStockData.SelectToken("data.eod[" + (i - 40) * 25 + "].volume").Value<float>();
                    }

                    var tradingStockData1 = (JObject)JsonConvert.DeserializeObject(wwwTrading1.downloadHandler.text);
                    //print(max.Value);

                    GameObject[] lines = GameObject.FindGameObjectsWithTag("Clone");

                    for (int i = 0; i < lines.Length; i++)
                    {
                        Destroy(lines[i]);
                    }

                    ShowYPrice(stockLowValues.Min(), stockHighValues.Max());
                    ShowYVolume();
                    break;
                }
        }                             
    }

    IEnumerator ShowBBMA()
    {
        switch (timeInterval.text)
        {
            case "1M":
                {
                    UnityWebRequest wwwTrading = UnityWebRequest.Get("https://api.marketstack.com/v1/tickers/" + selectedStockID + "/intraday?access_key=3c9529a8a320b0c5e8d8a0cf5a4d69e2" + "&interval=1min&offset=0&limit=1000");
                    yield return wwwTrading.SendWebRequest();

                    var tradingStockData = (JObject)JsonConvert.DeserializeObject(wwwTrading.downloadHandler.text);
                    print(wwwTrading.downloadHandler.text);

                    //Middle Band
                    float sumDeviation0 = 0f;
                    float sumDeviation1 = 0f;

                    for (int i = 0; i < stockLowValues.Length - 1; i++)
                    {
                        float sum0 = 0f;
                        float average0 = 0f;
                        float sum1 = 0f;
                        float average1 = 0f;
                        int index0 = 0;
                        int index1 = 0;

                        for (int j = 0; j < shortValue; j++)
                        {
                            if (tradingStockData.SelectToken("data.intraday[" + (i + j) + "].close").Value<string>() != null)
                            {
                                index0++;
                                sum0 += tradingStockData.SelectToken("data.intraday[" + (i + j) + "].close").Value<float>();
                            }
                            else
                            {
                            }
                        }

                        for (int j = 1; j <= shortValue; j++)
                        {
                            if (tradingStockData.SelectToken("data.intraday[" + (i + j) + "].close").Value<string>() != null)
                            {
                                index1++;
                                sum1 += tradingStockData.SelectToken("data.intraday[" + (i + j) + "].close").Value<float>();
                            }
                            else
                            {
                            }
                        }

                        if (index0 != 0)
                        {
                            average0 = sum0 / index0;
                        }

                        if (index1 != 0)
                        {
                            average1 = sum1 / index1;
                        }

                        sumDeviation0 += average0;
                        //print(average0);
                        //print(average1);
                        float yPosition = yPrice[yPrice.Length - 1].transform.position.y - yPrice[0].transform.position.y;
                        MakeLine(stocks[59 - i].transform.position.x * 2532f / Screen.width,
                            (((average0 - stockLowValues.Min()) / (stockHighValues.Max() - stockLowValues.Min())) * yPosition + yPrice[0].transform.position.y) * 1170f / Screen.height,
                            stocks[58 - i].transform.position.x * 2532f / Screen.width,
                            (((average1 - stockLowValues.Min()) / (stockHighValues.Max() - stockLowValues.Min())) * yPosition + yPrice[0].transform.position.y) * 1170f / Screen.height,
                            UnityEngine.Color.green);
                    }

                    for (int i = 0; i < stockLowValues.Length - 1; i++)
                    {
                        float sum0 = 0f;
                        float average0 = 0f;
                        float sum1 = 0f;
                        float average1 = 0f;
                        int index0 = 0;
                        int index1 = 0;

                        for (int j = 0; j < shortValue; j++)
                        {
                            if (tradingStockData.SelectToken("data.intraday[" + (i + j) + "].close").Value<string>() != null)
                            {
                                index0++;
                                sum0 += tradingStockData.SelectToken("data.intraday[" + (i + j) + "].close").Value<float>();
                            }
                            else
                            {
                            }
                        }

                        for (int j = 1; j <= shortValue; j++)
                        {
                            if (tradingStockData.SelectToken("data.intraday[" + (i + j) + "].close").Value<string>() != null)
                            {
                                index1++;
                                sum1 += tradingStockData.SelectToken("data.intraday[" + (i + j) + "].close").Value<float>();
                            }
                            else
                            {
                            }
                        }

                        if (index0 != 0)
                        {
                            average0 = sum0 / index0;
                        }

                        if (index1 != 0)
                        {
                            average1 = sum1 / index1;
                        }

                        sumDeviation1 += Mathf.Pow((sumDeviation0 / 59f - average0), 2);
                    }

                    for (int i = 0; i < stockLowValues.Length - 1; i++)
                    {
                        float sum0 = 0f;
                        float average0 = 0f;
                        float sum1 = 0f;
                        float average1 = 0f;
                        int index0 = 0;
                        int index1 = 0;

                        for (int j = 0; j < shortValue; j++)
                        {
                            if (tradingStockData.SelectToken("data.intraday[" + (i + j) + "].close").Value<string>() != null)
                            {
                                index0++;
                                sum0 += tradingStockData.SelectToken("data.intraday[" + (i + j) + "].close").Value<float>();
                            }
                            else
                            {
                            }
                        }

                        for (int j = 1; j <= shortValue; j++)
                        {
                            if (tradingStockData.SelectToken("data.intraday[" + (i + j) + "].close").Value<string>() != null)
                            {
                                index1++;
                                sum1 += tradingStockData.SelectToken("data.intraday[" + (i + j) + "].close").Value<float>();
                            }
                            else
                            {
                            }
                        }

                        if (index0 != 0)
                        {
                            average0 = sum0 / index0;
                        }

                        if (index1 != 0)
                        {
                            average1 = sum1 / index1;
                        }
                        //print(average0);
                        //print(average1);
                        float yPosition = yPrice[yPrice.Length - 1].transform.position.y - yPrice[0].transform.position.y;
                        MakeLine(stocks[59 - i].transform.position.x * 2532f / Screen.width,
                            (((average0 + 0.2f * Mathf.Sqrt(sumDeviation1) - stockLowValues.Min()) / (stockHighValues.Max() - stockLowValues.Min())) * yPosition + yPrice[0].transform.position.y) * 1170f / Screen.height,
                            stocks[58 - i].transform.position.x * 2532f / Screen.width,
                            (((average1 + 0.2f * Mathf.Sqrt(sumDeviation1) - stockLowValues.Min()) / (stockHighValues.Max() - stockLowValues.Min())) * yPosition + yPrice[0].transform.position.y) * 1170f / Screen.height,
                            UnityEngine.Color.yellow);

                        MakeLine(stocks[59 - i].transform.position.x * 2532f / Screen.width,
                            (((average0 - 0.2f * Mathf.Sqrt(sumDeviation1) - stockLowValues.Min()) / (stockHighValues.Max() - stockLowValues.Min())) * yPosition + yPrice[0].transform.position.y) * 1170f / Screen.height,
                            stocks[58 - i].transform.position.x * 2532f / Screen.width,
                            (((average1 - 0.2f * Mathf.Sqrt(sumDeviation1) - stockLowValues.Min()) / (stockHighValues.Max() - stockLowValues.Min())) * yPosition + yPrice[0].transform.position.y) * 1170f / Screen.height,
                            UnityEngine.Color.blue);
                    }

                    //BBMA
                    for (int i = 0; i < stockLowValues.Length - 1; i++)
                    {
                        float sum0 = 0f;
                        float average0 = 0f;
                        float sum1 = 0f;
                        float average1 = 0f;
                        int index0 = 0;
                        int index1 = 0;

                        for (int j = 0; j < longValue; j++)
                        {
                            if (tradingStockData.SelectToken("data.intraday[" + (i + j) + "].close").Value<string>() != null)
                            {
                                index0++;
                                sum0 += tradingStockData.SelectToken("data.intraday[" + (i + j) + "].close").Value<float>();
                            }
                            else
                            {
                            }
                        }

                        for (int j = 1; j <= longValue; j++)
                        {
                            if (tradingStockData.SelectToken("data.intraday[" + (i + j) + "].close").Value<string>() != null)
                            {
                                index1++;
                                sum1 += tradingStockData.SelectToken("data.intraday[" + (i + j) + "].close").Value<float>();
                            }
                            else
                            {
                            }
                        }

                        if (index0 != 0)
                        {
                            average0 = sum0 / index0;
                        }

                        if (index1 != 0)
                        {
                            average1 = sum1 / index1;
                        }
                        //print(average0);
                        //print(average1);
                        float yPosition = yPrice[yPrice.Length - 1].transform.position.y - yPrice[0].transform.position.y;
                        MakeLine(stocks[59 - i].transform.position.x * 2532f / Screen.width,
                            (((average0 - stockLowValues.Min()) / (stockHighValues.Max() - stockLowValues.Min())) * yPosition + yPrice[0].transform.position.y) * 1170f / Screen.height,
                            stocks[58 - i].transform.position.x * 2532f / Screen.width,
                            (((average1 - stockLowValues.Min()) / (stockHighValues.Max() - stockLowValues.Min())) * yPosition + yPrice[0].transform.position.y) * 1170f / Screen.height,
                            UnityEngine.Color.red);
                    }

                    break;
                }

            case "5M":
                {
                    UnityWebRequest wwwTrading = UnityWebRequest.Get("https://api.marketstack.com/v1/tickers/" + selectedStockID + "/intraday?access_key=3c9529a8a320b0c5e8d8a0cf5a4d69e2" + "&interval=5min&offset=0&limit=1000");
                    yield return wwwTrading.SendWebRequest();

                    var tradingStockData = (JObject)JsonConvert.DeserializeObject(wwwTrading.downloadHandler.text);
                    print(wwwTrading.downloadHandler.text);

                    //Middle Band
                    float sumDeviation0 = 0f;
                    float sumDeviation1 = 0f;

                    for (int i = 0; i < stockLowValues.Length - 1; i++)
                    {
                        float sum0 = 0f;
                        float average0 = 0f;
                        float sum1 = 0f;
                        float average1 = 0f;
                        int index0 = 0;
                        int index1 = 0;

                        for (int j = 0; j < shortValue; j++)
                        {
                            if (tradingStockData.SelectToken("data.intraday[" + (i + j) + "].close").Value<string>() != null)
                            {
                                index0++;
                                sum0 += tradingStockData.SelectToken("data.intraday[" + (i + j) + "].close").Value<float>();
                            }
                            else
                            {
                            }
                        }

                        for (int j = 1; j <= shortValue; j++)
                        {
                            if (tradingStockData.SelectToken("data.intraday[" + (i + j) + "].close").Value<string>() != null)
                            {
                                index1++;
                                sum1 += tradingStockData.SelectToken("data.intraday[" + (i + j) + "].close").Value<float>();
                            }
                            else
                            {
                            }
                        }

                        if (index0 != 0)
                        {
                            average0 = sum0 / index0;
                        }

                        if (index1 != 0)
                        {
                            average1 = sum1 / index1;
                        }

                        sumDeviation0 += average0;
                        //print(average0);
                        //print(average1);
                        float yPosition = yPrice[yPrice.Length - 1].transform.position.y - yPrice[0].transform.position.y;
                        MakeLine(stocks[59 - i].transform.position.x * 2532f / Screen.width,
                            (((average0 - stockLowValues.Min()) / (stockHighValues.Max() - stockLowValues.Min())) * yPosition + yPrice[0].transform.position.y) * 1170f / Screen.height,
                            stocks[58 - i].transform.position.x * 2532f / Screen.width,
                            (((average1 - stockLowValues.Min()) / (stockHighValues.Max() - stockLowValues.Min())) * yPosition + yPrice[0].transform.position.y) * 1170f / Screen.height,
                            UnityEngine.Color.green);
                    }

                    for (int i = 0; i < stockLowValues.Length - 1; i++)
                    {
                        float sum0 = 0f;
                        float average0 = 0f;
                        float sum1 = 0f;
                        float average1 = 0f;
                        int index0 = 0;
                        int index1 = 0;

                        for (int j = 0; j < shortValue; j++)
                        {
                            if (tradingStockData.SelectToken("data.intraday[" + (i + j) + "].close").Value<string>() != null)
                            {
                                index0++;
                                sum0 += tradingStockData.SelectToken("data.intraday[" + (i + j) + "].close").Value<float>();
                            }
                            else
                            {
                            }
                        }

                        for (int j = 1; j <= shortValue; j++)
                        {
                            if (tradingStockData.SelectToken("data.intraday[" + (i + j) + "].close").Value<string>() != null)
                            {
                                index1++;
                                sum1 += tradingStockData.SelectToken("data.intraday[" + (i + j) + "].close").Value<float>();
                            }
                            else
                            {
                            }
                        }

                        if (index0 != 0)
                        {
                            average0 = sum0 / index0;
                        }

                        if (index1 != 0)
                        {
                            average1 = sum1 / index1;
                        }

                        sumDeviation1 += Mathf.Pow((sumDeviation0 / 59f - average0), 2);
                    }

                    for (int i = 0; i < stockLowValues.Length - 1; i++)
                    {
                        float sum0 = 0f;
                        float average0 = 0f;
                        float sum1 = 0f;
                        float average1 = 0f;
                        int index0 = 0;
                        int index1 = 0;

                        for (int j = 0; j < shortValue; j++)
                        {
                            if (tradingStockData.SelectToken("data.intraday[" + (i + j) + "].close").Value<string>() != null)
                            {
                                index0++;
                                sum0 += tradingStockData.SelectToken("data.intraday[" + (i + j) + "].close").Value<float>();
                            }
                            else
                            {
                            }
                        }

                        for (int j = 1; j <= shortValue; j++)
                        {
                            if (tradingStockData.SelectToken("data.intraday[" + (i + j) + "].close").Value<string>() != null)
                            {
                                index1++;
                                sum1 += tradingStockData.SelectToken("data.intraday[" + (i + j) + "].close").Value<float>();
                            }
                            else
                            {
                            }
                        }

                        if (index0 != 0)
                        {
                            average0 = sum0 / index0;
                        }

                        if (index1 != 0)
                        {
                            average1 = sum1 / index1;
                        }
                        //print(average0);
                        //print(average1);
                        float yPosition = yPrice[yPrice.Length - 1].transform.position.y - yPrice[0].transform.position.y;
                        MakeLine(stocks[59 - i].transform.position.x * 2532f / Screen.width,
                            (((average0 + 0.2f * Mathf.Sqrt(sumDeviation1) - stockLowValues.Min()) / (stockHighValues.Max() - stockLowValues.Min())) * yPosition + yPrice[0].transform.position.y) * 1170f / Screen.height,
                            stocks[58 - i].transform.position.x * 2532f / Screen.width,
                            (((average1 + 0.2f * Mathf.Sqrt(sumDeviation1) - stockLowValues.Min()) / (stockHighValues.Max() - stockLowValues.Min())) * yPosition + yPrice[0].transform.position.y) * 1170f / Screen.height,
                            UnityEngine.Color.yellow);

                        MakeLine(stocks[59 - i].transform.position.x * 2532f / Screen.width,
                            (((average0 - 0.2f * Mathf.Sqrt(sumDeviation1) - stockLowValues.Min()) / (stockHighValues.Max() - stockLowValues.Min())) * yPosition + yPrice[0].transform.position.y) * 1170f / Screen.height,
                            stocks[58 - i].transform.position.x * 2532f / Screen.width,
                            (((average1 - 0.2f * Mathf.Sqrt(sumDeviation1) - stockLowValues.Min()) / (stockHighValues.Max() - stockLowValues.Min())) * yPosition + yPrice[0].transform.position.y) * 1170f / Screen.height,
                            UnityEngine.Color.blue);
                    }

                    //BBMA
                    for (int i = 0; i < stockLowValues.Length - 1; i++)
                    {
                        float sum0 = 0f;
                        float average0 = 0f;
                        float sum1 = 0f;
                        float average1 = 0f;
                        int index0 = 0;
                        int index1 = 0;

                        for (int j = 0; j < longValue; j++)
                        {
                            if (tradingStockData.SelectToken("data.intraday[" + (i + j) + "].close").Value<string>() != null)
                            {
                                index0++;
                                sum0 += tradingStockData.SelectToken("data.intraday[" + (i + j) + "].close").Value<float>();
                            }
                            else
                            {
                            }
                        }

                        for (int j = 1; j <= longValue; j++)
                        {
                            if (tradingStockData.SelectToken("data.intraday[" + (i + j) + "].close").Value<string>() != null)
                            {
                                index1++;
                                sum1 += tradingStockData.SelectToken("data.intraday[" + (i + j) + "].close").Value<float>();
                            }
                            else
                            {
                            }
                        }

                        if (index0 != 0)
                        {
                            average0 = sum0 / index0;
                        }

                        if (index1 != 0)
                        {
                            average1 = sum1 / index1;
                        }
                        //print(average0);
                        //print(average1);
                        float yPosition = yPrice[yPrice.Length - 1].transform.position.y - yPrice[0].transform.position.y;
                        MakeLine(stocks[59 - i].transform.position.x * 2532f / Screen.width,
                            (((average0 - stockLowValues.Min()) / (stockHighValues.Max() - stockLowValues.Min())) * yPosition + yPrice[0].transform.position.y) * 1170f / Screen.height,
                            stocks[58 - i].transform.position.x * 2532f / Screen.width,
                            (((average1 - stockLowValues.Min()) / (stockHighValues.Max() - stockLowValues.Min())) * yPosition + yPrice[0].transform.position.y) * 1170f / Screen.height,
                            UnityEngine.Color.red);
                    }

                    break;
                }

            case "15M":
                {
                    UnityWebRequest wwwTrading = UnityWebRequest.Get("https://api.marketstack.com/v1/tickers/" + selectedStockID + "/intraday?access_key=3c9529a8a320b0c5e8d8a0cf5a4d69e2" + "&interval=15min&offset=0&limit=1000");
                    yield return wwwTrading.SendWebRequest();

                    var tradingStockData = (JObject)JsonConvert.DeserializeObject(wwwTrading.downloadHandler.text);
                    print(wwwTrading.downloadHandler.text);

                    //Middle Band
                    float sumDeviation0 = 0f;
                    float sumDeviation1 = 0f;

                    for (int i = 0; i < stockLowValues.Length - 1; i++)
                    {
                        float sum0 = 0f;
                        float average0 = 0f;
                        float sum1 = 0f;
                        float average1 = 0f;
                        int index0 = 0;
                        int index1 = 0;

                        for (int j = 0; j < shortValue; j++)
                        {
                            if (tradingStockData.SelectToken("data.intraday[" + (i + j) + "].close").Value<string>() != null)
                            {
                                index0++;
                                sum0 += tradingStockData.SelectToken("data.intraday[" + (i + j) + "].close").Value<float>();
                            }
                            else
                            {
                            }
                        }

                        for (int j = 1; j <= shortValue; j++)
                        {
                            if (tradingStockData.SelectToken("data.intraday[" + (i + j) + "].close").Value<string>() != null)
                            {
                                index1++;
                                sum1 += tradingStockData.SelectToken("data.intraday[" + (i + j) + "].close").Value<float>();
                            }
                            else
                            {
                            }
                        }

                        if (index0 != 0)
                        {
                            average0 = sum0 / index0;
                        }

                        if (index1 != 0)
                        {
                            average1 = sum1 / index1;
                        }

                        sumDeviation0 += average0;
                        //print(average0);
                        //print(average1);
                        float yPosition = yPrice[yPrice.Length - 1].transform.position.y - yPrice[0].transform.position.y;
                        MakeLine(stocks[59 - i].transform.position.x * 2532f / Screen.width,
                            (((average0 - stockLowValues.Min()) / (stockHighValues.Max() - stockLowValues.Min())) * yPosition + yPrice[0].transform.position.y) * 1170f / Screen.height,
                            stocks[58 - i].transform.position.x * 2532f / Screen.width,
                            (((average1 - stockLowValues.Min()) / (stockHighValues.Max() - stockLowValues.Min())) * yPosition + yPrice[0].transform.position.y) * 1170f / Screen.height,
                            UnityEngine.Color.green);
                    }

                    for (int i = 0; i < stockLowValues.Length - 1; i++)
                    {
                        float sum0 = 0f;
                        float average0 = 0f;
                        float sum1 = 0f;
                        float average1 = 0f;
                        int index0 = 0;
                        int index1 = 0;

                        for (int j = 0; j < shortValue; j++)
                        {
                            if (tradingStockData.SelectToken("data.intraday[" + (i + j) + "].close").Value<string>() != null)
                            {
                                index0++;
                                sum0 += tradingStockData.SelectToken("data.intraday[" + (i + j) + "].close").Value<float>();
                            }
                            else
                            {
                            }
                        }

                        for (int j = 1; j <= shortValue; j++)
                        {
                            if (tradingStockData.SelectToken("data.intraday[" + (i + j) + "].close").Value<string>() != null)
                            {
                                index1++;
                                sum1 += tradingStockData.SelectToken("data.intraday[" + (i + j) + "].close").Value<float>();
                            }
                            else
                            {
                            }
                        }

                        if (index0 != 0)
                        {
                            average0 = sum0 / index0;
                        }

                        if (index1 != 0)
                        {
                            average1 = sum1 / index1;
                        }

                        sumDeviation1 += Mathf.Pow((sumDeviation0 / 59f - average0), 2);
                    }

                    for (int i = 0; i < stockLowValues.Length - 1; i++)
                    {
                        float sum0 = 0f;
                        float average0 = 0f;
                        float sum1 = 0f;
                        float average1 = 0f;
                        int index0 = 0;
                        int index1 = 0;

                        for (int j = 0; j < shortValue; j++)
                        {
                            if (tradingStockData.SelectToken("data.intraday[" + (i + j) + "].close").Value<string>() != null)
                            {
                                index0++;
                                sum0 += tradingStockData.SelectToken("data.intraday[" + (i + j) + "].close").Value<float>();
                            }
                            else
                            {
                            }
                        }

                        for (int j = 1; j <= shortValue; j++)
                        {
                            if (tradingStockData.SelectToken("data.intraday[" + (i + j) + "].close").Value<string>() != null)
                            {
                                index1++;
                                sum1 += tradingStockData.SelectToken("data.intraday[" + (i + j) + "].close").Value<float>();
                            }
                            else
                            {
                            }
                        }

                        if (index0 != 0)
                        {
                            average0 = sum0 / index0;
                        }

                        if (index1 != 0)
                        {
                            average1 = sum1 / index1;
                        }
                        //print(average0);
                        //print(average1);
                        float yPosition = yPrice[yPrice.Length - 1].transform.position.y - yPrice[0].transform.position.y;
                        MakeLine(stocks[59 - i].transform.position.x * 2532f / Screen.width,
                            (((average0 + 0.2f * Mathf.Sqrt(sumDeviation1) - stockLowValues.Min()) / (stockHighValues.Max() - stockLowValues.Min())) * yPosition + yPrice[0].transform.position.y) * 1170f / Screen.height,
                            stocks[58 - i].transform.position.x * 2532f / Screen.width,
                            (((average1 + 0.2f * Mathf.Sqrt(sumDeviation1) - stockLowValues.Min()) / (stockHighValues.Max() - stockLowValues.Min())) * yPosition + yPrice[0].transform.position.y) * 1170f / Screen.height,
                            UnityEngine.Color.yellow);

                        MakeLine(stocks[59 - i].transform.position.x * 2532f / Screen.width,
                            (((average0 - 0.2f * Mathf.Sqrt(sumDeviation1) - stockLowValues.Min()) / (stockHighValues.Max() - stockLowValues.Min())) * yPosition + yPrice[0].transform.position.y) * 1170f / Screen.height,
                            stocks[58 - i].transform.position.x * 2532f / Screen.width,
                            (((average1 - 0.2f * Mathf.Sqrt(sumDeviation1) - stockLowValues.Min()) / (stockHighValues.Max() - stockLowValues.Min())) * yPosition + yPrice[0].transform.position.y) * 1170f / Screen.height,
                            UnityEngine.Color.blue);
                    }

                    //BBMA
                    for (int i = 0; i < stockLowValues.Length - 1; i++)
                    {
                        float sum0 = 0f;
                        float average0 = 0f;
                        float sum1 = 0f;
                        float average1 = 0f;
                        int index0 = 0;
                        int index1 = 0;

                        for (int j = 0; j < longValue; j++)
                        {
                            if (tradingStockData.SelectToken("data.intraday[" + (i + j) + "].close").Value<string>() != null)
                            {
                                index0++;
                                sum0 += tradingStockData.SelectToken("data.intraday[" + (i + j) + "].close").Value<float>();
                            }
                            else
                            {
                            }
                        }

                        for (int j = 1; j <= longValue; j++)
                        {
                            if (tradingStockData.SelectToken("data.intraday[" + (i + j) + "].close").Value<string>() != null)
                            {
                                index1++;
                                sum1 += tradingStockData.SelectToken("data.intraday[" + (i + j) + "].close").Value<float>();
                            }
                            else
                            {
                            }
                        }

                        if (index0 != 0)
                        {
                            average0 = sum0 / index0;
                        }

                        if (index1 != 0)
                        {
                            average1 = sum1 / index1;
                        }
                        //print(average0);
                        //print(average1);
                        float yPosition = yPrice[yPrice.Length - 1].transform.position.y - yPrice[0].transform.position.y;
                        MakeLine(stocks[59 - i].transform.position.x * 2532f / Screen.width,
                            (((average0 - stockLowValues.Min()) / (stockHighValues.Max() - stockLowValues.Min())) * yPosition + yPrice[0].transform.position.y) * 1170f / Screen.height,
                            stocks[58 - i].transform.position.x * 2532f / Screen.width,
                            (((average1 - stockLowValues.Min()) / (stockHighValues.Max() - stockLowValues.Min())) * yPosition + yPrice[0].transform.position.y) * 1170f / Screen.height,
                            UnityEngine.Color.red);
                    }

                    break;
                }

            case "30M":
                {
                    UnityWebRequest wwwTrading = UnityWebRequest.Get("https://api.marketstack.com/v1/tickers/" + selectedStockID + "/intraday?access_key=3c9529a8a320b0c5e8d8a0cf5a4d69e2" + "&interval=30min&offset=0&limit=1000");
                    yield return wwwTrading.SendWebRequest();

                    var tradingStockData = (JObject)JsonConvert.DeserializeObject(wwwTrading.downloadHandler.text);
                    print(wwwTrading.downloadHandler.text);

                    //Middle Band
                    float sumDeviation0 = 0f;
                    float sumDeviation1 = 0f;

                    for (int i = 0; i < stockLowValues.Length - 1; i++)
                    {
                        float sum0 = 0f;
                        float average0 = 0f;
                        float sum1 = 0f;
                        float average1 = 0f;
                        int index0 = 0;
                        int index1 = 0;

                        for (int j = 0; j < shortValue; j++)
                        {
                            if (tradingStockData.SelectToken("data.intraday[" + (i + j) + "].close").Value<string>() != null)
                            {
                                index0++;
                                sum0 += tradingStockData.SelectToken("data.intraday[" + (i + j) + "].close").Value<float>();
                            }
                            else
                            {
                            }
                        }

                        for (int j = 1; j <= shortValue; j++)
                        {
                            if (tradingStockData.SelectToken("data.intraday[" + (i + j) + "].close").Value<string>() != null)
                            {
                                index1++;
                                sum1 += tradingStockData.SelectToken("data.intraday[" + (i + j) + "].close").Value<float>();
                            }
                            else
                            {
                            }
                        }

                        if (index0 != 0)
                        {
                            average0 = sum0 / index0;
                        }

                        if (index1 != 0)
                        {
                            average1 = sum1 / index1;
                        }

                        sumDeviation0 += average0;
                        //print(average0);
                        //print(average1);
                        float yPosition = yPrice[yPrice.Length - 1].transform.position.y - yPrice[0].transform.position.y;
                        MakeLine(stocks[59 - i].transform.position.x * 2532f / Screen.width,
                            (((average0 - stockLowValues.Min()) / (stockHighValues.Max() - stockLowValues.Min())) * yPosition + yPrice[0].transform.position.y) * 1170f / Screen.height,
                            stocks[58 - i].transform.position.x * 2532f / Screen.width,
                            (((average1 - stockLowValues.Min()) / (stockHighValues.Max() - stockLowValues.Min())) * yPosition + yPrice[0].transform.position.y) * 1170f / Screen.height,
                            UnityEngine.Color.green);
                    }

                    for (int i = 0; i < stockLowValues.Length - 1; i++)
                    {
                        float sum0 = 0f;
                        float average0 = 0f;
                        float sum1 = 0f;
                        float average1 = 0f;
                        int index0 = 0;
                        int index1 = 0;

                        for (int j = 0; j < shortValue; j++)
                        {
                            if (tradingStockData.SelectToken("data.intraday[" + (i + j) + "].close").Value<string>() != null)
                            {
                                index0++;
                                sum0 += tradingStockData.SelectToken("data.intraday[" + (i + j) + "].close").Value<float>();
                            }
                            else
                            {
                            }
                        }

                        for (int j = 1; j <= shortValue; j++)
                        {
                            if (tradingStockData.SelectToken("data.intraday[" + (i + j) + "].close").Value<string>() != null)
                            {
                                index1++;
                                sum1 += tradingStockData.SelectToken("data.intraday[" + (i + j) + "].close").Value<float>();
                            }
                            else
                            {
                            }
                        }

                        if (index0 != 0)
                        {
                            average0 = sum0 / index0;
                        }

                        if (index1 != 0)
                        {
                            average1 = sum1 / index1;
                        }

                        sumDeviation1 += Mathf.Pow((sumDeviation0 / 59f - average0), 2);
                    }

                    for (int i = 0; i < stockLowValues.Length - 1; i++)
                    {
                        float sum0 = 0f;
                        float average0 = 0f;
                        float sum1 = 0f;
                        float average1 = 0f;
                        int index0 = 0;
                        int index1 = 0;

                        for (int j = 0; j < shortValue; j++)
                        {
                            if (tradingStockData.SelectToken("data.intraday[" + (i + j) + "].close").Value<string>() != null)
                            {
                                index0++;
                                sum0 += tradingStockData.SelectToken("data.intraday[" + (i + j) + "].close").Value<float>();
                            }
                            else
                            {
                            }
                        }

                        for (int j = 1; j <= shortValue; j++)
                        {
                            if (tradingStockData.SelectToken("data.intraday[" + (i + j) + "].close").Value<string>() != null)
                            {
                                index1++;
                                sum1 += tradingStockData.SelectToken("data.intraday[" + (i + j) + "].close").Value<float>();
                            }
                            else
                            {
                            }
                        }

                        if (index0 != 0)
                        {
                            average0 = sum0 / index0;
                        }

                        if (index1 != 0)
                        {
                            average1 = sum1 / index1;
                        }
                        //print(average0);
                        //print(average1);
                        float yPosition = yPrice[yPrice.Length - 1].transform.position.y - yPrice[0].transform.position.y;
                        MakeLine(stocks[59 - i].transform.position.x * 2532f / Screen.width,
                            (((average0 + 0.2f * Mathf.Sqrt(sumDeviation1) - stockLowValues.Min()) / (stockHighValues.Max() - stockLowValues.Min())) * yPosition + yPrice[0].transform.position.y) * 1170f / Screen.height,
                            stocks[58 - i].transform.position.x * 2532f / Screen.width,
                            (((average1 + 0.2f * Mathf.Sqrt(sumDeviation1) - stockLowValues.Min()) / (stockHighValues.Max() - stockLowValues.Min())) * yPosition + yPrice[0].transform.position.y) * 1170f / Screen.height,
                            UnityEngine.Color.yellow);

                        MakeLine(stocks[59 - i].transform.position.x * 2532f / Screen.width,
                            (((average0 - 0.2f * Mathf.Sqrt(sumDeviation1) - stockLowValues.Min()) / (stockHighValues.Max() - stockLowValues.Min())) * yPosition + yPrice[0].transform.position.y) * 1170f / Screen.height,
                            stocks[58 - i].transform.position.x * 2532f / Screen.width,
                            (((average1 - 0.2f * Mathf.Sqrt(sumDeviation1) - stockLowValues.Min()) / (stockHighValues.Max() - stockLowValues.Min())) * yPosition + yPrice[0].transform.position.y) * 1170f / Screen.height,
                            UnityEngine.Color.blue);
                    }

                    //BBMA
                    for (int i = 0; i < stockLowValues.Length - 1; i++)
                    {
                        float sum0 = 0f;
                        float average0 = 0f;
                        float sum1 = 0f;
                        float average1 = 0f;
                        int index0 = 0;
                        int index1 = 0;

                        for (int j = 0; j < longValue; j++)
                        {
                            if (tradingStockData.SelectToken("data.intraday[" + (i + j) + "].close").Value<string>() != null)
                            {
                                index0++;
                                sum0 += tradingStockData.SelectToken("data.intraday[" + (i + j) + "].close").Value<float>();
                            }
                            else
                            {
                            }
                        }

                        for (int j = 1; j <= longValue; j++)
                        {
                            if (tradingStockData.SelectToken("data.intraday[" + (i + j) + "].close").Value<string>() != null)
                            {
                                index1++;
                                sum1 += tradingStockData.SelectToken("data.intraday[" + (i + j) + "].close").Value<float>();
                            }
                            else
                            {
                            }
                        }

                        if (index0 != 0)
                        {
                            average0 = sum0 / index0;
                        }

                        if (index1 != 0)
                        {
                            average1 = sum1 / index1;
                        }
                        //print(average0);
                        //print(average1);
                        float yPosition = yPrice[yPrice.Length - 1].transform.position.y - yPrice[0].transform.position.y;
                        MakeLine(stocks[59 - i].transform.position.x * 2532f / Screen.width,
                            (((average0 - stockLowValues.Min()) / (stockHighValues.Max() - stockLowValues.Min())) * yPosition + yPrice[0].transform.position.y) * 1170f / Screen.height,
                            stocks[58 - i].transform.position.x * 2532f / Screen.width,
                            (((average1 - stockLowValues.Min()) / (stockHighValues.Max() - stockLowValues.Min())) * yPosition + yPrice[0].transform.position.y) * 1170f / Screen.height,
                            UnityEngine.Color.red);
                    }

                    break;
                }

            case "Hr":
                {
                    UnityWebRequest wwwTrading = UnityWebRequest.Get("https://api.marketstack.com/v1/tickers/" + selectedStockID + "/intraday?access_key=3c9529a8a320b0c5e8d8a0cf5a4d69e2" + "&interval=1hour&offset=0&limit=1000");
                    yield return wwwTrading.SendWebRequest();

                    var tradingStockData = (JObject)JsonConvert.DeserializeObject(wwwTrading.downloadHandler.text);
                    print(wwwTrading.downloadHandler.text);

                    //Middle Band
                    float sumDeviation0 = 0f;
                    float sumDeviation1 = 0f;

                    for (int i = 0; i < stockLowValues.Length - 1; i++)
                    {
                        float sum0 = 0f;
                        float average0 = 0f;
                        float sum1 = 0f;
                        float average1 = 0f;
                        int index0 = 0;
                        int index1 = 0;

                        for (int j = 0; j < shortValue; j++)
                        {
                            if (tradingStockData.SelectToken("data.intraday[" + (i + j) + "].close").Value<string>() != null)
                            {
                                index0++;
                                sum0 += tradingStockData.SelectToken("data.intraday[" + (i + j) + "].close").Value<float>();
                            }
                            else
                            {
                            }
                        }

                        for (int j = 1; j <= shortValue; j++)
                        {
                            if (tradingStockData.SelectToken("data.intraday[" + (i + j) + "].close").Value<string>() != null)
                            {
                                index1++;
                                sum1 += tradingStockData.SelectToken("data.intraday[" + (i + j) + "].close").Value<float>();
                            }
                            else
                            {
                            }
                        }

                        if (index0 != 0)
                        {
                            average0 = sum0 / index0;
                        }

                        if (index1 != 0)
                        {
                            average1 = sum1 / index1;
                        }

                        sumDeviation0 += average0;
                        //print(average0);
                        //print(average1);
                        float yPosition = yPrice[yPrice.Length - 1].transform.position.y - yPrice[0].transform.position.y;
                        MakeLine(stocks[59 - i].transform.position.x * 2532f / Screen.width,
                            (((average0 - stockLowValues.Min()) / (stockHighValues.Max() - stockLowValues.Min())) * yPosition + yPrice[0].transform.position.y) * 1170f / Screen.height,
                            stocks[58 - i].transform.position.x * 2532f / Screen.width,
                            (((average1 - stockLowValues.Min()) / (stockHighValues.Max() - stockLowValues.Min())) * yPosition + yPrice[0].transform.position.y) * 1170f / Screen.height,
                            UnityEngine.Color.green);
                    }

                    for (int i = 0; i < stockLowValues.Length - 1; i++)
                    {
                        float sum0 = 0f;
                        float average0 = 0f;
                        float sum1 = 0f;
                        float average1 = 0f;
                        int index0 = 0;
                        int index1 = 0;

                        for (int j = 0; j < shortValue; j++)
                        {
                            if (tradingStockData.SelectToken("data.intraday[" + (i + j) + "].close").Value<string>() != null)
                            {
                                index0++;
                                sum0 += tradingStockData.SelectToken("data.intraday[" + (i + j) + "].close").Value<float>();
                            }
                            else
                            {
                            }
                        }

                        for (int j = 1; j <= shortValue; j++)
                        {
                            if (tradingStockData.SelectToken("data.intraday[" + (i + j) + "].close").Value<string>() != null)
                            {
                                index1++;
                                sum1 += tradingStockData.SelectToken("data.intraday[" + (i + j) + "].close").Value<float>();
                            }
                            else
                            {
                            }
                        }

                        if (index0 != 0)
                        {
                            average0 = sum0 / index0;
                        }

                        if (index1 != 0)
                        {
                            average1 = sum1 / index1;
                        }

                        sumDeviation1 += Mathf.Pow((sumDeviation0 / 59f - average0), 2);
                    }

                    for (int i = 0; i < stockLowValues.Length - 1; i++)
                    {
                        float sum0 = 0f;
                        float average0 = 0f;
                        float sum1 = 0f;
                        float average1 = 0f;
                        int index0 = 0;
                        int index1 = 0;

                        for (int j = 0; j < shortValue; j++)
                        {
                            if (tradingStockData.SelectToken("data.intraday[" + (i + j) + "].close").Value<string>() != null)
                            {
                                index0++;
                                sum0 += tradingStockData.SelectToken("data.intraday[" + (i + j) + "].close").Value<float>();
                            }
                            else
                            {
                            }
                        }

                        for (int j = 1; j <= shortValue; j++)
                        {
                            if (tradingStockData.SelectToken("data.intraday[" + (i + j) + "].close").Value<string>() != null)
                            {
                                index1++;
                                sum1 += tradingStockData.SelectToken("data.intraday[" + (i + j) + "].close").Value<float>();
                            }
                            else
                            {
                            }
                        }

                        if (index0 != 0)
                        {
                            average0 = sum0 / index0;
                        }

                        if (index1 != 0)
                        {
                            average1 = sum1 / index1;
                        }
                        //print(average0);
                        //print(average1);
                        float yPosition = yPrice[yPrice.Length - 1].transform.position.y - yPrice[0].transform.position.y;
                        MakeLine(stocks[59 - i].transform.position.x * 2532f / Screen.width,
                            (((average0 + 0.2f * Mathf.Sqrt(sumDeviation1) - stockLowValues.Min()) / (stockHighValues.Max() - stockLowValues.Min())) * yPosition + yPrice[0].transform.position.y) * 1170f / Screen.height,
                            stocks[58 - i].transform.position.x * 2532f / Screen.width,
                            (((average1 + 0.2f * Mathf.Sqrt(sumDeviation1) - stockLowValues.Min()) / (stockHighValues.Max() - stockLowValues.Min())) * yPosition + yPrice[0].transform.position.y) * 1170f / Screen.height,
                            UnityEngine.Color.yellow);

                        MakeLine(stocks[59 - i].transform.position.x * 2532f / Screen.width,
                            (((average0 - 0.2f * Mathf.Sqrt(sumDeviation1) - stockLowValues.Min()) / (stockHighValues.Max() - stockLowValues.Min())) * yPosition + yPrice[0].transform.position.y) * 1170f / Screen.height,
                            stocks[58 - i].transform.position.x * 2532f / Screen.width,
                            (((average1 - 0.2f * Mathf.Sqrt(sumDeviation1) - stockLowValues.Min()) / (stockHighValues.Max() - stockLowValues.Min())) * yPosition + yPrice[0].transform.position.y) * 1170f / Screen.height,
                            UnityEngine.Color.blue);
                    }

                    //BBMA
                    for (int i = 0; i < stockLowValues.Length - 1; i++)
                    {
                        float sum0 = 0f;
                        float average0 = 0f;
                        float sum1 = 0f;
                        float average1 = 0f;
                        int index0 = 0;
                        int index1 = 0;

                        for (int j = 0; j < longValue; j++)
                        {
                            if (tradingStockData.SelectToken("data.intraday[" + (i + j) + "].close").Value<string>() != null)
                            {
                                index0++;
                                sum0 += tradingStockData.SelectToken("data.intraday[" + (i + j) + "].close").Value<float>();
                            }
                            else
                            {
                            }
                        }

                        for (int j = 1; j <= longValue; j++)
                        {
                            if (tradingStockData.SelectToken("data.intraday[" + (i + j) + "].close").Value<string>() != null)
                            {
                                index1++;
                                sum1 += tradingStockData.SelectToken("data.intraday[" + (i + j) + "].close").Value<float>();
                            }
                            else
                            {
                            }
                        }

                        if (index0 != 0)
                        {
                            average0 = sum0 / index0;
                        }

                        if (index1 != 0)
                        {
                            average1 = sum1 / index1;
                        }
                        //print(average0);
                        //print(average1);
                        float yPosition = yPrice[yPrice.Length - 1].transform.position.y - yPrice[0].transform.position.y;
                        MakeLine(stocks[59 - i].transform.position.x * 2532f / Screen.width,
                            (((average0 - stockLowValues.Min()) / (stockHighValues.Max() - stockLowValues.Min())) * yPosition + yPrice[0].transform.position.y) * 1170f / Screen.height,
                            stocks[58 - i].transform.position.x * 2532f / Screen.width,
                            (((average1 - stockLowValues.Min()) / (stockHighValues.Max() - stockLowValues.Min())) * yPosition + yPrice[0].transform.position.y) * 1170f / Screen.height,
                            UnityEngine.Color.red);
                    }

                    break;
                }
            case "H4":
                {
                    UnityWebRequest wwwTrading = UnityWebRequest.Get("https://api.marketstack.com/v1/tickers/" + selectedStockID + "/intraday?access_key=3c9529a8a320b0c5e8d8a0cf5a4d69e2" + "&interval=3hour&offset=0&limit=1000");
                    yield return wwwTrading.SendWebRequest();

                    var tradingStockData = (JObject)JsonConvert.DeserializeObject(wwwTrading.downloadHandler.text);
                    print(wwwTrading.downloadHandler.text);

                    //Middle Band
                    float sumDeviation0 = 0f;
                    float sumDeviation1 = 0f;

                    for (int i = 0; i < stockLowValues.Length - 1; i++)
                    {
                        float sum0 = 0f;
                        float average0 = 0f;
                        float sum1 = 0f;
                        float average1 = 0f;
                        int index0 = 0;
                        int index1 = 0;

                        for (int j = 0; j < shortValue; j++)
                        {
                            if (tradingStockData.SelectToken("data.intraday[" + (i + j) + "].close").Value<string>() != null)
                            {
                                index0++;
                                sum0 += tradingStockData.SelectToken("data.intraday[" + (i + j) + "].close").Value<float>();
                            }
                            else
                            {
                            }
                        }

                        for (int j = 1; j <= shortValue; j++)
                        {
                            if (tradingStockData.SelectToken("data.intraday[" + (i + j) + "].close").Value<string>() != null)
                            {
                                index1++;
                                sum1 += tradingStockData.SelectToken("data.intraday[" + (i + j) + "].close").Value<float>();
                            }
                            else
                            {
                            }
                        }

                        if (index0 != 0)
                        {
                            average0 = sum0 / index0;
                        }

                        if (index1 != 0)
                        {
                            average1 = sum1 / index1;
                        }

                        sumDeviation0 += average0;
                        //print(average0);
                        //print(average1);
                        float yPosition = yPrice[yPrice.Length - 1].transform.position.y - yPrice[0].transform.position.y;
                        MakeLine(stocks[59 - i].transform.position.x * 2532f / Screen.width,
                            (((average0 - stockLowValues.Min()) / (stockHighValues.Max() - stockLowValues.Min())) * yPosition + yPrice[0].transform.position.y) * 1170f / Screen.height,
                            stocks[58 - i].transform.position.x * 2532f / Screen.width,
                            (((average1 - stockLowValues.Min()) / (stockHighValues.Max() - stockLowValues.Min())) * yPosition + yPrice[0].transform.position.y) * 1170f / Screen.height,
                            UnityEngine.Color.green);
                    }

                    for (int i = 0; i < stockLowValues.Length - 1; i++)
                    {
                        float sum0 = 0f;
                        float average0 = 0f;
                        float sum1 = 0f;
                        float average1 = 0f;
                        int index0 = 0;
                        int index1 = 0;

                        for (int j = 0; j < shortValue; j++)
                        {
                            if (tradingStockData.SelectToken("data.intraday[" + (i + j) + "].close").Value<string>() != null)
                            {
                                index0++;
                                sum0 += tradingStockData.SelectToken("data.intraday[" + (i + j) + "].close").Value<float>();
                            }
                            else
                            {
                            }
                        }

                        for (int j = 1; j <= shortValue; j++)
                        {
                            if (tradingStockData.SelectToken("data.intraday[" + (i + j) + "].close").Value<string>() != null)
                            {
                                index1++;
                                sum1 += tradingStockData.SelectToken("data.intraday[" + (i + j) + "].close").Value<float>();
                            }
                            else
                            {
                            }
                        }

                        if (index0 != 0)
                        {
                            average0 = sum0 / index0;
                        }

                        if (index1 != 0)
                        {
                            average1 = sum1 / index1;
                        }

                        sumDeviation1 += Mathf.Pow((sumDeviation0 / 59f - average0), 2);
                    }

                    for (int i = 0; i < stockLowValues.Length - 1; i++)
                    {
                        float sum0 = 0f;
                        float average0 = 0f;
                        float sum1 = 0f;
                        float average1 = 0f;
                        int index0 = 0;
                        int index1 = 0;

                        for (int j = 0; j < shortValue; j++)
                        {
                            if (tradingStockData.SelectToken("data.intraday[" + (i + j) + "].close").Value<string>() != null)
                            {
                                index0++;
                                sum0 += tradingStockData.SelectToken("data.intraday[" + (i + j) + "].close").Value<float>();
                            }
                            else
                            {
                            }
                        }

                        for (int j = 1; j <= shortValue; j++)
                        {
                            if (tradingStockData.SelectToken("data.intraday[" + (i + j) + "].close").Value<string>() != null)
                            {
                                index1++;
                                sum1 += tradingStockData.SelectToken("data.intraday[" + (i + j) + "].close").Value<float>();
                            }
                            else
                            {
                            }
                        }

                        if (index0 != 0)
                        {
                            average0 = sum0 / index0;
                        }

                        if (index1 != 0)
                        {
                            average1 = sum1 / index1;
                        }
                        //print(average0);
                        //print(average1);
                        float yPosition = yPrice[yPrice.Length - 1].transform.position.y - yPrice[0].transform.position.y;
                        MakeLine(stocks[59 - i].transform.position.x * 2532f / Screen.width,
                            (((average0 + 0.2f * Mathf.Sqrt(sumDeviation1) - stockLowValues.Min()) / (stockHighValues.Max() - stockLowValues.Min())) * yPosition + yPrice[0].transform.position.y) * 1170f / Screen.height,
                            stocks[58 - i].transform.position.x * 2532f / Screen.width,
                            (((average1 + 0.2f * Mathf.Sqrt(sumDeviation1) - stockLowValues.Min()) / (stockHighValues.Max() - stockLowValues.Min())) * yPosition + yPrice[0].transform.position.y) * 1170f / Screen.height,
                            UnityEngine.Color.yellow);

                        MakeLine(stocks[59 - i].transform.position.x * 2532f / Screen.width,
                            (((average0 - 0.2f * Mathf.Sqrt(sumDeviation1) - stockLowValues.Min()) / (stockHighValues.Max() - stockLowValues.Min())) * yPosition + yPrice[0].transform.position.y) * 1170f / Screen.height,
                            stocks[58 - i].transform.position.x * 2532f / Screen.width,
                            (((average1 - 0.2f * Mathf.Sqrt(sumDeviation1) - stockLowValues.Min()) / (stockHighValues.Max() - stockLowValues.Min())) * yPosition + yPrice[0].transform.position.y) * 1170f / Screen.height,
                            UnityEngine.Color.blue);
                    }

                    //BBMA
                    for (int i = 0; i < stockLowValues.Length - 1; i++)
                    {
                        float sum0 = 0f;
                        float average0 = 0f;
                        float sum1 = 0f;
                        float average1 = 0f;
                        int index0 = 0;
                        int index1 = 0;

                        for (int j = 0; j < longValue; j++)
                        {
                            if (tradingStockData.SelectToken("data.intraday[" + (i + j) + "].close").Value<string>() != null)
                            {
                                index0++;
                                sum0 += tradingStockData.SelectToken("data.intraday[" + (i + j) + "].close").Value<float>();
                            }
                            else
                            {
                            }
                        }

                        for (int j = 1; j <= longValue; j++)
                        {
                            if (tradingStockData.SelectToken("data.intraday[" + (i + j) + "].close").Value<string>() != null)
                            {
                                index1++;
                                sum1 += tradingStockData.SelectToken("data.intraday[" + (i + j) + "].close").Value<float>();
                            }
                            else
                            {
                            }
                        }

                        if (index0 != 0)
                        {
                            average0 = sum0 / index0;
                        }

                        if (index1 != 0)
                        {
                            average1 = sum1 / index1;
                        }
                        //print(average0);
                        //print(average1);
                        float yPosition = yPrice[yPrice.Length - 1].transform.position.y - yPrice[0].transform.position.y;
                        MakeLine(stocks[59 - i].transform.position.x * 2532f / Screen.width,
                            (((average0 - stockLowValues.Min()) / (stockHighValues.Max() - stockLowValues.Min())) * yPosition + yPrice[0].transform.position.y) * 1170f / Screen.height,
                            stocks[58 - i].transform.position.x * 2532f / Screen.width,
                            (((average1 - stockLowValues.Min()) / (stockHighValues.Max() - stockLowValues.Min())) * yPosition + yPrice[0].transform.position.y) * 1170f / Screen.height,
                            UnityEngine.Color.red);
                    }

                    break;
                }

            case "D+":
                {
                    UnityWebRequest wwwTrading = UnityWebRequest.Get("https://api.marketstack.com/v1/tickers/" + selectedStockID + "/eod?access_key=3c9529a8a320b0c5e8d8a0cf5a4d69e2" + "&offset=0&limit=1000");
                    yield return wwwTrading.SendWebRequest();

                    var tradingStockData = (JObject)JsonConvert.DeserializeObject(wwwTrading.downloadHandler.text);

                    //Middle Band
                    float sumDeviation0 = 0f;
                    float sumDeviation1 = 0f;

                    for (int i = 0; i < stockLowValues.Length - 1; i++)
                    {
                        float sum0 = 0f;
                        float average0 = 0f;
                        float sum1 = 0f;
                        float average1 = 0f;

                        for (int j = 0; j < shortValue; j++)
                        {
                            sum0 += tradingStockData.SelectToken("data.eod[" + (i + j) + "].close").Value<float>();
                        }

                        for (int j = 1; j <= shortValue; j++)
                        {
                            sum1 += tradingStockData.SelectToken("data.eod[" + (i + j) + "].close").Value<float>();
                        }

                        average0 = sum0 / shortValue;
                        average1 = sum1 / shortValue;
                        sumDeviation0 += average0;
                        print(average0);
                        print(average1);
                        float yPosition = yPrice[yPrice.Length - 1].transform.position.y - yPrice[0].transform.position.y;
                        MakeLine(stocks[59 - i].transform.position.x * 2532f / Screen.width,
                            (((average0 - stockLowValues.Min()) / (stockHighValues.Max() - stockLowValues.Min())) * yPosition + yPrice[0].transform.position.y) * 1170f / Screen.height,
                            stocks[58 - i].transform.position.x * 2532f / Screen.width,
                            (((average1 - stockLowValues.Min()) / (stockHighValues.Max() - stockLowValues.Min())) * yPosition + yPrice[0].transform.position.y) * 1170f / Screen.height,
                            UnityEngine.Color.green);
                    }

                    for (int i = 0; i < stockLowValues.Length - 1; i++)
                    {
                        float sum0 = 0f;
                        float average0 = 0f;
                        float sum1 = 0f;
                        float average1 = 0f;

                        for (int j = 0; j < shortValue; j++)
                        {
                            sum0 += tradingStockData.SelectToken("data.eod[" + (i + j) + "].close").Value<float>();
                        }

                        for (int j = 1; j <= shortValue; j++)
                        {
                            sum1 += tradingStockData.SelectToken("data.eod[" + (i + j) + "].close").Value<float>();
                        }

                        average0 = sum0 / shortValue;
                        average1 = sum1 / shortValue;
                        sumDeviation1 += Mathf.Pow((sumDeviation0 / 59f - average0), 2);
                    }

                    for (int i = 0; i < stockLowValues.Length - 1; i++)
                    {
                        float sum0 = 0f;
                        float average0 = 0f;
                        float sum1 = 0f;
                        float average1 = 0f;

                        for (int j = 0; j < shortValue; j++)
                        {
                            sum0 += tradingStockData.SelectToken("data.eod[" + (i + j) + "].close").Value<float>();
                        }

                        for (int j = 1; j <= shortValue; j++)
                        {
                            sum1 += tradingStockData.SelectToken("data.eod[" + (i + j) + "].close").Value<float>();
                        }

                        average0 = sum0 / shortValue;
                        average1 = sum1 / shortValue;
                        //print(average0);
                        //print(average1);
                        float yPosition = yPrice[yPrice.Length - 1].transform.position.y - yPrice[0].transform.position.y;
                        MakeLine(stocks[59 - i].transform.position.x * 2532f / Screen.width,
                            (((average0 + 0.2f * Mathf.Sqrt(sumDeviation1) - stockLowValues.Min()) / (stockHighValues.Max() - stockLowValues.Min())) * yPosition + yPrice[0].transform.position.y) * 1170f / Screen.height,
                            stocks[58 - i].transform.position.x * 2532f / Screen.width,
                            (((average1 + 0.2f * Mathf.Sqrt(sumDeviation1) - stockLowValues.Min()) / (stockHighValues.Max() - stockLowValues.Min())) * yPosition + yPrice[0].transform.position.y) * 1170f / Screen.height,
                            UnityEngine.Color.yellow);

                        MakeLine(stocks[59 - i].transform.position.x * 2532f / Screen.width,
                            (((average0 - 0.2f * Mathf.Sqrt(sumDeviation1) - stockLowValues.Min()) / (stockHighValues.Max() - stockLowValues.Min())) * yPosition + yPrice[0].transform.position.y) * 1170f / Screen.height,
                            stocks[58 - i].transform.position.x * 2532f / Screen.width,
                            (((average1 - 0.2f * Mathf.Sqrt(sumDeviation1) - stockLowValues.Min()) / (stockHighValues.Max() - stockLowValues.Min())) * yPosition + yPrice[0].transform.position.y) * 1170f / Screen.height,
                            UnityEngine.Color.blue);
                    }

                    //BBMA
                    for (int i = 0; i < stockLowValues.Length - 1; i++)
                    {
                        float sum0 = 0f;
                        float average0 = 0f;
                        float sum1 = 0f;
                        float average1 = 0f;

                        for (int j = 0; j < longValue; j++)
                        {
                            sum0 += tradingStockData.SelectToken("data.eod[" + (i + j) + "].close").Value<float>();
                        }

                        for (int j = 1; j <= longValue; j++)
                        {
                            sum1 += tradingStockData.SelectToken("data.eod[" + (i + j) + "].close").Value<float>();
                        }

                        average0 = sum0 / longValue;
                        average1 = sum1 / longValue;
                        //print(average0);
                        //print(average1);
                        float yPosition = yPrice[yPrice.Length - 1].transform.position.y - yPrice[0].transform.position.y;
                        MakeLine(stocks[59 - i].transform.position.x * 2532f / Screen.width,
                            (((average0 - stockLowValues.Min()) / (stockHighValues.Max() - stockLowValues.Min())) * yPosition + yPrice[0].transform.position.y) * 1170f / Screen.height,
                            stocks[58 - i].transform.position.x * 2532f / Screen.width,
                            (((average1 - stockLowValues.Min()) / (stockHighValues.Max() - stockLowValues.Min())) * yPosition + yPrice[0].transform.position.y) * 1170f / Screen.height,
                            UnityEngine.Color.red);
                    }

                    break;
                }

            case "D":
                {
                    UnityWebRequest wwwTrading = UnityWebRequest.Get("https://api.marketstack.com/v1/tickers/" + selectedStockID + "/eod?access_key=3c9529a8a320b0c5e8d8a0cf5a4d69e2" + "&offset=0&limit=1000");
                    yield return wwwTrading.SendWebRequest();

                    var tradingStockData = (JObject)JsonConvert.DeserializeObject(wwwTrading.downloadHandler.text);

                    //Middle Band
                    float sumDeviation0 = 0f;
                    float sumDeviation1 = 0f;

                    for (int i = 0; i < stockLowValues.Length - 1; i++)
                    {
                        float sum0 = 0f;
                        float average0 = 0f;
                        float sum1 = 0f;
                        float average1 = 0f;

                        for (int j = 0; j < shortValue; j++)
                        {
                            sum0 += tradingStockData.SelectToken("data.eod[" + (i + j) + "].close").Value<float>();
                        }

                        for (int j = 1; j <= shortValue; j++)
                        {
                            sum1 += tradingStockData.SelectToken("data.eod[" + (i + j) + "].close").Value<float>();
                        }

                        average0 = sum0 / shortValue;
                        average1 = sum1 / shortValue;
                        sumDeviation0 += average0;
                        //print(average0);
                        //print(average1);
                        float yPosition = yPrice[yPrice.Length - 1].transform.position.y - yPrice[0].transform.position.y;
                        MakeLine(stocks[59 - i].transform.position.x * 2532f / Screen.width,
                            (((average0 - stockLowValues.Min()) / (stockHighValues.Max() - stockLowValues.Min())) * yPosition + yPrice[0].transform.position.y) * 1170f / Screen.height,
                            stocks[58 - i].transform.position.x * 2532f / Screen.width,
                            (((average1 - stockLowValues.Min()) / (stockHighValues.Max() - stockLowValues.Min())) * yPosition + yPrice[0].transform.position.y) * 1170f / Screen.height,
                            UnityEngine.Color.green);
                    }

                    for (int i = 0; i < stockLowValues.Length - 1; i++)
                    {
                        float sum0 = 0f;
                        float average0 = 0f;
                        float sum1 = 0f;
                        float average1 = 0f;

                        for (int j = 0; j < shortValue; j++)
                        {
                            sum0 += tradingStockData.SelectToken("data.eod[" + (i + j) + "].close").Value<float>();
                        }

                        for (int j = 1; j <= shortValue; j++)
                        {
                            sum1 += tradingStockData.SelectToken("data.eod[" + (i + j) + "].close").Value<float>();
                        }

                        average0 = sum0 / shortValue;
                        average1 = sum1 / shortValue;
                        sumDeviation1 += Mathf.Pow((sumDeviation0 / 59f - average0), 2);
                    }

                    for (int i = 0; i < stockLowValues.Length - 1; i++)
                    {
                        float sum0 = 0f;
                        float average0 = 0f;
                        float sum1 = 0f;
                        float average1 = 0f;

                        for (int j = 0; j < shortValue; j++)
                        {
                            sum0 += tradingStockData.SelectToken("data.eod[" + (i + j) + "].close").Value<float>();
                        }

                        for (int j = 1; j <= shortValue; j++)
                        {
                            sum1 += tradingStockData.SelectToken("data.eod[" + (i + j) + "].close").Value<float>();
                        }

                        average0 = sum0 / shortValue;
                        average1 = sum1 / shortValue;
                        //print(average0);
                        //print(average1);
                        float yPosition = yPrice[yPrice.Length - 1].transform.position.y - yPrice[0].transform.position.y;
                        MakeLine(stocks[59 - i].transform.position.x * 2532f / Screen.width,
                            (((average0 + 0.2f * Mathf.Sqrt(sumDeviation1) - stockLowValues.Min()) / (stockHighValues.Max() - stockLowValues.Min())) * yPosition + yPrice[0].transform.position.y) * 1170f / Screen.height,
                            stocks[58 - i].transform.position.x * 2532f / Screen.width,
                            (((average1 + 0.2f * Mathf.Sqrt(sumDeviation1) - stockLowValues.Min()) / (stockHighValues.Max() - stockLowValues.Min())) * yPosition + yPrice[0].transform.position.y) * 1170f / Screen.height,
                            UnityEngine.Color.yellow);

                        MakeLine(stocks[59 - i].transform.position.x * 2532f / Screen.width,
                            (((average0 - 0.2f * Mathf.Sqrt(sumDeviation1) - stockLowValues.Min()) / (stockHighValues.Max() - stockLowValues.Min())) * yPosition + yPrice[0].transform.position.y) * 1170f / Screen.height,
                            stocks[58 - i].transform.position.x * 2532f / Screen.width,
                            (((average1 - 0.2f * Mathf.Sqrt(sumDeviation1) - stockLowValues.Min()) / (stockHighValues.Max() - stockLowValues.Min())) * yPosition + yPrice[0].transform.position.y) * 1170f / Screen.height,
                            UnityEngine.Color.blue);
                    }

                    //BBMA
                    for (int i = 0; i < stockLowValues.Length - 1; i++)
                    {
                        float sum0 = 0f;
                        float average0 = 0f;
                        float sum1 = 0f;
                        float average1 = 0f;

                        for (int j = 0; j < longValue; j++)
                        {
                            sum0 += tradingStockData.SelectToken("data.eod[" + (i + j) + "].close").Value<float>();
                        }

                        for (int j = 1; j <= longValue; j++)
                        {
                            sum1 += tradingStockData.SelectToken("data.eod[" + (i + j) + "].close").Value<float>();
                        }

                        average0 = sum0 / longValue;
                        average1 = sum1 / longValue;
                        //print(average0);
                        //print(average1);
                        float yPosition = yPrice[yPrice.Length - 1].transform.position.y - yPrice[0].transform.position.y;
                        MakeLine(stocks[59 - i].transform.position.x * 2532f / Screen.width,
                            (((average0 - stockLowValues.Min()) / (stockHighValues.Max() - stockLowValues.Min())) * yPosition + yPrice[0].transform.position.y) * 1170f / Screen.height,
                            stocks[58 - i].transform.position.x * 2532f / Screen.width,
                            (((average1 - stockLowValues.Min()) / (stockHighValues.Max() - stockLowValues.Min())) * yPosition + yPrice[0].transform.position.y) * 1170f / Screen.height,
                            UnityEngine.Color.red);
                    }

                    break;
                }

            case "W+":
                {
                    UnityWebRequest wwwTrading = UnityWebRequest.Get("https://api.marketstack.com/v1/tickers/" + selectedStockID + "/eod?access_key=3c9529a8a320b0c5e8d8a0cf5a4d69e2" + "&offset=0&limit=1000");
                    yield return wwwTrading.SendWebRequest();

                    var tradingStockData = (JObject)JsonConvert.DeserializeObject(wwwTrading.downloadHandler.text);

                    //Middle Band
                    float sumDeviation0 = 0f;
                    float sumDeviation1 = 0f;

                    for (int i = 0; i < stockLowValues.Length - 1; i++)
                    {
                        float sum0 = 0f;
                        float average0 = 0f;
                        float sum1 = 0f;
                        float average1 = 0f;

                        for (int j = 0; j < shortValue; j++)
                        {
                            sum0 += tradingStockData.SelectToken("data.eod[" + (i + j) * 5 + "].close").Value<float>();
                        }

                        for (int j = 1; j <= shortValue; j++)
                        {
                            sum1 += tradingStockData.SelectToken("data.eod[" + (i + j ) * 5 + "].close").Value<float>();
                        }

                        average0 = sum0 / shortValue;
                        average1 = sum1 / shortValue;
                        sumDeviation0 += average0;
                        //print(average0);
                        //print(average1);
                        float yPosition = yPrice[yPrice.Length - 1].transform.position.y - yPrice[0].transform.position.y;
                        MakeLine(stocks[59 - i].transform.position.x * 2532f / Screen.width,
                            (((average0 - stockLowValues.Min()) / (stockHighValues.Max() - stockLowValues.Min())) * yPosition + yPrice[0].transform.position.y) * 1170f / Screen.height,
                            stocks[58 - i].transform.position.x * 2532f / Screen.width,
                            (((average1 - stockLowValues.Min()) / (stockHighValues.Max() - stockLowValues.Min())) * yPosition + yPrice[0].transform.position.y) * 1170f / Screen.height,
                            UnityEngine.Color.green);
                    }

                    for (int i = 0; i < stockLowValues.Length - 1; i++)
                    {
                        float sum0 = 0f;
                        float average0 = 0f;
                        float sum1 = 0f;
                        float average1 = 0f;

                        for (int j = 0; j < shortValue; j++)
                        {
                            sum0 += tradingStockData.SelectToken("data.eod[" + (i + j ) * 5 + "].close").Value<float>();
                        }

                        for (int j = 1; j <= shortValue; j++)
                        {
                            sum1 += tradingStockData.SelectToken("data.eod[" + (i + j ) * 5 + "].close").Value<float>();
                        }

                        average0 = sum0 / shortValue;
                        average1 = sum1 / shortValue;
                        sumDeviation1 += Mathf.Pow((sumDeviation0 / 59f - average0), 2);
                    }

                    for (int i = 0; i < stockLowValues.Length - 1; i++)
                    {
                        float sum0 = 0f;
                        float average0 = 0f;
                        float sum1 = 0f;
                        float average1 = 0f;

                        for (int j = 0; j < shortValue; j++)
                        {
                            sum0 += tradingStockData.SelectToken("data.eod[" + (i + j ) * 5 + "].close").Value<float>();
                        }

                        for (int j = 1; j <= shortValue; j++)
                        {
                            sum1 += tradingStockData.SelectToken("data.eod[" + (i + j ) * 5 + "].close").Value<float>();
                        }

                        average0 = sum0 / shortValue;
                        average1 = sum1 / shortValue;
                        //print(average0);
                        //print(average1);
                        float yPosition = yPrice[yPrice.Length - 1].transform.position.y - yPrice[0].transform.position.y;
                        MakeLine(stocks[59 - i].transform.position.x * 2532f / Screen.width,
                            (((average0 + 0.2f * Mathf.Sqrt(sumDeviation1) - stockLowValues.Min()) / (stockHighValues.Max() - stockLowValues.Min())) * yPosition + yPrice[0].transform.position.y) * 1170f / Screen.height,
                            stocks[58 - i].transform.position.x * 2532f / Screen.width,
                            (((average1 + 0.2f * Mathf.Sqrt(sumDeviation1) - stockLowValues.Min()) / (stockHighValues.Max() - stockLowValues.Min())) * yPosition + yPrice[0].transform.position.y) * 1170f / Screen.height,
                            UnityEngine.Color.yellow);

                        MakeLine(stocks[59 - i].transform.position.x * 2532f / Screen.width,
                            (((average0 - 0.2f * Mathf.Sqrt(sumDeviation1) - stockLowValues.Min()) / (stockHighValues.Max() - stockLowValues.Min())) * yPosition + yPrice[0].transform.position.y) * 1170f / Screen.height,
                            stocks[58 - i].transform.position.x * 2532f / Screen.width,
                            (((average1 - 0.2f * Mathf.Sqrt(sumDeviation1) - stockLowValues.Min()) / (stockHighValues.Max() - stockLowValues.Min())) * yPosition + yPrice[0].transform.position.y) * 1170f / Screen.height,
                            UnityEngine.Color.blue);
                    }

                    //BBMA
                    for (int i = 0; i < stockLowValues.Length - 1; i++)
                    {
                        float sum0 = 0f;
                        float average0 = 0f;
                        float sum1 = 0f;
                        float average1 = 0f;

                        for (int j = 0; j < longValue; j++)
                        {
                            sum0 += tradingStockData.SelectToken("data.eod[" + (i + j ) * 5 + "].close").Value<float>();
                        }

                        for (int j = 1; j <= longValue; j++)
                        {
                            sum1 += tradingStockData.SelectToken("data.eod[" + (i + j ) * 5 + "].close").Value<float>();
                        }

                        average0 = sum0 / longValue;
                        average1 = sum1 / longValue;
                        //print(average0);
                        //print(average1);
                        float yPosition = yPrice[yPrice.Length - 1].transform.position.y - yPrice[0].transform.position.y;
                        MakeLine(stocks[59 - i].transform.position.x * 2532f / Screen.width,
                            (((average0 - stockLowValues.Min()) / (stockHighValues.Max() - stockLowValues.Min())) * yPosition + yPrice[0].transform.position.y) * 1170f / Screen.height,
                            stocks[58 - i].transform.position.x * 2532f / Screen.width,
                            (((average1 - stockLowValues.Min()) / (stockHighValues.Max() - stockLowValues.Min())) * yPosition + yPrice[0].transform.position.y) * 1170f / Screen.height,
                            UnityEngine.Color.red);
                    }

                    break;
                }

            case "W":
                {
                    UnityWebRequest wwwTrading = UnityWebRequest.Get("https://api.marketstack.com/v1/tickers/" + selectedStockID + "/eod?access_key=3c9529a8a320b0c5e8d8a0cf5a4d69e2" + "&offset=0&limit=1000");
                    yield return wwwTrading.SendWebRequest();

                    var tradingStockData = (JObject)JsonConvert.DeserializeObject(wwwTrading.downloadHandler.text);

                    //Middle Band
                    float sumDeviation0 = 0f;
                    float sumDeviation1 = 0f;

                    for (int i = 0; i < stockLowValues.Length - 1; i++)
                    {
                        float sum0 = 0f;
                        float average0 = 0f;
                        float sum1 = 0f;
                        float average1 = 0f;

                        for (int j = 0; j < shortValue; j++)
                        {
                            sum0 += tradingStockData.SelectToken("data.eod[" + (i + j ) * 5 + "].close").Value<float>();
                        }

                        for (int j = 1; j <= shortValue; j++)
                        {
                            sum1 += tradingStockData.SelectToken("data.eod[" + (i + j ) * 5 + "].close").Value<float>();
                        }

                        average0 = sum0 / shortValue;
                        average1 = sum1 / shortValue;
                        sumDeviation0 += average0;
                        //print(average0);
                        //print(average1);
                        float yPosition = yPrice[yPrice.Length - 1].transform.position.y - yPrice[0].transform.position.y;
                        MakeLine(stocks[59 - i].transform.position.x * 2532f / Screen.width,
                            (((average0 - stockLowValues.Min()) / (stockHighValues.Max() - stockLowValues.Min())) * yPosition + yPrice[0].transform.position.y) * 1170f / Screen.height,
                            stocks[58 - i].transform.position.x * 2532f / Screen.width,
                            (((average1 - stockLowValues.Min()) / (stockHighValues.Max() - stockLowValues.Min())) * yPosition + yPrice[0].transform.position.y) * 1170f / Screen.height,
                            UnityEngine.Color.green);
                    }

                    for (int i = 0; i < stockLowValues.Length - 1; i++)
                    {
                        float sum0 = 0f;
                        float average0 = 0f;
                        float sum1 = 0f;
                        float average1 = 0f;

                        for (int j = 0; j < shortValue; j++)
                        {
                            sum0 += tradingStockData.SelectToken("data.eod[" + (i + j ) * 5 + "].close").Value<float>();
                        }

                        for (int j = 1; j <= shortValue; j++)
                        {
                            sum1 += tradingStockData.SelectToken("data.eod[" + (i + j ) * 5 + "].close").Value<float>();
                        }

                        average0 = sum0 / shortValue;
                        average1 = sum1 / shortValue;
                        sumDeviation1 += Mathf.Pow((sumDeviation0 / 59f - average0), 2);
                    }

                    for (int i = 0; i < stockLowValues.Length - 1; i++)
                    {
                        float sum0 = 0f;
                        float average0 = 0f;
                        float sum1 = 0f;
                        float average1 = 0f;

                        for (int j = 0; j < shortValue; j++)
                        {
                            sum0 += tradingStockData.SelectToken("data.eod[" + (i + j ) * 5 + "].close").Value<float>();
                        }

                        for (int j = 1; j <= shortValue; j++)
                        {
                            sum1 += tradingStockData.SelectToken("data.eod[" + (i + j ) * 5 + "].close").Value<float>();
                        }

                        average0 = sum0 / shortValue;
                        average1 = sum1 / shortValue;
                        //print(average0);
                        //print(average1);
                        float yPosition = yPrice[yPrice.Length - 1].transform.position.y - yPrice[0].transform.position.y;
                        MakeLine(stocks[59 - i].transform.position.x * 2532f / Screen.width,
                            (((average0 + 0.2f * Mathf.Sqrt(sumDeviation1) - stockLowValues.Min()) / (stockHighValues.Max() - stockLowValues.Min())) * yPosition + yPrice[0].transform.position.y) * 1170f / Screen.height,
                            stocks[58 - i].transform.position.x * 2532f / Screen.width,
                            (((average1 + 0.2f * Mathf.Sqrt(sumDeviation1) - stockLowValues.Min()) / (stockHighValues.Max() - stockLowValues.Min())) * yPosition + yPrice[0].transform.position.y) * 1170f / Screen.height,
                            UnityEngine.Color.yellow);

                        MakeLine(stocks[59 - i].transform.position.x * 2532f / Screen.width,
                            (((average0 - 0.2f * Mathf.Sqrt(sumDeviation1) - stockLowValues.Min()) / (stockHighValues.Max() - stockLowValues.Min())) * yPosition + yPrice[0].transform.position.y) * 1170f / Screen.height,
                            stocks[58 - i].transform.position.x * 2532f / Screen.width,
                            (((average1 - 0.2f * Mathf.Sqrt(sumDeviation1) - stockLowValues.Min()) / (stockHighValues.Max() - stockLowValues.Min())) * yPosition + yPrice[0].transform.position.y) * 1170f / Screen.height,
                            UnityEngine.Color.blue);
                    }

                    //BBMA
                    for (int i = 0; i < stockLowValues.Length - 1; i++)
                    {
                        float sum0 = 0f;
                        float average0 = 0f;
                        float sum1 = 0f;
                        float average1 = 0f;

                        for (int j = 0; j < longValue; j++)
                        {
                            sum0 += tradingStockData.SelectToken("data.eod[" + (i + j ) * 5 + "].close").Value<float>();
                        }

                        for (int j = 1; j <= longValue; j++)
                        {
                            sum1 += tradingStockData.SelectToken("data.eod[" + (i + j ) * 5 + "].close").Value<float>();
                        }

                        average0 = sum0 / longValue;
                        average1 = sum1 / longValue;
                        //print(average0);
                        //print(average1);
                        float yPosition = yPrice[yPrice.Length - 1].transform.position.y - yPrice[0].transform.position.y;
                        MakeLine(stocks[59 - i].transform.position.x * 2532f / Screen.width,
                            (((average0 - stockLowValues.Min()) / (stockHighValues.Max() - stockLowValues.Min())) * yPosition + yPrice[0].transform.position.y) * 1170f / Screen.height,
                            stocks[58 - i].transform.position.x * 2532f / Screen.width,
                            (((average1 - stockLowValues.Min()) / (stockHighValues.Max() - stockLowValues.Min())) * yPosition + yPrice[0].transform.position.y) * 1170f / Screen.height,
                            UnityEngine.Color.red);
                    }
                    break;
                }
            case "M+":
                {
                    UnityWebRequest wwwTrading = UnityWebRequest.Get("https://api.marketstack.com/v1/tickers/" + selectedStockID + "/eod?access_key=3c9529a8a320b0c5e8d8a0cf5a4d69e2" + "&offset=0&limit=1000");
                    yield return wwwTrading.SendWebRequest();

                    var tradingStockData = (JObject)JsonConvert.DeserializeObject(wwwTrading.downloadHandler.text);

                    //Middle Band
                    float sumDeviation0 = 0f;
                    float sumDeviation1 = 0f;

                    for (int i = 0; i < stockLowValues.Length - 1; i++)
                    {
                        float sum0 = 0f;
                        float average0 = 0f;
                        float sum1 = 0f;
                        float average1 = 0f;

                        for (int j = 0; j < shortValue; j++)
                        {
                            sum0 += tradingStockData.SelectToken("data.eod[" + (i + j ) * 10 + "].close").Value<float>();
                        }

                        for (int j = 1; j <= shortValue; j++)
                        {
                            sum1 += tradingStockData.SelectToken("data.eod[" + (i + j ) * 10 + "].close").Value<float>();
                        }

                        average0 = sum0 / shortValue;
                        average1 = sum1 / shortValue;
                        sumDeviation0 += average0;
                        //print(average0);
                        //print(average1);
                        float yPosition = yPrice[yPrice.Length - 1].transform.position.y - yPrice[0].transform.position.y;
                        MakeLine(stocks[59 - i].transform.position.x * 2532f / Screen.width,
                            (((average0 - stockLowValues.Min()) / (stockHighValues.Max() - stockLowValues.Min())) * yPosition + yPrice[0].transform.position.y) * 1170f / Screen.height,
                            stocks[58 - i].transform.position.x * 2532f / Screen.width,
                            (((average1 - stockLowValues.Min()) / (stockHighValues.Max() - stockLowValues.Min())) * yPosition + yPrice[0].transform.position.y) * 1170f / Screen.height,
                            UnityEngine.Color.green);
                    }

                    for (int i = 0; i < stockLowValues.Length - 1; i++)
                    {
                        float sum0 = 0f;
                        float average0 = 0f;
                        float sum1 = 0f;
                        float average1 = 0f;

                        for (int j = 0; j < shortValue; j++)
                        {
                            sum0 += tradingStockData.SelectToken("data.eod[" + (i + j ) * 10 + "].close").Value<float>();
                        }

                        for (int j = 1; j <= shortValue; j++)
                        {
                            sum1 += tradingStockData.SelectToken("data.eod[" + (i + j ) * 10 + "].close").Value<float>();
                        }

                        average0 = sum0 / shortValue;
                        average1 = sum1 / shortValue;
                        sumDeviation1 += Mathf.Pow((sumDeviation0 / 59f - average0), 2);
                    }

                    for (int i = 0; i < stockLowValues.Length - 1; i++)
                    {
                        float sum0 = 0f;
                        float average0 = 0f;
                        float sum1 = 0f;
                        float average1 = 0f;

                        for (int j = 0; j < shortValue; j++)
                        {
                            sum0 += tradingStockData.SelectToken("data.eod[" + (i + j ) * 10 + "].close").Value<float>();
                        }

                        for (int j = 1; j <= shortValue; j++)
                        {
                            sum1 += tradingStockData.SelectToken("data.eod[" + (i + j ) * 10 + "].close").Value<float>();
                        }

                        average0 = sum0 / shortValue;
                        average1 = sum1 / shortValue;
                        //print(average0);
                        //print(average1);
                        float yPosition = yPrice[yPrice.Length - 1].transform.position.y - yPrice[0].transform.position.y;
                        MakeLine(stocks[59 - i].transform.position.x * 2532f / Screen.width,
                            (((average0 + 0.2f * Mathf.Sqrt(sumDeviation1) - stockLowValues.Min()) / (stockHighValues.Max() - stockLowValues.Min())) * yPosition + yPrice[0].transform.position.y) * 1170f / Screen.height,
                            stocks[58 - i].transform.position.x * 2532f / Screen.width,
                            (((average1 + 0.2f * Mathf.Sqrt(sumDeviation1) - stockLowValues.Min()) / (stockHighValues.Max() - stockLowValues.Min())) * yPosition + yPrice[0].transform.position.y) * 1170f / Screen.height,
                            UnityEngine.Color.yellow);

                        MakeLine(stocks[59 - i].transform.position.x * 2532f / Screen.width,
                            (((average0 - 0.2f * Mathf.Sqrt(sumDeviation1) - stockLowValues.Min()) / (stockHighValues.Max() - stockLowValues.Min())) * yPosition + yPrice[0].transform.position.y) * 1170f / Screen.height,
                            stocks[58 - i].transform.position.x * 2532f / Screen.width,
                            (((average1 - 0.2f * Mathf.Sqrt(sumDeviation1) - stockLowValues.Min()) / (stockHighValues.Max() - stockLowValues.Min())) * yPosition + yPrice[0].transform.position.y) * 1170f / Screen.height,
                            UnityEngine.Color.blue);
                    }

                    //BBMA
                    for (int i = 0; i < stockLowValues.Length - 1; i++)
                    {
                        float sum0 = 0f;
                        float average0 = 0f;
                        float sum1 = 0f;
                        float average1 = 0f;

                        for (int j = 0; j < 40; j++)
                        {
                            sum0 += tradingStockData.SelectToken("data.eod[" + (i + j ) * 10 + "].close").Value<float>();
                        }

                        for (int j = 1; j <= 40; j++)
                        {
                            sum1 += tradingStockData.SelectToken("data.eod[" + (i + j ) * 10 + "].close").Value<float>();
                        }

                        average0 = sum0 / 15;
                        average1 = sum1 / 15;
                        //print(average0);
                        //print(average1);
                        float yPosition = yPrice[yPrice.Length - 1].transform.position.y - yPrice[0].transform.position.y;
                        MakeLine(stocks[59 - i].transform.position.x * 2532f / Screen.width,
                            (((average0 - stockLowValues.Min()) / (stockHighValues.Max() - stockLowValues.Min())) * yPosition + yPrice[0].transform.position.y) * 1170f / Screen.height,
                            stocks[58 - i].transform.position.x * 2532f / Screen.width,
                            (((average1 - stockLowValues.Min()) / (stockHighValues.Max() - stockLowValues.Min())) * yPosition + yPrice[0].transform.position.y) * 1170f / Screen.height,
                            UnityEngine.Color.red);
                    }
                    break;
                }

            case "M":
                {
                    UnityWebRequest wwwTrading = UnityWebRequest.Get("https://api.marketstack.com/v1/tickers/" + selectedStockID + "/eod?access_key=3c9529a8a320b0c5e8d8a0cf5a4d69e2" + "&offset=0&limit=1000");
                    yield return wwwTrading.SendWebRequest();

                    var tradingStockData = (JObject)JsonConvert.DeserializeObject(wwwTrading.downloadHandler.text);
                    //print(wwwTrading.downloadHandler.text);
                    //Middle Band
                    float sumDeviation0 = 0f;
                    float sumDeviation1 = 0f;

                    for (int i = 0; i < stockLowValues.Length - 1; i++)
                    {
                        float sum0 = 0f;
                        float average0 = 0f;
                        float sum1 = 0f;
                        float average1 = 0f;

                        for (int j = 0; j < shortValue; j++)
                        {
                            sum0 += tradingStockData.SelectToken("data.eod[" + (i + j ) * 10 + "].close").Value<float>();
                        }

                        for (int j = 1; j <= shortValue; j++)
                        {
                            sum1 += tradingStockData.SelectToken("data.eod[" + (i + j ) * 10 + "].close").Value<float>();
                        }

                        average0 = sum0 / shortValue;
                        average1 = sum1 / shortValue;
                        sumDeviation0 += average0;
                        //print(average0);
                        //print(average1);
                        float yPosition = yPrice[yPrice.Length - 1].transform.position.y - yPrice[0].transform.position.y;
                        MakeLine(stocks[59 - i].transform.position.x * 2532f / Screen.width,
                            (((average0 - stockLowValues.Min()) / (stockHighValues.Max() - stockLowValues.Min())) * yPosition + yPrice[0].transform.position.y) * 1170f / Screen.height,
                            stocks[58 - i].transform.position.x * 2532f / Screen.width,
                            (((average1 - stockLowValues.Min()) / (stockHighValues.Max() - stockLowValues.Min())) * yPosition + yPrice[0].transform.position.y) * 1170f / Screen.height,
                            UnityEngine.Color.green);
                    }

                    for (int i = 0; i < stockLowValues.Length - 1; i++)
                    {
                        float sum0 = 0f;
                        float average0 = 0f;
                        float sum1 = 0f;
                        float average1 = 0f;

                        for (int j = 0; j < shortValue; j++)
                        {
                            sum0 += tradingStockData.SelectToken("data.eod[" + (i + j ) * 10 + "].close").Value<float>();
                        }

                        for (int j = 1; j <= shortValue; j++)
                        {
                            sum1 += tradingStockData.SelectToken("data.eod[" + (i + j ) * 10 + "].close").Value<float>();
                        }

                        average0 = sum0 / shortValue;
                        average1 = sum1 / shortValue;
                        sumDeviation1 += Mathf.Pow((sumDeviation0 / 59f - average0), 2);
                    }

                    for (int i = 0; i < stockLowValues.Length - 1; i++)
                    {
                        float sum0 = 0f;
                        float average0 = 0f;
                        float sum1 = 0f;
                        float average1 = 0f;

                        for (int j = 0; j < shortValue; j++)
                        {
                            sum0 += tradingStockData.SelectToken("data.eod[" + (i + j ) * 10 + "].close").Value<float>();
                        }

                        for (int j = 1; j <= shortValue; j++)
                        {
                            sum1 += tradingStockData.SelectToken("data.eod[" + (i + j ) * 10 + "].close").Value<float>();
                        }

                        average0 = sum0 / shortValue;
                        average1 = sum1 / shortValue;
                        //print(average0);
                        //print(average1);
                        float yPosition = yPrice[yPrice.Length - 1].transform.position.y - yPrice[0].transform.position.y;
                        MakeLine(stocks[59 - i].transform.position.x * 2532f / Screen.width,
                            (((average0 + 0.2f * Mathf.Sqrt(sumDeviation1) - stockLowValues.Min()) / (stockHighValues.Max() - stockLowValues.Min())) * yPosition + yPrice[0].transform.position.y) * 1170f / Screen.height,
                            stocks[58 - i].transform.position.x * 2532f / Screen.width,
                            (((average1 + 0.2f * Mathf.Sqrt(sumDeviation1) - stockLowValues.Min()) / (stockHighValues.Max() - stockLowValues.Min())) * yPosition + yPrice[0].transform.position.y) * 1170f / Screen.height,
                            UnityEngine.Color.yellow);

                        MakeLine(stocks[59 - i].transform.position.x * 2532f / Screen.width,
                            (((average0 - 0.2f * Mathf.Sqrt(sumDeviation1) - stockLowValues.Min()) / (stockHighValues.Max() - stockLowValues.Min())) * yPosition + yPrice[0].transform.position.y) * 1170f / Screen.height,
                            stocks[58 - i].transform.position.x * 2532f / Screen.width,
                            (((average1 - 0.2f * Mathf.Sqrt(sumDeviation1) - stockLowValues.Min()) / (stockHighValues.Max() - stockLowValues.Min())) * yPosition + yPrice[0].transform.position.y) * 1170f / Screen.height,
                            UnityEngine.Color.blue);
                    }

                    //BBMA
                    for (int i = 0; i < stockLowValues.Length - 1; i++)
                    {
                        float sum0 = 0f;
                        float average0 = 0f;
                        float sum1 = 0f;
                        float average1 = 0f;

                        for (int j = 0; j < 40; j++)
                        {
                            sum0 += tradingStockData.SelectToken("data.eod[" + (i + j ) * 10 + "].close").Value<float>();
                        }

                        for (int j = 1; j <= 40; j++)
                        {
                            sum1 += tradingStockData.SelectToken("data.eod[" + (i + j ) * 10 + "].close").Value<float>();
                        }

                        average0 = sum0 / 15;
                        average1 = sum1 / 15;
                        //print(average0);
                        //print(average1);
                        float yPosition = yPrice[yPrice.Length - 1].transform.position.y - yPrice[0].transform.position.y;
                        MakeLine(stocks[59 - i].transform.position.x * 2532f / Screen.width,
                            (((average0 - stockLowValues.Min()) / (stockHighValues.Max() - stockLowValues.Min())) * yPosition + yPrice[0].transform.position.y) * 1170f / Screen.height,
                            stocks[58 - i].transform.position.x * 2532f / Screen.width,
                            (((average1 - stockLowValues.Min()) / (stockHighValues.Max() - stockLowValues.Min())) * yPosition + yPrice[0].transform.position.y) * 1170f / Screen.height,
                            UnityEngine.Color.red);
                    }
                    break;
                }
        }        
    }

    public void GetDate(int pastdays)
    {
        DateTime[] lastDays = Enumerable.Range(0, pastdays)
        .Select(i => DateTime.Now.Date.AddDays(-i))
        .ToArray();
        fromDate = $"{lastDays[lastDays.Length - 1]:yyyy-MM-dd}";          
        toDate = $"{lastDays[0]:yyyy-MM-dd}";
    }

    public void ShowYPrice(float min, float max)
    {        
        for (int i = 0; i < yPrice.Length; i++)
        {
            yPrice[i].text = "" + Math.Round(min + ((max - min) / 12f * i), 1);
        }

        for (int i = 0; i < stocks.Length; i++)
        {
            if (stockCloseValues[i] >= stockOpenValues[i])
            {
                float delta = stockCloseValues[i] - stockOpenValues[i];
                float yPosition = yPrice[yPrice.Length - 1].transform.position.y - yPrice[0].transform.position.y;
                //print(((delta / 2f + stockOpenValues[i]) / (stockHighValues.Max() - stockLowValues.Min())));
                stocks[59 - i].transform.position = new Vector3(stocks[59 - i].transform.position.x,
                    ((delta / 2f + stockOpenValues[i] - min) / (max  - min)) * yPosition + yPrice[0].transform.position.y,
                    stocks[59 - i].transform.position.z);
                stocks[59 - i].GetComponentsInChildren<Image>()[1].gameObject.transform.position = new Vector3(stocks[59 - i].GetComponentsInChildren<Image>()[1].gameObject.transform.position.x,
                    stocks[59 - i].GetComponentsInChildren<Image>()[0].gameObject.transform.position.y + (((stockHighValues[i] - stockLowValues[i])/2f + stockLowValues[i]) - (delta/2f + stockOpenValues[i])) / (max - min) * yPosition,
                    stocks[59 - i].GetComponentsInChildren<Image>()[1].gameObject.transform.position.z);
                stocks[59 - i].GetComponentsInChildren<Image>()[0].gameObject.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, ((delta) / max) * yPosition);
                stocks[59 - i].GetComponentsInChildren<Image>()[1].gameObject.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, ((stockHighValues[i] - stockLowValues[i]) / (max - min)) * yPosition);

                stocks[59 - i].GetComponentsInChildren<Image>()[0].color = new Color32(0, 192, 163, 255);
                stocks[59 - i].GetComponentsInChildren<Image>()[1].color = new Color32(0, 192, 163, 255);
            }
            else
            {
                float delta = stockOpenValues[i] - stockCloseValues[i];
                float yPosition = yPrice[yPrice.Length - 1].transform.position.y - yPrice[0].transform.position.y;
                stocks[59 - i].transform.position = new Vector3(stocks[59 - i].transform.position.x,
                    ((delta / 2f + stockCloseValues[i] - min) / (max - min)) * yPosition + yPrice[0].transform.position.y,
                    stocks[59 - i].transform.position.z);
                stocks[59 - i].GetComponentsInChildren<Image>()[1].gameObject.transform.position = new Vector3(stocks[59 - i].GetComponentsInChildren<Image>()[1].gameObject.transform.position.x,
                    stocks[59 - i].GetComponentsInChildren<Image>()[0].gameObject.transform.position.y + (((stockHighValues[i] - stockLowValues[i]) / 2f + stockLowValues[i]) - (delta / 2f + stockCloseValues[i])) / (max - min) * yPosition,
                    stocks[59 - i].GetComponentsInChildren<Image>()[1].gameObject.transform.position.z);
                stocks[59 - i].GetComponentsInChildren<Image>()[0].gameObject.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, ((delta) / max) * yPosition);
                stocks[59 - i].GetComponentsInChildren<Image>()[1].gameObject.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, ((stockHighValues[i] - stockLowValues[i]) / (max - min)) * yPosition);

                stocks[59 - i].GetComponentsInChildren<Image>()[0].color = new Color32(252, 84, 84, 255);
                stocks[59 - i].GetComponentsInChildren<Image>()[1].color = new Color32(252, 84, 84, 255);
            }
        }
    }

    public void ShowYVolume()
    {        
        float max = stockVolumeValues.Max();       

        for (int i = 0; i < yVolume.Length; i++)
        {
            yVolume[i].text = "" + Math.Round((((max / 3f) * i) / 1000000f), 1) + "M";
        }

        for (int i = 0; i < charts.Length; i++)
        {
            float yPosition = yVolume[3].transform.position.y - yVolume[0].transform.position.y;            
            charts[59 - i].transform.position = new Vector3(charts[59 - i].transform.position.x,
                yPosition * stockVolumeValues[i]/(2f * max) * (Screen.height / 1170f) + yVolume[0].transform.position.y,
                charts[59 - i].transform.position.z);
            charts[59 - i].GetComponentInChildren<Image>().gameObject.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, yPosition * (stockVolumeValues[i]/ max));

            if (stockCloseValues[i] >= stockOpenValues[i])
            {
                charts[59 - i].GetComponentInChildren<Image>().color = new Color32(26, 66, 54, 255);
            }
            else
            {
                charts[59 - i].GetComponentInChildren<Image>().color = new Color32(86, 48, 48, 255);
            }
        }
    }

    public void DashBoardClick()
    {
        for (int i = 0; i < buttonSignals.Length; i++)
        {
            buttonSignals[i].gameObject.SetActive(false);
        }

        for (int i = 0; i < stockIDSignal.Length; i++)
        {
            stockIDSignal[i].text = StockEngine.instance.stocksID[i];
        }

        StartCoroutine(SignalM5());
        StartCoroutine(SignalM15());
        StartCoroutine(SignalM30());
        StartCoroutine(SignalH1());
        StartCoroutine(SignalH4());
        StartCoroutine(SignalD1());
        StartCoroutine(SignalW1());
        StartCoroutine(SignalMN());
    }

    public void DashboardExit()
    {
        StopCoroutine(SignalM5());
        StopCoroutine(SignalM15());
        StopCoroutine(SignalM30());
        StopCoroutine(SignalH1());
        StopCoroutine(SignalH4());
        StopCoroutine(SignalD1());
        StopCoroutine(SignalW1());
        StopCoroutine(SignalMN());
    }

    IEnumerator SignalM5()
    {     
        for (int i = 0; i < StockEngine.instance.stocksID.Length; i++)
        {
            UnityWebRequest wwwM5 = UnityWebRequest.Get("https://api.marketstack.com/v1/tickers/" + StockEngine.instance.stocksID[i] + "/intraday?access_key=3c9529a8a320b0c5e8d8a0cf5a4d69e2" + "&interval=5min&offset=0&limit=1000");
            yield return wwwM5.SendWebRequest();

            var tradingStockData = (JObject)JsonConvert.DeserializeObject(wwwM5.downloadHandler.text);            
            float sum0 = 0f;
            float averageLong0 = 0f;
            float sum1 = 0f;
            float averageLong1 = 0f;
            float averageShort0 = 0f;
            float averageShort1 = 0f;
            int index0 = 0;
            int index1 = 0;

            for (int j = 0; j < longValue; j++)
            {
                if (tradingStockData.SelectToken("data.intraday[" + (0 + j) + "].close").Value<string>() != null)
                {
                    index0++;
                    sum0 += tradingStockData.SelectToken("data.intraday[" + (0 + j) + "].close").Value<float>();
                }                    
            }

            for (int j = 1; j <= longValue; j++)
            {
                if (tradingStockData.SelectToken("data.intraday[" + (5 + j) + "].close").Value<string>() != null)
                {
                    index1++;
                    sum1 += tradingStockData.SelectToken("data.intraday[" + (5 + j) + "].close").Value<float>();
                }                
            }

            averageLong0 = sum0 / index0;
            averageLong1 = sum1 / index1;
            sum0 = 0f;
            sum1 = 0f;
            index0 = 0;
            index1 = 0;

            for (int j = 0; j < shortValue; j++)
            {
                if (tradingStockData.SelectToken("data.intraday[" + (0 + j) + "].close").Value<string>() != null)
                {
                    index0++;
                    sum0 += tradingStockData.SelectToken("data.intraday[" + (0 + j) + "].close").Value<float>();
                }
            }

            for (int j = 1; j <= shortValue; j++)
            {
                if (tradingStockData.SelectToken("data.intraday[" + (j) + "].close").Value<string>() != null)
                {
                    index1++;
                    sum1 += tradingStockData.SelectToken("data.intraday[" + (j) + "].close").Value<float>();
                }
            }

            averageShort0 = sum0 / index0;
            averageShort1 = sum1 / index1;

            print("S0:" + averageShort0);
            print("L0:" + averageLong0);
            print("S1:" + averageShort1);
            print("L1:" + averageLong1);

            if (averageLong0 > averageShort0)
            {
                if (averageShort1 > averageLong1)
                {
                    buttonSignals[i].gameObject.SetActive(true);
                    buttonSignals[i].color = UnityEngine.Color.red;
                    buttonSignals[i].GetComponentInChildren<Text>().text = "SELL CALL";
                    buttonSignals[i].GetComponentInChildren<Text>().color = UnityEngine.Color.white;
                    alertSource.Play();
                    alertImage.SetActive(true);
                    alertImage.GetComponentInChildren<Text>().text = "Alert from Sell Signal";
                    StartCoroutine(DelayAlertShow());
                }
            }
            else if (averageShort0 > averageLong0)
            {
                if (averageLong1 > averageShort1)
                {
                    buttonSignals[i].gameObject.SetActive(true);
                    buttonSignals[i].color = UnityEngine.Color.green;
                    buttonSignals[i].GetComponentInChildren<Text>().text = "BUY CALL";
                    buttonSignals[i].GetComponentInChildren<Text>().color = UnityEngine.Color.black;
                    alertSource.Play();
                    alertImage.SetActive(true);
                    alertImage.GetComponentInChildren<Text>().text = "Alert from Buy Signal";
                    StartCoroutine(DelayAlertShow());
                }
            }
        }               
    }

    IEnumerator SignalM15()
    {
        for (int i = 0; i < StockEngine.instance.stocksID.Length; i++)
        {
            UnityWebRequest wwwM15 = UnityWebRequest.Get("https://api.marketstack.com/v1/tickers/" + StockEngine.instance.stocksID[i] + "/intraday?access_key=3c9529a8a320b0c5e8d8a0cf5a4d69e2" + "&interval=15min&offset=0&limit=1000");
            yield return wwwM15.SendWebRequest();

            var tradingStockData = (JObject)JsonConvert.DeserializeObject(wwwM15.downloadHandler.text);

            float sum0 = 0f;
            float averageLong0 = 0f;
            float sum1 = 0f;
            float averageLong1 = 0f;
            float averageShort0 = 0f;
            float averageShort1 = 0f;
            int index0 = 0;
            int index1 = 0;

            for (int j = 0; j < longValue; j++)
            {
                if (tradingStockData.SelectToken("data.intraday[" + (0 + j) + "].close").Value<string>() != null)
                {
                    index0++;
                    sum0 += tradingStockData.SelectToken("data.intraday[" + (0 + j) + "].close").Value<float>();
                }
            }

            for (int j = 1; j <= longValue; j++)
            {
                if (tradingStockData.SelectToken("data.intraday[" + (j) + "].close").Value<string>() != null)
                {
                    index1++;
                    sum1 += tradingStockData.SelectToken("data.intraday[" + (j) + "].close").Value<float>();
                }
            }

            averageLong0 = sum0 / index0;
            averageLong1 = sum1 / index1;
            sum0 = 0f;
            sum1 = 0f;
            index0 = 0;
            index1 = 0;

            for (int j = 0; j < shortValue; j++)
            {
                if (tradingStockData.SelectToken("data.intraday[" + (0 + j) + "].close").Value<string>() != null)
                {
                    index0++;
                    sum0 += tradingStockData.SelectToken("data.intraday[" + (0 + j) + "].close").Value<float>();
                }
            }

            for (int j = 1; j <= shortValue; j++)
            {
                if (tradingStockData.SelectToken("data.intraday[" + (j) + "].close").Value<string>() != null)
                {
                    index1++;
                    sum1 += tradingStockData.SelectToken("data.intraday[" + (j) + "].close").Value<float>();
                }
            }

            averageShort0 = sum0 / index0;
            averageShort1 = sum1 / index1;

            print("S0:" + averageShort0);
            print("L0:" + averageLong0);
            print("S1:" + averageShort1);
            print("L1:" + averageLong1);

            if (averageLong0 > averageShort0)
            {
                if (averageShort1 > averageLong1)
                {
                    buttonSignals[i + 30].gameObject.SetActive(true);
                    buttonSignals[i + 30].color = UnityEngine.Color.red;
                    buttonSignals[i + 30].GetComponentInChildren<Text>().text = "SELL CALL";
                    buttonSignals[i + 30].GetComponentInChildren<Text>().color = UnityEngine.Color.white;
                    alertSource.Play();
                    alertImage.SetActive(true);
                    alertImage.GetComponentInChildren<Text>().text = "Alert from Sell Signal";
                    StartCoroutine(DelayAlertShow());
                }
            }
            else if (averageShort0 > averageLong0)
            {
                if (averageLong1 > averageShort1)
                {
                    buttonSignals[i + 30].gameObject.SetActive(true);
                    buttonSignals[i + 30].color = UnityEngine.Color.green;
                    buttonSignals[i + 30].GetComponentInChildren<Text>().text = "BUY CALL";
                    buttonSignals[i + 30].GetComponentInChildren<Text>().color = UnityEngine.Color.black;
                    alertSource.Play();
                    alertImage.SetActive(true);
                    alertImage.GetComponentInChildren<Text>().text = "Alert from Buy Signal";
                    StartCoroutine(DelayAlertShow());
                }
            }
        }
    }

    IEnumerator SignalM30()
    {
        for (int i = 0; i < StockEngine.instance.stocksID.Length; i++)
        {
            UnityWebRequest wwwM30 = UnityWebRequest.Get("https://api.marketstack.com/v1/tickers/" + StockEngine.instance.stocksID[i] + "/intraday?access_key=3c9529a8a320b0c5e8d8a0cf5a4d69e2" + "&interval=30min&offset=0&limit=1000");
            yield return wwwM30.SendWebRequest();

            var tradingStockData = (JObject)JsonConvert.DeserializeObject(wwwM30.downloadHandler.text);

            float sum0 = 0f;
            float averageLong0 = 0f;
            float sum1 = 0f;
            float averageLong1 = 0f;
            float averageShort0 = 0f;
            float averageShort1 = 0f;
            int index0 = 0;
            int index1 = 0;

            for (int j = 0; j < longValue; j++)
            {
                if (tradingStockData.SelectToken("data.intraday[" + (0 + j) + "].close").Value<string>() != null)
                {
                    index0++;
                    sum0 += tradingStockData.SelectToken("data.intraday[" + (0 + j) + "].close").Value<float>();
                }
            }

            for (int j = 1; j <= longValue; j++)
            {
                if (tradingStockData.SelectToken("data.intraday[" + (j) + "].close").Value<string>() != null)
                {
                    index1++;
                    sum1 += tradingStockData.SelectToken("data.intraday[" + (j) + "].close").Value<float>();
                }
            }

            averageLong0 = sum0 / index0;
            averageLong1 = sum1 / index1;
            sum0 = 0f;
            sum1 = 0f;
            index0 = 0;
            index1 = 0;

            for (int j = 0; j < shortValue; j++)
            {
                if (tradingStockData.SelectToken("data.intraday[" + (0 + j) + "].close").Value<string>() != null)
                {
                    index0++;
                    sum0 += tradingStockData.SelectToken("data.intraday[" + (0 + j) + "].close").Value<float>();
                }
            }

            for (int j = 1; j <= shortValue; j++)
            {
                if (tradingStockData.SelectToken("data.intraday[" + (j) + "].close").Value<string>() != null)
                {
                    index1++;
                    sum1 += tradingStockData.SelectToken("data.intraday[" + (j) + "].close").Value<float>();
                }
            }

            averageShort0 = sum0 / index0;
            averageShort1 = sum1 / index1;

            print("S0:" + averageShort0);
            print("L0:" + averageLong0);
            print("S1:" + averageShort1);
            print("L1:" + averageLong1);

            if (averageLong0 > averageShort0)
            {
                if (averageShort1 > averageLong1)
                {
                    buttonSignals[i + 60].gameObject.SetActive(true);
                    buttonSignals[i + 60].color = UnityEngine.Color.red;
                    buttonSignals[i + 60].GetComponentInChildren<Text>().text = "SELL CALL";
                    buttonSignals[i + 60].GetComponentInChildren<Text>().color = UnityEngine.Color.white;
                    alertSource.Play();
                    alertImage.SetActive(true);
                    alertImage.GetComponentInChildren<Text>().text = "Alert from Sell Signal";
                    StartCoroutine(DelayAlertShow());
                }
            }
            else if (averageShort0 > averageLong0)
            {
                if (averageLong1 > averageShort1)
                {
                    buttonSignals[i + 60].gameObject.SetActive(true);
                    buttonSignals[i + 60].color = UnityEngine.Color.green;
                    buttonSignals[i + 60].GetComponentInChildren<Text>().text = "BUY CALL";
                    buttonSignals[i + 60].GetComponentInChildren<Text>().color = UnityEngine.Color.black;
                    alertSource.Play();
                    alertImage.SetActive(true);
                    alertImage.GetComponentInChildren<Text>().text = "Alert from Buy Signal";
                    StartCoroutine(DelayAlertShow());
                }
            }
        }
    }

    IEnumerator SignalH1()
    {
        for (int i = 0; i < StockEngine.instance.stocksID.Length; i++)
        {
            UnityWebRequest wwwH1 = UnityWebRequest.Get("https://api.marketstack.com/v1/tickers/" + StockEngine.instance.stocksID[i] + "/intraday?access_key=3c9529a8a320b0c5e8d8a0cf5a4d69e2" + "&interval=1hour&offset=0&limit=1000");
            yield return wwwH1.SendWebRequest();

            var tradingStockData = (JObject)JsonConvert.DeserializeObject(wwwH1.downloadHandler.text);

            float sum0 = 0f;
            float averageLong0 = 0f;
            float sum1 = 0f;
            float averageLong1 = 0f;
            float averageShort0 = 0f;
            float averageShort1 = 0f;
            int index0 = 0;
            int index1 = 0;

            for (int j = 0; j < longValue; j++)
            {
                if (tradingStockData.SelectToken("data.intraday[" + (0 + j) + "].close").Value<string>() != null)
                {
                    index0++;
                    sum0 += tradingStockData.SelectToken("data.intraday[" + (0 + j) + "].close").Value<float>();
                }
            }

            for (int j = 1; j <= longValue; j++)
            {
                if (tradingStockData.SelectToken("data.intraday[" + (j) + "].close").Value<string>() != null)
                {
                    index1++;
                    sum1 += tradingStockData.SelectToken("data.intraday[" + (j) + "].close").Value<float>();
                }
            }

            averageLong0 = sum0 / index0;
            averageLong1 = sum1 / index1;
            sum0 = 0f;
            sum1 = 0f;
            index0 = 0;
            index1 = 0;

            for (int j = 0; j < shortValue; j++)
            {
                if (tradingStockData.SelectToken("data.intraday[" + (0 + j) + "].close").Value<string>() != null)
                {
                    index0++;
                    sum0 += tradingStockData.SelectToken("data.intraday[" + (0 + j) + "].close").Value<float>();
                }
            }

            for (int j = 1; j <= shortValue; j++)
            {
                if (tradingStockData.SelectToken("data.intraday[" + (j) + "].close").Value<string>() != null)
                {
                    index1++;
                    sum1 += tradingStockData.SelectToken("data.intraday[" + (j) + "].close").Value<float>();
                }
            }

            averageShort0 = sum0 / index0;
            averageShort1 = sum1 / index1;

            print("S0:" + averageShort0);
            print("L0:" + averageLong0);
            print("S1:" + averageShort1);
            print("L1:" + averageLong1);

            if (averageLong0 > averageShort0)
            {
                if (averageShort1 > averageLong1)
                {
                    buttonSignals[i + 90].gameObject.SetActive(true);
                    buttonSignals[i + 90].color = UnityEngine.Color.red;
                    buttonSignals[i + 90].GetComponentInChildren<Text>().text = "SELL CALL";
                    buttonSignals[i + 90].GetComponentInChildren<Text>().color = UnityEngine.Color.white;
                    alertSource.Play();
                    alertImage.SetActive(true);
                    alertImage.GetComponentInChildren<Text>().text = "Alert from Sell Signal";
                    StartCoroutine(DelayAlertShow());
                }
            }
            else if (averageShort0 > averageLong0)
            {
                if (averageLong1 > averageShort1)
                {
                    buttonSignals[i + 90].gameObject.SetActive(true);
                    buttonSignals[i + 90].color = UnityEngine.Color.green;
                    buttonSignals[i + 90].GetComponentInChildren<Text>().text = "BUY CALL";
                    buttonSignals[i + 90].GetComponentInChildren<Text>().color = UnityEngine.Color.black;
                    alertSource.Play();
                    alertImage.SetActive(true);
                    alertImage.GetComponentInChildren<Text>().text = "Alert from Buy Signal";
                    StartCoroutine(DelayAlertShow());
                }
            }
        }
    }

    IEnumerator SignalH4()
    {
        for (int i = 0; i < StockEngine.instance.stocksID.Length; i++)
        {
            UnityWebRequest wwwH4 = UnityWebRequest.Get("https://api.marketstack.com/v1/tickers/" + StockEngine.instance.stocksID[i] + "/intraday?access_key=3c9529a8a320b0c5e8d8a0cf5a4d69e2" + "&interval=3hour&offset=0&limit=1000");
            yield return wwwH4.SendWebRequest();

            var tradingStockData = (JObject)JsonConvert.DeserializeObject(wwwH4.downloadHandler.text);

            float sum0 = 0f;
            float averageLong0 = 0f;
            float sum1 = 0f;
            float averageLong1 = 0f;
            float averageShort0 = 0f;
            float averageShort1 = 0f;
            int index0 = 0;
            int index1 = 0;

            for (int j = 0; j < longValue; j++)
            {
                if (tradingStockData.SelectToken("data.intraday[" + (0 + j) + "].close").Value<string>() != null)
                {
                    index0++;
                    sum0 += tradingStockData.SelectToken("data.intraday[" + (0 + j) + "].close").Value<float>();
                }
            }

            for (int j = 1; j <= longValue; j++)
            {
                if (tradingStockData.SelectToken("data.intraday[" + (j) + "].close").Value<string>() != null)
                {
                    index1++;
                    sum1 += tradingStockData.SelectToken("data.intraday[" + (j) + "].close").Value<float>();
                }
            }

            averageLong0 = sum0 / index0;
            averageLong1 = sum1 / index1;
            sum0 = 0f;
            sum1 = 0f;
            index0 = 0;
            index1 = 0;

            for (int j = 0; j < shortValue; j++)
            {
                if (tradingStockData.SelectToken("data.intraday[" + (0 + j) + "].close").Value<string>() != null)
                {
                    index0++;
                    sum0 += tradingStockData.SelectToken("data.intraday[" + (0 + j) + "].close").Value<float>();
                }
            }

            for (int j = 1; j <= shortValue; j++)
            {
                if (tradingStockData.SelectToken("data.intraday[" + (j) + "].close").Value<string>() != null)
                {
                    index1++;
                    sum1 += tradingStockData.SelectToken("data.intraday[" + (j) + "].close").Value<float>();
                }
            }

            averageShort0 = sum0 / index0;
            averageShort1 = sum1 / index1;

            print("S0:" + averageShort0);
            print("L0:" + averageLong0);
            print("S1:" + averageShort1);
            print("L1:" + averageLong1);

            if (averageLong0 > averageShort0)
            {
                if (averageShort1 > averageLong1)
                {
                    buttonSignals[i + 120].gameObject.SetActive(true);
                    buttonSignals[i + 120].color = UnityEngine.Color.red;
                    buttonSignals[i + 120].GetComponentInChildren<Text>().text = "SELL CALL";
                    buttonSignals[i + 120].GetComponentInChildren<Text>().color = UnityEngine.Color.white;
                    alertSource.Play();
                    alertImage.SetActive(true);
                    alertImage.GetComponentInChildren<Text>().text = "Alert from Sell Signal";
                    StartCoroutine(DelayAlertShow());
                }
            }
            else if (averageShort0 > averageLong0)
            {
                if (averageLong1 > averageShort1)
                {
                    buttonSignals[i + 120].gameObject.SetActive(true);
                    buttonSignals[i + 120].color = UnityEngine.Color.green;
                    buttonSignals[i + 120].GetComponentInChildren<Text>().text = "BUY CALL";
                    buttonSignals[i + 120].GetComponentInChildren<Text>().color = UnityEngine.Color.black;
                    alertSource.Play();
                    alertImage.SetActive(true);
                    alertImage.GetComponentInChildren<Text>().text = "Alert from Buy Signal";
                    StartCoroutine(DelayAlertShow());
                }
            }
        }
    }

    IEnumerator SignalD1()
    {
        for (int i = 0; i < StockEngine.instance.stocksID.Length; i++)
        {
            UnityWebRequest wwwD1 = UnityWebRequest.Get("https://api.marketstack.com/v1/tickers/" + StockEngine.instance.stocksID[i] + "/eod?access_key=3c9529a8a320b0c5e8d8a0cf5a4d69e2" + "&offset=0&limit=1000");
            yield return wwwD1.SendWebRequest();

            var tradingStockData = (JObject)JsonConvert.DeserializeObject(wwwD1.downloadHandler.text);

            float sum0 = 0f;
            float averageLong0 = 0f;
            float sum1 = 0f;
            float averageLong1 = 0f;
            float averageShort0 = 0f;
            float averageShort1 = 0f;
            int index0 = 0;
            int index1 = 0;

            for (int j = 0; j < longValue; j++)
            {
                if (tradingStockData.SelectToken("data.eod[" + (0 + j) + "].close").Value<string>() != null)
                {
                    index0++;
                    sum0 += tradingStockData.SelectToken("data.eod[" + (0 + j) + "].close").Value<float>();
                }
            }

            for (int j = 1; j <= longValue; j++)
            {
                if (tradingStockData.SelectToken("data.eod[" + (j) + "].close").Value<string>() != null)
                {
                    index1++;
                    sum1 += tradingStockData.SelectToken("data.eod[" + (j) + "].close").Value<float>();
                }
            }

            averageLong0 = sum0 / index0;
            averageLong1 = sum1 / index1;
            sum0 = 0f;
            sum1 = 0f;
            index0 = 0;
            index1 = 0;

            for (int j = 0; j < shortValue; j++)
            {
                if (tradingStockData.SelectToken("data.eod[" + (0 + j) + "].close").Value<string>() != null)
                {
                    index0++;
                    sum0 += tradingStockData.SelectToken("data.eod[" + (0 + j) + "].close").Value<float>();
                }
            }

            for (int j = 1; j <= shortValue; j++)
            {
                if (tradingStockData.SelectToken("data.eod[" + (j) + "].close").Value<string>() != null)
                {
                    index1++;
                    sum1 += tradingStockData.SelectToken("data.eod[" + (j) + "].close").Value<float>();
                }
            }

            averageShort0 = sum0 / index0;
            averageShort1 = sum1 / index1;            

            if (averageLong0 > averageShort0)
            {
                if (averageShort1 > averageLong1)
                {
                    print("S0:" + averageShort0);
                    print("L0:" + averageLong0);
                    print("S1:" + averageShort1);
                    print("L1:" + averageLong1);
                    buttonSignals[i + 150].gameObject.SetActive(true);
                    buttonSignals[i + 150].color = UnityEngine.Color.red;
                    buttonSignals[i + 150].GetComponentInChildren<Text>().text = "SELL CALL";
                    buttonSignals[i + 150].GetComponentInChildren<Text>().color = UnityEngine.Color.white;
                    alertSource.Play();
                    alertImage.SetActive(true);
                    alertImage.GetComponentInChildren<Text>().text = "Alert from Sell Signal";
                    StartCoroutine(DelayAlertShow());
                }
            }
            else if (averageShort0 > averageLong0)
            {
                if (averageLong1 > averageShort1)
                {
                    print("S0:" + averageShort0);
                    print("L0:" + averageLong0);
                    print("S1:" + averageShort1);
                    print("L1:" + averageLong1);
                    buttonSignals[i + 150].gameObject.SetActive(true);
                    buttonSignals[i + 150].color = UnityEngine.Color.green;
                    buttonSignals[i + 150].GetComponentInChildren<Text>().text = "BUY CALL";
                    buttonSignals[i + 150].GetComponentInChildren<Text>().color = UnityEngine.Color.black;
                    alertSource.Play();
                    alertImage.SetActive(true);
                    alertImage.GetComponentInChildren<Text>().text = "Alert from Buy Signal";
                    StartCoroutine(DelayAlertShow());
                }
            }
        }
    }

    IEnumerator SignalW1()
    {
        for (int i = 0; i < StockEngine.instance.stocksID.Length; i++)
        {
            UnityWebRequest wwwW1 = UnityWebRequest.Get("https://api.marketstack.com/v1/tickers/" + StockEngine.instance.stocksID[i] + "/eod?access_key=3c9529a8a320b0c5e8d8a0cf5a4d69e2" + "&offset=0&limit=1000");
            yield return wwwW1.SendWebRequest();

            var tradingStockData = (JObject)JsonConvert.DeserializeObject(wwwW1.downloadHandler.text);

            float sum0 = 0f;
            float averageLong0 = 0f;
            float sum1 = 0f;
            float averageLong1 = 0f;
            float averageShort0 = 0f;
            float averageShort1 = 0f;
            int index0 = 0;
            int index1 = 0;

            for (int j = 0; j < longValue; j++)
            {
                if (tradingStockData.SelectToken("data.eod[" + (0 + j) * 5 + "].close").Value<string>() != null)
                {
                    index0++;
                    sum0 += tradingStockData.SelectToken("data.eod[" + (0 + j) * 5 + "].close").Value<float>();
                }
            }

            for (int j = 1; j <= longValue; j++)
            {
                if (tradingStockData.SelectToken("data.eod[" + (j ) * 5 + "].close").Value<string>() != null)
                {
                    index1++;
                    sum1 += tradingStockData.SelectToken("data.eod[" + (j ) * 5 + "].close").Value<float>();
                }
            }

            averageLong0 = sum0 / index0;
            averageLong1 = sum1 / index1;
            sum0 = 0f;
            sum1 = 0f;
            index0 = 0;
            index1 = 0;

            for (int j = 0; j < shortValue; j++)
            {
                if (tradingStockData.SelectToken("data.eod[" + (0 + j * 5) + "].close").Value<string>() != null)
                {
                    index0++;
                    sum0 += tradingStockData.SelectToken("data.eod[" + (0 + j * 5) + "].close").Value<float>();
                }
            }

            for (int j = 1; j <= shortValue; j++)
            {
                if (tradingStockData.SelectToken("data.eod[" + (j ) * 5 + "].close").Value<string>() != null)
                {
                    index1++;
                    sum1 += tradingStockData.SelectToken("data.eod[" + (j ) * 5 + "].close").Value<float>();
                }
            }

            averageShort0 = sum0 / index0;
            averageShort1 = sum1 / index1;            

            if (averageLong0 > averageShort0)
            {
                if (averageShort1 > averageLong1)
                {
                    print("S0:" + averageShort0);
                    print("L0:" + averageLong0);
                    print("S1:" + averageShort1);
                    print("L1:" + averageLong1);
                    buttonSignals[i + 180].gameObject.SetActive(true);
                    buttonSignals[i + 180].color = UnityEngine.Color.red;
                    buttonSignals[i + 180].GetComponentInChildren<Text>().text = "SELL CALL";
                    buttonSignals[i + 180].GetComponentInChildren<Text>().color = UnityEngine.Color.white;
                    alertSource.Play();
                    alertImage.SetActive(true);
                    alertImage.GetComponentInChildren<Text>().text = "Alert from Sell Signal";
                    StartCoroutine(DelayAlertShow());
                }
            }
            else if (averageShort0 > averageLong0)
            {
                if (averageLong1 > averageShort1)
                {
                    print("S0:" + averageShort0);
                    print("L0:" + averageLong0);
                    print("S1:" + averageShort1);
                    print("L1:" + averageLong1);
                    buttonSignals[i + 180].gameObject.SetActive(true);
                    buttonSignals[i + 180].color = UnityEngine.Color.green;
                    buttonSignals[i + 180].GetComponentInChildren<Text>().text = "BUY CALL";
                    buttonSignals[i + 180].GetComponentInChildren<Text>().color = UnityEngine.Color.black;
                    alertSource.Play();
                    alertImage.SetActive(true);
                    alertImage.GetComponentInChildren<Text>().text = "Alert from Buy Signal";
                    StartCoroutine(DelayAlertShow());
                }
            }
        }
    }

    IEnumerator SignalMN()
    {
        for (int i = 0; i < StockEngine.instance.stocksID.Length; i++)
        {
            UnityWebRequest wwwMN = UnityWebRequest.Get("https://api.marketstack.com/v1/tickers/" + StockEngine.instance.stocksID[i] + "/eod?access_key=3c9529a8a320b0c5e8d8a0cf5a4d69e2" + "&offset=0&limit=1000");
            yield return wwwMN.SendWebRequest();

            var tradingStockData = (JObject)JsonConvert.DeserializeObject(wwwMN.downloadHandler.text);

            float sum0 = 0f;
            float averageLong0 = 0f;
            float sum1 = 0f;
            float averageLong1 = 0f;
            float averageShort0 = 0f;
            float averageShort1 = 0f;
            int index0 = 0;
            int index1 = 0;

            for (int j = 0; j < 40; j++)
            {
                if (tradingStockData.SelectToken("data.eod[" + (0 + j * 10) + "].close").Value<string>() != null)
                {
                    index0++;
                    sum0 += tradingStockData.SelectToken("data.eod[" + (0 + j * 10) + "].close").Value<float>();
                }
            }

            for (int j = 1; j <= 40; j++)
            {
                if (tradingStockData.SelectToken("data.eod[" + (j ) * 10 + "].close").Value<string>() != null)
                {
                    index1++;
                    sum1 += tradingStockData.SelectToken("data.eod[" + (j ) * 10 + "].close").Value<float>();
                }
            }

            averageLong0 = sum0 / index0;
            averageLong1 = sum1 / index1;
            sum0 = 0f;
            sum1 = 0f;
            index0 = 0;
            index1 = 0;

            for (int j = 0; j < shortValue; j++)
            {
                if (tradingStockData.SelectToken("data.eod[" + (0 + j * 10) + "].close").Value<string>() != null)
                {
                    index0++;
                    sum0 += tradingStockData.SelectToken("data.eod[" + (0 + j * 10) + "].close").Value<float>();
                }
            }

            for (int j = 1; j <= shortValue; j++)
            {
                if (tradingStockData.SelectToken("data.eod[" + (1 + j ) * 10 + "].close").Value<string>() != null)
                {
                    index1++;
                    sum1 += tradingStockData.SelectToken("data.eod[" + (1 + j ) * 10 + "].close").Value<float>();
                }
            }

            averageShort0 = sum0 / index0;
            averageShort1 = sum1 / index1;            

            if (averageLong0 > averageShort0)
            {
                if (averageShort1 > averageLong1)
                {
                    print("S0:" + averageShort0);
                    print("L0:" + averageLong0);
                    print("S1:" + averageShort1);
                    print("L1:" + averageLong1);
                    buttonSignals[i + 210].gameObject.SetActive(true);
                    buttonSignals[i + 210].color = UnityEngine.Color.red;
                    buttonSignals[i + 210].GetComponentInChildren<Text>().text = "SELL CALL";
                    buttonSignals[i + 210].GetComponentInChildren<Text>().color = UnityEngine.Color.white;
                    alertSource.Play();
                    alertImage.SetActive(true);
                    alertImage.GetComponentInChildren<Text>().text = "Alert from Sell Signal";
                    StartCoroutine(DelayAlertShow());
                }
            }
            else if (averageShort0 > averageLong0)
            {
                if (averageLong1 > averageShort1)
                {
                    print("S0:" + averageShort0);
                    print("L0:" + averageLong0);
                    print("S1:" + averageShort1);
                    print("L1:" + averageLong1);
                    buttonSignals[i + 210].gameObject.SetActive(true);
                    buttonSignals[i + 210].color = UnityEngine.Color.green;
                    buttonSignals[i + 210].GetComponentInChildren<Text>().text = "BUY CALL";
                    buttonSignals[i + 210].GetComponentInChildren<Text>().color = UnityEngine.Color.black;
                    alertSource.Play();
                    alertImage.SetActive(true);
                    alertImage.GetComponentInChildren<Text>().text = "Alert from Buy Signal";
                    StartCoroutine(DelayAlertShow());
                }
            }
        }
    }

    IEnumerator DelayAlertShow()
    {
        yield return new WaitForSeconds(2f);

        alertImage.SetActive(false);
    }

    public void  MakeLine(float start_x, float start_y, float end_x, float end_y, UnityEngine.Color col)
    {        
        Image line = Instantiate(arrowImage, new Vector2(0, 0), Quaternion.identity);
        line.gameObject.tag = "Clone";
        line.transform.SetParent(GameObject.FindGameObjectWithTag("StockChart").transform, false);
        line.gameObject.SetActive(true);        
        line.rectTransform.sizeDelta = new Vector2(Mathf.Sqrt(Mathf.Pow(end_x - start_x, 2) + Mathf.Pow(end_y - start_y, 2)), line.rectTransform.sizeDelta.y);
        line.transform.position = new Vector2((start_x + end_x) / 2 * Screen.width / 2532f, (start_y + end_y) / 2 * Screen.height / 1170f);
        line.color = col;

        if (end_x - start_x > 0)
        {
            line.rectTransform.Rotate(0, 0, Mathf.Atan((end_y - start_y) / (end_x - start_x)) * 180 / 3.14f);
        }
        else
        {
            line.rectTransform.Rotate(0, 0, Mathf.Atan((end_y - start_y) / (end_x - start_x)) * 180 / 3.14f + 180);
        }
    }
}
