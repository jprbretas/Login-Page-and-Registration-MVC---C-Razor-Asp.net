using domit.recresource;
using domit.wdbconn;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Web.Mvc;
using padvMVC.Models;
using padvMVC.Negocio;
using System.Linq;
using System.Data;


namespace padvMVC.Controllers
{
    public class HomeController : GeralController
    {
        // GET: Home

        public ActionResult License(string emailUser, string aprovar, string lang, string emailAdm)
        {

            LoginController utilidade = new LoginController();
            exist_users user = new exist_users();
            Exist_UsersDto Models = new Exist_UsersDto();
            Exist_UsersDto adm = new Exist_UsersDto();
            historic_action_users just = new historic_action_users();

            
            byte[] byteEmailUser = System.Convert.FromBase64String(emailUser.Replace(" ",""));
            emailUser = System.Text.Encoding.UTF8.GetString(byteEmailUser);

            byte[] byteEmailAdmin = System.Convert.FromBase64String(emailAdm.Replace(" ", ""));
            emailAdm = System.Text.Encoding.UTF8.GetString(byteEmailAdmin);

            //  emailUser = PwdEncript.criptograph.Descriptografar(emailUser);
            // emailAdm = PwdEncript.criptograph.Descriptografar(emailAdm);

            Models = user.consultarUsuario(emailUser);
            adm = user.consultarUsuario(emailAdm);
            string ip = GetLocalIpAddress();

            bool userIsnew = true;
            bool approve = false;

            if (adm == null)
            {

                return Content("<label>" + @GeralResource.RecRes(580) + "</label>");
            }
            if (adm.user_accesslevel < 1)
            {

                return Content("<label>" + @GeralResource.RecRes(580) + "</label>");
            }

            if (aprovar == "1")
            {

                approve = true;
            }
            userIsnew = user.IsNewUser(Models.user_id);

            if (!approve)
            {

                ViewBag.mensagem = "";
                ViewBag.user = emailUser;
                ViewBag.data = DateTime.Now;
                ViewBag.email = emailAdm;
                ViewBag.approve = 0;
                user.SetAction2(Models);
            }
            else
            {

                if (Models.user_date_expire != null)
                {

                    if (Models.user_date_expire > DateTime.Now)
                    {

                        ViewBag.mensagem = @GeralResource.RecRes(138);
                        ViewBag.approve = 1;
                    }
                }
                else
                {

                    ViewBag.mensagem = @GeralResource.RecRes(138);
                    ViewBag.approve = 1;
                    user.license(Models.user_id);
                    just.InserirJustificativa(emailAdm, Models.user_id, ip);
                }
            }
            return View();
        }
        
        public ActionResult Justificar(string user_email, string Justificativa, string email_adm)
        {

            try
            {

            exist_users user = new exist_users();
            LoginController control = new LoginController();
            List<string> admsEmail = new List<string>();
            List<string> admsEmailMaster = new List<string>();
            countries_for_region regXpais = new countries_for_region();
            historic_action_users just = new historic_action_users();
            long userCountry;
            long userReg = 0;
            bool flag = true;
            Exist_UsersDto Models = new Exist_UsersDto();
            string ip = GetLocalIpAddress();

            Models = user.consultarUsuario(user_email);
            userCountry = Convert.ToInt64( Models.user_country);
            userReg = regXpais.PegarRegiaoViaPais(userCountry);
            admsEmail = user.pegarOsAdms(userReg);
            admsEmailMaster = user.pegarOsMasters();
            string aviso = @GeralResource.RecRes(634);

            just.InserirJustificativa(Justificativa, email_adm, Models.user_id, ip);

            foreach (var email in admsEmail)
            {

                control.JustifyEmail(email, aviso);
            }
            foreach (var email in admsEmailMaster)
            {

                control.JustifyEmail(email, aviso);
            }

            control.JustifyEmail(user_email, aviso);
            ViewBag.mensagem = @GeralResource.RecRes(582);

            user.BlockUser(Models.user_id);

            return Json(new { status = "success", message = @GeralResource.RecRes(582), JsonRequestBehavior.AllowGet });
        }

            catch (Exception ex)
            {

                throw ex;
            }
        }

        public ActionResult AlterarCurrency(string currency)
        {

            try
            {

                var usuario = getUser();
                long currency_id = Convert.ToInt64(currency);
                if (usuario.currency_id != currency_id)
                {

                    exist_users exist_users = new exist_users();
                    string retorno = exist_users.AlterarMoeda(currency_id, usuario.user_id);
                    usuario.currency_id = currency_id;
                    Session["usuario"] = usuario;
                }

                return Json(new { status = "success" });
            }
            catch (Exception ex)
            {

                return Json(new { status = "error" });
            }

        }

