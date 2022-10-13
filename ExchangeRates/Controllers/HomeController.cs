using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Xml;
using ExchangeRates.Models;
using HtmlAgilityPack;

namespace ExchangeRates.Controllers
{
    public class HomeController : Controller
    {



        public async Task<ActionResult> Index()
        {



            await Task.Run(() => KurCek());

            using (ExchangeRatesEntities db = new ExchangeRatesEntities())
            {
                db.Tbl_DovizKurlari.RemoveRange(db.Tbl_DovizKurlari.ToList()); // save yapmadan önce bütün dataları siliyor. yoksa her veri çektiğinde altına ekliyordu
                foreach (var item in MerkezBankasi)
                {
                    db.Tbl_DovizKurlari.Add(new Tbl_DovizKurlari
                    {
                        Adi = item.Isim,
                        COGNISA = "0",
                        COGNISS = "0",
                        Guncelleyen ="Sistem",
                        GuncellemeTarihi = DateTime.Now.ToShortDateString(),
                        Kod = item.Kod,
                        DovizAlis = item.ForexBuying,
                        DovizSatis= item.ForexSelling,
                        EfektifAlis=item.BanknoteBuying,
                        EfektifSatis= item.BanknoteSelling,
                        Tarih=ViewBag.Tarih.ToShortDateString(),
                        Tur=1,
                        YapiKrediAlis=item.YapiKrediAlis,
                        YapiKrediSatis=item.YapiKrediSatis,                        
                    });
                    
                    db.SaveChanges();
                }
            }
            //var tarihim = Convert.ToDateTime(DateTime.Now.AddDays(-7).ToShortDateString()); //Geçmiş tarih
            //var tarihimbugun = Convert.ToDateTime(DateTime.Now.ToShortDateString()); //Bugünün tarihi

            //ViewData["currencies"] = VeriGetir(tarihim);      // dovizler List değerini data'ya atıyoruz ön tarafta viewbag ile çekeceğiz.
            return View(MerkezBankasi);
        }
        List<Currency> MerkezBankasi;
        private async Task<string> KurCek()
        {

            MerkezBankasi = VeriGetir(DateTime.Today);

            YapiKrediVeriGetir(MerkezBankasi);

            return "ok";
        }

        public List<Currency> VeriGetir(DateTime tarih)
        {
            string url = string.Empty;
           var date = Convert.ToDateTime(DateTime.Now.ToShortDateString());

            //var date = Convert.ToDateTime(new DateTime(2022, 03, 20, 9, 5, 0)); // Bu iki satır eski tarihleri denemek için abi
            //tarih = date;

            List<Currency> currencies = new List<Currency>();
            if (date.Date == DateTime.Today)
            {
                url = "http://www.tcmb.gov.tr/kurlar/today.xml";
            }
            else
            {
                url = string.Format("http://www.tcmb.gov.tr/kurlar/{0}{1}/{2}{1}{0}.xml", tarih.Year, tarih.Month.ToString().PadLeft(2, '0'), tarih.Day.ToString().PadLeft(2, '0'));

            }


            try
            {
                XmlDocument xml = new XmlDocument();                                //XML döküman oluşturuyoruz.
                                                                                    //string PreviousDate = "http://www.tcmb.gov.tr/kurlar/202203/18032022.xml";
                                                                                    //string today = "http://www.tcmb.gov.tr/kurlar/today.xml";
                xml.Load(url);                                             //Load ile bağlantı kuruyoruz.
                var Tarih_Date_Nodes = xml.SelectSingleNode("//Tarih_Date");        //Count değerini almak için ana boğumu seçiyoruz.
                var CurrencyNodes = Tarih_Date_Nodes.SelectNodes("//Currency");     //Ana boğum altındaki kur boğumunu seçiyoruz.
                int CurrencyLength = CurrencyNodes.Count;                           // toplam kur boğumu sayısını elde ediyor ve for döngüsünde kullanıyoruz.
                DateTime Tarih = Convert.ToDateTime(xml.SelectSingleNode("//Tarih_Date").Attributes["Tarih"].Value);

                for (int i = 0; i < CurrencyLength; i++)
                {
                    var cn = CurrencyNodes[i];

                    currencies.Add(new Currency
                    {
                        Kod = cn.Attributes["Kod"].Value,
                        CrossOrder = cn.Attributes["CrossOrder"].Value,
                        CurrencyCode = cn.Attributes["CurrencyCode"].Value,
                        Unit = cn.ChildNodes[0].InnerXml,
                        Isim = cn.ChildNodes[1].InnerXml,
                        CurrencyName = cn.ChildNodes[2].InnerXml,
                        ForexBuying = cn.ChildNodes[3].InnerXml,
                        ForexSelling = cn.ChildNodes[4].InnerXml,
                        BanknoteBuying = cn.ChildNodes[5].InnerXml,
                        BanknoteSelling = cn.ChildNodes[6].InnerXml,
                        CrossRateOther = cn.ChildNodes[8].InnerXml,
                        CrossRateUSD = cn.ChildNodes[7].InnerXml,
                    });
                }

                ViewData["Tarih"] = tarih;

                return currencies;
            }
            catch (WebException ex)
            {

                if (ex.Response != null)
                {
                    // 404 not found
                    HttpWebResponse errorResponse = ex.Response as HttpWebResponse;
                    if (errorResponse.StatusCode == HttpStatusCode.NotFound)
                    {
                        // bir gün öncesi kontrol edilir
                        tarih = tarih.AddDays(-1);
                        return VeriGetir(tarih);
                    }
                    else
                    {
                        throw new Exception("Kur bilgisi bulunamadı.");
                    }
                }
                else
                {
                    throw new Exception("Kur bilgisi bulunamadı.");
                }
            }
        }





