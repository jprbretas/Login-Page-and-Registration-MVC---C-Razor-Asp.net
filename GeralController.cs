using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using padvMVC;
using domit.wdbconn;
using domit.recresource;
using padvMVC.Negocio;
using System.Data.Common;
using System.IO;
using padvMVC.Models;
using System.Data;
using BancoDados;
using System.Globalization;
using System.Threading;
using System.Dynamic;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Net;
using System.Web.Script.Serialization;
using System.Text;
using System.ComponentModel;
using System.Reflection;

namespace padvMVC.Controllers
{
    public abstract class GeralController : Controller
    {

        //public static dbconnection db { get; set; }
        //public static dbcomops_cls dbops { get; set; }
        public static recres rcr { get; set; }
        public static Operations opInt { get; set; }
        public SendEmails vSend { get; set; }
        public static DbConnection objConn;
        

        public GeralController()
        {

            //db = new dbconnection();
            opInt = new Operations();
            //dbops = new dbcomops_cls();
            rcr = new recres();
        }

        public Object ClearFields(Object models)
        {
            PropertyInfo[] properties = models.GetType().GetProperties();
            foreach (PropertyInfo property in properties)
            {
                if (property.PropertyType == typeof(string))
                {
                    if (property.GetValue(models) != null)
                    {
                        var val = property.GetValue(models).ToString().Trim();
                        property.SetValue(models, val);
                    }
                }
            }
            return models;
        }

        public String determinaCodigoCultura(String culture)
        {
            String getCulture = "";

            if (culture != "pt" & culture != "en" & culture != "fr" & culture != "es")
            {
                getCulture = "en-US";
            }
            else
            {
                switch (culture)
                {
                    case "es":
                        {
                            
                            getCulture = "es-ES";
                            break;
                        }

                    case "en":
                        {
                            
                            getCulture = "en-US";
                            break;
                        }

                    case "pt":
                        {
                            
                            getCulture = "pt-BR";
                            break;
                        }
                    case "fr":
                        {
                            
                            getCulture = "fr-FR";
                            break;
                        }
                }
            }

            return getCulture;
        }

