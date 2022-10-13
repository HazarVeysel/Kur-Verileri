using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Xml;
using ExchangeRates.Models;
using HtmlAgilityPack;

namespace ExchangeRates.Controllers
{
    public class HomeController2 : Controller
    {
        public ActionResult Index()
        {


            //XmlDocument xml = new XmlDocument();                                //XML döküman oluşturuyoruz.
            //string PreviousDate = "http://www.tcmb.gov.tr/kurlar/202203/18032022.xml";
            //string today = "http://www.tcmb.gov.tr/kurlar/today.xml";
            //xml.Load(PreviousDate);                                             //Load ile bağlantı kuruyoruz.
            //var Tarih_Date_Nodes = xml.SelectSingleNode("//Tarih_Date");        //Count değerini almak için ana boğumu seçiyoruz.
            //var CurrencyNodes = Tarih_Date_Nodes.SelectNodes("//Currency");     //Ana boğum altındaki kur boğumunu seçiyoruz.
            //int CurrencyLength = CurrencyNodes.Count;                           // toplam kur boğumu sayısını elde ediyor ve for döngüsünde kullanıyoruz.
            //DateTime Tarih = Convert.ToDateTime(xml.SelectSingleNode("//Tarih_Date").Attributes["Tarih"].Value);
            //List<Currency> currencies = new List<Currency>();
            //for (int i = 0; i < CurrencyLength; i++)
            //{
            //    var cn = CurrencyNodes[i]; // kur boğumunu alıyoruz.

            //    currencies.Add(new Currency
            //    {
            //        Kod = cn.Attributes["Kod"].Value,
            //        CrossOrder = cn.Attributes["CrossOrder"].Value,
            //        CurrencyCode = cn.Attributes["CurrencyCode"].Value,
            //        Unit = cn.ChildNodes[0].InnerXml,
            //        Isim = cn.ChildNodes[1].InnerXml,
            //        CurrencyName = cn.ChildNodes[2].InnerXml,
            //        ForexBuying = cn.ChildNodes[3].InnerXml,
            //        ForexSelling = cn.ChildNodes[4].InnerXml,
            //        BanknoteBuying = cn.ChildNodes[5].InnerXml,
            //        BanknoteSelling = cn.ChildNodes[6].InnerXml,
            //        CrossRateOther = cn.ChildNodes[8].InnerXml,
            //        CrossRateUSD = cn.ChildNodes[7].InnerXml,
            //    });
            //}
            //ViewData["Tarih"] = Tarih;
            List<Currency> MerkezBankasi = VeriGetir(DateTime.Today);
            YapiKrediVeriGetir(MerkezBankasi);

            foreach (var item in MerkezBankasi)
            {

            }

            //var tarihim = Convert.ToDateTime(DateTime.Now.AddDays(-7).ToShortDateString()); //Geçmiş tarih
            //var tarihimbugun = Convert.ToDateTime(DateTime.Now.ToShortDateString()); //Bugünün tarihi

            //ViewData["currencies"] = VeriGetir(tarihim);      // dovizler List değerini data'ya atıyoruz ön tarafta viewbag ile çekeceğiz.
            return View(MerkezBankasi);
        }


        private string addZero(int p)
        {
            if (p.ToString().Length == 1)
                return "0" + p;
            return p.ToString();
        }