        public List<Currency> YapiKrediVeriGetir(List<Currency> MerkezBankasi)
        {

            List<Currency> cur = new List<Currency>();

            using (WebClient client = new WebClient())
            {
                var link = "https://www.yapikredi.com.tr/yatirimci-kosesi/doviz-bilgileri";
                var uri = new Uri(link);
                var host = uri.Host;
                var scheme = uri.Scheme;

                client.Encoding = Encoding.UTF8;

                string html = client.DownloadString(link);


                HtmlAgilityPack.HtmlDocument htmlDocument = new HtmlAgilityPack.HtmlDocument();
                htmlDocument.LoadHtml(html);


                HtmlNodeCollection htmlNodes = htmlDocument.DocumentNode.SelectNodes("//tbody[@id='currencyResultContent']//tr");


                if (htmlNodes != null)
                {

                    foreach (HtmlNode node in htmlNodes)
                    {

                        HtmlAgilityPack.HtmlDocument _subDocument = new HtmlAgilityPack.HtmlDocument();
                        _subDocument.LoadHtml(node.InnerHtml);


                        HtmlNode DovizKodu = _subDocument.DocumentNode.SelectSingleNode("td[1]");
                        HtmlNode YapiKrediAlis = _subDocument.DocumentNode.SelectSingleNode("td[3]");
                        HtmlNode YapiKrediSatis = _subDocument.DocumentNode.SelectSingleNode("td[4]");


                        //string kurlar = satirlar.Attributes["td"].Value; // Devamını Oku butonunun içerisinde ki bağlantıyı almış oldum.

                        //HtmlNode icerikNode = _subDocument.DocumentNode.SelectSingleNode("td");
                        string kod = DovizKodu.InnerText;

                        if (MerkezBankasi.Any(x => x.CurrencyCode == kod))
                        {
                            Currency Obj = MerkezBankasi.SingleOrDefault(x => x.CurrencyCode == kod);
                            Obj.YapiKrediAlis = YapiKrediAlis.InnerHtml;
                            Obj.YapiKrediSatis = YapiKrediSatis.InnerHtml;
                        }

                        //cur.Add(new Currency
                        //{
                        //    YapiKrediAlis = YapiKrediAlis.InnerHtml,
                        //    YapiKrediSatis=YapiKrediSatis.InnerHtml
                        //});

                        string icerik = YapiKrediAlis.InnerHtml + " " + YapiKrediSatis.InnerHtml;
                    }
                }
            }
            return MerkezBankasi;
        }
    }
}