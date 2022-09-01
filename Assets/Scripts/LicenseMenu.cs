using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Net.NetworkInformation;

public class LicenseMenu : MonoBehaviour
{
    public GameObject mainCanvas;
    public Text deviceID;
    public InputField serialID;
    public Text toastInvalid;

    string deviceNum;
    public bool toastShowing = false;
    float time = 1.5f;

    void Start()
    {
        mainCanvas.transform.localScale = new Vector3(Screen.width / 2532f, Screen.height / 1170f, 1f);        
        
        if(string.Equals(PlayerPrefs.GetString("Activate"), "Activated"))
        {
            mainCanvas.SetActive(false);
            SceneManager.LoadScene("StockChart");
        }

        // Which Platform???
        if(Application.platform == RuntimePlatform.WindowsPlayer ||
            Application.platform == RuntimePlatform.WindowsEditor)
        {
            deviceNum = GetMACAddress().ToString();
            string temp = "";

            for (int i = 0; i < deviceNum.Length; i++)
            {
                temp += deviceNum[deviceNum.Length - i - 1];

                if(i % 4 == 3 && i != deviceNum.Length - 1)
                {
                    temp += "-";
                }
            }

            deviceID.text = temp;
        }
        else if(Application.platform == RuntimePlatform.Android)
        {
            deviceID.text = GetDeviceID();
        }
        else if(Application.platform == RuntimePlatform.IPhonePlayer)
        {
            deviceID.text = SystemInfo.deviceUniqueIdentifier.ToString();
        }
    }

    private void Update()
    {
        mainCanvas.transform.localScale = new Vector3(Screen.width / 2532f, Screen.height / 1170f, 1f);

        // Toast showing
        if (toastShowing)
        {
            time -= Time.deltaTime;
        }

        if (time < 0)
        {
            toastShowing = false;
            toastInvalid.gameObject.SetActive(false);
        }
    }

    public void OkBtnClick()
    {
        if(string.Equals(serialID.text, GenerateKey(deviceID.text)) ||
            string.Equals(serialID.text, GenerateKeyFormat(GenerateKey(deviceID.text))))
        {
            PlayerPrefs.SetString("Activate", "Activated");
            SceneManager.LoadScene("StockChart");
        }
        else if(serialID.text == "")
        {
            toastInvalid.text = "Please enter license.";
            toastInvalid.gameObject.SetActive(true);
            toastShowing = true;
            time = 1.5f;
        }
        else
        {
            toastInvalid.text = "Invalid license.";
            toastInvalid.gameObject.SetActive(true);
            toastShowing = true;
            time = 1.5f;
        }
    }

    private object GetMACAddress()
    {
        string macAddresses = "";

        foreach (NetworkInterface nic in NetworkInterface.GetAllNetworkInterfaces())
        {
            if (nic.OperationalStatus == OperationalStatus.Up)
            {
                macAddresses += nic.GetPhysicalAddress().ToString();
                break;
            }
        }

        return macAddresses;
    }

    static string GenerateKey(string deviceID)
    {
        string temp = "";
        
        for(int i = 0; i < deviceID.Length; i++)
        {
            if(deviceID[i] == '-')
            {
                continue;
            }

            temp += deviceID[i];
        }

        deviceID = temp;
        
        System.Text.Encoder enc = System.Text.Encoding.Unicode.GetEncoder();
        byte[] unicodeText = new byte[deviceID.Length * 2];
        enc.GetBytes(deviceID.ToCharArray(), 0, deviceID.Length, unicodeText, 0, true);
        MD5 md5 = new MD5CryptoServiceProvider();
        byte[] result = md5.ComputeHash(unicodeText);

        StringBuilder sb = new StringBuilder();

        for (int i = 0; i < result.Length; i++)
        {
            sb.Append(result[i].ToString("X2"));
        }

        string serial = "";

        for(int i = 0; i < 16; i++)
        {
            serial += sb.ToString()[i * 2 + 1];
        }

        return serial;
    }

    static string GenerateKeyFormat(string serial)
    {
        string serialFormat = "";

        for (int i = 0; i < 16; i++)
        {
            serialFormat += serial[i];
            
            if (i % 4 == 3 && i != 15)
            {
                serialFormat += "-";
            }
        }

        return serialFormat;
    }

    // Get Android DeviceID
    public static string GetDeviceID()
    {
        // Get Android ID
        AndroidJavaClass clsUnity = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        AndroidJavaObject objActivity = clsUnity.GetStatic<AndroidJavaObject>("currentActivity");
        AndroidJavaObject objResolver = objActivity.Call<AndroidJavaObject>("getContentResolver");
        AndroidJavaClass clsSecure = new AndroidJavaClass("android.provider.Settings$Secure");

        string android_id = clsSecure.CallStatic<string>("getString", objResolver, "android_id");

        // Get bytes of Android ID
        System.Text.UTF8Encoding ue = new System.Text.UTF8Encoding();
        byte[] bytes = ue.GetBytes(android_id);

        // Encrypt bytes with md5
        System.Security.Cryptography.MD5CryptoServiceProvider md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
        byte[] hashBytes = md5.ComputeHash(bytes);

        // Convert the encrypted bytes back to a string (base 16)
        string hashString = "";

        for (int i = 0; i < hashBytes.Length; i++)
        {
            hashString += System.Convert.ToString(hashBytes[i], 16).PadLeft(2, '0');
        }

        string temp = hashString.PadLeft(32, '0');
        string device_id = "";

        for(int i = 0; i < 12; i++)
        {
            device_id += temp[i * 2 + 1];

            if(i % 4 == 3 && i != 11)
            {
                device_id += "-";
            }
        }

        device_id = device_id.ToUpper();

        return device_id;
    }
}