        public List<Currency> VeriGetir(DateTime tarih)
        {
            string url = string.Empty;
            var date = Convert.ToDateTime(DateTime.Now.ToShortDateString());
            if (date.Date == DateTime.Today)
            {
                url = "http://www.tcmb.gov.tr/kurlar/today.xml";
            }
            else
            {
                url = string.Format("http://www.tcmb.gov.tr/kurlar/{0}{1}/{2}{1}{0}.xml", tarih.Year, addZero(tarih.Month), addZero(tarih.Day));

            }




            XmlDocument xml = new XmlDocument();                                //XML döküman oluşturuyoruz.
                                                                                //string PreviousDate = "http://www.tcmb.gov.tr/kurlar/202203/18032022.xml";
                                                                                //string today = "http://www.tcmb.gov.tr/kurlar/today.xml";
            xml.Load(url);                                             //Load ile bağlantı kuruyoruz.
            var Tarih_Date_Nodes = xml.SelectSingleNode("//Tarih_Date");        //Count değerini almak için ana boğumu seçiyoruz.
            var CurrencyNodes = Tarih_Date_Nodes.SelectNodes("//Currency");     //Ana boğum altındaki kur boğumunu seçiyoruz.
            int CurrencyLength = CurrencyNodes.Count;                           // toplam kur boğumu sayısını elde ediyor ve for döngüsünde kullanıyoruz.
            DateTime Tarih = Convert.ToDateTime(xml.SelectSingleNode("//Tarih_Date").Attributes["Tarih"].Value);
            List<Currency> currencies = new List<Currency>();
            for (int i = 0; i < CurrencyLength; i++)
            {
                var cn = CurrencyNodes[i]; // kur boğumunu alıyoruz.

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

            ViewData["Tarih"] = Tarih;

            return currencies;

        }



        public List<Currency> YapiKrediVeriGetir(List<Currency> MerkezBankasi)
        {

            List<Currency> cur = new List<Currency>();

            using (WebClient client = new WebClient())                      // Html'i indirmek için bir İstemci Oluşturuyoruz.
            {
                var link = "https://www.yapikredi.com.tr/yatirimci-kosesi/doviz-bilgileri";
                var uri = new Uri(link);                                    // link yazan alana verisini okumak istediğimiz web sayfasının bağlantısını yazıyoruz.
                var host = uri.Host;                                        // bu kısım verdiğimiz linkin Base Url (Merkez Bağlantısı Örn: "http://www.fatihbas.net/2019/04/19/cpu-sicakligi/" adresinden bize sadece "www.fatihbas.net" i döndürüyor)'ni döndürüyor.
                var scheme = uri.Scheme;                                    // bu kısım ise girmiş olduğumuz linkin "HTTP" veya "HTTPS" olup olmadığını döndürüyor.

                client.Encoding = Encoding.UTF8;                            // sayfa kodlama karakter ailesinin UTF-8 olduğunu belirtiyoruz (%90 web sayfaları UTF-8 Olarak kodlanmaktadır)

                string html = client.DownloadString(link);                  // bu satırda ise web sayfasının içeriğini indiriyoruz.

                                                                            // Bir HtmlDocument Oluşturarak indirmiş olduğumuz HTML ifadesini içerisine yüklüyoruz.
                HtmlAgilityPack.HtmlDocument htmlDocument = new HtmlAgilityPack.HtmlDocument();
                htmlDocument.LoadHtml(html);

                // artık parse işlemine geçebiliriz

                HtmlNodeCollection htmlNodes = htmlDocument.DocumentNode.SelectNodes("//tbody[@id='currencyResultContent']//tr"); // burada dikkat edilmesi gereken bir konu, ben birden fazla elemanı döndürmek istediğim için class'ı kullandım. gelen ifade düz metin olduğu için jQuery gibi gelişmiş değil dolayısı ile elementin sahip olduğu class'ı olduğu gibi veriyorum. Burada www.fatihbas.net i baz alarak size örneklendirmeye devam edeceğim
             

                if (htmlNodes != null)
                {
                    //bu alanda istediğimiz sonuçları okumaya yakın olduğumuzu görüyoruz.
                    foreach (HtmlNode node in htmlNodes)
                    {
                        // son olarak gelen elemanları sırası ile burada okuyacağız.
                        HtmlAgilityPack.HtmlDocument _subDocument = new HtmlAgilityPack.HtmlDocument();
                        _subDocument.LoadHtml(node.InnerHtml);
                        // Gelen nesnemizin alt elemanlarını okurken sorun yaşamamamız için yeni bir HtmlDocument oluşturuyoruz.
                        // Ve artık istediğimiz dataları okuyabiliriz.
                        HtmlNode DovizKodu = _subDocument.DocumentNode.SelectSingleNode("td[1]");
                        HtmlNode YapiKrediAlis = _subDocument.DocumentNode.SelectSingleNode("td[3]");
                        HtmlNode YapiKrediSatis = _subDocument.DocumentNode.SelectSingleNode("td[4]");

                        // linkNode Değişkeninde sayfamda bulunan "Devamını Oku (Read More)" butonunu getirmiş oldum.
                        //string kurlar = satirlar.Attributes["td"].Value; // Devamını Oku butonunun içerisinde ki bağlantıyı almış oldum.

                        //HtmlNode icerikNode = _subDocument.DocumentNode.SelectSingleNode("td");
                        string kod = DovizKodu.InnerText;

                        if (MerkezBankasi.Any(x=>x.CurrencyCode== kod))
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

                        string icerik = YapiKrediAlis.InnerHtml + " " + YapiKrediSatis.InnerHtml; // iceriklerimi HTML Olarak alıyorum. Yanlış Hatırlamıyorsam .InnerHtml yerine ".InnerText" yazarak direkt olarak HTML etiketleri (tag) olmadan salt metni alabilirsiniz. 
                    }
                }
            }
            return MerkezBankasi;
        }
    }
}