        public void SetLanguage(Controller context)
        {
            try
            {

                string culture = "";
                if (Session["culture"] != null && !Session["culture"].ToString().Equals(""))
                {

                    culture = Session["culture"].ToString();
                }
                else
                {
                    culture = "en";
                }

                Session["culture"] = culture;

                //context.TempData.Keep("culture");
                Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture(Session["getCulture"].ToString());
                Thread.CurrentThread.CurrentUICulture = Thread.CurrentThread.CurrentCulture;

                recres rcr = new recres();

                string Success = rcr.ConfigLanguage(context.Server.MapPath("~/padv.cfg"), (string)Session["lang_code"]);
                if (Success == "OK")
                {

                    context.Session["rcr"] = rcr;
                    GeralResource.SetGeral();

                }
                else
                {
                    Console.WriteLine("Tratar erro para falta de instancia ODBC windows");
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public List<Growth_PhasisDto> GetCollumnsPhasis(DataTable tabela, bool compare = false)
        {

            List<Growth_PhasisDto> colunas = new List<Growth_PhasisDto>();

            var index = 0;
            var qtd_phasi = 0;

            if (compare)
            {

                foreach (DataColumn col in tabela.Columns)
                {
                    qtd_phasi++;
                    colunas.Add(new Growth_PhasisDto() { phasis_id = qtd_phasi, phasis_mmc = col.ColumnName });

                }
            }
            else
            {

                foreach (DataColumn col in tabela.Columns)
                {

                    index++;
                    if (index > 4)
                    {
                        qtd_phasi++;
                        colunas.Add(new Growth_PhasisDto() { phasis_id = qtd_phasi, phasis_mmc = col.ColumnName });
                    }
                }
            }
            return colunas;
        }
        public List<string> GetCollumns(DataTable tabela)
        {

            List<string> colunas = new List<string>();

            foreach (DataColumn col in tabela.Columns)
            {

                colunas.Add(col.ColumnName);

            }

            return colunas;
        }
        public DataTable UpdateDefaultTableau(ref DataTable dt, long formula_id)
        {

            try
            {
                formulas_composition formulas_composition = new formulas_composition();
                try
                {
                    var col_composition = formulas_composition.consultarPorFormula(formula_id);
                    var col_composition_order = col_composition.OrderBy(f => f.ingred_id.ingred_id).OrderBy(f => f.phasis_id.phasis_id).ToList();

                    foreach (var item in col_composition_order)
                    {
                        foreach (DataRow row in dt.Rows)
                        {
                            if (row["ingred_id"] != DBNull.Value)
                            {
                                if (row["ingred_id"].ToString() != "")
                                {
                                    var ingred = item.ingred_id.ingred_id;
                                    if (Convert.ToInt32(row["ingred_id"]) == ingred)
                                    {

                                        int iphasis = item.phasis_id.phasis_id;
                                        row[iphasis + 3] = item.ingred_qtty;
                                        row[3] = item.price;
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    opInt.WRITE_ERR_LOG(ex.Message + "_UpdateDefaultTableau1");
                    throw ex;
                }

                Session["dt"] = dt;
                return dt;

            }
            catch (Exception ex)
            {
                opInt.WRITE_ERR_LOG(ex.Message + "_UpdateDefaultTableau2");
                throw ex;
            }
        }
        public DataTable DefaultTableau(long sp_id, long frm_id = 0)
        {
            // cria tabela com tableau para receber ingredientes usados por ração, da espécie 

            DataTable dtTableau = new DataTable();
            long r;
            var retorno = false;

            try
            {

                var usuario = getUser();
                // 
                growth_phasis growth_phasis = new growth_phasis();
                formulas_composition formulas_composition = new formulas_composition();
                Local_Ingred_User util = new Local_Ingred_User();

                DataTable dt_phases = new DataTable();

                if (frm_id == 0)
                {

                    dt_phases = growth_phasis.GetPhasesPorEspecieEUsuario(sp_id, usuario.user_id);
                    if (dt_phases.Rows.Count < 1)
                    {

                        growth_phasis.InsertDefaultGP(usuario.user_id);
                        dt_phases = growth_phasis.GetPhasesPorEspecieEUsuario(sp_id, usuario.user_id);
                    }

                }
                else
                {
                    dt_phases = formulas_composition.GetPhasesPorFormula(frm_id, sp_id, usuario.user_id);
                    if (dt_phases.Rows.Count < 1)
                    {

                        growth_phasis.InsertDefaultGP(usuario.user_id);
                        dt_phases = growth_phasis.GetPhasesPorEspecieEUsuario(sp_id, usuario.user_id);
                    }
                }

                try
                {

                    dtTableau.Columns.Add("ingred_id", typeof(string));  // col 0
                    dtTableau.Columns.Add("line_type", typeof(string));  // col 1
                    dtTableau.Columns.Add("Ration definition", typeof(string));   // col 2
                    dtTableau.Columns.Add("price", typeof(string));

                    foreach (DataRow item in dt_phases.Rows)
                    {
                        dtTableau.Columns.Add(item["phasis_mmc"].ToString(), typeof(float));
                    }

                    dtTableau.Rows.Add("", "", "");
                    // replace na unidade usada
                    var unidd = "kg";
                    if (usuario.system_id == 2)
                    {

                        unidd = "ld";
                    }
                    dtTableau.Rows.Add("19", "C", GeralResource.RecRes(12).Replace("{md}", unidd)); // title 'feed budget' 1 line type 'C'
                    dtTableau.Rows.Add("", "T", GeralResource.RecRes(11));    // title 'ingred inclusion' line type 2

                    // 
                    exist_ingredients exist_ingredients = new exist_ingredients();
                    var col_ingredientes = exist_ingredients.ConsultaringredientesParaFeedProgram(0);
                    var filter_ingreds = col_ingredientes.Where(f => f.ingred_line != "C").OrderBy(f => f.ingred_order).ToList();

                    // tab exist_ingredientes: line_type 'I' ingrediente; 'O' outros; 'C' consumo
                    // seleciona só os 'I'ngredientes

                    // ingreds, inicia na linha 2

                    // adiciona linhas com ingredientes

                    foreach (var item in filter_ingreds)
                    {
                        var resource = 0;
                        if (item.resource_id != null)
                        {

                            resource = item.resource_id.Value;
                        }
                        string name_ingrediente = "";
                        if (resource == 0)
                        {
                            name_ingrediente = item.ingred_mmc;
                        }
                        else
                        {
                            name_ingrediente = GeralResource.RecRes(resource);
                        }
                        if (resource != 115) // foi tirado o Oil/fat por enquanto
                        {
                            dtTableau.Rows.Add(item.ingred_id, item.ingred_line, name_ingrediente);
                        }
                    }
                    foreach (DataRow it_rw in dtTableau.Rows)
                    {
                        for (int _col = 3; _col < dtTableau.Columns.Count; _col++)
                        {
                            var ingred_qtty = it_rw[_col];
                            if (it_rw["line_type"].ToString() == "C" && ingred_qtty == DBNull.Value)
                            { // caso formula nova default 1 para peso
                                it_rw[_col] = 1;
                            }
                        }
                    }
                    try
                    {

                        //dtTableau.Rows.Add("16", "O", GeralResource.RecRes(115));
                        dtTableau.Rows.Add("17", "O", GeralResource.RecRes(116));
                        Session["dt"] = dtTableau;
                        dtTableau.Dispose();
                    }
                    catch (Exception ex)
                    {

                        opInt.WRITE_ERR_LOG(ex.Message + "_DefaultTableau3");
                        throw ex;
                    }
                    long ing_id = 0;
                    dtTableau.AcceptChanges();
                    foreach (DataRow row in dtTableau.Rows)
                    {
                        var local = row["line_type"].ToString();
            
                        bool flag = true;

                        if (local == "L")
                        {

                            ing_id = Convert.ToInt64( row["ingred_id"]);
                            flag = util.VerificarIngUser(usuario.user_id, ing_id);

                            if (!flag)
                            {

                                row.Delete();
                            }
                        }
                    }
                    dtTableau.AcceptChanges();

                    return dtTableau;
                }
                catch (Exception ex)
                {

                    opInt.WRITE_ERR_LOG(ex.Message + "_DefaultTableau");
                    throw ex;
                }
            }
            catch (Exception ex)
            {

                dtTableau.Dispose();
                opInt.WRITE_ERR_LOG(ex.Message + "_DefaultTableau1");
                throw ex;
            }
        }
        /*public static void SetRequestState(HttpApplication current)
        {

            try//Mudando a linguagem conforme a sessão do usuario 
            {

                //var dataKey = "__ControllerTempData";
                //var dataDict = current.Session[dataKey] as IDictionary<string, object>;
                string culture = "";

                if (current.Session["langcode"] != null)
                {
                    culture = current.Session["langcode"].ToString();
                }
                else
                {
                    culture = "US";
                }

                var rcr = (recres)current.Session["rcr"];
                GeralResource.SetGeral();

                if (culture != "")
                {

                    Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture(culture);
                    Thread.CurrentThread.CurrentUICulture = new CultureInfo(culture);
                }
            }
            catch(Exception e)
            {

                Console.WriteLine(e.Message.ToString());
            };
        }*/
        public IFormatProvider GetCurrentProvider(Controller current)
        {

            try
            {

                var provider = new CultureInfo(Session["lang_code"].ToString());
                return provider;
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
        /*public static string GetLangCode(Controller current)
        {

            try
            {

                string langcode = current.Session["langcode"].ToString();
                return langcode;
            }
            catch (Exception ex)
            {

                throw ex;
            }
        } */
        public List<dynamic> GetModelTable(DataTable tabela)
        {

            var Model = new List<dynamic>();

            foreach (DataRow row in tabela.Rows)
            {

                var obj = (IDictionary<string, object>)new ExpandoObject();

                foreach (DataColumn col in tabela.Columns)
                {

                    obj.Add(col.ColumnName, row[col.ColumnName]);
                    
                }
                Model.Add(obj);
            }
            return Model;
        }
        public List<dynamic> GetModelTableFilled(DataTable tabela)
        {

            var Model = new List<dynamic>();

            foreach (DataRow row in tabela.Rows)
            {

                var obj = (IDictionary<string, object>)new ExpandoObject();
                string rep = "-";

                foreach (DataColumn col in tabela.Columns)
                {
                    if(row[col.ColumnName].ToString()=="")
                    {
                        obj.Add(col.ColumnName, rep);
                    }
                    else
                    {
                        obj.Add(col.ColumnName, row[col.ColumnName]);
                    }
                                      
                }
                Model.Add(obj);
            }



            return Model;
            
        }
        public string GetPwdMasterSystem()
        {
            var retorno = "";
            try
            {

                confg_system confg_system = new confg_system();
                var collection = confg_system.Consultar();
                var filter = collection.Where(f => f.codmaster == "SYS").ToList();
                if (filter.Count > 0)
                {

                    string pwd_master_login = filter[0].pwd_master_login;
                }

                return retorno;
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
        public long GetLangId(Controller current, string lang)
        {

            long retorno = 1;

            
            lang = Session["lang_code"].ToString();

            var x = PwdEncript.criptograph.Descriptografar("zEDSC8Lxh/P88a+UO9jQqw==");

            dbconnection db = new dbconnection();

            db.CFG_APLICATIVO = current.Server.MapPath("~/padv.cfg");
            opInt.ERR_LOGERRFILENAME = current.Server.MapPath("~/App_Data/") + Properties.Settings.Default.ErrLogFileName;
            db.setERRLOGFILENAME = opInt.ERR_LOGERRFILENAME;
           
            if (!db.LOADCONFIG)
            {

                retorno = 0;
                throw new Exception();
            }
            ConfigDB.Conexao_Parametros("DOMIT_SERVER", db.getDBNAME, db.getDBPWD, db.getDBTYPE, db.getDBUSER);

           var ret = ConfigDB.GetInstanciaCon().TestConnection();

            current.Session["rcr"] = rcr;
            try
            {

                exist_languages exist_languages = new exist_languages();
               // var languages = exist_languages.Consultar();


                if (lang == null)
                {
                    
                    Session["lang_code"] = exist_languages.LangViaId(Convert.ToInt64(Session["lang_id"]));
                    
                }
            }
            catch (Exception ex)
            {

                throw ex;
            }
            return retorno;
        }
        public string RenderViewToString(string viewName, object model)
        {

            ViewData.Model = model;
            using (var sw = new StringWriter())
            {

                var viewResult = ViewEngines.Engines.FindPartialView(ControllerContext, viewName);
                var viewContext = new ViewContext(ControllerContext, viewResult.View, ViewData, TempData, sw);
                viewResult.View.Render(viewContext, sw);
                viewResult.ViewEngine.ReleaseView(ControllerContext, viewResult.View);
                return sw.GetStringBuilder().ToString();
            }
        }
        public static void RemoveReferences(ModelStateDictionary modelState, string nameobjct)
        {

            string expressionText = ExpressionHelper.GetExpressionText(nameobjct);

            foreach (var ms in modelState.ToArray())
            {

                if (ms.Key.StartsWith(expressionText + ".") || ms.Key == expressionText)
                {

                    modelState.Remove(ms);
                }
            }
        }
        public void Log_Write(long sUser, int iAction, int iKeyType, string sKey, string sDetail)
        {

            try
            {

                if (sDetail.Length > 500)
                {

                    sDetail = sDetail.Substring(0, 500);
                }
                gLogSisDto gLogSisDto = new gLogSisDto();
                gLogSisDto.DETALHES = sDetail;
                gLogSisDto.ACAO = iAction;
                gLogSisDto.TIPO_CHAVE = iKeyType;
                gLogSisDto.user_id = sUser;
                gLogSisDto.CHAVE = sKey;
                gLogSisDto.CODSISTEMA = 50;

                glogsis glogsis = new glogsis();
                string retorno = glogsis.Incluir(gLogSisDto);
                if (retorno == "0")
                {

                    throw new Exception();
                }
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
        public void paises(long country_id = 0)
        {

            exist_countries exist_countries = new exist_countries();
            var collection_countries = exist_countries.Consultar(0);

            collection_countries.Insert(0, new CountriesDto { name = GeralResource.RecRes(623), country_id = 0 });

            var arrSelectList = new SelectList(collection_countries, "country_id", "name", country_id);

            ViewBag.userLog = getUser();
            ViewBag.paises = arrSelectList;
        }
        public List<Exist_UsersDto> Buscar(List<Exist_UsersDto> collection, string buscarpor, string coluna)
        {

            var qtd = 0;
            if (coluna != "" && coluna != "chbx")
            {

                for (int i = 0; i < collection.Count; i++)
                {

                    PropertyInfo prop = collection[i].GetType().GetProperty(coluna);
                    if (prop.GetValue(collection[i]).ToString().ToUpper().Contains(buscarpor.Trim().ToUpper()))
                    {

                        var item = collection[i];
                        collection.Remove(collection[i]);
                        collection.Insert(0, item);
                        qtd++;
                    }
                }
            }
            ViewBag.ResultSearch = qtd;
            return collection;
        }
        public void GetDropDownRegioes()
        {

            try
            {

                region region = new region();
                var regioes = region.Consultar();
                ViewBag.regioes = new SelectList(regioes, "region_id", "name");
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
        public void tratamentos(string set_trat ="")
        {

            ViewBag.tratamentos = "";
            List<SelectListItem> tratamentos = new List<SelectListItem>();
            string item1 = GeralResource.RecRes(181).ToUpper();
            string item2 = GeralResource.RecRes(182).ToUpper();
            string item3 = GeralResource.RecRes(183).ToUpper();
            string item4 = GeralResource.RecRes(184).ToUpper();

            tratamentos.Add(new SelectListItem { Value = "" });
            tratamentos.Add(new SelectListItem { Value = "1", Text = item1 });
            tratamentos.Add(new SelectListItem { Value = "2", Text = item2 });
            tratamentos.Add(new SelectListItem { Value = "3", Text = item3 });
            tratamentos.Add(new SelectListItem { Value = "4", Text = item4 });

            var list = new SelectList(tratamentos, "Value", "Text", set_trat);
            ViewBag.tratamentos = list;
        }
        public Exist_UsersDto getUser()
        {

            try
            {

                exist_users util = new exist_users();

                var usuario = (Exist_UsersDto)Session["usuario"];
                if (usuario != null)
                {

                    usuario = util.consultarUsuario(usuario.user_id);
                }

                return usuario;
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
        public  Exist_UsersDto SetFormulaSession()
        {
            try
            {

            }
            catch (Exception ex)
            {

                throw ex;
            }
            return null;
        }
        public string StringToString(string aString)
        {

            int i = 0;

            aString = aString.Replace(".", "");

            return aString;
        }
        public string GetLocalIpAddress()
        {

            UnicastIPAddressInformation mostSuitableIp = null;

            var networkInterfaces = NetworkInterface.GetAllNetworkInterfaces();

            foreach (var network in networkInterfaces)
            {

                if (network.OperationalStatus != OperationalStatus.Up)
                    continue;

                var properties = network.GetIPProperties();

                if (properties.GatewayAddresses.Count == 0)
                    continue;

                foreach (var address in properties.UnicastAddresses)
                {

                    if (address.Address.AddressFamily != AddressFamily.InterNetwork)
                        continue;

                    if (IPAddress.IsLoopback(address.Address))
                        continue;

                    if (!address.IsDnsEligible)
                    {

                        if (mostSuitableIp == null)
                            mostSuitableIp = address;
                        continue;
                    }

                    // The best IP is the IP got from DHCP server
                    if (address.PrefixOrigin != PrefixOrigin.Dhcp)
                    {

                        if (mostSuitableIp == null || !mostSuitableIp.IsDnsEligible)
                            mostSuitableIp = address;
                        continue;
                    }
                    return address.Address.ToString();
                }
            }

            return mostSuitableIp != null
                ? mostSuitableIp.Address.ToString()
                : "";
        }
        public string Shorten(string longUrl)
        {

            string login = "suportedev2domit";
            string apikey = "R_028c0d370e4a4cd6a1441c50f2298c6b";

            var url = string.Format("https://api-ssl.bitly.com/shorten?format=json&version=2.0.1&longUrl={0}&login={1}&apiKey={2}", HttpUtility.UrlEncode(longUrl), login, apikey);
           // var url = string.Format("https://api-ssl.bitly.com/v3/shorten?access_token={0}&longUrl={1}",apikey, HttpUtility.UrlEncode(longUrl));

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            try
            {

                WebResponse response = request.GetResponse();
                using (Stream responseStream = response.GetResponseStream())
                {

                    StreamReader reader = new StreamReader(responseStream, Encoding.UTF8);
                    JavaScriptSerializer js = new JavaScriptSerializer();
                    dynamic jsonResponse = js.Deserialize<dynamic>(reader.ReadToEnd());
                    string s = jsonResponse["results"][longUrl]["shortUrl"];
                    return s;
                }
            }
            catch (WebException ex)
            {

                WebResponse errorResponse = ex.Response;
                using (Stream responseStream = errorResponse.GetResponseStream())
                {

                    StreamReader reader = new StreamReader(responseStream, Encoding.GetEncoding("utf-8"));
                    String errorText = reader.ReadToEnd();
                    // log errorText
                }
                throw ex;
            }
        }
        public static DataTable ToDataTable<T>(IList<T> data)
        {
            PropertyDescriptorCollection properties =
                TypeDescriptor.GetProperties(typeof(T));
            DataTable table = new DataTable();
            foreach (PropertyDescriptor prop in properties)
                table.Columns.Add(prop.Name, Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType);
            foreach (T item in data)
            {
                DataRow row = table.NewRow();
                foreach (PropertyDescriptor prop in properties)
                    row[prop.Name] = prop.GetValue(item) ?? DBNull.Value;
                table.Rows.Add(row);
            }
            return table;
        }
        public string CreatePassword()
        {

            const string valid = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";
            var length = 5;
            StringBuilder res = new StringBuilder();
            Random rnd = new Random();
            while (0 < length--)
            {

                res.Append(valid[rnd.Next(valid.Length)]);
            }
            return res.ToString();
        }
        public double CalculateEntropy(string password)
        {

            var cardinality = 0;
            // Password contains lowercase letters.
            if (password.Any(c => char.IsLower(c)))
            {

                cardinality += 1;
            } // Password contains uppercase letters.
            if (password.Any(c => char.IsUpper(c)))
            {
                cardinality += 1;
            }// Password contains numbers.
            if (password.Any(c => char.IsDigit(c)))
            {

                cardinality += 1;
            }// Password contains symbols.
            if (password.IndexOfAny("\\|¬¦`!\"£$%^&*()_+-=[]{};:'@#~<>,./? ".ToCharArray()) >= 0)
            {

                cardinality += 1;
            }
            if (password.Length < 8 || password.Length > 16 || cardinality < 2)
            {

                cardinality = 0;
            }
            return cardinality;
        }
    }
}