        [RedirectingAction]
        [RefreshAttribute]
        public ActionResult Feed_Ingredients()
        {

            //Exist_UsersDto usuario;
            //usuario = getUser();
            
            paises();
            tratamentos();
            ViewBag.usuario = Session["usuario"];

            return View();
        }
        public ActionResult LoadGridFeedIngreds(string mode)
        {

            ingred_matrix ingred_matrix = new ingred_matrix();
            exist_base_tables exist_base_tables = new exist_base_tables();

            try
            {
                var provider = GetCurrentProvider(this);
                ViewBag.provider = provider;
                //DataTable dtMtx = MountForVisualizationGrid(provider);

                var usuario = (Exist_UsersDto)Session["usuario"];
                DataTable dt = new DataTable();
                DataTable dtTables = new DataTable();
                DataTable dtMatrix = new DataTable();
                exist_nutrients nutridb = new exist_nutrients();
                Local_Ingred_User util = new Local_Ingred_User();

                DataTable dtMatrix_Data = ingred_matrix.GetMatrixDadosBase(usuario);
                DataTable dt_linhas = ingred_matrix.ConsultarIngredientesBaseELocal(usuario.user_id, usuario.lang_id.lang_id);
                DataTable dtNutrientes = dtMatrix_Data.DefaultView.ToTable(true, "nutr_id", "nutr_name", "unit");


                /*dt_linhas.AcceptChanges();
                foreach (DataRow row in dt_linhas.Rows)
                {
                    var local = Convert.ToInt32(row["local"]);
                    long ing_id = Convert.ToInt64(row["ingred_id"]);
                    bool flag = true;

                    if (local == 1)
                    {

                        flag = util.VerificarIngUser(usuario.user_id, ing_id);

                        if (!flag)
                        {

                            row.Delete();
                        }
                    }
                }
                dt_linhas.AcceptChanges(); */

                var tabelas = exist_base_tables.Consultar();


                // criar o for impedidor

                TempData["dt_nutrients"] = dt;
                if (dtNutrientes != null)
                {

                    dtMatrix.Columns.Add("ingred_id", typeof(String));
                    dtMatrix.Columns.Add("table_id", typeof(String));
                    dtMatrix.Columns.Add("ingred", typeof(String));
                    dtMatrix.Columns.Add("local", typeof(String));
                    foreach (DataRow row in dtNutrientes.Rows)
                    {

                       
                        DataColumn dc = new DataColumn();
                        dc.ColumnName = row["nutr_name"].ToString().Trim() + " " + row["unit"].ToString().Trim();
                        dc.DataType = typeof(String);


                        dtMatrix.Columns.Add(dc);
                    }

                    dt.Dispose();
                }


                DataTable dtMatrix_Data_Local = ingred_matrix.GetMatrixDadosLocal(usuario.user_id);

                DataTable dtMatrix_Other = ingred_matrix.GetMatrixDataOutros(usuario);

                if (dt_linhas != null)
                {

                    foreach (DataRow row in dt_linhas.Rows)
                    {

                        var i_id = row[0];
                        DataRow trow_ingred = null;
                        trow_ingred = dtMatrix.NewRow();
                        trow_ingred["ingred_id"] = row[0].ToString();
                        trow_ingred["table_id"] = "0";
                        trow_ingred["ingred"] = row[1].ToString();

                        string local = row["local"].ToString();
                        trow_ingred["local"] = local;
                        if (local == "0")
                        {
                            dtMatrix.Rows.Add(trow_ingred);
                        }


                        try
                        {

                            foreach (Exist_Base_TablesDto tb in tabelas)
                            {

                                DataRow trow_values_pdr = null;
                                trow_values_pdr = dtMatrix.NewRow();
                                DataRow trow_values_alt = null;
                                trow_values_alt = dtMatrix.NewRow();

                                long tabela_id = Convert.ToInt64(tb.table_id);
                                trow_values_pdr["ingred_id"] = row[0].ToString();
                                trow_values_pdr["table_id"] = tabela_id.ToString();
                                trow_values_alt["table_id"] = "0";
                                trow_values_pdr["ingred"] = tb.table_description;
                                trow_values_pdr["local"] = row["local"].ToString();

                                DataView DVFilter = null;
                                DataView DvFilterNutr = new DataView(dtNutrientes);


                                if (local == "1") // ingrediente local
                                {

                                    DVFilter = new DataView(dtMatrix_Data_Local);

                                    if (dtMatrix_Data_Local.Rows.Count > 0)
                                    {

                                        DVFilter.RowFilter = "ingred_id=" + i_id + " and table_id=" + tabela_id + "";
                                    }
                                    else
                                    {

                                        DVFilter.RowFilter = "ingred_id=0";
                                    }
                                }
                                else
                                {

                                    DVFilter = new DataView(dtMatrix_Other);
                                    DVFilter.RowFilter = "ingred_id=" + i_id + " and table_id=" + tabela_id;

                                }

                                var alt_nutr = false;

                                if (DVFilter != null)
                                {

                                    foreach (DataRow item in DVFilter.ToTable().Rows)
                                    {

                                        DvFilterNutr.RowFilter = "nutr_id=" + item["nutr_id"];
                                        DataView filt_Nut = new DataView(DVFilter.ToTable());
                                        filt_Nut.RowFilter = "nutr_id=" + item["nutr_id"];

                                        var nut = DvFilterNutr.ToTable().Rows[0]["nutr_name"].ToString().Trim() + " " + DvFilterNutr.ToTable().Rows[0]["unit"].ToString().Trim();
                                        decimal val = Convert.ToDecimal(filt_Nut.ToTable().Rows[0]["nutr_value"]);

                                        if (local == "1")
                                        {

                                            trow_ingred[nut] = val.ToString("N3", provider);
                                        }
                                        else
                                        {

                                            trow_values_pdr[nut] = val.ToString("N3", provider);
                                        }

                                        if (filt_Nut.ToTable().Rows[0]["value_local"] != DBNull.Value)
                                        {

                                            val = Convert.ToDecimal(filt_Nut.ToTable().Rows[0]["value_local"]);
                                            trow_values_alt[nut] = val.ToString("N3", provider);
                                            trow_values_alt["local"] = "0";

                                            alt_nutr = true;
                                        }

                                    }

                                    if (local == "1")
                                    {
                                        dtMatrix.Rows.Add(trow_ingred);
                                        break;
                                    }
                                    else
                                    {
                                        dtMatrix.Rows.Add(trow_values_pdr);
                                    }


                                    if (alt_nutr)
                                    {

                                        dtMatrix.Rows.Add(trow_values_alt);
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {

                            Log.LogError.WriteLog(ex.Message, "_MountForVisualizationGrid");
                            throw ex;
                        }


                    }
                }


                GetColluns(dtMatrix);
                var Model = GetModelTable(dtMatrix);
                ViewBag.mode = mode;
                return PartialView(Model);
            }
            catch (Exception ex)
            {

                Log.LogError.WriteLog(ex.Message, "_");
                return Content("<label> Error: Please contact admin! </label>");
            }
        }
        public ActionResult ExcluirIngredienteLocal(string id_ing = "0")
        {

            try
            {

                long ing_id = 0;
                if (id_ing.Trim() != "")
                {

                    ing_id = Convert.ToInt64(id_ing);
                }
                if (ing_id != 0)
                {

                    local_ingred_tables local_ingred_tables = new local_ingred_tables();
                    exist_ingredients exist_ingredients = new exist_ingredients();
                    Local_Ingred_User Local_Ingred_User = new Local_Ingred_User();

                    local_ingred_tables.GetConnection().CriaTransaction();
                    exist_ingredients.SetConnection(local_ingred_tables.GetConnection());
                    Local_Ingred_User.SetConnection(local_ingred_tables.GetConnection());

                    Local_Ingred_User.Excluir(ing_id);
                    local_ingred_tables.ExcluirPorIngrediente(ing_id);
                    exist_ingredients.Excluir(ing_id);
                    // excluir do vinculo de tabelas

                    local_ingred_tables.GetConnection().TransCommit();
                }
                return Json(new { status = "success" });
            }
            catch (Exception ex)
            {

                return Json(new { status = "error" });
            }
        }
        public ActionResult SalvaIngredienteLocal(string name_ingrediente,long ingred_id, string model)
        {

            try
            {
                // incluir no cadastro de ingrediente
                // salvar valores para todas as tabelas 

                string[] arrValues = new string[0];
                arrValues = model.Split(',');


                var usuario = getUser().user_id;
                var lang = GetLangId(this, Session["lang_code"].ToString());

                exist_base_tables exist_base_tables = new exist_base_tables();
                exist_ingredients exist_ingredients = new exist_ingredients();
                local_ingred_tables local_ingred_tables = new local_ingred_tables();
                Local_Ingred_User Local_Ingred_User = new Local_Ingred_User();
                var tabelas = exist_base_tables.Consultar(usuario, lang);

                exist_ingredients.GetConnection().CriaTransaction();

                local_ingred_tables.SetConnection(exist_ingredients.GetConnection());
                Local_Ingred_User.SetConnection(exist_ingredients.GetConnection());

                var retorno = exist_ingredients.Incluir(name_ingrediente);
                long ingrediente = Convert.ToInt64(retorno);
                DataTable dt_nutrs = (DataTable)TempData["dt_nutrients"];
                TempData.Keep("dt_nutrients");

                Local_Ingred_AlterUserDto local_ingred = new Local_Ingred_AlterUserDto();
                local_ingred.user_id = usuario;
                local_ingred.ingred_id = ingrediente;

                Local_Ingred_User.Incluir(ingrediente, usuario);

                foreach (Exist_Base_TablesDto tabela in tabelas)
                {

                    local_ingred.table_id = tabela.table_id;

                    var index = -1;
                    for (int i = 0; i < arrValues.Length - 1; i++)
                    {

                        var nutr_id = Convert.ToInt64(dt_nutrs.Rows[i]["nutr_id"]);
                        local_ingred.nutr_id = nutr_id;
                        local_ingred.nutr_value = Convert.ToDecimal(arrValues[i]);
                        local_ingred_tables.Inserir(local_ingred);
                    }
                }
                exist_ingredients.GetConnection().TransCommit();
                return Json(new { status = "success" });
            }
            catch (Exception ex)
            {

                return Json(new { status = "error" });
            }
        }
        private void GetColluns(DataTable dtMtx)
        {

            List<string> list = new List<string>();
            foreach (DataColumn item in dtMtx.Columns)
            {

                list.Add(item.ColumnName);
            }

            ViewBag.colunas = list;
        }
        /*private DataTable MountForVisualizationGrid(IFormatProvider provider)
        {

            try
            {

                //var usuario = getUser();
                var usuario = (Exist_UsersDto)Session["usuario"];
                DataTable dt = new DataTable();
                DataTable dt_linhas = new DataTable();
                DataTable dtTables = new DataTable();
                DataTable dtMatrix = new DataTable();
                exist_ingredients utilIngred = new exist_ingredients();
                Local_Ingred_User util = new Local_Ingred_User();

                ingred_matrix ingred_matrix = new ingred_matrix();
                exist_base_tables exist_base_tables = new exist_base_tables();
        
                DataTable dtMatrix_Data = ingred_matrix.GetMatrixDadosBase(usuario);

                DataTable dtMatrix_Data_Local = ingred_matrix.GetMatrixDadosLocal(usuario.user_id); 

                DataTable dtMatrix_Other = ingred_matrix.GetMatrixDataOutros(usuario);

                dt_linhas = ingred_matrix.ConsultarIngredientesBaseELocal(usuario.user_id, usuario.lang_id.lang_id);
                DataTable dtNutrientes = dtMatrix_Data.DefaultView.ToTable(true, "nutr_id", "nutr_name", "unit");
                
                dt_linhas.AcceptChanges();
                foreach (DataRow row in dt_linhas.Rows)
                {

                    var local = Convert.ToInt32( row["local"]);
                    long ing_id = Convert.ToInt64(row["ingred_id"]);
                    bool flag = true;

                    if(local ==1)
                    {

                     flag =   util.VerificarIngUser(usuario.user_id, ing_id);

                        if(!flag)
                        {

                            row.Delete();
                        }
                    }           
                }
                dt_linhas.AcceptChanges();

                var tabelas = exist_base_tables.Consultar();

                TempData["dt_nutrients"] = dt;
                if (dtNutrientes != null)
                {

                    dtMatrix.Columns.Add("ingred_id", typeof(String));
                    dtMatrix.Columns.Add("table_id", typeof(String));
                    dtMatrix.Columns.Add("ingred", typeof(String));
                    dtMatrix.Columns.Add("local", typeof(String));
                    foreach (DataRow row in dtNutrientes.Rows)
                    {
                        
                        if (row["nutr_name"].ToString() == "REM")
                        {

                            row["nutr_name"] = utilIngred.ConsultarIngrediente(Convert.ToInt64(row["nutr_id"])).ingred_mmc;
                            
                         }

                        DataColumn dc = new DataColumn();
                        dc.ColumnName = row["nutr_name"].ToString().Trim() + " " + row["unit"].ToString().Trim();
                        dc.DataType = typeof(String);
                       
                        dtMatrix.Columns.Add(dc);
                    }

                    dt.Dispose();
                }

                if (dt_linhas != null)
                {

                    foreach (DataRow row in dt_linhas.Rows)
                    {

                        var i_id = row[0];
                        DataRow trow_ingred = null;
                        trow_ingred = dtMatrix.NewRow();
                        trow_ingred["ingred_id"] = row[0].ToString();
                        trow_ingred["table_id"] = "0";
                        trow_ingred["ingred"] = row[1].ToString();

                        string local = row["local"].ToString();
                        trow_ingred["local"] = local;
                        if (local == "0")
                        {

                            dtMatrix.Rows.Add(trow_ingred);
                        } 
                        try
                        {

                            foreach (Exist_Base_TablesDto tb in tabelas)
                            {

                                DataRow trow_values_pdr = null;
                                trow_values_pdr = dtMatrix.NewRow();
                                DataRow trow_values_alt = null;
                                trow_values_alt = dtMatrix.NewRow();

                                long tabela_id = Convert.ToInt64(tb.table_id);
                                trow_values_pdr["ingred_id"] = row[0].ToString();
                                trow_values_pdr["table_id"] = tabela_id.ToString();
                                trow_values_alt["table_id"] = "0";
                                trow_values_pdr["ingred"] = tb.table_description;
                                trow_values_pdr["local"] = row["local"].ToString();

                                DataView DVFilter = null;
                                DataView DvFilterNutr = new DataView(dtNutrientes);


                                if (local == "1") // ingrediente local
                                {

                                    DVFilter = new DataView(dtMatrix_Data_Local);

                                    if (dtMatrix_Data_Local.Rows.Count > 0)
                                    {

                                        DVFilter.RowFilter = "ingred_id=" + i_id + " and table_id=" + tabela_id + "";
                                    }
                                    else
                                    {

                                        DVFilter.RowFilter = "ingred_id=0";
                                    }
                                }
                                else
                                {

                                    DVFilter = new DataView(dtMatrix_Other);
                                    DVFilter.RowFilter = "ingred_id=" + i_id + " and table_id=" + tabela_id;

                                }
                                var alt_nutr = false;

                                if (DVFilter != null)
                                {

                                    foreach (DataRow item in DVFilter.ToTable().Rows)
                                    {

                                        DvFilterNutr.RowFilter = "nutr_id=" + item["nutr_id"];
                                        DataView filt_Nut = new DataView(DVFilter.ToTable());
                                        filt_Nut.RowFilter = "nutr_id=" + item["nutr_id"];

                                        var nut = DvFilterNutr.ToTable().Rows[0]["nutr_name"].ToString().Trim() + " " + DvFilterNutr.ToTable().Rows[0]["unit"].ToString().Trim();
                                        decimal val = Convert.ToDecimal(filt_Nut.ToTable().Rows[0]["nutr_value"]);
                                       
                                        if (local == "1")
                                        {

                                            trow_ingred[nut] = val.ToString("N3", provider);
                                        }
                                        else
                                        {

                                            trow_values_pdr[nut] = val.ToString("N3", provider);
                                        }
                                        if (filt_Nut.ToTable().Rows[0]["value_local"] != DBNull.Value)
                                        {

                                            val = Convert.ToDecimal(filt_Nut.ToTable().Rows[0]["value_local"]);
                                            trow_values_alt[nut] = val.ToString("N3", provider);
                                            trow_values_alt["local"] = "0";

                                            alt_nutr = true;
                                        }
                                    }
                                    if (local == "1")
                                    {

                                        dtMatrix.Rows.Add(trow_ingred);
                                        break;
                                    }
                                    else
                                    {

                                        dtMatrix.Rows.Add(trow_values_pdr);
                                    }
                                    if (alt_nutr)
                                    {

                                        dtMatrix.Rows.Add(trow_values_alt);
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {

                            Log.LogError.WriteLog(ex.Message, "_MountForVisualizationGrid");
                            throw ex;
                        }
                    }
                }
                return dtMatrix;
            }
            catch (Exception ex)
            {

                Log.LogError.WriteLog(ex.Message, "_MountForVisualizationGrid2");
                throw ex;
            }
        } */

        [RedirectingAction]
        [RefreshAttribute]
        public ActionResult Formulation()
        {

            try
            {

               // var usuario = getUser();
                //var user_id = usuario.user_id;
                //var lang_id = usuario.lang_id;
                //var table_id = usuario.table_id;
                //var langcode = usuario.lang_id.lang_mmc;

                //var langcode = GetLangCode(this);
                ViewBag.langcode = Session["lang_code"];

                Exist_UsersDto usuario = (Exist_UsersDto)Session["usuario"];


                exist_base_tables basetb = new exist_base_tables();
                List<Exist_Base_TablesDto> tabelas = basetb.Consultar(usuario.user_id, usuario.lang_id.lang_id);
                ViewBag.tabelas = tabelas;
                var tabela = tabelas.Where(f => f.table_id == usuario.table_id).ToList()[0];
                ViewBag.tabela_ativa_id = usuario.table_id;
                ViewBag.tabela_ativa_nome = tabela.table_description;

                Measure_Systems MeasSystem = new Measure_Systems();
                Measure_SystemsDtoCollection measure_systems = MeasSystem.Consultar();
                ViewBag.units = measure_systems;

                List<SelectListItem> BaseCalcs = new List<SelectListItem>();
                BaseCalcs.Add(new SelectListItem() { Text = GeralResource.RecRes(242), Value = "0" });
                BaseCalcs.Add(new SelectListItem() { Text = GeralResource.RecRes(243), Value = "1" });

                ViewBag.BaseCalcs = BaseCalcs;

                List<SelectListItem> EnUnits = new List<SelectListItem>();
                EnUnits.Add(new SelectListItem() { Text = GeralResource.RecRes(1112), Value = "1" });
                EnUnits.Add(new SelectListItem() { Text = GeralResource.RecRes(1113), Value = "2" });

                ViewBag.EnUnits = EnUnits;
                ViewBag.Dose = usuario.dose;
                GetConfigEnergia(usuario);

                paises();
                tratamentos();
                ViewBag.usuario = usuario;
                ViewBag.specie = usuario.specie_id;
                GetSelectionSubProducts();
            }
            catch (Exception ex)
            {

                ViewBag.ErrorPage = ex.Message;
            }
            return View();
        }
        public void GetSelectionSubProducts()
        {

            long user_id = Convert.ToInt64(Session["user_id"]);

            exist_Products exist_Products = new exist_Products();
            DataTable dtprd = exist_Products.ConsultarProdutosSelecionadosPorUsuario(user_id);

            List<Products_SubtypeDto> listSubProds = new List<Products_SubtypeDto>();

            foreach (DataRow item in dtprd.Rows)
            {

                Products_SubtypeDto subProdDto = new Products_SubtypeDto();
                subProdDto.product_id = Convert.ToInt64(item["product_id"]);
                subProdDto.product_subtype = item["product_subtype"].ToString();
                listSubProds.Add(subProdDto);
            }
            ViewBag.SelectionSubProdutos = listSubProds;
        }
        public void GetConfigEnergia(Exist_UsersDto usuario)
        {

            enunit_config enunit_config = new enunit_config();
            long en_measure_id = 0;
            en_measure_id = enunit_config.ConsultarSistemaEnergia(usuario.system_id.Value, usuario.measure_config_id);
          
            ViewBag.en_measure_id = en_measure_id;
        }
        public void DashboardAverages()
        {

            try
            {

                Saved_CalculationsDtoCollection uplift = new Saved_CalculationsDtoCollection();
                Calculation calculations = new Calculation();
                saved_calculations utilCalc = new saved_calculations();
                UpliftsCalcs uplifts = new UpliftsCalcs();
                FormulationsDtoCollection formulas = new FormulationsDtoCollection();
                formulations formUtil = new formulations();

                var usuario = getUser();
                var species = usuario.specie_id;
                var usuario_id = usuario.user_id;
                var provider = GetCurrentProvider(this);
                uplifts = calculations.DashboardAVGS(usuario_id, species, "", provider);
                uplift = utilCalc.GetCaculations(usuario_id, species);
                formulas = formUtil.FormulasConsulta(usuario_id, species);

                ViewBag.avg_ame = uplifts.AME;
                ViewBag.qtdCalculations = uplift.Count;
                ViewBag.avg_posp = uplifts.Phosphorus;
                ViewBag.avg_calcio = uplifts.Calcium;
                ViewBag.avg_daa = uplifts.DAA;

                ViewBag.avg_custo = calculations.SavingsMedio(formulas);
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
        public void GetMonth()
        {

            string getCulture = "";
            string culture = Convert.ToString(Session["lang_code"]);

            switch (culture)
            {

                case "es":
                    {

                        culture = "es";
                        getCulture = "es-ES";
                        break;
                    }
                case "en":
                    {

                        culture = "en";
                        getCulture = "en-US";
                        break;
                    }
                case "pt":
                    {

                        culture = "pt";
                        getCulture = "pt-BR";
                        break;
                    }
                case "fr":
                    {

                        culture = "fr";
                        getCulture = "fr-FR";
                        break;
                    }
            }
            var cultures = new CultureInfo(getCulture);
            var Month = cultures.DateTimeFormat.GetMonthName(DateTime.Today.Month);

            ViewBag.Month = Month.ToString();
        }

        [RedirectingAction]
        [RefreshAttribute]
        public ActionResult Index()
        {

            try
            {

                Exist_UsersDto usuario;
                usuario = getUser();
                GetDayOfWeek();
                GetDay();
                GetMonth();
                DashboardAverages();

                paises();
                tratamentos();

                ViewBag.usuario = usuario;
                ViewBag.langcode = Session["lang_code"]; //GetLangCode(this);
            }
            catch (Exception ex)
            {

                ViewBag.ErroPage = ex.Message;
            }
            
            return View();
        }
        public ActionResult RecentFeeds(string value)
        {

            try
            {

                var provider = GetCurrentProvider(this);
                List<Saved_CalculationsDto> listFormulasCalculadas = new List<Saved_CalculationsDto>();
                List<FormulationsDto> listaDeFormulas = new List<FormulationsDto>();
                List<Saved_CalculationsDto> list_produtos = new List<Saved_CalculationsDto>();
                saved_calculations calcUtilSaved = new saved_calculations();
                formulations formulaNumeral = new formulations();
                Calculation calcUtil = new Calculation();
                products utilProd = new products();
                saved_calculations util2 = new saved_calculations();
                var species = Convert.ToInt64(Session["specie_id"]);

                //var usuario = getUser();

                var usuario = (Exist_UsersDto)Session["usuario"];

                listaDeFormulas = formulaNumeral.GetLastest(usuario.user_id, species);
                foreach (var item in listaDeFormulas)
                {

                    listFormulasCalculadas = listFormulasCalculadas.Concat(calcUtilSaved.ConsultarFormula(item.formula_id)).ToList();
                }

                var upliftList = new List<UpliftsCalcs>();

                var formulas = listFormulasCalculadas.GroupBy(p => p.formula_id.formula_id).Select(g => g.First()).ToList();
                var divisor = formulas.Count / 3;
                string content = "";
                var pular = 0;

                foreach (var item in formulas)
                {

                    var produtos = listFormulasCalculadas.Where(f => f.product_level_name == item.product_level_name).ToList();
                    produtos = produtos.GroupBy(p => p.product_level_name).Select(g => g.First()).ToList();

                    foreach (var prod in produtos)
                    {

                        list_produtos.Add(new Saved_CalculationsDto() { product_level_name = prod.product_level_name, formula_id = item.formula_id });
                        upliftList.Add(calcUtil.CalculateUplifts(item.formula_id.formula_id, prod.product_level_name, provider));
                    }
                }
                ViewBag.UpliftsCalcs = upliftList;

                ViewBag.produtos = list_produtos;
                var listFormulas = formulas;

                if (divisor == 0)
                {

                    content = "<div class='carousel-item active' >";
                    content = content + "<div class='row row-small-gutter'>";
                    ViewBag.formulas = listFormulas;
                    content = content + RenderViewToString("RecentFeeds", new FormulationsDtoCollection());
                    content = content + "</div> </div>";
                }
                else
                {

                    for (int i = 0; i < divisor; i++)
                    {

                        string check_index = i == 0 ? "active" : "";
                        content = "<div class='carousel-item " + check_index + " ' >";
                        content = content + "<div class='row row-small-gutter'>";

                        if (pular == 0)
                        {

                            listFormulas = listFormulas.Take(3).ToList();
                            pular = 3;
                        }
                        else
                        {

                            listFormulas = listFormulas.Skip(pular).Take(3).ToList();
                        }

                        ViewBag.formulas = listFormulas;
                        content = content + RenderViewToString("RecentFeeds", new FormulationsDtoCollection());
                        content = content + "</div> </div>";
                    }
                }
                return Json(new { content = content, divisor = divisor });
            }
            catch (Exception ex)
            {

                return Content("<label>" + "Erro. Contacte o administrador" + "</label>");
            }
        }

        [RedirectingAction]
        [RefreshAttribute]
        public ActionResult Relatorio_de_Uso()
        {

            Exist_UsersDto usuario;
            usuario = getUser();
            paises();
            tratamentos();
            ViewBag.usuario = usuario;
            return View("~/Views/Relatorio_de_Uso/Relatorio_de_Uso.cshtml");
        }
        [RedirectingAction]
        [RefreshAttribute]
        public ActionResult Adms_do_sistema()
        {

            Exist_UsersDto usuario;
            Exist_UsersDtoCollection adms = new Exist_UsersDtoCollection();
            exist_users util = new exist_users();

            adms = util.ConsultarTodosAdms();
            usuario = getUser();
            paises();
            tratamentos();
            ViewBag.usuario = usuario;
            var SchemeList = (from scheme in adms select scheme).ToList();
            ViewBag.Adms_do_Sistema = adms;

            return View("~/Views/Adms_do_sistema/Adms_do_Sistema.cshtml", SchemeList);
        }

        [RedirectingAction]
        [RefreshAttribute]
        public ActionResult Atualizar_Usuarios()
        {

            Exist_UsersDto usuario;
            Exist_UsersDtoCollection usuarios = new Exist_UsersDtoCollection();
            exist_users util = new exist_users();

            usuario = getUser();

            usuarios = util.ConsultarUsuarios();
            paises();
            tratamentos();
            ViewBag.usuario = usuario;

            var SchemeList = (from scheme in usuarios select scheme).ToList();
            ViewBag.usuarios = usuarios;

            return View("~/Views/Atualizar_Usuarios/Atualizar_Usuarios.cshtml", SchemeList);
        }

        [RedirectingAction]
        [RefreshAttribute]
        public ActionResult Log_do_Sistema()
        {

            Exist_UsersDto usuario;
            usuario = getUser();
            paises();
            tratamentos();
            ViewBag.usuario = usuario;
            return View("~/Views/Log_do_Sistema/Log_do_Sistema.cshtml");
        }

        [RedirectingAction]
        [RefreshAttribute]
        public ActionResult Carregar_Planilha_MAGIC1()
        {

            Exist_UsersDto usuario;
            usuario = getUser();
            paises();
            tratamentos();
            ViewBag.usuario = usuario;

            return View("~/Views/Carregar_Planilha_MAGIC1/Carregar_Planilha_MAGIC1.cshtml");
        }

        [RedirectingAction]
        [RefreshAttribute]
        public ActionResult Recursos_de_idiomas()
        {
            
            return View("~/Views/Recursos_de_idiomas/Recursos_de_idiomas.cshtml");
        }
        public ActionResult Contact()
        {

            return View();
        }
        public ActionResult Legals()
        {

            return View();
        }

        [RedirectingAction]
        [RefreshAttribute]
        public ActionResult Product_Info()
        {

            Exist_UsersDto usuario;
            usuario = getUser();
            paises();
            tratamentos();
            ViewBag.usuario = usuario;
            return View();
        }
        public ActionResult Login()
        {

            try
            {

                string culture = "";
           
                if (Request.UserLanguages != null)
                {

                        culture = Request.UserLanguages[0].Substring(0, 2);
                }
                else
                {
                    culture = "en";
                }


                string getCulture = determinaCodigoCultura(culture);

                Session["getCulture"] = getCulture;
                Session["Idiom"] = culture;
                Session["lang_code"] = culture;
                Session["culture"] = getCulture;

                Session["lang_id"] = GetLangId(this, (string)Session["Idiom"]);

                Session["rcr"] = rcr;

                SetLanguage(this);

                paises();
                tratamentos();
            }
            catch (Exception ex)
            {

                ViewBag.ErrorPage = ex.Message;
            }
            return View();
        }
        public ActionResult editarUsuario(long user_id)
        {

            exist_users user = new exist_users();
            Exist_UsersDto Models = new Exist_UsersDto();
            Models = user.consultarUsuario(user_id);
            string cSenha = Request.Form["user_password_confirm"];
            object retorno = new object();
            RemoveReferences(ModelState, "user_country");

            if (Models.countries.country_id == 0)
            {

                ModelState.AddModelError("user_country.country_id", "pais é obrigatório");
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
            retorno = user.Editar(Models);

            if (retorno == "0")
            {

                return Json(new { status = "validation", message = "Erro ao conectar na Database" });
            }
            return Json(new { status = "success", message = "cadastro com sucesso" });
        }
        public ActionResult AlterarSenha(Exist_UsersDto Models)
        {   // alterar senha quando o usuario digitou senha temporaria

            try
            {

                exist_users user = new exist_users();
                string cSenha = Request.Form["user_password_confirm"];
                string truePass = Models.user_password;
                Models = user.consultarUsuario(Models.user_id);
                Models.user_password = truePass;
                Models.user_password_confirm = cSenha;
                RemoveReferences(ModelState, "user_country");

                if (Models.user_password != Models.user_password_confirm)
                {

                    ModelState.AddModelError("user_password", GeralResource.RecRes(120));
                    paises();
                    tratamentos();
                    string contentView = RenderViewToString("~/Views/Home/FormPass.cshtml", Models);
                    return Json(new { status = "validation", view = contentView });
                }
                if (CalculateEntropy(Models.user_password) < 2)
                {

                    ModelState.AddModelError("user_password", GeralResource.RecRes(626));
                    paises();
                    tratamentos();
                    string contentView = RenderViewToString("~/Views/Home/FormPass.cshtml", Models);
                    return Json(new { status = "validation", view = contentView });
                }
                string retorno = user.AlterarSenha(Models);
                if (retorno != "1")
                {

                }
                return Json(new { status = "success", message = GeralResource.RecRes(448) });
            }
            catch (Exception ex)
            {

                return Json(new { status = "error", message = GeralResource.RecRes(561) });
            }
        }
        public void GetDayOfWeek()
        {

            string getCulture = "";
            string culture = Convert.ToString(Session["lang_code"]);

            switch (culture)
            {

                case "es":
                    {
                        culture = "es";
                        getCulture = "es-ES";
                        break;
                    }

                case "en":
                    {
                        culture = "en";
                        getCulture = "en-US";
                        break;
                    }

                case "pt":
                    {

                        culture = "pt";
                        getCulture = "pt-BR";
                        break;
                    }
                case "fr":
                    {
                        culture = "fr";
                        getCulture = "fr-FR";
                        break;
                    }
            }
            var cultures = new CultureInfo(getCulture);
            var day = cultures.DateTimeFormat.GetDayName(DateTime.Today.DayOfWeek);

            ViewBag.DayWeek = day.ToString();

        }
        public void GetDay()
        {

            var t = DateTime.Now.Day;

            ViewBag.Day = t.ToString();
        }
    }
}
