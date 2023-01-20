// Copyright  2015-2020 Pico Technology Co., Ltd. All Rights Reserved.


using LitJson;
using System.Collections.Generic;

public class CommonDic
{
    private static CommonDic dic;
    private static Dictionary<string, string> mydic;
    public static CommonDic getInstance()
    {
        if (dic == null)
        {
            dic = new CommonDic();
        }
        if (mydic == null)
        {
            mydic = new Dictionary<string, string>();
            InitDicData(mydic);
        }
        return dic;
    }

    public Dictionary<string, string> getDic()
    {
        return mydic;
    }

    private string _app_ID = null;
    private string _app_Key = null;

    private string _user_token = null;
    private string _access_token = "";
    private string _open_id = null;
    private string _refresh_token = null;
    private string _expires_in = null;

    private string _user_info = null;

    private string _subject = null;
    private string _body = null;
    private string _order_id = null;
    private string _total = null;
    private string _goods_tag = null;
    private string _notify_url = null;
    private string _trade_type = null;
    private string _pay_code = "";

    private string _order_info = null;

    private string _code = null;
    private string _msg = "null";


    private string _merchant_ID = null;
    private string _payKey = null;

    private string _isSuucess = null;
    private string _loginMsg = null;

    public void setParameters(string name, string value)
    {
        if (name.Equals("subject"))
        {
            subject = value;
        }
        if (name.Equals("body"))
        {
            body = value;
        }
        if (name.Equals("order_id"))
        {
            order_id = value;
        }
        if (name.Equals("total"))
        {
            total = value;
        }
        if (name.Equals("goods_tag"))
        {
            goods_tag = value;
        }
        if (name.Equals("notify_url"))
        {
            notify_url = value;
        }
        if (name.Equals("trade_type"))
        {
            trade_type = value;
        }
        if (name.Equals("pay_code"))
        {
            pay_code = value;
        }
    }

    public string PayOrderString()
    {
        string json = JsonMapper.ToJson(CommonDic.getInstance());
        return json;
    }

    public string subject
    {
        get { return _subject; }
        set { _subject = value; }
    }
    public string body
    {
        get { return _body; }
        set { _body = value; }
    }
    public string order_id
    {
        get { return _order_id; }
        set { _order_id = value; }
    }
    public string total
    {
        get { return _total; }
        set { _total = value; }
    }
    public string goods_tag
    {
        get { return _goods_tag; }
        set { _goods_tag = value; }
    }
    public string notify_url
    {
        get { return _notify_url; }
        set { _notify_url = value; }
    }
    public string pay_code
    {
        get { return _pay_code; }
        set { _pay_code = value; }
    }

    public string trade_type
    {
        get { return _trade_type; }
        set { _trade_type = value; }
    }

    public string user_token
    {
        get { return _user_token; }
        set { _user_token = value; }
    }
    public string access_token
    {
        get { return _access_token; }
        set { _access_token = value; }
    }

    public string open_id
    {
        get { return _open_id; }
        set { _open_id = value; }
    }

    public string refresh_token
    {
        get { return _refresh_token; }
        set { _refresh_token = value; }
    }

    public string expires_in
    {
        get { return _expires_in; }
        set { _expires_in = value; }
    }

    public string isSuccess
    {
        get { return _isSuucess; }
        set { _isSuucess = value; }
    }

    public string loginMsg
    {
        get { return _loginMsg; }
        set { _loginMsg = value; }
    }

    public string user_info
    {
        get { return _user_info; }
        set { _user_info = value; }
    }

    public string order_info
    {
        get { return _order_info; }
        set { _order_info = value; }
    }

    public string code
    {
        get { return _code; }
        set { _code = value; }
    }

    public string msg
    {
        get { return _msg; }
        set { _msg = value; }
    }

    public string app_ID
    {
        get { return _app_ID; }
        set { _app_ID = value; }
    }

    public string app_Key
    {
        get { return _app_Key; }
        set { _app_Key = value; }
    }

    public string merchant_ID
    {
        get { return _merchant_ID; }
        set { _merchant_ID = value; }
    }

    public string paykey
    {
        get { return _payKey; }
        set { _payKey = value; }
    }

    public static void InitDicData(Dictionary<string, string> mydic)
    {
        mydic.Add("00000", "网络异常");
        mydic.Add("10000", "登录成功");
        mydic.Add("10001", "用户未登陆");
        mydic.Add("10002", "请输入正确金额");
        mydic.Add("10003", "登陆过期，请重新登陆");
        mydic.Add("11000", "商户验证成功");
        mydic.Add("11001", "商户验证失败");
        mydic.Add("11002", "用户验证参数错误或请求过期");
        mydic.Add("11003", "商户未验证");
        mydic.Add("12000", "支付成功");
        mydic.Add("12001", "支付失败");
        mydic.Add("12003", "P币不足");
        mydic.Add("12004", "余额可用");
        mydic.Add("13000", "生成订单");
        mydic.Add("13001", "获取数据失败");
        mydic.Add("13002", "生成订单失败");
        mydic.Add("14000", "查询订单成功");
        mydic.Add("14001", "订单不存在/有误");
        mydic.Add("14002", "用户取消支付操作");
        mydic.Add("15000", "未输入商品信息");
        mydic.Add("15001", "未输入预付ID");
        mydic.Add("15002", "请输入Pico支付订单号或商户订单号");
        mydic.Add("NOAUTH", "商户无此接口权限");
        mydic.Add("SYSTEMERROR", "系统错误");
        mydic.Add("APP_ID_NOT_EXIST", "APP_ID不存在");
        mydic.Add("MCHID_NOT_EXIST", "MCHID不存在");
        mydic.Add("APP_ID_MCHID_NOT_MATCH", "app_id和mch_id不匹配ID");
        mydic.Add("LACK_PARAMS", "缺少参数");
        mydic.Add("SIGNERROR", "签名错误");
        mydic.Add("NO_DATA", "没有查询到数据");
    }
}
