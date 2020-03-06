using domit.recresource;
using domit.wdbconn;
using padvMVC.Models;
using padvMVC.Negocio;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text.RegularExpressions;
using System.Web.Mvc;

namespace padvMVC.Controllers
{

    public class LoginController : GeralController
    {

        public long var;
        public Exist_UsersDto usuario = new Exist_UsersDto();
        public string MSG_ERROR_SENDER = "";
        private Operations opIntGr = new Operations();
        private bool CheckDateExpiration(DateTime DataSenha)
        {

            var retorno = false;
            if (string.IsNullOrEmpty(DataSenha.ToString()))
            {

                retorno = true;
            }
            else if (DataSenha < DateTime.Now)
            {

                retorno = true;
            }
            return retorno;
        }
        public ActionResult AlterarIdioma(string language, string lang_id)
        {

            try
            {

                Session["lang_id"] = lang_id;
                Session["lang_code"] = language;
                Session["culture"] = language;
                SetLanguage(this);

                return Json(new { status = "success" });
            }
            catch (Exception ex)
            {

                return Json(new { status = "error" });
            }
        }
        protected bool CheckUserMatrixTable()
        {

            var retorno = false;
            try
            {

                exist_base_tables exist_base_tables = new exist_base_tables();
                exist_users exist_users = new exist_users();

                try
                {

                    usuario = getUser();
                    long table_id = usuario.table_id.Value;
                    var collection = exist_base_tables.Consultar();
                    var filter = collection.Where(f => f.table_id == table_id).ToList();

                    if (filter.Count > 0)
                    {

                        retorno = true;
                    }
                    else
                        try
                        {

                            filter = collection.Where(f => f.table_isdefault == true).ToList();

                            if (filter.Count > 0)
                            {

                                string retorno_qr = exist_users.AtualizarTabelaDeAnalise(usuario.user_id, filter[0].table_id);
                                if (retorno_qr == "1")
                                {

                                    retorno = true;
                                }
                            }
                        }
                        catch (Exception ex)
                        {

                            opInt.WRITE_ERR_LOG(ex.Message + " _CheckUserMatrixTable1");
                        }
                }
                catch (Exception ex)
                {

                    opInt.WRITE_ERR_LOG(ex.Message + " _CheckUserMatrixTable2");
                    throw new Exception();
                }
            }
            catch (Exception ex)
            {

                opInt.WRITE_ERR_LOG(ex.Message + " _CheckUserMatrixTable3");
                throw new Exception();
            }

            return retorno;
        }
        protected bool CheckUserMeasureConfig()
        {

            var retorno = false;
            exist_measure_config exist_measure_config = new exist_measure_config();

            try
            {

                try
                {

                    var usuario = getUser();
                    var collection = exist_measure_config.Consultar();
                    var filter = collection.Where(f => f.measure_config_id == usuario.measure_config_id).ToList();

                    if (collection.Count > 0)
                    {

                        retorno = true;
                    }
                }
                catch (Exception ex)
                {

                    opInt.WRITE_ERR_LOG(ex.Message + " _CheckUserMeasureConfig1");
                    throw ex;
                }
            }
            catch (Exception ex)
            {

                opInt.WRITE_ERR_LOG(ex.Message + " _CheckUserMeasureConfig2");
                throw ex;
            }

            return retorno;
        }
        private string GetIPAddress()
        {

            string sIPAddress;
            sIPAddress = Request.ServerVariables["REMOTE_ADDR"];

            return sIPAddress;
        }
        protected bool CreateUserDefaultMeasureConfig()
        {

            var retorno = false;
            try
            {

                try
                {

                    exist_measure_config exist_measure_config = new exist_measure_config();
                    measure_config measure_config = new measure_config();
                    exist_users exist_users = new exist_users();

                    var usuario = getUser();
                    string retorno_qr = exist_measure_config.Incluir(usuario.user_name);
                    if (retorno_qr != "1")
                    {

                        throw new Exception();
                    }
                    try
                    {

                        retorno_qr = measure_config.Incluir(Convert.ToInt64(retorno_qr));
                        if (retorno_qr == "1")
                        {

                            long measure_config_id = Convert.ToInt64(retorno_qr);

                            retorno_qr = exist_users.AlterarConfiguracaoMedida(measure_config_id, usuario.user_id);
                            if (retorno_qr == "1")
                            {

                                Session["measure_config_id"] = measure_config_id;
                                retorno = true;
                            }
                        }
                    }
                    catch (Exception ex)
                    {

                        opInt.WRITE_ERR_LOG(ex.Message + " _CreateUserDefaultMeasureConfig2");
                        throw ex;
                    }
                }
                catch (Exception ex)
                {

                    opInt.WRITE_ERR_LOG(ex.Message + " _CreateUserDefaultMeasureConfig3");
                    throw ex;
                }
            }
            catch (Exception ex)
            {
                opInt.WRITE_ERR_LOG(ex.Message + " _CreateUserDefaultMeasureConfig1");
                throw ex;
            }

            return retorno;
        }
        protected bool CheckUserSystem()
        {

            string retorno = "";
            bool retornar = false;
            try
            {

                exist_users exist_users = new exist_users();
                Measure_Systems Measure_Systems = new Measure_Systems();

                try
                {

                    usuario = getUser();
                    var system_id = usuario.system_id;
                    var collection = Measure_Systems.Consultar(system_id.Value);

                    if (collection.Count > 0)
                    {

                        retornar = true;
                    }
                    else
                        try
                        {

                            system_id = Measure_Systems.GetSistemaMedidaPadrao();

                            retorno = exist_users.AlterarSistemaMedida(usuario.user_id, system_id.Value);

                            if (retorno.ToString() == "1")
                            {

                                retornar = true;
                            }
                        }
                        catch (Exception ex)
                        {

                            opInt.WRITE_ERR_LOG(ex.Message + " _CheckUserSystem1");
                        }
                }
                catch (Exception ex)
                {

                    opInt.WRITE_ERR_LOG(ex.Message + " _CheckUserSystem2");
                }
            }
            catch (Exception ex)
            {

                opInt.WRITE_ERR_LOG(ex.Message + " _CheckUserSystem3");
            }
            return retornar;
        }
        protected bool PrepareUserDefaults()
        {

            var retorno = false;
            try
            {

                if (!CheckUserMeasureConfig())
                {

                    if (!CreateUserDefaultMeasureConfig())
                    {

                        return retorno;
                    }
                }
                if (!CheckUserMatrixTable())
                {

                    return retorno;
                }
                if (!CheckUserSystem())
                {

                    return retorno;
                }
                retorno = true;
                var usuario = getUser();
                
            }
            catch (Exception ex)
            {

                throw ex;
            }

            return retorno;
        }
        public ActionResult Logar(FormCollection form)
        {

            bool resp;

            exist_users exist_users = new exist_users();

            string user_email = form["email"].ToString().Trim().ToLower();
            string senha = form["senha"].ToString().Trim();

            string pwd = "";
            try
            {

                try
                {

                    usuario = exist_users.consultarUsuario(user_email);
                }
                catch (Exception ex)
                {

                    opInt.WRITE_ERR_LOG(ex.Message + " _BtnLogin5");
                    return Json(new { status = "error", msg = GeralResource.RecRes(561) });
                }

                if (usuario == null)
                {
                    return Json(new { status = "validation", msg = GeralResource.RecRes(585) });
                }
                else
                {

                    pwd = usuario.user_password;
                    pwd = PwdEncript.criptograph.Descriptografar(pwd);
                    var pwd_master = GetPwdMasterSystem();

                    if (pwd == senha || pwd_master == senha)
                    {

                        bool bornot = false;
                        if (usuario.user_blocked == null)
                            bornot = false;
                        else if (usuario.user_blocked == false)
                            bornot = false;
                        else
                            bornot = true;

                        if ((bornot == false))
                        {

                            Session["usuario"] = usuario;
                            Session["user_id"] = usuario.user_id;
                            Session["lang_id"] = usuario.lang_id.lang_id;
                            Session["culture"] = usuario.lang_id.lang_mmc;
                            Session["lang_code"] = usuario.lang_id.lang_mmc;

                            ViewBag.usuario = usuario;

                            SetLanguage(this);

                            try
                            {

                                long lang_id = Convert.ToInt64(Session["lang_id"]);
                                long user_id = usuario.user_id;
                                exist_users.AlterarIdioma(user_id, lang_id);
                            }
                            catch (Exception ex)
                            {

                                opInt.WRITE_ERR_LOG(ex.Message + " - " + "" + " _BtnLogin4");
                            }

                            string orig_name = Properties.Settings.Default.MG1; // '"magic1.xlsx"
                            string dest_name = Session["user_id"].ToString().Trim() + "_" + orig_name;
                            System.IO.File.Delete(Server.MapPath("~/App_Data/" + dest_name));
                            string orig_name1 = "magic2.xlsx";
                            string dest_name1 = Session["user_id"].ToString().Trim() + "_" + orig_name1;
                            System.IO.File.Delete(Server.MapPath("~/App_Data/" + dest_name1));
                            // 

                            Session["logado"] = true;

                            // Verifica data expirada
                            Session["license_expired"] = false;

                            if (usuario.user_accesslevel != 9)
                            {

                                if ((usuario.user_date_expire != null))
                                {

                                    if (CheckDateExpiration(usuario.user_date_expire.Value))
                                    {

                                        try
                                        {

                                            Session["license_expired"] = true;
                                            paises(usuario.user_country);
                                            tratamentos(usuario.treatment);

                                            var model = RenderViewToString("~/Views/Home/Form.cshtml", usuario);
                                            return Json(new { status = "license_expired", view = model });

                                        }
                                        catch (Exception ex)
                                        {

                                            throw ex;
                                        }
                                    }
                                }
                                if (CalculateEntropy(pwd) < 2 && usuario.user_accesslevel != 9)
                                {

                                    try
                                    {

                                        paises();
                                        tratamentos();
                                        ViewBag.fd_disable = true;
                                        var model = RenderViewToString("~/Views/Home/FormPass.cshtml", usuario);
                                        return Json(new { status = "temp_password", view = model });
                                    }
                                    catch (Exception ex)
                                    {

                                        throw ex;
                                    }
                                }
                            }
                        }
                    }
                    else
                    {

                        return Json(new { status = "validation", msg = GeralResource.RecRes(585) });
                    }
                }
            }
            catch (Exception ex)
            {

                opInt.WRITE_ERR_LOG(ex.Message + " _BtnLogin1");
                return Json(new { status = "error", msg = GeralResource.RecRes(561) });
            }
            try
            {

                if (PrepareUserDefaults())
                    return Json(new { status = "success" });
                else
                {

                    return Json(new { status = "error", msg = GeralResource.RecRes(561) });
                }
            }
            catch (Exception ex)
            {

                return Json(new { status = "error", msg = GeralResource.RecRes(561) });
            }
        }
        public ActionResult Register(Exist_UsersDto Model)
        {

            exist_users exist_users = new exist_users();

            Exist_UsersDto usuarioSolicitante = Model;

            exist_users.GetConnection().CriaTransaction();
            try
            {

                string cSenha = Request.Form["user_password_confirm"];
                object retorno = new object();
                long userCountry;
                List<string> admsEmail = new List<string>();
                List<string> admsEmailMaster = new List<string>();
                long userReg = 0;

                bool regStat = true;
                bool isNew = true;
                var validate = true;

                countries_for_region regXpais = new countries_for_region();
                regXpais.SetConnection(exist_users.GetConnection());

                userCountry = Model.user_country;
                userReg = regXpais.PegarRegiaoViaPais(userCountry);

                var license_expire = Convert.ToBoolean(Session["license_expired"]);
                if (!license_expire)
                { // licença não expirada 

                    RemoveReferences(ModelState, "user_country");

                    int flag = 0;
                    int i = 0;

                    foreach(var item in ModelState.Values)
                    {

                        flag = flag + ModelState.Values.ElementAt(i).Errors.Count;
                        i++;
                    }

                    if (!ModelState.IsValid && flag>1)   
                    {

                        paises();
                        tratamentos();
                        string contentView = RenderViewToString("~/Views/Home/Form.cshtml", Model);
                        return Json(new { status = "validation", view = contentView });
                    }
                    if (Model.user_country == 0)
                    {

                        ModelState.AddModelError("user_country", GeralResource.RecRes(583));
                        validate = false;
                    }
                    if (Model.user_password != Model.user_password_confirm)
                    {

                        ModelState.AddModelError("user_password", GeralResource.RecRes(120));
                        validate = false;
                    }
                    if (CalculateEntropy(Model.user_password) < 2)
                    {

                        ModelState.AddModelError("user_password", GeralResource.RecRes(626));
                        validate = false;
                    }
                    if (!validate)
                    {

                        paises();
                        tratamentos();
                        string contentView = RenderViewToString("~/Views/Home/Form.cshtml", Model);
                        return Json(new { status = "validation", view = contentView });
                    }
                    regStat = exist_users.ExistUser(Model.user_email);
                    isNew = exist_users.IsNewUser(Model.user_email);

                    if (!regStat && isNew)
                    {

                        retorno = exist_users.Incluir(Model);
                    }
                    if (regStat)
                    {

                        return Json(new { status = "error", message = "Este email já está cadastrado" });
                    }

                    if (Model._action == 2)
                    {

                        return Json(new { status = "error", message = GeralResource.RecRes(587) });
                    }
                }
                else
                {
                    // licença expirada altera titulo do e-mail e body e-mail
                    Session["license_expired"] = false;
                }
                try
                {

                    admsEmail = exist_users.pegarOsAdms(userReg);
                    admsEmailMaster = exist_users.pegarOsMasters();

                    foreach (var email in admsEmail)
                    {

                        AdmEmailGetterAndSender(email, usuarioSolicitante, license_expire); //(ativar depois)
                    }
                    foreach (var email in admsEmailMaster)
                    {

                       AdmEmailGetterAndSender(email, usuarioSolicitante, license_expire); //(ativar depois)
                    }
                }
                catch (Exception ex)
                {

                    exist_users.GetConnection().TransRoll();
                    return Json(new { status = "error", message = GeralResource.RecRes(561) });
                }
                if (!license_expire)
                {  // confirmacao de inclusao somente depois envio de e-mail's

                    exist_users.GetConnection().TransCommit();
                }
                return Json(new { status = "success", message = GeralResource.RecRes(584) });
            }
            catch (Exception ex)
            {

                return Json(new { status = "error", message = GeralResource.RecRes(561) });
            }
        }
        public ActionResult recoverPass(string user_email)
        {

            try
            {

                bool flag = true;
                List<string> admsEmail = new List<string>();
                List<string> admsEmailMaster = new List<string>();
                exist_users user = new exist_users();
                countries_for_region regXpais = new countries_for_region();
                long userCountry;
                long userReg = 0;
                Exist_UsersDto Models = new Exist_UsersDto();
                if (IsValidEmail(user_email) == false || user.ExistEmail(user_email) == false)
                {

                    return Json(new { status = "validation", message = "email inválido ou inexistente" });
                }
                Models = user.consultarUsuario(user_email);
                userCountry = Models.user_country;
                userReg = regXpais.PegarRegiaoViaPais(userCountry);
                admsEmail = user.pegarOsAdms(userReg);
                admsEmailMaster = user.pegarOsMasters();
                Models.user_password = CreatePassword();

                foreach (var email in admsEmail)
                {

                    AdmEmailGetterAndSenderRecover(email, Models);
                }
                foreach (var email in admsEmailMaster)
                {

                    AdmEmailGetterAndSenderRecover(email, Models);

                }
                if (IsValidEmail(user_email) == false)
                {

                    return Json(new { status = "validation", message = "email inválido" });
                }
                flag = getAndSendEmail(user_email.ToString());

                if (flag)
                {

                    return Json(new { status = "success", message = GeralResource.RecRes(590) });
                }
                else
                {

                    return Json(new { status = "validation", message = GeralResource.RecRes(591) });
                }
            }
            catch (Exception ex)
            {

                return Json(new { status = "error", message = "error" });
            }
        }
        public bool AdmEmailGetterAndSender(string email, Exist_UsersDto userLogado, bool expire_license)
        {

            bool check = true;
            string senhaAdm = "";
            gservicosemail email2 = new gservicosemail();
            // string codemail = Properties.Settings.Default.CODSRVEMAIL;
            string codemail = Properties.Settings.Default.CODSRVEMAIL_LOCAL;
            gServicosEmailDto admEmail = email2.consultarEmail(codemail);
            MailMessage mail = new MailMessage();
            SmtpClient smtp = new SmtpClient();

            try
            {

                if (email2 != null)
                {

                    mail.From = new MailAddress(admEmail.smtpusuario);
                    smtp.Port = admEmail.smtpporta;
                    smtp.EnableSsl = false;
                    exist_countries paises = new exist_countries();
                    senhaAdm = PwdEncript.criptograph.Descriptografar(admEmail.smtpsenha);
                    smtp.DeliveryMethod = SmtpDeliveryMethod.Network; // [2] Added this
                    smtp.UseDefaultCredentials = false; // [3] Changed this
                    smtp.Credentials = new NetworkCredential(mail.From.ToString(), senhaAdm);  // [4] Added this. Note, first parameter is NOT string.
                    smtp.Host = admEmail.smtpservidor;
                    countries_for_region CFR = new countries_for_region();
                    region reg = new region();
                    exist_users userUtil = new exist_users();
                    Exist_UsersDto admin = new Exist_UsersDto();
                    exist_languages Existlang = new exist_languages();

                    string region = "";
                    string pais = "";
                    string lang = "";
                    admin = userUtil.consultarUsuario(email);


                    if (!expire_license)
                    {

                        mail.Subject = GeralResource.RecRes(2);
                    }
                    else
                    {

                        mail.Subject = GeralResource.RecRes(92);
                    }
                    pais = paises.PegarPais(userLogado.user_country).name;
                    lang = Existlang.LangViaId(admin.lang_id.lang_id).lang_mmc;
                    region = reg.PegarRegiaoViaId(CFR.PegarRegiaoViaPais(userLogado.user_country));
                    userUtil.consultarUsuario(email);

                    byte[] byteUserEmail = System.Text.Encoding.UTF8.GetBytes(userLogado.user_email);
                    String emailUsuarioCript = Convert.ToBase64String(byteUserEmail);

                    byte[] byteUserAdmin = System.Text.Encoding.UTF8.GetBytes(admin.user_email);
                    String emailAdminCript = Convert.ToBase64String(byteUserAdmin);

                    //Usando este metodo para emails da erro na descriptografia
                    //userLogado.user_email = PwdEncript.criptograph.Criptografar(userLogado.user_email);
                    //admin.user_email = PwdEncript.criptograph.Criptografar(admin.user_email);
                    mail.IsBodyHtml = true;
                    string urlApprove = "http://localhost:56075/Home/License?emailUser=" + emailUsuarioCript + "&aprovar=1&lang=" + lang + "&emailAdm=" + emailAdminCript;
                    string urlDeny = "http://localhost:56075/Home/License?emailUser=" + emailUsuarioCript + "&aprovar=0&lang=" + lang + "&emailAdm=" + emailAdminCript;
                    //string urlApprove = "https://www.advancepredictor.com/License/?ProcLicense=" + userLogado.user_email + ";1;" + lang + ";" + admin.user_email;
                    //string urlDeny = "https://www.advancepredictor.com/License?ProcLicense=" + userLogado.user_email + ";0;" + lang + ";" + admin.user_email;
                    // urlDeny = Shorten(urlDeny);
                    //urlApprove = Shorten(urlApprove);
                    string st = "<p class=\"MsoNormal\"> </p><div align=\"center\">";
                    st = st + "<table class=\"MsoNormalTable\" style=\"width: 100.0 % \" border=\"0\" width=\"100 % \" cellspacing=\"0\" cellpadding=\"0\">";
                    st = st + "<tbody>";
                    st = st + "<tr>";
                    st = st + "<td style=\"padding: 0cm 0cm 0cm 0cm\" valign=\"top\"><div align=\"center\">";
                    st = st + "<table class=\"MsoNormalTable\" style=\"width: 450.0pt; background: #F2F0EE\" border=\"0\" width=\"600\" cellspacing=\"0\" cellpadding=\"0\">";
                    st = st + "<tbody>";
                    st = st + "<tr>";
                    st = st + "<td style=\"padding: 0cm 0cm 0cm 0cm\" colspan=\"2\">";
                    st = st + "<p class=\"MsoNormal\">";
                    st = st + "<span style=\"font-family: 'Arial',sans-serif; border: solid windowtext 1.0pt; padding: 0cm\">";
                    st = st + "<img id=\"_x0000_i1025\" style=\"width: 1.0416in; height: 1.0416in\" src=\"./?_task=mail&amp;_action=get&amp;_mbox=INBOX&amp;_uid=5&amp;_token=M4uSfdxS6yRLYdHL97PkJ3L3T3515n47&amp;_part=2.2&amp;_embed=1&amp;_mimeclass=image\" alt=\"Imagem removida pelo remetente.\" width=\"100\" height=\"100\" />";
                    st = st + "</span>";
                    st = st + "<span style=\"font-family: 'Arial',sans-serif\"></span>";
                    st = st + "</p>";
                    st = st + "</td>";
                    st = st + "</tr>";
                    st = st + "<tr>";
                    st = st + "<td style=\"width: 213.0pt; padding: 0cm 0cm 0cm 0cm\" width=\"284\"><p class=\"MsoNormal\">";
                    st = st + "<span style=\"font-family: 'Arial',sans-serif; border: solid windowtext 1.0pt; padding: 0cm\">";
                    st = st + "<img id=\"_x0000_i1026\" style=\"width: 1.0416in; height: 1.0416in\" src=\"./?_task=mail&amp;_action=get&amp;_mbox=INBOX&amp;_uid=5&amp;_token=M4uSfdxS6yRLYdHL97PkJ3L3T3515n47&amp;_part=2.2&amp;_embed=1&amp;_mimeclass=image\" alt=\"Imagem removida pelo remetente.\" width=\"100\" height=\"100\" />";
                    st = st + "</span>";
                    st = st + "<span style=\"font-family: 'Arial',sans-serif\"></span>";
                    st = st + "</p>";
                    st = st + "</td>";
                    st = st + "<td style=\"padding: 3.75pt 3.75pt 3.75pt 3.75pt\"><table id=\"DataUser\" class=\"MsoNormalTable\" style=\"width: 225.0pt; border: solid #E975A2 1.0pt\" border=\"1\" width=\"300\" cellspacing=\"0\" cellpadding=\"0\">";
                    st = st + "<tbody>";
                    st = st + "<tr>";
                    st = st + "<td style=\"border: none; padding: 2.25pt 2.25pt 2.25pt 2.25pt\"> </td>";
                    st = st + "</tr>";
                    st = st + "<tr>";
                    st = st + "<td style=\"border: none; padding: 2.25pt 2.25pt 2.25pt 2.25pt\">";
                    st = st + "<p class=\"MsoNormal\"><strong><span style=\"font-family: 'Arial',sans-serif; color: windowtext\">Name: </span>";
                    st = st + "</strong>";
                    st = st + "<span style=\"font-family: 'Arial',sans-serif; color: windowtext\">" + userLogado.user_name + " </span>";
                    st = st + "</p>";
                    st = st + "</td>";
                    st = st + "</tr>";
                    st = st + "<tr>";
                    st = st + "<td style=\"border: none; padding: 2.25pt 2.25pt 2.25pt 2.25pt\"><p class=\"MsoNormal\">";
                    st = st + "<strong><span style=\"font-family: 'Arial',sans-serif; color: windowtext\">E-mail: </span></strong>";
                    st = st + "<span style=\"font-family: 'Arial',sans-serif; color: windowtext\">" + userLogado.user_email + " </span>";
                    st = st + "</p></td></tr><tr><td style=\"border: none; padding: 2.25pt 2.25pt 2.25pt 2.25pt\"><p class=\"MsoNormal\">";
                    st = st + "<strong><span style=\"font-family: 'Arial',sans-serif; color: windowtext\">Company: </span></strong>";
                    st = st + "<span style=\"font-family: 'Arial',sans-serif; color: windowtext\">" + userLogado.user_company + " </span></p></td></tr>";
                    st = st + "<tr><td style=\"border: none; padding: 2.25pt 2.25pt 2.25pt 2.25pt\"><p class=\"MsoNormal\"><strong>";
                    st = st + "<span style=\"font-family: 'Arial',sans-serif; color: windowtext\">Region: </span></strong>";
                    st = st + "<span style=\"font-family: 'Arial',sans-serif; color: windowtext\">" + region + " </span></p></td></tr>";
                    st = st + "<tr><td style=\"border: none; padding: 2.25pt 2.25pt 2.25pt 2.25pt\"><p class=\"MsoNormal\"><strong>";
                    st = st + "<span style=\"font-family: 'Arial',sans-serif; color: windowtext\">Country: </span></strong>";
                    st = st + "<span style=\"font-family: 'Arial',sans-serif; color: windowtext\">" + pais + "</span></p></td></tr><tr>";
                    st = st + "<td style=\"border: none; padding: 2.25pt 2.25pt 2.25pt 2.25pt\"><p class=\"MsoNormal\"><strong>";
                    st = st + "<span style=\"font-family: 'Arial',sans-serif; color: windowtext\">Phone: </span></strong>";
                    st = st + "<span style=\"font-family: 'Arial',sans-serif; color: windowtext\">" + userLogado.user_phone + " </span></p></td></tr>";
                    st = st + "<tr><td style=\"border: none; padding: 2.25pt 2.25pt 2.25pt 2.25pt\">";
                    st = st + "<table id=\"actions\" class=\"MsoNormalTable\" style=\"width: 100.0%\" border=\"0\" width=\"100%\" cellspacing=\"0\" cellpadding=\"0\">";
                    st = st + "<tbody><tr><td style=\"padding: 0cm 0cm 0cm 0cm\"><div align=\"center\">";
                    st = st + "<table id=\"License\" class=\"MsoNormalTable\" border=\"0\" cellspacing=\"0\" cellpadding=\"0\"><tbody><tr style=\"height: 18.75pt\">";
                    st = st + "<td style=\"width: 97.5pt; background: #E975A2; padding: 0cm 0cm 0cm 0cm; height: 18.75pt\" width=\"130\">";
                    st = st + "<p class=\"MsoNormal\" style=\"margin-left: 7.5pt\"><strong><span style=\"font-family: 'Arial',sans-serif; color: white\">";
                    st = st + "<a href=\" " + urlApprove + "\"><span style=\"font-size: 12.0pt; color: white; text-decoration: none\">Approve</span></a></span>";
                    st = st + "</strong></p></td></tr></tbody></table></div></td><td style=\"padding: 0cm 0cm 0cm 0cm\"><div align=\"center\">";
                    st = st + "<table id=\"NotLicense\" class=\"MsoNormalTable\" border=\"0\" cellspacing=\"0\" cellpadding=\"0\"><tbody><tr style=\"height: 18.75pt\">";
                    st = st + "<td style=\"width: 97.5pt; background: #E975A2; padding: 0cm 0cm 0cm 0cm; height: 18.75pt\" width=\"130\">";
                    st = st + "<p class=\"MsoNormal\" style=\"margin-left: 7.5pt\"><strong><span style=\"font-family: 'Arial',sans-serif; color: white\">";
                    st = st + "<a href=\"" + urlDeny + "\"><span style=\"font-size: 12.0pt; color: white; text-decoration: none\">Deny</span></a> </span></strong></p>";
                    st = st + "</tr></tbody></table></div></td></tr></tbody></table></td></tr></tbody></table></td></tr><tr>";
                    st = st + "<td style=\"padding: 0cm 0cm 0cm 0cm\" colspan=\"2\"><table id=\"actions2\" class=\"MsoNormalTable\" style=\"width: 100.0%\" border=\"0\" width=\"100%\" cellspacing=\"8\" cellpadding=\"0\">";
                    st = st + "<tbody><tr><td style=\"padding: 0cm 0cm 0cm 0cm\"><p class=\"MsoNormal\" style=\"margin-top: 7.5pt\"><span style=\"font-family: 'Arial',sans-serif; color: windowtext\">Case you didn't see the image buttons, copy the appropriated address line to you web browser: </span></p></td></tr><tr><td style=\"padding: 0cm 0cm 0cm 0cm\"><p class=\"MsoNormal\" style=\"margin-top: 7.5pt\"><strong><span style=\"font-family: 'Arial',sans-serif; color: windowtext\">Approve :</span></strong><span style=\"font-family: 'Arial',sans-serif; color: windowtext\">" + urlApprove + " </span></p></td></tr><tr><td style=\"padding: 0cm 0cm 0cm 0cm\"><p class=\"MsoNormal\" style=\"margin-top: 7.5pt\"><strong><span style=\"font-family: 'Arial',sans-serif; color: windowtext\">Deny :</span></strong><span style=\"font-family: 'Arial',sans-serif; color: windowtext\">" + urlDeny + " </span></p></td></tr></tbody></table></td></tr></tbody></table></div></td></tr></tbody></table></div><p class=\"MsoNormal\"> </p></div></div></div></div></div></div></div></div>";

                    mail.To.Add(new MailAddress(email));

                    mail.Body = st;
                    smtp.Send(mail);
                }
                else
                {

                    MSG_ERROR_SENDER = " _getReadServicesEmail";
                    check = false;
                }
            }
            catch (Exception ex)
            {

                MSG_ERROR_SENDER = ex.Message + " _getServicesEmail";
                check = false;
                opIntGr.WRITE_ERR_LOG(ex.Message + "_getServicesEmail");
                
            }
            return check;
        }
        public bool AdmEmailGetterAndSender(string email, string just)
        {

            bool check = true;
            string senhaAdm = "";
            gservicosemail email2 = new gservicosemail();
            string codemail = Properties.Settings.Default.CODSRVEMAIL;
            gServicosEmailDto admEmail = email2.consultarEmail(codemail);
            MailMessage mail = new MailMessage();
            SmtpClient smtp = new SmtpClient();

            exist_users userUtil = new exist_users();

            try
            {

                if (email2 != null)
                {

                    mail.From = new MailAddress(admEmail.smtpusuario);
                    smtp.Port = admEmail.smtpporta;
                    smtp.EnableSsl = true;
                    senhaAdm = PwdEncript.criptograph.Descriptografar(admEmail.smtpsenha);
                    exist_countries paises = new exist_countries();
                    Exist_UsersDto user = new Exist_UsersDto();
                    exist_users utilUser = new exist_users();
                    smtp.DeliveryMethod = SmtpDeliveryMethod.Network; // [2] Added this
                    smtp.UseDefaultCredentials = false; // [3] Changed this
                    smtp.Credentials = new NetworkCredential(mail.From.ToString(), senhaAdm);  // [4] Added this. Note, first parameter is NOT string.
                    smtp.Host = admEmail.smtpservidor;
                    countries_for_region CFR = new countries_for_region();
                    region reg = new region();
                    string region = "";
                    string pais = "";

                    user = utilUser.consultarUsuario(email);
                    pais = paises.PegarPais(user.user_country).name;
                    region = reg.PegarRegiaoViaId(CFR.PegarRegiaoViaPais(user.user_country));

                    mail.Subject = GeralResource.RecRes(2);
                    mail.IsBodyHtml = true;
                    mail.To.Add(new MailAddress(email));
                    mail.Body = just;
                    smtp.Send(mail);
                }
                else
                {

                    MSG_ERROR_SENDER = " _getReadServicesEmail";
                    check = false;
                }
            }
            catch (Exception ex)
            {

                MSG_ERROR_SENDER = ex.Message + " _getServicesEmail";
                check = false;
                opIntGr.WRITE_ERR_LOG(ex.Message + "_getServicesEmail");
            }

            return check;
        }
        public bool AdmEmailGetterAndSender(string email, Exist_UsersDto userLogado)
        {

            bool check = true;
            string senhaAdm = "";
            gservicosemail email2 = new gservicosemail();
            string codemail = Properties.Settings.Default.CODSRVEMAIL;
            gServicosEmailDto admEmail = email2.consultarEmail(codemail);
            MailMessage mail = new MailMessage();
            SmtpClient smtp = new SmtpClient();
            exist_users userUtil = new exist_users();
            try
            {

                if (email2 != null)
                {

                    mail.From = new MailAddress(admEmail.smtpusuario);
                    smtp.Port = admEmail.smtpporta;
                    smtp.EnableSsl = true;
                    senhaAdm = PwdEncript.criptograph.Descriptografar(admEmail.smtpsenha);
                    exist_countries paises = new exist_countries();
                    Exist_UsersDto user = new Exist_UsersDto();
                    exist_users utilUser = new exist_users();
                    smtp.DeliveryMethod = SmtpDeliveryMethod.Network; // [2] Added this
                    smtp.UseDefaultCredentials = false; // [3] Changed this
                    smtp.Credentials = new NetworkCredential(mail.From.ToString(), senhaAdm);  // [4] Added this. Note, first parameter is NOT string.
                    smtp.Host = admEmail.smtpservidor;
                    countries_for_region CFR = new countries_for_region();
                    region reg = new region();
                    string region = "";
                    string pais = "";

                    user = utilUser.consultarUsuario(email);
                    pais = paises.PegarPais(user.user_country).name;
                    region = reg.PegarRegiaoViaId(CFR.PegarRegiaoViaPais(user.user_country));

                    mail.Subject = GeralResource.RecRes(2);
                    mail.IsBodyHtml = true;
                    mail.To.Add(new MailAddress(email));
                    mail.Body = "";
                    smtp.Send(mail);
                }
                else
                {

                    MSG_ERROR_SENDER = " _getReadServicesEmail";
                    check = false;
                }
            }
            catch (Exception ex)
            {

                MSG_ERROR_SENDER = ex.Message + " _getServicesEmail";
                check = false;
                opIntGr.WRITE_ERR_LOG(ex.Message + "_getServicesEmail");
            }
            return check;
        }
        public bool JustifyEmail(string email, string justificativa)
        {

            bool check = true;
            string senhaAdm = "";
            gservicosemail email2 = new gservicosemail();
            string codemail = Properties.Settings.Default.CODSRVEMAIL_LOCAL;
            gServicosEmailDto admEmail = email2.consultarEmail(codemail);
            MailMessage mail = new MailMessage();
            Exist_UsersDto adm = new Exist_UsersDto();
            SmtpClient smtp = new SmtpClient();

            try
            {

                if (email2 != null)
                {

                    mail.From = new MailAddress(admEmail.smtpusuario);
                    smtp.Port = admEmail.smtpporta;
                    smtp.EnableSsl = false;
                    senhaAdm = PwdEncript.criptograph.Descriptografar(admEmail.smtpsenha);
                    exist_countries paises = new exist_countries();
                    smtp.DeliveryMethod = SmtpDeliveryMethod.Network; // [2] Added this
                    smtp.UseDefaultCredentials = false; // [3] Changed this
                    smtp.Credentials = new NetworkCredential(mail.From.ToString(), senhaAdm);  // [4] Added this. Note, first parameter is NOT string.
                    smtp.Host = admEmail.smtpservidor;
                    countries_for_region CFR = new countries_for_region();
                    region reg = new region();
                    exist_users userUtil = new exist_users();

                    adm = userUtil.consultarUsuario(email);

                    string region = "";
                    string pais = "";

                    bool isNew = true;

                    //   isNew = userUtil.IsNewUser(userLogado.user_id);
                    mail.Subject = "Reason for non approval";
                    pais = paises.PegarPais(adm.user_country).name;
                    region = reg.PegarRegiaoViaId(CFR.PegarRegiaoViaPais(adm.user_country));

                    mail.IsBodyHtml = true;
                    string st = justificativa;

                    mail.To.Add(new MailAddress(email));

                    mail.Body = st;
                    smtp.Send(mail);
                }
                else
                {

                    MSG_ERROR_SENDER = " _getReadServicesEmail";
                    check = false;
                }
                //}
            }
            catch (Exception ex)
            {

                MSG_ERROR_SENDER = ex.Message + " _getServicesEmail";
                check = false;
                opIntGr.WRITE_ERR_LOG(ex.Message + "_getServicesEmail");
            }
            return check;
        }
        public bool AdmEmailGetterAndSenderRecover(string email, Exist_UsersDto userLogado)
        {

            bool check = true;
            string senhaAdm = "";
            gservicosemail email2 = new gservicosemail();
            string codemail = Properties.Settings.Default.CODSRVEMAIL;
            gServicosEmailDto admEmail = email2.consultarEmail(codemail);
            MailMessage mail = new MailMessage();
            SmtpClient smtp = new SmtpClient();
            try
            {

                if (email2 != null)
                {

                    mail.From = new MailAddress(admEmail.smtpusuario);
                    smtp.Port = admEmail.smtpporta;
                    smtp.EnableSsl = true;
                    senhaAdm = PwdEncript.criptograph.Descriptografar(admEmail.smtpsenha);
                    exist_countries paises = new exist_countries();
                    exist_users util = new exist_users();
                    smtp.DeliveryMethod = SmtpDeliveryMethod.Network; // [2] Added this
                    smtp.UseDefaultCredentials = false; // [3] Changed this
                    smtp.Credentials = new NetworkCredential(mail.From.ToString(), senhaAdm);  // [4] Added this. Note, first parameter is NOT string.
                    smtp.Host = admEmail.smtpservidor;
                    string senhaUser = "";
                    string pais = "";

                    senhaUser = CreatePassword();
                    mail.Subject = GeralResource.RecRes(2);
                    pais = paises.PegarPais(userLogado.user_country).name;
                    mail.IsBodyHtml = true;
                    string st = " <p class=\"MsoNormal\"><!-- o ignored --> </p><div align=\"center\"><table class=\"MsoNormalTable\" border=\"0\" cellspacing=\"0\" cellpadding=\"0\" width=\"100 % \" style=\"width: 100.0 % \"><tr><td valign=\"top\" style=\"padding: 0cm 4.5pt 0cm 4.5pt\"><div align=\"center\"><table class=\"MsoNormalTable\" border=\"0\" cellspacing=\"0\" cellpadding=\"0\" width=\"600\" style=\"width: 450.0pt; background: #F2F0EE\"><tr><td style=\"padding: 0cm 0cm 0cm 0cm\"><p class=\"MsoNormal\"><span style=\"font-family: &quot;Arial&quot;,sans-serif; border: solid windowtext 1.0pt; padding: 0cm\"><img width=\"100\" height=\"100\" style=\"width: 1.0416in; height: 1.0416in\" id=\"_x0000_i1025\" src=\"./?_task=mail&amp;_action=get&amp;_mbox=INBOX&amp;_uid=4&amp;_token=2jQiCICvuGWyr5eqiKZ8wNQ7tADNyVdu&amp;_part=2&amp;_embed=1&amp;_mimeclass=image\" alt=\"Imagem removida pelo remetente.\" /></span><span style=\"font-family: &quot;Arial&quot;,sans-serif\"><!-- o ignored --></span></p></td></tr><tr><td style=\"padding: 0cm 0cm 0cm 0cm; margin-top: 15px!important\"><table class=\"MsoNormalTable\" border=\"0\" cellspacing=\"0\" cellpadding=\"0\" width=\"100%\" style=\"width: 100.0%\"><tr><td colspan=\"2\" style=\"padding: 0cm 11.25pt 0cm 11.25pt\"></td></tr><tr><td colspan=\"2\" style=\"padding: 0cm 11.25pt 0cm 11.25pt\"></td></tr><tr><td colspan=\"2\" style=\"padding: 0cm 11.25pt 0cm 11.25pt\"><p class=\"auto-style21\" style=\"margin-bottom: 11.25pt\"><span style=\"font-size: 13.0pt; font-family: &quot;Arial&quot;,sans-serif; color: #435058\">Sua Senha é:<!-- o ignored --></span></p></td></tr><tr><td colspan=\"2\" style=\"padding: 0cm 11.25pt 0cm 11.25pt\"></td></tr><tr><td width=\"284\" style=\"width: 213.0pt; padding: 0cm 11.25pt 0cm 11.25pt\"><p class=\"MsoNormal\"><span style=\"font-size: 13.0pt; font-family: &quot;Arial&quot;,sans-serif; color: windowtext; border: solid windowtext 1.0pt; padding: 0cm\"><img width=\"100\" height=\"100\" style=\"width: 1.0416in; height: 1.0416in\" id=\"_x0000_i1026\" src=\"./?_task=mail&amp;_action=get&amp;_mbox=INBOX&amp;_uid=4&amp;_token=2jQiCICvuGWyr5eqiKZ8wNQ7tADNyVdu&amp;_part=2&amp;_embed=1&amp;_mimeclass=image\" alt=\"\" /></span><span style=\"font-size: 13.0pt; font-family: &quot;Arial&quot;,sans-serif; color: windowtext\"><!-- o ignored --></span></p></td><td width=\"286\" style=\"width: 214.5pt; padding: 0cm 11.25pt 0cm 11.25pt\"><table class=\"MsoNormalTable\" border=\"0\" cellspacing=\"0\" cellpadding=\"0\" style=\"width: 286px!important; text-align: left!important\" id=\"DataUser\"><tr><td style=\"padding: 0cm 0cm 0cm 0cm\"></td></tr><tr><td style=\"padding: 0cm 0cm 0cm 0cm\"></td></tr><tr><td style=\"padding: 0cm 0cm 0cm 0cm\"><p class=\"MsoNormal\"><span style=\"font-size: 16.5pt; font-family: &quot;Arial&quot;,sans-serif; color: #E975A2; text-transform: uppercase\">Entrar <!-- o ignored --></span></p></td></tr><tr><td style=\"padding: 0cm 0cm 0cm 0cm\"><p class=\"MsoNormal\"><b><span style=\"font-size: 16.5pt; font-family: &quot;Arial&quot;,sans-serif; color: #435058\">" + userLogado.user_email + " </span></b><span style=\"font-family: &quot;Arial&quot;,sans-serif; color: #435058\"><!-- o ignored --></span></p></td></tr><tr><td style=\"padding: 0cm 0cm 0cm 0cm\"></td></tr><tr><td style=\"padding: 0cm 0cm 0cm 0cm\"><p class=\"MsoNormal\"><span style=\"font-size: 16.5pt; font-family: &quot;Arial&quot;,sans-serif; color: #E975A2; text-transform: uppercase\">Senha: </span></p></td></tr><tr><td style=\"padding: 0cm 0cm 0cm 0cm\"><p class=\"MsoNormal\"><b><span style=\"font-size: 16.5pt; font-family: &quot;Arial&quot;,sans-serif; color: #435058\">" + senhaUser + "</span></b><span style=\"font-family: &quot;Arial&quot;,sans-serif; color: #435058\"> <!-- o ignored --></span></p></td></tr><tr><td style=\"padding: 0cm 0cm 0cm 0cm\"></td></tr><tr><td style=\"padding: 0cm 0cm 0cm 0cm\"><div align=\"center\"><table class=\"MsoNormalTable\" border=\"0\" cellspacing=\"0\" cellpadding=\"0\"><tr style=\"height: 18.75pt\"><td width=\"180\" style=\"width: 135.0pt; background: #E975A2; padding: 0cm 0cm 0cm 0cm; height: 18.75pt\"><p class=\"MsoNormal\" align=\"center\" style=\"margin-left: 7.5pt; text-align: center\"><b><span style=\"font-family: &quot;Arial&quot;,sans-serif; color: white\"><a href=\"http://www.advancepredictor.com/\" target=\"_blank\" rel=\"noreferrer\"><span style=\"font-size: 12.0pt; color: white; text-decoration: none\">Ir para o site</span></a> <!-- o ignored --></span></b></p></td></tr></table></div></td></tr></table></td></tr><tr><td colspan=\"2\" style=\"padding: 0cm 11.25pt 0cm 11.25pt\"><table class=\"MsoNormalTable\" border=\"0\" cellspacing=\"8\" cellpadding=\"0\" width=\"100%\" style=\"width: 100.0%\" id=\"actions2\"><tr><td style=\"padding: 0cm 0cm 0cm 0cm\"></td></tr><tr><td style=\"padding: 0cm 0cm 0cm 0cm\"><p><span style=\"font-size: 10.0pt; font-family: &quot;Arial&quot;,sans-serif; color: #4B4747\">Obrigado por usar o nosso aplicativo.<br /><br />A equipe do Projeto Rovabio® Advance Predictor.<!-- o ignored --></span></p></td></tr><tr><td style=\"padding: 0cm 0cm 0cm 0cm\"></td></tr></table></td></tr></table></td></tr></table></div></td></tr></table></div><p class=\"MsoNormal\"><!-- o ignored --> </p></div></div></div></ div >";
                    userLogado.user_password = senhaUser;

                    util.Editar(userLogado);

                    mail.To.Add(new MailAddress(email));

                    mail.Body = st;
                    smtp.Send(mail);
                }
                else
                {

                    MSG_ERROR_SENDER = " _getReadServicesEmail";
                    check = false;
                }
            }
            catch (Exception ex)
            {

                MSG_ERROR_SENDER = ex.Message + " _getServicesEmail";
                check = false;
                opIntGr.WRITE_ERR_LOG(ex.Message + "_getServicesEmail");
                throw ex;
            }
            return check;
        }
        public ActionResult Logoff()
        {

            Session.Clear();
            Session.Abandon();

            return RedirectToAction("Login", "Home");
        }
        bool IsValidEmail(string email)
        {

            try
            {

                var addr = new MailAddress(email);
                return addr.Address == email;
            }
            catch
            {

                return false;
            }
        }
        public bool getAndSendEmail(string email)
        {

            bool check = true;
            string senhaAdm = "";
            gservicosemail email2 = new gservicosemail();
            exist_users user = new exist_users();
            string codemail = Properties.Settings.Default.CODSRVEMAIL;
            gServicosEmailDto admEmail = email2.consultarEmail(codemail);
            MailMessage mail = new MailMessage();
            SmtpClient smtp = new SmtpClient();
            string userSenha = "";
            try
            {

                if (email2 != null)
                {

                    mail.From = new MailAddress(admEmail.smtpusuario);
                    smtp.Port = admEmail.smtpporta;
                    smtp.EnableSsl = true;
                    senhaAdm = PwdEncript.criptograph.Descriptografar(admEmail.smtpsenha);
                    smtp.DeliveryMethod = SmtpDeliveryMethod.Network; // [2] Added this
                    smtp.UseDefaultCredentials = false; // [3] Changed this
                    smtp.Credentials = new NetworkCredential(mail.From.ToString(), senhaAdm);  // [4] Added this. Note, first parameter is NOT string.
                    smtp.Host = admEmail.smtpservidor;

                    //recipient address
                    mail.To.Add(new MailAddress(email));

                    //Formatted mail body
                    mail.Subject = GeralResource.RecRes(9);
                    mail.IsBodyHtml = true;
                    string st = GeralResource.RecRes(49);
                    userSenha = user.pegarSenhaViaEmail(email);
                    userSenha = PwdEncript.criptograph.Descriptografar(userSenha);
                    mail.Body = st + userSenha;
                    smtp.Send(mail);
                }
                else
                {

                    MSG_ERROR_SENDER = " _getReadServicesEmail";
                    check = false;
                }
            }
            catch (Exception ex)
            {

                MSG_ERROR_SENDER = ex.Message + " _getServicesEmail";
                check = false;
                opIntGr.WRITE_ERR_LOG(ex.Message + "_getServicesEmail");
            }
            return check;
        }
        public ActionResult editarUsuario(Exist_UsersDto Models)
        {
            try
            {

                exist_users user = new exist_users();
                string cSenha = Request.Form["user_password_confirm"];

                RemoveReferences(ModelState, "user_country");


                //    return Json(new { status = "validation", view = contentView });
                //}
                if (Models.user_password != Models.user_password_confirm)
                {

                    ModelState.AddModelError("user_password", GeralResource.RecRes(120));
                    paises();
                    tratamentos();
                    string contentView = RenderViewToString("~/Views/Home/editarForm.cshtml", Models);
                    return Json(new { status = "validation", view = contentView });
                }
                if (CalculateEntropy(Models.user_password) < 2)
                {

                    ModelState.AddModelError("user_password", GeralResource.RecRes(626));
                    paises();
                    tratamentos();
                    string contentView = RenderViewToString("~/Views/Home/editarForm.cshtml", Models);
                    return Json(new { status = "validation", view = contentView });
                }
                if (!ModelState.IsValid)
                {

                    paises();
                    tratamentos();
                    string contentView = RenderViewToString("~/Views/Home/editarForm.cshtml", Models);
                    return Json(new { status = "validation", view = contentView });
                }
                var retorno = user.Editar(Models);

                if (retorno == "0")
                {

                    return Json(new { status = "validation", message = GeralResource.RecRes(592) });
                }

                return Json(new { status = "success", message = GeralResource.RecRes(448) });
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
    }
}

