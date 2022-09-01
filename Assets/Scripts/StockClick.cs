using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class StockClick : MonoBehaviour
{
    public static StockClick instance;
    public Text dashboardComName;
    public Text rightComName;
    public Text rightSubName;
    public Text rightLPrice;
    public Text rightLCurrency;
    public Text rightPChange;
    public Text rightOpen;
    public Text rightHigh;
    public Text rightLow;
    public Text rightClose;
    public Text rightVolume;
    public Text rightMarketCap;
    public Text rightStockVolume;
    public Image rightPChangeBox;

    void Awake()
    {
        instance = this;
    }

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
        for (int i = 0; i < StockEngine.instance.stocksName.Length; i++)
        {
            if (this.GetComponentsInChildren<Text>()[0].text == StockEngine.instance.stocksName[i])
            {
                dashboardComName.text = StockEngine.instance.stocksName[i];
                rightComName.text = StockEngine.instance.stocksName[i];
                rightSubName.text = StockEngine.instance.stocksName[i];
                rightLPrice.text = "" + StockEngine.instance.stocksLPrice[i];   
                rightLCurrency.text = "" + StockEngine.instance.stocksCurrency[i];
                rightOpen.text = "" + StockEngine.instance.stocksOpen[i];
                rightHigh.text = "" + StockEngine.instance.stocksHigh[i];
                rightLow.text = "" + StockEngine.instance.stocksLow[i];
                rightClose.text = "" + StockEngine.instance.stocksClose[i];
                rightMarketCap.text = "" + StockEngine.instance.stocksMarketCap[i];
                rightStockVolume.text = "" + Math.Round((StockEngine.instance.stocksVolume[i]/1000000f), 1) + "M";    

                if(StockEngine.instance.stocksPChange[i] >= 0)
                {
                    rightPChangeBox.color = new Color32(17, 58, 36, 255);
                    rightPChange.color = Color.green;
                    rightPChange.text = "(+" + Math.Round(StockEngine.instance.stocksPChange[i], 2) + "%)";
                }  
                else
                {
                    rightPChangeBox.color = new Color32(106, 28, 11, 255);
                    rightPChange.color = Color.red;
                    rightPChange.text = "(" + Math.Round(StockEngine.instance.stocksPChange[i], 2) + "%)";
                }

                TradingView.instance.DrawTradingView(StockEngine.instance.stocksID[i]);
                TradingView.instance.StockRange(StockEngine.instance.stocksID[i]);
                TradingView.instance.selectedStockID = StockEngine.instance.stocksID[i];
            }
        }
    }
}
