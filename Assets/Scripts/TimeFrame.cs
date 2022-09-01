using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TimeFrame : MonoBehaviour
{
    public Text[] timeFrameText;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ElementClick()
    {
        for (int i = 0; i < timeFrameText.Length; i++)
        {
            timeFrameText[i].text = this.GetComponentsInChildren<Text>()[0].text;
        }

        switch (this.GetComponentsInChildren<Text>()[0].text)
        {
            case "1M":
                {
                    TradingView.instance.multiplier = "1";
                    TradingView.instance.timeSpan = "minute";
                    TradingView.instance.DrawTradingView(TradingView.instance.selectedStockID);
                    break;
                }

            case "5M":
                {
                    TradingView.instance.multiplier = "5";
                    TradingView.instance.timeSpan = "minute";
                    TradingView.instance.DrawTradingView(TradingView.instance.selectedStockID);
                    break;
                }

            case "15M":
                {
                    TradingView.instance.multiplier = "15";
                    TradingView.instance.timeSpan = "minute";
                    TradingView.instance.DrawTradingView(TradingView.instance.selectedStockID);
                    break;
                }

            case "30M":
                {
                    TradingView.instance.multiplier = "30";
                    TradingView.instance.timeSpan = "minute";
                    TradingView.instance.DrawTradingView(TradingView.instance.selectedStockID);
                    break;
                }

            case "Hr":
                {
                    TradingView.instance.multiplier = "1";
                    TradingView.instance.timeSpan = "hour";
                    TradingView.instance.DrawTradingView(TradingView.instance.selectedStockID);
                    break;
                }
            case "H4":
                {
                    TradingView.instance.multiplier = "4";
                    TradingView.instance.timeSpan = "hour";
                    TradingView.instance.DrawTradingView(TradingView.instance.selectedStockID);
                    break;
                }

            case "D+":
                {
                    TradingView.instance.multiplier = "1";
                    TradingView.instance.timeSpan = "day";
                    TradingView.instance.DrawTradingView(TradingView.instance.selectedStockID);
                    break;
                }

            case "D":
                {
                    TradingView.instance.multiplier = "1";
                    TradingView.instance.timeSpan = "day";
                    TradingView.instance.DrawTradingView(TradingView.instance.selectedStockID);
                    break;
                }

            case "W+":
                {
                    TradingView.instance.multiplier = "1";
                    TradingView.instance.timeSpan = "week";
                    TradingView.instance.DrawTradingView(TradingView.instance.selectedStockID);
                    break;
                }

            case "W":
                {
                    TradingView.instance.multiplier = "1";
                    TradingView.instance.timeSpan = "week";
                    TradingView.instance.DrawTradingView(TradingView.instance.selectedStockID);
                    break;
                }
            case "M+":
                {
                    TradingView.instance.multiplier = "1";
                    TradingView.instance.timeSpan = "month";
                    TradingView.instance.DrawTradingView(TradingView.instance.selectedStockID);
                    break;
                }

            case "M":
                {
                    TradingView.instance.multiplier = "1";
                    TradingView.instance.timeSpan = "month";
                    TradingView.instance.DrawTradingView(TradingView.instance.selectedStockID);
                    break;
                }
            case "KLSE":
                {
                    for (int i = 0; i < 30; i++)
                    {
                        StockEngine.instance.stocksID[i] = StockEngine.instance.totalStocksID[i + 0 * 30];
                        StockEngine.instance.stocksCurrency[i] = "MYR";
                    }

                    StockEngine.instance.GetMainStockInfo();

                    break;
                }
            case "SGX":
                {
                    for (int i = 0; i < 30; i++)
                    {
                        StockEngine.instance.stocksID[i] = StockEngine.instance.totalStocksID[i + 1 * 30];
                        StockEngine.instance.stocksCurrency[i] = "SGD";
                    }

                    StockEngine.instance.GetMainStockInfo();

                    break;
                }
            case "NASDAQ":
                {
                    for (int i = 0; i < 30; i++)
                    {
                        StockEngine.instance.stocksID[i] = StockEngine.instance.totalStocksID[i + 2 * 30];
                        StockEngine.instance.stocksCurrency[i] = "USD";
                    }

                    StockEngine.instance.GetMainStockInfo();

                    break;
                }
            case "NYSE":
                {
                    for (int i = 0; i < 30; i++)
                    {
                        StockEngine.instance.stocksID[i] = StockEngine.instance.totalStocksID[i + 3 * 30];
                        StockEngine.instance.stocksCurrency[i] = "USD";
                    }

                    StockEngine.instance.GetMainStockInfo();

                    break;
                }
            case "AMEX":
                {
                    for (int i = 0; i < 30; i++)
                    {
                        StockEngine.instance.stocksID[i] = StockEngine.instance.totalStocksID[i + 4 * 30];
                        StockEngine.instance.stocksCurrency[i] = "USD";
                    }

                    StockEngine.instance.GetMainStockInfo();

                    break;
                }
            case "JSX":
                {
                    for (int i = 0; i < 30; i++)
                    {
                        StockEngine.instance.stocksID[i] = StockEngine.instance.totalStocksID[i + 5 * 30];
                        StockEngine.instance.stocksCurrency[i] = "RP";
                    }

                    StockEngine.instance.GetMainStockInfo();

                    break;
                }
            case "HKSE":
                {
                    for (int i = 0; i < 30; i++)
                    {
                        StockEngine.instance.stocksID[i] = StockEngine.instance.totalStocksID[i + 6 * 30];
                        StockEngine.instance.stocksCurrency[i] = "HKD";
                    }

                    StockEngine.instance.GetMainStockInfo();

                    break;
                }
            case "SET":
                {
                    for (int i = 0; i < 30; i++)
                    {
                        StockEngine.instance.stocksID[i] = StockEngine.instance.totalStocksID[i + 7 * 30];
                        StockEngine.instance.stocksCurrency[i] = "B";
                    }

                    StockEngine.instance.GetMainStockInfo();

                    break;
                }

        }        
    }
}
