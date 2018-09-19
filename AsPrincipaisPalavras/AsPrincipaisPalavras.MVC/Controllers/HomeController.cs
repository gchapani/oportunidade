using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using System.Xml;

namespace AsPrincipaisPalavras.MVC.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            string _url = "https://www.minutoseguros.com.br/blog/feed";

            var httpRequest = (HttpWebRequest)WebRequest.Create(_url);
            var response = (HttpWebResponse)httpRequest.GetResponse();

            var getStream = response.GetResponseStream();

            var xmlDoc = new XmlDocument();

            xmlDoc.Load(getStream);

            getStream.Close();

            List<List<string>> retorno = new List<List<string>>();
            Dictionary<string, int> TopicoPalavras = new Dictionary<string, int>();

            string _TextoSemFormatacao;

            //Recupera as informações de texto das Tags e passa para a lista
            foreach (XmlNode node in xmlDoc.SelectNodes("rss/channel/item"))
            {
                List<string> r = new List<string>();

                foreach (XmlNode n in node.ChildNodes)
                {
                    bool valido = true;

                    if (n.Name == "title")
                    {
                        TopicoPalavras.Add(n.InnerText, 0);
                    }

                    //Remove as informações de Data
                    valido = !DateTime.TryParse(n.InnerText, out DateTime _DateTime);

                    //Remove as informações de números
                    valido = valido && !int.TryParse(n.InnerText, out int _int);

                    //Remove as informações de URL
                    valido = valido && !(Uri.TryCreate(n.InnerText, UriKind.Absolute, out Uri uriResult) && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps));

                    if (valido)
                    {
                        //Remove as tags HTML
                        _TextoSemFormatacao = Regex.Replace(n.InnerText.ToLower(), "<.*?>", String.Empty);

                        //Remove acentos
                        _TextoSemFormatacao = RemoveAccents(_TextoSemFormatacao);

                        //Remove caracteres especiais
                        _TextoSemFormatacao = Regex.Replace(_TextoSemFormatacao, @"[^0-9A-Za-z ,]+", " ");

                        r.Add(_TextoSemFormatacao.Trim());
                    }
                }

                retorno.Add(r);
            }

            //Contabiliza as palavras
            Dictionary<string, int> ContaPalavras = new Dictionary<string, int>();
            int totalPalavras = 0;
            int i = 0;

            foreach (List<string> l1 in retorno)
            {
                foreach (string l2 in l1)
                {
                    string[] palavras = l2.Split(' ');

                    foreach (string palavra in palavras)
                    {
                        string p = palavra.Trim().Replace(",", "").Replace(".", "");

                        if (p != "" && RemovePrepositionsArticle(p) && !int.TryParse(p, out int _int))
                        {
                            totalPalavras++;

                            if (ContaPalavras.Keys.Where(x => x == p).Count() == 0)
                            {
                                ContaPalavras.Add(p, 1);
                            }
                            else
                            {
                                ContaPalavras[p]++;
                            }
                        }
                    }
                }

                TopicoPalavras[TopicoPalavras.ElementAt(i).Key] = totalPalavras;
                i++;
            }

            ViewBag.Palavras = ContaPalavras.OrderByDescending(x => x.Value);
            ViewBag.TopicoPalavras = TopicoPalavras;

            return View();
        }

        static string RemoveAccents(string text)
        {
            var normalizedString = text.Normalize(NormalizationForm.FormD);
            var stringBuilder = new StringBuilder();

            foreach (var c in normalizedString)
            {
                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(c);
                }
            }

            return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
        }

        static bool RemovePrepositionsArticle(string palavra)
        {
            if (palavra == "o" ||
                palavra == "os" ||
                palavra == "a" ||
                palavra == "as" ||
                palavra == "um" ||
                palavra == "uns" ||
                palavra == "uma" ||
                palavra == "umas" ||
                palavra == "a" ||
                palavra == "ao" ||
                palavra == "aos" ||
                palavra == "à" ||
                palavra == "às" ||
                palavra == "de" ||
                palavra == "do" ||
                palavra == "dos" ||
                palavra == "da" ||
                palavra == "das" ||
                palavra == "dum" ||
                palavra == "duns" ||
                palavra == "duma" ||
                palavra == "dumas" ||
                palavra == "em" ||
                palavra == "no" ||
                palavra == "nos" ||
                palavra == "na" ||
                palavra == "nas" ||
                palavra == "num" ||
                palavra == "nuns" ||
                palavra == "numa" ||
                palavra == "numas" ||
                palavra == "por" ||
                palavra == "pelo" ||
                palavra == "pelos" ||
                palavra == "pela" ||
                palavra == "pelas")
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}